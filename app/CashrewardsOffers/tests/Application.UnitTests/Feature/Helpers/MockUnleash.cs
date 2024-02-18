using CashrewardsOffers.Application.Feature.Services;
using Moq;
using System;

namespace CashrewardsOffers.Application.UnitTests.Feature.Helpers
{
    public class MockUnleash : Mock<IUnleashService>
    {
        public MockUnleash()
        {
            Setup(u => u.IsEnabled(null)).Throws(new Exception("Value cannot be null. (Parameter 'key')"));
        }
    }
}
