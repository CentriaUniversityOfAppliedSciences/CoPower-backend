namespace Copower_API.Models.User
{
    /// <summary>
    /// Reset password model
    /// </summary>
    public class ResetPasswordModel
    {
        /// <summary>
        /// Reset token
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        /// New password
        /// </summary>
        public required string NewPassword { get; set; }
    }
}
