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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Feature
{
    public class EnrolFeatureForMembersQueryConsumerTests
    {
        private class TestState
        {
            public EnrolFeatureForMembersQueryConsumer EnrolFeatureForMembersQueryConsumer { get; }

            public EnrolFeatureForMembersResponse EnrolFeatureResponse { get; private set; }

            public MockUnleash Unleash { get; } = new();

            public MockFeaturePersistenceContext FeaturePersistenceContext { get; } = new();

            public Mock<ICurrentUserService> CurrentUserService { get; } = new();

            public Mock<IShopGoSource> ShopGoSource { get; } = new();

            private readonly Mock<ConsumeContext<EnrolFeatureForMembersQuery>> _consumeContextMock = new();
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

                _consumeContextMock.Setup(c => c.RespondAsync(It.IsAny<EnrolFeatureForMembersResponse>()))
                    .Callback<EnrolFeatureForMembersResponse>(response => EnrolFeatureResponse = response);

                EnrolFeatureForMembersQueryConsumer = new EnrolFeatureForMembersQueryConsumer(
                    _configuration,
                    FeaturePersistenceContext.Object,
                    CurrentUserService.Object,
                    Unleash.Object,
                    ShopGoSource.Object);
            }

            public async Task WhenConsume() =>
                await EnrolFeatureForMembersQueryConsumer.Consume(_consumeContextMock.Object);

            public void GivenLookupIds(string idType, params (string idSource, string cognitoId)[] ids)
            {
                switch (idType)
                {
                    case "MemberId":
                        ids.ToList().ForEach(idPair =>
                            ShopGoSource.Setup(s => s.LookupCognitoIdFromMemberId(idPair.idSource)).Returns(Task.FromResult(idPair.cognitoId)));
                        break;
                    case "NewMemberId":
                        ids.ToList().ForEach(idPair =>
                            ShopGoSource.Setup(s => s.LookupCognitoIdFromNewMemberId(idPair.idSource)).Returns(Task.FromResult(idPair.cognitoId)));
                        break;
                    case "PersonId":
                        ids.ToList().ForEach(idPair =>
                            ShopGoSource.Setup(s => s.LookupCognitoIdFromPersonId(idPair.idSource)).Returns(Task.FromResult(idPair.cognitoId)));
                        break;
                }
            }

            public void GivenQueryInput(string feature, string idType, string[] memberIds)
            {
                _consumeContextMock.Setup(c => c.Message).Returns(new EnrolFeatureForMembersQuery()
                {
                    Feature = feature,
                    IdType = idType,
                    Ids = memberIds
                });
            }

            public void GivenFeatureEnabled(string featureToggle)
            {
                Unleash.Setup(u => u.IsEnabled(featureToggle)).Returns(true);
            }
        }

        [Test]
        public async Task Consume_ShouldEnrolUserToExp1_WhenExp1IsSpecifiedAsParameter_AndCognitoIdIsGiven()
        {
            var state = new TestState();
            state.GivenFeatureEnabled(FeatureToggle.EnrolExp1);
            state.GivenFeatureEnabled(FeatureToggle.Exp1);

            state.GivenQueryInput("Exp1", "CognitoId", new string[] {"111111", "222222"});

            await state.WhenConsume();

            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, "111111"),
                Times.Once());
            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, "222222"),
                Times.Once());

            var enrolledMembers = state.EnrolFeatureResponse.Ids;
            enrolledMembers[0].Should().Be("111111");
            enrolledMembers[1].Should().Be("222222");
        }

        [Test]
        public async Task Consume_WhenMemberAlreadyEnrolledIntoExp1_ShouldNotOverrideEnrollmentToExp2()
        {
            var state = new TestState();
            state.GivenFeatureEnabled(FeatureToggle.EnrolExp1);
            state.GivenFeatureEnabled(FeatureToggle.Exp1);
            state.GivenFeatureEnabled(FeatureToggle.EnrolExp2);
            state.GivenFeatureEnabled(FeatureToggle.Exp2);

            state.GivenQueryInput("Exp2", "CognitoId", new string[] { "111111" });

            await state.FeaturePersistenceContext.Object.Enrol("Exp1", "111111");

            await state.WhenConsume();

            state.FeaturePersistenceContext.Verify(f => f.Enrol("Exp2", "111111"),
                Times.Never());

            var enrolledMembers = state.EnrolFeatureResponse.Ids;
            enrolledMembers.Should().BeEmpty();
        }

        [Test]
        public async Task Consume_ShouldEnrolUserToExp1_WhenExp1IsSpecifiedAsParameter_AndMemberIdIsGiven()
        {
            var state = new TestState();
            state.GivenFeatureEnabled(FeatureToggle.EnrolExp1);
            state.GivenFeatureEnabled(FeatureToggle.Exp1);

            state.GivenLookupIds("MemberId", ("111111", "222222"));
            state.GivenQueryInput("Exp1", "MemberId", new string[] { "111111" });

            await state.WhenConsume();

            state.ShopGoSource.Verify(f => f.LookupCognitoIdFromMemberId("111111"));
            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, "222222"),
                Times.Once());

            var enrolledMembers = state.EnrolFeatureResponse.Ids;
            enrolledMembers[0].Should().Be("111111");
        }

        [Test]
        public async Task Consume_ShouldEnrolUserToExp1_WhenExp1IsSpecifiedAsParameter_AndNewMemberIdIsGiven()
        {
            var state = new TestState();
            state.GivenFeatureEnabled(FeatureToggle.EnrolExp1);
            state.GivenFeatureEnabled(FeatureToggle.Exp1);

            state.GivenLookupIds("NewMemberId", ("111111", "222222"));
            state.GivenQueryInput("Exp1", "NewMemberId", new string[] { "111111" });

            await state.WhenConsume();

            state.ShopGoSource.Verify(f => f.LookupCognitoIdFromNewMemberId("111111"));
            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, "222222"),
                Times.Once());

            var enrolledMembers = state.EnrolFeatureResponse.Ids;
            enrolledMembers[0].Should().Be("111111");
        }

        [Test]
        public async Task Consume_ShouldEnrolUserToExp1_WhenExp1IsSpecifiedAsParameter_AndPersonIdIsGiven()
        {
            var state = new TestState();
            state.GivenFeatureEnabled(FeatureToggle.EnrolExp1);
            state.GivenFeatureEnabled(FeatureToggle.Exp1);

            state.GivenLookupIds("PersonId", ("111111", "222222"));
            state.GivenQueryInput("Exp1", "PersonId", new string[] { "111111" });

            await state.WhenConsume();

            state.ShopGoSource.Verify(f => f.LookupCognitoIdFromPersonId("111111"));
            state.FeaturePersistenceContext.Verify(f => f.Enrol(FeatureToggle.Exp1, "222222"),
                Times.Once());

            var enrolledMembers = state.EnrolFeatureResponse.Ids;
            enrolledMembers[0].Should().Be("111111");
        }
    }
}