namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Measurement sources list
    /// </summary>
    public class SourcesList
    {
        /// <summary>
        /// First level source
        /// </summary>
        public required Guid Source0 { get; set; }
        /// <summary>
        /// Second level source
        /// </summary>
        public required string Source1 { get; set; }
    }
}
