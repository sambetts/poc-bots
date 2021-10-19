
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using RickrollBot.Services.ServiceSetup;
using System.IO;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace RickrollBot.Services.Http
{
    /// <summary>
    /// Initialize the HttpConfiguration for OWIN.
    /// </summary>
    public class HttpConfigurationInitializer
    {
        /// <summary>
        /// Configuration settings like Authentication, Routes for OWIN.
        /// </summary>
        /// <param name="app">Builder to configure.</param>
        /// <param name="logger">Graph logger.</param>
        public void ConfigureSettings(IAppBuilder app, IGraphLogger logger, AzureSettings settings)
        {
            HttpConfiguration httpConfig = new HttpConfiguration();
            httpConfig.MapHttpAttributeRoutes();
            httpConfig.MessageHandlers.Add(new LoggingMessageHandler(isIncomingMessageHandler: true, logger: logger, urlIgnorers: new[] { "/logs" }));

            httpConfig.Services.Add(typeof(IExceptionLogger), new ExceptionLogger(logger));

            // TODO: Provide serializer settings hooks
            // httpConfig.Formatters.JsonFormatter.SerializerSettings = RealTimeMediaSerializer.GetSerializerSettings();
            httpConfig.EnsureInitialized();

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // Use the HTTP configuration initialized above
            app.UseWebApi(httpConfig);

            // Make sure base dir exists
            Directory.CreateDirectory(settings.BaseContentDir);
            var physicalFileSystem = new PhysicalFileSystem(settings.BaseContentDir);
            var options = new FileServerOptions
            {
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem
            };
            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new[]
            {
                "index.html"
            };

            app.UseFileServer(options);
        }
    }
}
