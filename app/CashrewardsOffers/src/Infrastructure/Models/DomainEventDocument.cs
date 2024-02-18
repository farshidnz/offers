using CashrewardsOffers.Infrastructure.Persistence;
using MongoDB.Bson;
using System;

namespace CashrewardsOffers.Infrastructure.Models
{
    public class DomainEventDocument : IDocument
    {
        public ObjectId _id { get; set; }
        public Guid EventID { get; set; }
        public string EventType { get; set; }
        public string EventPayload { get; set; }
    }
}
