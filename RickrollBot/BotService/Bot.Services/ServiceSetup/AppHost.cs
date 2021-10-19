
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Owin.Hosting;
using Microsoft.Skype.Internal.Media.H264;
using RickrollBot.Services.Bot;
using RickrollBot.Services.Contract;
using RickrollBot.Services.Http;
using RickrollBot.Services.Util;
using System;
using System.Collections.Generic;
using System.IO;

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
        private IDisposable _callHttpServer;

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
            _appInsights = ServiceProvider.GetRequiredService<TelemetryClient>();

            _graphLogger = Resolve<IGraphLogger>();


            var logger = new AppInsightsGraphLogger(_appInsights);
            _logSub = this._graphLogger.Subscribe(logger);

            _settings = Resolve<IOptions<AzureSettings>>().Value;
            _settings.Initialize();


            using (var fileReader = new H264FileReader(
                                @"C:\Users\sambetts\Desktop\outfile.720.h264",
                                (uint)1440,
                                (uint)1080,
                                30))
            {
                var listOfFrames = new List<H264Frame>();
                var totalNumberOfFrames = fileReader.GetTotalNumberOfFrames();
                for (int i = 0; i < totalNumberOfFrames; i++)
                {
                    H264Frame frame = new H264Frame();
                    fileReader.GetNextFrame(frame);
                    listOfFrames.Add(frame);
                }
            }


            _botService = Resolve<IBotService>();
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void StartServer()
        {
            try
            {
                _botService.Initialize();

                var callStartOptions = new StartOptions();

                var settings = (AzureSettings)_settings;

                var rootPath = Path.Combine(Environment.CurrentDirectory, "wwwroot");
                settings.BaseContentDir = rootPath;




                foreach (var url in settings.CallControlListeningUrls)
                {
                    callStartOptions.Urls.Add(url);
                    _graphLogger.Info($"Listening on: {url}");
                }

                _callHttpServer = WebApp.Start(
                    callStartOptions,
                    (appBuilder) =>
                    {
                        var startup = new HttpConfigurationInitializer();
                        startup.ConfigureSettings(appBuilder, _graphLogger, settings);
                    });

                _graphLogger.Info($"Root HTTP dir is {settings.BaseContentDir}");
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
