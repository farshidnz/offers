using CashrewardsOffers.Application.IntegrationTests.TestingHelpers;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.Enums;
using CashrewardsOffers.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.IntegrationTests.OffersApplication
{
    public class S3MerchantHistoryArchiveTests
    {
        private class TestState
        {
            public S3MerchantHistoryArchive S3MerchantHistoryArchive { get; }

            public TestState()
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.json", true)
                    .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.Development.json", true)
                    .Build();

                S3MerchantHistoryArchive = new S3MerchantHistoryArchive(config, new AmazonS3ClientFactory(config));
            }
        }

        private static List<MerchantHistory> Data => new()
        {
            new MerchantHistory { MerchantId = 1001, Name = "Merchant 1", HyphenatedString = "merchant-1", Client = Client.Cashrewards, ClientCommissionString = "3%", ChangeTime = DateTimeOffset.UtcNow },
            new MerchantHistory { MerchantId = 1002, Name = "Merchant 2", HyphenatedString = "merchant-2", Client = Client.Cashrewards, ClientCommissionString = "up to 10%", ChangeTime = DateTimeOffset.UtcNow },
            new MerchantHistory { MerchantId = 1003, Name = "Merchant 3", HyphenatedString = "merchant-3", Client = Client.Cashrewards, ClientCommissionString = "$100", ChangeTime = DateTimeOffset.UtcNow }
        };

        [Test]
        [Ignore("Dependent on AWS CLI credentials")]
        public async Task Put_ShouldWriteDataToS3Storage()
        {
            var state = new TestState();

            await state.S3MerchantHistoryArchive.Put(2022, 1, 2, ".json", JsonConvert.SerializeObject(Data));
        }
    }
}
