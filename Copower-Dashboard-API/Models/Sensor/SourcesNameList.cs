namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Map database source identifier to a name
    /// </summary>
    public class SourcesNameList
    {
        /// <summary>
        /// Database source identifier
        /// </summary>
        public required Guid Id { get; set; }
        /// <summary>
        /// Database name
        /// </summary>
        public required String Name { get; set; }
    }
}
