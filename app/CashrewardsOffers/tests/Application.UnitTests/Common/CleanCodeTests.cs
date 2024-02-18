using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace CashrewardsOffers.Application.UnitTests.Common
{
    public class CleanCodeTests
    {
        [Test]
        public void ApplicationLayerShouldNotReferenceInfrustructure()
        {
            Assembly.Load("CashrewardsOffers.Application").GetReferencedAssemblies()
                .Where(a => a.Name == "CashrewardsOffers.Infrastructure")
                .Should().BeEmpty();
        }
    }
}
