using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Offers.Queries.GetInitialOfferSet.v1;
using CashrewardsOffers.Application.UnitTests.Helpers;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Offers
{
    public class GetInitialOfferSetQueryConsumerTests
    {
        private class TestState
        {
            public GetInitialOfferSetQueryConsumer GetInitialOfferSetQueryConsumer { get; }

            public Mock<IEventInitialisationService<OfferInitial>> EventInitialisationService { get; } = new();

            public QueryConsumeContextMock<GetInitialOfferSetQuery, GetInitialOfferSetResponse> ConsumeContext { get; } = new();

            public TestState()
            {
                GetInitialOfferSetQueryConsumer = new(EventInitialisationService.Object);
            }
        }

        [Test]
        public async Task Consume_ShouldTriggerInitialisation()
        {
            var state = new TestState();

            await state.GetInitialOfferSetQueryConsumer.Consume(state.ConsumeContext.Object);

            state.ConsumeContext.Response.Message.Should().Be("Offer initialisation triggered");
        }

        [Test]
        public async Task Consume_ShouldNotTriggerInitialisation_GivenAlreadyRunning()
        {
            var state = new TestState();
            state.EventInitialisationService.Setup(e => e.IsRunning).Returns(true);

            await state.GetInitialOfferSetQueryConsumer.Consume(state.ConsumeContext.Object);

            state.ConsumeContext.Response.Message.Should().Be("Offer initialisation is already running");
        }

        [Test]
        public async Task Consume_ShouldNotTriggerInitialisation_GivenAlreadyTriggered()
        {
            var state = new TestState();
            state.EventInitialisationService.Setup(e => e.IsTriggered).Returns(true);

            await state.GetInitialOfferSetQueryConsumer.Consume(state.ConsumeContext.Object);

            state.ConsumeContext.Response.Message.Should().Be("Offer initialisation is already pending");
        }
    }
}
