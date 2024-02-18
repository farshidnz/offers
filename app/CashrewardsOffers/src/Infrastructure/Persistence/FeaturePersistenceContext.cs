using CashrewardsOffers.Application.Common.Interfaces;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class FeaturePersistenceContext : IFeaturePersistenceContext
    {
        private readonly IDocumentPersistenceContext<FeatureDocument> _documentPersistenceContext;

        public FeaturePersistenceContext(IDocumentPersistenceContext<FeatureDocument> documentPersistenceContext)
        {
            _documentPersistenceContext = documentPersistenceContext;
        }

        public async Task<long> Count(string feature) =>
            await _documentPersistenceContext.Count(Builders<FeatureDocument>.Filter.Eq("Feature", feature));

        public async Task Enrol(string feature, string cognitoId) =>
            await _documentPersistenceContext.Insert(new FeatureDocument
            {
                CognitoId = cognitoId,
                Feature = feature
            });

        public async Task<string> Get(string cognitoId)
        {
            var result = await _documentPersistenceContext.FindFirst(Builders<FeatureDocument>.Filter.Eq("CognitoId", cognitoId));

            return result?.Feature;
        }
    }
}
