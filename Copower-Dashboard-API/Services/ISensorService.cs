using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Models.Sensor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using Serilog;
using System.Data;

namespace Copower_API.Services
{
    /// <summary>
    /// Services for Sensor Controller
    /// </summary>
    public interface ISensorService
    {
        /// <summary>
        /// Asynchronously adds a new sensor using the specified data and associates it with the given organization and
        /// user.
        /// </summary>
        /// <param name="sensorData">An object containing the details of the sensor to add. Cannot be null.</param>
        /// <param name="userId">The unique identifier of the user performing the operation.</param>
        /// <param name="orgId">The identifier of the organization to which the sensor will be associated. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the sensor
        /// was added successfully; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> Add(SensorAddEditModel sensorData, Guid userId, Guid orgId);

        /// <summary>
        /// Deletes the specified sensor from the organization.
        /// </summary>
        /// <param name="requesterId">The unique identifier of the user requesting the deletion. Can be null if the operation does not require
        /// user context.</param>
        /// <param name="orgId">The unique identifier of the organization that owns the sensor.</param>
        /// <param name="sensorId">The unique identifier of the sensor to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the sensor
        /// was successfully deleted; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> Delete(Guid? requesterId, Guid orgId, Guid sensorId);

        /// <summary>
        /// Updates the details of an existing sensor with the specified data.
        /// </summary>
        /// <param name="sensorData">An object containing the updated sensor information to apply. Cannot be null.</param>
        /// <param name="userId">The unique identifier of the user performing the edit operation. If null, the operation may be performed
        /// without user context.</param>
        /// <param name="orgId">The unique identifier of the organization to which the sensor belongs. Cannot be null or empty.</param>
        /// <param name="sensorId">The unique identifier of the sensor to update. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the sensor
        /// was successfully updated; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> Edit(SensorAddEditModel sensorData, Guid userId, Guid orgId, Guid sensorId);

        /// <summary>
        /// Retrieves a list of sensors that the specified user can administer.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose administrable sensors are to be retrieved. If null, retrieves
        /// sensors for the current user context.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="SensorAdminList"/>
        /// with the sensors the user can administer.</returns>
        Task<SensorAdminList> Get(Guid? userId);

        /// <summary>
        /// Retrieves a list of sensor edit records accessible to the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user for whom to retrieve sensor edit records. If null, retrieves records not
        /// associated with a specific user.</param>
        /// <param name="dashboardType">Dashboard type (default, public, user)</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of sensor edit records
        /// available to the user. The list is empty if no records are found.</returns>
        Task<List<SensorEditList>> GetEdit(Guid? userId, string dashboardType);

        /// <summary>
        /// Retrieves a list of sensors associated with the specified organisation, optionally filtered by user.
        /// </summary>
        /// <param name="organisation">The unique identifier of the organisation for which to retrieve sensors.</param>
        /// <param name="userId">The unique identifier of the user to filter sensors by, or null to retrieve sensors for all users in the
        /// organisation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an array of SensorListModel
        /// objects representing the sensors. The array is empty if no sensors are found.</returns>
        Task<SensorListModel[]> GetList(Guid organisation, Guid? userId);
    }
    /// <summary>
    /// Services for Sensor Controller
    /// </summary>
    /// <remarks>
    /// Constructor for services
    /// </remarks>
    /// <param name="commonContextFactory">Common context factory</param>
    /// <param name="commondataContextFactory">Common data context factory</param>
    /// <param name="database1ContextFactory">Database1 context factory</param>
    /// <param name="database2ContextFactory">Database2 context factory</param>
    /// <param name="generalService">General service</param>
    /// <param name="utilsService">Utils service</param>
    public class SensorService(IDbContextFactory<CommonContext> commonContextFactory, IDbContextFactory<CommondataContext> commondataContextFactory, IDbContextFactory<Database1Context> database1ContextFactory, IDbContextFactory<Database2Context> database2ContextFactory, IGeneralService generalService, IUtilsService utilsService) : ISensorService
    {
        readonly IDbContextFactory<CommonContext> _commonContextFactory = commonContextFactory;
        readonly IDbContextFactory<CommondataContext> _commondataContextFactory = commondataContextFactory;
        readonly IDbContextFactory<Database1Context> _database1ContextFactory = database1ContextFactory;
        readonly IDbContextFactory<Database2Context> _database2ContextFactory = database2ContextFactory;

