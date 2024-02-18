using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure;
using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using Mapster;
using MongoDB.Bson;
using NUnit.Framework;

namespace CashrewardsOffers.Infrustructure.UnitTests.Mapping
{
    public class MerchantMappingTests
    {
        static Merchant MerchantDomainEntity(string id = null) => new()
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
            PopularMerchantRankingForBrowser = 5,
            PopularMerchantRankingForMobile = 7,
            BasicTerms = "basic terms",
            IsPaused = true,
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

        static MerchantDocument MerchantDocument(ObjectId id) => new()
        {
            _id = id,
            MerchantId = 222,
            Client = 1000000,
            PremiumClient = null,
            HyphenatedString = "merchant-hyphenated-string",
            LogoUrl = "regular-image-url",
            Commission = 95,
            ClientComm = 90,
            MemberComm = 85,
            RewardType = 118,
            ClientProgramType = 101,
            CommissionType = 100,
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
            PopularMerchantRankingForBrowser = 5,
            PopularMerchantRankingForMobile = 7,
            BasicTerms = "basic terms",
            ExtentedTerms = "extended terms",
            IsPaused = true,
            Premium = new PremiumMerchantDocument
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

        [Test]
        public void Adapt_ShouldMapToMerchantDocument_GivenMerchantDomainEntity()
        {
            DependencyInjection.RegisterMappingProfiles();

            var merchantDocument = MerchantDomainEntity().Adapt<MerchantDocument>();

            merchantDocument.Should().BeEquivalentTo(MerchantDocument(ObjectId.Empty));
        }

        [Test]
        public void Adapt_ShouldMapToMerchantDomainEntity_GivenMerchantDocument()
        {
            DependencyInjection.RegisterMappingProfiles();

            var domainMerchant = MerchantDocument(ObjectId.Parse("000000000000000000000001")).Adapt<Merchant>();

            domainMerchant.Should().BeEquivalentTo(MerchantDomainEntity("000000000000000000000001"));
        }
    }
}
