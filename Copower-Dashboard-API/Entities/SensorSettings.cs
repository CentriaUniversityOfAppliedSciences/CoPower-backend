using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// Sensor settings
    /// </summary>
    public class SensorSettings
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public required Guid Id { get; set; }

        /// <summary>
        /// When the sensor was created
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Is the sensor deleted and if has when was it deleted
        /// </summary>
        public DateTimeOffset? Deleted { get; set; }

        /// <summary>
        /// Is the sensor disabled
        /// </summary>
        public Boolean? Disabled { get; set; }

        /// <summary>
        /// Can the sensor be shown in dashboard
        /// </summary>
        public Boolean? DisplayDashboard { get; set; }

        /// <summary>
        /// What database is the data for the sensor is on
        /// </summary>
        public required Guid DBID { get; set; }

        /// <summary>
        /// What value in the database is the data for the sensor
        /// </summary>
        public required string DBVALUE { get; set; }

        /// <summary>
        /// Device source
        /// </summary>
        public required string DeviceSource { get; set; }

        /// <summary>
        /// Name of the sensor
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Organisation who owns the sensor
        /// </summary>
        public required Guid Organisation { get; set; }

        /// <summary>
        /// Is the sensor shared
        /// </summary>
        public required int Shared { get; set; }

        /// <summary>
        /// Unit of the sensor
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// When the sensor has been updated
        /// </summary>
        public DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// To what the value should be changed to
        /// </summary>
        public double? ValueChange { get; set; }
    }
}
