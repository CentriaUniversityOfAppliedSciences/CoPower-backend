namespace Copower_API.Models.Users
{
    /// <summary>
    /// User add model
    /// </summary>
    public class UsersAdd
    {
        /// <summary>
        /// Access level for the user.
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
