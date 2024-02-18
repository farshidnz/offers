using CashrewardsOffers.Domain.Enums;
using System;

namespace CashrewardsOffers.Domain.Entities
{
    public class RankedMerchant
    {
        public string Id { get; set; }
        public (int, string) Key => (CategoryId, HyphenatedString);
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
