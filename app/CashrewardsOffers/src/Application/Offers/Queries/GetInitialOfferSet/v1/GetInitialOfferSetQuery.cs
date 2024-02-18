using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Offers.Queries.GetInitialOfferSet.v1
{
    public class GetInitialOfferSetQuery
    {
    }

    public class GetInitialOfferSetResponse
    {
        public string Message { get; set; }
    }

    public class GetInitialOfferSetQueryConsumer : IConsumer<GetInitialOfferSetQuery>
    {
        private readonly IEventInitialisationService<OfferInitial> _eventOfferInitialisationService;

        public GetInitialOfferSetQueryConsumer(
            IEventInitialisationService<OfferInitial> eventOfferInitialisationService)
        {
            _eventOfferInitialisationService = eventOfferInitialisationService;
        }

        public async Task Consume(ConsumeContext<GetInitialOfferSetQuery> context)
        {
            GetInitialOfferSetResponse response = new();

            if (_eventOfferInitialisationService.IsRunning)
            {
                response.Message = "Offer initialisation is already running";
            }
            else if (_eventOfferInitialisationService.IsTriggered)
            {
                response.Message = "Offer initialisation is already pending";
            }
            else
            {
                _eventOfferInitialisationService.IsTriggered = true;
                response.Message = "Offer initialisation triggered";
            }

            await context.RespondAsync(response);
        }
    }
}
