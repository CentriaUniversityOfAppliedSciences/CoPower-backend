using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.API;
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
        /// <param name="data">List of measurement data to save</param>
        /// <param name="sensorId">API key for authentication</param>
        /// <param name="userId">User Id</param>
        /// <returns></returns>
        Task<Boolean> SaveMeasurements(Guid userId, Guid sensorId, MeasurementsSaveModel data);
    }
    /// <summary>
    /// Services for Sensor Controller
    /// </summary>
    /// <remarks>
    /// Constructor for services
    /// </remarks>
    /// <param name="commonContextFactory">Common Context Factory</param>
    /// <param name="commondataContextFactory">Commondata Context Factory</param>
    /// <param name="database1APIContextFactory">Database1 API Context Factory</param>
    /// <param name="database1ContextFactory">Database1 Context Factory</param>
    /// <param name="database2APIContextFactory">Database2 API Context Factory</param>
    /// <param name="database2ContextFactory">Database2 Context Factory</param>
    /// <param name="dBQueries">Database queries</param>
    /// <param name="generalService">General services</param>
    /// <param name="settings">App settings</param>
    /// <param name="utilsService">Utils Service</param>
    public class MeasurementsService(IDbContextFactory<CommonContext> commonContextFactory, IDbContextFactory<CommondataContext> commondataContextFactory, IDbContextFactory<Database1APIContext> database1APIContextFactory, IDbContextFactory<Database1Context> database1ContextFactory, IDbContextFactory<Database2APIContext> database2APIContextFactory, IDbContextFactory<Database2Context> database2ContextFactory, IDBQueries dBQueries, IGeneralService generalService, IOptions<Settings> settings, IUtilsService utilsService) : IMeasurementsService
    {
        private readonly IDbContextFactory<CommonContext> _commonContextFactory = commonContextFactory;
        private readonly IDbContextFactory<CommondataContext> _commondataContextFactory = commondataContextFactory;
        private readonly IDbContextFactory<Database1APIContext> _database1APIContextFactory = database1APIContextFactory;
        private readonly IDbContextFactory<Database1Context> _database1ContextFactory = database1ContextFactory;
        private readonly IDbContextFactory<Database2APIContext> _database2APIContextFactory = database2APIContextFactory;
        private readonly IDbContextFactory<Database2Context> _database2ContextFactory = database2ContextFactory;

        /// <inheritdoc/>
        public async Task<List<MeasurementsHMIModel>> GetHMI(Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Measurements.GetHMI", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
                await using var database1Context = await _database1ContextFactory.CreateDbContextAsync();

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

                generalService.WriteLogMessage("api", reqid, "Measurements.GetHMI", "HMI dashboard retrieved successfully > " + HMIData.Count);
                return HMIData;
            }
            catch (Exception e)
            {
                generalService.WriteLogMessage("api", reqid, "Measurements.GetHMI", "Error occurred > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<List<dynamic>> GetMeasurements(Guid? userId, Guid sensorId, DateTime startTime, DateTime endTime)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "User.UpdateUser") ?? throw new Exception("355281");
                utilsService.CheckIfHasOrganisation(user);

                if (!utilsService.CheckTextInput(sensorId.ToString()))
                {
                    generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "Invalid sensor id input > " + sensorId.ToString());
                    throw new Exception("581319");
                }

                if (!utilsService.CheckUUID(sensorId))
                {
                    generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "Invalid sensor id uuid > " + sensorId.ToString());
                    throw new Exception("900011");
                }

                if ((endTime - startTime).TotalDays < 0)
                {
                    generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "End is later than start > " + (endTime - startTime).TotalDays);
                    throw new Exception("790563");
                }

                var sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId && s.Deleted == null && ((s.Organisation == user.Organisation) || (s.Shared > 0) || (user.Access == "appadmin"))) ?? throw new Exception("997905");

                var dbid = await commonContext.DB.FirstOrDefaultAsync(d => d.Id == sensor.DBID);
                if (dbid == null)
                {
                    generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "DBID not found > " + sensor.Id + " | " + sensor.DBID);
                    throw new Exception("852606");
                }
                var chartFetchSettings = await commonContext.ChartDataFetchSettings.FirstOrDefaultAsync(a => a.Id == dbid.ChartFetch) ?? throw new Exception("355281");
                var dbName = dbid.DBId;

                string stime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
                string etime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);

                var cresults = new List<dynamic>();

                string sqlQuery = dBQueries.GetMeasurementsSQL(chartFetchSettings, dbName, sensor.DBVALUE, stime, etime, sensor.ValueChange);

                List<MeasurementData> results = [];
                switch (dbName)
                {
                    case "commondata":
                        {
                            await using var commondataContext = await _commondataContextFactory.CreateDbContextAsync();
                            results = await commondataContext.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            await commondataContext.DisposeAsync();
                            break;
                        }
                    case "database1":
                        {
                            await using var database1Context = await _database1ContextFactory.CreateDbContextAsync();
                            results = await database1Context.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            await database1Context.DisposeAsync();
                            break;
                        }
                    case "database2":
                        {
                            await using var database2Context = await _database2ContextFactory.CreateDbContextAsync();
                            results = await database2Context.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            await database2Context.DisposeAsync();
                            break;
                        }
                    default:
                        { 
                            generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "Invalid database name > " + dbName);
                            throw new Exception("376240");
                        }
                }

                if (results != null)
                {
                    cresults.AddRange(results.Select(row => new { x = new DateTimeOffset(row.Date).ToUniversalTime(), y = row.Value }));
                }

                generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "Measurements retrieved > " + cresults.Count);
                return cresults;
            }
            catch (Exception e)
            {
                generalService.WriteLogMessage("api", reqid, "Measurements.GetMeasurements", "Error occurred > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> SaveMeasurements(Guid userId, Guid sensorId, MeasurementsSaveModel data)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Measurements.SaveMeasurements", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "User.UpdateUser") ?? throw new Exception("355281");
                utilsService.CheckIfHasOrganisation(user);

                var org = await commonContext.Organisation.FirstOrDefaultAsync(a => a.Id == user.Organisation) ?? throw new Exception("809318");

                var sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(a => a.Id == sensorId) ?? throw new Exception("345191");

                if (sensor.Organisation != user.Organisation)
                    throw new Exception("345191");

                var db = await commonContext.DB.FirstOrDefaultAsync(a => a.Id == sensor.DBID) ?? throw new Exception("345191");
                var sqlBuilder = new StringBuilder("INSERT INTO \"" + sensor.DBVALUE + "\" (\"Date\", \"Value\") VALUES ");
                var parameters = new List<object>();
                sqlBuilder.Append($"({{{parameters.Count}}}, {{{parameters.Count + 1}}})");
                parameters.Add(data.Date.ToUniversalTime());
                parameters.Add(data.Value);

                switch (db.DBId)
                {
                    case "database1":
                        {
                            await using var database1APIContext = await _database1APIContextFactory.CreateDbContextAsync();
                            var rowsAffected = database1APIContext.Database.ExecuteSqlInterpolated(
                                FormattableStringFactory.Create(sqlBuilder.ToString(), [.. parameters]));
                            await database1APIContext.DisposeAsync();
                            break;
                        }
                    case "database2":
                        {
                            await using var database2APIContext = await _database2APIContextFactory.CreateDbContextAsync();
                            var rowsAffected = database2APIContext.Database.ExecuteSqlInterpolated(
                                FormattableStringFactory.Create(sqlBuilder.ToString(), [.. parameters]));
                            await database2APIContext.DisposeAsync();
                            break;
                        }
                    default:
                        {
                            throw new Exception("´420981");
                        }
                }

                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Measurements.SaveMeasurements", "Measurement saved successfully");
                return true;
            }
            catch (Exception e)
            {
                generalService.WriteLogMessage("api", reqid, "Measurements.SaveMeasurements", "Error occurred > " + e.Message);
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
