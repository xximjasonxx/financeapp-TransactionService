
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
            var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString", EnvironmentVariableTarget.Process);
            var client = new QueueClient(connectionString, "current-balance-update-queue");

            var rawContents = JsonConvert.SerializeObject(ev);
            var message = new Message(Encoding.UTF8.GetBytes(rawContents));
            await client.SendAsync(message);
        }

        public static async Task PostDepositForProcessing(PendingDeposit pendingDeposit)
        {
            var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString", EnvironmentVariableTarget.Process);
            var client = new QueueClient(connectionString, "new-deposits");

            var rawContents = JsonConvert.SerializeObject(pendingDeposit);
            var message = new Message(Encoding.UTF8.GetBytes(rawContents));
            await client.SendAsync(message);
        }
    }
}