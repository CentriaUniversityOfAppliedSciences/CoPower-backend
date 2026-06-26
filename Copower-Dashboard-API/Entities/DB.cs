using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// DB table
    /// </summary>
    public class DB
    {
        /// <summary>
        /// Chart fetch setting
        /// </summary>
        public required int ChartFetch { get; set; }

        /// <summary>
        /// DB id
        /// </summary>
        public required String DBId { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public required Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the identification number associated with the entity.
        /// </summary>
        public int? IdNumber { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public required string Name { get; set; }
    }
}
