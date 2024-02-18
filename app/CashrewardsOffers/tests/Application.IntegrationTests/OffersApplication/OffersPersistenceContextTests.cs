using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.IntegrationTests.OffersApplication
{
    using static Testing;

    [Category("Integration")]
    public class OffersPersistenceContextTests : TestBase
    {
        [Test]
        public async Task GetAllOffers_ShouldReturnAllOffers()
        {
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 100 });
            await GivenOffer(new Offer { Client = Client.Cashrewards, PremiumClient = null, OfferId = 101 });
            await GivenOffer(new Offer { Client = Client.Anz, PremiumClient = null, OfferId = 102 });
            await GivenOffer(new Offer { Client = Client.MoneyMe, PremiumClient = null, OfferId = 103 });
            using var scope = ScopeFactory.CreateScope();
            var offersPersistenceContext = scope.ServiceProvider.GetService<IOffersPersistenceContext>();

            var offers = (await offersPersistenceContext.GetAllOffers()).ToList();

            offers.Count.Should().Be(4);
            offers[0].Id.Should().NotBeNull();
        }
    }
}
