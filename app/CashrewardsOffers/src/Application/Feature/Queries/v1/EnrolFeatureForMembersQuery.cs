using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using CashrewardsOffers.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unleash;

namespace CashrewardsOffers.Application.Feature.Queries.v1
{
    public enum MemberIdType { Invalid, CognitoId, PersonId, MemberId, NewMemberId }

    public class EnrolFeatureForMembersQuery
    {
        public string IdType { get; set; }
        public string Feature { get; set; }
        public string[] Ids { get; set; }
    }

    public class EnrolFeatureForMembersResponse
    {
        public List<string> Ids { get; set; }
    }

    public class EnrolFeatureForMembersQueryConsumer : IConsumer<EnrolFeatureForMembersQuery>
    {
        private readonly IFeaturePersistenceContext _featurePersistenceContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnleashService _unleash;
        private readonly IShopGoSource _shopGoSource;
        private readonly long _maxUsersExperiment1;
        private readonly long _maxUsersExperiment2;
        private readonly long _maxUsersExperiment3;

        public EnrolFeatureForMembersQueryConsumer(
            IConfiguration configuration,
            IFeaturePersistenceContext featurePersistenceContext,
            ICurrentUserService currentUserService,
            IUnleashService unleash,
            IShopGoSource shopGoSource)
        {
            _featurePersistenceContext = featurePersistenceContext;
            _currentUserService = currentUserService;
            _unleash = unleash;
            _shopGoSource = shopGoSource;
            _maxUsersExperiment1 = long.TryParse(configuration["Feature:Experiment1MaxEnrolmentCount"], out var e1) ? e1 : 1000;
            _maxUsersExperiment2 = long.TryParse(configuration["Feature:Experiment2MaxEnrolmentCount"], out var e2) ? e2 : 1000;
            _maxUsersExperiment3 = long.TryParse(configuration["Feature:Experiment3MaxEnrolmentCount"], out var e3) ? e3 : 1000;
        }

        private async Task<string> LookupCognitoId(string idType, string id)
        {
            var type = Enum.Parse(typeof(MemberIdType), idType);
            switch (type)
            {
                case MemberIdType.CognitoId:
                    return id.ToLower();
                case MemberIdType.MemberId:
                    return await _shopGoSource.LookupCognitoIdFromMemberId(id);
                case MemberIdType.PersonId:
                    return await _shopGoSource.LookupCognitoIdFromPersonId(id);
                case MemberIdType.NewMemberId:
                    return await _shopGoSource.LookupCognitoIdFromNewMemberId(id);
                default:
                    return null;
            }
        }

        private async Task<Dictionary<string, long>> GetExperimentCounts(Experiment[] enabledExperiments)
        {
            Dictionary<string, long> experimentCounts = new Dictionary<string, long>();
            
            foreach (var enabledExperiment in enabledExperiments)
            {
                var count = await _featurePersistenceContext.Count(enabledExperiment.FeatureToggle);
                experimentCounts[enabledExperiment.FeatureToggle] = count;
            }

            return experimentCounts;
        } 

        public async Task Consume(ConsumeContext<EnrolFeatureForMembersQuery> context)
        {
            Log.Information($"Query: {JsonConvert.SerializeObject(context.Message)}");

            var response = new EnrolFeatureForMembersResponse();
            response.Ids = new List<string>();

            var enabledExperiments = EnabledExperiments;
            if (enabledExperiments.Any())
            {
                var experimentLookup = enabledExperiments.ToDictionary(exp => exp.FeatureToggle, exp => exp);
                var experimentCounts = await GetExperimentCounts(enabledExperiments);
                
                foreach (var memberId in context.Message.Ids)
                {
                    if (experimentLookup.TryGetValue(context.Message.Feature, out var experiment)
                        && experimentCounts[context.Message.Feature] < experiment.MaxEnrolmentCount)
                    {
                        var cognitoId = await LookupCognitoId(context.Message.IdType, memberId);
                        if (cognitoId == null)
                        {
                            continue;
                        }

                        var currentMemberFeature = await _featurePersistenceContext.Get(cognitoId);
                        if (currentMemberFeature == null)
                        {
                            response.Ids.Add(memberId);
                            Log.Information($"Enrolling member {cognitoId} to feature: {experiment.FeatureToggle}");
                            await _featurePersistenceContext.Enrol(experiment.FeatureToggle, cognitoId);
                        }
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
