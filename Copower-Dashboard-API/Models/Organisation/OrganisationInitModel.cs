namespace Copower_API.Models.Organisation
{
    /// <summary>
    /// Organisation init data
    /// </summary>
    public class OrganisationInitModel
    {
        /// <summary>
        /// Id of the database
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Name of the database
        /// </summary>
        public required String Name { get; set; }
    }
}
