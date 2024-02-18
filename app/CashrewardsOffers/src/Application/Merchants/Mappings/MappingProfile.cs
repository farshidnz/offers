using CashrewardsOffers.Application.Merchants.Models;
using CashrewardsOffers.Application.Merchants.Queries.GetEdmMerchants.v1;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using Mapster;

namespace CashrewardsOffers.Application.Merchants.Mappings
{
    public class MappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ShopGoMerchant, Merchant>()
                .Map(dest => dest.Client, src => src.ClientId);

            config.NewConfig<ShopGoMerchant, Merchant>()
                .Map(dest => dest.Client, src => src.ClientId)
                .Map(dest => dest.ClientProgramType, src => src.ClientProgramTypeId)
                .Map(dest => dest.CommissionType, src => src.TierCommTypeId)
                .Map(dest => dest.RewardType, src => src.TierTypeId)
                .Map(dest => dest.LogoUrl, src => src.RegularImageUrl)
                .Map(dest => dest.Name, src => src.MerchantName)
                .Map(dest => dest.MobileAppEnabled, src => src.IsMobileAppEnabled);

            config.NewConfig<Merchant, MerchantPremium>();

            config.NewConfig<Merchant, EdmMerchantInfo>();

            config.NewConfig<MerchantPremium, EdmMerchantPremiumInfo>()
                .Map(dest => dest.ClientCommissionString, src => src.ClientCommissionString);

            config.NewConfig<MerchantHistory, MerchantHistoryInfo>()
                .Map(dest => dest.ChangeInSydneyTime, src => SydneyTime.ToSydneyTime(src.ChangeTime));
        }
    }
}
