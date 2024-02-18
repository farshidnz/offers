using CashrewardsOffers.API.Filters;
using CashrewardsOffers.Application.Merchants.Queries.GetInitialMerchantSet.v1;
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
    public class MerchantInitialiseController : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(GetInitialMerchantSetResponse), (int)HttpStatusCode.OK)]
        public async Task<GetInitialMerchantSetResponse> Post(GetInitialMerchantSetQuery query = null) =>
            await Mediator.Query<GetInitialMerchantSetQuery, GetInitialMerchantSetResponse>(query ?? new GetInitialMerchantSetQuery());
    }
}
