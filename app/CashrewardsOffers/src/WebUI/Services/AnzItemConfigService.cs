using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.API.Services
{
    public static class AnzItemConfigService
    {
        public static void Setup(string environment = null, string anzquery = null)
        {
            var ProdEnvValues = new List<string>() { "", "prod", "live", "production", null };
            var isProdEnv = (string env) => ProdEnvValues.Contains(environment?.Trim()?.ToLower());
            var host = isProdEnv(environment) 
            ? "https://www.cashrewards.com.au/" 
            : $"https://www.{environment?.ToLower()}.aws.cashrewards.com.au/";

            var anzHostUriBuilder = new UriBuilder(new Uri(host));

            var EmptyAnzQueryValues = new List<string>() { null, "" };
            var isEmptyAnzQuery = (string query) => EmptyAnzQueryValues.Contains(query?.Trim());
            if (!isEmptyAnzQuery(anzquery))
                anzHostUriBuilder.Query = anzquery.Trim();
            
            Domain.Entities.AnzItem.HostUri = anzHostUriBuilder.Uri;
        }
    }
}
