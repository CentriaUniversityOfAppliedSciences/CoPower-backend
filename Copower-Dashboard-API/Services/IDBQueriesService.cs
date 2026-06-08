using Copower_API.Context;
using Copower_API.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        /// <param name="database">The name of the database containing the measurements table. Cannot be null or empty.</param>
        /// <param name="table">The name of the table from which to select measurement records. Cannot be null or empty.</param>
        /// <param name="startTime">The start of the time range for the query, specified as a string. Only records with a timestamp greater than
        /// or equal to this value are included.</param>
        /// <param name="endTime">The end of the time range for the query, specified as a string. Only records with a timestamp less than or
        /// equal to this value are included.</param>
        /// <returns>A SQL query string that selects measurement records from the specified table and database within the given
        /// time range.</returns>
        string GetMeasurementsSQL(string database, string table, string startTime, string endTime);
    }

    /// <summary>
    /// Database queries service
    /// </summary>
    /// <param name="commondataContext">Common data context</param>
    /// <param name="database1Context">Copower data context</param>
    public partial class DBQueries(CommondataContext commondataContext, Database1Context database1Context) : IDBQueries
    {
        private const string V = @"[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]+)?([Zz]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?";
        private readonly string datePattern = V;

        /// <summary>
        /// Common data context
        /// </summary>
        public CommondataContext CommondataContext { get; } = commondataContext;
        /// <summary>
        /// CoPower data context
        /// </summary>
        public Database1Context Database1Context { get; } = database1Context;

        /// <inheritdoc/>
        public string GetLatestMeasurementSQL(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new Exception("GLMS362197");

            return @$"
                SELECT
                    ""Id"", ""Date"", ""Value""
                FROM
                    ""{table}""
                ORDER BY
                    ""Date"" DESC
                LIMIT
                    1
            ";
        }

        /// <inheritdoc/>
        public string GetMeasurementsSQL(string database, string table, string startTime, string endTime)
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
            if (difference.TotalDays <= 7)
            {
                return @$"
                   SELECT
                       DATE_TRUNC('minute', ""Date"") AS ""Date"",
                       ROUND(AVG(""Value""::numeric), 1) AS ""Value""
                   FROM
                       ""{table}""
                   WHERE
                       ""Date"" BETWEEN '{startTime}' AND '{endTime}'
                   GROUP BY
                       DATE_TRUNC('minute', ""Date"")
                   ORDER BY
                       ""Date""
                ";
            }
            else if (difference.TotalDays <= 30)
            {
                return @$"
                    SELECT
                        DATE_TRUNC('hour', ""Date"") AS ""Date"",
                        ROUND(AVG(""Value""::numeric), 1) AS ""Value""
                    FROM
                        ""{table}""
                    WHERE
                        ""Date"" BETWEEN '{startTime}' AND '{endTime}'
                    GROUP BY
                        DATE_TRUNC('hour', ""Date"")
                    ORDER BY
                        ""Date""
                ";
            }
            else if (difference.TotalDays <= 365)
            {
                return @$"
                    SELECT
                        DATE_TRUNC('day', ""Date"") AS ""Date"",
                        ROUND(AVG(""Value""::numeric), 1) AS ""Value""
                    FROM
                        ""{table}""
                    WHERE
                        ""Date"" BETWEEN '{startTime}' AND '{endTime}'
                    GROUP BY
                        DATE_TRUNC('day', ""Date"")
                    ORDER BY
                        ""Date""
                ";
            }
            else
            {
                return @$"
                    SELECT
                        DATE_TRUNC('month', ""Date"") AS ""Date"",
                        ROUND(AVG(""Value""::numeric), 1) AS ""Value""
                    FROM
                        ""{table}""
                    WHERE
                        ""Date"" BETWEEN '{startTime}' AND '{endTime}'
                    GROUP BY
                        DATE_TRUNC('month', ""Date"")
                    ORDER BY
                        ""Date""
                ";
            }
        }
    }
}
