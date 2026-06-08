namespace Copower_API.Models.User
{
    /// <summary>
    /// Profile update model
    /// </summary>
    public class UserProfileUpdateModel
    {
        /// <summary>
        /// Email address of the user.
        /// </summary>
        public required string Email { get; set; }
        /// <summary>
        /// Name of the user.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Current password of the user.
        /// </summary>
        public required string CurrentPassword { get; set; }
        /// <summary>
        /// New password of the user. (optional)
        /// </summary>
        public string? NewPassword { get; set; }
    }
}
