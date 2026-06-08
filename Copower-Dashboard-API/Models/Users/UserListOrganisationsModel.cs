namespace Copower_API.Models.Users
{
    /// <summary>
    /// User list organisations model
    /// </summary>
    public class UserListOrganisationsModel
    {
        /// <summary>
        /// Id of the organisation
        /// </summary>
        public required Guid? Id { get; set; }

        /// <summary>
        /// Name of the organisation
        /// </summary>
        public required string Name { get; set; }
    }
}
