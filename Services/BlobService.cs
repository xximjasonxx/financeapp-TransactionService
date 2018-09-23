
using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TransactionService.Services
{
    public static class BlobService
    {
        static CloudBlobClient GetClient()
        {
            var connectionString = Environment.GetEnvironmentVariable("BlobConnectionString", EnvironmentVariableTarget.Process);
            var account = CloudStorageAccount.Parse(connectionString);
            return account.CreateCloudBlobClient();
        }

        public static async Task<string> SaveDepositImageAsync(byte[] imageData)
        {
            var newImageId = Guid.NewGuid().ToString();
            var container = GetClient().GetContainerReference("deposit-images");
            var blob = container.GetBlockBlobReference(newImageId);

            await blob.UploadFromByteArrayAsync(imageData, 0, imageData.Length);
            return newImageId;
        }

        public static string GetDepositImageUrl(string imageId)
        {
            var container = GetClient().GetContainerReference("deposit-images");
            var blob = container.GetBlockBlobReference(imageId);

            return blob.StorageUri.ToString();
        }
    }
}