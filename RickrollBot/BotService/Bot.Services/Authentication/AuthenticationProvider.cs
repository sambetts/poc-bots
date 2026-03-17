// ***********************************************************************
// Assembly         : RickrollBot.Services
// 
// Created          : 09-07-2020
//

// Last Modified On : 08-17-2020
// ***********************************************************************
// <copyright file="AuthenticationProvider.cs" company="Microsoft">
//     Copyright ©  2020
// </copyright>
// <summary></summary>
// ***********************************************************************>

// THIS CODE HAS NOT BEEN TESTED RIGOROUSLY.USING THIS CODE IN PRODUCTION ENVIRONMENT IS STRICTLY NOT RECOMMENDED.
// THIS SAMPLE IS PURELY FOR DEMONSTRATION PURPOSES ONLY.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND.

using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using RickrollBot.Model.Constants;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;

namespace RickrollBot.Services.Authentication
{
    /// <summary>
    /// The authentication provider for this bot instance.
    /// </summary>
    /// <seealso cref="IRequestAuthenticationProvider" />
    public class AuthenticationProvider : ObjectRoot, IRequestAuthenticationProvider
    {
        /// <summary>
        /// The application name.
        /// </summary>
        private readonly string appName;

        /// <summary>
        /// The application identifier.
        /// </summary>
        private readonly string appId;

        /// <summary>
        /// The application secret.
        /// </summary>
        private readonly string appSecret;

        /// <summary>
        /// The MSAL confidential client application.
        /// </summary>
        private readonly IConfidentialClientApplication msalClient;

        /// <summary>
        /// The open ID configuration refresh interval.
        /// </summary>
        private readonly TimeSpan openIdConfigRefreshInterval = TimeSpan.FromHours(2);

        /// <summary>
        /// The previous update timestamp for OpenIdConfig.
        /// </summary>
        private DateTime prevOpenIdConfigUpdateTimestamp = DateTime.MinValue;

        /// <summary>
        /// The open identifier configuration.
        /// </summary>
        private OpenIdConnectConfiguration openIdConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationProvider" /> class.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="appId">The application identifier.</param>
        /// <param name="appSecret">The application secret.</param>
        /// <param name="tenantId">The AAD tenant identifier.</param>
        /// <param name="logger">The logger.</param>
        public AuthenticationProvider(string appName, string appId, string appSecret, string tenantId, IGraphLogger logger)
            : base(logger.NotNull(nameof(logger)).CreateShim(nameof(AuthenticationProvider)))
        {
            this.appName = appName.NotNullOrWhitespace(nameof(appName));
            this.appId = appId.NotNullOrWhitespace(nameof(appId));
            this.appSecret = appSecret.NotNullOrWhitespace(nameof(appSecret));

            // Client credentials flow requires a tenant-specific authority;
            // the /common endpoint does not support app-only tokens.
            var authority = string.IsNullOrWhiteSpace(tenantId)
                ? "https://login.microsoftonline.com/common"
                : $"https://login.microsoftonline.com/{tenantId}";

            // Initialize MSAL confidential client
            this.msalClient = ConfidentialClientApplicationBuilder
                .Create(this.appId)
                .WithClientSecret(this.appSecret)
                .WithAuthority(new Uri(authority))
                .Build();
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// This method will be called any time there is an outbound request.
        /// In this case we are using the Microsoft.Identity.Client library (MSAL)
        /// to stamp the outbound http request with the OAuth 2.0 token using an AAD application id
        /// and application secret.  Alternatively, this method can support certificate validation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="tenant">The tenant.</param>
        /// <returns>The <see cref="Task" />.</returns>
        public async Task AuthenticateOutboundRequestAsync(HttpRequestMessage request, string tenant)
        {
            const string schema = "Bearer";

            // Define scopes for Microsoft Graph
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            this.GraphLogger.Info("AuthenticationProvider: Generating OAuth token using MSAL.");

            AuthenticationResult result;
            try
            {
                result = await this.AcquireTokenWithRetryAsync(scopes, attempts: 3).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.GraphLogger.Error(ex, $"Failed to generate token for client: {this.appId}");
                throw;
            }

            this.GraphLogger.Info($"AuthenticationProvider: Generated OAuth token. Expires in {result.ExpiresOn.Subtract(DateTimeOffset.UtcNow).TotalMinutes} minutes.");

            request.Headers.Authorization = new AuthenticationHeaderValue(schema, result.AccessToken);
        }

        /// <summary>
        /// Validates the request asynchronously.
        /// This method will be called any time we have an incoming request.
        /// Returning invalid result will trigger a Forbidden response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="RequestValidationResult" /> structure.</returns>
        public async Task<RequestValidationResult> ValidateInboundRequestAsync(HttpRequestMessage request)
        {
            var token = request?.Headers?.Authorization?.Parameter;
            if (string.IsNullOrWhiteSpace(token))
            {
                return new RequestValidationResult { IsValid = false };
            }

            // Currently the service does not sign outbound request using AAD, instead it is signed
            // with a private certificate.  In order for us to be able to ensure the certificate is
            // valid we need to download the corresponding public keys from a trusted source.
            const string authDomain = AzureConstants.AuthDomain;
            if (this.openIdConfiguration == null || DateTime.Now > this.prevOpenIdConfigUpdateTimestamp.Add(this.openIdConfigRefreshInterval))
            {
                this.GraphLogger.Info("Updating OpenID configuration");

                // Download the OIDC configuration which contains the JWKS
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
                    new ConfigurationManager<OpenIdConnectConfiguration>(
                        authDomain,
                        new OpenIdConnectConfigurationRetriever());
                this.openIdConfiguration = await configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);

                this.prevOpenIdConfigUpdateTimestamp = DateTime.Now;
            }

            // The incoming token should be issued by graph.
            var authIssuers = new[]
            {
                "https://graph.microsoft.com",
                "https://api.botframework.com",
            };

            // Configure the TokenValidationParameters.
            // Set the Issuer(s) and Audience(s) to validate and
            // assign the SigningKeys which were downloaded from AuthDomain.
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuers = authIssuers,
                ValidAudience = this.appId,
                IssuerSigningKeys = this.openIdConfiguration.SigningKeys,
            };

