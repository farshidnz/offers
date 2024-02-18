using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure.Persistence;
using MongoDB.Bson;
using System;

namespace CashrewardsOffers.Domain.Entities
{
    public class MerchantHistoryDocument : IDocument
    {
        public ObjectId _id { get; set; }
        public DateTimeOffset ChangeTime { get; set; }
        public int MerchantId { get; set; }
        public Client Client { get; set; }
        public string Name { get; set; }
        public string HyphenatedString { get; set; }
        public string ClientCommissionString { get; set; }
    }
}
