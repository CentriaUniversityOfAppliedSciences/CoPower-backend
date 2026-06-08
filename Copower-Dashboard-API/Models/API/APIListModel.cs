namespace Copower_API.Models.API
{
    /// <summary>
    /// Init model for the API key, used to return the API key information to the user when they request it
    /// </summary>
    public class APIListModel
    {
        /// <summary>
        /// Is the API key active
        /// </summary>
        public required Boolean? Active { get; set; }

        /// <summary>
        /// When was the API key created
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Name of the creator who created the API key
        /// </summary>
        public required String Creator { get; set; }

        /// <summary>
        /// API key Id
        /// </summary>
        public required String Id { get; set; }

        /// <summary>
        /// When was the API key last used
        /// </summary>s
        public DateTimeOffset? LastUsed { get; set; }

        /// <summary>
        /// Organisation name
        /// </summary>
        public required String Organisation { get; set; }

        /// <summary>
        /// Organisation Id
        /// </summary>
        public required Guid OrganisationId { get; set; }
    }
}
