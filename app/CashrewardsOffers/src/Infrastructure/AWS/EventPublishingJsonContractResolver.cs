using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace CashrewardsOffers.Infrastructure.AWS
{
    public class EventPublishingJsonContractResolver : DefaultContractResolver
    {
        private readonly HashSet<string> propNamesToIgnore;

        public EventPublishingJsonContractResolver(IEnumerable<string> propNamesToIgnore)
        {
            this.propNamesToIgnore = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (propNamesToIgnore.Contains(property.PropertyName))
            {
                property.ShouldSerialize = (x) => { return false; };
            }

            return property;
        }
    }
}