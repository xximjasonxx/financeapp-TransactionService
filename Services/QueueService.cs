
using System;
using System.Threading.Tasks;
using TransactionService.Models;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace TransactionService.Services
{
    public static class QueueService
    {
        public static async Task PostAmountChangeEvent(AmountChangeEvent ev)
        {
            var connectionString = Environment.GetEnvironmentVariable("CurrentBalanceUpdateQueueConnectionString", EnvironmentVariableTarget.Process);
            var client = new QueueClient(connectionString, "current-balance-update-queue");

            var rawContents = JsonConvert.SerializeObject(ev);
            var message = new Message(Encoding.UTF8.GetBytes(rawContents));
            await client.SendAsync(message);
        }
    }
}