using System.ComponentModel.DataAnnotations;
using Copower_API.Models.Organisation;

namespace Copower_API.Models.Users
{
    /// <summary>
    /// User list users model
    /// </summary>
    public class UserListUsersModel
    {
        /// <summary>
        /// Access level of the user.
        /// </summary>
        public string? Access { get; set; }

        /// <summary>
        /// Creation date of the user.
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Disabled status of the user.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Identifier for the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Last login date and time of the user.
        /// </summary>
        public DateTimeOffset? LastLogin { get; set; }

        /// <summary>
        /// Organisation details of the user.
        /// </summary>
        public Guid? Organisation { get; set; }

        /// <summary>
        /// Has the user registered yet, allows to resend invitation if hasn't
        /// </summary>
        public required Boolean Registered { get; set; }

        /// <summary>
        /// Updated date of the user.
        /// </summary>
        public DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// Name of the user.
        /// </summary>
        public required string Username { get; set; }
    }
}
