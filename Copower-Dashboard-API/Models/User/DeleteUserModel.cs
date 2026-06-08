namespace Copower_API.Models.User
{
    /// <summary>
    /// Delete user model
    /// </summary>
    public class DeleteUserModel
    {
        /// <summary>
        /// Password of the user to be deleted. This is required to confirm the user's identity and authorize the deletion process.
        /// </summary>
        public required String Password { get; set; }
    }
}
