using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace CashrewardsOffers.Application.UnitTests.Merchants.Helpers
{
    public class MockMerchantsPersistenceContext : Mock<IMerchantsPersistenceContext>
    {
        private readonly Dictionary<(Client, Client?, int), Merchant> _database = new();

        public MockMerchantsPersistenceContext()
        {
            Setup(c => c.DeleteMerchant(It.IsAny<Merchant>())).Callback((Merchant merchant) => _database.Remove(merchant.Key));

            Setup(c => c.GetAllMerchants()).ReturnsAsync(() => _database.Values.ToList());

            Setup(c => c.GetMerchants(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<List<int>>()))
                .ReturnsAsync((int clientId, int? premiumClientId, List<int> merchantIds) =>
                    _database.Values.Where(
                        m => m.Client == (Client)clientId &&
                        m.PremiumClient == (Client?)premiumClientId &&
                        merchantIds.Contains(m.MerchantId)
                    ).ToList());

            Setup(c => c.InsertMerchant(It.IsAny<Merchant>())).Callback((Merchant merchant) => _database.Add(merchant.Key, merchant));

            Setup(c => c.UpdateMerchant(It.IsAny<Merchant>())).Callback((Merchant merchant) =>
            {
                _database.Remove(merchant.Key);
                _database.Add(merchant.Key, merchant);
            });
        }
    }
}
