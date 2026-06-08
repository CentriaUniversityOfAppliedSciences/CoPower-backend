namespace Copower_API.Models.Measurements
{
    /// <summary>
    /// Measurement save model
    /// </summary>
    public class MeasurementsSaveModel
    {
        /// <summary>
        /// Sensor id of the measurement
        /// </summary>
        public required Guid Sensor { get; set; }
        /// <summary>
        /// Value for the measurment
        /// </summary>
        public required List<MeasurementsSaveSubmodel> Values { get; set; }
    }

    /// <summary>
    /// Measurements save submodel
    /// </summary>
    public class MeasurementsSaveSubmodel
    {
        /// <summary>
        /// Date for the measurement
        /// </summary>
        public required DateTimeOffset Date { get; set; }
        /// <summary>
        /// Value for the measurment
        /// </summary>
        public required double Value { get; set; }
    }
}
