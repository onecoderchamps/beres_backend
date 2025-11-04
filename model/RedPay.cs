using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Beres.Shared.Models
{
    public class RedPayModel : BaseModel
    {
         [BsonId]
        public string? Id { get; set; }

        [BsonElement("IdUser")]
        public string? IdUser { get; set; }
    }
}
