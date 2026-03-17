using Microsoft.Extensions.Logging;
using Microsoft.Skype.Bots.Media;

using MediaLogLevel = Microsoft.Skype.Bots.Media.LogLevel;

namespace RickrollBot.Services.Util
{
    /// <summary>
    /// Bridges <see cref="IMediaPlatformLogger"/> to <see cref="ILogger"/>
    /// so that Microsoft.Skype.Bots.Media log output appears in the console
    /// alongside all other ASP.NET Core log messages.
    /// </summary>
    public class MediaPlatformLogger : IMediaPlatformLogger
    {
        private readonly ILogger _logger;

        public MediaPlatformLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void WriteLog(MediaLogLevel level, string logStatement)
        {
            var msLevel = level switch
            {
                MediaLogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                MediaLogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                MediaLogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
                MediaLogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Debug,
                _ => Microsoft.Extensions.Logging.LogLevel.Debug,
            };

            _logger.Log(msLevel, "{LogStatement}", logStatement);
        }
    }
}
