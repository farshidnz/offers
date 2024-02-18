using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashrewardsOffers.Domain.Extensions
{
    public static class EnumerableExtension
    {
        public static Dictionary<TKey, TSource> ToDictionaryIgnoringDuplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var dictionary = new Dictionary<TKey, TSource>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, item);
                }
            }

            return dictionary;
        }
    }
}
