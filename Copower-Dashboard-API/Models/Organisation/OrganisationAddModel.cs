namespace Copower_API.Models.Organisation
{
    /// <summary>
    /// Add organisation model
    /// </summary>
    public class OrganisationAdd
    {
        /// <summary>
        /// Disabled status of the organisation
        /// </summary>
        public required Boolean? Disabled { get; set; }
        /// <summary>
        /// Name of the organisation
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Gets or sets the type identifier for the current instance.
        /// </summary>
        public int Type { get; set; }
    }
}
