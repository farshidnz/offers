using CashrewardsOffers.Application.Common.Models;
using MassTransit;
using Moq;

namespace CashrewardsOffers.Application.UnitTests.Helpers
{
    public class EventConsumeContextMock<TE> : Mock<ConsumeContext<DomainEventNotification<TE>>> where TE : class, new()
    {
        public TE DomainEvent { get; } = new();

        public EventConsumeContextMock()
        {
            Setup(c => c.Message).Returns(new DomainEventNotification<TE>(DomainEvent, null));
        }
    }
}