        /// <inheritdoc/>
        public async Task<Boolean> Add(SensorAddEditModel sensorData, Guid userId, Guid orgId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Sensor.Add", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Sensor.Add") ?? throw new Exception("220852");
                utilsService.CheckIfHasOrganisation(user);

                if (((user.Access != "admin") && (user.Access != "appadmin")) || ((user.Access == "admin") && (user.Organisation != orgId)))
                    throw new Exception("220852");

                var sindex = sensorData.Source.IndexOf('.');
                var stable = sensorData.Source[(sindex + 1)..];
                Guid sdbindex = Guid.Parse(sensorData.Source[..sindex]);

                var sdb = await commonContext.DB.FirstOrDefaultAsync(s => s.Id == sdbindex) ?? throw new Exception("510037");
                var tableCheck = await CheckThatTableExists(sdb.DBId, stable);
                if (tableCheck == false)
                    throw new Exception("753964");

                Entities.SensorSettings newsensor = new()
                {
                    Created = DateTime.UtcNow,
                    DBID = sdb.Id ?? Guid.Empty,
                    DBVALUE = sensorData.Source[(sindex + 1)..],
                    DeviceSource = sensorData.DeviceSource,
                    Disabled = sensorData.Disabled,
                    DisplayDashboard = true,
                    Id = Guid.NewGuid(),
                    Name = sensorData.Name,
                    Organisation = orgId,
                    Shared = sensorData.Shared,
                    Unit = sensorData.Unit,
                    ValueChange = sensorData.ValueChange
                };

                await commonContext.SensorSettings.AddAsync(newsensor);
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Sensor.Add", "Sensor added successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Sensor.Add", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> Delete(Guid? requesterId, Guid orgId, Guid sensorId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Sensor.Delete", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(requesterId, reqid, "Sensor.Delete");
                utilsService.CheckIfHasOrganisation(user);

                if (((user.Access != "admin") && (user.Access != "appadmin")) || ((user.Access == "admin") && (user.Organisation != orgId)))
                    throw new Exception("220852");

                var sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId && s.Deleted == null) ?? throw new Exception("441354");
                if ((user.Access == "admin") && (sensor.Organisation != user.Organisation))
                    throw new Exception("967738");

