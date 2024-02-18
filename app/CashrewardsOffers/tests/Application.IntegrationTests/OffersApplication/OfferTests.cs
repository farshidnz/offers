using CashrewardsOffers.Application.Offers.Queries.GetOffers.v1;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;

namespace CashrewardsOffers.Application.IntegrationTests.OffersApplication
{
    using static Testing;

    [Category("Integration")]
    public class OfferTests : TestBase
    {
        private static OfferMerchantCategory Category(int categoryId, string name = null) =>
            new OfferMerchantCategory() { CategoryId = categoryId, Name = name };

        private static OfferMerchantCategory[] Categories(params (int categoryId, string name)[] categories) =>
            categories.Select(c => Category(c.categoryId, c.name)).ToArray();

        [Test]
        public async Task GetOffersQuery_ShouldReturnOffersForFavouriteMerchants_GivenVariousOffers()
        {
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 100 });
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 101 });
            await GivenOffer(new Offer { Client = Client.Anz, PremiumClient = null, OfferId = 102 });
            await GivenOffer(new Offer { Client = Client.MoneyMe, PremiumClient = null, OfferId = 103 });
            var query = new GetOffersQuery { ClientId = (int)Client.Cashrewards };

            var response = await SendQuery<GetOffersQuery, GetOffersResponse>(query);

            response.Data.Should().HaveCount(2);
        }

        [Test]
        public async Task GetOffersQuery_ShouldReturnOffersForCategories_GivenVariousOffers()
        {
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 100, Merchant = new OfferMerchant { Id = 200, Categories = Categories((300, null), (301, null), (302, null)) } });
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 101, Merchant = new OfferMerchant { Id = 201, Categories = Categories((300, null), (310, null), (320, null)) } });
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 102, Merchant = new OfferMerchant { Id = 202, Categories = Categories((310, null)) } });
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 103, Merchant = new OfferMerchant { Id = 203, Categories = Categories((312, null)) } });
            var query = new GetOffersQuery { ClientId = (int)Client.Cashrewards, CategoryId = 310 };

            var response = await SendQuery<GetOffersQuery, GetOffersResponse>(query);

            response.Data.Should().HaveCount(2);
            response.Data.Single(o => o.Id == 101);
            response.Data.Single(o => o.Id == 102);
        }
    }
}
