using CashrewardsOffers.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IOffersPersistenceContext
    {
        Task<List<Offer>> GetAllOffers();
        Task<Offer> GetOffer(int clientId, int? premiumClientId, int offerId);
        Task<List<Offer>> GetOffers(int clientId, int? premiumClientId, bool? isFeatured, bool isMobile, int[] excludedNetworkIds, int? categoryId, bool excludePausedMerchants = false);
        Task InsertOffer(Offer offer);
        Task UpdateOffer(Offer offer);
        Task DeleteOffer(Offer offer);
    }
}
