using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashrewardsOffers.Domain.Entities
{
    public class OfferEndingString
    {
        private readonly DateTimeOffset _offerEndingDate;
        private readonly DateTimeOffset _now;

        public OfferEndingString(DateTimeOffset offerEndingDate, DateTimeOffset now)
        {
            _offerEndingDate = offerEndingDate;
            _now = now;
        }

        public string EdmEndingString
        {
            get
            {
                var timeRemaining = _offerEndingDate - _now;
                if (timeRemaining <= TimeSpan.Zero)
                {
                    return "Ended";
                }

                if (timeRemaining > TimeSpan.FromDays(30))
                {
                    return "Ongoing";
                }

                if (timeRemaining > TimeSpan.FromHours(24))
                {
                    return $"{Math.Ceiling(timeRemaining.TotalDays)} days";
                }

                if (_offerEndingDate.Day != _now.Day)
                {
                    return "Tomorrow";
                }

                if (timeRemaining > TimeSpan.FromHours(8))
                {
                    return "Today";
                }

                return "Soon";
            }
        }
    }
}
