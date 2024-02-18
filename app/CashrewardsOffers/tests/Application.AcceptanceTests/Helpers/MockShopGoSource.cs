using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Offers.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using CashrewardsOffers.Application.EDM;
using CashrewardsOffers.Application.Merchants.Models;

namespace Application.AcceptanceTests.Helpers
{
    public class MockShopGoSource : Mock<IShopGoSource>
    {
        private class Person
        {
            public Person(string personId, string cognitoId, string memberId, string newMemberId, int premiumStatus)
            {
                PersonId = personId;
                CognitoId = cognitoId;
                MemberId = memberId;
                NewMemberId = newMemberId;
                PremiumStatus = premiumStatus;
            }

            public string PersonId { get; }
            public string CognitoId { get; }
            public string MemberId { get; }
            public string NewMemberId { get; }
            public int PremiumStatus { get; }
        }

        private List<Person> _people = new();

        public List<ShopGoOffer> Offers { get; } = new();

        public List<ShopGoMerchant> Merchants { get; } = new();

        public List<ShopGoTier> Tiers { get; } = new();

        public List<(string cognitoId, ShopGoFavourite favourite)> Favourites { get; } = new();

        public List<ShopGoEdmItem> EdmItems { get; } = new();

        public MockShopGoSource()
        {
            Setup(s => s.GetOffers())
                .ReturnsAsync(Offers);

            Setup(s => s.GetMerchants())
                .ReturnsAsync(Merchants);

            Setup(s => s.GetTiers())
                .ReturnsAsync(Tiers);

            Setup(s => s.GetFavourites(It.IsAny<string>()))
                .ReturnsAsync((string cognitoId) => Favourites
                    .Where(f => f.cognitoId == cognitoId)
                    .Select(f => f.favourite));


            Setup(s => s.GetFavouritesByNewMemberId(It.IsAny<string>()))
                .ReturnsAsync((string newMemberId) =>
                {
                    var cognitoId = _people.Single(p => p.NewMemberId == newMemberId).CognitoId;
                    return Favourites
                        .Where(f => f.cognitoId == cognitoId)
                        .Select(f => f.favourite);
                });

            Setup(s => s.LookupCognitoIdFromPersonId(It.IsAny<string>()))
                .ReturnsAsync((string personId) => _people
                    .Where(m => m.PersonId == personId)
                    .Select(m => m.CognitoId)
                    .Single());

            Setup(s => s.LookupCognitoIdFromMemberId(It.IsAny<string>()))
                .ReturnsAsync((string memberId) => _people
                    .Where(m => m.MemberId == memberId)
                    .Select(m => m.CognitoId)
                    .First());

            Setup(s => s.LookupCognitoIdFromNewMemberId(It.IsAny<string>()))
                .ReturnsAsync((string newMemberId) => _people
                    .Where(m => m.NewMemberId == newMemberId)
                    .Select(m => m.CognitoId)
                    .Single());
            

            Setup(s => s.GetPremiumStatus(It.IsAny<string>()))
                .ReturnsAsync((string newMemberId) => _people
                    .Where(m => m.NewMemberId == newMemberId)
                    .Select(m => m.PremiumStatus)
                    .Single());

            Setup(s => s.GetPersonFromNewMemberId(It.IsAny<string>()))
                .ReturnsAsync((string newMemberId) => _people
                    .Where(m => m.NewMemberId == newMemberId)
                    .Select(m => new ShopGoPerson { CognitoId = m.CognitoId, PremiumStatus = m.PremiumStatus })
                    .Single());

            Setup(s => s.GetEdmCampaignItems(It.IsAny<int>()))
                .ReturnsAsync((int edmCampaignId) => EdmItems
                    .Where(i => i.EDMCampaignId == edmCampaignId)
                    .Select(i => new ShopGoEdmItem()
                    {
                        EDMCampaignId = edmCampaignId, MerchantId = i.MerchantId, OfferId = i.OfferId, Type = i.Type
                    })
                );
        }

        public void GivenPerson(string personId, string cognitoId, string memberId, string newMemberId, int premiumStatus) =>
            _people.Add(new Person(personId, cognitoId, memberId, newMemberId, premiumStatus));

        public void GivenEdmItem(int edmCampaignId, string type, int merchantId)
        {
            var item = new ShopGoEdmItem();
            item.EDMCampaignId = edmCampaignId;
            item.Type = type;
            if (type == "M") { item.MerchantId = merchantId; }
            else if (type == "O") { item.OfferId = merchantId; }

            EdmItems.Add(item);
        }
    }
}
