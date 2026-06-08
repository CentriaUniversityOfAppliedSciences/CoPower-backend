namespace Copower_API.Models.Organisation
{
    /// <summary>
    /// Organisation list model
    /// </summary>
    public class OrganisationList
    {
        /// <summary>
        /// Creation date of the organisation
        /// </summary>
        public required DateTimeOffset Created { get; set; }
        /// <summary>
        /// Disabled status of the organisation
        /// </summary>
        public required Boolean Disabled { get; set; }
        /// <summary>
        /// Id of the organisation
        /// </summary>
        public required Guid? Id { get; set; }
        /// <summary>
        /// Name of the organisation
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Gets or sets the type identifier associated with the entity.
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// Update time of the organisation
        /// </summary>
        public required DateTimeOffset? Updated { get; set; }
    }
}
