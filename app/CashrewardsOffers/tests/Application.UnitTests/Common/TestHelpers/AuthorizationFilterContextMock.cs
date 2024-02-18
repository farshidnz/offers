using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CashrewardsOffers.Application.UnitTests.Common.TestHelpers
{
    public class AuthorizationFilterContextMock
    {
        public AuthorizationFilterContext Object { get; set; }
        public string Result => JsonConvert.SerializeObject(Object.Result);

        private Mock<IServiceProvider> _serviceProvider = new();

        public AuthorizationFilterContextMock()
        {
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(a => a.RequestServices).Returns(_serviceProvider.Object);
            var actionContext = new ActionContext(httpContextMock.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            Object = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata> { });
        }

        public void AddRequestService<T>(Mock<T> serviceMock) where T : class
        {
            _serviceProvider.Setup(s => s.GetService(typeof(T))).Returns(serviceMock.Object);
        }
    }
}
