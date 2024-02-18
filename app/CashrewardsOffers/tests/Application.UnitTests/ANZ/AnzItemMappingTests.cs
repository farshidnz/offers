using CashrewardsOffers.Application.ANZ.Queries.GetAnzItems.v1;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using Mapster;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.ANZ
{
    public class AnzItemMappingTests
    {
        private static AnzItem AnzItemDomainEntity
        {
            get
            {
                var item = new AnzItemFactory().Create(merchantId: 1002165, offerId: 427696);
                item.LastUpdated = new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero).UtcTicks;
                item.Merchant.Name = "Typo";
                item.Merchant.Link = "https://cashrewards.com.au/typo";
                item.Merchant.StartDateTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.FromHours(10)).ToUniversalTime();
                item.Merchant.EndDateTime = new DateTimeOffset(2023, 1, 23, 23, 59, 0, TimeSpan.FromHours(10)).ToUniversalTime();
                item.Merchant.LogoUrl = "//cdn.cashrewards.com/typo.jpg";
                item.Merchant.ClientCommissionString = "5% cashback";
                item.Merchant.CashbackGuidelines = "<p><ul><li>Disable your ad blocking software during your shopping sessions.</li><li></p>";
                item.Merchant.SpecialTerms = "<p>Note that purchases from Typo will track as 'Cotton On'.</p>Cashback is eligible with use of perks vouchers/rewards.";
                item.Merchant.NetworkId = TrackingNetwork.Instore;
                item.Merchant.IsPopularFlag = true;
                item.Merchant.PopularRanking = 0;
                item.Merchant.InstoreRanking = 3;
                item.Merchant.MobileEnabled = true;
                item.Merchant.Categories = new List<AnzMerchantCategory>
                {
                    new AnzMerchantCategory { Id = 310, Name = "Electronics" },
                    new AnzMerchantCategory { Id = 305, Name = "Entertainment" },
                    new AnzMerchantCategory { Id = 326, Name = "Food and Dining" },
                    new AnzMerchantCategory { Id = 317, Name = "Sports & Fitness" }
                };
                item.Offer.Title = "Join today and earn rewards when you shop";
                item.Offer.Terms = "<p>T&C apply. See merchant website for details.</p>";
                item.Offer.Link = "https://cashrewards.com.au/typo?coupon=join-today-and-earn-rewards-when-you-shop-427696";
                item.Offer.EndDateTime = new DateTimeOffset(2022, 12, 31, 23, 59, 59, TimeSpan.FromHours(0)).UtcTicks;
                item.Offer.WasRate = null;
                item.Offer.IsFeatured = true;
                item.Offer.FeaturedRanking = 7;

                return item;
            }
        }

        private static AnzItemInfo AnzItemInfo => new()
        {
            Id = "1002165-427696",
            Merchant = new AnzMerchantInfo
            {
                Id = 1002165,
                Name = "Typo",
                Link = "https://cashrewards.com.au/typo",
                StartDateTime = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndDateTime = new DateTimeOffset(9999, 12, 31, 0, 0, 0, TimeSpan.Zero),
                LogoUrl = "//cdn.cashrewards.com/typo.jpg",
                ClientCommissionString = "5% cashback",
                CashbackGuidelines = "<p><ul><li>Disable your ad blocking software during your shopping sessions.</li><li></p>",
                SpecialTerms = "<p>Note that purchases from Typo will track as 'Cotton On'.</p>Cashback is eligible with use of perks vouchers/rewards.",
                IsPopular = false,
                PopularRanking = 0,
                IsInstore = false,
                InstoreRanking = 0,
                Categories = new List<AnzMerchantCategoryInfo>
                {
                    new AnzMerchantCategoryInfo { Id = 310, Name = "Electronics" },
                    new AnzMerchantCategoryInfo { Id = 305, Name = "Entertainment" },
                    new AnzMerchantCategoryInfo { Id = 326, Name = "Food and Dining" },
                    new AnzMerchantCategoryInfo { Id = 317, Name = "Sports & Fitness" }
                }
            },
            Offer = new AnzOfferInfo
            {
                Id = 427696,
                Title = "Join today and earn rewards when you shop",
                Terms = "<p>T&C apply. See merchant website for details.</p>",
                Link = "https://cashrewards.com.au/typo?coupon=join-today-and-earn-rewards-when-you-shop-427696",
                EndDateTime = new DateTimeOffset(2022, 12, 31, 23, 59, 59, TimeSpan.Zero),
                WasRate = null,
                IsFeatured = true,
                FeaturedRanking = 7
            }
        };

        [SetUp]
        public void Setup()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));
        }

        [Test]
        public void Adapt_ShouldMapToResponse_GivenDomainEntity()
        {
            var anzItemInfo = AnzItemDomainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Should().BeEquivalentTo(AnzItemInfo);
        }

        [Test]
        public void Adapt_ShouldSetIsDeletedToTrue_GivenNoCarouselsAreActive()
        {
            var domainEntity = AnzItemDomainEntity;
            domainEntity.IsDeleted = false;
            domainEntity.Merchant.IsPopularFlag = false;
            domainEntity.Merchant.PopularMerchantRankingForBrowser = 0;
            domainEntity.Merchant.NetworkId = TrackingNetwork.LinkShare;
            domainEntity.Offer.IsFeatured = false;

            var anzItemInfo = domainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.IsDeleted.Should().Be(true);
        }

        [Test]
        public void Adapt_ShouldSetPopularRankingToZero_GivenIsPopularIsFalse()
        {
            var domainEntity = AnzItemDomainEntity;
            domainEntity.IsDeleted = false;
            domainEntity.Offer.Id = 0;
            domainEntity.Merchant.PopularMerchantRankingForBrowser = 0;
            domainEntity.Merchant.PopularRanking = 9;
            domainEntity.Merchant.NetworkId = TrackingNetwork.LinkShare;

            var anzItemInfo = domainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Merchant.IsPopular.Should().Be(false);
            anzItemInfo.Merchant.PopularRanking.Should().Be(0);
        }

        [Test]
        public void Adapt_ShouldSetIsPopularToFalseAndRankingToZero_GivenItemIsDeleted()
        {
            var domainEntity = AnzItemDomainEntity;
            domainEntity.IsDeleted = true;
            domainEntity.Offer.Id = 0;
            domainEntity.Merchant.PopularMerchantRankingForBrowser = 9;
            domainEntity.Merchant.PopularRanking = 9;
            domainEntity.Merchant.NetworkId = TrackingNetwork.LinkShare;

            var anzItemInfo = domainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Merchant.IsPopular.Should().Be(false);
            anzItemInfo.Merchant.PopularRanking.Should().Be(0);
        }

        [Test]
        public void Adapt_ShouldSetInstoreRankingToZero_GivenIsInstoreIsFalse()
        {
            var domainEntity = AnzItemDomainEntity;
            domainEntity.IsDeleted = false;
            domainEntity.Offer.Id = 0;
            domainEntity.Merchant.InstoreRanking = 9;
            domainEntity.Merchant.NetworkId = TrackingNetwork.LinkShare;

            var anzItemInfo = domainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Merchant.IsInstore.Should().Be(false);
            anzItemInfo.Merchant.InstoreRanking.Should().Be(0);
        }

        [Test]
        public void Adapt_ShouldSetIsInstoreToFalseAndInstoreRankingToZero_GivenItemIsDeleted()
        {
            var domainEntity = AnzItemDomainEntity;
            domainEntity.IsDeleted = true;
            domainEntity.Offer.Id = 0;
            domainEntity.Merchant.InstoreRanking = 9;
            domainEntity.Merchant.NetworkId = TrackingNetwork.Instore;

            var anzItemInfo = domainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Merchant.IsInstore.Should().Be(false);
            anzItemInfo.Merchant.InstoreRanking.Should().Be(0);
        }

        [Test]
        public void Adapt_ShouldSetFeaturedRankingToZero_GivenIsFeaturedIsFalse()
        {
            var domainEntity = AnzItemDomainEntity;
            domainEntity.IsDeleted = false;
            domainEntity.Offer.FeaturedRanking = 9;
            domainEntity.Offer.IsFeatured = false;

            var anzItemInfo = domainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Offer.IsFeatured.Should().Be(false);
            anzItemInfo.Offer.FeaturedRanking.Should().Be(0);
        }

        [Test]
        public void Adapt_ShouldSetIsFeaturedToFalseAndFeaturedRankingToZero_GivenItemIsDeleted()
        {
            var domainEntity = AnzItemDomainEntity;
            domainEntity.IsDeleted = true;
            domainEntity.Offer.FeaturedRanking = 9;
            domainEntity.Offer.IsFeatured = true;

            var anzItemInfo = domainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Offer.IsFeatured.Should().Be(false);
            anzItemInfo.Offer.FeaturedRanking.Should().Be(0);
        }

        [Test]
        public void Adapt_ShouldSetMerchantStartAndEndDatesToLowAndHighValuesRespectively_Always()
        {
            var anzItemInfo = AnzItemDomainEntity.Adapt<AnzItemInfo>();

            anzItemInfo.Merchant.StartDateTime.Should().Be(new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero));
            anzItemInfo.Merchant.EndDateTime.Should().Be(new DateTimeOffset(9999, 12, 31, 0, 0, 0, TimeSpan.Zero));
        }
    }
}
