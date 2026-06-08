namespace Copower_API.Models.Organisation
{
    /// <summary>
    /// Organisation Database List Model
    /// </summary>
    public class OrganisationDatabaseListModel
    {
        /// <summary>
        /// Id of the organisation
        /// </summary>
        public required int Id { get; set; }

        /// <summary>
        /// Name of the organisation
        /// </summary>
        public required string Name { get; set; }
    }
}
