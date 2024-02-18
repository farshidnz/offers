using CashrewardsOffers.Application.Common.Models;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Infrastructure.AWS;
using Mapster;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Extensions
{
    public static class MediatorExtensions
    {
        public static async Task<QueryResponse> Query<QueryType, QueryResponse>(this IMediator mediator, QueryType query) where QueryType : class
                                                                                                                          where QueryResponse : class
        {
            var client = mediator.CreateRequestClient<QueryType>();

            var response = await client.GetResponse<QueryResponse>(query);

            return response.Message;
        }

        public static async Task<ActionResult> Command<CommandType>(this IMediator mediator, CommandType command)
        {
            await mediator.Send(command);

            return new NoContentResult();
        }

        public static async Task PublishEvent(this IMediator mediator, DomainEvent domainEvent, Amazon.SQS.Model.Message sqsMessage)
        {
            var messageMetadata = sqsMessage.Adapt<AWSMessageMetadata>();

            await PublishEvent(mediator, domainEvent, messageMetadata);
        }

        public static async Task PublishEvent(this IMediator mediator, DomainEvent domainEvent, AWSMessageMetadata messageMetadata = default)
        {
            domainEvent.Metadata = messageMetadata?.GetEventMetadata() ?? domainEvent.Metadata;

            var message = Activator.CreateInstance(typeof(DomainEventNotification<>)
                                   .MakeGenericType(domainEvent.GetType()), domainEvent, messageMetadata);

            await mediator.Publish(message);
        }
    }
}
