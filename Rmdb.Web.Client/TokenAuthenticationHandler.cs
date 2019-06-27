using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Rmdb.Web.Client
{ 
    public class TokenAuthenticationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private string _accessToken = string.Empty;

        public TokenAuthenticationHandler(
            IHttpContextAccessor contextAccessor)
            : base()
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public TokenAuthenticationHandler(HttpMessageHandler innerHandler,          
            IHttpContextAccessor contextAccessor)
      : base(innerHandler)
        {     
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }
                     
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await RequestTokenAsync(request);

            // set bearer token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);             
            return await base.SendAsync(request, cancellationToken);
        }

        public virtual async Task RequestTokenAsync(HttpRequestMessage request)
        {
            _accessToken = await _contextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
        }
    }
}
