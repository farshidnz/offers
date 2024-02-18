using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Infrastructure.Models;
using Mapster;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class MerchantsPersistenceContext : IMerchantsPersistenceContext
    {
        private readonly IDocumentPersistenceContext<MerchantDocument> _documentPersistenceContext;

        public MerchantsPersistenceContext(IDocumentPersistenceContext<MerchantDocument> documentPersistenceContext)
        {
            _documentPersistenceContext = documentPersistenceContext;
        }

        public async Task<List<Merchant>> GetAllMerchants() =>
            (await _documentPersistenceContext.Find(Builders<MerchantDocument>.Filter.Empty)).Adapt<List<Merchant>>();

        public async Task<List<Merchant>> GetMerchants(int clientId, int? premiumClientId, List<int> merchantIds)
        {
            var andFilters = new List<FilterDefinition<MerchantDocument>>
            {
                Builders<MerchantDocument>.Filter.Eq("Client", clientId),
                Builders<MerchantDocument>.Filter.Eq("PremiumClient", premiumClientId)
            };

            andFilters.Add(Builders<MerchantDocument>.Filter.In("MerchantId", merchantIds));

            var filter = Builders<MerchantDocument>.Filter.And(andFilters);
            var documents = await _documentPersistenceContext.Find(filter);
            return documents.Adapt<List<Merchant>>();
        }

        public async Task InsertMerchant(Merchant merchant)
        {
            await _documentPersistenceContext.Insert(merchant.Adapt<MerchantDocument>(), merchant.DomainEvents.ToList());
        }

        public async Task UpdateMerchant(Merchant merchant)
        {
            await _documentPersistenceContext.Replace(merchant.Adapt<MerchantDocument>(), merchant.DomainEvents.ToList());
        }

        public async Task DeleteMerchant(Merchant merchant)
        {
            await _documentPersistenceContext.Delete(merchant.Adapt<MerchantDocument>(), merchant.DomainEvents.ToList());
        }
    }
}
