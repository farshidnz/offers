using Microsoft.Extensions.Configuration;
using Unleash;

namespace CashrewardsOffers.Application.Feature.Services
{
    public interface IUnleashService
    {
        bool IsEnabled(string toggleName);
        public bool IsEnabled(string toggleName, string userId);
    }

    public class UnleashService : IUnleashService
    {
        private readonly IUnleash _unleash;
        private readonly string _environment;

        public UnleashService(
            IConfiguration configuration,
            IUnleash unleash)
        {
            _unleash = unleash;
            _environment = configuration["ENVIRONMENT"];
        }

        public bool IsEnabled(string toggleName) => _unleash.IsEnabled(toggleName, new UnleashContext { Environment = _environment });

        public bool IsEnabled(string toggleName, string userId) => _unleash.IsEnabled(toggleName, new UnleashContext { Environment = _environment, UserId = userId });
    }
}
