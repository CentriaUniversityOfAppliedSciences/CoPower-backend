using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Models.Organisation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Copower_API.Services
{
    /// <summary>
    /// IOrganisationService
    /// </summary>
    public interface IOrganisationService
    {
        /// <summary>
        /// Asynchronously adds a new organisation for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to associate with the new organisation. If null, the organisation will not
        /// be linked to a specific user.</param>
        /// <param name="organisation">An object containing the details of the organisation to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// organisation was added successfully; otherwise, <see langword="false"/>.</returns>
        Task<bool> AddNew(Guid? userId, OrganisationAdd organisation);

        /// <summary>
        /// Deletes the specified user from the given organization asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete. If null, no user will be deleted.</param>
        /// <param name="orgid">The identifier of the organization from which to delete the user. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result is <see langword="true"/> if the
        /// user was successfully deleted; otherwise, <see langword="false"/>.</returns>
        Task<bool> Delete(Guid userId, Guid orgid);

        /// <summary>
        /// Updates the details of an existing organisation for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose organisation is to be edited. Cannot be null.</param>
        /// <param name="organisation">An object containing the updated organisation details. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// organisation was successfully updated; otherwise, <see langword="false"/>.</returns>
        Task<bool> Edit(Guid userId, OrganisationEdit organisation);

        /// <summary>
        /// Get list of databases for organisation management initialization. This method retrieves a list of databases that can be used for organisation management purposes. The list is typically used to populate dropdowns or selection fields in the user interface when initializing organisation management features.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>List of organisation database models</returns>
        Task<List<OrganisationInitModel>> GetInit(Guid userId);

        /// <summary>
        /// Asynchronously retrieves a list of organisations accessible to the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user for whom to retrieve accessible organisations. If null, retrieves
        /// organisations without filtering by user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of organisations
        /// accessible to the specified user. The list is empty if no organisations are found.</returns>
        Task<List<OrganisationList>> GetList(Guid userId);

        /// <summary>
        /// Updates the details of the user's organisation.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose organisation is to be edited. Cannot be null.</param>
        /// <param name="updateData">Updated data for the organisation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// organisation was successfully updated; otherwise, <see langword="false"/>.</returns>
        Task<bool> Update(Guid userId, OrganisationUpdate updateData);
    }

    /// <summary>
    /// Organisation Service
    /// </summary>
    /// <param name="commonContextFactory">Common context factory</param>
    /// <param name="generalService">General service</param>
    /// <param name="utilsService">Utils service</param>
    public class OrganisationService(IDbContextFactory<CommonContext> commonContextFactory, IGeneralService generalService, IUtilsService utilsService) : IOrganisationService
    {
        private readonly IDbContextFactory<CommonContext> _commonContextFactory = commonContextFactory;
        /// <inheritdoc/>
        /// 
        public async Task<bool> AddNew(Guid? userId, OrganisationAdd organisation)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Organisation.AddNew", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Organisation.AddNew");
                utilsService.CheckIfHasOrganisation(user);

                if (user.Access != "appadmin")
                {
                    generalService.WriteLogMessage("api", reqid, "Organisation.AddNew", "No access > " + userId);
                    throw new Exception("898511");
                }

                var db = await commonContext.DB.FirstOrDefaultAsync(a => a.IdNumber == organisation.Type) ?? throw new Exception("576301");

                var newOrganisation = new Organisation
                {
                    Created = DateTimeOffset.UtcNow,
                    Disabled = organisation.Disabled ?? false,
                    Id = Guid.NewGuid(),
                    Name = organisation.Name,
                    Type = organisation.Type
                };

                await commonContext.Organisation.AddAsync(newOrganisation);
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Organisation.AddNew", "New organisation added successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Organisation.AddNew", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        /// 
        public async Task<bool> Delete(Guid userId, Guid orgid)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Organisation.Delete", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Organisation.Delete");
                utilsService.CheckIfHasOrganisation(user);

                if (user.Access != "appadmin")
                {
                    generalService.WriteLogMessage("api", reqid, "Organisation.Delete", "No access > " + userId);
                    throw new Exception("898511");
                }

                var org = await commonContext.Organisation.FirstOrDefaultAsync((o) => o.Id == orgid && o.Deleted == null);
                if (org == null)
                {
                    generalService.WriteLogMessage("api", reqid, "Organisation.Delete", "Invalid organisation > U:" + userId + " O:" + orgid);
                    throw new Exception("805913");
                }

                string[] dlt = ["", ""];

                List<Entities.User> ousers = [.. await commonContext.User.Where(u => u.Organisation == org.Id).ToListAsync()];
                if (ousers.Count > 0)
                {
                    List<Guid?> usrs = [];
                    foreach (Entities.User u in ousers)
                        if (u.Organisation != null)
                            usrs.Add(u.Organisation);

                    dlt[0] += string.Join(',', usrs);
                }

                List<SensorSettings> osensors = [.. await commonContext.SensorSettings.Where(s => s.Organisation == org.Id).ToListAsync()];
                if (osensors.Count > 0)
                {
                    List<Guid?> snr = [];
                    foreach (SensorSettings sensor in osensors)
                        snr.Add(sensor.Organisation);

                    dlt[1] += string.Join(',', snr);
                }

                org.Deleted = DateTimeOffset.UtcNow;
                org.DeletedItems = dlt;

                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Organisation.Delete", "Organisation deleted successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Organisation.Delete", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> Edit(Guid userId, OrganisationEdit edit)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Organisation.Edit", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Organisation.Edit");
                utilsService.CheckIfHasOrganisation(user);

                if (user.Access != "appadmin")
                    utilsService.CheckIfHasOrganisation(user);

                if ((user.Access != "appadmin") && (user.Access != "admin"))
                {
                    generalService.WriteLogMessage("api", reqid, "Organisation.Edit", "No access > " + userId);
                    throw new Exception("898511");
                }

                var org = await commonContext.Organisation.FirstOrDefaultAsync((o) => o.Id == edit.Id && o.Deleted == null);
                if (org == null)
                {
                    generalService.WriteLogMessage("api", reqid, "Organisation.Edit", "Invalid organisation > " + edit.Id);
                    throw new Exception("805913");
                }

                if (user.Access == "admin")
                {
                    if (org.Id != user.Organisation)
                    {
                        generalService.WriteLogMessage("api", reqid, "Organisation.Edit", "No access to organisation > " + edit.Id);
                        throw new Exception("210281");
                    }
                    
                    org.Name = edit.Name;
                    org.Updated = DateTimeOffset.UtcNow;

                    await commonContext.SaveChangesAsync();
                } else if (user.Access == "appadmin")
                {
                    org.Disabled = edit.Disabled;
                    org.Name = edit.Name;
                    org.Updated = DateTimeOffset.UtcNow;

                    await commonContext.SaveChangesAsync();
                }

                generalService.WriteLogMessage("api", reqid, "Organisation.Edit", "Organisation edited successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Organisation.Edit", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<List<OrganisationInitModel>> GetInit(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Organisation.GetInit", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Organisation.GetInit") ?? throw new Exception("200502");
                utilsService.CheckIfHasOrganisation(user);

                if ((user.Access != "appadmin") && (user.Access != "admin"))
                    throw new Exception("339745");

                var org = await commonContext.Organisation.FirstOrDefaultAsync(a => a.Id == user.Organisation) ?? throw new Exception("762268");

                var dbList = new List<OrganisationInitModel>();
                if (user.Access == "appadmin")
                {
                    var dbs = await commonContext.DB.Where(a => a.IdNumber != null).ToListAsync();
                    
                    foreach (var d in dbs)
                    {
                        dbList.Add(new OrganisationInitModel
                        {
                            Id = d.IdNumber,
                            Name = d.Name
                        });
                    }
                }
                else
                {
                    dbList.Add(new OrganisationInitModel
                    {
                        Name = org.Name
                    });
                }

                generalService.WriteLogMessage("api", reqid, "Organisation.GetInit", "Request success > " + dbList.Count);
                return dbList;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Organisation.GetInit", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<List<OrganisationList>> GetList(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Organisation.GetList", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Organisation.GetList") ?? throw new Exception("200502");
                utilsService.CheckIfHasOrganisation(user);

                if (user.Access != "appadmin")
                    throw new Exception("339745");

                var orgs = new List<Organisation>();
                var rorgs = new List<OrganisationList>();
                
                if (user.Access == "appadmin")
                    orgs = [.. await commonContext.Organisation.OrderBy(o => o.Name).Where(o => o.Deleted == null).ToListAsync()];
                else
                    orgs = [.. await commonContext.Organisation.OrderBy(o => o.Name).Where(o => o.Id == user.Organisation && o.Disabled == false && o.Deleted == null).ToListAsync()];

                if (orgs.Count > 0)
                {
                    foreach (var o in orgs)
                    {
                        rorgs.Add(new OrganisationList
                        {
                            Created = o.Created ?? DateTimeOffset.UtcNow,
                            Disabled = o.Disabled ?? false,
                            Id = o.Id,
                            Name = o.Name,  
                            Type = o.Type,
                            Updated = o.Updated
                        });
                    }
                }

                generalService.WriteLogMessage("api", reqid, "Organisation.GetList", "Organisations retrieved > " + rorgs.Count);
                return rorgs;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Organisation.GetList", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> Update(Guid userId, OrganisationUpdate updateData)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Organisation.Update", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Organisation.Update");
                utilsService.CheckIfHasOrganisation(user);

                if ((user.Access != "appadmin") && (user.Access != "admin"))
                {
                    generalService.WriteLogMessage("api", reqid, "Organisation.Update", "No access > " + userId);
                    throw new Exception("898511");
                }

                var org = await commonContext.Organisation.FirstOrDefaultAsync((o) => o.Id == user.Organisation && o.Deleted == null);
                if (org == null)
                {
                    generalService.WriteLogMessage("api", reqid, "Organisation.Update", "Invalid organisation > " + updateData.Name);
                    throw new Exception("805913");
                }

                org.Name = updateData.Name;
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Organisation.Update", "Organisation updated successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                    generalService.WriteLogMessage("api", reqid, "Organisation.Update", "Error occured > " + e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}
