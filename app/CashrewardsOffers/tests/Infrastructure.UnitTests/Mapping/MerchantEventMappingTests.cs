using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Infrastructure;
using CashrewardsOffers.Infrastructure.Models;
using CashrewardsOffers.Infrastructure.Services;
using FluentAssertions;
using Mapster;
using MongoDB.Bson;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Infrustructure.UnitTests.Mapping
{
    public class MerchantEventMappingTests
    {
        private static List<DomainEvent> DomainEvents => new()
        {
            new MerchantChanged
            {
                Id = "62959692f37aeacd7919d629",
                Metadata = new EventMetadata
                {
                    EventID = Guid.Parse("11111111-1111-1111-1111-111111111e1d"),
                    EventSource = "CashrewardsOffer",
                    EventType = typeof(MerchantChanged).Name,
                    Domain = "Offer",
                    CorrelationID = Guid.Parse("11111111-1111-1111-1111-111111111c1d"),
                    RaisedDateTimeUTC = new DateTimeOffset(2022, 1, 1, 1, 2, 3, TimeSpan.Zero),
                    PublishedDateTimeUTC = new DateTimeOffset(2022, 1, 1, 1, 2, 4, TimeSpan.Zero),
                    ContextualSequenceNumber = 1
                },
                Client = Client.Cashrewards,
                MerchantId = 1,
                HyphenatedString = "merchant-1",
                ClientCommissionString = "2%"
            },
            new MerchantDeleted
            {
                Id = "62959692f37aeacd7919d622",
                Metadata = new EventMetadata
                {
                    EventID = Guid.Parse("22222222-2222-2222-2222-222222222e1d"),
                    EventSource = "CashrewardsOffer",
                    EventType = typeof(MerchantChanged).Name,
                    Domain = "Offer",
                    CorrelationID = Guid.Parse("22222222-2222-2222-2222-222222222c1d"),
                    RaisedDateTimeUTC = new DateTimeOffset(2022, 1, 1, 1, 2, 3, TimeSpan.Zero),
                    PublishedDateTimeUTC = new DateTimeOffset(2022, 1, 1, 1, 2, 4, TimeSpan.Zero),
                    ContextualSequenceNumber = 1
                },
                Client = Client.Cashrewards,
                MerchantId = 1,
                HyphenatedString = "merchant-1",
            },
            new MerchantInitial
            {
                Id = "62959692f37aeacd7919d629",
                Metadata = new EventMetadata
                {
                    EventID = Guid.Parse("33333333-3333-3333-3333-333333333e1d"),
                    EventSource = "CashrewardsOffer",
                    EventType = typeof(MerchantChanged).Name,
                    Domain = "Offer",
                    CorrelationID = Guid.Parse("33333333-3333-3333-3333-333333333c1d"),
                    RaisedDateTimeUTC = new DateTimeOffset(2022, 1, 1, 1, 2, 3, TimeSpan.Zero),
                    PublishedDateTimeUTC = new DateTimeOffset(2022, 1, 1, 1, 2, 4, TimeSpan.Zero),
                    ContextualSequenceNumber = 1
                },
                Client = Client.Cashrewards,
                MerchantId = 1,
                HyphenatedString = "merchant-1",
                ClientCommissionString = "2%"
            },
        };

        private static List<DomainEventDocument> DomainEventDocuments => new()
        {
            new DomainEventDocument
            {
                _id = new ObjectId("62959692f37aeacd7919d629"),
                EventID = Guid.Parse("11111111-1111-1111-1111-111111111e1d"),
                EventType = "MerchantChanged",
                EventPayload = JsonConvert.SerializeObject(DomainEvents[0])
            },
            new DomainEventDocument
            {
                _id = new ObjectId("62959692f37aeacd7919d622"),
                EventID = Guid.Parse("22222222-2222-2222-2222-222222222e1d"),
                EventType = "MerchantDeleted",
                EventPayload = JsonConvert.SerializeObject(DomainEvents[1])
            },
            new DomainEventDocument
            {
                _id = new ObjectId("62959692f37aeacd7919d629"),
                EventID = Guid.Parse("33333333-3333-3333-3333-333333333e1d"),
                EventType = "MerchantInitial",
                EventPayload = JsonConvert.SerializeObject(DomainEvents[2])
            }
        };

        [Test]
        public void Adapt_ShouldMapToDomainEventDocument_GivenDomainEvent()
        {
            DependencyInjection.RegisterMappingProfiles();

            var domainEventDocument = DomainEvents[0].Adapt<DomainEventDocument>();

            domainEventDocument.Should().BeEquivalentTo(DomainEventDocuments[0]);
        }

        [Test]
        public void Adapt_ShouldMapToDomainEventDocuments_GivenDomainEvents()
        {
            DependencyInjection.RegisterMappingProfiles();

            var domainEventDocuments = DomainEvents.Adapt<List<DomainEventDocument>>();

            domainEventDocuments.Should().BeEquivalentTo(DomainEventDocuments);
        }

        [Test]
        public void Adapt_ShouldMapToDomainEvent_GivenDomainEventDocument()
        {
            DependencyInjection.RegisterMappingProfiles();

            var domainEvent = DomainEventDocuments[0]
                .BuildAdapter()
                .AddParameters("eventTypeResolver", new EventTypeResolver())
                .AdaptToType<DomainEvent>();

            domainEvent.Should().BeOfType<MerchantChanged>();
            domainEvent.Should().BeEquivalentTo(DomainEvents[0]);
        }

        [Test]
        public void Adapt_ShouldMapToDomainEvents_GivenDomainEventDocuments()
        {
            DependencyInjection.RegisterMappingProfiles();

            var domainEvents = DomainEventDocuments
                .BuildAdapter()
                .AddParameters("eventTypeResolver", new EventTypeResolver())
                .AdaptToType<List<DomainEvent>>();

            domainEvents.Should().BeEquivalentTo(DomainEvents);
            domainEvents[0].Should().BeOfType<MerchantChanged>();
            domainEvents[1].Should().BeOfType<MerchantDeleted>();
            domainEvents[2].Should().BeOfType<MerchantInitial>();
        }
    }
}
