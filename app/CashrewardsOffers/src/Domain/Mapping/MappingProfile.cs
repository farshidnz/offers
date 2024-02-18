using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Events;
using Mapster;

namespace CashrewardsOffers.Domain.Mapping
{
    public class MappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            MerchantEventMapping(config);
            OfferEventMapping(config);
        }

        private static void MerchantEventMapping(TypeAdapterConfig config)
        {
            config.NewConfig<Merchant, MerchantEventBase>()
                .Map(dest => dest.Id, src => (string)null);

            config.NewConfig<Merchant, MerchantInitial>()
                .Map(dest => dest.Id, src => (string)null);

            config.NewConfig<Merchant, MerchantChanged>()
                .Map(dest => dest.Id, src => (string)null);

            config.NewConfig<Merchant, MerchantDeleted>()
                .Map(dest => dest.Id, src => (string)null);
        }

        private static void OfferEventMapping(TypeAdapterConfig config)
        {
            config.NewConfig<Offer, OfferEventBase>()
                .Map(dest => dest.Id, src => (string)null)
                .Map(dest => dest.EndDateTime, src => src.EndDateTime.UtcTicks);

            config.NewConfig<Offer, OfferInitial>()
                .Map(dest => dest.Id, src => (string)null)
                .Map(dest => dest.EndDateTime, src => src.EndDateTime.UtcTicks);

            config.NewConfig<Offer, OfferChanged>()
                .Map(dest => dest.Id, src => (string)null)
                .Map(dest => dest.EndDateTime, src => src.EndDateTime.UtcTicks);

            config.NewConfig<Offer, OfferDeleted>()
                .Map(dest => dest.Id, src => (string)null);

            config.NewConfig<OfferMerchant, OfferMerchantChanged>()
                .Map(dest => dest.MobileAppEnabled, src => src.IsMobileAppEnabled);
        }
    }
}
