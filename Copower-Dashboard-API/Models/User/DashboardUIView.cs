namespace Copower_API.Models.User
{
    /// <summary>
    /// Dashboard view model for the user.
    /// </summary>
    public class DashboardUIView
    {
        /// <summary>
        /// Name of the sensor
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Sensors
        /// </summary>
        public required List<UserSensorViewData> Sensors { get; set; }
    }

    /// <summary>
    /// Sensor object associated with the dashboard.
    /// </summary>
    public class UserSensorViewData
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
        public required Guid Sensor { get; set; }
        /// <summary>
        /// Type of the sensor UI element.
        /// </summary>
        public required string Type { get; set; }
        /// <summary>
        /// Sensor unit
        /// </summary>
        public required string Unit { get; set; }
    }
}
