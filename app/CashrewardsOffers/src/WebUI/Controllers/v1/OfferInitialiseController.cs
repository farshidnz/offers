using CashrewardsOffers.API.Filters;
using CashrewardsOffers.Application.Offers.Queries.GetInitialOfferSet.v1;
using CashrewardsOffers.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace CashrewardsOffers.API.Controllers.v1
{
    [ApiVersion("1")]
    [Authorize]
    [UnleashAuthorizationFilter("InitialisationEvent")]
    public class OfferInitialiseController : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(GetInitialOfferSetResponse), (int)HttpStatusCode.OK)]
        public async Task<GetInitialOfferSetResponse> Post(GetInitialOfferSetQuery query = null) =>
            await Mediator.Query<GetInitialOfferSetQuery, GetInitialOfferSetResponse>(query ?? new GetInitialOfferSetQuery());
    }
}
