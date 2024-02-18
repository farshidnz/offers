using CashrewardsOffers.Application.Offers.Queries.GetEdmOffers.v1;
using CashrewardsOffers.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace CashrewardsOffers.API.Controllers.v1
{
    [ApiVersion("1")]
    public class EdmOffersController : BaseController
    {
        [HttpGet]
        [ProducesResponseType(typeof(GetEdmOffersResponse), (int)HttpStatusCode.OK)]
        public async Task<GetEdmOffersResponse> Get(string newMemberId, int? edmCampaignId) =>
            await Mediator.Query<GetEdmOffersQuery, GetEdmOffersResponse>(new GetEdmOffersQuery { NewMemberId = newMemberId, EDMCampaignId = edmCampaignId } );
    }
}
