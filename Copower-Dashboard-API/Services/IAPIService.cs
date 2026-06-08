using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

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
    }

    /// <summary>
    /// API Services
    /// </summary>
    /// <param name="commonContext"></param>
    /// <param name="generalService"></param>
    /// <param name="settings"></param>
    /// <param name="utilsService"></param>
    public partial class APIService(CommonContext commonContext, IGeneralService generalService, IOptions<Settings> settings, IUtilsService utilsService) : IAPIService
    {
        /// <inheritdoc/>
        async public Task<Boolean> Add(Guid userId, Guid? orgId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                var user = await utilsService.GetUser(userId, reqid, "API.Add") ?? throw new Exception("355281");
                if ((user.Access != "appadmin") && (user.Access != "admin"))
                    throw new Exception("983458");
                if ((user.Access == "admin") && (orgId != null))
                    throw new Exception("548975");

                if ((orgId == null) && (user.Organisation == null))
                    throw new Exception("923042");

                var org = (user.Access == "appadmin" ? commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == orgId) : commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == user.Organisation)) ?? throw new Exception("836588");

                String apikey = utilsService.GenerateRandomString(settings.Value.APITokenLength);

                API newapikey = new()
                {
                    Creator = user.Id,
                    Id = apikey,
                    Organisation = (Guid)(orgId != null ? orgId : (user.Organisation != null ? user.Organisation : Guid.Empty))
                };

                commonContext.API.Add(newapikey);
                await commonContext.SaveChangesAsync();

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
                var user = await utilsService.GetUser(userId, reqid, "API.Delete") ?? throw new Exception("355281");
                if ((user.Access != "appadmin") && (user.Access != "admin"))
                    throw new Exception("983458");

                var dbkey = commonContext.API.FirstOrDefault(a => a.Id == apikey) ?? throw new Exception("193428");
                if ((dbkey.Organisation != user.Organisation) && (user.Access != "appadmin"))
                    throw new Exception("193428");

                dbkey.Deleted = DateTime.UtcNow;
                await commonContext.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "API.GetList", "Error occured > " + e.Message);
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
                var user = await utilsService.GetUser(userId, reqid, "API.Delete") ?? throw new Exception("355281");
                if (user.Access != "appadmin")
                    throw new Exception("983458");

                List<Organisation> data = [.. commonContext.Organisation.Where(d => d.Deleted == null)];

                List<APIInitModel> list = [];
                foreach (var org in data)
                {
                    list.Add(new APIInitModel
                    {
                        Id = org.Id ?? Guid.Empty,
                        Name = org.Name
                    });
                }

                return list;
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
        async public Task<List<APIListModel>> GetList(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                var user = await utilsService.GetUser(userId, reqid, "API.GetList") ?? throw new Exception("355281");

                List<API> dbdata = [];
                if (user.Access == "appadmin")
                {
                    dbdata = [.. commonContext.API.Where(d => d.Deleted == null)];
                }
                else if (user.Access == "admin")
                {
                    dbdata = [.. commonContext.API.Where(d => d.Deleted == null && d.Organisation == user.Organisation)];
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
    }
}