            ClaimsPrincipal claimsPrincipal;
            try
            {
                // Now validate the token using JsonWebTokenHandler for .NET 10
                var tokenHandler = new JsonWebTokenHandler();
                var validationResult = await tokenHandler.ValidateTokenAsync(token, validationParameters).ConfigureAwait(false);
                if (!validationResult.IsValid)
                {
                    this.GraphLogger.Error(validationResult.Exception, $"Token validation failed for client: {this.appId}.");
                    return new RequestValidationResult() { IsValid = false };
                }
                claimsPrincipal = new ClaimsPrincipal(validationResult.ClaimsIdentity);
            }

            // Token expired... should somehow return 401 (Unauthorized)
            // catch (SecurityTokenExpiredException ex)
            // Tampered token
            // catch (SecurityTokenInvalidSignatureException ex)
            // Some other validation error
            // catch (SecurityTokenValidationException ex)
            catch (Exception ex)
            {
                // Some other error
                this.GraphLogger.Error(ex, $"Failed to validate token for client: {this.appId}.");
                return new RequestValidationResult() { IsValid = false };
            }

            const string ClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
            var tenantClaim = claimsPrincipal.FindFirst(claim => claim.Type.Equals(ClaimType, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(tenantClaim?.Value))
            {
                // No tenant claim given to us.  reject the request.
                return new RequestValidationResult { IsValid = false };
            }

            request.Options.Set(new HttpRequestOptionsKey<string>(HttpConstants.HeaderNames.Tenant), tenantClaim.Value);
            return new RequestValidationResult { IsValid = true, TenantId = tenantClaim.Value };
        }

        /// <summary>
        /// Acquires the token and retries if failure occurs using MSAL.
        /// </summary>
        /// <param name="scopes">The scopes for the token.</param>
        /// <param name="attempts">The attempts.</param>
        /// <returns>The <see cref="AuthenticationResult" />.</returns>
        private async Task<AuthenticationResult> AcquireTokenWithRetryAsync(string[] scopes, int attempts)
        {
            while (true)
            {
                attempts--;

                try
                {
                    return await this.msalClient.AcquireTokenForClient(scopes).ExecuteAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    if (attempts < 1)
                    {
                        throw;
                    }
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}
