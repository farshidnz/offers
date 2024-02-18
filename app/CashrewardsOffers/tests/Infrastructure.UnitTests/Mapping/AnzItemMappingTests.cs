using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure;
using CashrewardsOffers.Infrastructure.Models;
using FluentAssertions;
using Mapster;
using MongoDB.Bson;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Infrustructure.UnitTests.Mapping
{
    public class AnzItemMappingTests
    {
        private static AnzItem AnzItemDomainEntity
        {
            get
            {
                var item = new AnzItemFactory().Create(merchantId: 1002165, offerId: 427696);
                item.Id = "62ba2bad9a3a7653c2e5beb2";
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
                item.Merchant.IsFeatured = true;
                item.Merchant.IsPopularFlag = true;
                item.Merchant.IsHomePageFeatured = true;
                item.Merchant.MobileEnabled = true;
                item.Merchant.IsPremiumDisabled = true;
                item.Merchant.PopularMerchantRankingForBrowser = 7;
                item.Merchant.PopularRanking = 1;
                item.Merchant.InstoreRanking = 3;
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
                item.Offer.WasRate = null;
                item.Offer.IsFeatured = true;
                item.Offer.FeaturedRanking = 7;

                return item;
            }
        }

        private static AnzItemDocument AnzItemDocument => new()
        {
            _id = new ObjectId("62ba2bad9a3a7653c2e5beb2"),
            ItemId = "1002165-427696",
            LastUpdated = new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero).UtcTicks,
            Merchant = new AnzMerchantDocument
            {
                Id = 1002165,
                Name = "Typo",
                Link = "https://cashrewards.com.au/typo",
                StartDateTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.FromHours(10)).ToUniversalTime(),
                EndDateTime = new DateTimeOffset(2023, 1, 23, 23, 59, 0, TimeSpan.FromHours(10)).ToUniversalTime(),
                LogoUrl = "//cdn.cashrewards.com/typo.jpg",
                ClientCommissionString = "5% cashback",
                CashbackGuidelines = "<p><ul><li>Disable your ad blocking software during your shopping sessions.</li><li></p>",
                SpecialTerms = "<p>Note that purchases from Typo will track as 'Cotton On'.</p>Cashback is eligible with use of perks vouchers/rewards.",
                NetworkId = TrackingNetwork.Instore,
                IsFeatured = true,
                IsHomePageFeatured = true,
                IsPopularFlag = true,
                PopularRanking = 1,
                InstoreRanking = 3,
                MobileEnabled = true,
                IsPremiumDisabled = true,
                PopularMerchantRankingForBrowser = 7,
                Categories = new List<AnzMerchantCategoryDocument>
                {
                    new AnzMerchantCategoryDocument { Id = 310, Name = "Electronics" },
                    new AnzMerchantCategoryDocument { Id = 305, Name = "Entertainment" },
                    new AnzMerchantCategoryDocument { Id = 326, Name = "Food and Dining" },
                    new AnzMerchantCategoryDocument { Id = 317, Name = "Sports & Fitness" }
                }
            },
            Offer = new AnzOfferDocument
            {
                Id = 427696,
                Title = "Join today and earn rewards when you shop",
                Terms = "<p>T&C apply. See merchant website for details.</p>",
                Link = "https://cashrewards.com.au/typo?coupon=join-today-and-earn-rewards-when-you-shop-427696",
                WasRate = null,
                IsFeatured = true,
                FeaturedRanking = 7
            }
        };

        [Test]
        public void Adapt_ShouldMapToDocument_GivenDomainEntity()
        {
            DependencyInjection.RegisterMappingProfiles();

            var anzItemDocument = AnzItemDomainEntity.Adapt<AnzItemDocument>();

            anzItemDocument.Should().BeEquivalentTo(AnzItemDocument);
        }

        [Test]
        public void Adapt_ShouldMapToDomainEntity_GivenDocument()
        {
            DependencyInjection.RegisterMappingProfiles();

            var anzItemDomainEntity = AnzItemDocument
                .BuildAdapter()
                .AddParameters("anzItemFactory", new AnzItemFactory())
                .AdaptToType<AnzItem>();

            anzItemDomainEntity.Should().BeEquivalentTo(AnzItemDomainEntity);
        }
    }
}
