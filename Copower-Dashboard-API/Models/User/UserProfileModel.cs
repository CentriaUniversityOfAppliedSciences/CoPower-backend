namespace Copower_API.Models.User
{
    /// <summary>
    /// Profile model
    /// </summary>
    public class UserProfileModel
    {
        /// <summary>
        /// Email of the user.
        /// </summary>
        public required String Email { get; set; }
        /// <summary>
        /// Name of the user.
        /// </summary>
        public required String Name { get; set; }
    }
}
