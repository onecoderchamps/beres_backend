using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beres.Shared.Models
{
    public class Event : BaseModel
    {
        [BsonId]
        public string? Id { get; set; }
        [BsonElement("Name")]
        public string? Name { get; set; }
        [BsonElement("Image")]
        public string? Image { get; set; }
        [BsonElement("DueDate")]
        public string? DueDate { get; set; }
        [BsonElement("Price")]
        public float? Price { get; set; }
        [BsonElement("Desc")]
        public string? Desc { get; set; }
        [BsonElement("Location")]
        public string? Location { get; set; }
    }
}