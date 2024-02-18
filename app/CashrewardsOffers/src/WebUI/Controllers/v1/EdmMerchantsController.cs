using CashrewardsOffers.Application.Merchants.Queries.GetEdmMerchants.v1;
using CashrewardsOffers.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace CashrewardsOffers.API.Controllers.v1
{
    [ApiVersion("1")]
    public class EdmMerchantsController : BaseController
    {
        [HttpGet]
        [ProducesResponseType(typeof(GetEdmMerchantsResponse), (int)HttpStatusCode.OK)]
        public async Task<GetEdmMerchantsResponse> Get(string newMemberId) =>
            await Mediator.Query<GetEdmMerchantsQuery, GetEdmMerchantsResponse>(new GetEdmMerchantsQuery { NewMemberId = newMemberId } );
    }
}
