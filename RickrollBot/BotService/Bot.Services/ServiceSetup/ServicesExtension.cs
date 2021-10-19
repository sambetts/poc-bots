
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RickrollBot.Services.Contract;
using System;

namespace RickrollBot.Services.ServiceSetup
{
    /// <summary>
    /// Class ServicesExtension.
    /// </summary>
    public static class ServicesExtension
    {
        /// <summary>
        /// Adds the core services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            new ServiceHost().Configure(services, configuration);
        }

        /// <summary>
        /// Configures the configuration object.
        /// </summary>
        /// <typeparam name="TConfig">The type of the t configuration.</typeparam>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>TConfig.</returns>
        /// <exception cref="ArgumentNullException">services</exception>
        /// <exception cref="ArgumentNullException">configuration</exception>
        public static TConfig ConfigureConfigObject<TConfig>(this IServiceCollection services, IConfiguration configuration) where TConfig : class, new()
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var config = new TConfig();
            configuration.Bind(config);

            if (config is IInitializable init)
            {
                init.Initialize();
            }

            services.AddSingleton(config);
            return config;
        }


        /// <summary>
        /// Configures the configuration object.
        /// </summary>
        /// <typeparam name="TConfig">The type of the t configuration.</typeparam>
        /// <param name="services">The services.</param>
        /// <returns>TConfig.</returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public static TConfig ConfigureConfigObject<TConfig>(this IServiceCollection services) where TConfig : class, new()
        {

            if (services == null) throw new ArgumentNullException(nameof(services));

            var config = new TConfig();

            if (config is IInitializable init)
            {
                init.Initialize();
            }

            services.AddSingleton(config);
            return config;
        }

    }
}
