using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// User entity representing a user in the system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identifier for the user.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Access level of the user.
        /// </summary>
        public string? Access { get; set; }

        /// <summary>
        /// Creation date of the user.
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Deleted status of the user.
        /// </summary>
        public DateTimeOffset? Deleted { get; set; }

        /// <summary>
        /// Disabled status of the user.
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Failed login attempts of the user.
        /// </summary>
        public int? FailedLogins { get; set; }

        /// <summary>
        /// Last login date and time of the user.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Name of the user.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Organisation associated with the user.
        /// </summary>
        public Guid? Organisation { get; set; }

        /// <summary>
        /// Password for the user.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Has the user been registered
        /// </summary>
        public DateTimeOffset? Registered { get; set; }

        /// <summary>
        /// Updated date and time of the user.
        /// </summary>
        public DateTimeOffset? Updated { get; set; }
    }

    /// <summary>
    /// Sensor UI element for the user.
    /// </summary>
    public class UserDashboard
    {
        /// <summary>
        /// Name of the sensor UI element.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Sensor associated with the UI element.
        /// </summary>
        public required List<UserSensorData> Sensors { get; set; }

        /// <summary>
        /// Set of the grid item in x,y
        /// </summary>
        public int[] Size { get; set; } = [1, 1];
    }

    /// <summary>
    /// Sensor object associated with the dashboard.
    /// </summary>
    public class UserSensorData
    {
        /// <summary>
        /// Color associated with the sensor.
        /// </summary>
        public required string Color { get; set; }

        /// <summary>
        /// Name of the measurement
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Sensor identifier.
        /// </summary>
        public required string Sensor { get; set; }

        /// <summary>
        /// Type of the sensor UI element.
        /// </summary>
        public required string Type { get; set; }
    }
}
