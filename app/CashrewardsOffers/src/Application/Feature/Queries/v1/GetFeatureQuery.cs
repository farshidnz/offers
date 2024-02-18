using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using CashrewardsOffers.Domain.Entities;
using MassTransit;
using Newtonsoft.Json;
using Serilog;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Feature.Queries.v1
{
    public class GetFeatureQuery
    {
    }

    public class GetFeatureResponse
    {
        public string Feature { get; set; }
    }

    public class GetFeatureQueryConsumer : IConsumer<GetFeatureQuery>
    {
        private readonly IFeaturePersistenceContext _featurePersistenceContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnleashService _unleash;

        public GetFeatureQueryConsumer(
            IFeaturePersistenceContext featurePersistenceContext,
            ICurrentUserService currentUserService,
            IUnleashService unleash)
        {
            _featurePersistenceContext = featurePersistenceContext;
            _currentUserService = currentUserService;
            _unleash = unleash;
        }

        public async Task Consume(ConsumeContext<GetFeatureQuery> context)
        {
            Log.Information($"Query: {JsonConvert.SerializeObject(context.Message)}");

            var response = new GetFeatureResponse();

            if (_unleash.IsEnabled(FeatureToggle.Exp1) || _unleash.IsEnabled(FeatureToggle.Exp2) || _unleash.IsEnabled(FeatureToggle.Exp3))
            {
                var userFeature = await _featurePersistenceContext.Get(_currentUserService.UserId);
                if (!string.IsNullOrEmpty(userFeature) && _unleash.IsEnabled(userFeature))
                {
                    response.Feature = userFeature;
                }
            }

            await context.RespondAsync(response);
        }
    }
}
