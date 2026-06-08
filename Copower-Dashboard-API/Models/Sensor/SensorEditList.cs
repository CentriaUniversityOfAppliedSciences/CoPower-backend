namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Sensor edit list model
    /// </summary>
    public class SensorEditList
    {
        /// <summary>
        /// ID
        /// </summary>
        public required Guid Id { get; set; }
        /// <summary>
        /// Name of the sensor
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Owning organisation
        /// </summary>
        public required string Organisation { get; set; }
        /// <summary>
        /// Unit of the sensor
        /// </summary>
        public required string Unit { get; set; }
    }
}
