using System;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using TransactionService.Models;

namespace TransactionService.Services
{
    public static class TransService
    {
        static IMongoCollection<Transaction> GetCollection()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoConnectionString", EnvironmentVariableTarget.Process));
            var database = client.GetDatabase("transactions");
            return database.GetCollection<Transaction>("transactions");
        }

        public static async Task<string> WriteNewTransaction(Transaction transaction)
        {
            var collection = GetCollection();
            await collection.InsertOneAsync(transaction);

            return transaction.Id;
        }

        public static async Task SendTransactionForProcessing(Transaction transaction)
        {
            using (var client = new HttpClient())
            {
                var jsonContent = JsonConvert.SerializeObject(transaction);
                var requestPayload = new StringContent(jsonContent);
                var response = await client.PostAsync(Environment.GetEnvironmentVariable("SendTransactionsLogicAppUrl", EnvironmentVariableTarget.Process), requestPayload);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to send Transaction for Processing");

                return;
            }
        }
    }
}