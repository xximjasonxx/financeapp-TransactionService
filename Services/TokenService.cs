
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TransactionService.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TransactionService.Services
{
    public static class TokenService
    {
        public static async Task<User> GetUserIdForToken(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("auth-key", token);
                client.DefaultRequestHeaders.Add("x-functions-key", "CQJ/LKNibcTaRUrHmr2maKyfLNo3HNZ8rNGSLIuslwO7BZfW0qxbTA==");
                
                var response = await client.GetAsync($"https://financeapp-authservice.azurewebsites.net/api/validate_token");
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<User>(responseContent);
            }
        }
    }
}