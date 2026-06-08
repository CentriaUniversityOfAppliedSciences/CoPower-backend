using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// Reset token
    /// </summary>
    public class ResetTokens
    {
        /// <summary>
        /// When the token was created
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// When the token will expire
        /// </summary>
        public DateTimeOffset Expiry { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public required Guid Id { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// When the token was used
        /// </summary>
        public DateTimeOffset? Used { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        public required Guid UserId { get; set; }
    }
}
