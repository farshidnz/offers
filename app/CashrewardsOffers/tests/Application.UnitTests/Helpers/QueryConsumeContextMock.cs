using MassTransit;
using Moq;

namespace CashrewardsOffers.Application.UnitTests.Helpers
{
    public class QueryConsumeContextMock<TQ, TR> : Mock<ConsumeContext<TQ>> where TQ : class, new() where TR : class
    {
        public TQ Query { get; } = new();
        public TR Response { get; private set; }

        public QueryConsumeContextMock()
        {
            Setup(c => c.Message).Returns(Query);
            Setup(c => c.RespondAsync(It.IsAny<TR>())).Callback((object r) => Response = r as TR);
        }
    }
}
