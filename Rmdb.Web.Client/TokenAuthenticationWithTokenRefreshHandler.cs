using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Rmdb.Web.Client
{
    public class TokenAuthenticationWithTokenRefreshHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private string _accessToken = string.Empty;

        public TokenAuthenticationWithTokenRefreshHandler(
            IHttpClientFactory clientFactory,
            IHttpContextAccessor contextAccessor)
            : base()
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

        }

        public TokenAuthenticationWithTokenRefreshHandler(HttpMessageHandler innerHandler,            
               IHttpClientFactory clientFactory,
               IHttpContextAccessor contextAccessor)
                : base(innerHandler)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
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
            var expiresAt = await _contextAccessor.HttpContext.GetTokenAsync("expires_at");
            var expires = DateTime.Parse(expiresAt);

            if (expires.ToUniversalTime()> DateTime.UtcNow.AddSeconds(-60))
            {
                _accessToken = await _contextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
                return;
            }

            var client = _clientFactory.CreateClient("IdentityServerClient");

            var disco = await client.GetDiscoveryDocumentAsync();  
            if (disco.IsError) throw new Exception(disco.Error);

            var tokenResult = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "rmdbwebclient",
                ClientSecret = "2E51842C-56EF-481A-938C-A0C4BF648215",
                RefreshToken = await _contextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken)
            });

            if (!tokenResult.IsError)
            {
                // update the tokens & exipration value
                var updatedTokens = new List<AuthenticationToken>();
                updatedTokens.Add(new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = tokenResult.IdentityToken
                });
                updatedTokens.Add(new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = tokenResult.AccessToken
                });
                updatedTokens.Add(new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = tokenResult.RefreshToken
                });

                var updatedExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                updatedTokens.Add(new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = updatedExpiresAt.ToString("o", CultureInfo.InvariantCulture)
                });

                // get authenticate result, containing the current principal & 
                // properties
                var currentAuthenticateResult = await _contextAccessor.HttpContext.AuthenticateAsync("RMDBCookies");

                // store the updated tokens
                currentAuthenticateResult.Properties.StoreTokens(updatedTokens);

                // sign in
                await _contextAccessor.HttpContext.SignInAsync("RMDBCookies",
                    currentAuthenticateResult.Principal,
                    currentAuthenticateResult.Properties);

                // set new access token 
                _accessToken = tokenResult.AccessToken;
            }           
        }
    } 
}
