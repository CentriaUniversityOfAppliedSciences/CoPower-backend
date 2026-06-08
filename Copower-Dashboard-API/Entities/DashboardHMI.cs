namespace Copower_API.Entities
{
    /// <summary>
    /// Dashboard HMI (Human-Machine Interface) entity representing the structure of the dashboard and its associated sensors.
    /// </summary>
    public class DashboardHMI
    {
        /// <summary>
        /// Name of the sensor HMI element.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Sensor associated with the HMI element.
        /// </summary>
        public required List<HMISensorData> Sensors { get; set; }
    }

    /// <summary>
    /// Sensor object associated with the dashboard.
    /// </summary>
    public class HMISensorData
    {
        /// <summary>
        /// Gets or sets the type of the element represented as a string.
        /// </summary>
        /// <remarks>This property is required and must be assigned a valid string value that specifies
        /// the element type.</remarks>
        public required string ElementType { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        /// <remarks>The Id property is optional, if not provided no data collection is done. It serves
        /// as the primary key for identifying the entity within a collection or database.</remarks>
        public required string Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the entity. This property is required and cannot be null or empty.
        /// </summary>
        /// <remarks>The Name property is essential for identifying the entity and must be provided during
        /// initialization.</remarks>
        public required string Name { get; set; }
        /// <summary>
        /// Gets or sets the unit of measurement for the associated value.
        /// </summary>
        /// <remarks>This property is required and must be a non-empty string that represents the unit,
        /// such as "meters" or "seconds".</remarks>
        public required string Unit { get; set; }
    }
}
