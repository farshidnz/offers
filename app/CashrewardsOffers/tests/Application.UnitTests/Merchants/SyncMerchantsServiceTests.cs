using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Merchants.Models;
using CashrewardsOffers.Application.Merchants.Services;
using CashrewardsOffers.Application.Offers.Services;
using CashrewardsOffers.Application.UnitTests.Common.TestHelpers;
using CashrewardsOffers.Application.UnitTests.Merchants.Helpers;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class SyncMerchantsServiceTests
    {
        private class TestState
        {
            public SyncMerchantsService SyncMerchantsService { get; }
            public Mock<IShopGoSource> ShopGoSource { get; } = new();
            public MockMerchantsPersistenceContext MerchantsPersistenceContext { get; } = new();
            private List<ShopGoCategory> ShopGoCategories { get; } = new();

            public TestState()
            {
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Infrastructure"));

                SyncMerchantsService = new SyncMerchantsService(ShopGoSource.Object, MerchantsPersistenceContext.Object, Mock.Of<IPopularMerchantRankingService>());
            }

            public void SetupCategory(int categoryId, string name)
            {
                ShopGoCategories.Add(new ShopGoCategory() { CategoryId = categoryId, Name = name });
            }
        }

        private ShopGoMerchant ShopGoMerchantFor(Client client, int merchantId = 123) => new()
        {
            ClientId = (int)client,
            MerchantId = merchantId,
            ClientComm = 95,
            MemberComm = 95,
            Commission = 95
        };

        private static MerchantCategory Category(int id, string name)
        {
            return new MerchantCategory() { CategoryId = id, Name = name };
        }

        private static MerchantCategory[] Categories(params (int id, string name)[] p)
        {
            return p.Select(i => Category(i.id, i.name)).ToArray();
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldAddMerchantForCrOnly_AndMerchantForCrAnzPremium_GivenCashrewardsMerchant()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant> { ShopGoMerchantFor(Client.Cashrewards, 123) });

            await state.SyncMerchantsService.SyncMerchantsAsync();

            var merchants = await state.MerchantsPersistenceContext.Object.GetAllMerchants();
            merchants.Count.Should().Be(2);
            merchants.Single(o => o.Key == (Client.Cashrewards, null, 123)).MerchantId.Should().Be(123);
            merchants.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).MerchantId.Should().Be(123);
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldAddMerchantForCrAnzPremium_GivenAnzOnlyMerchant()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant> { ShopGoMerchantFor(Client.Anz, 123) });

            await state.SyncMerchantsService.SyncMerchantsAsync();

            var merchants = await state.MerchantsPersistenceContext.Object.GetAllMerchants();
            merchants.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).MerchantId.Should().Be(123);
            merchants.Count.Should().Be(1);
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldAddMerchantForCrOnly_AndMerchantForCrAnzPremium_GivenCrAndAnzMerchantsWithTheSameMerchantId()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant>
            {
                ShopGoMerchantFor(Client.Cashrewards, 123),
                ShopGoMerchantFor(Client.Anz, 123),
            });

            await state.SyncMerchantsService.SyncMerchantsAsync();

            var merchants = await state.MerchantsPersistenceContext.Object.GetAllMerchants();
            merchants.Single(o => o.Key == (Client.Cashrewards, null, 123)).MerchantId.Should().Be(123);
            merchants.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).MerchantId.Should().Be(123);
            merchants.Count.Should().Be(2);
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldSetBaseRateForMerchant_GivenAnzOnlyMerchant()
        {
            var state = new TestState();
            var shopGoMerchant = ShopGoMerchantFor(Client.Anz, 123);
            shopGoMerchant.ClientComm = 98;
            shopGoMerchant.MemberComm = 98;
            shopGoMerchant.Commission = 98;
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant> { shopGoMerchant });
            state.ShopGoSource.Setup(s => s.GetMerchantBaseRatesById(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ShopGoMerchantBaseRate> { new ShopGoMerchantBaseRate { MerchantId = 123, ClientComm = 95, MemberComm = 95, Commission = 95 } });

            await state.SyncMerchantsService.SyncMerchantsAsync();

            var merchants = await state.MerchantsPersistenceContext.Object.GetAllMerchants();
            var premiumMerchant = merchants.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123));
            premiumMerchant.ClientComm.Should().Be(95);
            premiumMerchant.MemberComm.Should().Be(95);
            premiumMerchant.Commission.Should().Be(95);
            premiumMerchant.Premium.ClientComm.Should().Be(98);
            premiumMerchant.Premium.MemberComm.Should().Be(98);
            premiumMerchant.Premium.Commission.Should().Be(98);
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldExcludePremiumMerchant_GivenPremiumDisabledMerchant()
        {
            var state = new TestState();
            var shopGoMerchant = ShopGoMerchantFor(Client.Anz, 123);
            shopGoMerchant.IsPremiumDisabled = true;
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant> { shopGoMerchant });

            await state.SyncMerchantsService.SyncMerchantsAsync();

            var merchants = await state.MerchantsPersistenceContext.Object.GetAllMerchants();
            merchants.SingleOrDefault(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).Should().BeNull();
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldIncludeBaseMerchant_GivenPremiumDisabledMerchant()
        {
            var state = new TestState();
            var shopGoMerchant = ShopGoMerchantFor(Client.Cashrewards, 123);
            shopGoMerchant.IsPremiumDisabled = true;
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant> { shopGoMerchant });

            await state.SyncMerchantsService.SyncMerchantsAsync();

            var merchants = await state.MerchantsPersistenceContext.Object.GetAllMerchants();
            merchants.Count(o => o.Key == (Client.Cashrewards, null, 123)).Should().Be(1);
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldRemoveDuplicateMerchants_GivenMerchantsWithTheSameKey()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant> { ShopGoMerchantFor(Client.Cashrewards, 123) });
            state.MerchantsPersistenceContext.Setup(c => c.GetAllMerchants()).ReturnsAsync(new List<Merchant>
            {
                new Merchant { Id = "1", Client = Client.Cashrewards, MerchantId = 123 },
                new Merchant { Id = "2", Client = Client.Cashrewards, MerchantId = 123 }
            });

            await state.SyncMerchantsService.SyncMerchantsAsync();

            state.MerchantsPersistenceContext.Verify(c => c.DeleteMerchant(It.Is<Merchant>(o => o.Id == "2")), Times.Once);
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldMapCategoriesFromShopGo()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetMerchants()).ReturnsAsync(new List<ShopGoMerchant> { ShopGoMerchantFor(Client.Cashrewards, 123) });
            state.MerchantsPersistenceContext.Setup(c => c.GetAllMerchants()).ReturnsAsync(new List<Merchant>
            {
                new Merchant { Id = "1", Client = Client.Cashrewards, MerchantId = 123, Categories = Categories((100, "cat1"), (101, "cat2")) },
                new Merchant { Id = "2", Client = Client.Cashrewards, MerchantId = 124, Categories = Categories((101, "cat2"), (102, "cat3"))}
            });

            state.SetupCategory(100, "cat1");
            state.SetupCategory(101, "cat2");
            state.SetupCategory(102, "cat3");

            await state.SyncMerchantsService.SyncMerchantsAsync();


            var merchants = await state.MerchantsPersistenceContext.Object.GetAllMerchants();
            merchants.Count.Should().Be(2);

            var merchant1 = merchants.Single(o => o.Key == (Client.Cashrewards, null, 123));
            merchant1.Categories[0].CategoryId.Should().Be(100);
            merchant1.Categories[0].Name.Should().Be("cat1");
            merchant1.Categories[1].CategoryId.Should().Be(101);
            merchant1.Categories[1].Name.Should().Be("cat2");

            var merchant2 = merchants.Single(o => o.Key == (Client.Cashrewards, null, 124));
            merchant2.Categories[0].CategoryId.Should().Be(101);
            merchant2.Categories[0].Name.Should().Be("cat2");
            merchant2.Categories[1].CategoryId.Should().Be(102);
            merchant2.Categories[1].Name.Should().Be("cat3");
        }

    }
}
