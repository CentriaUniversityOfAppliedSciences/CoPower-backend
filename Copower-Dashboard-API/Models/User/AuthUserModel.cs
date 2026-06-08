using Copower_API.Entities;

namespace Copower_API.Models.User
{
    /// <summary>
    /// Authentication user response
    /// </summary>
    public class AuthUserModel
    {
        /// <summary>
        /// Name
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Authentication token
        /// </summary>
        public required string Token { get; set; }
    }
}
