
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RickrollBot.Services.ServiceSetup;
using System;

namespace RickrollBot.Services.Contract
{
    /// <summary>
    /// Interface IServiceHost
    /// </summary>
    public interface IServiceHost
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        IServiceCollection Services { get; }
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>The service provider.</value>
        IServiceProvider ServiceProvider { get; }
        /// <summary>
        /// Configures the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>ServiceHost.</returns>
        ServiceHost Configure(IServiceCollection services, IConfiguration configuration);

    }
}
