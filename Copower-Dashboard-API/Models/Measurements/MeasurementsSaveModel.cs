namespace Copower_API.Models.Measurements
{
    /// <summary>
    /// Measurement save model
    /// </summary>
    public class MeasurementsSaveModel
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
