using Copower_API.Helpers;
using Microsoft.Extensions.Logging;
using Serilog;


namespace Copower_API.Services
{
    /// <summary>
    /// IGeneralService
    /// </summary>
    public interface IGeneralService
    {
        /// <summary>
        /// Stops the server and performs any necessary cleanup operations. Optionally provides an error message to
        /// indicate the reason for stopping.
        /// </summary>
        /// <param name="errorMessage">An optional error message that describes the reason for stopping the server. If null or empty, the server
        /// stops without specifying a reason.</param>
        void StopServer(string? errorMessage);

        /// <summary>
        /// Writes a log entry with the specified source, request identifier, function name, and message.
        /// </summary>
        /// <param name="source">The name of the component or subsystem generating the log entry. Cannot be null or empty.</param>
        /// <param name="reqid">The unique identifier for the request or operation associated with the log entry. Cannot be null or empty.</param>
        /// <param name="function">The name of the function or method where the log entry originated. Cannot be null or empty.</param>
        /// <param name="message">The log message to record. Cannot be null.</param>
        void WriteLogMessage(string source, string reqid, string function, string message);
    }

    /// <summary>
    /// General Service
    /// </summary>
    /// <param name="appLifeTime">App life time</param>
    /// <param name="configuration">Server configuration</param>
    /// <param name="logger">Logger service</param>
    public class GeneralService(IHostApplicationLifetime appLifeTime, IConfiguration configuration, ILogger<GeneralService> logger) : IGeneralService
    {
        private readonly IHostApplicationLifetime _appLifeTime = appLifeTime;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<GeneralService> _logger = logger;
        private readonly String serverMode = configuration.GetSection("Environment").Get<string>();
        /// <inheritdoc/>
        public void StopServer(string? errorMessage)
        {
            Console.WriteLine("Stopping server...");
            if (errorMessage != null)
                WriteLogMessage("server", "", "GeneralService.StopServer", "Stopping server with " + errorMessage);
            else
                WriteLogMessage("server", "", "GeneralService.StopServer", "Stopping server...");

            _appLifeTime.StopApplication();
        }

        /// <inheritdoc/>
        public void WriteLogMessage(string source, string reqid, string function, string message)
        {
            try
            {
                DateTime time = DateTime.Now;
                TimeZoneInfo timezone = TimeZoneInfo.Local;

                var logtime = time.ToString("dd.MM.yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture) + '+' + timezone.BaseUtcOffset;
                var logmessage = message + '\n';
                logmessage = logtime + " # " + reqid + " # " + source + " # " + function + " # " + logmessage;
                var appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();

                if (serverMode == "development")
                    Console.WriteLine($"LogTime: {logtime} | RequestId: {reqid} | Source: {source} | Function: {function} | Message: {message}");

                if (appsettings != null)
                    _logger.LogInformation("LogTime: {LogTime} | RequestId: {RequestId} | Source: {Source} | Function: {Function} | Message: {Message}",
                                logtime, reqid, source, function, message);
                else
                    Console.WriteLine("Appsettings error encountered, unable to write log message");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
