using CashrewardsOffers.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public interface IRankedMerchantPersistenceContext
    {
        Task<IEnumerable<RankedMerchant>> GetAllRankedMerchants();
        Task<IEnumerable<RankedMerchant>> GetRankedMerchants(int clientId, int? premiumClientId, int[] categoryIds);
        Task InsertRankedMerchant(RankedMerchant rankedMerchant);
        Task UpdateRankedMerchant(RankedMerchant rankedMerchant);
        Task DeleteRankedMerchant(RankedMerchant rankedMerchant);
    }
}
