using CashrewardsOffers.API.Filters;
using CashrewardsOffers.Application.Feature.Queries.v1;
using CashrewardsOffers.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace CashrewardsOffers.API.Controllers.v1
{
    [ApiVersion("1")]
    [Authorize]
    public class FeatureController : BaseController
    {
        [HttpGet]
        [Route("enrol")]
        [ProducesResponseType(typeof(EnrolFeatureResponse), (int)HttpStatusCode.OK)]
        public async Task<EnrolFeatureResponse> Enrol() =>
            await Mediator.Query<EnrolFeatureQuery, EnrolFeatureResponse>(new EnrolFeatureQuery());

        [HttpPost]
        [Route("enrollMembers/{feature}/{idType}")]
        [UnleashAuthorizationFilter("BulkEnrolMembers")]
        [ProducesResponseType(typeof(EnrolFeatureForMembersResponse), (int)HttpStatusCode.OK)]
        public async Task<EnrolFeatureForMembersResponse> EnrolMembers([FromBody] string[] ids, string feature, string idType) =>
            await Mediator.Query<EnrolFeatureForMembersQuery, EnrolFeatureForMembersResponse>(
                new EnrolFeatureForMembersQuery() { IdType = idType, Feature = feature, Ids = ids });

        [HttpGet]
        [ProducesResponseType(typeof(GetFeatureResponse), (int)HttpStatusCode.OK)]
        public async Task<GetFeatureResponse> Get() =>
            await Mediator.Query<GetFeatureQuery, GetFeatureResponse>(new GetFeatureQuery());
    }
}
