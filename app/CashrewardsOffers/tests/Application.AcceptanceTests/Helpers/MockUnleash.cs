using CashrewardsOffers.Application.Feature.Services;
using Moq;
using System;
using System.Collections.Generic;

namespace Application.AcceptanceTests.Helpers
{
    public class MockUnleash : Mock<IUnleashService>
    {
        public Dictionary<string, bool> Toggles = new();

        public MockUnleash()
        {
            Setup(u => u.IsEnabled(null)).Throws(new Exception("Value cannot be null. (Parameter 'key')"));
            Setup(u => u.IsEnabled(It.IsAny<string>())).Returns((string toggleName) => Toggles.TryGetValue(toggleName, out var t) && t);
        }
    }
}
