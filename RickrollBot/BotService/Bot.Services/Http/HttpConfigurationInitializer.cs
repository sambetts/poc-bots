
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Graph.Communications.Common.Telemetry;
using RickrollBot.Services.ServiceSetup;
using System.IO;

namespace RickrollBot.Services.Http
{
    /// <summary>
    /// Initialize the HTTP configuration for ASP.NET Core.
    /// </summary>
    public class HttpConfigurationInitializer
    {
        /// <summary>
        /// Configuration settings like Authentication, Routes for ASP.NET Core.
        /// </summary>
        /// <param name="app">Application builder to configure.</param>
        /// <param name="logger">Graph logger.</param>
        /// <param name="settings">Azure settings.</param>
        public void ConfigureSettings(IApplicationBuilder app, IGraphLogger logger, AzureSettings settings)
        {
            // Enable CORS
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // Make sure base dir exists
            Directory.CreateDirectory(settings.BaseContentDir);

            // Configure static file serving
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(settings.BaseContentDir),
                RequestPath = "",
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });

            // Enable default files (index.html)
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new PhysicalFileProvider(settings.BaseContentDir),
                DefaultFileNames = new[] { "index.html" }
            });

            // Enable routing
            app.UseRouting();

            // Map controllers
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
