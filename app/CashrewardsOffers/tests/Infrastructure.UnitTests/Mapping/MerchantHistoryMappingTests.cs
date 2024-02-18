using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure;
using CashrewardsOffers.Infrastructure.Models;
using FluentAssertions;
using Mapster;
using MongoDB.Bson;
using NUnit.Framework;

namespace CashrewardsOffers.Infrustructure.UnitTests.Mapping
{
    public class MerchantHistoryMappingTests
    {
        private static MerchantHistory MerchantHistoryDomainEntity =>
            new MerchantHistory
            {
                Id = "62959692f37aeacd7919d629",
                Client = Client.Cashrewards,
                MerchantId = 1,
                HyphenatedString = "merchant-1",
                ClientCommissionString = "2%"
            };

        private static MerchantHistoryDocument MerchantHistoryDocument =>
            new MerchantHistoryDocument
            {
                _id = new ObjectId("62959692f37aeacd7919d629"),
                Client = Client.Cashrewards,
                MerchantId = 1,
                HyphenatedString = "merchant-1",
                ClientCommissionString = "2%"
            };

        [Test]
        public void Adapt_ShouldMapToDocument_GivenDomainEntity()
        {
            DependencyInjection.RegisterMappingProfiles();

            var document = MerchantHistoryDomainEntity.Adapt<MerchantHistoryDocument>();

            document.Should().BeEquivalentTo(MerchantHistoryDocument);
        }

        [Test]
        public void Adapt_ShouldMapToEntity_GivenDocument()
        {
            DependencyInjection.RegisterMappingProfiles();

            var document = MerchantHistoryDocument.Adapt<MerchantHistory>();

            document.Should().BeEquivalentTo(MerchantHistoryDomainEntity);
        }
    }
}
