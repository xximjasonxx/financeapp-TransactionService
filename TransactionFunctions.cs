
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TransactionService.Services;
using TransactionService.Extensions;
using TransactionService.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace TransactionService.Functions
{
    public static class TransactionFunctions
    {
        [FunctionName("CreateTransaction")]
        public static async Task<IActionResult> CreateTransaction([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            var token = req.Headers["auth-key"].ToString().AsJwtToken();
            var user = await TokenService.GetUserIdForToken(token);
            if (user == null)
                return new NotFoundResult();

            var transaction = JsonConvert.DeserializeObject<Transaction>(await req.ReadAsStringAsync());
            transaction.Owner = Guid.Parse(user.UserId);
            transaction.CreatedDate = DateTime.UtcNow;
            await TransService.WriteNewTransactionAsync(transaction);
            await TransService.SendTransactionForProcessingAsync(transaction);

            return new AcceptedResult(transaction.Id.ToString(), transaction.Id.ToString());
        }

        [FunctionName("CreateDeposit")]
        public static async Task<IActionResult> CreateDeposit([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            var token = req.Headers["auth-key"].ToString().AsJwtToken();
            var user = await TokenService.GetUserIdForToken(token);
            if (user == null)
                return new NotFoundResult();

            var request = JsonConvert.DeserializeObject<DepositRequest>(await req.ReadAsStringAsync());
            var imageBytes = Convert.FromBase64String(request.DepositImageBase64);
            var imageId = await BlobService.SaveDepositImageAsync(imageBytes);

            var pendingDeposit = new PendingDeposit
            {
                ImageId = imageId,
                TargetAccount = request.TargetAccount,
                DepositOwner = user.UserId
            };

            await QueueService.PostDepositForProcessing(pendingDeposit);
            
            return new AcceptedResult(imageId, imageId);
        }

        [FunctionName("ProcessDeposit")]
        public static async Task ProcessDeposit([ServiceBusTrigger("new-deposits", Connection = "ServiceBusConnectionString")]string pendingDepositContents, ILogger logger)
        {
            var pendingDeposit = JsonConvert.DeserializeObject<PendingDeposit>(pendingDepositContents);
            var imageUrl = BlobService.GetDepositImageUrl(pendingDeposit.ImageId);
            var amount = await VisionService.DetermineImageValueAsync(imageUrl);

            var transaction = new Transaction
            {
                CreatedDate = DateTime.UtcNow,
                TargetAccount = Guid.Parse(pendingDeposit.TargetAccount),
                Owner = Guid.Parse(pendingDeposit.DepositOwner),
                Amount = amount
            };

            var transactionId = await TransService.WriteNewTransactionAsync(transaction);
            transaction.Id = transactionId;

            await TransService.SendTransactionForProcessingAsync(transaction);
        }

        [FunctionName("ProcessTransactions")]
        public static async Task<IActionResult> ProcessTransactions([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            var batchArray = JArray.Parse(await req.ReadAsStringAsync());
            await Task.Run(() => {
                var rawTransactions = batchArray.Select(obj => JsonConvert.DeserializeObject<Transaction>(obj["content"].ToString()));

                var sendTasks = rawTransactions
                    .GroupBy(x => x.TargetAccount)
                    .Select(group => QueueService.PostAmountChangeEvent(new AmountChangeEvent
                    {
                        TargetAccountId = group.Key.ToString(),
                        ValueChangeAmount = group.Sum(x => x.Amount)
                    })).ToArray();

                Task.WaitAll(sendTasks);

                var updateTasks = rawTransactions.Select(transaction => TransService.ApproveTransaction(transaction)).ToArray();
                Task.WaitAll(updateTasks);
            });

            return new AcceptedResult();
        }
    }
}
