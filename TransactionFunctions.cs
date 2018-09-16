
using System;
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
            transaction.CreatedData = DateTime.UtcNow;
            await TransService.WriteNewTransaction(transaction);

            return new AcceptedResult(transaction.Id.ToString(), transaction.Id.ToString());
        }

        [FunctionName("ProcessTransactions")]
        public static async Task<IActionResult> ProcessTransactions([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Request Received");

            return new OkResult();
        }
    }
}
