using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using CashrewardsOffers.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unleash;

namespace CashrewardsOffers.Application.Feature.Queries.v1
{
    public class EnrolFeatureQuery
    {
    }

    public class EnrolFeatureResponse
    {
        public string Feature { get; set; }
    }

    public class EnrolFeatureQueryConsumer : IConsumer<EnrolFeatureQuery>
    {
        private readonly IFeaturePersistenceContext _featurePersistenceContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnleashService _unleash;
        private readonly long _maxUsersExperiment1;
        private readonly long _maxUsersExperiment2;
        private readonly long _maxUsersExperiment3;
        private readonly Random _random = new();

        public EnrolFeatureQueryConsumer(
            IConfiguration configuration,
            IFeaturePersistenceContext featurePersistenceContext,
            ICurrentUserService currentUserService,
            IUnleashService unleash)
        {
            _featurePersistenceContext = featurePersistenceContext;
            _currentUserService = currentUserService;
            _unleash = unleash;
            _maxUsersExperiment1 = long.TryParse(configuration["Feature:Experiment1MaxEnrolmentCount"], out var e1) ? e1 : 1000;
            _maxUsersExperiment2 = long.TryParse(configuration["Feature:Experiment2MaxEnrolmentCount"], out var e2) ? e2 : 1000;
            _maxUsersExperiment3 = long.TryParse(configuration["Feature:Experiment3MaxEnrolmentCount"], out var e3) ? e3 : 1000;
        }

        public async Task Consume(ConsumeContext<EnrolFeatureQuery> context)
        {
            Log.Information($"Query: {JsonConvert.SerializeObject(context.Message)}");

            var response = new EnrolFeatureResponse();

            var enabledExperiments = EnabledExperiments;
            if (enabledExperiments.Any())
            {
                var experiment = enabledExperiments[_random.Next(enabledExperiments.Length)];
                if (await _featurePersistenceContext.Count(experiment.FeatureToggle) < experiment.MaxEnrolmentCount)
                {
                    response.Feature = await _featurePersistenceContext.Get(_currentUserService.UserId);
                    if (response.Feature == null)
                    {
                        Log.Information($"Enroling to feature: {experiment.FeatureToggle}");
                        await _featurePersistenceContext.Enrol(experiment.FeatureToggle, _currentUserService.UserId);
                        response.Feature = experiment.FeatureToggle;
                    }

                    if (!enabledExperiments.Any(e => e.FeatureToggle == response.Feature))
                    {
                        response.Feature = null;
                    }
                }
            }

            await context.RespondAsync(response);
        }

        private Experiment[] EnabledExperiments => new Experiment[]
            {
                new Experiment(FeatureToggle.Exp1, FeatureToggle.EnrolExp1, _maxUsersExperiment1),
                new Experiment(FeatureToggle.Exp2, FeatureToggle.EnrolExp2, _maxUsersExperiment2),
                new Experiment(FeatureToggle.Exp3, FeatureToggle.EnrolExp3, _maxUsersExperiment3),
            }
            .Where(f => _unleash.IsEnabled(f.EnrolFeatureToggle) && _unleash.IsEnabled(f.FeatureToggle))
            .ToArray();

    }
}
