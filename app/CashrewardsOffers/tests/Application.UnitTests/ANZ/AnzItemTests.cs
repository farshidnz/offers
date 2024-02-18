using CashrewardsOffers.API.Services;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Mapster;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.ANZ
{
    public class AnzItemTests
    {
        private class TestState
        {
            public TestState(int merchantId = 100, int? offerId = null)
            {
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));
                AnzItemConfigService.Setup("prod");
                AnzItem = NewItem(merchantId, offerId);
            }

            public AnzItem AnzItem { get; }

            public static AnzItem NewItem(int merchantId = 100, int? offerId = null)
            {
                var item = new AnzItemFactory().Create(merchantId, offerId);
                item.Merchant.IsPaused = false;
                item.Merchant.IsPremiumDisabled = false;
                item.Merchant.MobileEnabled = true;
                return item;
            }

            public MerchantChanged MerchantChangedEvent { get; } = new MerchantChanged
            {
                MerchantId = 100,
                IsPaused = false,
                IsPremiumDisabled = false,
                MobileEnabled = true
            };

            public OfferChanged OfferChangedEvent { get; } = new OfferChanged
            {
                OfferId = 1000,
                Merchant = new OfferMerchantChanged
                {
                    Id = 100,
                    IsPremiumDisabled = false,
                    MobileEnabled = true
                },
                IsMerchantPaused = false
            };
        }

        #region ApplyChanges_With_MerchantChangedEvent

        [Test]
        public void ApplyChanges_ShouldNotChangeAnzItem_GivenNoMerchantChanges()
        {
            var state = new TestState();

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Should().BeEquivalentTo(TestState.NewItem());
            state.AnzItem.LastUpdated.Should().Be(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldClearDeletedFlag_GivenMerchantChangedEvent()
        {
            var state = new TestState();
            state.AnzItem.IsDeleted = true;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.IsDeleted.Should().BeFalse();
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldNotSetMerchantLink_GivenNoMerchantChanges()
        {
            var state = new TestState();
            state.AnzItem.Merchant.Link = "https://www.cashrewards.com.au/merchant-hypthenated-string";
            state.MerchantChangedEvent.HyphenatedString = "merchant-hypthenated-string";

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.Link.Should().Be("https://www.cashrewards.com.au/merchant-hypthenated-string");
            state.AnzItem.LastUpdated.Should().Be(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetMerchantLink_GivenMerchantHyphenatedStringChanged()
        {
            
            var state = new TestState();
            state.MerchantChangedEvent.HyphenatedString = "merchant-hypthenated-string";

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.Link.Should().Be("https://www.cashrewards.com.au/merchant-hypthenated-string");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetClientCommissionString_GivenMerchantClientCommissionStringChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.ClientCommissionString = "up to 12.5%";

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.ClientCommissionString.Should().Be("up to 12.5%");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsPopularFlag_GivenMerchantIsPopularChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.IsPopular = true;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.IsPopularFlag.Should().Be(true);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetNetworkId_GivenMerchantNetworkIdChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.NetworkId = TrackingNetwork.Instore;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.NetworkId.Should().Be(TrackingNetwork.Instore);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetMobileEnabled_GivenMerchantMobileEnabledChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.MobileEnabled = false;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.MobileEnabled.Should().Be(false);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsPremiumDisabled_GivenMerchantIsPremiumDisabledChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.IsPremiumDisabled = true;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.IsPremiumDisabled.Should().Be(true);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsPaused_GivenMerchantIsPausedChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.IsPaused = true;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.IsPaused.Should().Be(true);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetPopularMerchantRankingForBrowser_GivenMerchantPopularMerchantRankingForBrowserChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.PopularMerchantRankingForBrowser = 7;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.PopularMerchantRankingForBrowser.Should().Be(7);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }
        
                [Test]
        public void ApplyChanges_ShouldSetName_GivenMerchantNameChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.Name = "merhcnat name";

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.Name.Should().Be("merhcnat name");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetLogoUrl_GivenMerchantLogoUrlChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.LogoUrl = "https://logo.png";

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.LogoUrl.Should().Be("https://logo.png");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetCashbackGuidelines_GivenMerchantBasicTermsChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.BasicTerms = "basic terms";

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.CashbackGuidelines.Should().Be("basic terms");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetSpecialTerms_GivenMerchantExtentedTermsChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.ExtentedTerms = "extended terms";

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.SpecialTerms.Should().Be("extended terms");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsHomePageFeatured_GivenMerchantIsHomePageFeaturedChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.IsHomePageFeatured = true;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.IsHomePageFeatured.Should().Be(true);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsFeatured_GivenMerchantIsFeaturedChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.IsFeatured = true;

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.IsFeatured.Should().Be(true);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetCategories_GivenAnyMerchantCategoriesChanged()
        {
            var state = new TestState();
            state.MerchantChangedEvent.Categories = new List<MerchantChangedCategoryItem> { new MerchantChangedCategoryItem { CategoryId = 111, Name = "abc" } };

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.Categories.Should().BeEquivalentTo(new List<AnzMerchantCategory> { new AnzMerchantCategory { Id = 111, Name = "abc" } });
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldNotSetCategories_GivenAAllMerchantCategoriesAreTheSame()
        {
            var state = new TestState();
            state.AnzItem.Merchant.Categories = new List<AnzMerchantCategory> { new AnzMerchantCategory { Id = 111, Name = "abc" } };
            state.MerchantChangedEvent.Categories = new List<MerchantChangedCategoryItem> { new MerchantChangedCategoryItem { CategoryId = 111, Name = "abc" } };

            state.AnzItem.ApplyChanges(state.MerchantChangedEvent);

            state.AnzItem.Merchant.Categories.Should().BeEquivalentTo(new List<AnzMerchantCategory> { new AnzMerchantCategory { Id = 111, Name = "abc" } });
            state.AnzItem.LastUpdated.Should().Be(DateTimeOffset.MinValue.UtcTicks);
        }

        #endregion

        #region ApplyChanges_With_OfferChangedEvent

        [Test]
        public void ApplyChanges_ShouldNotChangeAnzItem_GivenNoOfferChanges()
        {
            var state = new TestState(offerId: 1000);

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Should().BeEquivalentTo(TestState.NewItem(offerId: 1000));
            state.AnzItem.LastUpdated.Should().Be(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldClearDeletedFlag_GivenOfferChangedEvent()
        {
            var state = new TestState(offerId: 1000);
            state.AnzItem.IsDeleted = true;

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.IsDeleted.Should().BeFalse();
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldNotSetOfferLink_GivenNoOfferChanges()
        {
            var state = new TestState(offerId: 1000);
            state.AnzItem.Merchant.Link = "https://www.cashrewards.com.au/merchant-hypthenated-string";
            state.AnzItem.Offer.Link = "https://www.cashrewards.com.au/merchant-hypthenated-string?coupon=offer-hypthenated-string";
            state.OfferChangedEvent.Merchant.HyphenatedString = "merchant-hypthenated-string";
            state.OfferChangedEvent.HyphenatedString = "offer-hypthenated-string";

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.Link.Should().Be("https://www.cashrewards.com.au/merchant-hypthenated-string?coupon=offer-hypthenated-string");
            state.AnzItem.Merchant.Link.Should().Be("https://www.cashrewards.com.au/merchant-hypthenated-string");
            state.AnzItem.LastUpdated.Should().Be(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetOfferLink_GivenOfferHyphenatedStringChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.Merchant.HyphenatedString = "merchant-hypthenated-string";
            state.OfferChangedEvent.HyphenatedString = "offer-hypthenated-string";

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.Link.Should().Be("https://www.cashrewards.com.au/merchant-hypthenated-string?coupon=offer-hypthenated-string");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetOfferTitle_GivenOfferTitleChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.Title = "title";

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.Title.Should().Be("title");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetOfferTerms_GivenOfferTermsChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.Terms = "terms";

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.Terms.Should().Be("terms");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetOfferWasRate_GivenOfferWasRateChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.WasRate = "1%";

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.WasRate.Should().Be("1%");
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetOfferEndDateTime_GivenOfferEndDateTimeChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.EndDateTime = new DateTime(2022, 1, 2, 3, 4, 5).Ticks;

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.EndDateTime.Should().Be(new DateTime(2022, 1, 2, 3, 4, 5).Ticks);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetOfferRanking_GivenOfferRankingChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.Ranking = 9;

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.Ranking.Should().Be(9);
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsFeatured_GivenOfferIsFeaturedChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.IsFeatured = true;

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Offer.IsFeatured.Should().BeTrue();
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsPremiumDisabled_GivenOfferIsPremiumDisabledChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.Merchant.IsPremiumDisabled = true;

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Merchant.IsPremiumDisabled.Should().BeTrue();
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetMobileEnabled_GivenOfferMobileEnabledChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.Merchant.MobileEnabled = false;

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Merchant.MobileEnabled.Should().BeFalse();
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        [Test]
        public void ApplyChanges_ShouldSetIsPaused_GivenOfferMerchantIsPausedChanged()
        {
            var state = new TestState(offerId: 1000);
            state.OfferChangedEvent.IsMerchantPaused = true;

            state.AnzItem.ApplyChanges(state.OfferChangedEvent);

            state.AnzItem.Merchant.IsPaused.Should().BeTrue();
            state.AnzItem.LastUpdated.Should().NotBe(DateTimeOffset.MinValue.UtcTicks);
        }

        #endregion

        #region IsUnavailable

        [Test]
        public void IsUnavailable_ShouldBeTrue_GivenItemIsNotOnAnyCarousels()
        {
            var state = new TestState();
            state.AnzItem.Merchant.NetworkId = TrackingNetwork.LinkShare;
            state.AnzItem.Merchant.IsPopularFlag = false;
            state.AnzItem.Offer.IsFeatured = false;

            state.AnzItem.IsUnavailable.Should().BeTrue();
        }

        [Test]
        public void IsUnavailable_ShouldBeTrue_GivenMerchantIsPaused()
        {
            var state = new TestState();
            state.AnzItem.Merchant.NetworkId = TrackingNetwork.Instore;
            state.AnzItem.Merchant.IsPaused = true;

            state.AnzItem.IsUnavailable.Should().BeTrue();
        }

        [Test]
        public void IsUnavailable_ShouldBeTrue_GivenMerchantIsPremiumDisabled()
        {
            var state = new TestState();
            state.AnzItem.Merchant.NetworkId = TrackingNetwork.Instore;
            state.AnzItem.Merchant.IsPremiumDisabled = true;

            state.AnzItem.IsUnavailable.Should().BeTrue();
        }

        [Test]
        public void IsUnavailable_ShouldBeTrue_GivenMerchantIsNotMobileEnabled()
        {
            var state = new TestState();
            state.AnzItem.Merchant.NetworkId = TrackingNetwork.Instore;
            state.AnzItem.Merchant.MobileEnabled = false;

            state.AnzItem.IsUnavailable.Should().BeTrue();
        }

        #endregion
    }
}
