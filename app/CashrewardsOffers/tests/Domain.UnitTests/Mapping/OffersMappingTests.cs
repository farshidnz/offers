using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Mapster;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CashrewardsOffers.Domain.UnitTests.Mapping
{
    public class OffersMappingTests
    {
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
            EndDateTime = new DateTimeOffset(2027, 4, 29, 23, 59, 59, TimeSpan.FromHours(10)),
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

        public static OfferChanged OfferChangedEvent => new()
        {
            OfferId = 111,
            Client = Client.Cashrewards,
            HyphenatedString = "offer-hyphenated-string",
            IsFeatured = true,
            IsMerchantPaused = true,
            Merchant = new OfferMerchantChanged
            {
                Id = 222,
                HyphenatedString = "merchant-hyphenated-string",
                ClientCommissionString = "$72.68 cashback",
                MobileEnabled = true,
                MobileAppEnabled = true,
                IsPremiumDisabled = true,
                Name = "merchant-name",
                LogoUrl = "regular-image-url",
                NetworkId = 333,
                BasicTerms = "basic terms",
                ExtentedTerms = "extended terms",
                IsHomePageFeatured = true,
                Categories = new List<OfferMerchantChangedCategoryItem>
                {
                    new OfferMerchantChangedCategoryItem() { CategoryId = 1, Name = "cat1" },
                    new OfferMerchantChangedCategoryItem() { CategoryId = 2, Name = "cat2" },
                    new OfferMerchantChangedCategoryItem() { CategoryId = 3, Name = "cat3" }
                }
            },
            Title = "title",
            Terms = "offer-terms",
            WasRate = "2.5% cashback",
            EndDateTime = new DateTimeOffset(2027, 4, 29, 23, 59, 59, TimeSpan.FromHours(10)).UtcTicks,
            Ranking = 5
        };

        public static OfferDeleted OfferDeletedEvent => new()
        {
            OfferId = 111,
            Client = Client.Cashrewards,
            HyphenatedString = "offer-hyphenated-string",
            Merchant = new OfferMerchantDeleted
            {
                Id = 222,
                HyphenatedString = "merchant-hyphenated-string",
            }
        };

        [SetUp]
        public void SetUp()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Domain"));
        }

        [Test]
        public void Adapt_ShouldMapToOfferInitialEvent_GivenDomainOffer()
        {
            var offerChangedEvent = DomainOffer("1234").Adapt<OfferInitial>();

            offerChangedEvent.Should().BeEquivalentTo(OfferChangedEvent);
        }

        [Test]
        public void Adapt_ShouldMapToOfferChangedEvent_GivenDomainOffer()
        {
            var offerChangedEvent = DomainOffer("1234").Adapt<OfferChanged>();

            offerChangedEvent.Should().BeEquivalentTo(OfferChangedEvent);
            OfferChangedEvent.EndDateTime.Should().Be(639446039990000000L);
        }

        [Test]
        public void Adapt_ShouldMapToOfferDeletedEvent_GivenDomainOffer()
        {
            var offerDeletedEvent = DomainOffer("1234").Adapt<OfferDeleted>();

            offerDeletedEvent.Should().BeEquivalentTo(OfferDeletedEvent);
        }
    }
}
