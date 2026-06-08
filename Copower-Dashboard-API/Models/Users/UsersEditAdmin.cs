namespace Copower_API.Models.Users
{
    /// <summary>
    /// User edit admin model
    /// </summary>
    public class UsersEditAdmin
    {
        /// <summary>
        /// Access level of the user.
        /// </summary>
        public required string Access { get; set; }

        /// <summary>
        /// Disabled status of the user.
        /// </summary>
        public required bool Disabled { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Name of the user.
        /// </summary>
        public required string Name { get; set; }
    }
}
