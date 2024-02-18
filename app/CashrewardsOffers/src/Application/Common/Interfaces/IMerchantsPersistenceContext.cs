using CashrewardsOffers.Application.Offers.Services;
using CashrewardsOffers.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IMerchantsPersistenceContext
    {
        Task<List<Merchant>> GetAllMerchants();
        Task<List<Merchant>> GetMerchants(int clientId, int? premiumClientId, List<int> merchantIds);
        Task InsertMerchant(Merchant merchant);
        Task UpdateMerchant(Merchant merchant);
        Task DeleteMerchant(Merchant merchant);
    }
}
