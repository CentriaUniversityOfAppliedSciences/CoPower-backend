using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text.RegularExpressions;

namespace Copower_API.Services
{
    /// <summary>
    /// Services for Dashboard Controller
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the list of dashboard sensors available to the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose dashboard sensors are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="DashboardUIView"/> objects representing the user's dashboard sensors. The list is empty if the user
        /// has no sensors.</returns>
        Task<List<DashboardUIView>> GetDashboardSensors(Guid userId);

        /// <summary>
        /// Retrieves the list of default dashboard views available to the current user asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="DashboardUIView"/> objects representing the default dashboards. The list is empty if no default
        /// dashboards are available.</returns>
        Task<List<DashboardUIView>> GetDashboard(string dashboardType, Guid? userId);

        /// <summary>
        /// Retrieves the dashboard data associated with the specified user for the HMI interface.
        /// </summary>
        /// <remarks>The returned dashboard data may include various metrics and statistics relevant to
        /// the user's profile. Ensure that the provided userId corresponds to an existing user.</remarks>
        /// <param name="userId">The unique identifier of the user whose dashboard data is to be retrieved. This parameter must not be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of UserDashboard objects
        /// for the specified user.</returns>
        Task<List<DashboardHMI>> GetHMIDashboard(Guid userId);

        /// <summary>
        /// Updates the dashboard configuration with the specified UI edits for the given user.
        /// </summary>
        /// <param name="dashboardType">Dashboard type (default, public, user)</param>
        /// <param name="model">A list of dashboard UI edits to apply. Cannot be null or empty.</param>
        /// <param name="userId">The unique identifier of the user whose dashboard is being updated. If null, the update applies to the
        /// current user context.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the update
        /// was successful; otherwise, <see langword="false"/>.</returns>
        Task<bool> UpdateDashboard(string dashboardType, List<DashboardUIEdit> model, Guid? userId);
    }
    /// <summary>
    /// Services for User Controller
    /// </summary>
    public partial class DashboardService(IDbContextFactory<CommonContext> commonContextFactory, IGeneralService generalService, IUtilsService utilsService, IOptions<Settings> settings) : IDashboardService
    {
        private readonly IDbContextFactory<CommonContext> _commonContextFactory = commonContextFactory;

        /// <inheritdoc/>
        public async Task<List<DashboardUIView>> GetDashboardSensors(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Dashboard.GetDashboardSensors", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Dashboard.GetDashboardSensors") ?? throw new Exception("355281");
                utilsService.CheckIfHasOrganisation(user);

                var dashboard = await commonContext.DashboardDefault.FirstOrDefaultAsync(d => d.Id == user.Id.ToString());
                if ((dashboard == null) || (dashboard.Dashboard == null))
                {
                    dashboard = new DashboardDefault
                    {
                        Dashboard = [],
                        Id = user.Id.ToString()
                    };
                }

                var dashboardObjects = new List<DashboardUIView>();

                foreach (var sui in dashboard.Dashboard)
                {
                    var dashboardObject = new DashboardUIView
                    {
                        Name = sui.Name,
                        Sensors = []
                    };
                    foreach (var sobj in sui.Sensors)
                    {
                        Guid sensorIdGuid = Guid.Parse(sobj.Sensor);
                        var snr = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorIdGuid && (s.Shared == 2 || (s.Shared > 0 && s.Organisation == user.Organisation)));
                        if (snr != null)
                        {
                            dashboardObject.Sensors.Add(new UserSensorViewData
                            {
                                Color = sobj.Color,
                                Name = snr.Name,
                                Sensor = sensorIdGuid,
                                Type = sobj.Type,
                                Unit = snr.Unit ?? ""
                            });
                        }
                    }
                    dashboardObjects.Add(dashboardObject);
                }

                generalService.WriteLogMessage("api", reqid, "Dashboard.GetDashboardSensors", "Dashboard sensors retrieved successfully > " + dashboardObjects.Count);
                return dashboardObjects;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.UpdateDashboard", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        async public Task<List<DashboardUIView>> GetDashboard(string dashboardType, Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Dashboard.GetDashboardSensors", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                generalService.WriteLogMessage("api", reqid, "Dashboard.GetDashboard", "Load default dashboard");
                DashboardDefault? dashboard = null;
                User? user = null;

                switch (dashboardType)
                {
                    case "default":
                        {
                            user = (await utilsService.GetUser(userId, reqid, "Dashboard.GetDashboard") ?? throw new Exception("347543")) ?? throw new Exception("729831");
                            utilsService.CheckIfHasOrganisation(user);

                            dashboard = await commonContext.DashboardDefault.FirstOrDefaultAsync(d => d.Id == dashboardType) ?? throw new Exception("897460");
                            break;
                        }
                    case "user":
                        {
                            user = (await utilsService.GetUser(userId, reqid, "Dashboard.GetDashboard") ?? throw new Exception("347543")) ?? throw new Exception("729831");
                            utilsService.CheckIfHasOrganisation(user);

                            dashboard = await commonContext.DashboardDefault.FirstOrDefaultAsync(d => d.Id == user.Id.ToString());
                            dashboard ??= new DashboardDefault
                                {
                                    Dashboard = [],
                                    Id = user.Id.ToString()
                                };
                            break;
                        }
                    case "public":
                        {
                            dashboard = await commonContext.DashboardDefault.FirstOrDefaultAsync(d => d.Id == dashboardType) ?? throw new Exception("897461");
                            break;
                        }
                }
                
                if ((dashboard == null) || (dashboard.Dashboard == null))
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.GetDashboard", "No dashboard found > " + dashboardType);
                    throw new Exception("355281");
                }

                var dashboardObjects = new List<DashboardUIView>();

                foreach (var sui in dashboard.Dashboard)
                {
                    var dashboardObject = new DashboardUIView
                    {
                        Name = sui.Name,
                        Sensors = [],
                        Size = sui.Size
                    };
                    foreach (var sobj in sui.Sensors)
                    {
                        Guid sensorIdGuid = Guid.Parse(sobj.Sensor);
                        var snr = await GetSensor(sensorIdGuid, dashboardType, user);
                        if (snr != null)
                        {
                            dashboardObject.Sensors.Add(new UserSensorViewData
                            {
                                Color = sobj.Color,
                                Name = snr.Name + (snr.DeviceSource.Length > 0 ? " (" + snr.DeviceSource + ")" : ""),
                                Sensor = sensorIdGuid,
                                Type = sobj.Type,
                                Unit = snr.Unit ?? ""
                            });
                        }
                    }
                    dashboardObjects.Add(dashboardObject);
                }

                generalService.WriteLogMessage("api", reqid, "Dashboard.GetDashboardSensors", "Dashboard sensors retrieved successfully > " + dashboardObjects.Count);
                return dashboardObjects;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.GetDefault", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<List<DashboardHMI>> GetHMIDashboard(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Dashboard.GetHMI", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Dashboard.GetHMI");
                if (user.Access != "appadmin")
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.GetHMI", "Invalid daccess > " + user.Id + " " + user.Access);
                    throw new Exception("783455");
                }

                DashboardDefault? dbObj = await commonContext.DashboardDefault.FirstOrDefaultAsync(d => d.Id == "hmi") ?? throw new Exception("897462");
                if ((dbObj == null) || (dbObj.Dashboard == null))
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.GetHMI", "HMI dashboard not found");
                    throw new Exception("355281");
                }

                List<DashboardHMI> hmiObj = [];

                foreach (var obj in dbObj.Dashboard)
                {
                    var fObj = new DashboardHMI
                    {
                        Name = obj.Name,
                        Sensors = []
                    };
                    foreach (var subobj in obj.Sensors)
                    {
                        fObj.Sensors.Add(new HMISensorData
                        {
                            ElementType = subobj.Color,
                            Id = subobj.Sensor,
                            Name = subobj.Name,
                            Unit = subobj.Type
                        });
                    }
                    hmiObj.Add(fObj);
                }
                
                generalService.WriteLogMessage("api", reqid, "Dashboard.GetHMI", "HMI dashboard retrieved successfully > " + hmiObj.Count);
                return hmiObj;
            }
            catch (Exception e)
            {

                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.GetHMI", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateDashboard(string dashboardType, List<DashboardUIEdit> dashboard, Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Dashboard.UpdateDashboard", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Dashboard.UpdateDashboard") ?? throw new Exception("984084");
                utilsService.CheckIfHasOrganisation(user);

                if ((user.Access != "appadmin") && (dashboardType != "user"))
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.UpdateDashboard", "Invalid daccess > " + dashboardType + ", " + user.Id);
                    throw new Exception("783455");
                }

                foreach (var obj in dashboard)
                {
                    if (obj.Name.Length > settings.Value.InputMax.DashboardChartName)
                        throw new Exception("993842");

                    if (obj.Size.Length != 2)
                        throw new Exception("179489");

                    if ((obj.Size[0] < settings.Value.Dashboard.ChartSize.MinWidth) || (obj.Size[0] > settings.Value.Dashboard.ChartSize.MaxWidth) || (obj.Size[1] < settings.Value.Dashboard.ChartSize.MinHeight) || (obj.Size[1] > settings.Value.Dashboard.ChartSize.MaxHeight))
                        throw new Exception("179489");

                    foreach (var snr in obj.Sensors)
                    {
                        if ((ColorHEXRegex().IsMatch(snr.Color) == false) ||
                            (utilsService.CheckUUID(Guid.Parse(snr.Sensor)) == false) ||
                            (settings.Value.ChartTypes.Contains(snr.Type) == false))
                            throw new Exception("993843");

                        var sensor = await GetSensor(Guid.Parse(snr.Sensor), dashboardType, user) ?? throw new Exception("993844");
                        snr.Name = sensor.Name;

                        if ((user.Access != "appadmin") && (user.Organisation != sensor.Organisation))
                            throw new Exception("146850");
                    }
                }

                List<UserDashboard> dashboardUpdate = [.. dashboard.Select(obj => new UserDashboard
                {
                    Name = obj.Name,
                    Sensors = obj.Sensors,
                    Size = obj.Size
                })];

                switch (dashboardType)
                {
                    case "default":
                    case "public":
                        {
                            await commonContext.DashboardDefault
                                .Where(x => x.Id == dashboardType)
                                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Dashboard, dashboardUpdate));
                            break;
                        }
                    case "user":
                        {
                            var currentDashboard = await commonContext.DashboardDefault.FirstOrDefaultAsync(d => d.Id == user.Id.ToString());
                            if (currentDashboard == null)
                            {
                                await commonContext.DashboardDefault.AddAsync(new DashboardDefault
                                {
                                    Dashboard = dashboardUpdate,
                                    Id = user.Id.ToString() ?? throw new Exception("385100")
                                });
                            }
                            else
                            {
                                await commonContext.DashboardDefault
                                    .Where(x => x.Id == user.Id.ToString())
                                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Dashboard, dashboardUpdate));
                            }
                            break;
                        }
                }
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Dashboard.UpdateDashboard", "Dashboard updated successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Dashboard.UpdateDashboard", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        [GeneratedRegex(@"^[0-9A-Fa-f]{6}$", RegexOptions.Compiled)]
        private static partial Regex ColorHEXRegex();

        internal async Task<SensorSettings?> GetSensor(Guid sensorId, string dashboardType, User? user)
        {
            SensorSettings? sensor = null;
            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

            if (user != null)
            {
                if (user.Access == "appadmin")
                {
                    sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId);
                }
                else
                {
                    if ((dashboardType == "default") || (dashboardType == "user"))
                    {
                        sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId);
                        if (sensor != null)
                        {
                            if ((sensor.Organisation != user.Organisation) && (sensor.Shared == 0))
                                sensor = null;
                        }
                    }
                }
            }
            else
                sensor = await commonContext.SensorSettings.FirstOrDefaultAsync(s => s.Id == sensorId && s.Shared == 2);

            return sensor;
        }
    }
}
