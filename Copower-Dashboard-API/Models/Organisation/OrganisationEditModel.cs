namespace Copower_API.Models.Organisation
{
    /// <summary>
    /// Organisation edit model
    /// </summary>
    public class OrganisationEdit
    {
        /// <summary>
        /// Disabled status of the organisation.
        /// </summary>
        public required Boolean Disabled { get; set; }
        /// <summary>
        /// Id of the organisation.
        /// </summary>
        public required Guid Id { get; set; }
        /// <summary>
        /// Name of the organisation.
        /// </summary>
        public required string Name { get; set; }
    }
}
