using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// Chart data fetch settings entity representing the configuration for aggregating data for chart.
    /// </summary>
    public class ChartDataFetchSettings
    {
        /// <summary>
        /// Gets or sets the aggregation interval for one day length.
        /// </summary>
        public required string Day1 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for two days length.
        /// </summary>
        public required string Day2 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for three days length.
        /// </summary>
        public required string Day3 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for seven days length.
        /// </summary>
        public required string Day7 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for fourteen days length.
        /// </summary>
        public required string Day14 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for thirty days length.
        /// </summary>
        public required string Day30 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for ninety days length.
        /// </summary>
        public required string Day90 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for one hundred eighty days length.
        /// </summary>
        public required string Day180 { get; set; }

        /// <summary>
        /// Gets or sets the aggregation interval for three hundred sixty-five days length.
        /// </summary>
        public required string Day365 { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the chart data update settings entity.
        /// </summary>
        [Key]
        public required int Id { get; set; }
    }
}
