
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Skype.Internal.Media.H264;
using RickrollBot.Services.Bot;
using RickrollBot.Services.Contract;
using RickrollBot.Services.Http;
using RickrollBot.Services.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RickrollBot.Services.ServiceSetup
{
    /// <summary>
    /// Class AppHost.
    /// </summary>
    public class AppHost
    {
        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <value>The service provider.</value>
        private IServiceProvider ServiceProvider { get; set; }
        /// <summary>
        /// Gets or sets the service collection.
        /// </summary>
        /// <value>The service collection.</value>
        private IServiceCollection ServiceCollection { get; set; }
        /// <summary>
        /// Gets the application host instance.
        /// </summary>
        /// <value>The application host instance.</value>
        public static AppHost AppHostInstance { get; private set; }

        /// <summary>
        /// The call HTTP server
        /// </summary>
        private WebApplication _callHttpServer;

        /// <summary>
        /// The settings
        /// </summary>
        private IAzureSettings _settings;
        /// <summary>
        /// The bot service
        /// </summary>
        private IBotService _botService;
        /// <summary>
        /// The logger
        /// </summary>
        private IGraphLogger _graphLogger;

        private TelemetryClient _appInsights;
        private IDisposable _logSub;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppHost" /> class.

        /// </summary>
        public AppHost()
        {
            AppHostInstance = this;
        }

        /// <summary>
        /// Boots this instance.
        /// </summary>
        public void Boot()
        {
            DotNetEnv.Env.Load();


            var builder = new ConfigurationBuilder();

            // tell the builder to look for the appsettings.json file
            builder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            ServiceCollection = new ServiceCollection();
            ServiceCollection.AddCoreServices(configuration);

            ServiceProvider = ServiceCollection.BuildServiceProvider();

            // Add logging to Application Insights
            _appInsights = ServiceProvider.GetService<TelemetryClient>();

            _graphLogger = Resolve<IGraphLogger>();


            if (_appInsights != null)
            {
                var logger = new AppInsightsGraphLogger(_appInsights);
                _logSub = this._graphLogger.Subscribe(logger);
            }

            _settings = Resolve<IOptions<AzureSettings>>().Value;
            _settings.Initialize();


            _botService = Resolve<IBotService>();
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void StartServer()
        {
            try
            {
                var settings = (AzureSettings)_settings;

                var rootPath = Path.Combine(Environment.CurrentDirectory, "wwwroot");
                settings.BaseContentDir = rootPath;

                // Create ASP.NET Core web application
                var builder = WebApplication.CreateBuilder();

                // Add services to the container – include the Bot.Services assembly
                // so controllers in that project are discovered (the entry assembly
                // is Bot.Console, which contains no controllers).
                builder.Services.AddControllers()
                    .AddApplicationPart(typeof(Http.Controllers.JoinCallController).Assembly);
                builder.Services.AddCors();

                // Copy services from the existing service collection
                foreach (var service in ServiceCollection)
                {
                    builder.Services.Add(service);
                }

                // Override IBotService with the already-initialized instance so
                // controllers receive the same BotService that had Initialize()
                // called on it (the copied descriptor would create a new,
                // un-initialized instance).
                builder.Services.AddSingleton(_botService);

                // Use HTTP.sys (Windows only) - supports path-based URL prefixes and SSL via netsh
                builder.WebHost.UseHttpSys(options =>
                {
                    foreach (var url in settings.CallControlListeningUrls)
                    {
                        options.UrlPrefixes.Add(url);
                        _graphLogger.Info($"Listening on: {url}");
                    }
                });

                _callHttpServer = builder.Build();

                // Wire media platform logs into the ASP.NET Core logging
                // pipeline so they appear in the console alongside all other
                // application log messages.
                var mediaLogger = _callHttpServer.Services
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Microsoft.Skype.Bots.Media");
                settings.MediaPlatformSettings.MediaPlatformLogger =
                    new Util.MediaPlatformLogger(mediaLogger);

                // Initialize the bot service (and media platform) now that
                // the media platform logger is wired up.
                _botService.Initialize();

                // Configure the HTTP request pipeline
                var startup = new HttpConfigurationInitializer();
                startup.ConfigureSettings(_callHttpServer, _graphLogger, settings);

                _graphLogger.Info($"Root HTTP dir is {settings.BaseContentDir}");

                // Start the server asynchronously
                _ = _callHttpServer.RunAsync();
            }
            catch (Exception ex)
            {
                _graphLogger.Error(ex, $"Unhandled exception in {nameof(StartServer)}");
                throw;
            }
        }

        /// <summary>
        /// Resolves this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T Resolve<T>()
        {
            return ServiceProvider.GetService<T>();
        }
    }
}
