using CashrewardsOffers.Application.ANZ.Queries.GetAnzItems.v1;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Domain.ValueObjects;
using Mapster;
using System;

namespace CashrewardsOffers.Application.ANZ.Mappings
{
    public class MappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AnzItem, AnzItemInfo>()
                .Map(dest => dest.Id, src => src.ItemId)
                .Map(dest => dest.Merchant.StartDateTime, src => AnzTime.MinValue)
                .Map(dest => dest.Merchant.EndDateTime, src => AnzTime.MaxValue)
                .Map(dest => dest.Merchant.IsInstore, src => src.IsInstore)
                .Map(dest => dest.Merchant.InstoreRanking, src => src.IsInstore ? src.Merchant.InstoreRanking : 0)
                .Map(dest => dest.Merchant.IsPopular, src => src.IsPopular)
                .Map(dest => dest.Merchant.PopularRanking, src => src.IsPopular ? src.Merchant.PopularRanking : 0)
                .Map(dest => dest.Offer, src => src.Offer.Id > 0 ? src.Offer : null)
                .Map(dest => dest.Offer.IsFeatured, src => !src.IsDeleted && src.Offer.IsFeatured)
                .Map(dest => dest.Offer.FeaturedRanking, src => !src.IsDeleted && src.Offer.IsFeatured ? src.Offer.FeaturedRanking : 0)
                .Map(dest => dest.Offer.EndDateTime, src => new DateTimeOffset(src.Offer.EndDateTime, TimeSpan.Zero).ToUniversalTime())
                .Map(dest => dest.IsDeleted, src => src.IsUnavailable);

            config.NewConfig<MerchantChangedCategoryItem, AnzMerchantCategory>()
                .Map(dest => dest.Id, src => src.CategoryId);

            config.NewConfig<OfferMerchantChangedCategoryItem, AnzMerchantCategory>()
                .Map(dest => dest.Id, src => src.CategoryId);
        }
    }
}
