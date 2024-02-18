using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace CashrewardsOffers.Infrastructure.Models
{
    [BsonIgnoreExtraElements]
    public class DocDbUserInfo
    {
        public int ok { get; set; }
        public List<DocDbUser> users { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class DocDbUser
    {
        public string _id { get; set; }
        public string user { get; set; }
        public string db { get; set; }
    }
}
