using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using Mapster;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class OffersPersistenceContext : IOffersPersistenceContext
    {
        private readonly IDocumentPersistenceContext<OfferDocument> _documentPersistenceContext;

        public OffersPersistenceContext(IDocumentPersistenceContext<OfferDocument> documentPersistenceContext)
        {
            _documentPersistenceContext = documentPersistenceContext;
        }

        public async Task<List<Offer>> GetAllOffers() =>
            (await _documentPersistenceContext.Find(Builders<OfferDocument>.Filter.Empty)).Adapt<List<Offer>>();

        public async Task<Offer> GetOffer(int clientId, int? premiumClientId, int offerId)
        {
            var filter = Builders<OfferDocument>.Filter.And(
                Builders<OfferDocument>.Filter.Eq("Client", clientId),
                Builders<OfferDocument>.Filter.Eq("PremiumClient", premiumClientId),
                Builders<OfferDocument>.Filter.Eq("OfferId", offerId));
            var document = await _documentPersistenceContext.FindFirst(filter);
            return document.Adapt<Offer>();
        }

        public async Task<List<Offer>> GetOffers(int clientId, int? premiumClientId, bool? isFeatured, bool isMobile, int[] excludedNetworkIds, int? categoryId, bool excludePausedMerchants = false)
        {
            var andFilters = new List<FilterDefinition<OfferDocument>>
            {
                Builders<OfferDocument>.Filter.Eq("Client", clientId),
                Builders<OfferDocument>.Filter.Eq("PremiumClient", premiumClientId)
            };

            if (categoryId.HasValue)
            {
                andFilters.Add(Builders<OfferDocument>.Filter.Where(doc => doc.Merchant.CategoryObjects.Any(c => c.CategoryId == categoryId.Value)));

                if (isFeatured == true)
                {
                    andFilters.Add(Builders<OfferDocument>.Filter.Eq("IsCategoryFeatured", isFeatured.Value));
                }
            }
            else
            {
                if (isFeatured == true)
                {
                    andFilters.Add(Builders<OfferDocument>.Filter.Eq("IsFeatured", isFeatured.Value));
                }
            }

            if (isMobile)
            {
                andFilters.Add(Builders<OfferDocument>.Filter.Eq("Merchant.IsMobileAppEnabled", true));
            }

            if (excludedNetworkIds?.Length > 0)
            {
                andFilters.Add(Builders<OfferDocument>.Filter.Nin("Merchant.NetworkId", excludedNetworkIds));
            }

            if (excludePausedMerchants)
            {
                andFilters.Add(Builders<OfferDocument>.Filter.Eq("IsMerchantPaused", false));
            }

            var filter = Builders<OfferDocument>.Filter.And(andFilters);
            var documents = await _documentPersistenceContext.Find(filter);
            return documents.Adapt<List<Offer>>();
        }

        public async Task InsertOffer(Offer offer)
        {
            await _documentPersistenceContext.Insert(offer.Adapt<OfferDocument>(), offer.DomainEvents.ToList());
        }

        public async Task UpdateOffer(Offer offer)
        {
            await _documentPersistenceContext.Replace(offer.Adapt<OfferDocument>(), offer.DomainEvents.ToList());
        }

        public async Task DeleteOffer(Offer offer)
        {
            await _documentPersistenceContext.Delete(offer.Adapt<OfferDocument>(), offer.DomainEvents.ToList());
        }
    }
}
