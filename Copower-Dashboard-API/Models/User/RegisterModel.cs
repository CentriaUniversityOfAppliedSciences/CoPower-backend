namespace Copower_API.Models.User
{
    /// <summary>
    /// User registration model
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Email of the user
        /// </summary>
        public required string Email { get; set; }
        /// <summary>
        /// Password of the user
        /// </summary>
        public required string Password { get; set; }
        /// <summary>
        /// Registeration token
        /// </summary>
        public required string Token { get; set; }
    }

    /// <summary>
    /// Return model after successful registration
    /// </summary>
    public class RegisterReturnModel
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Email of the user
        /// </summary>
        public required string Email { get; set; }
        /// <summary>
        /// Organisation of the user
        /// </summary>
        public required string Organisation { get; set; }
    }
}
