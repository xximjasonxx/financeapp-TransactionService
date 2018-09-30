
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace TransactionService.Models
{
    public class PendingDeposit
    {
        [BsonId, JsonIgnore]
        public ObjectId _Id { get; set; }

        [JsonProperty("id"), BsonIgnore]
        public string Id
        {
            get { return _Id.ToString(); }
            set
            {
                ObjectId parsedValue;
                if (ObjectId.TryParse(value, out parsedValue))
                    _Id = parsedValue;
                else
                    _Id = ObjectId.Empty;
            }
        }

        public string Source { get; set; }
        public string ImageUrl { get; set; }
        public string TargetAccount { get; set; }
        public string DepositOwner { get; set; }
    }
}