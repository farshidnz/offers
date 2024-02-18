using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Infrastructure.Models;
using CashrewardsOffers.Infrastructure.Persistence;
using CashrewardsOffers.Infrastructure.Services;
using Mapster;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace CashrewardsOffers.Infrastructure.Mappings
{
    public class MappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            ToDocumentMapping(config);
            FromDocumentMapping(config);
        }

        private static void ToDocumentMapping(TypeAdapterConfig config)
        {
            config.NewConfig<DomainEvent, DomainEventDocument>()
                .Map(dest => dest._id, src => src.Id == null ? ObjectId.Empty : new ObjectId(src.Id))
                .Map(dest => dest.EventID, src => src.Metadata.EventID)
                .Map(dest => dest.EventType, src => src.GetType().Name)
                .Map(dest => dest.EventPayload, src => JsonConvert.SerializeObject(src));

            config.NewConfig<RankedMerchant, RankedMerchantDocument>()
                .Map(dest => dest._id, src => src.Id == null ? ObjectId.Empty : new ObjectId(src.Id));

            config.NewConfig<Merchant, MerchantDocument>()
                .Map(dest => dest._id, src => src.Id);


            config.NewConfig<Offer, OfferDocument>()
                .Map(dest => dest._id, src => src.Id == null ? ObjectId.Empty : new ObjectId(src.Id));

            config.NewConfig<OfferMerchant, OfferMerchantDocument>()
                .Map(dest => dest.MerchantId, src => src.Id)
                .Map(dest => dest.CategoryObjects, src => src.Categories)
                .Map(dest => dest.Categories, src => src.Categories != null ? src.Categories.Select(c => c.CategoryId) : null);

            config.NewConfig<AnzItem, AnzItemDocument>()
                .Map(dest => dest._id, src => src.Id == null ? ObjectId.Empty : new ObjectId(src.Id));

            config.NewConfig<MerchantHistory, MerchantHistoryDocument>()
                .Map(dest => dest._id, src => src.Id == null ? ObjectId.Empty : new ObjectId(src.Id));
        }

        private static void FromDocumentMapping(TypeAdapterConfig config)
        {
            config.NewConfig<DomainEventDocument, DomainEvent>()
                .ConstructUsing(src => ConstructFromPayload(src))
                .Map(dest => dest.Id, src => src._id.ToString());

            config.NewConfig<RankedMerchantDocument, RankedMerchant>()
                .Map(dest => dest.Id, src => src._id.ToString());

            config.NewConfig<MerchantDocument, Merchant>()
                .Map(dest => dest.Id, src => src._id.ToString());

            config.NewConfig<OfferDocument, Offer>()
                .Map(dest => dest.Id, src => src._id.ToString())
                .Map(dest => dest.Merchant.Categories, src => src.Merchant.CategoryObjects);

            config.NewConfig<OfferMerchantDocument, OfferMerchant>()
                .Map(dest => dest.Id, src => src.MerchantId)
                .Map(dest => dest.Categories, src => src.CategoryObjects);

            config.NewConfig<AnzItemDocument, AnzItem>()
                .ConstructUsing(src => (MapContext.Current.Parameters["anzItemFactory"] as IAnzItemFactory).Create(src.Merchant.Id, src.Offer.Id))
                .Map(dest => dest.Id, src => src._id.ToString());

            config.NewConfig<MerchantHistoryDocument, MerchantHistory>()
                .Map(dest => dest.Id, src => src._id.ToString());
        }

        private static DomainEvent ConstructFromPayload(DomainEventDocument src)
        {
            var eventType = (MapContext.Current.Parameters["eventTypeResolver"] as IEventTypeResolver).GetEventType(src.EventType);
            return JsonConvert.DeserializeObject(src.EventPayload, eventType) as DomainEvent;
        }
    }
}
