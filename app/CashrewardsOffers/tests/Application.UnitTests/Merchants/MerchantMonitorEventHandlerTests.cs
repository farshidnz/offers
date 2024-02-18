using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Merchants.EventHandlers;
using CashrewardsOffers.Application.UnitTests.Helpers;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Events;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Merchants
{
    public class MerchantMonitorEventHandlerTests
    {
        private class TestState
        {
            public MerchantMonitorEventHandler MerchantMonitorEventHandler { get; }

            public Mock<IMerchantHistoryPersistenceContext> MerchantHistoryPersistenceContext { get; } = new();

            public EventConsumeContextMock<MerchantChanged> MerchantChangedConsumeContext { get; } = new();
            public EventConsumeContextMock<MerchantDeleted> MerchantDeletedConsumeContext { get; } = new();

            public List<MerchantHistory> AddedHistories { get; } = new();

            public TestState()
            {
                MerchantHistoryPersistenceContext
                    .Setup(c => c.Add(It.IsAny<MerchantHistory>()))
                    .Callback((MerchantHistory h) => AddedHistories.Add(h));

                MerchantMonitorEventHandler = new MerchantMonitorEventHandler(MerchantHistoryPersistenceContext.Object);
            }
        }

        [Test]
        public async Task Consume_ShouldAddHistory_GivenMerchantChangedEvent()
        {
            var state = new TestState();
            state.MerchantChangedConsumeContext.DomainEvent.ClientCommissionString = "10%";

            await state.MerchantMonitorEventHandler.Consume(state.MerchantChangedConsumeContext.Object);

            state.AddedHistories[0].ClientCommissionString.Should().Be("10%");
        }

        [Test]
        public async Task Consume_ShouldAddHistory_GivenMerchantDeletedEvent()
        {
            var state = new TestState();

            await state.MerchantMonitorEventHandler.Consume(state.MerchantDeletedConsumeContext.Object);

            state.AddedHistories[0].ClientCommissionString.Should().Be("0%");
        }
    }
}
