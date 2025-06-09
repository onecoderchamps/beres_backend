using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beres.Shared.Models
{
    public class Banner : BaseModel
    {
        [BsonId]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string? Name { get; set; }
        [BsonElement("Image")]
        public string? Image { get; set; }
    }
}