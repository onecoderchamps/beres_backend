using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beres.Shared.Models
{
    public class Order : BaseModel
    {
        [BsonId]
        public string? Id { get; set; }
        [BsonElement("Type")]
        public string? Type { get; set; }
        [BsonElement("IdUser")]
        public string? IdUser { get; set; }
        [BsonElement("Status")]
        public string? Status { get; set; }
        [BsonElement("Price")]
        public float? Price { get; set; }
        [BsonElement("UniqueCode")]
        public float? UniqueCode { get; set; }
        [BsonElement("Desc")]
        public string? Desc { get; set; }
    }
}