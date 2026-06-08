using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.Organisation;
using Copower_API.Models.Public;
using Copower_API.Models.Sensor;
using Copower_API.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace Copower_API.Services
{
    /// <summary>
    /// Public service interface
    /// </summary>
    public interface IPublicService
    {
        /// <summary>
        /// Retrieves a list of measurement records for a specified sensor within a given time range, optionally
        /// filtered by user.
        /// </summary>
        /// <param name="sensorId">The unique identifier of the sensor for which to retrieve measurements.</param>
        /// <param name="startTime">The start of the time range for which to retrieve measurements. Only measurements recorded at or after this
        /// time are included.</param>
        /// <param name="endTime">The end of the time range for which to retrieve measurements. Only measurements recorded before or at this
        /// time are included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of dynamic objects
        /// representing the measurements that match the specified criteria. The list is empty if no measurements are
        /// found.</returns>
        Task<List<dynamic>> GetMeasurements(Guid sensorId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Retrieves a list of organisations asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of <see
        /// cref="OrganisationListModel"/> objects representing the available organisations. The array is empty if no
        /// organisations are found.</returns>
        Task<List<DashboardUIView>> GetPublicDashboard();
    }

    /// <summary>
    /// Public service implementation
    /// </summary>
    /// <param name="commonContext">Common database context</param>
    /// <param name="commondataContext">Common data context</param>
    /// <param name="database1Context">Database1 context</param>
    /// <param name="dBQueries">Database queries</param>
    /// <param name="generalService">General service</param>
    /// <param name="settings">Settings</param>
    /// <param name="utilsService">Utilities service</param>
    public class PublicService(CommonContext commonContext, CommondataContext commondataContext, Database1Context database1Context, IDBQueries dBQueries, IGeneralService generalService, IOptions<Settings> settings, IUtilsService utilsService) : IPublicService
    {
        /// <inheritdoc/>
        public async Task<List<dynamic>> GetMeasurements(Guid sensorId, DateTime startTime, DateTime endTime)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                Log.Information("api", reqid, "Public.GetMeasurements", "New request");

                if (!utilsService.CheckTextInput(sensorId.ToString()))
                {
                    Log.Information("api", reqid, "Public.GetMeasurements", "Invalid sensor id input > " + sensorId.ToString());
                    throw new Exception("581319");
                }

                if (!utilsService.CheckUUID(sensorId))
                {
                    Log.Information("api", reqid, "Public.GetMeasurements", "Invalid sensor id uuid > " + sensorId.ToString());
                    throw new Exception("900011");
                }

                if ((endTime - startTime).TotalDays < 0)
                {
                    Log.Information("api", reqid, "Public.GetMeasurements", "End is later than start > " + (endTime - startTime).TotalDays);
                    throw new Exception("790563");
                }

                string dbname = "";

                var sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId && s.Deleted == null && s.Shared == 2) ?? throw new Exception("997905");

                var dbid = await commonContext.DB.FirstOrDefaultAsync(d => d.Id == sensor.DBID);
                if (dbid == null)
                {
                    Log.Information("api", reqid, "Public.GetMeasurements", "DBID not found > " + sensor.Id + " | " + sensor.DBID);
                    throw new Exception("852606");
                }

                dbname = dbid.Name;

                string stime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
                string etime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);

                var cresults = new List<dynamic>();

                string sqlQuery = dBQueries.GetMeasurementsSQL(dbname, sensor.DBVALUE, stime, etime);

                switch (dbname)
                {
                    case "commondata":
                        {
                            var results = await commondataContext.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            if (settings.Value.VAT.RequiredMeasurements.Contains(sensor.DBVALUE) == true)
                            {
                                foreach (var row in results)
                                {
                                    row.Value = (float)Math.Round(utilsService.AddVATtoValue(row.Value), 1);
                                }
                            }
                            cresults.AddRange(results.Select(row => new { x = new DateTimeOffset(row.Date).ToUnixTimeSeconds(), y = row.Value }));
                            break;
                        }
                    case "database1":
                        {
                            var results = await database1Context.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync();
                            cresults.AddRange(results.Select(row => new { x = new DateTimeOffset(row.Date).ToUnixTimeSeconds(), y = row.Value }));
                            break;
                        }
                    default:
                        {
                            Log.Information("api", reqid, "Public.GetMeasurements", "Invalid database name > " + dbname);
                            throw new Exception("376240");
                        }
                }

                Log.Information("api", reqid, "Public.GetMeasurements", "Request success > " + cresults.Count);
                return cresults;
            }
            catch (Exception e)
            {
                Log.Information("api", reqid, "Public.GetMeasurements", "Error occurred > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Get organisations which have public sensors
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<DashboardUIView>> GetPublicDashboard()
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Public.GetPublicDashboard", "Load public dashboard");
                var defaultDashboard = await commonContext.DashboardDefault.FirstOrDefaultAsync(d => d.Id == "public") ?? throw new Exception("355281");
                if (defaultDashboard.Dashboard == null)
                {
                    generalService.WriteLogMessage("api", reqid, "Public.GetPublicDashboard", "No public dashboard found");
                    throw new Exception("355281");
                }

                var dashboardObjects = new List<DashboardUIView>();

                foreach (var sui in defaultDashboard.Dashboard)
                {
                    var dashboardObject = new DashboardUIView
                    {
                        Name = sui.Name,
                        Sensors = []
                    };
                    foreach (var sobj in sui.Sensors)
                    {
                        var snr = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == Guid.Parse(sobj.Sensor) && s.Shared == 2);
                        if (snr != null)
                        {
                            dashboardObject.Sensors.Add(new UserSensorViewData
                            {
                                Color = sobj.Color,
                                Name = snr.Name + (snr.DeviceSource.Length > 0 ? " (" + snr.DeviceSource + ")" : ""),
                                Sensor = Guid.Parse(sobj.Sensor),
                                Type = sobj.Type,
                                Unit = snr.Unit ?? ""
                            });
                        }
                    }
                    dashboardObjects.Add(dashboardObject);
                }

                return dashboardObjects;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Public.GetPublicDashboard", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }
    }
}
