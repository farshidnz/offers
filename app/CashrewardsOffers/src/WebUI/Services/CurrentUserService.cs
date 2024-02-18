using CashrewardsOffers.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CashrewardsOffers.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId => PersonCognitoId;

        public string PersonCognitoId => _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;
    }
}
