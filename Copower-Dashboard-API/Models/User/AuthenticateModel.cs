using Newtonsoft.Json;

namespace Copower_API.Models.User
{
    /// <summary>
    /// Authentication model
    /// </summary>
    public class AuthenticateModel
    {
        /// <summary>
        /// Email
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Email { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Password { get; set; }
    }
}
