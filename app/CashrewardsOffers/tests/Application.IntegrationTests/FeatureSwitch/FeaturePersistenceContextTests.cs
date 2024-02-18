using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.IntegrationTests.FeatureSwitch
{
    using static Testing;

    [Category("Integration")]
    public class FeaturePersistenceContextTests : TestBase
    {
        [Test]
        public async Task Count_ShouldCountEnrolmentsToAFeatureToggle()
        {
            await GivenFeature(FeatureToggle.Exp1, "111");
            await GivenFeature(FeatureToggle.Exp1, "222");
            await GivenFeature(FeatureToggle.Exp1, "333");
            using var scope = ScopeFactory.CreateScope();
            var featurePersistenceContext = scope.ServiceProvider.GetService<IFeaturePersistenceContext>();

            var featureCount = await featurePersistenceContext.Count(FeatureToggle.Exp1);

            featureCount.Should().Be(3);
        }

        [Test]
        public async Task Enrol_ShouldAddEnrolmentToFeatureToggle()
        {
            using var scope = ScopeFactory.CreateScope();
            var featurePersistenceContext = scope.ServiceProvider.GetService<IFeaturePersistenceContext>();

            await featurePersistenceContext.Enrol(FeatureToggle.Exp1, "111");

            var feature = await featurePersistenceContext.Get("111");
            feature.Should().Be(FeatureToggle.Exp1);
        }

        [Test]
        public async Task Get_ShouldReturnNull_GivenFeatureIsNotEnabledForAUser()
        {
            using var scope = ScopeFactory.CreateScope();
            var featurePersistenceContext = scope.ServiceProvider.GetService<IFeaturePersistenceContext>();

            var feature = await featurePersistenceContext.Get("111");
            feature.Should().BeNull();
        }
    }
}
