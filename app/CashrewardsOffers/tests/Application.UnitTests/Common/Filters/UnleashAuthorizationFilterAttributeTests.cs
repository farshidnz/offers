using CashrewardsOffers.API.Filters;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using CashrewardsOffers.Application.UnitTests.Common.TestHelpers;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace CashrewardsOffers.Application.UnitTests.Common.Filters
{
    public class UnleashAuthorizationFilterAttributeTests
    {
        private class TestState
        {
            public UnleashAuthorizationFilterAttribute UnleashAuthorizationFilterAttribute { get; }
            public AuthorizationFilterContextMock AuthorizationFilterContextMock { get; } = new();
            public Mock<IUnleashService> UnleashService { get; } = new();
            public Mock<ICurrentUserService> CurrentUserService { get; } = new();

            public TestState(string toggleName)
            {
                AuthorizationFilterContextMock.AddRequestService(UnleashService);
                AuthorizationFilterContextMock.AddRequestService(CurrentUserService);

                UnleashAuthorizationFilterAttribute = new(toggleName);
            }
        }

        [Test]
        public void OnAuthorization_ShouldThrowArgumentNullException_GivenContextIsNull()
        {
            var state = new TestState("MyToggle");

            Action action = () => state.UnleashAuthorizationFilterAttribute.OnAuthorization(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void OnAuthorization_ShouldReturn401_GivenFeatureIsNotEnabled()
        {
            var state = new TestState("MyToggle");

            state.UnleashAuthorizationFilterAttribute.OnAuthorization(state.AuthorizationFilterContextMock.Object);

            state.AuthorizationFilterContextMock.Result.Should().Contain(@"StatusCode"":401");
        }

        [Test]
        public void OnAuthorization_ShouldAuthorize_GivenFeatureIsEnabledForUser()
        {
            var state = new TestState("MyToggle");
            state.CurrentUserService.Setup(c => c.UserId).Returns("1234");
            state.UnleashService.Setup(u => u.IsEnabled("MyToggle", "1234")).Returns(true);

            state.UnleashAuthorizationFilterAttribute.OnAuthorization(state.AuthorizationFilterContextMock.Object);

            state.AuthorizationFilterContextMock.Result.Should().Be("null");
        }
    }
}
