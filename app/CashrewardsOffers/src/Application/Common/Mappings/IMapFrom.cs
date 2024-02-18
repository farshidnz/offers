using Mapster;

namespace CashrewardsOffers.Application.Common.Mappings
{
    public interface IMapFrom<T>
    {
        void Mapping(TypeAdapterConfig config) => config.ForType(typeof(T), GetType());
    }
}