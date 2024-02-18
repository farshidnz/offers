using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.IntegrationTests.Anz
{
    using static Testing;

    public class AnzItemPersistenceContextTests : TestBase
    {

        [Test]
        public async Task Update_ShouldUpdateSpecifiedFields()
        {
            var scope = GivenScope();
            var anzItemFactory = scope.ServiceProvider.GetService(typeof(IAnzItemFactory)) as IAnzItemFactory;
            var anzItemPersistanceContext = scope.ServiceProvider.GetService(typeof(IAnzItemPersistenceContext)) as IAnzItemPersistenceContext;

            var item = anzItemFactory.Create(100, 300);
            item.LastUpdated = 12345678;
            item.Merchant.InstoreRanking = 6;
            await anzItemPersistanceContext.Insert(item);

            var preUpdatedItem = await anzItemPersistanceContext.Get("100-300");
            preUpdatedItem.Merchant.Id.Should().Be(100);
            preUpdatedItem.Offer.Id.Should().Be(300);
            preUpdatedItem.LastUpdated.Should().Be(12345678);
            preUpdatedItem.Merchant.InstoreRanking.Should().Be(6);

            await anzItemPersistanceContext.Update("100-300", ("Merchant.InstoreRanking", 99), ("LastUpdated", 98765));

            var updatedItem = await anzItemPersistanceContext.Get("100-300");
            updatedItem.Merchant.Id.Should().Be(100);
            updatedItem.Offer.Id.Should().Be(300);
            updatedItem.LastUpdated.Should().Be(98765);
            updatedItem.Merchant.InstoreRanking.Should().Be(99);
        }
    }
}
