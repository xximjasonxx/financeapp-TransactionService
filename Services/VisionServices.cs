using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace TransactionService.Services
{
    public class VisionService
    {
        private readonly static IList<VisualFeatureTypes> VisualFeatures = new List<VisualFeatureTypes>
        {
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.Tags
        };

        public static async Task<decimal> DetermineImageValueAsync(string imageUrl)
        {
            var visionSubscriptionKey = Environment.GetEnvironmentVariable("ComputerVisionKey", EnvironmentVariableTarget.Process);
            var client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(visionSubscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });
            client.Endpoint = Environment.GetEnvironmentVariable("ComputerVisionEndpoint", EnvironmentVariableTarget.Process);

            var analysisResult = await client.AnalyzeImageAsync(imageUrl, VisualFeatures);
            return DetermineValue(analysisResult);
        }

        static decimal DetermineValue(ImageAnalysis analysisResult)
        {
            // tags = $1 per level of confidence per tag
            // faces - $10 per

            var faceValue = analysisResult.Faces.Count * 10;
            var tagValue = (decimal)analysisResult.Tags.Sum(x => Math.Round(x.Confidence * 100, 2));

            return faceValue + tagValue;
        }
    }
}