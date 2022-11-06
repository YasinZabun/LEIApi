using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Data.Model
{
    public class LeiRecordModel
    {
        [BsonIgnore]
        private string id
        {
            get;
            set;
        }
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        public string Id
        {
            get { return id; }
            set
            {
                if (value != null && ObjectId.TryParse(value, out ObjectId objectId))
                {
                    id = objectId.ToString();
                }
                else id = ObjectId.GenerateNewId().ToString();
            }
        }
        public string? TransactionUti { get; set; }
        public string? Isin { get; set; }
        public double Notional { get; set; }
        public string? NotionalCurrency { get; set; }
        public string? TransactionType { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement(Order = 101)]
        public DateTime TransactionDatetime { get; set; }
        public double Rate { get; set; }
        public string? Lei { get; set; }
        public string? LegalName { get; set; }
        public string[]? Bic { get; set; }
        public double TransactionCost { get; set; }
    }
}