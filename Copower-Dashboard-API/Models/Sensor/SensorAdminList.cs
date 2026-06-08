namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Sensor admin list model
    /// </summary>
    public class SensorAdminList
    {
        /// <summary>
        /// Organisations
        /// </summary>
        public required List<SensorAdminListOrganisations> Organisations { get; set; }

        /// <summary>
        /// Measurement sources
        /// </summary>
        public required List<SourcesList> Sources { get; set; }

        /// <summary>
        /// Measurement source names
        /// </summary>
        public required List<SourcesNameList> SourcesName { get; set; }
    }
}
