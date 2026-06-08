using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// Measurement data
    /// </summary>
    public class MeasurementData
    {
        /// <summary>
        /// Date of the measurement
        /// </summary>
        public required DateTime Date { get; set; }
        /// <summary>
        /// Value of the measurement
        /// </summary>
        public required double Value { get; set; }
    }
}
