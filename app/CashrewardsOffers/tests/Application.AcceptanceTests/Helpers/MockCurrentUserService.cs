using CashrewardsOffers.Application.Common.Interfaces;
using Moq;

namespace Application.AcceptanceTests.Helpers
{
    public class MockCurrentUserService : Mock<ICurrentUserService>
    {
        public string UserId { get; set; } = "123";

        public MockCurrentUserService()
        {
            Setup(s => s.UserId).Returns(() => UserId);
        }
    }
}