                sensor.Deleted = DateTime.UtcNow;
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Sensor.Delete", "Sensor deleted successfully");
                return false;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Sensor.Delete", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> Edit(SensorAddEditModel sensorData, Guid userId, Guid orgId, Guid sensorId)
        {
            var reqid = utilsService.GetRequestId();
            
            try
            {
                generalService.WriteLogMessage("api", reqid, "Sensor.Edit", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Sensor.Edit");
                utilsService.CheckIfHasOrganisation(user);

                if (((user.Access != "admin") && (user.Access != "appadmin")) || ((user.Access == "admin") && (user.Organisation != orgId)))
                    throw new Exception("220852");

                var sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId);
                if ((sensor == null) || ((user.Access != "appadmin") && (user.Organisation != sensor.Organisation)))
                    throw new Exception("436259");

                var sindex = sensorData.Source.IndexOf('.');
                var stable = sensorData.Source[(sindex + 1)..];
                var sdbindex = sensorData.Source[..sindex];

                var sdb = await commonContext.DB.FirstOrDefaultAsync(s => s.Name == sdbindex) ?? throw new Exception("510037");
                if (await CheckThatTableExists(sdb.DBId, stable) == false)
                    throw new Exception("753964");

                sensor.DBID = sdb.Id ?? Guid.Empty;
                sensor.DBVALUE = stable;
                sensor.DeviceSource = sensorData.DeviceSource;
                sensor.Disabled = sensorData.Disabled;
                sensor.Name = sensorData.Name;
                sensor.Unit = sensorData.Unit;
                sensor.Updated = DateTime.UtcNow;
                sensor.ValueChange = sensorData.ValueChange;
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Sensor.Edit", "Sensor edited successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Sensor.Edit", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<SensorAdminList> Get(Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Sensor.Get", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Sensor.Get");
                utilsService.CheckIfHasOrganisation(user);

                // Check that requester is either admin or appadmin
                if ((user.Access != "admin") && (user.Access != "appadmin"))
                    throw new Exception("187870");

                var sensors = new List<SensorAdminListOrganisations>();
                var sources = new List<SourcesList>();
                var sourcesName = new List<SourcesNameList>();

                // Sensors & sources
                if (user.Access == "appadmin")
                {   
                    var dbs = await commonContext.DB.Where(a => a.DBId != "none").ToListAsync();

                    foreach (var db in dbs)
                    {
                        sources.AddRange(await GetSourceDataFromTables(db.DBId));
                        sourcesName.Add(new SourcesNameList
                        {
                            Id = db.Id ?? Guid.Empty,
                            Name = db.Name
                        });
                    }
                    sourcesName = [.. sourcesName.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)];

                    // Organisations
                    var orgs = await commonContext.Organisation.ToListAsync();
                    foreach (var org in orgs)
                    {
                        // Sensors
                        var orgSensors = new List<SensorAdminListSensors>();

                        var orgsnrs = await commonContext.SensorSettings.Where(s => s.Organisation == org.Id && s.Deleted == null).OrderBy(s => s.Name).ToListAsync();
                        foreach (var sensor in orgsnrs)
                        {
                            var sourcedb = await commonContext.DB.FirstOrDefaultAsync((s) => s.Id == sensor.DBID);
                            if (sourcedb == null)
                                continue;

                            var sensorLastData = await GetNewestDataPoint(sourcedb.DBId, sensor.DBVALUE);

                            orgSensors.Add(new SensorAdminListSensors
                            {
                                Created = sensor.Created,
                                DeviceSource = sensor.DeviceSource,
                                Disabled = sensor.Disabled ?? false,
                                Id = sensor.Id,
                                LastData = sensorLastData,
                                Name = sensor.Name,
                                Shared = sensor.Shared,
                                Source = sourcedb.Id + "." + sensor.DBVALUE,
                                Unit = sensor.Unit ?? "",
                                Updated = sensor.Updated,
                                ValueChange = sensor.ValueChange ?? 1.0
                            });
                        }

                        sensors.Add(new SensorAdminListOrganisations
                        {
                            Id = org.Id,
                            Name = org.Name,
                            Sensors = orgSensors
                        });
                    }
                }
                else if (user.Access == "admin")
                {
                    var org = await commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == user.Organisation && o.Deleted == null && o.Disabled == false) ?? throw new Exception("238941");

                    var db = await commonContext.DB.FirstOrDefaultAsync(d => d.IdNumber == org.Type && d.DBId != "none") ?? throw new Exception("573023");
                    sources.AddRange(await GetSourceDataFromTables(db.DBId));
                    sourcesName.Add(new SourcesNameList
                    {
                        Id = db.Id ?? Guid.Empty,
                        Name = db.Name
                    });
                    // Sensors
                    var orgSensors = new List<SensorAdminListSensors>();

                    var orgsnrs = await commonContext.SensorSettings.Where(s => s.Organisation == org.Id && s.Deleted == null).OrderBy(s => s.Name).ToListAsync();
                    foreach (var sensor in orgsnrs)
                    {
                        var sourcedb = await commonContext.DB.FirstOrDefaultAsync((s) => s.Id == sensor.DBID);
                        if (sourcedb == null)
                            continue;

                        var sensorLastData = await GetNewestDataPoint(sourcedb.DBId, sensor.DBVALUE);

                        orgSensors.Add(new SensorAdminListSensors
                        {
                            Created = sensor.Created,
                            DeviceSource = sensor.DeviceSource,
                            Disabled = sensor.Disabled ?? false,
                            Id = sensor.Id,
                            LastData = sensorLastData,
                            Name = sensor.Name,
                            Shared = sensor.Shared,
                            Source = sourcedb.Id + "." + sensor.DBVALUE,
                            Unit = sensor.Unit ?? "",
                            Updated = sensor.Updated,
                            ValueChange = sensor.ValueChange ?? 1.0
                        });
                    }

                    sensors.Add(new SensorAdminListOrganisations
                    {
                        Id = org.Id,
                        Name = org.Name,
                        Sensors = orgSensors
                    });
                }

