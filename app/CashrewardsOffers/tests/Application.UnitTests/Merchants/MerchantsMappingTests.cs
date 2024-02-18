using CashrewardsOffers.Application.Merchants.Models;
using CashrewardsOffers.Application.Merchants.Queries.GetEdmMerchants.v1;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using Mapster;
using NUnit.Framework;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class MerchantsMappingTests
    {
        static ShopGoMerchant ShopGoMerchant => new()
        {
            ClientId = 1000000,
            MerchantId = 222,
            HyphenatedString = "merchant-hyphenated-string",
            RegularImageUrl = "regular-image-url",
            ClientProgramTypeId = 101,
            TierCommTypeId = 100,
            TierTypeId = 118,
            Commission = 95,
            ClientComm = 90,
            MemberComm = 85,
            RewardName = "cashback",
            IsFlatRate = true,
            Rate = 7.5m,
            NetworkId = 1000053,
            IsFeatured = true,
            IsHomePageFeatured = true,
            IsPopular = true,
            MobileEnabled = true,
            IsMobileAppEnabled = true,
            IsPremiumDisabled = true,
            IsPaused = true,
            BasicTerms = "basic terms",
            ExtentedTerms = "extended terms"
        };

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

        static MerchantPremium DomainMerchantPremium => new()
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

        static EdmMerchantInfo EdmMerchantInfo => new()
        {
            MerchantId = 222,
            LogoUrl = "regular-image-url",
            ClientCommissionString = "$72.68 cashback",
            HyphenatedString = "merchant-hyphenated-string",
            Premium = new EdmMerchantPremiumInfo
            {
                ClientCommissionString = "$75.13 cashback"
            }
        };

        [Test]
        public void Adapt_ShouldMapToDomainMerchant_GivenShopGoMerchant()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var domainMerchant = ShopGoMerchant.BuildAdapter().AdaptToType<Merchant>();

            var expectedDomainMerchant = DomainMerchant();
            expectedDomainMerchant.Premium = null;
            domainMerchant.Should().BeEquivalentTo(expectedDomainMerchant, opts => opts.Excluding(s => s.PopularMerchantRankingForBrowser).Excluding(s => s.PopularMerchantRankingForMobile));
        }

        [Test]
        public void Adapt_ShouldMapToDomainMerchantPremium_GivenDomainMerchant()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var domainMerchantPremium = DomainMerchant().Adapt<MerchantPremium>();

            domainMerchantPremium.Should().BeEquivalentTo(DomainMerchantPremium);
        }

        [Test]
        public void Adapt_ShouldMapToEdmMerchantInfo_GivenDomainMerchant()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var edmMerchantInfo = DomainMerchant().BuildAdapter().AddParameters("isFeatured", true).AdaptToType<EdmMerchantInfo>();

            edmMerchantInfo.Should().BeEquivalentTo(EdmMerchantInfo);
        }
    }
}
