using CashrewardsOffers.Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;

namespace Application.AcceptanceTests.Helpers
{
    public partial class Fixture
    {
        public async Task GivenEvent<TEvent>(TEvent domainEvent)
        {
            var consumer = _host.Services.GetService<IConsumer<DomainEventNotification<TEvent>>>();
            if (consumer == null) throw new Exception($"There is no consumer for events of type DomainEventNotification<{typeof(TEvent).Name}>");

            var context = new Mock<ConsumeContext<DomainEventNotification<TEvent>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<TEvent>(domainEvent, null));
            await consumer.Consume(context.Object);
        }

        public void GivenNowUtcIs(DateTimeOffset updateTime)
        {
            var anzItemFactory = (AnzItemFactory?)_host.Services.GetService<IAnzItemFactory>();
            anzItemFactory.ShouldNotBeNull();

            var mock = new Mock<IDomainTime>();
            mock.Setup(c => c.UtcNow).Returns(updateTime);
            anzItemFactory.DomainTime = mock.Object;
        }
    }
}
