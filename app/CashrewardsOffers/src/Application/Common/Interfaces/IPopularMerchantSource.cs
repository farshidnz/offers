using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Interfaces
{
    public static class PopularMerchantTarget
    {
        public const string Mobile = "popular-MOBILE_APP";
        public const string Browser = "popular-BROWSER_APP";
    }

    public interface IPopularMerchantSource
    {
        Task<List<int>> GetPopularMerchantIds(string target);
    }
}
