using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Merchants.Queries.GetInitialMerchantSet.v1;
using CashrewardsOffers.Application.UnitTests.Helpers;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class GetInitialMerchantSetQueryConsumerTests
    {
        private class TestState
        {
            public GetInitialMerchantSetQueryConsumer GetInitialMerchantSetQueryConsumer { get; }

            public Mock<IEventInitialisationService<MerchantInitial>> EventInitialisationService { get; } = new();

            public QueryConsumeContextMock<GetInitialMerchantSetQuery, GetInitialMerchantSetResponse> ConsumeContext { get; } = new();

            public TestState()
            {
                GetInitialMerchantSetQueryConsumer = new(EventInitialisationService.Object);
            }
        }

        [Test]
        public async Task Consume_ShouldTriggerInitialisation()
        {
            var state = new TestState();

            await state.GetInitialMerchantSetQueryConsumer.Consume(state.ConsumeContext.Object);

            state.ConsumeContext.Response.Message.Should().Be("Merchant initialisation triggered");
        }

        [Test]
        public async Task Consume_ShouldNotTriggerInitialisation_GivenAlreadyRunning()
        {
            var state = new TestState();
            state.EventInitialisationService.Setup(e => e.IsRunning).Returns(true);

            await state.GetInitialMerchantSetQueryConsumer.Consume(state.ConsumeContext.Object);

            state.ConsumeContext.Response.Message.Should().Be("Merchant initialisation is already running");
        }

        [Test]
        public async Task Consume_ShouldNotTriggerInitialisation_GivenAlreadyTriggered()
        {
            var state = new TestState();
            state.EventInitialisationService.Setup(e => e.IsTriggered).Returns(true);

            await state.GetInitialMerchantSetQueryConsumer.Consume(state.ConsumeContext.Object);

            state.ConsumeContext.Response.Message.Should().Be("Merchant initialisation is already pending");
        }
    }
}