                var results = new SensorAdminList
                {
                    Organisations = sensors,
                    Sources = sources,
                    SourcesName = sourcesName
                };

                generalService.WriteLogMessage("api", reqid, "Sensor.Get", "Sensor objects found > " + results.Organisations.Count);
                return results;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Sensor.Get", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<List<SensorEditList>> GetEdit(Guid? reqId, string dashboardType)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Sensor.GetEdit", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(reqId, reqid, "Sensor.Edit");
                utilsService.CheckIfHasOrganisation(user);

                var sensors = new List<SensorEditList>();
                List<Entities.SensorSettings> snrs = [];
                if (user.Access == "appadmin")
                {
                    if (dashboardType == "public")
                        snrs = [.. await commonContext.SensorSettings.Where(s => s.Deleted == null && s.Shared == 2).OrderBy(s => s.Name).ToListAsync()];
                    else
                        snrs = [.. await commonContext.SensorSettings.Where(s => s.Deleted == null).OrderBy(s => s.Name).ToListAsync()];
                }
                else
                {
                    if ((dashboardType == "default") || (dashboardType == "public"))
                    {
                        generalService.WriteLogMessage("api", reqid, "Sensor.GetEdit", "Invalid access > " + user.Id);
                        throw new Exception("348751");
                    }
                    snrs = [.. await commonContext.SensorSettings.Where(s => (s.Organisation == user.Organisation || s.Shared > 0) && s.Deleted == null).OrderBy(s => s.Name).ToListAsync()];
                }

                foreach (var sensor in snrs)
                {
                    var org = await commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == sensor.Organisation);

