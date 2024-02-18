using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.MerchantSuggestions.Services;
using CashrewardsOffers.Application.UnitTests.Common.TestHelpers;
using CashrewardsOffers.Application.UnitTests.MerchantSuggestions.Helpers;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.MerchantSuggestions
{
    public class SyncRankedMerchantServiceTests
    {
        private class TestState
        {
            public SyncRankedMerchantService SyncMerchantService { get; }
            public Mock<IShopGoSource> ShopGoSource { get; } = new();
            public MockRankedMerchantPersistenceContext RankedMerchantPersistenceContext { get; } = new();
            public Mock<IMerchantPreferenceS3Source> MerchantPreferenceS3Source { get; } = new();

            public TestState()
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.json", true)
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.Development.json", true)
                    .Build();

                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Infrastructure"));

                SyncMerchantService = new SyncRankedMerchantService(ShopGoSource.Object, RankedMerchantPersistenceContext.Object,
                    MerchantPreferenceS3Source.Object);
            }
        }

        private ShopGoRankedMerchant CreateShopGoMerchant(int clientId, int merchantId, string hyphenatedString, bool isPremiumDisabled = false, string regularImageUrl = null) => new ShopGoRankedMerchant
        {
            ClientId = clientId,
            MerchantId = merchantId,
            HyphenatedString = hyphenatedString,
            IsPremiumDisabled = isPremiumDisabled,
            RegularImageUrl = regularImageUrl
        };

        private RankedMerchant CreateRankedMerchant(int merchantId, string hyphenatedString, int categoryId = 100, string categoryName = null, string id = null) => new RankedMerchant()
        {
            Id = id ?? Guid.NewGuid().ToString(),
            MerchantId = merchantId,
            HyphenatedString = hyphenatedString,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        [Test]
        public async Task SyncMerchantsAsync_ShouldOnlyLookupCRMerchants()
        {
            var shopGoMerchantSrc = new List<ShopGoRankedMerchant>
            {
                CreateShopGoMerchant((int)Client.Cashrewards, 1, "merchant-1", false, "CR-Image-URL"),
                CreateShopGoMerchant((int)Client.Anz, 2, "merchant-2", false, "ANZ-Image-URL")
            };

            var s3MerchantSrc = new List<RankedMerchant>
            {
                CreateRankedMerchant(1, "merchant-1")
            };


            var state = new TestState();
            state.ShopGoSource.Setup(s => s.GetRankedMerchants()).ReturnsAsync(shopGoMerchantSrc);
            state.MerchantPreferenceS3Source.Setup(s => s.DownloadLatestRankedMerchants()).ReturnsAsync(s3MerchantSrc);

            await state.SyncMerchantService.TrySyncMerchantAsync();

            var merchants = (await state.RankedMerchantPersistenceContext.Object.GetAllRankedMerchants()).ToList();
            merchants.Count.Should().Be(1);
            merchants[0].HyphenatedString.Should().Be("merchant-1");
            merchants[0].RegularImageUrl.Should().Be("CR-Image-URL");
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldUpdateExistingMerchants()
        {

            var shopGoMerchantSrc = new List<ShopGoRankedMerchant>
            {
                CreateShopGoMerchant((int)Client.Cashrewards, 1, "merchant-1")
            };

            var s3MerchantSrc = new List<RankedMerchant>
            {
                CreateRankedMerchant(1, "merchant-1", 100, "category-after")
            };


            var state = new TestState();

            await state.RankedMerchantPersistenceContext.Object.InsertRankedMerchant(CreateRankedMerchant(1, "merchant-1", 100, "category-before"));

            state.ShopGoSource.Setup(s => s.GetRankedMerchants()).ReturnsAsync(shopGoMerchantSrc);
            state.MerchantPreferenceS3Source.Setup(s => s.DownloadLatestRankedMerchants()).ReturnsAsync(s3MerchantSrc);

            await state.SyncMerchantService.TrySyncMerchantAsync();

            var merchants = (await state.RankedMerchantPersistenceContext.Object.GetAllRankedMerchants()).ToList();
            merchants.Count.Should().Be(1);
            merchants[0].CategoryName.Should().Be("category-after");
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldRemoveExistingMerchants_GivenTheyWereRemovedFromSource()
        {

            var shopGoMerchantSrc = Array.Empty<ShopGoRankedMerchant>();
            var s3MerchantSrc = Array.Empty<RankedMerchant>();

            var state = new TestState();

            await state.RankedMerchantPersistenceContext.Object.InsertRankedMerchant(CreateRankedMerchant(1, "merchant-1"));

            state.ShopGoSource.Setup(s => s.GetRankedMerchants()).ReturnsAsync(shopGoMerchantSrc);
            state.MerchantPreferenceS3Source.Setup(s => s.DownloadLatestRankedMerchants()).ReturnsAsync(s3MerchantSrc);
            await state.SyncMerchantService.TrySyncMerchantAsync();

            var merchants = (await state.RankedMerchantPersistenceContext.Object.GetAllRankedMerchants()).ToList();
            merchants.Count.Should().Be(0);
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldNotUpdateUnchangedMerchants()
        {
            var shopGoMerchantSrc = new List<ShopGoRankedMerchant>
            {
                CreateShopGoMerchant((int)Client.Cashrewards, 1, "merchant-1")
            };

            var s3MerchantSrc = new List<RankedMerchant>()
            {
                CreateRankedMerchant(1, "merchant-1", 100, "category-after")
            };

            var state = new TestState();

            await state.RankedMerchantPersistenceContext.Object.InsertRankedMerchant(CreateRankedMerchant(1, "merchant-1", 100, "category-after"));

            state.ShopGoSource.Setup(s => s.GetRankedMerchants()).ReturnsAsync(shopGoMerchantSrc);
            state.MerchantPreferenceS3Source.Setup(s => s.DownloadLatestRankedMerchants()).ReturnsAsync(s3MerchantSrc);

            await state.SyncMerchantService.TrySyncMerchantAsync();

            state.RankedMerchantPersistenceContext.Verify(c => c.UpdateRankedMerchant(It.IsAny<RankedMerchant>()), Times.Never());
        }

        [Test]
        public async Task SyncMerchantsAsync_ShouldRemoveDuplicated_GivenDataGetsMessedUpSomehow()
        {
            var shopGoMerchantSrc = new List<ShopGoRankedMerchant>()
            {
                CreateShopGoMerchant((int)Client.Cashrewards, 1, "merchant-1")
            };

            var s3MerchantSrc = new List<RankedMerchant>()
            {
                CreateRankedMerchant(1, "merchant-1")
            };

            var state = new TestState();

            state.RankedMerchantPersistenceContext.Setup(c => c.GetAllRankedMerchants()).ReturnsAsync(new List<RankedMerchant>()
            {
                CreateRankedMerchant(1, "merchant-1"),
                CreateRankedMerchant(1, "merchant-1")
            });

            state.ShopGoSource.Setup(s => s.GetRankedMerchants()).ReturnsAsync(shopGoMerchantSrc);
            state.MerchantPreferenceS3Source.Setup(s => s.DownloadLatestRankedMerchants()).ReturnsAsync(s3MerchantSrc);

            await state.SyncMerchantService.TrySyncMerchantAsync();

            state.RankedMerchantPersistenceContext.Verify(c => c.DeleteRankedMerchant(It.IsAny<RankedMerchant>()), Times.Once());
        }
    }
}
