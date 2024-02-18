using CashrewardsOffers.Application.MerchantSuggestions.Services;
using CashrewardsOffers.Application.Offers.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using CashrewardsOffers.Application.EDM;
using CashrewardsOffers.Application.Merchants.Models;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IShopGoSource
    {
        Task<IEnumerable<ShopGoOffer>> GetOffers();
        Task<IEnumerable<ShopGoRankedMerchant>> GetRankedMerchants();
        Task<IEnumerable<ShopGoMerchantBaseRate>> GetMerchantBaseRatesById(IEnumerable<int> merchantIds, int clientId);
        Task<IEnumerable<ShopGoFavourite>> GetFavourites(string cognitoId);
        Task<IEnumerable<ShopGoFavourite>> GetFavouritesByNewMemberId(string newMemberId);
        Task<IEnumerable<ShopGoCategory>> GetMerchantCategories();
        Task<IEnumerable<ShopGoMerchant>> GetMerchants();
        Task<IEnumerable<ShopGoEdmItem>> GetEdmCampaignItems(int campaignId);
        Task<IEnumerable<ShopGoTier>> GetTiers();

        Task<string> LookupCognitoIdFromPersonId(string personId);
        Task<string> LookupCognitoIdFromMemberId(string memberId);
        Task<string> LookupCognitoIdFromNewMemberId(string newMemberId);
        Task<int> GetPremiumStatus(string newMemberId);
        Task<ShopGoPerson> GetPersonFromNewMemberId(string newMemberId);
    }
}
