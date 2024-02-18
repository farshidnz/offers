using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashrewardsOffers.Application.Common.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value) where T : struct
        {
            return Enum.TryParse<T>(value, true, out T result) ? result : default;
        }
    }
}