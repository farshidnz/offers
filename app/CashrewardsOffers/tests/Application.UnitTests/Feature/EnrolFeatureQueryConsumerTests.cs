using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Queries.v1;
using CashrewardsOffers.Application.UnitTests.Feature.Helpers;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Feature
{
    public class EnrolFeatureQueryConsumerTests
    {
        private class TestState
        {
            public EnrolFeatureQueryConsumer EnrolFeatureQueryConsumer { get; }

            public EnrolFeatureResponse EnrolFeatureResponse { get; private set; }

            public MockUnleash Unleash { get; } = new();

            public MockFeaturePersistenceContext FeaturePersistenceContext { get; } = new();

            public Mock<ICurrentUserService> CurrentUserService { get; } = new();

            private readonly Mock<ConsumeContext<EnrolFeatureQuery>> _consumeContextMock = new();
            private readonly IConfiguration _configuration;

            public TestState()
            {
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.Load("CashrewardsOffers.Application"));

                _configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Feature:Experiment1MaxEnrolmentCount"] = "1000"
                    })
                    .Build();

                _consumeContextMock.Setup(c => c.Message).Returns(new EnrolFeatureQuery());
                _consumeContextMock.Setup(c => c.RespondAsync(It.IsAny<EnrolFeatureResponse>())).Callback<EnrolFeatureResponse>(response => EnrolFeatureResponse = response);

                EnrolFeatureQueryConsumer = new EnrolFeatureQueryConsumer(
                    _configuration,
                    FeaturePersistenceContext.Object,
                    CurrentUserService.Object,
                    Unleash.Object);
            }

            public async Task WhenConsume() => await EnrolFeatureQueryConsumer.Consume(_consumeContextMock.Object);
        }

        [Test]
        public async Task Consume_ShouldEnrolUserToExp1_GivenExp1EnrolFeatureIsEnabled()
        {
            var state = new TestState();
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp1)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(true);

            await state.WhenConsume();

            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, It.IsAny<string>()), Times.Once());
            state.EnrolFeatureResponse.Feature.Should().Be(FeatureToggle.Exp1);
        }

        [Test]
        public async Task Consume_ShouldNotEnrolUserToExp1_GivenExp1EnrolFeatureIsDisabled()
        {
            var state = new TestState();
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp1)).Returns(false);

            await state.WhenConsume();

            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, It.IsAny<string>()), Times.Never());
            state.EnrolFeatureResponse.Feature.Should().BeNull();
        }

        [Test]
        public async Task Consume_ShouldNotEnrolUserToExp1_GivenExp1EnrolmentIsFull()
        {
            var state = new TestState();
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp1)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(true);
            state.FeaturePersistenceContext
                .Setup(c => c.Count(It.IsAny<string>()))
                .ReturnsAsync(1000);

            await state.WhenConsume();

            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, It.IsAny<string>()), Times.Never());
            state.EnrolFeatureResponse.Feature.Should().BeNull();
        }

        [Test]
        public async Task Consume_ShouldNotEnrolUserToExp1_AndShouldReturnExistingFeature_GivenUserIsAlreadyEnroled()
        {
            var state = new TestState();
            state.CurrentUserService.Setup(u => u.UserId).Returns("111");
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp1)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(true);
            await state.FeaturePersistenceContext.Object.Enrol(FeatureToggle.Exp1, "111");
            state.FeaturePersistenceContext.Invocations.Clear();

            await state.WhenConsume();

            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, It.IsAny<string>()), Times.Never());
            state.EnrolFeatureResponse.Feature.Should().Be(FeatureToggle.Exp1);
        }

        [Test]
        public async Task Consume_ShouldEnrolUserToRandomExp_GivenMultipleExperimentsAreEnabled()
        {
            var state = new TestState();
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp1)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp2)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp2)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp3)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp3)).Returns(true);

            await state.WhenConsume();

            state.EnrolFeatureResponse.Feature.Should().BeOneOf(FeatureToggle.Exp1, FeatureToggle.Exp2, FeatureToggle.Exp3);
            state.FeaturePersistenceContext.Verify(f => f.Enrol(state.EnrolFeatureResponse.Feature, It.IsAny<string>()), Times.Once());
        }

        [Test]
        public async Task Consume_ShouldReturnNull_GivenUserIsAlreadyEnrolled_AndTheFeatureIsDisabled()
        {
            var state = new TestState();
            state.CurrentUserService.Setup(u => u.UserId).Returns("111");
            await state.FeaturePersistenceContext.Object.Enrol(FeatureToggle.Exp1, "111");
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp1)).Returns(false);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp1)).Returns(false);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp2)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp2)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.EnrolExp3)).Returns(true);
            state.Unleash.Setup(u => u.IsEnabled(FeatureToggle.Exp3)).Returns(true);

            await state.WhenConsume();

            state.EnrolFeatureResponse.Feature.Should().BeNull();
        }
    }
}
