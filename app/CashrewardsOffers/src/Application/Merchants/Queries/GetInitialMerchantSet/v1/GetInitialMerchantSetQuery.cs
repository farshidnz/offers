using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Events;
using MassTransit;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Merchants.Queries.GetInitialMerchantSet.v1
{
    public class GetInitialMerchantSetQuery
    {
    }

    public class GetInitialMerchantSetResponse
    {
        public string Message { get; set; }
    }

    public class GetInitialMerchantSetQueryConsumer : IConsumer<GetInitialMerchantSetQuery>
    {
        private readonly IEventInitialisationService<MerchantInitial> _eventMerchantInitialisationService;

        public GetInitialMerchantSetQueryConsumer(
            IEventInitialisationService<MerchantInitial> eventMerchantInitialisationService)
        {
            _eventMerchantInitialisationService = eventMerchantInitialisationService;
        }

        public async Task Consume(ConsumeContext<GetInitialMerchantSetQuery> context)
        {
            GetInitialMerchantSetResponse response = new();

            if (_eventMerchantInitialisationService.IsRunning)
            {
                response.Message = "Merchant initialisation is already running";
            }
            else if (_eventMerchantInitialisationService.IsTriggered)
            {
                response.Message = "Merchant initialisation is already pending";
            }
            else
            {
                _eventMerchantInitialisationService.IsTriggered = true;
                response.Message = "Merchant initialisation triggered";
            }

            await context.RespondAsync(response);
        }
    }
}
