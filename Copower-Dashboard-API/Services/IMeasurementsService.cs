using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.Measurements;
using Copower_API.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.Runtime.CompilerServices;
using System.Text;

namespace Copower_API.Services
{
    /// <summary>
    /// Services for Measurements Controller
    /// </summary>
    public interface IMeasurementsService
    {
        /// <summary>
        /// Gets HMI view measurements. This method retrieves the latest measurement data for each sensor defined in the application settings for HMI,
        /// </summary>
        /// <param name="userId">Requesting user Id</param>
        /// <returns></returns>
        Task<List<MeasurementsHMIModel>> GetHMI(Guid? userId);

        /// <summary>
        /// Retrieves a list of measurement records for a specified sensor within a given time range, optionally
        /// filtered by user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to filter measurements by. Specify null to retrieve measurements for all
        /// users.</param>
        /// <param name="sensorId">The unique identifier of the sensor for which to retrieve measurements.</param>
        /// <param name="startTime">The start of the time range for which to retrieve measurements. Only measurements recorded at or after this
        /// time are included.</param>
        /// <param name="endTime">The end of the time range for which to retrieve measurements. Only measurements recorded before or at this
        /// time are included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of dynamic objects
        /// representing the measurements that match the specified criteria. The list is empty if no measurements are
        /// found.</returns>
        Task<List<dynamic>> GetMeasurements(Guid? userId, Guid sensorId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Save measurements to database. This method validates the provided API key and saves the measurement data for each sensor in the input list to the corresponding database tables based on the sensor settings. It performs various checks on the input data, such as ensuring that the API key is valid, the number of sensors and values does not exceed specified limits, and that the sensors belong to the same organization as the API key. If any validation fails, an exception is thrown with an appropriate error message. If all validations pass, the method constructs and executes SQL insert statements to save the measurement data to the respective databases. Finally, it returns true if the operation is successful.
        /// </summary>
        /// <param name="apikey">API key for authentication</param>
        /// <param name="data">List of measurement data to save</param>
        /// <returns></returns>
        Task<Boolean> SaveMeasurements(String apikey, List<MeasurementsSaveModel> data);
        
    }
    /// <summary>
    /// Services for Sensor Controller
    /// </summary>
    /// <remarks>
    /// Constructor for services
    /// </remarks>
    /// <param name="commonContext">Common Context</param>
    /// <param name="commondataContext">Commondata Context</param>
    /// <param name="database1APIContext">Database1 API Context</param>
    /// <param name="database1Context">Database1 Context</param>
    /// <param name="database2APIContext">Database2 API Context</param>
    /// <param name="database2Context">Database2 Context</param>
    /// <param name="dBQueries">Database queries</param>
    /// <param name="generalService">General services</param>
    /// <param name="settings">App settings</param>
    /// <param name="utilsService">Utils Service</param>
    public class MeasurementsService(CommonContext commonContext, CommondataContext commondataContext, Database1APIContext database1APIContext, Database1Context database1Context, Database2APIContext database2APIContext, Database2Context database2Context, IDBQueries dBQueries, IGeneralService generalService, IOptions<Settings> settings, IUtilsService utilsService) : IMeasurementsService
    {
        /// <inheritdoc/>
        public async Task<List<MeasurementsHMIModel>> GetHMI(Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                var user = await utilsService.GetUser(userId, reqid, "Measurements.GetHMI");
                if (user.Access != "appadmin")
                {
                    generalService.WriteLogMessage("api", reqid, "Measurements.GetHMI", "Invalid daccess > " + user.Id + " " + user.Access);
                    throw new Exception("783455");
                }

                List<MeasurementsHMIModel> HMIData = [];
                DateTime hourAgo = DateTime.Now.AddHours(-1);

                foreach (var sensor in settings.Value.HMIMeasurements)
                {
                    var deviceId = sensor.Table.Split("-")[0];
                    var testi = sensor.Table;
                    var HMIObj = HMIData.FirstOrDefault(h => h.Id == deviceId);
                    if (HMIObj == null)
                    {
                        HMIObj = new MeasurementsHMIModel
                        {
                            Id = deviceId,
                            Updated = null,
                            Sensors = []
                        };
                        HMIData.Add(HMIObj);
                    }

                    var mdata = await database1Context.Set<MeasurementData>().FromSqlRaw(dBQueries.GetLatestMeasurementSQL(sensor.Table)).ToListAsync();
                    if ((mdata != null) && (mdata.Count > 0))
                    {
                        if (hourAgo.CompareTo(mdata[0].Date) < 0) // Check if value is too old
                        {
                            HMIObj.Sensors.Add(new MeasurementsHMIDataModel { Sensor = sensor.UI, Value = null });
                            continue;
                        }
                        else if (HMIObj.Updated != null)
                        {
                            if (mdata[0].Date > HMIObj.Updated)
                                HMIObj.Updated = mdata[0].Date;
                        }
                        else { HMIObj.Updated = mdata[0].Date; }
                        HMIObj.Sensors.Add(new MeasurementsHMIDataModel { Sensor = sensor.UI, Value = (float?)Math.Round(ConvertValueUnit(sensor, mdata[0].Value), 1) });
                    }
                    else
                    {
                        HMIObj.Sensors.Add(new MeasurementsHMIDataModel { Sensor = sensor.UI, Value = null });
                    }
                }

                return HMIData;
            }
            catch (Exception e)
            {
                Log.Information("api", reqid, "Measurements.GetHMI", "Error occurred > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<List<dynamic>> GetMeasurements(Guid? userId, Guid sensorId, DateTime startTime, DateTime endTime)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                Log.Information("api", reqid, "Measurements.GetMeasurements", "New request");

                var user = await utilsService.GetUser(userId, reqid, "User.UpdateUser") ?? throw new Exception("355281");
                utilsService.CheckIfHasOrganisation(user);

                if (!utilsService.CheckTextInput(sensorId.ToString()))
                {
                    Log.Information("api", reqid, "Measurements.GetMeasurements", "Invalid sensor id input > " + sensorId.ToString());
                    throw new Exception("581319");
                }

                if (!utilsService.CheckUUID(sensorId))
                {
                    Log.Information("api", reqid, "Measurements.GetMeasurements", "Invalid sensor id uuid > " + sensorId.ToString());
                    throw new Exception("900011");
                }

                if ((endTime - startTime).TotalDays < 0)
                {
                    Log.Information("api", reqid, "Measurements.GetMeasurements", "End is later than start > " + (endTime - startTime).TotalDays);
                    throw new Exception("790563");
                }

                string dbname = "";

                var sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId && s.Deleted == null && ((s.Organisation == user.Organisation) || (s.Shared > 0) || (user.Access == "appadmin"))) ?? throw new Exception("997905");

                var dbid = await commonContext.DB.FirstOrDefaultAsync(d => d.Id == sensor.DBID);
                if (dbid == null)
                {
                    Log.Information("api", reqid, "Measurements.GetMeasurements", "DBID not found > " + sensor.Id + " | " + sensor.DBID);
                    throw new Exception("852606");
                }

                dbname = dbid.DBId;

                string stime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
                string etime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);

