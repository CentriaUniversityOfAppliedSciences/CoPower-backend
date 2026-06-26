namespace Copower_API.Helpers
{
    /// <summary>
    /// Represents a measurement entity in the HMI system, providing properties for its associated data table and user
    /// interface representation.
    /// </summary>
    /// <remarks>Use this class to encapsulate information about a measurement for display and processing
    /// within HMI applications. The Table property identifies the source or category of the measurement data, while the
    /// UI property specifies how the measurement should be presented in the user interface.</remarks>
    public class HMIMeasurement
    {
        /// <summary>
        /// Divide the measurement by this value, optional
        /// </summary>
        public required int? Divide { get; set; }
        /// <summary>
        /// Table name
        /// </summary>
        public required string Table { get; set; }
        /// <summary>
        ///  User interface name
        /// </summary>
        public required string UI { get; set; }
    }

    /// <summary>
    /// Input settings
    /// </summary>
    public class SettingsInput
    {
        /// <summary>
        /// Length of the chart on the dashboard
        /// </summary>
        public int DashboardChartName { get; set; }
        /// <summary>
        /// Email max length
        /// </summary>
        public int Email { get; set; }

        /// <summary>
        /// Name max length
        /// </summary>
        public int Name { get; set; }

        /// <summary>
        /// Organisation max length
        /// </summary>
        public int Organisation { get; set; }

        /// <summary>
        /// Password max length
        /// </summary>
        public int Password { get; set; }

        /// <summary>
        /// Password minimum length
        /// </summary>
        public int Password_min { get; set; }

        /// <summary>
        /// General maximum length of a long string
        /// </summary>
        public int StringLong { get; set; }

        /// <summary>
        /// General maximum length of a short string
        /// </summary>
        public int StringShort { get; set; }
    }

    /// <summary>
    /// Server settings
    /// </summary>
    public class SettingsServer
    {
        /// <summary>
        /// Server path for API
        /// </summary>
        public required string API { get; set; }
        /// <summary>
        /// Server path for dashboard
        /// </summary>
        public required string Dashboard { get; set; }
        /// <summary>
        /// Server path
        /// </summary>
        public required string Server { get; set; }
    }

    /// <summary>
    /// Represents the configuration settings for value-added tax (VAT), including required measurements which need it and the VAT
    /// percentage value.
    /// </summary>
    /// <remarks>The 'requiredMeasurements' property includes which measurements need to have mandatory
    /// VAT value added ´to them. The 'value' property represents the VAT percentage as an float, typically corresponding
    /// to the applicable tax rate.</remarks>
    public class SettingsVAT
    {
        /// <summary>
        /// Gets or sets the collection of measurement names required for the operation. Defaults to empty array.
        /// </summary>
        /// <remarks>This property must be set with valid measurement names before executing the
        /// operation. Each name should correspond to a predefined set of acceptable measurements.</remarks>
        public required string[] RequiredMeasurements { get; set; } = [];
        /// <summary>
        /// Gets or sets the required VAT value is calculated into the value. Defaults to zero.
        /// </summary>
        public required float Value { get; set; } = 0;
    }

    /// <summary>
    /// Settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Generated password length for API user
        /// </summary>
        public required int APITokenLength { get; set; }
        /// <summary>
        /// Gets or sets the expiration time, in hours, for authentication tokens.
        /// </summary>
        public required double AuthTokenExpire { get; set; }
        /// <summary>
        /// Available chart types for dashboard in hours
        /// </summary>
        public required List<string> ChartTypes { get; set; }
        /// <summary>
        /// Dashboard settings
        /// </summary>
        public required DashboardSettings Dashboard { get; set; }
        /// <summary>
        /// Forgotten password token expire in hours
        /// </summary>
        public required int ForgottenPasswordTokenExpire { get; set; }
        /// <summary>
        /// Input settings min/max values
        /// </summary>
        /// 
        public required List<HMIMeasurement> HMIMeasurements { get; set; }
        /// <summary>
        /// What is the maximum length of the input values for different fields in the application, such as email, name, password, etc. This ensures that user input adheres to defined constraints and prevents issues related to excessively long input data.
        /// </summary>
        public required SettingsInput InputMax { get; set; }
        /// <summary>
        /// Available languages
        /// </summary>
        public required List<string> Languages { get; set; }
        /// <summary>
        /// Password reset token length
        /// </summary>
        public required int ResetPasswordTokenLength { get; set; }
        /// <summary>
        /// Server paths
        /// </summary>
        public required SettingsServer ServerPath { get; set; }
        /// <summary>
        /// Gets or sets the VAT configuration settings for the application.
        /// </summary>
        /// <remarks>This property must be initialized before use. It determines how value-added tax (VAT)
        /// is calculated and applied to transactions throughout the application.</remarks>
        public required SettingsVAT VAT { get; set; }
    }

    /// <summary>
    /// Chart item sizes
    /// </summary>
    public class ChartSizes
    {
        /// <summary>
        /// Maximum height
        /// </summary>
        public required int MaxHeight { get; set; }
        /// <summary>
        /// Maximum width
        /// </summary>
        public required int MaxWidth { get; set; }
        /// <summary>
        /// Minimum height
        /// </summary>
        public required int MinHeight { get; set; }
        /// <summary>
        /// Minimum height
        /// </summary>
        public required int MinWidth { get; set; }
    }

    /// <summary>
    /// Dashboard settings
    /// </summary>
    public class DashboardSettings
    {
        /// <summary>
        /// Chart item sizes
        /// </summary>
        public required ChartSizes ChartSize { get; set; }
    }
}
