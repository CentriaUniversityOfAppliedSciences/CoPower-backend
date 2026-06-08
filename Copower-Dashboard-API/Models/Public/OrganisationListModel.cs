using Copower_API.Models.Sensor;

namespace Copower_API.Models.Public
{
    /// <summary>
    /// Organisation list model
    /// </summary>
    public class OrganisationListModel
    {
        /// <summary>
        /// Id of the organisation
        /// </summary>
        public required string Id { get; set; }
        /// <summary>
        /// Name of the organisation
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Sensors in the organisation
        /// </summary>
        public required List<SensorListModel> Sensors { get; set; } = new List<SensorListModel>();
    }
}
