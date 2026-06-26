using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Copower_API.Services
{
    /// <summary>
    /// API Services interfaces
    /// </summary>
    public interface IAPIService
    {
        /// <summary>
        /// Add new API key for the user, appadmin can add for any organisation, admin can add for their own organisation only
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="orgId">Organisation identifier</param>
        /// <returns>Boolean indicating success or failure of the operation</returns>
        public Task<Boolean> Add(Guid userId, Guid? orgId);

        /// <summary>
        /// Delete an API key from an organisation, appadmin can delete any key, admin can delete keys from their own organisation only
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="apikey">API key to delete</param>
        /// <returns>Boolean indicating success or failure of the operation</returns>
        public Task<Boolean> Delete(Guid userId, String apikey);

        /// <summary>
        /// Get initialisation data for API list
        /// </summary>
        /// <param name="userId">User identification</param>
        /// <returns>List of initialisation data</returns>
        public Task<List<APIInitModel>> GetInit(Guid userId);

        /// <summary>
        /// Gets the list of API keys
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>List of API keys</returns>
        public Task<List<APIListModel>> GetList(Guid userId);

        /// <summary>
        /// Save measurements to database. This method validates the provided API key and saves the measurement data for each sensor in the input list to the corresponding database tables based on the sensor settings. It performs various checks on the input data, such as ensuring that the API key is valid, the number of sensors and values does not exceed specified limits, and that the sensors belong to the same organization as the API key. If any validation fails, an exception is thrown with an appropriate error message. If all validations pass, the method constructs and executes SQL insert statements to save the measurement data to the respective databases. Finally, it returns true if the operation is successful.
        /// </summary>
        /// <param name="apikey">API key for authentication</param>
        /// <param name="data">List of measurement data to save</param>
        /// <returns></returns>
        Task<Boolean> SaveMeasurements(String apikey, List<APIMeasurementsSaveModel> data);
    }

    /// <summary>
    /// API Services
    /// </summary>
    /// <param name="commonContextFactory">Common context factory</param>
    /// <param name="database1APIContextFactory">Database1API context factory</param>
    /// <param name="database2APIContextFactory">Database2API context factory</param>
    /// <param name="generalService">General service</param>
    /// <param name="settings">Settings</param>
    /// <param name="utilsService">Utils Service</param>
    public partial class APIService(IDbContextFactory<CommonContext> commonContextFactory, IDbContextFactory<Database1APIContext> database1APIContextFactory, IDbContextFactory<Database2APIContext> database2APIContextFactory, IGeneralService generalService, IOptions<Settings> settings, IUtilsService utilsService) : IAPIService
    {
        private readonly IDbContextFactory<CommonContext> _commonContextFactory = commonContextFactory;
        private readonly IDbContextFactory<Database1APIContext> _database1APIContextFactory = database1APIContextFactory;
        private readonly IDbContextFactory<Database2APIContext> _database2APIContextFactory = database2APIContextFactory;

        /// <inheritdoc/>
        async public Task<Boolean> Add(Guid userId, Guid? orgId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "API.Add", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "API.Add") ?? throw new Exception("355281");
                if ((user.Access != "appadmin") && (user.Access != "admin"))
                    throw new Exception("983458");
                if ((user.Access == "admin") && (orgId != null))
                    throw new Exception("548975");

                if ((orgId == null) && (user.Organisation == null))
                    throw new Exception("923042");

                var org = (user.Access == "appadmin" ? await commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == orgId) : await commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == user.Organisation)) ?? throw new Exception("836588");

                String apikey = utilsService.GenerateRandomString(settings.Value.APITokenLength);

                API newapikey = new()
                {
                    Creator = user.Id,
                    Id = apikey,
                    Organisation = (Guid)(orgId != null ? orgId : (user.Organisation != null ? user.Organisation : Guid.Empty))
                };

                await commonContext.API.AddAsync(newapikey);
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "API.Add", "API key created successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "API.Add", "Error occured > " + e.Message);
                    throw new Exception("308459");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        async public Task<Boolean> Delete(Guid userId, String apikey)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "API.Delete", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "API.Delete") ?? throw new Exception("355281");
                if ((user.Access != "appadmin") && (user.Access != "admin"))
                    throw new Exception("983458");

                var dbkey = await commonContext.API.FirstOrDefaultAsync(a => a.Id == apikey) ?? throw new Exception("193428");
                if ((dbkey.Organisation != user.Organisation) && (user.Access != "appadmin"))
                    throw new Exception("193428");

                dbkey.Deleted = DateTime.UtcNow;
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "API.Delete", "API key deleted successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "API.Delete", "Error occured > " + e.Message);
                    throw new Exception("308459");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        async public Task<List<APIInitModel>> GetInit(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "API.GetInit", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "API.GetInit") ?? throw new Exception("355281");
                if (user.Access != "appadmin")
                    throw new Exception("983458");

                List<Organisation> data = [.. await commonContext.Organisation.Where(d => d.Deleted == null).ToListAsync()];

                List<APIInitModel> list = [];
                foreach (var org in data)
                {
                    list.Add(new APIInitModel
                    {
                        Id = org.Id ?? Guid.Empty,
                        Name = org.Name
                    });
                }

                generalService.WriteLogMessage("api", reqid, "API.GetInit", "Init data retrieved successfully");
                return list;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "API.GetInit", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }
        
        /// <inheritdoc/>
        async public Task<List<APIListModel>> GetList(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "API.GetList", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "API.GetList") ?? throw new Exception("355281");

                List<API> dbdata = [];
                if (user.Access == "appadmin")
                {
                    dbdata = [.. await commonContext.API.Where(d => d.Deleted == null).ToListAsync()];
                }
                else if (user.Access == "admin")
                {
                    dbdata = [.. await commonContext.API.Where(d => d.Deleted == null && d.Organisation == user.Organisation).ToListAsync()];
                }
                else
                    throw new Exception("983458");

                List<APIListModel> data = [];

                foreach (var item in dbdata)
                {
                    User creator = await utilsService.GetUser(item.Creator, reqid, "API.GetList");
                    Organisation? org = await commonContext.Organisation.FirstOrDefaultAsync(d => d.Id == item.Organisation);

                    data.Add(new APIListModel
                    {
                        Id = item.Id ?? "",
                        Active = item.Active,
                        Created = item.Created,
                        Creator = creator?.Name ?? "?",
                        LastUsed = item.LastUsed,
                        Organisation = org?.Name ?? "?",
                        OrganisationId = org?.Id ?? Guid.Empty
                    });
                }

                generalService.WriteLogMessage("api", reqid, "API.GetList", "API list retrieved successfully > " + data.Count);
                return data;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "API.GetList", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> SaveMeasurements(String apikey, List<APIMeasurementsSaveModel> data)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "API.SaveMeasurements", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
                await using var database1APIContext = await _database1APIContextFactory.CreateDbContextAsync();
                await using var database2APIContext = await _database2APIContextFactory.CreateDbContextAsync();

                if (apikey.Length != settings.Value.APITokenLength)
                    throw new Exception("927391");

                var akey = await commonContext.API.FirstOrDefaultAsync(a => a.Id == apikey) ?? throw new Exception("994359");
                if (data.Count > 40)
                    throw new Exception("230809");

                var org = await commonContext.Organisation.FirstOrDefaultAsync(a => a.Id == akey.Organisation) ?? throw new Exception("809318");

                foreach (var s in data)
                {
                    if (s.Values.Count > 10)
                        continue;

                    var sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(a => a.Id == s.Sensor);
                    if (sensor == null)
                        continue;

                    if (sensor.Organisation != akey.Organisation)
                        continue;

                    var db = await commonContext.DB.FirstOrDefaultAsync(a => a.Id == sensor.DBID);
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
                        default:
                            {
                                throw new Exception("´420981");
                            }
                    }
                }

                akey.LastUsed = DateTime.UtcNow;
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "API.SaveMeasurements", "Measurements saved successfully");
                return true;
            }
            catch (Exception e)
            {
                generalService.WriteLogMessage("api", reqid, "API.SaveMeasurements", "Error occurred > " + e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}
