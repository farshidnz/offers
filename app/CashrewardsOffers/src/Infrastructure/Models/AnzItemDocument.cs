using CashrewardsOffers.Infrastructure.Persistence;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Infrastructure.Models
{
    public class AnzItemDocument : IDocument
    {
        public ObjectId _id { get; set; }
        public string ItemId { get; set; }
        public long LastUpdated { get; set; }
        public bool IsDeleted { get; set; }
        public AnzMerchantDocument Merchant { get; set; }
        public AnzOfferDocument Offer { get; set; }
    }

    public class AnzMerchantDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public string LogoUrl { get; set; }
        public string ClientCommissionString { get; set; }
        public string SpecialTerms { get; set; }
        public string CashbackGuidelines { get; set; }
        public int NetworkId { get; set; }
        public bool IsHomePageFeatured { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPopularFlag { get; set; }
        public int PopularRanking { get; set; }
        public int InstoreRanking { get; set; }
        public string InstoreTerms { get; set; }
        public bool MobileEnabled { get; set; }
        public bool IsPremiumDisabled { get; set; }
        public bool IsPaused { get; set; }
        public int PopularMerchantRankingForBrowser { get; set; }
        public List<AnzMerchantCategoryDocument> Categories { get; set; }
    }

    public class AnzOfferDocument
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Terms { get; set; }
        public string Link { get; set; }
        public string WasRate { get; set; }
        public bool IsFeatured { get; set; }
        public int FeaturedRanking { get; set; }
        public long EndDateTime { get; set; }
        public int Ranking { get; set; }

    }

    public class AnzMerchantCategoryDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
