using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Infrastructure.Persistence;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Application.UnitTests.Feature.Helpers
{
    public class MockFeaturePersistenceContext : Mock<IFeaturePersistenceContext>
    {
        private readonly List<FeatureDocument> _database = new();

        public MockFeaturePersistenceContext()
        {
            Setup(c => c.Count(It.IsAny<string>())).ReturnsAsync((string feature) =>
                _database.Count(f => f.Feature == feature));

            Setup(c => c.Enrol(It.IsAny<string>(), It.IsAny<string>())).Callback((string feature, string userId) =>
                _database.Add(new FeatureDocument { Feature = feature, CognitoId = userId }));

            Setup(c => c.Get(It.IsAny<string>())).ReturnsAsync((string userId) =>
                _database.SingleOrDefault(f => f.CognitoId == userId)?.Feature);
        }
    }
}
