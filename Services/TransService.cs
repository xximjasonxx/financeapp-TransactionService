using System;
using System.Threading.Tasks;
using MongoDB.Driver;
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
    }
}