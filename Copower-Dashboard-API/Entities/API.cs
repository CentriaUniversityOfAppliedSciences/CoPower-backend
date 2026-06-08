using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// API key class
    /// </summary>
    public class API
    {
        /// <summary>
        /// Is the API key active
        /// </summary>
        public Boolean? Active { get; set; }

        /// <summary>
        /// When was the API key created
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Who created the API key
        /// </summary>
        public required Guid Creator { get; set; }

        /// <summary>
        /// When was the API key deleted
        /// </summary>
        public DateTimeOffset? Deleted { get; set; }

        /// <summary>
        /// API key Id
        /// </summary>
        [Key]
        public required String Id { get; set; }

        /// <summary>
        /// When was the API key last used
        /// </summary>
        public DateTimeOffset? LastUsed { get; set; }

        /// <summary>
        /// To which organisation does the API key belong to
        /// </summary>
        public Guid? Organisation { get; set; }
    }
}
