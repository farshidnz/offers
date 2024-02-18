using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.MerchantSuggestions.Services;
using CashrewardsOffers.Application.Offers.Services;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CashrewardsOffers.Application.EDM;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Application.Merchants.Models;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class ShopGoSource : IShopGoSource
    {
        private readonly string _connectionString;

        public ShopGoSource(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ShopGoReadOnlyConnectionString");
        }

        public virtual IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ShopGoOffer>> GetOffers() =>
            await CreateConnection().QueryAsync<ShopGoOffer>(@"
                SELECT
                    o.OfferId,
                    o.ClientId,
                    o.MerchantId,
                    o.CouponCode,
                    o.OfferTitle,
                    o.OfferDescription,
                    o.HyphenatedString,
                    o.DateEnd,
                    o.MerchantName,
                    o.RegularImageUrl,
                    o.OfferCount,
                    o.ClientProgramTypeId,
                    o.TierCommTypeId,
                    o.TierTypeId,
                    o.Commission,
                    o.ClientComm,
                    o.MemberComm,
                    o.RewardName,
                    o.MerchantShortDescription,
                    o.MerchantHyphenatedString,
                    o.OfferTerms,
                    o.IsFlatRate,
                    o.Rate,
                    o.Ranking,
                    o.OfferBackgroundImageUrl,
                    o.OfferBadgeCode,
                    o.MerchantBadgeCode,
                    o.OfferPastRate,
                    o.IsFeatured,
                    o.IsCategoryFeatured,
                    o.IsCashbackIncreased,
                    o.IsPremiumFeature,
                    o.MerchantIsPremiumDisabled,
                    m.MobileEnabled,
                    mx.IsMobileAppEnabled,
                    o.NetworkId,
                    o.IsMerchantPaused,
                    m.BasicTerms,
                    m.ExtentedTerms,
                    m.IsPopular,
                    m.IsHomePageFeatured
                FROM OfferView o
                JOIN Merchant m ON m.MerchantId = o.MerchantId
                JOIN MerchantExtension mx ON mx.MerchantId = o.MerchantId
            ");

        public async Task<IEnumerable<ShopGoRankedMerchant>> GetRankedMerchants() =>
            await CreateConnection().QueryAsync<ShopGoRankedMerchant>(@"
                SELECT
                    ClientId,
                    MerchantId,
                    HyphenatedString,
                    RegularImageUrl,
                    IsPremiumDisabled
                FROM MerchantView");

        public async Task<IEnumerable<ShopGoMerchantBaseRate>> GetMerchantBaseRatesById(IEnumerable<int> merchantIds, int clientId) =>
            await CreateConnection().QueryAsync<ShopGoMerchantBaseRate>(@"
                SELECT
                    MerchantId,
                    TierCommTypeId,
                    TierTypeId,
                    Commission,
                    ClientComm,
                    MemberComm,
                    Rate,
                    ClientProgramTypeId,
                    IsFlatRate,
                    RewardName 
                FROM MerchantFullView
                WHERE MerchantId IN @MerchantIds AND ClientId = @ClientId",
                new
                {
                    MerchantIds = merchantIds,
                    ClientId = clientId
                });

        public async Task<IEnumerable<ShopGoFavourite>> GetFavourites(string cognitoId) =>
            await CreateConnection().QueryAsync<ShopGoFavourite>(@"
                SELECT
                    mf.MerchantId,mf.HyphenatedString,mf.SelectionOrder
                FROM MemberFavourite mf
                JOIN CognitoMember cm ON cm.MemberId = mf.MemberId
                WHERE cm.CognitoId=@CognitoId",
                new
                {
                    CognitoId = cognitoId
                });

        public async Task<IEnumerable<ShopGoFavourite>> GetFavouritesByNewMemberId(string newMemberId) =>
            await CreateConnection().QueryAsync<ShopGoFavourite>(@"
                SELECT
                    mf.MerchantId,mf.HyphenatedString,mf.SelectionOrder
                FROM MemberFavourite mf
                JOIN Member m ON m.MemberId = mf.MemberId
                WHERE m.MemberNewId=@NewMemberId",
                new
                {
                    NewMemberId = newMemberId
                });

        public async Task<IEnumerable<ShopGoCategory>> GetMerchantCategories() =>
            await CreateConnection().QueryAsync<ShopGoCategory>(@"
                SELECT MC.MerchantId, MC.CategoryId, C.Name
                FROM MerchantCategoryMap MC
                LEFT JOIN Category C ON C.CategoryId = MC.CategoryId");

        public async Task<IEnumerable<ShopGoMerchant>> GetMerchants() =>
            await CreateConnection().QueryAsync<ShopGoMerchant>(@"
                SELECT
                    ClientId,
                    MerchantId,
                    HyphenatedString,
                    RegularImageUrl,
                    ClientProgramTypeId,
                    TierCommTypeId,
                    TierTypeId,
                    Commission,
                    ClientComm,
                    MemberComm,
                    RewardName,
                    IsFlatRate,
                    Rate,
                    IsPremiumDisabled,
                    NetworkId,
                    IsFeatured,
                    IsHomePageFeatured,
                    IsPopular,
                    MobileEnabled,
                    IsMobileAppEnabled,
                    MerchantName,
                    BasicTerms,
                    ExtentedTerms,
                    IsPaused
                FROM MerchantFullView");

        public async Task<IEnumerable<ShopGoTier>> GetTiers() =>
            await CreateConnection().QueryAsync<ShopGoTier>(@$"
                SELECT
                    ClientTierId,
                    MerchantTierId,
                    MerchantId,
                    ClientId,
                    TierName,
                    TierCommTypeId,
                    Commission,
                    ClientComm,
                    MemberComm,
                    TierSpecialTerms
                FROM MerchantTierView
                WHERE TierTypeId != {(int)RewardType.Hidden}");

        public async Task<string> LookupCognitoIdFromPersonId(string personId)
        {
            var cognitoId = await CreateConnection().QueryFirstOrDefaultAsync<Guid?>(@"
                SELECT TOP 1 P.CognitoId
                FROM Person P
                WHERE P.PersonId = @PersonId", new { PersonId = personId });
            return cognitoId?.ToString();
        }

        public async Task<string> LookupCognitoIdFromMemberId(string memberId)
        {
            var cognitoId = await CreateConnection().QueryFirstOrDefaultAsync<Guid?>(@"
                SELECT TOP 1 P.CognitoId
                FROM Person P
                WHERE P.PersonId IS NOT NULL AND P.PersonId = (SELECT TOP 1 M.PersonId FROM Member M WHERE M.MemberId = @MemberId)", new { MemberId = memberId });
            return cognitoId?.ToString();
        }

        public async Task<string> LookupCognitoIdFromNewMemberId(string newMemberId)
        {
            var cognitoId = await CreateConnection().QueryFirstOrDefaultAsync<Guid?>(@"
                SELECT TOP 1 P.CognitoId
                FROM Person P
                WHERE P.PersonId IS NOT NULL AND P.PersonId = (SELECT TOP 1 M.PersonId FROM Member M WHERE M.MemberNewId = @NewMemberId)", new { NewMemberId = newMemberId });
            return cognitoId?.ToString();
        }

        public async Task<int> GetPremiumStatus(string newMemberId)
        {
            var premiumStatus = await CreateConnection().QueryFirstOrDefaultAsync<int?>(@"
                SELECT TOP 1 p.PremiumStatus
                FROM Person p
                WHERE p.PersonId IS NOT NULL AND p.PersonId = (SELECT TOP 1 m.PersonId FROM Member m WHERE m.MemberNewId = @NewMemberId)",
                new
                {
                    NewMemberId = newMemberId
                });

            return premiumStatus ?? 0;
        }

        public async Task<ShopGoPerson> GetPersonFromNewMemberId(string newMemberId) =>
            await CreateConnection().QueryFirstOrDefaultAsync<ShopGoPerson>(@"
                SELECT TOP 1 p.PremiumStatus, p.CognitoId
                FROM Person p
                WHERE p.PersonId IS NOT NULL AND p.PersonId = (SELECT TOP 1 m.PersonId FROM Member m WHERE m.MemberNewId = @NewMemberId)",
                new
                {
                    NewMemberId = newMemberId
                });

        public async Task<IEnumerable<ShopGoEdmItem>> GetEdmCampaignItems(int campaignId) =>
            await CreateConnection().QueryAsync<ShopGoEdmItem>(@"
                SELECT E.EDMItemId, E.EDMId, E.EDMCampaignId, E.Title, E.MerchantId, E.OfferId, E.Type
                FROM EdmItem E
                WHERE E.EDMCampaignId = @CampaignId",
                new
                {
                    CampaignId = campaignId
                });
    }
}
