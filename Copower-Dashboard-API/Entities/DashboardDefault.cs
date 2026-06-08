using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// Default dashboard entity.
    /// </summary>
    public class DashboardDefault
    {
        /// <summary>
        /// Organisation identifier (or "default").
        /// </summary>
        [Key]
        public required string Id { get; set; }

        /// <summary>
        /// Dashboard
        /// </summary>
        public List<UserDashboard>? Dashboard { get; set; }
    }
}
