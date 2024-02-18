using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Mapster;
using NUnit.Framework;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class MerchantsMappingTests
    {
        static Merchant DomainMerchant(string id = null) => new()
        {
            Id = id,
            MerchantId = 222,
            Client = Client.Cashrewards,
            HyphenatedString = "merchant-hyphenated-string",
            LogoUrl = "regular-image-url",
            Commission = 95,
            ClientComm = 90,
            MemberComm = 85,
            RewardType = RewardType.Cashback2,
            ClientProgramType = ClientProgramType.ProductProgram,
            CommissionType = CommissionType.Dollar,
            Rate = 7.5m,
            IsFlatRate = true,
            RewardName = "cashback",
            NetworkId = 1000053,
            IsFeatured = true,
            IsHomePageFeatured = true,
            IsPopular = true,
            MobileEnabled = true,
            MobileAppEnabled = true,
            IsPremiumDisabled = true,
            IsPaused = true,
            PopularMerchantRankingForBrowser = 2,
            PopularMerchantRankingForMobile = 3,
            BasicTerms = "basic terms",
            ExtentedTerms = "extended terms",
            Premium = new MerchantPremium
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

        static MerchantInitial MerchantInitialEvent => new()
        {
            MerchantId = 222,
            Client = Client.Cashrewards,
            HyphenatedString = "merchant-hyphenated-string",
            ClientCommissionString = "$72.68 cashback",
            PremiumClientCommissionString = "$75.13 cashback",
            LogoUrl = "regular-image-url",
            NetworkId = 1000053,
            IsFeatured = true,
            IsHomePageFeatured = true,
            IsPopular = true,
            MobileEnabled = true,
            MobileAppEnabled = true,
            IsPremiumDisabled = true,
            PopularMerchantRankingForBrowser = 2,
            PopularMerchantRankingForMobile = 3,
            BasicTerms = "basic terms",
            ExtentedTerms = "extended terms"
        };

        static MerchantChanged MerchantChangedEvent => new()
        {
            MerchantId = 222,
            Client = Client.Cashrewards,
            HyphenatedString = "merchant-hyphenated-string",
            ClientCommissionString = "$72.68 cashback",
            PremiumClientCommissionString = "$75.13 cashback",
            LogoUrl = "regular-image-url",
            NetworkId = 1000053,
            IsFeatured = true,
            IsHomePageFeatured = true,
            IsPopular = true,
            MobileEnabled = true,
            MobileAppEnabled = true,
            IsPremiumDisabled = true,
            IsPaused = true,
            PopularMerchantRankingForBrowser = 2,
            PopularMerchantRankingForMobile = 3,
            BasicTerms = "basic terms",
            ExtentedTerms = "extended terms"
        };

        static MerchantDeleted MerchantDeletedEvent => new()
        {
            MerchantId = 222,
            Client = Client.Cashrewards,
            HyphenatedString = "merchant-hyphenated-string",
        };

        [Test]
        public void Adapt_ShouldMapToMerchantInitialEvent_GivenDomainMerchant()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Domain"));

            var merchantChangedEvent = DomainMerchant("1234").Adapt<MerchantInitial>();

            merchantChangedEvent.Should().BeEquivalentTo(MerchantChangedEvent);
        }

        [Test]
        public void Adapt_ShouldMapToMerchantChangedEvent_GivenDomainMerchant()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Domain"));

            var merchantChangedEvent = DomainMerchant("1234").Adapt<MerchantChanged>();

            merchantChangedEvent.Should().BeEquivalentTo(MerchantChangedEvent);
        }

        [Test]
        public void Adapt_ShouldMapToMerchantDeletedEvent_GivenDomainMerchant()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Domain"));

            var merchantDeletedEvent = DomainMerchant("1234").Adapt<MerchantDeleted>();

            merchantDeletedEvent.Should().BeEquivalentTo(MerchantDeletedEvent);
        }
    }
}
