using System.ComponentModel.DataAnnotations;

namespace Copower_API.Models.User
{
    /// <summary>
    /// Forgotten password token validation request
    /// </summary>
    public class TokenValidationRequest
    {
        /// <summary>
        /// Forgotten password token
        /// </summary>
        public required string Token { get; set; }
    }
}
