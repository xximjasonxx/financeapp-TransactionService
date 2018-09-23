using System;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Bson;
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

        public static async Task<string> WriteNewTransactionAsync(Transaction transaction)
        {
            var collection = GetCollection();

            transaction.Status = TransactionStatus.Pending;
            await collection.InsertOneAsync(transaction);

            return transaction.Id;
        }

        public static async Task SendTransactionForProcessingAsync(Transaction transaction)
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

        public static async Task ApproveTransaction(Transaction transaction)
        {
            var collection = GetCollection();
            var updateDef = Builders<Transaction>.Update.Set(o => o.Status, TransactionStatus.Approved);
            
            await collection.UpdateOneAsync(o => o._Id == transaction._Id, updateDef);
        }
    }
}