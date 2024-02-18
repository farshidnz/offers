using MongoDB.Bson;
using System;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class RankedMerchantDocument : IDocument
    {
        public ObjectId _id { get; set; }
        public int MerchantId { get; set; }
        public int GeneratedRank { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string MerchantName { get; set; }
        public string HyphenatedString { get; set; }
        public int RankByCategory { get; set; }
        public int OverallRank { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public string RegularImageUrl { get; set; }
    }
}
