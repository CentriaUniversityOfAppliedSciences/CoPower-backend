namespace Copower_API.Models.Measurements
{
    /// <summary>
    /// Measurements get model
    /// </summary>
    public class MeasurementsGetModel
    {
        /// <summary>
        /// End time
        /// </summary>
        public required DateTime EndTime { get; set; }
        /// <summary>
        /// Start time
        /// </summary>
        public required DateTime StartTime { get; set; }
    }
}
