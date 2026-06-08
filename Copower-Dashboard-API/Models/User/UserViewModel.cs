using System.ComponentModel.DataAnnotations;
using Copower_API.Models.Organisation;

namespace Copower_API.Models.User
{
    /// <summary>
    /// User view model
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// User access level
        /// </summary>
        public string? Access { get; set; }
        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime? Created { get; set; }
        /// <summary>
        /// Disabled status
        /// </summary>
        public bool? Disabled { get; set; }
        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        public string? Email { get; set; }
        /// <summary>
        /// Name of the user
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Organisation ID
        /// </summary>
        public string? Organisation { get; set; }
        /// <summary>
        /// Password hash
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// Update date
        /// </summary>
        public DateTime? Updated { get; set; }
    }
}
