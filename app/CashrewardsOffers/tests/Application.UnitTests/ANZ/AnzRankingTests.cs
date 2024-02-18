using CashrewardsOffers.Application.ANZ.Services;
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
    public class AnzRankingTests
    {

        [Test]
        public void AnzItemsToUpdate_Using_RankingsUpdateParams_ChangedRanking()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var MakeAnzItem = (int offerId, int merchantId, long endDateTime, int ranking, bool isFeatured, int featuredRanking) =>
            {
                var anzItem = new AnzItemFactory().Create(offerId, merchantId);
                anzItem.Offer.EndDateTime = endDateTime;
                anzItem.Offer.Ranking = ranking;
                anzItem.Offer.IsFeatured = isFeatured;
                anzItem.Offer.FeaturedRanking = featuredRanking;
                return anzItem;
            };

            var first = MakeAnzItem(1, 1, new DateTime(2015, 1, 1).Ticks, 1, true, 1);
            var second = MakeAnzItem(2, 2, new DateTime(2015, 1, 1).Ticks, 0, true, 2);
            var third = MakeAnzItem(3, 3, new DateTime(2015, 1, 2).Ticks, 1, true, 3);


            var items = new List<AnzItem> { third, first, second };

            var changedRankings = new Rankings()
                .Add(new FeaturedOffersRanking())
                .AnzItemsToUpdate(items);

            changedRankings.Should().NotBeNull();
            changedRankings.Count.Should().Be(2);
            changedRankings.Contains(first).Should().BeFalse();
            changedRankings.Contains(second).Should().BeTrue();
            changedRankings.Contains(third).Should().BeTrue();

            changedRankings.Count.Should().Be(2);
            changedRankings.Find(item => first.ItemId == item.ItemId).Should().BeNull();
            changedRankings.Find(item => second.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(3);
            changedRankings.Find(item => third.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(2);
        }

        [Test]
        public void AnzItemsToUpdate_Using_RankingsUpdateParams_ChangedRanking_TwoRankingsAtSameTime()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var MakeAnzItem = (string name, int offerId, int merchantId, long endDateTime, int ranking, bool isFeatured, int featuredRanking, bool isInstore, int inStoreRanking) =>
            {
                var anzItem = new AnzItemFactory().Create(merchantId, isInstore ? null : offerId);
                anzItem.Offer.EndDateTime = endDateTime;
                anzItem.Offer.Ranking = ranking;
                anzItem.Offer.IsFeatured = isFeatured;
                anzItem.Offer.FeaturedRanking = featuredRanking;
                anzItem.Merchant.IsHomePageFeatured = isInstore;
                anzItem.Merchant.IsFeatured = isInstore;
                anzItem.Merchant.IsPopularFlag = false;
                anzItem.Merchant.NetworkId = isInstore ? Domain.Enums.TrackingNetwork.Instore : Domain.Enums.TrackingNetwork.LinkShare;
                anzItem.Merchant.Name = name;
                anzItem.Merchant.InstoreRanking = inStoreRanking;
                return anzItem;
            };

            var first = MakeAnzItem("a", 1, 1, new DateTime(2015, 1, 1).Ticks, 1, true, 1, true, 2);
            var second = MakeAnzItem("b", 2, 2, new DateTime(2015, 1, 1).Ticks, 0, true, 2, false, 0);
            var third = MakeAnzItem("c", 3, 3, new DateTime(2015, 1, 2).Ticks, 1, true, 3, true, 1);
            var fourth = MakeAnzItem("d", 4, 4, new DateTime(2015, 1, 5).Ticks, 9, true, 4, true, 3);
            var fifth = MakeAnzItem("e", 5, 5, new DateTime(2015, 1, 6).Ticks, 0, true, 5, false, 0);
            var sixth = MakeAnzItem("f", 6, 6, new DateTime(2015, 1, 7).Ticks, 0, false, 0, false, 0);


            var items = new List<AnzItem> { fourth, third, first, sixth, second, fifth };

            var changedRankings = new Rankings()
                .Add(new FeaturedOffersRanking())
                .Add(new InStoreMerchantRanking())
                .AnzItemsToUpdate(items);

            changedRankings.Should().NotBeNull();
            changedRankings.Count.Should().Be(4);
            changedRankings.Contains(first).Should().BeTrue();
            changedRankings.Contains(second).Should().BeTrue();
            changedRankings.Contains(third).Should().BeTrue();
            changedRankings.Contains(fourth).Should().BeTrue();
            changedRankings.Contains(fifth).Should().BeFalse();
            changedRankings.Contains(sixth).Should().BeFalse();

            changedRankings.Count.Should().Be(4);

            changedRankings.Find(item => first.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(2);
            changedRankings.Find(item => second.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(4);
            changedRankings.Find(item => third.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(3);
            changedRankings.Find(item => fourth.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(1);
            changedRankings.Find(item => fifth.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(5);
            changedRankings.Find(item => sixth.ItemId == item.ItemId)?.Offer?.FeaturedRanking.Should().Be(6);

            changedRankings.Find(item => first.ItemId == item.ItemId)?.Merchant?.InstoreRanking.Should().Be(1);
            changedRankings.Find(item => second.ItemId == item.ItemId)?.Merchant?.InstoreRanking.Should().Be(0);
            changedRankings.Find(item => third.ItemId == item.ItemId)?.Merchant?.InstoreRanking.Should().Be(2);
            changedRankings.Find(item => fourth.ItemId == item.ItemId)?.Merchant?.InstoreRanking.Should().Be(3);
            changedRankings.Find(item => fifth.ItemId == item.ItemId)?.Merchant?.InstoreRanking.Should().Be(0);
            changedRankings.Find(item => sixth.ItemId == item.ItemId)?.Merchant?.InstoreRanking.Should().Be(0);
        }

        [Test]
        public void AnzItemsToUpdate_ShouldOnlyUpdateInstoreMerchantItems_GivenVariousItems()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var MakeAnzItem = (string name, int merchantId, int? offerId, int networkId) =>
            {
                var anzItem = new AnzItemFactory().Create(merchantId, offerId);
                anzItem.Merchant.Name = name;
                anzItem.Merchant.NetworkId = networkId;
                anzItem.Merchant.IsPopularFlag = true;
                return anzItem;
            };

            var items = new List<AnzItem>
            {
                MakeAnzItem("offer1", 100, 200, TrackingNetwork.Instore),
                MakeAnzItem("offer2", 100, 300, TrackingNetwork.LinkShare),
                MakeAnzItem("merchant1", 100, null, TrackingNetwork.Instore),
                MakeAnzItem("merchant2", 200, null, TrackingNetwork.LinkShare),
            };

            var changedRankings = new Rankings()
                .Add(new InStoreMerchantRanking())
                .AnzItemsToUpdate(items);

            changedRankings.Count.Should().Be(1);
            changedRankings[0].Merchant.Name.Should().Be("merchant1");
        }

        [Test]
        public void AnzItemsToUpdate_ShouldOnlyUpdatePopularMerchantItems_GivenVariousItems()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

            var MakeAnzItem = (string name, int merchantId, int? offerId, int popularMerchantRankingForBrowser) =>
            {
                var anzItem = new AnzItemFactory().Create(merchantId, offerId);
                anzItem.Merchant.Name = name;
                anzItem.Merchant.NetworkId = Domain.Enums.TrackingNetwork.LinkShare;
                anzItem.Merchant.PopularMerchantRankingForBrowser = popularMerchantRankingForBrowser;
                return anzItem;
            };

            var items = new List<AnzItem>
            {
                MakeAnzItem("offer1", 100, 200, 0),
                MakeAnzItem("offer2", 100, 300, 3),
                MakeAnzItem("merchant1", 100, null, 0),
                MakeAnzItem("merchant2", 200, null, 7),
            };

            var changedRankings = new Rankings()
                .Add(new PopularMerchantRanking())
                .AnzItemsToUpdate(items);

            changedRankings.Count.Should().Be(1);
            changedRankings[0].Merchant.Name.Should().Be("merchant2");
        }
    }
}
