using System.ComponentModel.DataAnnotations;

namespace Copower_API.Models.Email
{
    /// <summary>
    /// Parameters
    /// </summary>
    public class EmailParameters
    {
        /// <summary>
        /// Email language
        /// </summary>
        [MaxLength(2)]
        public string Language { get; set; } = "en";
        /// <summary>
        /// Message of the email
        /// </summary>
        public required string Message { get; set; }
        /// <summary>
        /// UUID of the recipient of the email
        /// </summary>
        [MaxLength(36)]
        public required Copower_API.Entities.User Recipient { get; set; }
        /// <summary>
        /// Topic of the email
        /// </summary>
        public required string Subject { get; set; }
    }
}
