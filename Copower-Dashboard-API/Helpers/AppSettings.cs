namespace Copower_API.Helpers
{
    /// <summary>
    /// App settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Name of the application
        /// </summary>
        public required string AppName { get; set; }

        /// <summary>
        /// Base path of the API
        /// </summary>
        public required string BasePath { get; set; }

        /// <summary>
        /// Client API key
        /// </summary>
        public required string ClientAPIKey { get; set; }

        /// <summary>
        /// Activate logging
        /// </summary>
        public required Boolean LoggingActive { get; set; }

        /// <summary>
        /// How many results per one page in pagination
        /// </summary>
        public required int ResultsPerPage { get; set; }

        /// <summary>
        /// Secret
        /// </summary>
        public required string Secret { get; set; }
    }
}
