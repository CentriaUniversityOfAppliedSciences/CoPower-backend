namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Sensor update model
    /// </summary>
    public class SensorUpdateModel
    {
        /// <summary>
        /// Disabled status of the sensor.
        /// </summary>
        public Boolean? Disabled { get; set; }
        /// <summary>
        /// ID of the sensor.
        /// </summary>
        public required string Id { get; set; }
        /// <summary>
        /// Location of the sensor.
        /// </summary>
        public string? Location { get; set; }
        /// <summary>
        /// Name of the sensor.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Unit of the sensor.
        /// </summary>
        public string? Unit { get; set; }
    }
}
