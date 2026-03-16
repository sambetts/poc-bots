// ***********************************************************************
// Assembly         : RickrollBot.Services
// 
// Created          : 09-07-2020
//

// Last Modified On : 08-17-2020
// ***********************************************************************
// <copyright file="ExceptionLogger.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>
// <summary>Defines the ExceptionLogger type.</summary>
// ***********************************************************************-

using Microsoft.Graph.Communications.Common.Telemetry;
using System.Threading;
using System.Threading.Tasks;

namespace RickrollBot.Services.Http
{
    /// <summary>
    /// The exception logger.
    /// NOTE: ASP.NET Core exception handling is done via middleware, not IExceptionLogger
    /// This class is kept for reference but no longer used
    /// </summary>
    public class ExceptionLogger
    {
        /// <summary>
        /// The logger
        /// </summary>
        private IGraphLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLogger" /> class.
        /// </summary>
        /// <param name="logger">Graph logger.</param>
        public ExceptionLogger(IGraphLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Logs exceptions
        /// </summary>
        public Task LogAsync(System.Exception exception, CancellationToken cancellationToken)
        {
            this.logger.Error(exception, "Exception processing HTTP request.");
            return Task.CompletedTask;
        }
    }
}
