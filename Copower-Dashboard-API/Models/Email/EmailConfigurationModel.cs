namespace Copower_API.Models.Email
{
    /// <summary>
    /// Email Configuration
    /// </summary>
    public class EmailConfigurationModel
    {
        /// <summary>
        /// Is Email active?
        /// </summary>
        public required Boolean Active { get; set; }
        /// <summary>
        /// Client ID
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// Client Secret
        /// </summary>
        public required string ClientSecret { get; set; }
        /// <summary>
        /// From address
        /// </summary>
        public required string FromEmail { get; set; }
        /// <summary>
        /// From name
        /// </summary>
        public required string FromName { get; set; }
        /// <summary>
        /// Tenant ID
        /// </summary>
        public required string TenantId { get; set; }
    }
}
