using CashrewardsOffers.API.Services;
using CashrewardsOffers.Domain.Attributes;
using CashrewardsOffers.Domain.Common;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Events;
using CashrewardsOffers.Domain.UnitTests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Domain.UnitTests.Entities
{
    public class AnzItemTests
    {
        public static OfferEventBase MakeOfferEventBase(string merchantHyphenatedString, string offerHyphenatedString)
        {
            return new OfferEventBase()
            {
                Merchant = new OfferMerchantChanged()
                {
                    HyphenatedString = merchantHyphenatedString
                },
                HyphenatedString = offerHyphenatedString
            };
        }

        public static MerchantEventBase MakeMerchantEventBase(string merchantHyphenatedString)
        {
            return new MerchantEventBase()
            {
                HyphenatedString = merchantHyphenatedString
            };
        }


        [Test]
        public void AnzItemConfig_Works()
        {
            AnzItemConfigService.Setup("prod");
            AnzItem.HostUri.Should().NotBeNull();
        }

        [Test]
        public void NoAnzItemConfig_Offer()
        {
            AnzItemConfigService.Setup("prod");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            OfferEventBase offerChangeEvent = MakeOfferEventBase("merchant-string", "offer-string");
            anzItem.ApplyChanges(offerChangeEvent);
            anzItem.Offer.Link.Should().Be($"https://www.cashrewards.com.au/merchant-string?coupon=offer-string");
        }

        [Test]
        public void NoAnzItemConfig_Merchant()
        {
            AnzItemConfigService.Setup("prod");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            MerchantEventBase merchantChangeEvent = MakeMerchantEventBase("merchant-string");
            anzItem.ApplyChanges(merchantChangeEvent);
            anzItem.Merchant.Link.Should().Be($"https://www.cashrewards.com.au/merchant-string");
        }


        [Test]
        public void BaseAnzItemConfig_Offer()
        {
            AnzItemConfigService.Setup("prod");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            OfferEventBase offerChangeEvent = MakeOfferEventBase("merchant-string", "offer-string");
            anzItem.ApplyChanges(offerChangeEvent);
            anzItem.Offer.Link.Should().Be($"https://www.cashrewards.com.au/merchant-string?coupon=offer-string");
        }

        [Test]
        public void BaseAnzItemConfig_Merchant()
        {
            AnzItemConfigService.Setup("prod");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            MerchantEventBase merchantChangeEvent = MakeMerchantEventBase("merchant-string");
            anzItem.ApplyChanges(merchantChangeEvent);
            anzItem.Merchant.Link.Should().Be($"https://www.cashrewards.com.au/merchant-string");
        }

        [Test]
        public void ProdAnzItemConfigs_Offer()
        {
            var prodEnvs = new List<string>() { "", "prod", "live", "production" };

            foreach(var env in prodEnvs)
            {
                AnzItemConfigService.Setup(env);
                AnzItem anzItem = new AnzItemFactory().Create(100);
                OfferEventBase offerChangeEvent = MakeOfferEventBase("merchant-string", "offer-string");
                anzItem.ApplyChanges(offerChangeEvent);
                anzItem.Offer.Link.Should().Be($"https://www.cashrewards.com.au/merchant-string?coupon=offer-string");
            }
        }

        [Test]
        public void ProdAnzItemConfigs_Merchant()
        {
            var prodEnvs = new List<string>() { "", "prod", "live", "production" };

            foreach (var env in prodEnvs)
            {
                AnzItemConfigService.Setup(env);
                AnzItem anzItem = new AnzItemFactory().Create(100);
                MerchantEventBase merchantChangeEvent = MakeMerchantEventBase("merchant-string");
                anzItem.ApplyChanges(merchantChangeEvent);
                anzItem.Merchant.Link.Should().Be($"https://www.cashrewards.com.au/merchant-string");
            }
        }

        [Test]
        public void NoAnzTokenConfigs()
        {
            var anzTokens = new List<string>() { "", "   ", "\t\t\t\n\r" };

            foreach (var token in anzTokens)
            {
                AnzItemConfigService.Setup("prod", token);
                AnzItem anzItem = new AnzItemFactory().Create(100);
                OfferEventBase offerChangeEvent = MakeOfferEventBase("merchant-string", "offer-string");
                anzItem.ApplyChanges(offerChangeEvent);
                anzItem.Offer.Link.Should().Be($"https://www.cashrewards.com.au/merchant-string?coupon=offer-string");
            }
        }

        [Test]
        public void CrPersonAnzItemConfig_Offer()
        {
            AnzItemConfigService.Setup("crperson");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            OfferEventBase offerChangeEvent = MakeOfferEventBase("merchant-string", "offer-string");
            anzItem.ApplyChanges(offerChangeEvent);
            anzItem.Offer.Link.Should().Be($"https://www.crperson.aws.cashrewards.com.au/merchant-string?coupon=offer-string");
        }

        [Test]
        public void CrPersonAnzItemConfig_Merchant()
        {
            AnzItemConfigService.Setup("crperson");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            MerchantEventBase merchantChangeEvent = MakeMerchantEventBase("merchant-string");
            anzItem.ApplyChanges(merchantChangeEvent);
            anzItem.Merchant.Link.Should().Be($"https://www.crperson.aws.cashrewards.com.au/merchant-string");
        }

        [Test]
        public void CrPersonAnzItemConfig_WithTestAnzToken_Offer()
        {
            AnzItemConfigService.Setup("crperson", "anzappref=true");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            OfferEventBase offerChangeEvent = MakeOfferEventBase("merchant-string", "offer-string");
            anzItem.ApplyChanges(offerChangeEvent);
            anzItem.Offer.Link.Should().BeOneOf($"https://www.crperson.aws.cashrewards.com.au/merchant-string?coupon=offer-string&anzappref=true",
                $"https://www.crperson.aws.cashrewards.com.au/merchant-string?anzappref=true&coupon=offer-string");
        }

        [Test]
        public void CrPersonAnzItemConfig_WithTestAnzToken_Merchant()
        {
            AnzItemConfigService.Setup("crperson", "anzappref=true");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            MerchantEventBase merchantChangeEvent = MakeMerchantEventBase("merchant-string");
            anzItem.ApplyChanges(merchantChangeEvent);
            anzItem.Merchant.Link.Should().Be($"https://www.crperson.aws.cashrewards.com.au/merchant-string?anzappref=true");
        }

        [Test]
        public void ProdAnzItemConfig_WithTestAnzToken()
        {
            AnzItemConfigService.Setup(null, "anzappref=true");
            AnzItem anzItem = new AnzItemFactory().Create(100);
            OfferEventBase offerChangeEvent = MakeOfferEventBase("merchant-string", "offer-string");
            anzItem.ApplyChanges(offerChangeEvent);
            anzItem.Offer.Link.Should().BeOneOf($"https://www.cashrewards.com.au/merchant-string?coupon=offer-string&anzappref=true",
                $"https://www.cashrewards.com.au/merchant-string?anzappref=true&coupon=offer-string");
        }
    }
}
