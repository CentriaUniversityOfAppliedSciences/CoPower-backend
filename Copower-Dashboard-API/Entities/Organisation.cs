using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// Organisation entity
    /// </summary>
    public class Organisation
    {
        /// <summary>
        /// ID
        /// </summary>
        [Key]
        public required Guid? Id { get; set; }
        /// <summary>
        /// Creation date
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Deleted time
        /// </summary>
        public DateTimeOffset? Deleted { get; set; }
        /// <summary>
        /// Deleted items
        /// </summary>
        public string[]? DeletedItems { get; set; }
        /// <summary>
        /// Disabled flag
        /// </summary>
        public Boolean? Disabled { get; set; }
        /// <summary>
        /// Name of the organisation
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Gets or sets the type identifier associated with the entity.
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// Updated time
        /// </summary>
        public DateTimeOffset? Updated { get; set; }
    }
}
