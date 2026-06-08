namespace Copower_API.Models.User
{
    /// <summary>
    /// Forgot password model
    /// </summary>
    public class ForgotPasswordModel
    {
        /// <summary>
        /// Email of the user
        /// </summary>
        public required String Email { get; set; }
        /// <summary>
        /// Language of the user
        /// </summary>
        public String Language { get; set; } = "en";

    }
}
