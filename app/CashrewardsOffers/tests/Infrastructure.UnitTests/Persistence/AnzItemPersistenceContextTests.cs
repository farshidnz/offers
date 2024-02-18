using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Infrastructure;
using CashrewardsOffers.Infrastructure.Models;
using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Persistence
{
    public class AnzItemPersistenceContextTests
    {
        private class TestState
        {
            public AnzItemPersistenceContext AnzItemPersistenceContext { get; }

            public Mock<IDocumentPersistenceContext<AnzItemDocument>> DocumentPersistenceContext { get; } = new();

            public string FindFilter { get; set; }

            public FindOptions<AnzItemDocument, AnzItemDocument> FindOptions { get; set; }

            public TestState()
            {
                DependencyInjection.RegisterMappingProfiles();

                DocumentPersistenceContext
                    .Setup(d => d.Find(It.IsAny<FilterDefinition<AnzItemDocument>>(), It.IsAny<FindOptions<AnzItemDocument, AnzItemDocument>>()))
                    .Callback((FilterDefinition<AnzItemDocument> filter, FindOptions<AnzItemDocument, AnzItemDocument> options) =>
                    {
                        var serializerRegistry = BsonSerializer.SerializerRegistry;
                        var documentSerializer = serializerRegistry.GetSerializer<AnzItemDocument>();
                        FindFilter = filter.Render(documentSerializer, serializerRegistry).ToString();
                        FindOptions = options;
                    });

                AnzItemPersistenceContext = new AnzItemPersistenceContext(DocumentPersistenceContext.Object, new AnzItemFactory());
            }
        }

        [Test]
        public async Task GetAll_ShouldCallFindWithApplicableFilters()
        {
            var state = new TestState();

            await state.AnzItemPersistenceContext.GetAllActiveCarouselItems();

            state.DocumentPersistenceContext.Verify(d => d.Find(It.IsAny<FilterDefinition<AnzItemDocument>>(), It.IsAny<FindOptions<AnzItemDocument, AnzItemDocument>>()));
            state.FindFilter.Should().Be(
                @"{ " +
                @"""IsDeleted"" : false, " +
                @"""Merchant.MobileEnabled"" : true, " +
                @"""Merchant.IsPremiumDisabled"" : false, " +
                @"""Merchant.IsPaused"" : { ""$ne"" : true }, " +
                @"""$or"" : [" +
                @"{ ""Offer._id"" : 0, ""Merchant.NetworkId"" : 1000053 }, " +
                @"{ ""Offer._id"" : 0, ""Merchant.IsPopularFlag"" : true, ""Merchant.PopularMerchantRankingForBrowser"" : { ""$gt"" : 0 } }, " +
                @"{ ""Offer.IsFeatured"" : true }, " +
                @"{ ""Offer.IsExclusive"" : true }] " +
                @"}");
        }

        [Test]
        public async Task GetPage_ShouldCallFindWithApplicableFilters()
        {
            var state = new TestState();

            var checkResults = () =>
            {
                state.DocumentPersistenceContext.Verify(d => d.Find(It.IsAny<FilterDefinition<AnzItemDocument>>(), It.IsAny<FindOptions<AnzItemDocument, AnzItemDocument>>()));
                state.FindFilter.Should().Be(
                @"{ " +
                @"""IsDeleted"" : false, " +
                @"""Merchant.MobileEnabled"" : true, " +
                @"""Merchant.IsPremiumDisabled"" : false, " +
                @"""Merchant.IsPaused"" : { ""$ne"" : true }, " +
                @"""$or"" : [" +
                @"{ ""Offer._id"" : 0, ""Merchant.NetworkId"" : 1000053 }, " +
                @"{ ""Offer._id"" : 0, ""Merchant.IsPopularFlag"" : true, ""Merchant.PopularMerchantRankingForBrowser"" : { ""$gt"" : 0 } }, " +
                @"{ ""Offer.IsFeatured"" : true }, " +
                @"{ ""Offer.IsExclusive"" : true }] " +
                @"}");
            };

            await state.AnzItemPersistenceContext.GetPage();
            checkResults();
            await state.AnzItemPersistenceContext.GetPage(1);
            checkResults();
            await state.AnzItemPersistenceContext.GetPage(1, 2);
            checkResults();
            await state.AnzItemPersistenceContext.GetPage(2, 4);
            checkResults();
            await state.AnzItemPersistenceContext.GetPage(2, 1);
            checkResults();
        }

        [Test]
        public async Task GetCount_ShouldNotBeNegative()
        {
            var state = new TestState();
            var count = await state.AnzItemPersistenceContext.GetCount();
            count.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
