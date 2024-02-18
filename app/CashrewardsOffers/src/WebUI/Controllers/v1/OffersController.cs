using CashrewardsOffers.Application.Offers.Queries.GetOffers.v1;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CashrewardsOffers.API.Controllers.v1
{
    [ApiVersion("1")]
    public class OffersController : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(GetOffersResponse), (int)HttpStatusCode.OK)]
        public async Task<GetOffersResponse> Post(GetOffersQuery query) =>
            await Mediator.Query<GetOffersQuery, GetOffersResponse>(query);

        [HttpGet]
        [ProducesResponseType(typeof(GetOffersResponse), (int)HttpStatusCode.OK)]
        public async Task<GetOffersResponse> Get(
            int clientId = (int)Client.Cashrewards,
            int? premiumClientId = null,
            int? categoryId = null,
            bool isMobile = false,
            bool isFeatured = false
            ) =>
            await Mediator.Query<GetOffersQuery, GetOffersResponse>(new GetOffersQuery
            {
                ClientId = clientId,
                PremiumClientId = premiumClientId,
                CategoryId = categoryId,
                IsMobile = isMobile,
                IsFeatured = isFeatured
            });
    }
}
