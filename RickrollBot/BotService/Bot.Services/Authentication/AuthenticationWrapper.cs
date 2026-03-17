// ***********************************************************************
// Assembly         : RickrollBot.Services
// 
// Created          : 09-07-2020
//

// Last Modified On : 08-17-2020
// ***********************************************************************
// <copyright file="AuthenticationWrapper.cs" company="Microsoft">
//     Copyright ©  2020
// </copyright>
// <summary></summary>
// ***********************************************************************>

using Microsoft.Graph;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RickrollBot.Services.Authentication
{
    /// <summary>
    /// A wrapper for the <see cref="IRequestAuthenticationProvider" />
    /// that maps to the <see cref="IAuthenticationProvider" />.
    /// </summary>
    /// <seealso cref="IRequestAuthenticationProvider" />
    /// <seealso cref="IAuthenticationProvider" />
    public class AuthenticationWrapper : IRequestAuthenticationProvider, IAuthenticationProvider
    {
        /// <summary>
        /// The authentication provider
        /// </summary>
        private readonly IRequestAuthenticationProvider authenticationProvider;
        /// <summary>
        /// The tenant
        /// </summary>
        private readonly string tenant;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationWrapper" /> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="tenant">The tenant.</param>
        public AuthenticationWrapper(IRequestAuthenticationProvider authenticationProvider, string tenant = null)
        {
            this.authenticationProvider = authenticationProvider.NotNull(nameof(authenticationProvider));
            this.tenant = tenant;
        }

        /// <inheritdoc />
        public Task AuthenticateOutboundRequestAsync(HttpRequestMessage request, string tenant)
        {
            return this.authenticationProvider.AuthenticateOutboundRequestAsync(request, tenant);
        }

        /// <inheritdoc />
        public Task<RequestValidationResult> ValidateInboundRequestAsync(HttpRequestMessage request)
        {
            return this.authenticationProvider.ValidateInboundRequestAsync(request);
        }

        /// <inheritdoc />
        public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            // Create an HttpRequestMessage and authenticate it, then transfer the auth header
            var httpRequest = new HttpRequestMessage();
            var authTask = this.AuthenticateOutboundRequestAsync(httpRequest, this.tenant);
            authTask.Wait(cancellationToken);

            if (httpRequest.Headers.Authorization != null)
            {
                request.Headers.Add("Authorization", $"{httpRequest.Headers.Authorization.Scheme} {httpRequest.Headers.Authorization.Parameter}");
            }

            return Task.CompletedTask;
        }
    }
}
