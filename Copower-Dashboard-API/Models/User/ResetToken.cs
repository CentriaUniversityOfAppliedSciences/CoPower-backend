using System.ComponentModel.DataAnnotations;

namespace Copower_API.Models.User
{
    /// <summary>
    /// Reset token
    /// </summary>
    public class ResetToken
    {
        /// <summary>
        /// Unique identifier for the reset token
        /// </summary>
        public required Guid Id { get; set; }
        /// <summary>
        /// User Id associated with the reset token
        /// </summary>
        public required string UserId { get; set; }
        /// <summary>
        /// Reset token string
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        /// Indicates whether the token has been used
        /// </summary>
        public required bool IsUsed { get; set; }
        /// <summary>
        /// When the token will expire
        /// </summary>
        public required DateTimeOffset Expiry { get; set; }
    }
}
