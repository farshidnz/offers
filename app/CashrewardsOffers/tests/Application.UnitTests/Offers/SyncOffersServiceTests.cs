using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Offers.Services;
using CashrewardsOffers.Application.UnitTests.Common.TestHelpers;
using CashrewardsOffers.Application.UnitTests.Offers.Helpers;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Offers
{
    public class SyncOffersServiceTests
    {
        private class TestState
        {
            public SyncOffersService SyncOffersService { get; }
            public Mock<IShopGoSource> ShopGoSource { get; } = new();
            public MockOffersPersistenceContext OffersPersistenceContext { get; } = new();

            public TestState()
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.json", true)
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.Development.json", true)
                    .Build();

                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Domain"));
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Infrastructure"));

                ShopGoSource.Setup(s => s.GetTiers()).ReturnsAsync(new List<ShopGoTier>());

                SyncOffersService = new SyncOffersService(ShopGoSource.Object, OffersPersistenceContext.Object, configuration);
            }
        }

        private static ShopGoOffer ShopGoOfferFor(Client client, int offerId, int merchantId = 222) => new()
        {
            ClientId = (int)client,
            OfferId = offerId,
            MerchantId = merchantId,
            ClientComm = 95,
            MemberComm = 95,
            Commission = 95
        };

        [Test]
        public async Task SyncOffersAsync_ShouldAddOfferForCrOnly_AndOfferForCrAnzPremium_GivenCashrewardsOffer()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer> { ShopGoOfferFor(Client.Cashrewards, 123) });

            await state.SyncOffersService.SyncOffersAsync();

            var offers = await state.OffersPersistenceContext.Object.GetAllOffers();
            offers.Single(o => o.Key == (Client.Cashrewards, null, 123)).OfferId.Should().Be(123);
            offers.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).OfferId.Should().Be(123);
            offers.Count.Should().Be(2);
        }

        [Test]
        public async Task SyncOffersAsync_ShouldAddOfferForCrAnzPremium_GivenAnzOnlyOffer()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer> { ShopGoOfferFor(Client.Anz, 123) });

            await state.SyncOffersService.SyncOffersAsync();

            var offers = await state.OffersPersistenceContext.Object.GetAllOffers();
            offers.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).OfferId.Should().Be(123);
            offers.Count.Should().Be(1);
        }

        [Test]
        public async Task SyncOffersAsync_ShouldAddOfferForCrOnly_AndOfferForCrAnzPremium_GivenCrAndAnzOffersWithTheSameOfferId()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer>
            {
                ShopGoOfferFor(Client.Cashrewards, 123),
                ShopGoOfferFor(Client.Anz, 123),
            });

            await state.SyncOffersService.SyncOffersAsync();

            var offers = await state.OffersPersistenceContext.Object.GetAllOffers();
            offers.Single(o => o.Key == (Client.Cashrewards, null, 123)).OfferId.Should().Be(123);
            offers.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).OfferId.Should().Be(123);
            offers.Count.Should().Be(2);
        }

        [Test]
        public async Task SyncOffersAsync_ShouldSetBaseRateForMerchant_GivenAnzOnlyOffer()
        {
            var state = new TestState();
            var shopGoOffer = ShopGoOfferFor(Client.Anz, 123);
            shopGoOffer.ClientComm = 98;
            shopGoOffer.MemberComm = 98;
            shopGoOffer.Commission = 98;
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer> { shopGoOffer });
            state.ShopGoSource.Setup(s => s.GetMerchantBaseRatesById(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ShopGoMerchantBaseRate> { new ShopGoMerchantBaseRate { MerchantId = 222, ClientComm = 95, MemberComm = 95, Commission = 95 } });

            await state.SyncOffersService.SyncOffersAsync();

            var offers = await state.OffersPersistenceContext.Object.GetAllOffers();
            var premiumOffer = offers.Single(o => o.Key == (Client.Cashrewards, Client.Anz, 123));
            premiumOffer.Merchant.ClientComm.Should().Be(95);
            premiumOffer.Merchant.MemberComm.Should().Be(95);
            premiumOffer.Merchant.Commission.Should().Be(95);
            premiumOffer.Premium.ClientComm.Should().Be(98);
            premiumOffer.Premium.MemberComm.Should().Be(98);
            premiumOffer.Premium.Commission.Should().Be(98);
        }

        [Test]
        public async Task SyncOffersAsync_ShouldExcludePremiumOffer_GivenPremiumDisabledMerchant()
        {
            var state = new TestState();
            var shopGoOffer = ShopGoOfferFor(Client.Anz, 123);
            shopGoOffer.MerchantIsPremiumDisabled = true;
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer> { shopGoOffer });

            await state.SyncOffersService.SyncOffersAsync();

            var offers = await state.OffersPersistenceContext.Object.GetAllOffers();
            offers.SingleOrDefault(o => o.Key == (Client.Cashrewards, Client.Anz, 123)).Should().BeNull();
        }

        [Test]
        public async Task SyncOffersAsync_ShouldIncludeBaseOffer_GivenPremiumDisabledMerchant()
        {
            var state = new TestState();
            var shopGoOffer = ShopGoOfferFor(Client.Cashrewards, 123);
            shopGoOffer.MerchantIsPremiumDisabled = true;
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer> { shopGoOffer });

            await state.SyncOffersService.SyncOffersAsync();

            var offers = await state.OffersPersistenceContext.Object.GetAllOffers();
            offers.Count(o => o.Key == (Client.Cashrewards, null, 123)).Should().Be(1);
        }

        [Test]
        public async Task SyncOffersAsync_ShouldRemoveDuplicateOffer_GivenOffersWithTheSameKey()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer> { ShopGoOfferFor(Client.Cashrewards, 123) });
            state.OffersPersistenceContext.Setup(c => c.GetAllOffers()).ReturnsAsync(new List<Offer>
            {
                new Offer { Id = "1", Client = Client.Cashrewards, OfferId = 123 },
                new Offer { Id = "2", Client = Client.Cashrewards, OfferId = 123 }
            });

            await state.SyncOffersService.SyncOffersAsync();

            state.OffersPersistenceContext.Verify(c => c.DeleteOffer(It.Is<Offer>(o => o.Id == "2")), Times.Once);
        }

        [Test]
        public async Task SyncOffersAsync_ShouldSetMerchantCategory_GivenShopGoCategories()
        {
            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer> { ShopGoOfferFor(Client.Cashrewards, 123, 100) });
            state.ShopGoSource.Setup(s => s.GetMerchantCategories()).ReturnsAsync(new List<ShopGoCategory>
            {
                new ShopGoCategory { MerchantId = 100, CategoryId = 312 },
                new ShopGoCategory { MerchantId = 100, CategoryId = 313 },
                new ShopGoCategory { MerchantId = 100, CategoryId = 314 },
                new ShopGoCategory { MerchantId = 101, CategoryId = 315 }
            });

            await state.SyncOffersService.SyncOffersAsync();

            var offers = await state.OffersPersistenceContext.Object.GetAllOffers();
            var offer = offers.Single(o => o.Key == (Client.Cashrewards, null, 123));

            var categories = new OfferMerchantCategory[]
            {
                new OfferMerchantCategory() { CategoryId = 312 },
                new OfferMerchantCategory() { CategoryId = 313 },
                new OfferMerchantCategory() { CategoryId = 314 }
            };
            offer.Merchant.Categories.Should().BeEquivalentTo(categories);
        }

        [Test]
        public async Task SyncOffersAsync_ShouldGenerateDeleteAndChangeEvents_GivenMerchantIdChanges()
        {
            var state = new TestState();
            state.OffersPersistenceContext.Setup(c => c.GetAllOffers()).ReturnsAsync(new List<Offer>
            {
                new Offer { Id = "1", Client = Client.Cashrewards, OfferId = 123, Merchant = new OfferMerchant { Id = 100 } },
            });
            state.ShopGoSource.Setup(s => s.GetOffers()).ReturnsAsync(new List<ShopGoOffer>
            {
                ShopGoOfferFor(Client.Cashrewards, offerId: 123, merchantId: 200)
            });

            await state.SyncOffersService.SyncOffersAsync();

            state.OffersPersistenceContext.DomainEventsGenerated.Count.Should().Be(2);
            (state.OffersPersistenceContext.DomainEventsGenerated[0] as OfferDeleted).Merchant.Id.Should().Be(100);
            (state.OffersPersistenceContext.DomainEventsGenerated[1] as OfferChanged).Merchant.Id.Should().Be(200);
        }
    }
}
