using Copower_API.Entities;

namespace Copower_API.Models.User
{
    /// <summary>
    /// Dashboard UI model
    /// </summary>
    public class DashboardUIEdit
    {
        /// <summary>
        /// Name
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Sensors
        /// </summary>
        public required List<UserSensorData> Sensors { get; set; }

        /// <summary>
        /// Size of the grid item in x,y
        /// </summary>
        public int[] Size { get; set; } = [1, 1];
    }
}
