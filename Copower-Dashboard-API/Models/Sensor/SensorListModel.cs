namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Sensor list model
    /// </summary>
    public class SensorListModel
    {
        /// <summary>
        /// Name of the sensor.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// ID of the sensor.
        /// </summary>
        public required Guid Id { get; set; }
        /// <summary>
        /// Unit of the sensor.
        /// </summary>
        public required string Unit { get; set; }
    }
}
