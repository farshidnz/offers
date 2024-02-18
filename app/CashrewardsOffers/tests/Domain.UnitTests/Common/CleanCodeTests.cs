using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace CashrewardsOffers.Domain.UnitTests.Common
{
    public class CleanCodeTests
    {
        [Test]
        public void DomainLayerShouldNotHaveAnyApplicationDependencies()
            {
            Assembly.Load("CashrewardsOffers.Domain").GetReferencedAssemblies()
                .Where(a => 
                    !a.Name.StartsWith("System.") &&
                    !a.Name.StartsWith("nCrunch.") &&
                    a.Name != "Mapster" &&
                    a.Name != "netstandard" &&
                    a.Name != "Newtonsoft.Json" &&
                    a.Name != "KellermanSoftware.Compare-NET-Objects")
                .Should().BeEmpty();
        }
    }
}
