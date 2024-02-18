using CashrewardsOffers.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Merchants.Services
{
    public interface IPopularMerchantRankingService
    {
        Task LoadRankings();
        int GetBrowserRanking(int merchantId);
        int GetMobileRanking(int merchantId);
    }

    public class PopularMerchantRankingService : IPopularMerchantRankingService
    {
        private Dictionary<int, int> _popularRankForBrowser;
        private Dictionary<int, int> _popularRankForMobile;
        private readonly IPopularMerchantSource _popularMerchantSource;

        public PopularMerchantRankingService(
            IPopularMerchantSource popularMerchantSource)
        {
            _popularMerchantSource = popularMerchantSource;
        }

        public async Task LoadRankings()
        {
            _popularRankForBrowser = await LoadRankings(PopularMerchantTarget.Browser);
            _popularRankForMobile = await LoadRankings(PopularMerchantTarget.Mobile);
        }

        private async Task<Dictionary<int, int>> LoadRankings(string target) =>
            (await _popularMerchantSource.GetPopularMerchantIds(target))
                .Select((id, index) => (id, index))
                .ToDictionary(rank => rank.id, rank => rank.index + 1);

        public int GetBrowserRanking(int merchantId) => GetRanking(_popularRankForBrowser, merchantId);

        public int GetMobileRanking(int merchantId) => GetRanking(_popularRankForMobile, merchantId);

        private static int GetRanking(Dictionary<int, int> lookup, int merchantId) =>
            lookup.TryGetValue(merchantId, out var rank) ? rank : 0;
    }
}