                    sensors.Add(new SensorEditList
                    {
                        Id = sensor.Id,
                        Organisation = org != null ? org.Name : "?",
                        Name = sensor.Name,
                        Unit = sensor.Unit ?? ""
                    });
                }

                generalService.WriteLogMessage("api", reqid, "Sensor.GetEdit", "Sensor edit objects found > " + sensors.Count);
                return sensors;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Sensor.GetEdit", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<SensorListModel[]> GetList(Guid organisation, Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Sensor.GetList", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                if (utilsService.CheckTextInput(organisation.ToString()) == false)
                {
                    generalService.WriteLogMessage("api", reqid, "Sensor.GetList", "Invalid organisation id input > " + organisation.ToString());
                    throw new Exception("890163");
                }

                if (utilsService.CheckUUID(organisation) == false)
                {
                    generalService.WriteLogMessage("api", reqid, "Sensor.GetList", "Invalid organisation id uuid > " + organisation.ToString());
                    throw new Exception("303086");
                }

                var org = await commonContext.Organisation.FirstOrDefaultAsync((o) => o.Id == organisation && o.Deleted == null);
                if (org == null)
                {
                    generalService.WriteLogMessage("api", reqid, "Sensor.GetList", "Invalid organisation > " + organisation.ToString());
                    throw new Exception("805913");
                }

                var sensors = new List<SensorListModel>();
                var sensorSettings = new List<Entities.SensorSettings>();

                if (userId == null) // Public request
                {
                    sensorSettings = [.. await commonContext.SensorSettings.Where(s => s.Organisation == org.Id && s.Shared == 2 && s.Disabled == false && s.DisplayDashboard == true && s.Deleted == null).OrderBy(s => s.Name).ToListAsync()];
                }
                else // User request
                {
                    sensorSettings = [.. await commonContext.SensorSettings.Where(s => s.Organisation == org.Id && s.Shared > 0 && s.Disabled == false && s.DisplayDashboard == true && s.Deleted == null).OrderBy(s => s.Name).ToListAsync()];
                }

                if (sensorSettings.Count == 0) // No sensors found
                    return [];

                foreach (var sensorSetting in sensorSettings)
                {
                    sensors.Add(new SensorListModel
                    {
                        Name = sensorSetting.Name,
                        Id = sensorSetting.Id,
                        Unit = sensorSetting.Unit ?? ""
                    });
                }

                generalService.WriteLogMessage("api", reqid, "Sensor.GetList", "Sensors found > " + sensors.Count);
                return [.. sensors];
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Sensor.GetList", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of source data entries by enumerating tables and their columns from the database associated
        /// with the specified source name.
        /// </summary>
        /// <remarks>The method excludes columns named "Id" and "Date" from the results. The returned list
        /// is ordered first by source name and then by table name. If an invalid source name is provided, the method
        /// returns an empty list.</remarks>
        /// <param name="source">The name of the data source to query. Valid values are "commondata", "database1" or "database2". The method
        /// selects the corresponding database connection based on this value.</param>
        /// <returns>A list of SourcesList objects representing the source and table names for each relevant column found in the
        /// specified database. Returns an empty list if an error occurs or if no matching tables or columns are found.</returns>
        async private Task<List<SourcesList>> GetSourceDataFromTables(String source)
        {
            try
            {
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
                await using var commondataContext = await _commondataContextFactory.CreateDbContextAsync();
                await using var database1Context = await _database1ContextFactory.CreateDbContextAsync();
                await using var database2Context = await _database2ContextFactory.CreateDbContextAsync();

                System.Data.Common.DbConnection? connection = null;
                switch (source)
                {
                    case "commondata":
                        {
                            connection = commondataContext.Database.GetDbConnection();
                            break;
                        }
                    case "database1":
                        {
                            connection = database1Context.Database.GetDbConnection();
                            break;
                        }
                    case "database2":
                        {
                            connection = database2Context.Database.GetDbConnection();
                            break;
                        }
                    default:
                        {
                            throw new Exception("932874");
                        }
                }

                var db = await commonContext.DB.FirstOrDefaultAsync(d => d.DBId == source) ?? throw new Exception("889341");

                await connection.OpenAsync();

                DataTable databaseSchema = connection.GetSchema("Tables");

                var sourcesResult = new List<SourcesList>();
                foreach (DataRow table in databaseSchema.Rows)
                {
                    var tableName = table["TABLE_NAME"]?.ToString();

                    if (String.IsNullOrEmpty(tableName))
                        continue;

                    DataTable tableSchema = connection.GetSchema("Columns", [null, null, tableName]);

                    foreach (DataRow column in tableSchema.Rows)
                    {
                        string? columnName = column["COLUMN_NAME"].ToString();
                        if ((String.IsNullOrEmpty(columnName) == true) || (columnName == "Id") || (columnName == "Date"))
                            continue;

                        sourcesResult.Add(new SourcesList
                        {
                            Source0 = db.Id ?? Guid.Empty,
                            Source1 = tableName
                        });
                    }
                }

                sourcesResult = [.. sourcesResult.OrderBy(i => i.Source0).ThenBy(i => i.Source1)];

                return sourcesResult;
            }
            catch (Exception e)
            {
                return [];
            }
        }

        /// <summary>
        /// Check that table exists in database
        /// </summary>
        /// <param name="dbname">Database name</param>
        /// <param name="tableName">Table name</param>
        /// <returns>False if the table does not exist, true if the table exists</returns>
        async internal Task<Boolean> CheckThatTableExists(String dbname, String tableName)
        {
            var dbs = await GetSourceDataFromTables(dbname);
            return dbs.Any(x => x.Source1 == tableName);
        }

        async internal Task<DateTimeOffset?> GetNewestDataPoint(String dbname, String tableName)
        {
            List<MeasurementData> results = [];
            var sqlQuery = $@"SELECT * FROM ""{tableName}"" ORDER BY ""Date"" DESC LIMIT 1";

            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
            await using var commondataContext = await _commondataContextFactory.CreateDbContextAsync();
            await using var database1Context = await _database1ContextFactory.CreateDbContextAsync();
            await using var database2Context = await _database2ContextFactory.CreateDbContextAsync();

            try
            {
                switch (dbname)
                {
                    case "commondata": { results = await commonContext.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync(); break; }
                    case "database1": { results = await database1Context.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync(); break; }
                    case "database2": { results = await database2Context.Set<MeasurementData>().FromSqlRaw(sqlQuery).ToListAsync(); break; }
                }

                if (results.Count == 1)
                {
                    return results[0].Date;
                }
                else return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
