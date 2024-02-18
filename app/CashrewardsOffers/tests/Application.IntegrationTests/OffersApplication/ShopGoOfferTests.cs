using CashrewardsOffers.Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.IntegrationTests.OffersApplication
{
    using static Testing;

    [Category("Integration")]
    public class ShopGoOfferTests : TestBase
    {
        [Test]
        [Ignore("requires shopgo database")]
        public async Task ShouldReturnOffersForFavouriteMerchants()
        {
            using var scope = ScopeFactory.CreateScope();
            var shopGoOffersSource = scope.ServiceProvider.GetService<IShopGoSource>();

            var offers = (await shopGoOffersSource.GetOffers()).ToList();

            offers.Count.Should().BeGreaterThan(400);
        }

        [Test]
        [Ignore("requires shopgo database")]
        public async Task ShouldReturnShopGoPerson_GivenNewMemberId()
        {
            using var scope = ScopeFactory.CreateScope();
            var shopGoOffersSource = scope.ServiceProvider.GetService<IShopGoSource>();

            var person = (await shopGoOffersSource.GetPersonFromNewMemberId("5C600794-577F-48C1-8197-70B34A3BB1D7"));

            person.CognitoId.Should().Be("99e9de07-f85a-47cf-ae6d-0fe1b56b45fc");
        }
    }
}
