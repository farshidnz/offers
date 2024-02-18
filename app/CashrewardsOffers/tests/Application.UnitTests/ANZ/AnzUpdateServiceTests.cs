using CashrewardsOffers.Application.ANZ.Services;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.ANZ
{
    public class AnzUpdateServiceTests
    {
        private class TestState
        {
            public AnzUpdateService AnzUpdateService { get; }

            public Mock<IAnzItemPersistenceContext> AnzItemPersistenceContext { get; } = new();

            public List<AnzItem> AnzItems { get; } = new();

            public TestState()
            {
                AnzItemPersistenceContext.Setup(c => c.GetAllActiveCarouselItems()).ReturnsAsync(AnzItems);
                AnzItemPersistenceContext.Setup(p => p.Get(It.IsAny<string>())).ReturnsAsync((string itemId) => AnzItems.FirstOrDefault(i => i.ItemId == itemId));

                AnzUpdateService = new AnzUpdateService(AnzItemPersistenceContext.Object, new AnzItemFactory());
            }

            public void GivenAnzItem(int merchantId, int? offerId = null, string name = null,
                bool isHomePageFeatured = false, bool isFeatured = false, bool isPopularFlag = false, int networkId = (int)TrackingNetwork.LinkShare,
                int instoreRanking = 0)
            {
                var anzItem = new AnzItemFactory().Create(merchantId, offerId);
                anzItem.Merchant.Name = name ?? $"merchant-{merchantId}";
                anzItem.Merchant.IsHomePageFeatured = isHomePageFeatured;
                anzItem.Merchant.IsFeatured = isFeatured;
                anzItem.Merchant.IsPopularFlag = isPopularFlag;
                anzItem.Merchant.NetworkId = networkId;
                anzItem.Merchant.InstoreRanking = instoreRanking;
                AnzItems.Add(anzItem);
            }
        }

        #region UpdateMerchant

        [Test]
        public async Task UpdateMerchant_ShouldUpdateWithChanges_GivenModelExists()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 123);

            await state.AnzUpdateService.UpdateMerchant(new MerchantChanged { MerchantId = 123, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Once);
        }

        [Test]
        public async Task UpdateMerchant_ShouldInsertChanges_GivenModelDoesNotExist()
        {
            var state = new TestState();

            await state.AnzUpdateService.UpdateMerchant(new MerchantChanged { MerchantId = 123, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Once);
        }

        [Test]
        public async Task UpdateMerchant_ShouldNotMakeChanges_GivenClientIsNotForCashrewards()
        {
            var state = new TestState();

            await state.AnzUpdateService.UpdateMerchant(new MerchantChanged { MerchantId = 123, Client = Client.Anz });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Never);
            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Never);
        }

        [Test]
        public async Task UpdateMerchant_ShouldUpdateExistingItemsSequence_GivenNewItem()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 100, isPopularFlag: true, networkId: TrackingNetwork.Instore, isHomePageFeatured: true);
            state.GivenAnzItem(merchantId: 101, isPopularFlag: true, networkId: TrackingNetwork.Instore);
            state.GivenAnzItem(merchantId: 102, isPopularFlag: true, networkId: TrackingNetwork.Instore, isFeatured: true);
            state.GivenAnzItem(merchantId: 103, isPopularFlag: true, networkId: TrackingNetwork.Instore, isHomePageFeatured: true);

            await state.AnzUpdateService.UpdateMerchant(new MerchantChanged { MerchantId = 110, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Once);
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.IsAny<string>(), It.IsAny<(string, object)[]>()), Times.Exactly(4));
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "100"), It.Is<(string Name, object Value)[]>(fs => (int)fs.Single(f => f.Name == "Merchant.InstoreRanking").Value == 1)), Times.Once);
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "101"), It.Is<(string Name, object Value)[]>(fs => (int)fs.Single(f => f.Name == "Merchant.InstoreRanking").Value == 4)), Times.Once);
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "102"), It.Is<(string Name, object Value)[]>(fs => (int)fs.Single(f => f.Name == "Merchant.InstoreRanking").Value == 3)), Times.Once);
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "103"), It.Is<(string Name, object Value)[]>(fs => (int)fs.Single(f => f.Name == "Merchant.InstoreRanking").Value == 2)), Times.Once);
        }

        [Test]
        public async Task UpdateMerchant_ShouldUpdateItemsThatNeedResequencing_GivenNewItem()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 100, isPopularFlag: true, networkId: TrackingNetwork.Instore, instoreRanking: 1, isHomePageFeatured: true);
            state.GivenAnzItem(merchantId: 101, isPopularFlag: true, networkId: TrackingNetwork.Instore, instoreRanking: 99);
            state.GivenAnzItem(merchantId: 102, isPopularFlag: true, networkId: TrackingNetwork.Instore, instoreRanking: 99, isFeatured: true);
            state.GivenAnzItem(merchantId: 103, isPopularFlag: true, networkId: TrackingNetwork.Instore, instoreRanking: 2, isHomePageFeatured: true);

            await state.AnzUpdateService.UpdateMerchant(new MerchantChanged { MerchantId = 110, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Once);
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.IsAny<string>(), It.IsAny<(string, object)[]>()), Times.Exactly(2));
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "101"), It.Is<(string Name, object Value)[]>(fs => (int)fs.Single(f => f.Name == "Merchant.InstoreRanking").Value == 4)), Times.Once);
            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "102"), It.Is<(string Name, object Value)[]>(fs => (int)fs.Single(f => f.Name == "Merchant.InstoreRanking").Value == 3)), Times.Once);
        }

        #endregion

        #region UpdateOffer

        [Test]
        public async Task UpdateOffer_ShouldUpdateWithChanges_GivenModelExists()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 123, offerId: 234);

            await state.AnzUpdateService.UpdateOffer(new OfferChanged { Merchant = new OfferMerchantChanged { Id = 123 }, OfferId = 234, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Once);
        }

        [Test]
        public async Task UpdateOffer_ShouldInsertChanges_GivenModelDoesNotExist()
        {
            var state = new TestState();

            await state.AnzUpdateService.UpdateOffer(new OfferChanged { Merchant = new OfferMerchantChanged { Id = 123 }, OfferId = 234, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Once);
        }

        [Test]
        public async Task UpdateOffer_ShouldNotMakeChanges_GivenClientIsNotForCashrewards()
        {
            var state = new TestState();

            await state.AnzUpdateService.UpdateOffer(new OfferChanged { Merchant = new OfferMerchantChanged { Id = 123 }, OfferId = 234, Client = Client.Anz });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Never);
            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Never);
        }

        #endregion

        #region DeleteMerchant

        [Test]
        public async Task DeleteMerchant_ShouldUpdateItemToDeleted_GivenItemExists()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 123);

            await state.AnzUpdateService.DeleteMerchant(new MerchantDeleted { MerchantId = 123, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "123"), It.Is<(string Name, object Value)[]>(fs => (bool)fs.Single(f => f.Name == "IsDeleted").Value)), Times.Once);
        }

        [Test]
        public async Task DeleteMerchant_ShouldNotMakeChanges_GivenItemtDoesNotExist()
        {
            var state = new TestState();

            await state.AnzUpdateService.DeleteMerchant(new MerchantDeleted { MerchantId = 123, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Never);
            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Never);
        }

        [Test]
        public async Task DeleteMerchant_ShouldNotMakeChanges_GivenClientIsNotForCashrewards()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 123);

            await state.AnzUpdateService.DeleteMerchant(new MerchantDeleted { MerchantId = 123, Client = Client.Anz });

            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Never);
        }

        #endregion

        #region DeleteOffer

        [Test]
        public async Task DeleteOffer_ShouldUpdateItemToDeleted_GivenItemExists()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 123, offerId: 234);

            await state.AnzUpdateService.DeleteOffer(new OfferDeleted { Merchant = new OfferMerchantDeleted { Id = 123 }, OfferId = 234, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Update(It.Is<string>(i => i == "123-234"), It.Is<(string Name, object Value)[]>(fs => (bool)fs.Single(f => f.Name == "IsDeleted").Value)), Times.Once);
        }

        [Test]
        public async Task DeleteOffer_ShouldNotMakeChanges_GivenItemDoesNotExist()
        {
            var state = new TestState();

            await state.AnzUpdateService.DeleteOffer(new OfferDeleted { Merchant = new OfferMerchantDeleted { Id = 123 }, OfferId = 234, Client = Client.Cashrewards });

            state.AnzItemPersistenceContext.Verify(p => p.Insert(It.IsAny<AnzItem>()), Times.Never);
            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Never);
        }

        [Test]
        public async Task DeleteOffer_ShouldNotMakeChanges_GivenClientIsNotForCashrewards()
        {
            var state = new TestState();
            state.GivenAnzItem(merchantId: 123, offerId: 234);

            await state.AnzUpdateService.DeleteOffer(new OfferDeleted { Merchant = new OfferMerchantDeleted { Id = 123 }, OfferId = 234, Client = Client.Anz });

            state.AnzItemPersistenceContext.Verify(p => p.Replace(It.IsAny<AnzItem>()), Times.Never);
        }

        #endregion
    }
}
