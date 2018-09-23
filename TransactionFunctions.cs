
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
            await TransService.WriteNewTransaction(transaction);
            await TransService.SendTransactionForProcessing(transaction);

            return new AcceptedResult(transaction.Id.ToString(), transaction.Id.ToString());
        }

        [FunctionName("CreateDeposit")]
        public static async Task<IActionResult> CreateDeposit([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            return new AcceptedResult(Guid.NewGuid().ToString(), string.Empty);
        }

        [FunctionName("ProcessTransactions")]
        public static async Task<IActionResult> ProcessTransactions([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            var batchArray = JArray.Parse(await req.ReadAsStringAsync());
            await Task.Run(() => {
                var sendTasks = batchArray.Select(obj => JsonConvert.DeserializeObject<Transaction>(obj["content"].ToString()))
                    .GroupBy(x => x.TargetAccount)
                    .Select(group => QueueService.PostAmountChangeEvent(new AmountChangeEvent
                    {
                        TargetAccountId = group.Key.ToString(),
                        ValueChangeAmount = group.Sum(x => x.Amount)
                    })).ToArray();

                Task.WaitAll(sendTasks);
            });

            return new AcceptedResult();
        }
    }
}
