using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.Feature.Queries.v1;
using CashrewardsOffers.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.AcceptanceTests
{
    public class EnrolFeatureQueryTests
    {
        [Test]
        public async Task EnrolFeatureQuery_ShouldEnrolPersonIntoExperiment()
        {
            var fixture = new Fixture();
            fixture.GivenUnleashFeatureToggle(FeatureToggle.EnrolExp1);
            fixture.GivenUnleashFeatureToggle(FeatureToggle.Exp1);
            fixture.GivenPerson();

            var response = await fixture.WhenISendTheQuery<EnrolFeatureQuery, EnrolFeatureResponse>(new EnrolFeatureQuery());

            response!.Feature.Should().Be(FeatureToggle.Exp1);
        }

        [Test]
        public async Task EnrolFeatureQuery_ShouldEnrolPeopleUpToCapacity_GivenSingleExperiment()
        {
            var fixture = new Fixture(new Dictionary<string, string> { ["Feature:Experiment1MaxEnrolmentCount"] = "5" });
            fixture.GivenUnleashFeatureToggle(FeatureToggle.EnrolExp1);
            fixture.GivenUnleashFeatureToggle(FeatureToggle.Exp1);

            for (int i = 0; i < 5; i++)
            {
                fixture.GivenPerson(cognitoId: i.ToString());
                var response1 = await fixture.WhenISendTheQuery<EnrolFeatureQuery, EnrolFeatureResponse>(new EnrolFeatureQuery());
                response1!.Feature.Should().Be(FeatureToggle.Exp1);
            }

            fixture.GivenPerson(cognitoId: "6");
            var response2 = await fixture.WhenISendTheQuery<EnrolFeatureQuery, EnrolFeatureResponse>(new EnrolFeatureQuery());
            response2!.Feature.Should().BeNull();
        }

        [Test]
        public async Task EnrolFeatureQuery_ShouldEnrolPeopleUpToCapacity_GivenAllExperiments()
        {
            var fixture = new Fixture(new Dictionary<string, string>
            {
                ["Feature:Experiment1MaxEnrolmentCount"] = "3",
                ["Feature:Experiment2MaxEnrolmentCount"] = "4",
                ["Feature:Experiment3MaxEnrolmentCount"] = "5"
            });
            fixture.GivenUnleashFeatureToggle(FeatureToggle.EnrolExp1);
            fixture.GivenUnleashFeatureToggle(FeatureToggle.EnrolExp2);
            fixture.GivenUnleashFeatureToggle(FeatureToggle.EnrolExp3);
            fixture.GivenUnleashFeatureToggle(FeatureToggle.Exp1);
            fixture.GivenUnleashFeatureToggle(FeatureToggle.Exp2);
            fixture.GivenUnleashFeatureToggle(FeatureToggle.Exp3);

            var enrolCounts = new Dictionary<string, int>
            {
                ["Exp1"] = 0,
                ["Exp2"] = 0,
                ["Exp3"] = 0,
                ["not-enroled"] = 0
            };
            for (int i = 0; i < 200; i++)
            {
                fixture.GivenPerson(cognitoId: i.ToString());
                var response = await fixture.WhenISendTheQuery<EnrolFeatureQuery, EnrolFeatureResponse>(new EnrolFeatureQuery());
                enrolCounts[response!.Feature ?? "not-enroled"]++;
            }

            enrolCounts["Exp1"].Should().Be(3);
            enrolCounts["Exp2"].Should().Be(4);
            enrolCounts["Exp3"].Should().Be(5);
        }
    }
}