                var cresults = new List<dynamic>();

                string sqlQuery = dBQueries.GetMeasurementsSQL(dbname, sensor.DBVALUE, stime, etime);

                List<MeasurementData>? results = null;
                switch (dbname)
                {
                    case "commondata":
                        {
                            results = await commondataContext.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            break;
                        }
                    case "database1":
                        {
                            results = await database1Context.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            break;
                        }
                    case "database2":
                        {
                            results = await database2Context.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            break;
                        }
                    default:
                        { 
                            Log.Information("api", reqid, "Measurements.GetMeasurements", "Invalid database name > " + dbname);
                            throw new Exception("376240");
                        }
                }

                if (results != null)
                {
                    if (sensor.ValueChange != 1)
                    {
                        foreach (var row in results)
                        {
                            row.Value *= sensor.ValueChange ?? 1.0;
                        }
                    }
                    cresults.AddRange(results.Select(row => new { x = new DateTimeOffset(row.Date).ToUnixTimeSeconds(), y = row.Value }));
                }

                Log.Information("api", reqid, "Measurements.GetMeasurements", "Request success > " + cresults.Count);
                return cresults;
            }
            catch (Exception e)
            {
                Log.Information("api", reqid, "Measurements.GetMeasurements", "Error occurred > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> SaveMeasurements(String apikey, List<MeasurementsSaveModel> data)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                Log.Information("api", reqid, "Measurements.SaveMeasurements", "New request");

                if (apikey.Length != settings.Value.APITokenLength)
                    throw new Exception("927391");

                var akey = commonContext.API.FirstOrDefault(a => a.Id == apikey) ?? throw new Exception("994359");
                if (data.Count > 30)
                    throw new Exception("230809");

                var org = commonContext.Organisation.FirstOrDefault(a => a.Id == akey.Organisation) ?? throw new Exception("809318");

                foreach (var s in data)
                {
                    if (s.Values.Count > 100)
                        continue;

                    var sensor = commonContext.SensorSettings.FirstOrDefault(a => a.Id == s.Sensor);
                    if (sensor == null)
                        continue;

                    if (sensor.Organisation != akey.Organisation)
                        continue;

                    var db = commonContext.DB.FirstOrDefault(a => a.Id == sensor.DBID);
                    if (db == null)
                        continue;

                    var sqlBuilder = new StringBuilder("INSERT INTO \"" + sensor.DBVALUE + "\" (\"Date\", \"Value\") VALUES ");
                    var parameters = new List<object>();

                    for (int i = 0; i < s.Values.Count; i++)
                    {
                        var u = s.Values[i];

                        sqlBuilder.Append($"({{{parameters.Count}}}, {{{parameters.Count + 1}}})");

                        parameters.Add(u.Date.ToUniversalTime());
                        parameters.Add(u.Value);

                        if (i < s.Values.Count - 1)
                            sqlBuilder.Append(", ");
                    }

                    Console.WriteLine($"Params: {parameters.Count} {string.Join(", ", parameters)}");
                    Console.WriteLine(sqlBuilder.ToString());

                    switch (db.DBId)
                    {
                        case "database1":
                            {
                                var rowsAffected = database1APIContext.Database.ExecuteSqlInterpolated(
                                    FormattableStringFactory.Create(sqlBuilder.ToString(), [.. parameters]));
                                break;
                            }
                        case "database2":
                            {
                                var rowsAffected = database2APIContext.Database.ExecuteSqlInterpolated(
                                    FormattableStringFactory.Create(sqlBuilder.ToString(), [.. parameters]));
                                break;
                            }
                    }
                }

                akey.LastUsed = DateTime.UtcNow;
                await commonContext.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                Log.Information("api", reqid, "Measurements.SaveMeasurements", "Error occurred > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Convert value unit based on sensor settings, if Divide is set, divide the value by it, otherwise return original value
        /// </summary>
        /// <param name="sensor">Sensor object</param>
        /// <param name="value">Measurement value</param>
        /// <returns>Measurement value divided by the division value, if there is no division value then return the original value back</returns>
        private static double ConvertValueUnit(HMIMeasurement? sensor, double value)
        {
            if ((sensor == null) || (sensor.Divide == null))
                return value;
            return value / (double)sensor.Divide;
        }
    }
}
