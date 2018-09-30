
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace TransactionService.Models
{
    public class Transaction
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

        public string TargetAccount { get; set; }

        public decimal Amount { get; set; }

        public string Owner { get; set; }

        public DateTime CreatedDate { get; set; }

        public TransactionStatus Status { get; set; }
    }
}