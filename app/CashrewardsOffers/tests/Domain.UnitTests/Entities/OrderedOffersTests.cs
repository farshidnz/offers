using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Domain.UnitTests.Entities
{
    public class OrderedOffersTests
    {
        private int _nextOfferId = 1;
        public Offer GivenOffer(string title, int ranking, int merchantId = 0, string offerString = null, DateTimeOffset? endDateTime = null)
        {
            offerString ??= "10%";
            var isFlatRate = !offerString.ToLower().StartsWith("up to ");
            var isDollarRate = offerString[isFlatRate ? 0 : 6] == '$';
            var rateStart = isFlatRate ? (isDollarRate ? 1 : 0) : (isDollarRate ? 7 : 6);
            var rate = decimal.Parse(offerString.Substring(rateStart, offerString.Length - rateStart - (isDollarRate ? 0 : 1)));
            return new Offer
            {
                Client = Client.Cashrewards,
                OfferId = _nextOfferId++,
                Title = title,
                Ranking = ranking,
                Merchant = new OfferMerchant
                {
                    Id = merchantId,
                    HyphenatedString = $"merch-{merchantId}",
                    ClientComm = 100,
                    MemberComm = 100,
                    Commission = rate,
                    CommissionType = isDollarRate ? CommissionType.Dollar : CommissionType.Percentage,
                    IsFlatRate = isFlatRate
                },
                EndDateTime = endDateTime ?? DateTimeOffset.MinValue
            };
        }

        [Test]
        public void ReorderOffersForExperiments_ShouldOrderTopTwoOffersBasedOnFavouritesAndCashbackAmount_GivenExperiment1()
        {
            var orderedOffers = new OrderedOffers(new List<Offer>
            {
                GivenOffer(title: "Offer 1", ranking: 10, merchantId: 1, offerString: "up to 20%"),
                GivenOffer(title: "Offer 2", ranking: 9, merchantId: 2, offerString: "up to $25"),
                GivenOffer(title: "Offer 3", ranking: 8, merchantId: 3, offerString: "10%"),
                GivenOffer(title: "Offer 4", ranking: 7, merchantId: 4, offerString: "12%"),
                GivenOffer(title: "Offer 5", ranking: 6, merchantId: 5, offerString: "14%"),
                GivenOffer(title: "Offer 6", ranking: 5, merchantId: 6, offerString: "up to 25%"),
                GivenOffer(title: "Offer 7", ranking: 4, merchantId: 7, offerString: "$50"),
                GivenOffer(title: "Offer 8", ranking: 3, merchantId: 8, offerString: "50%"),
                GivenOffer(title: "Offer 9", ranking: 2, merchantId: 9, offerString: "up to 50%"),
                GivenOffer(title: "Offer 10", ranking: 1, merchantId: 10, offerString: "up to $50")
            });

            orderedOffers.ReorderOffersForExperiments(
                FeatureToggle.Exp1,
                new List<FavouriteMerchant>
                {
                    new FavouriteMerchant { MerchantId = 3, HyphenatedString = "merch-3" },
                    new FavouriteMerchant { MerchantId = 4, HyphenatedString = "merch-4" },
                    new FavouriteMerchant { MerchantId = 5, HyphenatedString = "merch-5" },
                    new FavouriteMerchant { MerchantId = 6, HyphenatedString = "merch-6" },
                    new FavouriteMerchant { MerchantId = 7, HyphenatedString = "merch-7" },
                    new FavouriteMerchant { MerchantId = 8, HyphenatedString = "merch-8" },
                    new FavouriteMerchant { MerchantId = 9, HyphenatedString = "merch-9" },
                    new FavouriteMerchant { MerchantId = 10, HyphenatedString = "merch-10" }
            });

            orderedOffers.Offers[0].Title.Should().Be("Offer 8");
            orderedOffers.Offers[0].IsPersonalised.Should().BeTrue();
            orderedOffers.Offers[1].Title.Should().Be("Offer 9");
            orderedOffers.Offers[1].IsPersonalised.Should().BeTrue();
            orderedOffers.Offers[2].Title.Should().Be("Offer 1");
            orderedOffers.Offers[2].IsPersonalised.Should().BeFalse();
            orderedOffers.Offers[3].Title.Should().Be("Offer 2");
            orderedOffers.Offers[4].Title.Should().Be("Offer 3");
            orderedOffers.Offers[5].Title.Should().Be("Offer 4");
            orderedOffers.Offers[6].Title.Should().Be("Offer 5");
            orderedOffers.Offers[7].Title.Should().Be("Offer 6");
            orderedOffers.Offers[8].Title.Should().Be("Offer 7");
            orderedOffers.Offers[9].Title.Should().Be("Offer 10");
        }

        [Test]
        public void ReorderOffersForExperiments_ShouldOrderTopTwoOffersBasedOnFavouritesAndOfferEndingDate_GivenExperiment2()
        {
            var orderedOffers = new OrderedOffers(new List<Offer>
            {
                GivenOffer(title: "Offer 1", ranking: 9, merchantId: 1, endDateTime: new DateTimeOffset(2022, 1, 7, 0, 0, 0, TimeSpan.Zero)),
                GivenOffer(title: "Offer 2", ranking: 8, merchantId: 2, endDateTime: new DateTimeOffset(2022, 1, 6, 0, 0, 0, TimeSpan.Zero)),
                GivenOffer(title: "Offer 3", ranking: 7, merchantId: 3, endDateTime: new DateTimeOffset(2022, 1, 5, 0, 0, 0, TimeSpan.Zero)),
                GivenOffer(title: "Offer 4", ranking: 6, merchantId: 4, endDateTime: new DateTimeOffset(2022, 1, 4, 0, 0, 0, TimeSpan.Zero)),
                GivenOffer(title: "Offer 5", ranking: 5, merchantId: 5, endDateTime: new DateTimeOffset(2022, 1, 3, 0, 0, 0, TimeSpan.Zero)),
                GivenOffer(title: "Offer 6", ranking: 4, merchantId: 6, endDateTime: new DateTimeOffset(2022, 1, 2, 0, 0, 0, TimeSpan.Zero)),
                GivenOffer(title: "Offer 7", ranking: 3, merchantId: 7, endDateTime: new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            });

            orderedOffers.ReorderOffersForExperiments(
                FeatureToggle.Exp2,
                new List<FavouriteMerchant>
                {
                    new FavouriteMerchant { MerchantId = 3, HyphenatedString = "merch-3" },
                    new FavouriteMerchant { MerchantId = 4, HyphenatedString = "merch-4" },
                    new FavouriteMerchant { MerchantId = 5, HyphenatedString = "merch-5" },
                    new FavouriteMerchant { MerchantId = 6, HyphenatedString = "merch-6" }
                });

            orderedOffers.Offers[0].Title.Should().Be("Offer 6");
            orderedOffers.Offers[0].IsPersonalised.Should().BeTrue();
            orderedOffers.Offers[1].Title.Should().Be("Offer 5");
            orderedOffers.Offers[1].IsPersonalised.Should().BeTrue();
            orderedOffers.Offers[2].Title.Should().Be("Offer 1");
            orderedOffers.Offers[2].IsPersonalised.Should().BeFalse();
            orderedOffers.Offers[3].Title.Should().Be("Offer 2");
            orderedOffers.Offers[4].Title.Should().Be("Offer 3");
            orderedOffers.Offers[5].Title.Should().Be("Offer 4");
            orderedOffers.Offers[6].Title.Should().Be("Offer 7");
        }

        [Test]
        public void ReorderOffersForExperiments_ShouldOrderTopTwoOffersBasedOnFavouritesSelectionOrder_GivenExperiment3()
        {
            var orderedOffers = new OrderedOffers(new List<Offer>
            {
                GivenOffer(title: "Offer 1", ranking: 9, merchantId: 1),
                GivenOffer(title: "Offer 2", ranking: 8, merchantId: 2),
                GivenOffer(title: "Offer 3", ranking: 7, merchantId: 3),
                GivenOffer(title: "Offer 4", ranking: 6, merchantId: 4),
                GivenOffer(title: "Offer 5", ranking: 5, merchantId: 5),
                GivenOffer(title: "Offer 6", ranking: 4, merchantId: 6),
                GivenOffer(title: "Offer 7", ranking: 3, merchantId: 7)
            });

            orderedOffers.ReorderOffersForExperiments(
                FeatureToggle.Exp3,
                new List<FavouriteMerchant>
                {
                    new FavouriteMerchant { MerchantId = 3, HyphenatedString = "merch-3", SelectionOrder = 3 },
                    new FavouriteMerchant { MerchantId = 4, HyphenatedString = "merch-4", SelectionOrder = 0 },
                    new FavouriteMerchant { MerchantId = 5, HyphenatedString = "merch-5", SelectionOrder = 2 },
                    new FavouriteMerchant { MerchantId = 6, HyphenatedString = "merch-6", SelectionOrder = 1 }
                });

            orderedOffers.Offers[0].Title.Should().Be("Offer 4");
            orderedOffers.Offers[0].IsPersonalised.Should().BeTrue();
            orderedOffers.Offers[1].Title.Should().Be("Offer 6");
            orderedOffers.Offers[1].IsPersonalised.Should().BeTrue();
            orderedOffers.Offers[2].Title.Should().Be("Offer 1");
            orderedOffers.Offers[2].IsPersonalised.Should().BeFalse();
            orderedOffers.Offers[3].Title.Should().Be("Offer 2");
            orderedOffers.Offers[4].Title.Should().Be("Offer 3");
            orderedOffers.Offers[5].Title.Should().Be("Offer 5");
            orderedOffers.Offers[6].Title.Should().Be("Offer 7");
        }

    }
}
