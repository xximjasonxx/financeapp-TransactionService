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
        static IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoConnectionString", EnvironmentVariableTarget.Process));
            return client.GetDatabase("transactions");
        }

        public static async Task<string> WriteNewTransactionAsync(Transaction transaction)
        {
            var database = GetDatabase();
            var collection = database.GetCollection<Transaction>("transactions");

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
            var database = GetDatabase();
            var collection = database.GetCollection<Transaction>("transactions");
            var updateDef = Builders<Transaction>.Update.Set(o => o.Status, TransactionStatus.Approved);
            
            await collection.UpdateOneAsync(o => o._Id == transaction._Id, updateDef);
        }

        public static async Task<string> RecordDepositRequest(PendingDeposit pendingDeposit)
        {
            var database = GetDatabase();
            var collection = database.GetCollection<PendingDeposit>("deposit-requests");

            await collection.InsertOneAsync(pendingDeposit);
            return pendingDeposit.Id;
        }
    }
}