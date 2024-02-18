using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Infrastructure;
using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Persistence
{
    public class MerchantHistoryPersistenceContextTests
    {
        private class TestState
        {
            public MerchantHistoryPersistenceContext MerchantHistoryPersistenceContext { get; }

            public Mock<IDocumentPersistenceContext<MerchantHistoryDocument>> DocumentPersistenceContext { get; } = new();

            public TestState()
            {
                DependencyInjection.RegisterMappingProfiles();

                MerchantHistoryPersistenceContext = new MerchantHistoryPersistenceContext(DocumentPersistenceContext.Object);
            }
        }

        [Test]
        public async Task GetByDateRange_ShouldUseDateRangeFilter()
        {
            var state = new TestState();
            string calledFilter = null;
            state.DocumentPersistenceContext
                .Setup(d => d.Find(It.IsAny<FilterDefinition<MerchantHistoryDocument>>(), It.IsAny<FindOptions<MerchantHistoryDocument, MerchantHistoryDocument>>()))
                .Callback((FilterDefinition<MerchantHistoryDocument> filter, FindOptions<MerchantHistoryDocument, MerchantHistoryDocument> options) =>
                {
                    var serializerRegistry = BsonSerializer.SerializerRegistry;
                    var documentSerializer = serializerRegistry.GetSerializer<MerchantHistoryDocument>();
                    calledFilter = filter.Render(documentSerializer, serializerRegistry).ToString();
                });

            await state.MerchantHistoryPersistenceContext.GetByDateRange(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2022, 1, 2, 0, 0, 0, TimeSpan.Zero));

            calledFilter.Should().Be(@"{ ""ChangeTime"" : { ""$gte"" : [NumberLong(""637765920000000000""), 0], ""$lt"" : [NumberLong(""637766784000000000""), 0] } }");
        }

        [Test]
        public async Task DeleteByDateRange_ShouldUseDateRangeFilter()
        {
            var state = new TestState();
            string calledFilter = null;
            var collection = new Mock<IMongoCollection<MerchantHistoryDocument>>();
            state.DocumentPersistenceContext
                .Setup(d => d.Collection).Returns(collection.Object);
            collection
                .Setup(d => d.DeleteManyAsync(It.IsAny<FilterDefinition<MerchantHistoryDocument>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<DeleteResult>())
                .Callback((FilterDefinition<MerchantHistoryDocument> filter, CancellationToken token) =>
                {
                    var serializerRegistry = BsonSerializer.SerializerRegistry;
                    var documentSerializer = serializerRegistry.GetSerializer<MerchantHistoryDocument>();
                    calledFilter = filter.Render(documentSerializer, serializerRegistry).ToString();
                });

            await state.MerchantHistoryPersistenceContext.DeleteByDateRange(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2022, 1, 2, 0, 0, 0, TimeSpan.Zero));

            calledFilter.Should().Be(@"{ ""ChangeTime"" : { ""$gte"" : [NumberLong(""637765920000000000""), 0], ""$lt"" : [NumberLong(""637766784000000000""), 0] } }");
        }
    }
}
