namespace Copower_API.Models.Measurements
{
    /// <summary>
    /// Represents a model for HMI measurements, encapsulating a unique identifier, the timestamp of the latest update,
    /// and a collection of associated sensor measurement data.
    /// </summary>
    /// <remarks>This class is used to manage and transfer measurement data within the application, ensuring
    /// each measurement instance is uniquely identifiable and contains relevant sensor data. It is typically utilized
    /// for communication between application layers or external systems where measurement tracking and sensor data
    /// aggregation are required.</remarks>
    public class MeasurementsHMIModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        /// <remarks>The Id property is required and must be provided to uniquely identify the entity
        /// within its context.</remarks>
        public required String Id { get; set; }

        /// <summary>
        /// When was the fetched data updated
        /// </summary>
        public required DateTimeOffset? Updated { get; set; }
        
        /// <summary>
        /// Gets or sets the list of sensor measurements.
        /// </summary>
        /// <remarks>This property is required and must contain the collection of sensor data points
        /// associated with this measurement instance.</remarks>
        public required List<MeasurementsHMIDataModel> Sensors { get; set; }
    }

    /// <summary>
    /// Represents the data model for a measurement in the HMI, associating a specific sensor with its corresponding
    /// measurement value.
    /// </summary>
    /// <remarks>Each instance of this class encapsulates the essential data required to identify a
    /// sensor and store its measurement value. The measurement value may be absent, indicated by a null value. This
    /// model is typically used to transfer measurement data between application layers or external
    /// systems.</remarks>
    public class MeasurementsHMIDataModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the sensor.
        /// </summary>
        /// <remarks>This property is required and must not be null or empty. It is used to associate data
        /// and operations with a specific sensor instance.</remarks>
        public required String Sensor { get; set; }
        /// <summary>
        /// Gets or sets the required floating-point measurement value. This property can be null to indicate that no
        /// value is present.
        /// </summary>
        /// <remarks>The property must be assigned a valid value before use. If not set, it defaults to
        /// null, representing the absence of a measurement.</remarks>
        public required float? Value { get; set; }
    }
}
