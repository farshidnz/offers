using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Queries.v1;
using CashrewardsOffers.Application.UnitTests.Feature.Helpers;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using Mapster;
using MassTransit;
using Moq;
using NUnit.Framework;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Feature
{
    public class GetFeatureQueryConsumerTests
    {
        private class TestState
        {
            public GetFeatureQueryConsumer GetFeatureQueryConsumer { get; }

            public GetFeatureResponse GetFeatureResponse { get; private set; }

            public MockUnleash Unleash { get; } = new();

            public MockFeaturePersistenceContext FeaturePersistenceContext { get; } = new();

            public Mock<ICurrentUserService> CurrentUserService { get; } = new();

            private readonly Mock<ConsumeContext<GetFeatureQuery>> _consumeContextMock = new();

            public TestState()
            {
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

                _consumeContextMock.Setup(c => c.Message).Returns(new GetFeatureQuery());
                _consumeContextMock.Setup(c => c.RespondAsync(It.IsAny<GetFeatureResponse>())).Callback<GetFeatureResponse>(response => GetFeatureResponse = response);

                GetFeatureQueryConsumer = new GetFeatureQueryConsumer(
                    FeaturePersistenceContext.Object,
                    CurrentUserService.Object,
                    Unleash.Object);
            }

            public async Task WhenConsume() => await GetFeatureQueryConsumer.Consume(_consumeContextMock.Object);
        }

        [Test]
        public async Task Consume_ShouldGetFeatureToggle_GivenFeatureIsEnabled()
        {
            var state = new TestState();
            state.CurrentUserService.Setup(u => u.UserId).Returns("111");
            await state.FeaturePersistenceContext.Object.Enrol(FeatureToggle.Exp1, "111");
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(true);

            await state.WhenConsume();

            state.GetFeatureResponse.Feature.Should().Be(FeatureToggle.Exp1);
        }

        [Test]
        public async Task Consume_ShouldNotGetFeatureToggle_GivenFeatureIsDisabled()
        {
            var state = new TestState();
            state.CurrentUserService.Setup(u => u.UserId).Returns("111");
            await state.FeaturePersistenceContext.Object.Enrol(FeatureToggle.Exp1, "111");
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(false);

            await state.WhenConsume();

            state.GetFeatureResponse.Feature.Should().BeNull();
        }

        [Test]
        public async Task Consume_ShouldNotGetFeatureToggle_GivenUserIsNotEnroledToFeature()
        {
            var state = new TestState();
            state.CurrentUserService.Setup(u => u.UserId).Returns("111");
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(true);

            await state.WhenConsume();

            state.GetFeatureResponse.Feature.Should().BeNull();
        }

        [Test]
        public async Task Consume_ShouldNotHitDatabase_GivenFeatureToggleIsDisabled()
        {
            var state = new TestState();
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(false);

            await state.WhenConsume();

            state.FeaturePersistenceContext.Verify(f => f.Get(It.IsAny<string>()), Times.Never);
        }
    }
}
