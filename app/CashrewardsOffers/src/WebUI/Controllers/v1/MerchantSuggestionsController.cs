using CashrewardsOffers.Application.MerchantSuggestions.Queries.GetMerchantSuggestions.v1;
using CashrewardsOffers.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace CashrewardsOffers.API.Controllers.v1
{
    [ApiVersion("1")]
    public class MerchantSuggestionsController : BaseController
    {
        [HttpPost]
        [ProducesResponseType(typeof(GetMerchantSuggestionsResponse), (int)HttpStatusCode.OK)]
        public async Task<GetMerchantSuggestionsResponse> Get(GetMerchantSuggestionsQuery query) =>
            await Mediator.Query<GetMerchantSuggestionsQuery, GetMerchantSuggestionsResponse>(query);
    }
}
