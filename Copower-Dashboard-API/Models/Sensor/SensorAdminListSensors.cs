namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Sensor admin list sensors model
    /// </summary>
    public class SensorAdminListSensors
    {
        /// <summary>
        /// Creation date of the sensor
        /// </summary>
        public required DateTimeOffset? Created { get; set; }
        /// <summary>
        /// Device source
        /// </summary>
        public required String DeviceSource { get; set; }
        /// <summary>
        /// Disabled status of the sensor
        /// </summary>
        public required bool Disabled { get; set; }
        /// <summary>
        /// Id of the sensor
        /// </summary>
        public required Guid Id { get; set; }
        /// <summary>
        /// Last data received date of the sensor
        /// </summary>
        public DateTimeOffset? LastData { get; set; }
        /// <summary>
        /// Name of the sensor
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the resource is shared.
        /// </summary>
        public required int Shared { get; set; }
        /// <summary>
        /// Measurement source of the sensor
        /// </summary>
        public required string Source { get; set; }
        /// <summary>
        /// Unit of the sensor
        /// </summary>
        public required string Unit { get; set; }
        /// <summary>
        /// Update date of the sensor
        /// </summary>
        public DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// To what the value should be changed to
        /// </summary>
        public required double ValueChange { get; set; }
    }
}
