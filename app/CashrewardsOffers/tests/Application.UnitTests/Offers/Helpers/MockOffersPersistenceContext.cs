using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Application.UnitTests.Offers.Helpers
{
    public class MockOffersPersistenceContext : Mock<IOffersPersistenceContext>
    {
        private readonly Dictionary<(Client, Client?, int), Offer> _database = new();

        public MockOffersPersistenceContext()
        {
            Setup(c => c.DeleteOffer(It.IsAny<Offer>())).Callback((Offer offer) => _database.Remove(offer.Key));

            Setup(c => c.GetAllOffers()).ReturnsAsync(() => _database.Values.ToList());

            Setup(c => c.GetOffer(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int>())).ReturnsAsync((int clientId, int? premiumClientId, int offerId) =>
                _database.TryGetValue(((Client)clientId, (Client?)premiumClientId, offerId), out var o) ? o : null);

            Setup(c => c.GetOffers(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<int[]>(), It.IsAny<int?>(), It.IsAny<bool>()))
                .ReturnsAsync((int clientId, int? premiumClientId, bool? isFeatured, bool isMobile, int[] excludedNetworkIds, int? categoryId, bool excludePausedMerchants) =>
                    _database.Values.Where(
                        o => o.Client == (Client)clientId &&
                        o.PremiumClient == (Client?)premiumClientId && 
                        (!excludePausedMerchants || !o.IsMerchantPaused) &&
                        (!categoryId.HasValue || o.Merchant.Categories.Any(c => c.CategoryId == categoryId.Value)) &&
                        (isFeatured != true || (!categoryId.HasValue && o.IsFeatured == isFeatured) || (categoryId.HasValue && o.IsCategoryFeatured == isFeatured))
                    ).ToList());

            Setup(c => c.InsertOffer(It.IsAny<Offer>())).Callback((Offer offer) => _database.Add(offer.Key, offer));

            Setup(c => c.UpdateOffer(It.IsAny<Offer>())).Callback((Offer offer) =>
            {
                _database.Remove(offer.Key);
                _database.Add(offer.Key, offer);
                DomainEventsGenerated.AddRange(offer.DomainEvents);
            });
        }

        public List<DomainEvent> DomainEventsGenerated { get; } = new();
    }
}
