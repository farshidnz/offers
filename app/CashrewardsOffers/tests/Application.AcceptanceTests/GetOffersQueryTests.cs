using Application.AcceptanceTests.Helpers;
using CashrewardsOffers.Application.Feature.Queries.v1;
using CashrewardsOffers.Application.Offers.Queries.GetOffers.v1;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.UnitTests.Offers
{
    public class GetOffersQueryTests
    {
        [Test]
        public async Task GetOffersQuery_ShouldReturnOffers_GivenOffersInDatabase()
        {
            var fixture = new Fixture();
            fixture.GivenOffer(title: "Offer 1");
            await fixture.GivenOfferSyncJobHasRun();

            var response = await fixture.WhenISendTheQuery<GetOffersQuery, GetOffersResponse>(
                new GetOffersQuery
                {
                    ClientId = (int)Client.Cashrewards
                });

            response!.Data.Length.Should().Be(1);
            response!.Data[0].Title.Should().Be("Offer 1");
        }

        [Test]
        public async Task GetOffersQuery_ShouldOrderOffersByEndDateTime_GivenOffersInDatabase()
        {
            var fixture = new Fixture();
            fixture.GivenOffer(title: "Offer 1", ranking: 5);
            fixture.GivenOffer(title: "Offer 2", ranking: 4);
            fixture.GivenOffer(title: "Offer 3", ranking: 1, dateEnd: new DateTime(2022, 1, 3));
            fixture.GivenOffer(title: "Offer 4", ranking: 1, dateEnd: new DateTime(2022, 1, 2));
            fixture.GivenOffer(title: "Offer 5", ranking: 1, dateEnd: new DateTime(2022, 1, 1));
            await fixture.GivenOfferSyncJobHasRun();

            var response = await fixture.WhenISendTheQuery<GetOffersQuery, GetOffersResponse>(
                new GetOffersQuery
                {
                    ClientId = (int)Client.Cashrewards
                });

            response!.Data.Length.Should().Be(5);
            response!.Data[0].Title.Should().Be("Offer 1");
            response!.Data[1].Title.Should().Be("Offer 2");
            response!.Data[2].Title.Should().Be("Offer 5");
            response!.Data[3].Title.Should().Be("Offer 4");
            response!.Data[4].Title.Should().Be("Offer 3");
        }

        [Test]
        public async Task GetOffersQuery_ShouldOrderTopTwoOffersBasedOnFavouritesAndCashbackAmount_GivenUserIsEnroledInExperiment1Feature()
        {
            var fixture = new Fixture();
            fixture.GivenUnleashFeatureToggle(toggleName: FeatureToggle.Exp1);
            fixture.GivenUnleashFeatureToggle(toggleName: FeatureToggle.EnrolExp1);
            fixture.GivenOffer(title: "Offer 1", ranking: 10, merchantId: 1, offerString: "up to 20%");
            fixture.GivenOffer(title: "Offer 2", ranking: 9, merchantId: 2, offerString: "up to $25");
            fixture.GivenOffer(title: "Offer 3", ranking: 8, merchantId: 3, offerString: "10%");
            fixture.GivenOffer(title: "Offer 4", ranking: 7, merchantId: 4, offerString: "12%");
            fixture.GivenOffer(title: "Offer 5", ranking: 6, merchantId: 5, offerString: "14%");
            fixture.GivenOffer(title: "Offer 6", ranking: 5, merchantId: 6, offerString: "up to 25%");
            fixture.GivenOffer(title: "Offer 7", ranking: 4, merchantId: 7, offerString: "$50");
            fixture.GivenOffer(title: "Offer 8", ranking: 3, merchantId: 8, offerString: "50%");
            fixture.GivenOffer(title: "Offer 9", ranking: 2, merchantId: 9, offerString: "up to 50%");
            fixture.GivenOffer(title: "Offer 10", ranking: 1, merchantId: 10, offerString: "up to $50");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 3, hyphenatedString: "merch-3");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId:  4, hyphenatedString: "merch-4");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId:  5, hyphenatedString: "merch-5"); 
            fixture.GivenUserFavourite(cognitoId: "100", merchantId:  6, hyphenatedString: "merch-6"); 
            fixture.GivenUserFavourite(cognitoId: "100", merchantId:  7, hyphenatedString: "merch-7");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId:  8, hyphenatedString: "merch-8");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId:  9, hyphenatedString: "merch-9");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 10, hyphenatedString: "merch-10");
            await fixture.GivenOfferSyncJobHasRun();

            await fixture.WhenISendTheQuery<EnrolFeatureForMembersQuery, EnrolFeatureForMembersResponse>(
                new EnrolFeatureForMembersQuery
                {
                    IdType = MemberIdType.CognitoId.ToString(),
                    Feature = FeatureToggle.Exp1,
                    Ids = new[] { "100" }
                });

            fixture.GivenPerson(cognitoId: "100");
            var response = await fixture.WhenISendTheQuery<GetOffersQuery, GetOffersResponse>(
                new GetOffersQuery
                {
                    ClientId = (int)Client.Cashrewards
                });

            response!.Data.Length.Should().Be(10);
            response!.Data[0].Title.Should().Be("Offer 8");
            response!.Data[1].Title.Should().Be("Offer 9");
            response!.Data[2].Title.Should().Be("Offer 1");
            response!.Data[3].Title.Should().Be("Offer 2");
            response!.Data[4].Title.Should().Be("Offer 3");
            response!.Data[5].Title.Should().Be("Offer 4");
            response!.Data[6].Title.Should().Be("Offer 5");
            response!.Data[7].Title.Should().Be("Offer 6");
            response!.Data[8].Title.Should().Be("Offer 7");
            response!.Data[9].Title.Should().Be("Offer 10");
        }

        [Test]
        public async Task GetOffersQuery_ShouldOrderTopTwoOffersBasedOnFavouritesAndOfferEndingDate_GivenUserIsEnroledInExperiment2Feature()
        {
            var fixture = new Fixture();
            fixture.GivenUnleashFeatureToggle(toggleName: FeatureToggle.Exp2);
            fixture.GivenUnleashFeatureToggle(toggleName: FeatureToggle.EnrolExp2);
            fixture.GivenPerson(cognitoId: "100");
            fixture.GivenOffer(title: "Offer 1", ranking: 9, merchantId: 1, dateEnd: new DateTime(2022, 1, 7));
            fixture.GivenOffer(title: "Offer 2", ranking: 8, merchantId: 2, dateEnd: new DateTime(2022, 1, 6));
            fixture.GivenOffer(title: "Offer 3", ranking: 7, merchantId: 3, dateEnd: new DateTime(2022, 1, 5));
            fixture.GivenOffer(title: "Offer 4", ranking: 6, merchantId: 4, dateEnd: new DateTime(2022, 1, 4));
            fixture.GivenOffer(title: "Offer 5", ranking: 5, merchantId: 5, dateEnd: new DateTime(2022, 1, 3));
            fixture.GivenOffer(title: "Offer 6", ranking: 4, merchantId: 6, dateEnd: new DateTime(2022, 1, 2));
            fixture.GivenOffer(title: "Offer 7", ranking: 3, merchantId: 7, dateEnd: new DateTime(2022, 1, 1));
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 3, hyphenatedString: "merch-3");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 4, hyphenatedString: "merch-4");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 5, hyphenatedString: "merch-5");
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 6, hyphenatedString: "merch-6");
            await fixture.GivenOfferSyncJobHasRun();

            await fixture.WhenISendTheQuery<EnrolFeatureForMembersQuery, EnrolFeatureForMembersResponse>(
                new EnrolFeatureForMembersQuery
                {
                    IdType = MemberIdType.CognitoId.ToString(),
                    Feature = FeatureToggle.Exp2,
                    Ids = new[] { "100" }
                });

            fixture.GivenPerson(cognitoId: "100");
            var response = await fixture.WhenISendTheQuery<GetOffersQuery, GetOffersResponse>(
                new GetOffersQuery
                {
                    ClientId = (int)Client.Cashrewards
                });

            response!.Data.Length.Should().Be(7);
            response!.Data[0].Title.Should().Be("Offer 6");
            response!.Data[1].Title.Should().Be("Offer 5");
            response!.Data[2].Title.Should().Be("Offer 1");
            response!.Data[3].Title.Should().Be("Offer 2");
            response!.Data[4].Title.Should().Be("Offer 3");
            response!.Data[5].Title.Should().Be("Offer 4");
            response!.Data[6].Title.Should().Be("Offer 7");
        }

        [Test]
        public async Task GetEdmOffersQuery_ShouldOrderTopTwoOffersBasedOnFavouritesSelectionOrder_GivenUserIsEnroledInExperiment3Feature()
        {
            var fixture = new Fixture();
            fixture.GivenUnleashFeatureToggle(toggleName: FeatureToggle.Exp3);
            fixture.GivenUnleashFeatureToggle(toggleName: FeatureToggle.EnrolExp3);
            fixture.GivenOffer(title: "Offer 1", ranking: 9, merchantId: 1);
            fixture.GivenOffer(title: "Offer 2", ranking: 8, merchantId: 2);
            fixture.GivenOffer(title: "Offer 3", ranking: 7, merchantId: 3);
            fixture.GivenOffer(title: "Offer 4", ranking: 6, merchantId: 4);
            fixture.GivenOffer(title: "Offer 5", ranking: 5, merchantId: 5);
            fixture.GivenOffer(title: "Offer 6", ranking: 4, merchantId: 6);
            fixture.GivenOffer(title: "Offer 7", ranking: 3, merchantId: 7);
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 3, hyphenatedString: "merch-3", selectionOrder: 3);
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 4, hyphenatedString: "merch-4", selectionOrder: 0);
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 5, hyphenatedString: "merch-5", selectionOrder: 2);
            fixture.GivenUserFavourite(cognitoId: "100", merchantId: 6, hyphenatedString: "merch-6", selectionOrder: 1);
            await fixture.GivenOfferSyncJobHasRun();

            await fixture.WhenISendTheQuery<EnrolFeatureForMembersQuery, EnrolFeatureForMembersResponse>(
                new EnrolFeatureForMembersQuery
                {
                    IdType = MemberIdType.CognitoId.ToString(),
                    Feature = FeatureToggle.Exp3,
                    Ids = new[] { "100" }
                });

            fixture.GivenPerson(cognitoId: "100");
            var response = await fixture.WhenISendTheQuery<GetOffersQuery, GetOffersResponse>(
                new GetOffersQuery
                {
                    ClientId = (int)Client.Cashrewards
                });

            response!.Data[0].Title.Should().Be("Offer 4");
            response!.Data[1].Title.Should().Be("Offer 6");
            response!.Data[2].Title.Should().Be("Offer 1");
            response!.Data[3].Title.Should().Be("Offer 2");
            response!.Data[4].Title.Should().Be("Offer 3");
            response!.Data[5].Title.Should().Be("Offer 5");
            response!.Data[6].Title.Should().Be("Offer 7");
        }

        [Test]
        public async Task GetOffersQuery_ShouldExcludePausedMerchants_WhenPauseMerchantIsEnabled()
        {
            var fixture = new Fixture();
            fixture.GivenOffer(title: "Offer 1", ranking: 1);
            fixture.GivenOffer(title: "Offer 2", ranking: 2);
            fixture.GivenOffer(title: "Offer 3", ranking: 3, isMerchantPaused: true);
            await fixture.GivenOfferSyncJobHasRun();

            fixture.GivenUnleashFeatureToggle(FeatureToggle.MerchantPause, true);

            var response = await fixture.WhenISendTheQuery<GetOffersQuery, GetOffersResponse>(
                new GetOffersQuery
                {
                    ClientId = (int)Client.Cashrewards
                });

            response!.Data.Length.Should().Be(2);
            response!.Data[0].Title.Should().Be("Offer 2");
            response!.Data[1].Title.Should().Be("Offer 1");
        }
    }
}
