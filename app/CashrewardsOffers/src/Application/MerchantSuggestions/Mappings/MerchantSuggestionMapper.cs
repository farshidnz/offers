using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashrewardsOffers.Application.MerchantSuggestions.Queries.GetMerchantSuggestions.v1;
using CashrewardsOffers.Domain.Entities;
using Mapster;

namespace CashrewardsOffers.Application.MerchantSuggestions.Mappings
{
    public interface IMerchantSuggestionMapper
    {
        (int Count, int TotalCount, IEnumerable<MerchantSuggestion> Merchants) MapFromRankedMerchants(IEnumerable<RankedMerchant> src, int startingRank,
            int pageSize, int[] categories);
    }

    public class MerchantSuggestionMapper : IMerchantSuggestionMapper
    {
        public (int Count, int TotalCount, IEnumerable<MerchantSuggestion> Merchants) MapFromRankedMerchants(IEnumerable<RankedMerchant> src, int startingRank,
            int pageSize, int[] categories)
        {
            var merchantSrc = src.ToList();
            Dictionary<int, List<RankedMerchant>> merchantRanking = new Dictionary<int, List<RankedMerchant>>();

            merchantSrc.ForEach(m =>
            {
                var merchantRank = (categories != null && categories.Length > 0 && categories.Contains(m.CategoryId))
                    ? m.GeneratedRank
                    : m.GeneratedRank + merchantSrc.Count;

                if (!merchantRanking.ContainsKey(merchantRank))
                {
                    merchantRanking[merchantRank] = new List<RankedMerchant>();
                }

                merchantRanking[merchantRank].Add(m);
            });

            var orderedList = merchantRanking
                .OrderBy(kvp => kvp.Key)
                .SelectMany(kvp => kvp.Value)
                .GroupBy(m => m.MerchantName)
                .Select(g => g.First())
                .ToList();

            if (startingRank > orderedList.Count)
            {
                return (0, 0, new List<MerchantSuggestion>());
            }

            var finalList = orderedList.Skip(startingRank - 1)
                .Take(pageSize)
                .Adapt<List<MerchantSuggestion>>();

            int idx = 1;
            finalList.ForEach(m => m.Rank = idx++);
            return (finalList.Count, orderedList.Count, finalList);
        }
    }
}
