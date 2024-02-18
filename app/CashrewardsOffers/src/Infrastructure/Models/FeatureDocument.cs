using MongoDB.Bson;
using System;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class FeatureDocument : IDocument
    {
        public ObjectId _id { get; set; }
        public string CognitoId { get; set; }
        public string Feature { get; set; }
    }
}
