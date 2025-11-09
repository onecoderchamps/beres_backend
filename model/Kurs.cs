using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Beres.Shared.Models
{
    public class Kurs
    {
        [BsonId]
        public string? Id { get; set; }
        [BsonElement("Currency")]
        public int? Currency { get; set; }
        [BsonElement("From")]
        public string? From { get; set; }
        [BsonElement("To")]
        public string? To { get; set; }
    }

    public class Diskons
    {
        [BsonId]
        public string? Id { get; set; }
        [BsonElement("Code")]
        public string? Code { get; set; }

        [BsonElement("DiskonValue")]
        public int? DiskonValue { get; set; }
    }
}
