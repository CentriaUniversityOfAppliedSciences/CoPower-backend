namespace Copower_API.Models.Sensor
{
    /// <summary>
    /// Sensor list organisations model
    /// </summary>
    public class SensorAdminListOrganisations
    {
        /// <summary>
        /// Id of the organisation
        /// </summary>
        public required Guid? Id { get; set; }

        /// <summary>
        /// Name of the organisation
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// List of sensors
        /// </summary>
        public required List<SensorAdminListSensors> Sensors { get; set; }
    }
}
