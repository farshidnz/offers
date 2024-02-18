using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.IO;
using Serilog;

namespace CashrewardsOffers.Infrastructure.Persistence
{
    public class MerchantPreferenceS3Source : S3Source, IMerchantPreferenceS3Source
    {
        private string DateKey => $"{DateTime.Now.ToString("yyyyMM")}";

        public MerchantPreferenceS3Source(
            IConfiguration configuration,
            IAmazonS3ClientFactory amazonS3ClientFactory
            ) : base(amazonS3ClientFactory)
        {
            this.BucketName = configuration["MERCHANT_PREFERENCES_BUCKET"];
            this.StoragePath = configuration["MERCHANT_PREFERENCES_PATH"];
        }

        public async Task<IEnumerable<RankedMerchant>> DownloadLatestRankedMerchants()
        {
            var random = new Random();
            var files = (await this.GetFileKeys(DateKey)).ToList();
            Log.Information("Ranked merchant sync found {Count} ranked merchant files in S3", files.Count);

            var fileKey = files.Skip(random.Next(0, files.Count)).Take(1).First();

            var fileContent = await DownloadTextFileAsync(fileKey);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<RankedMerchant>>(fileContent);
        }

    }
}
