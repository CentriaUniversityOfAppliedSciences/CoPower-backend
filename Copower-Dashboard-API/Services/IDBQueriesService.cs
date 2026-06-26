using Copower_API.Context;
using Copower_API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Copower_API.Services
{
    /// <summary>
    /// Interface for database queries service
    /// </summary>
    public interface IDBQueries
    {
        /// <summary>
        /// Generates a SQL query string that retrieves the most recent measurement record from the specified table in
        /// the given database.
        /// </summary>
        /// <remarks>Ensure that the specified database and table exist and are accessible. The structure
        /// of the generated SQL query may depend on the schema of the table.</remarks>
        /// <param name="table">The name of the table containing the measurement data. This parameter cannot be null or empty.</param>
        /// <returns>A SQL query string that, when executed, returns the latest measurement record from the specified table.</returns>
        string GetLatestMeasurementSQL(string table);

        /// <summary>
        /// Generates a SQL query string to retrieve measurement records from the specified table within the given time
        /// range.
        /// </summary>
        /// <param name="chartDataFetchSettings">The settings for fetching chart data.</param>
        /// <param name="database">The name of the database containing the measurements table. Cannot be null or empty.</param>
        /// <param name="table">The name of the table from which to select measurement records. Cannot be null or empty.</param>
        /// <param name="startTime">The start of the time range for the query, specified as a string. Only records with a timestamp greater than
        /// or equal to this value are included.</param>
        /// <param name="endTime">The end of the time range for the query, specified as a string. Only records with a timestamp less than or
        /// equal to this value are included.</param>
        /// <param name="valueChange">How to change the value, optional</param>
        /// <returns>A SQL query string that selects measurement records from the specified table and database within the given
        /// time range.</returns>
        string GetMeasurementsSQL(ChartDataFetchSettings chartDataFetchSettings, string database, string table, string startTime, string endTime, double? valueChange = 1);
    }

    /// <summary>
    /// Database queries service
    /// </summary>
    public partial class DBQueries() : IDBQueries
    {
        private const string V = @"[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]+)?([Zz]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?";
        private readonly string datePattern = V;

        /// <inheritdoc/>
        public string GetLatestMeasurementSQL(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new Exception("GLMS362197");

            return @$"
                SELECT
                    ""Date"", ""Value""
                FROM
                    ""{table}""
                ORDER BY
                    ""Date"" DESC
                LIMIT
                    1
            ";
        }

        /// <inheritdoc/>
        public string GetMeasurementsSQL(ChartDataFetchSettings chartDataFetchSettings, string database, string table, string startTime, string endTime, double? valueChange = 1)
        {
            if (string.IsNullOrEmpty(database))
                throw new Exception("GMS283579");

            if (string.IsNullOrEmpty(table))
                throw new Exception("GMS362197");

            if (string.IsNullOrEmpty(startTime))
                throw new Exception("GMS140290A");

            if (Regex.IsMatch(startTime, datePattern) == false)
                throw new Exception("GMS439590A");

            if (string.IsNullOrEmpty(endTime))
                throw new Exception("GMS140290B");

            if (Regex.IsMatch(endTime, datePattern) == false)
                throw new Exception("GMS439590B");

            DateTime startTimeObj = DateTime.Parse(startTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime endTimeObj = DateTime.Parse(endTime, null, System.Globalization.DateTimeStyles.RoundtripKind);

            TimeSpan difference = endTimeObj - startTimeObj;

            var aggregateRange = "1 month";
            if (difference.TotalDays <= 1)
            {
                aggregateRange = chartDataFetchSettings.Day1;
            }
            else if (difference.TotalDays <= 2)
            {
                aggregateRange = chartDataFetchSettings.Day2;
            }
            else if (difference.TotalDays <= 3)
            {
                aggregateRange = chartDataFetchSettings.Day3;
            }
            else if (difference.TotalDays <= 7)
            {
                aggregateRange = chartDataFetchSettings.Day7;
            }
            else if (difference.TotalDays <= 14)
            {
                aggregateRange = chartDataFetchSettings.Day14;
            }
            else if (difference.TotalDays <= 30)
            {
                aggregateRange = chartDataFetchSettings.Day30;
            }
            else if (difference.TotalDays <= 90)
            {
                aggregateRange = chartDataFetchSettings.Day90;
            }
            else if (difference.TotalDays <= 180)
            {
                aggregateRange = chartDataFetchSettings.Day180;
            }
            else if (difference.TotalDays <= 365)
            {
                aggregateRange = chartDataFetchSettings.Day365;
            }

            if (aggregateRange == "null")
            {
                return @$"
                    SELECT
                        ""Date"",
                        ROUND(""Value""::numeric * {valueChange}, 1) AS ""Value""
                    FROM
                        ""{table}""
                    WHERE
                        ""Date"" BETWEEN '{startTime}' AND '{endTime}'
                    GROUP BY
                        1
                    ORDER BY
                        1
                ";
            }
            else
            {
                return @$"
                    SELECT
                        date_bin('{aggregateRange}', ""Date"", TIMESTAMP '2000-01-01') AS ""Date"",
                        ROUND(AVG(""Value""::numeric) * {valueChange}, 1) AS ""Value""
                    FROM
                        ""{table}""
                    WHERE
                        ""Date"" BETWEEN '{startTime}' AND '{endTime}'
                    GROUP BY
                        1
                    ORDER BY
                        1
                ";
            }
        }
    }
}
