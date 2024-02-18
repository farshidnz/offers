using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Application.UnitTests.MerchantSuggestions.Helpers
{
    public class MockRankedMerchantPersistenceContext : Mock<IRankedMerchantPersistenceContext>
    {
        private readonly Dictionary<(int, string), RankedMerchant> _database = new();

        public MockRankedMerchantPersistenceContext()
        {
            Setup(c => c.GetAllRankedMerchants()).ReturnsAsync(() => _database.Values.ToList());
            Setup(c => c.GetRankedMerchants(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int[]>())).ReturnsAsync((int clientId, int? premiumClientId, int[] categoryIds) =>
                _database
                    .Values
                    .Where(c => categoryIds.Contains(c.CategoryId))
                    .Where(c => !premiumClientId.HasValue || !c.IsPremiumDisabled)
                    .ToList()
            );
            Setup(c => c.InsertRankedMerchant(It.IsAny<RankedMerchant>())).Callback((RankedMerchant rankedMerchant) =>
            {
                _database[rankedMerchant.Key] = rankedMerchant;
            });
            Setup(c => c.UpdateRankedMerchant(It.IsAny<RankedMerchant>())).Callback((RankedMerchant rankedMerchant) =>
            {
                _database[rankedMerchant.Key] = rankedMerchant;
            });
            Setup(c => c.DeleteRankedMerchant(It.IsAny<RankedMerchant>())).Callback((RankedMerchant rankedMerchant) =>
            {
                _database.Remove(rankedMerchant.Key);
            });
        }
    }
}
