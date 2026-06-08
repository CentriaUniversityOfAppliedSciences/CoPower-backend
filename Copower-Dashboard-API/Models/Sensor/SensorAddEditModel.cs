namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Sensor add/edit model
    /// </summary>
    public class SensorAddEditModel
    {
        /// <summary>
        /// Device source
        /// </summary>
        public required String DeviceSource { get; set; }

        /// <summary>
        /// Disabled status of the sensor.
        /// </summary>
        public required Boolean Disabled { get; set; }
        /// <summary>
        /// Name of the sensor.
        /// </summary>
        public required String Name { get; set; }
        /// <summary>
        /// Is the sensor shared.
        /// </summary>
        public required int Shared { get; set; }
        /// <summary>
        /// Measurement source of the sensor.
        /// </summary>
        public required String Source { get; set; }
        /// <summary>
        /// Unit of the sensor.
        /// </summary>
        public required String Unit { get; set; }

        /// <summary>
        /// With the value should be divided with
        /// </summary>
        public required float ValueChange { get; set; }
    }
}
