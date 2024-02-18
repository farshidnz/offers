using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrustructure.UnitTests.Persistence
{
    public class DocumentDbPersistenceContextTests
    {
        public class TestDoc : IDocument
        {
            public ObjectId _id { get; set; }
            public string SomeProperty { get; set; }
        }

        private class TestState
        {
            public DocumentDbPersistenceContext<TestDoc> DocumentDbPersistenceContext { get; }

            public string UpdateDefinition { get; private set; }

            public TestState()
            {
                var collection = new Mock<IMongoCollection<TestDoc>>();
                collection
                    .Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<TestDoc>>(), It.IsAny<UpdateDefinition<TestDoc>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
                    .Callback((FilterDefinition<TestDoc> filter, UpdateDefinition<TestDoc> updateDefinition, UpdateOptions updateOptions, CancellationToken cancellationToken) =>
                    {
                        var serializerRegistry = BsonSerializer.SerializerRegistry;
                        var documentSerializer = serializerRegistry.GetSerializer<TestDoc>();
                        UpdateDefinition = updateDefinition.Render(documentSerializer, serializerRegistry).ToString();
                    });

                var database = new Mock<IMongoDatabase>();
                database.Setup(d => d.GetCollection<TestDoc>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>())).Returns(collection.Object);
                var documentDbConnection = new Mock<IDocumentDbConnection>();
                documentDbConnection.Setup(c => c.Database).Returns(database.Object);

                DocumentDbPersistenceContext = new DocumentDbPersistenceContext<TestDoc>(
                    Mock.Of<IConfiguration>(),
                    documentDbConnection.Object,
                    Mock.Of<IEventOutboxPersistenceContext>());
            }
        }

        [Test]
        public async Task Update_ShouldBuildUpdateDefinition()
        {
            var state = new TestState();

            await state.DocumentDbPersistenceContext.Update(FilterDefinition<TestDoc>.Empty, ("SomeProperty", "SomeValue"));

            state.UpdateDefinition.Should().Be(@"{ ""$set"" : { ""SomeProperty"" : ""SomeValue"" } }");
        }
    }
}
