
using Microsoft.Graph.Communications.Common.Telemetry;
using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace RickrollBot.Services.Util
{
    public class AppInsightsGraphLogger : IObserver<LogEvent>
    {
        private readonly LogEventFormatter _formatter = new LogEventFormatter();
        private TelemetryClient _telemetryClient;

        public AppInsightsGraphLogger(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="logEvent">The current notification information.</param>
        public void OnNext(LogEvent logEvent)
        {
            // Log event.
            // Event Severity: logEvent.Level
            // Http trace: logEvent.EventType == LogEventType.HttpTrace
            // Log trace: logEvent.EventType == LogEventType.Trace
            var logString = this._formatter.Format(logEvent);

            if (logEvent.Level == System.Diagnostics.TraceLevel.Error)
            {
                var lExTel = new ExceptionTelemetry(new Exception(logString));
                _telemetryClient.TrackException(lExTel);
            }
            else
            {
                // Filter config set in ServiceHost.Configure should take care of if we want to track or not
                _telemetryClient.TrackTrace(logString);
            }

#if DEBUG
            if (logEvent.Level != System.Diagnostics.TraceLevel.Verbose && logEvent.Level != System.Diagnostics.TraceLevel.Info)
            {
                Console.WriteLine(logString);
            }
#endif
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            // Error occurred with the logger, not with the SDK.
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            // Graph Logger has completed logging (shutdown).
        }
    }
}