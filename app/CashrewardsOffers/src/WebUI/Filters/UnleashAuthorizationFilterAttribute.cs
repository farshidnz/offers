using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace CashrewardsOffers.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UnleashAuthorizationFilterAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _featureToggleName;

        public UnleashAuthorizationFilterAttribute(string featureToggleName)
        {
            _featureToggleName = featureToggleName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var unleash = (IUnleashService)context.HttpContext.RequestServices.GetService(typeof(IUnleashService));
            var currentUserService = (ICurrentUserService)context.HttpContext.RequestServices.GetService(typeof(ICurrentUserService));

            if (!unleash.IsEnabled(_featureToggleName, currentUserService.UserId?.ToLower()) &&
                !unleash.IsEnabled(_featureToggleName, currentUserService.UserId?.ToUpper()))
            {
                context.Result = new JsonResult(new { Message = "Access Denied" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}
