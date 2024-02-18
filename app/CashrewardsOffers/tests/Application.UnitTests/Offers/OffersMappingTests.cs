using CashrewardsOffers.Application.Offers.Queries.GetEdmOffers.v1;
using CashrewardsOffers.Application.Offers.Queries.GetOffers.v1;
using CashrewardsOffers.Application.Offers.Services;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.ValueObjects;
using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using Mapster;
using MongoDB.Bson;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.Offers
{
    public class OffersMappingTests
    {
        static ShopGoOffer ShopGoOffer => new()
        {
            OfferId = 111,
            ClientId = 1000000,
            MerchantId = 222,
            CouponCode = "coupon",
            OfferTitle = "title",
            OfferDescription = "description",
            HyphenatedString = "offer-hyphenated-string",
            DateEnd = new DateTime(2021, 12, 6, 23, 59, 0),
            MerchantName = "merchant-name",
            RegularImageUrl = "regular-image-url",
            OfferCount = 3,
            ClientProgramTypeId = 101,
            TierCommTypeId = 100,
            TierTypeId = 118,
            Commission = 95,
            ClientComm = 90,
            MemberComm = 85,
            RewardName = "cashback",
            MerchantShortDescription = "merchant-short-desc",
            MerchantHyphenatedString = "merchant-hyphenated-string",
            OfferTerms = "offer-terms",
            IsFlatRate = true,
            Rate = 7.5m,
            OfferBackgroundImageUrl = "offer-background-image-url",
            OfferBadgeCode = "offer-badge-code",
            MerchantBadgeCode = "merchant-badge-code",
            OfferPastRate = "2.5% cashback",
            IsFeatured = true,
            IsCashbackIncreased = true,
            IsPremiumFeature = true,
            Ranking = 5,
            MerchantIsPremiumDisabled = true,
            MobileEnabled = true,
            IsMobileAppEnabled = null, // null should default to true
            IsMerchantPaused = true,
            NetworkId = 333,
            BasicTerms = "basic terms",
            ExtentedTerms = "extended terms",
            IsPopular = true,
            IsHomePageFeatured = true,
            IsCategoryFeatured = true
        };

        static Dictionary<int, ShopGoCategory[]> ShopGoCategories => new()
        {
            [222] = new[]
            {
                new ShopGoCategory() { CategoryId = 1, Name = "cat1" },
                new ShopGoCategory() { CategoryId = 2, Name = "cat2" },
                new ShopGoCategory() { CategoryId = 3, Name = "cat3" },
            }
        };

        static List<ShopGoTier> ShopGoMerchantTiers => new()
        {
            new ShopGoTier
            {
                ClientTierId = 300,
                MerchantTierId = 400,
                MerchantId = 222,
                ClientId = 1000000,
                TierName = "Electronics",
                TierCommTypeId = 100,
                Commission = 95,
                ClientComm = 90,
                MemberComm = 85,
                TierSpecialTerms = "Capped at $45"
            }
        };

        static Offer DomainOffer(string id = null) => new()
        {
            Id = id,
            OfferId = 111,
            Client = Client.Cashrewards,
            PremiumClient = null,
            CouponCode = "coupon",
            Title = "title",
            Description = "description",
            HyphenatedString = "offer-hyphenated-string",
            EndDateTime = new DateTimeOffset(2021, 12, 6, 23, 59, 0, TimeSpan.FromHours(11)),
            Terms = "offer-terms",
            OfferBackgroundImageUrl = "offer-background-image-url",
            OfferBadge = "offer-badge-code",
            WasRate = "2.5% cashback",
            IsFeatured = true,
            IsCashbackIncreased = true,
            IsPremiumFeature = true,
            IsMerchantPaused = true,
            Ranking = 5,
            IsPersonalised = false,
            IsCategoryFeatured = true,
            Merchant = new OfferMerchant
            {
                Id = 222,
                Name = "merchant-name",
                HyphenatedString = "merchant-hyphenated-string",
                LogoUrl = "regular-image-url",
                Description = "merchant-short-desc",
                Commission = 95,
                ClientComm = 90,
                MemberComm = 85,
                OfferCount = 3,
                RewardType = RewardType.Cashback2,
                IsCustomTracking = true,
                MerchantBadge = "merchant-badge-code",
                ClientProgramType = ClientProgramType.ProductProgram,
                CommissionType = CommissionType.Dollar,
                Rate = 7.5m,
                IsFlatRate = true,
                RewardName = "cashback",
                MobileEnabled = true,
                IsMobileAppEnabled = true,
                NetworkId = 333,
                IsPremiumDisabled = true,
                BasicTerms = "basic terms",
                ExtentedTerms = "extended terms",
                IsHomePageFeatured = true,
                Categories = new OfferMerchantCategory[]
                {
                    new OfferMerchantCategory() { CategoryId = 1, Name = "cat1" }, 
                    new OfferMerchantCategory() { CategoryId = 2, Name = "cat2" },
                    new OfferMerchantCategory() { CategoryId = 3, Name = "cat3" }
                },
                Tiers = new List<OfferMerchantTier>
                {
                    new OfferMerchantTier
                    {
                        TierName= "Electronics",
                        CommissionType = CommissionType.Dollar,
                        Commission = 95,
                        ClientComm = 90,
                        MemberComm = 85,
                        TierSpecialTerms = "Capped at $45"
                    }
                }
            },
            Premium = new OfferPremium
            {
                Commission = 96,
                ClientComm = 91,
                MemberComm = 86,
                ClientProgramType = ClientProgramType.ProductProgram,
                CommissionType = CommissionType.Dollar,
                Rate = 7.5m,
                IsFlatRate = true,
                RewardName = "cashback"
            }
        };

        static OfferDocument OfferDocument(ObjectId id) => new()
        {
            _id = id,
            OfferId = 111,
            Client = 1000000,
            PremiumClient = null,
            CouponCode = "coupon",
            Title = "title",
            Description = "description",
            HyphenatedString = "offer-hyphenated-string",
            EndDateTime = new DateTimeOffset(2021, 12, 6, 23, 59, 0, TimeSpan.FromHours(11)),
            Terms = "offer-terms",
            OfferBackgroundImageUrl = "offer-background-image-url",
            OfferBadge = "offer-badge-code",
            WasRate = "2.5% cashback",
            IsFeatured = true,
            IsCashbackIncreased = true,
            IsPremiumFeature = true,
            IsMerchantPaused = true,
            Ranking = 5,
            IsCategoryFeatured = true,
            Merchant = new OfferMerchantDocument
            {
                MerchantId = 222,
                Name = "merchant-name",
                HyphenatedString = "merchant-hyphenated-string",
                LogoUrl = "regular-image-url",
                Description = "merchant-short-desc",
                Commission = 95,
                ClientComm = 90,
                MemberComm = 85,
                OfferCount = 3,
                RewardType = 118,
                IsCustomTracking = true,
                MerchantBadge = "merchant-badge-code",
                ClientProgramType = 101,
                CommissionType = 100,
                Rate = 7.5m,
                IsFlatRate = true,
                RewardName = "cashback",
                MobileEnabled = true,
                IsMobileAppEnabled = true,
                NetworkId = 333,
                IsPremiumDisabled = true,
                IsHomePageFeatured = true,
                BasicTerms = "basic terms",
                ExtentedTerms = "extended terms",
                Categories = new int[] { 1, 2, 3 },
                CategoryObjects = new List<OfferMerchantCategoryDocument>
                {
                    new OfferMerchantCategoryDocument() { CategoryId = 1, Name = "cat1" },
                    new OfferMerchantCategoryDocument() { CategoryId = 2, Name = "cat2" },
                    new OfferMerchantCategoryDocument() { CategoryId = 3, Name = "cat3" }
                },
                Tiers = new List<OfferMerchantTierDocument>
                {
                    new OfferMerchantTierDocument
                    {
                        TierName= "Electronics",
                        CommissionType = 100,
                        Commission = 95,
                        ClientComm = 90,
                        MemberComm = 85,
                        TierSpecialTerms = "Capped at $45"
                    }
                }
            },
            Premium = new PremiumOfferDocument
            {
                Commission = 96,
                ClientComm = 91,
                MemberComm = 86,
                ClientProgramType = 101,
                CommissionType = 100,
                Rate = 7.5m,
                IsFlatRate = true,
                RewardName = "cashback"
            }
        };

        static OfferInfo OfferInfo => new()
        {
            Id = 111,
            Title = "title",
            CouponCode = "coupon",
            EndDateTime = new DateTime(2021, 12, 6, 23, 59, 0),
            Description = "description",
            HyphenatedString = "offer-hyphenated-string",
            IsFeatured = true,
            Terms = "offer-terms",
            MerchantId = 222,
            MerchantLogoUrl = "regular-image-url",
            OfferBackgroundImageUrl = "offer-background-image-url",
            OfferBadge = "offer-badge-code",
            IsCashbackIncreased = true,
            IsPremiumFeature = true,
            WasRate = "2.5% cashback",
            Merchant = new MerchantInfo
            {
                Id = 222,
                Name = "merchant-name",
                HyphenatedString = "merchant-hyphenated-string",
                LogoUrl = "regular-image-url",
                Description = "merchant-short-desc",
                Commission = 72.68m,
                IsFlatRate = true,
                CommissionType = "dollar",
                OfferCount = 3,
                RewardType = "Cashback",
                IsCustomTracking = true,
                MerchantBadge = "merchant-badge-code"
            },
            Premium = new PremiumInfo
            {
                Commission = 75.13m,
                IsFlatRate = true,
                ClientCommissionString = "$75.13 cashback"
            },
            ClientCommissionString = "$72.68 cashback",
            RegularImageUrl = "regular-image-url",
            OfferPastRate = null,
            MerchantHyphenatedString = "merchant-hyphenated-string",
            IsPersonalised = false
        };

        static OfferPremium DomainOfferPremium => new()
        {
            Commission = 95,
            ClientComm = 90,
            MemberComm = 85,
            ClientProgramType = ClientProgramType.ProductProgram,
            CommissionType = CommissionType.Dollar,
            Rate = 7.5m,
            IsFlatRate = true,
            RewardName = "cashback",
            RewardType = RewardType.Cashback2
        };

        static EdmOfferInfo EdmOfferInfo => new()
        {
            Title = "title",
            ClientCommissionString = "$72.68 cashback",
            WasRate = "2.5% cashback",
            OfferEndString = "Soon",
            OfferBackgroundImageUrl = "offer-background-image-url",
            MerchantLogoUrl = "regular-image-url",
            EndDateTime = new DateTime(2021, 12, 6, 23, 59, 0),
            MerchantHyphenatedString = "merchant-hyphenated-string",
            OfferHyphenatedString = "offer-hyphenated-string",
            MerchantId = 222,
            OfferId = 111,
            Terms = "Capped at $45",
            Premium = new EdmOfferPremiumInfo()
            {
                ClientCommissionString = "$75.13 cashback"
            }
        };

        [SetUp]
        public void SetUp()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Infrastructure"));
        }

        [Test]
        public void Adapt_ShouldMapToDomainOffer_GivenShopGoOffer()
        {
            string customTrackingMerchantList = "222";

            var domainOffer = ShopGoOffer.BuildAdapter()
                .AddParameters("customTrackingMerchantList", customTrackingMerchantList)
                .AddParameters("merchantCategoriesLookup", ShopGoCategories)
                .AddParameters("merchantTiers", ShopGoMerchantTiers)
                .AdaptToType<Offer>();

            var expectedDomainOffer = DomainOffer();
            expectedDomainOffer.Premium = null;
            domainOffer.Should().BeEquivalentTo(expectedDomainOffer);
        }

        [Test]
        public void Adapt_ShouldMapToOfferDocument_GivenDomainOffer()
        {
            var offerDocument = DomainOffer().Adapt<OfferDocument>();

            offerDocument.Should().BeEquivalentTo(OfferDocument(ObjectId.Empty));
        }

        [Test]
        public void Adapt_ShouldMapToDomainOffer_GivenOfferDocument()
        {
            var domainOffer = OfferDocument(ObjectId.Parse("000000000000000000000001")).Adapt<Offer>();

            domainOffer.Should().BeEquivalentTo(DomainOffer("000000000000000000000001"));
        }

        [Test]
        public void Adapt_ShouldMapToDomainOfferPremium_GivenDomainOffer()
        {
            var domainOfferPremium = DomainOffer().Adapt<OfferPremium>();

            domainOfferPremium.Should().BeEquivalentTo(DomainOfferPremium);
        }

        [Test]
        public void Adapt_ShouldMapToOfferInfo_GivenDomainOffer()
        {
            var offerInfo = DomainOffer().BuildAdapter().AddParameters("isFeatured", true).AdaptToType<OfferInfo>();

            offerInfo.Should().BeEquivalentTo(OfferInfo);
        }

        [Test]
        public void Adapt_ShouldMapToOfferInfoWithoutOfferBadge_GivenDomainOfferIsFeatured()
        {
            var domainOffer = DomainOffer();
            domainOffer.OfferBadge = BadgeCodes.AnzPremiumOffers;

            var offerInfo = domainOffer.BuildAdapter().AddParameters("isFeatured", true).AdaptToType<OfferInfo>();

            offerInfo.OfferBadge.Should().BeEmpty();
        }

        [Test]
        public void Adapt_ShouldMapToEdmOfferInfo_GivenDomainOffer()
        {
            var offerInfo = DomainOffer().BuildAdapter().AddParameters("now", new DateTimeOffset(2021, 12, 6, 0, 0, 0, TimeSpan.Zero)).AdaptToType<EdmOfferInfo>();

            var expected = EdmOfferInfo;
            expected.OfferEndString = "Today";
            offerInfo.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Adapt_ShouldMapToEdmOfferInfo_GivenDomainOffer_AndGivenNowParameterIsMissing()
        {
            var offerInfo = DomainOffer().Adapt<EdmOfferInfo>();

            offerInfo.Should().NotBeNull();
        }
    }
}
