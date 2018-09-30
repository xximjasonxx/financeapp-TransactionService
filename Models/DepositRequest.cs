
using Newtonsoft.Json;

namespace TransactionService.Models
{
    public class DepositRequest
    {
        public string TargetAccount {get; set; }

        [JsonProperty("DepositImage")]
        public string DepositImageBase64 { get; set; }

        public string Source { get; set; }
    }
}