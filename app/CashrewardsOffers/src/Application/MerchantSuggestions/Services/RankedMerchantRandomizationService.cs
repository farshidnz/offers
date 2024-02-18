using CashrewardsOffers.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Application.MerchantSuggestions.Services
{
    public interface IRankedMerchantRandomizationService
    {
        IEnumerable<RankedMerchant> ShuffleRankedMerchants(IEnumerable<RankedMerchant> input);
    }

    public class RankedMerchantRandomizationService : IRankedMerchantRandomizationService
    {
        public IEnumerable<RankedMerchant> ShuffleRankedMerchants(IEnumerable<RankedMerchant> input)
        {
            var random = new Random();
            var rankedMerchants = input.ToList();
            var categories = rankedMerchants.GroupBy(i => i.CategoryName);
            var outputList = new List<RankedMerchant>();

            foreach (var category in categories)
            {
                var merchantsInCategory = category.ToList();

                var numberOfMerchants = merchantsInCategory.Count;

                var recordsToShuffle = Math.Floor((float)numberOfMerchants * 20 / 100);
                recordsToShuffle = recordsToShuffle % 2 == 0 ? recordsToShuffle : recordsToShuffle + 1;
                Shuffle(random, merchantsInCategory, numberOfMerchants, recordsToShuffle);

                int index = 1;
                merchantsInCategory.ForEach(m =>
                {
                    m.GeneratedRank = index++;
                });

                outputList.AddRange(merchantsInCategory);
            }

            return outputList;
        }

        public virtual void Shuffle(Random random, List<RankedMerchant> merchantsInCategory, int numberOfMerchants, double recordsToShuffle)
        {
            for (int i = 0; i < recordsToShuffle / 2; i++)
            {
                int firstRecord = (int)Math.Floor(random.NextDouble() * (recordsToShuffle * 2));
                int secondRecord = (int)Math.Floor(random.NextDouble() * (float)numberOfMerchants);

                var temp = merchantsInCategory[firstRecord];
                merchantsInCategory[firstRecord] = merchantsInCategory[secondRecord];
                merchantsInCategory[secondRecord] = temp;
            }
        }
    }
}
