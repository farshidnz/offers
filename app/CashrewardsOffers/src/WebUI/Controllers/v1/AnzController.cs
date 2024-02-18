using CashrewardsOffers.Application.ANZ.Queries.GetAnzItems.v1;
using CashrewardsOffers.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using CashrewardsOffers.API.Filters;

namespace CashrewardsOffers.API.Controllers.v1
{
    [ApiVersion("1")]
    public class AnzController : BaseController
    {
        [HttpGet]
        [ProducesResponseType(typeof(GetAnzItemsResponse), (int)HttpStatusCode.OK)]
        [UnleashAuthorizationFilter("AnzApi")]
        public async Task<GetAnzItemsResponse> Get(int offersPerPage, int pageNumber, long? updatedAfter) =>
            await Mediator.Query<GetAnzItemsQuery, GetAnzItemsResponse>(
                new GetAnzItemsQuery
                {
                    OffersPerPage = offersPerPage,
                    PageNumber = pageNumber,
                    UpdatedAfter = updatedAfter
                });
    }
}
