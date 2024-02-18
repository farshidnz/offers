using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using Mapster;
using MongoDB.Driver;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class RankedMerchantPersistenceContext : IRankedMerchantPersistenceContext
    {
        private readonly IDocumentPersistenceContext<RankedMerchantDocument> _documentPersistenceContext;

        public RankedMerchantPersistenceContext(IDocumentPersistenceContext<RankedMerchantDocument> documentPersistenceContext)
        {
            _documentPersistenceContext = documentPersistenceContext;
        }

        public async Task<IEnumerable<RankedMerchant>> GetAllRankedMerchants() =>
            (await _documentPersistenceContext.Find(Builders<RankedMerchantDocument>.Filter.Empty)).Adapt<List<RankedMerchant>>();

        public async Task<IEnumerable<RankedMerchant>> GetRankedMerchants(int clientId, int? premiumClientId, int[] categoryIds)
        {
            var andFilters = new List<FilterDefinition<RankedMerchantDocument>>();

            /*
            if (categoryIds != null && categoryIds.Length > 0)
            {
                andFilters.Add(Builders<RankedMerchantDocument>.Filter.In("CategoryId", categoryIds));
            }
            */

            if (premiumClientId.HasValue)
            {
                andFilters.Add(Builders<RankedMerchantDocument>.Filter.Eq("IsPremiumDisabledMerchant", false));
            }

            var filter = andFilters.Count > 0 
                ? Builders<RankedMerchantDocument>.Filter.And(andFilters)
                : Builders<RankedMerchantDocument>.Filter.Empty;
            var documents = await _documentPersistenceContext.Find(filter);
            return documents.Adapt<List<RankedMerchant>>();
        }

        public async Task InsertRankedMerchant(RankedMerchant rankedMerchant)
        {
            await _documentPersistenceContext.Insert(rankedMerchant.Adapt<RankedMerchantDocument>());
        }

        public async Task UpdateRankedMerchant(RankedMerchant rankedMerchant)
        {
            await _documentPersistenceContext.Replace(rankedMerchant.Adapt<RankedMerchantDocument>());
        }

        public async Task DeleteRankedMerchant(RankedMerchant rankedMerchant)
        {
            await _documentPersistenceContext.Delete(rankedMerchant.Adapt<RankedMerchantDocument>());
        }
    }
}
