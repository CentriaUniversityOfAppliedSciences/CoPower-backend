using System.ComponentModel.DataAnnotations;
using Copower_API.Models.Organisation;

namespace Copower_API.Models.Users
{
    /// <summary>
    /// User list model
    /// </summary>
    public class UserListModel
    {
        /// <summary>
        /// Organisations
        /// </summary>
        public required List<UserListOrganisationsModel> Organisations { get; set; }

        /// <summary>
        /// Users
        /// </summary>
        public required List<UserListUsersModel> Users { get; set; }
    }
}
