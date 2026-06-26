using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Copower_API.Services
{
    /// <summary>
    /// Services for User Controller
    /// </summary>
    public interface IUsersService
    {
        /// <summary>
        /// Adds a new user to the specified organization.
        /// </summary>
        /// <param name="requesterId">The unique identifier of the user making the request. Can be null if the operation is performed by a system
        /// process.</param>
        /// <param name="orgId">The unique identifier of the organization to which the user will be added.</param>
        /// <param name="add">An object containing the details of the user to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user was
        /// added successfully; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> Add(Guid? requesterId, Guid orgId, UsersAdd add);

        /// <summary>
        /// Deletes the specified user from the given organization.
        /// </summary>
        /// <param name="requesterId">The unique identifier of the user requesting the deletion. Can be null if the operation does not require
        /// authentication.</param>
        /// <param name="orgId">The unique identifier of the organization from which the user will be deleted.</param>
        /// <param name="userId">The unique identifier of the user to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user was
        /// successfully deleted; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> Delete(Guid? requesterId, Guid orgId, Guid userId);

        /// <summary>
        /// Updates the specified user's administrative details within the given organization.
        /// </summary>
        /// <param name="requesterId">The unique identifier of the user making the request. Can be null if the operation is performed by a system
        /// process.</param>
        /// <param name="orgId">The unique identifier of the organization to which the user belongs.</param>
        /// <param name="userId">The unique identifier of the user whose details are to be updated.</param>
        /// <param name="edit">An object containing the updated administrative information for the user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the update
        /// was successful; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> Edit(Guid? requesterId, Guid orgId, Guid userId, UsersEditAdmin edit);

        /// <summary>
        /// Retrieves a list of all users, optionally filtered by a specific user identifier.
        /// </summary>
        /// <param name="userId">An optional user identifier to filter the results. If specified, only the user matching the given identifier
        /// is included; otherwise, all users are returned.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UserListModel"/>
        /// with the list of users matching the filter criteria. The list is empty if no users are found.</returns>
        Task<UserListModel> GetAllUsers(Guid? userId);

        /// <summary>
        /// Resends an invitation email to a user who has been invited but has not yet registered.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who will receive the invitation.</param>
        /// <param name="resendUserId">The unique identifier of the user whose invitation will be resent.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the invitation was successfully resent; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> ResendInvitation(Guid userId, Guid resendUserId);
    }
    /// <summary>
    /// Services for User Controller
    /// </summary>
    public class UsersService(IDbContextFactory<CommonContext> commonContextFactory, IConfiguration configuration, IGeneralService generalService, IUtilsService utilsService, IEmailService emailService, IOptions<Settings> settings) : IUsersService
    {
        private readonly IDbContextFactory<CommonContext> _commonContextFactory = commonContextFactory;

        /// <inheritdoc/>
        public async Task<Boolean> Add(Guid? requesterId, Guid orgId, UsersAdd addData)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Users.Add", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(requesterId, reqid, "Users.Add");
                utilsService.CheckIfHasOrganisation(user);

                // Check that requester is either admin or appadmin
                if ((user.Access != "admin") && (user.Access != "appadmin"))
                    throw new Exception("187870");

                // Only appadmin can create appadmins
                if ((user.Access == "admin") && (addData.Access == "appadmin"))
                    throw new Exception("187870");

                // Check that requester is in the same organisation if admin
                if ((user.Access == "admin") && (user.Organisation != orgId))
                    throw new Exception("358228");

                // Check inputs
                if (utilsService.CheckEmailValidity(addData.Email) == false)
                    throw new Exception("474019");

                if (utilsService.CheckTextInput(addData.Name, settings.Value.InputMax.Name) == false)
                    throw new Exception("474019");

                if (utilsService.CheckAccess(addData.Access) == false)
                    throw new Exception("474019");

                var existingUser = await utilsService.GetUserByEmail(addData.Email, reqid, "register");
                if (existingUser != null)
                {
                    if (existingUser.Organisation == null)
                    {
                        existingUser.Organisation = orgId;
                        await commonContext.SaveChangesAsync();
                        generalService.WriteLogMessage("api", reqid, "Users.Add", "User added successfully #1");
                        return true;
                    }
                    else
                        throw new Exception("178743");
                }
                else
                {
                    await CreateUser(addData, orgId);
                    generalService.WriteLogMessage("api", reqid, "Users.Add", "User added successfully #2");
                    return true;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Users.Add", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> Delete(Guid? requesterId, Guid orgId, Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Users.Delete", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(requesterId, reqid, "Users.Delete");
                utilsService.CheckIfHasOrganisation(user);

                // Check that requester is either admin or appadmin
                if ((user.Access != "admin") && (user.Access != "appadmin"))
                    throw new Exception("187870");

                // Disable self deletion
                if (requesterId == userId)
                    throw new Exception("571383");

                // Check that requester is in the same organisation if admin
                if ((user.Access == "admin") && (user.Organisation != orgId))
                    throw new Exception("358228");

                var deleteUser = await utilsService.GetUser(userId, reqid, "Users.Delete", true);

                // Check if the user is in the same organisation if admin
                if ((user.Access == "admin") && (deleteUser.Organisation == user.Organisation))
                    throw new Exception("890935");

                //deleteUser.Deleted = DateTimeOffset.UtcNow;
                deleteUser.Organisation = null;

                await commonContext.SaveChangesAsync();
                
                generalService.WriteLogMessage("api", reqid, "Users.Delete", "User deleted successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Users.Delete", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> Edit(Guid? requesterId, Guid orgId, Guid userId, UsersEditAdmin edit)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Users.Edit", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(requesterId, reqid, "Users.Edit");
                utilsService.CheckIfHasOrganisation(user);

                // Check that requester is either admin or appadmin
                if ((user.Access != "admin") && (user.Access != "appadmin"))
                    throw new Exception("187870");

                // Disable self edit
                if (requesterId == userId)
                    throw new Exception("571383");

                // Check that requester is in the same organisation if admin
                if ((user.Access == "admin") && (user.Organisation != orgId))
                    throw new Exception("358228");

                // Check inputs
                if (utilsService.CheckEmailValidity(edit.Email) == false)
                    throw new Exception("474019");

                if (utilsService.CheckTextInput(edit.Name, settings.Value.InputMax.Name) == false)
                    throw new Exception("474019");

                if (utilsService.CheckAccess(edit.Access) == false)
                    throw new Exception("474019");

                var editUser = await utilsService.GetUser(userId, reqid, "Users.Edit", true);

                // Check if the user is in the same organisation if admin
                if ((user.Access == "admin") && (editUser.Organisation == user.Organisation))
                    throw new Exception("890935");

                editUser.Access = edit.Access;
                editUser.Email = edit.Email;
                editUser.Name = edit.Name;
                editUser.Disabled = edit.Disabled;
                editUser.Updated = DateTime.UtcNow;

                commonContext.Update(editUser);
                await commonContext.SaveChangesAsync();

                generalService.WriteLogMessage("api", reqid, "Users.Edit", "User updated successfully");
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Users.Edit", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<UserListModel> GetAllUsers(Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "Users.GetAllUsers", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Users.GetAllUsers");
                utilsService.CheckIfHasOrganisation(user);

                var rlist = new UserListModel
                {
                    Organisations = [],
                    Users = []
                };

                if (user.Access == "appadmin")
                {
                    var orgs = await commonContext.Organisation.Where(o => o.Deleted == null).ToListAsync();

                    foreach (Organisation o in orgs)
                    {
                        rlist.Organisations.Add(new UserListOrganisationsModel
                        {
                            Id = o.Id,
                            Name = o.Name
                        });
                    }

                    List<User> users = [.. await commonContext.User.Where(u => u.Deleted == null).OrderByDescending(u => u.Name).ToListAsync()];

                    foreach (User u in users)
                    {
                        rlist.Users.Add(new UserListUsersModel
                        {
                            Access = u.Access,
                            Created = u.Created,
                            Disabled = u.Disabled ?? false,
                            Email = u.Email,
                            Id = u.Id,
                            LastLogin = u.LastLogin,
                            Organisation = u.Organisation ?? Guid.Empty,
                            Registered = u.Registered != null,
                            Updated = u.Updated,
                            Username = u.Name ?? ""
                        });
                    }
                }
                else if (user.Access == "admin")
                {
                    var org = await commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == user.Organisation && o.Deleted == null);
                    if (org == null)
                    {
                        generalService.WriteLogMessage("api", reqid, "Users.GetAlUsers", "Invalid organisation for user > " + userId);
                        throw new Exception("907092");
                    }

                    rlist.Organisations.Add(new UserListOrganisationsModel
                    {
                        Id = org.Id,
                        Name = org.Name
                    });

                    List<User> users = [.. await commonContext.User.Where(u => u.Organisation == user.Organisation && u.Deleted == null).OrderByDescending(u => u.Name).ToListAsync()];

                    foreach (User u in users)
                    {
                        rlist.Users.Add(new UserListUsersModel
                        {
                            Access = u.Access,
                            Created = u.Created,
                            Disabled = u.Disabled ?? false,
                            Email = u.Email,
                            Id = u.Id,
                            LastLogin = u.LastLogin,
                            Organisation = u.Organisation ?? Guid.Empty,
                            Registered = u.Registered != null,
                            Updated = u.Updated,
                            Username = u.Name ?? ""
                        });
                    }
                }

                generalService.WriteLogMessage("api", reqid, "Users.GetAllUsers", "Retrieved all users successfully");
                return rlist;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Users.GetAllUsers", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> ResendInvitation(Guid userId, Guid resendUserId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("users", reqid, "Users.ResendInvitation", "New request");
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var user = await utilsService.GetUser(userId, reqid, "Users.GetAllUsers");
                utilsService.CheckIfHasOrganisation(user);

                var org = await commonContext.Organisation.FirstOrDefaultAsync(a => a.Id == user.Organisation) ?? throw new Exception("307849");

                var resendUser = await utilsService.GetUser(resendUserId, reqid, "Users.ResendInvitation");
                if (resendUser == null)
                {
                    generalService.WriteLogMessage("users", reqid, "Users.ResendInvitation", "User to resend invitation to not found > " + userId + " > " + resendUserId);
                    throw new Exception("907092");
                }
                if (resendUser.Registered != null)
                {
                    generalService.WriteLogMessage("users", reqid, "Users.ResendInvitation", "User to resend invitation to is already registered > " + userId + " > " + resendUserId);
                    throw new Exception("907092");
                }

                if ((user.Access != "appadmin") && (user.Organisation != resendUser.Organisation))
                {
                    generalService.WriteLogMessage("users", reqid, "Users.ResendInvitation", "User is not appadmin and Resend user is on different organisation > " + userId + " > " + resendUser);
                    throw new Exception("907092");
                }

                // generate reset token and send reset email for new user
                String resetToken = await CreateResetToken(resendUser.Id);

                var mailReqId = Guid.NewGuid().ToString();
                var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? throw new Exception("293080");
                var resetLink = $"{frontendBaseUrl}/register/{resetToken}";

                try
                {
                    Boolean emailSent = await emailService.PrepareSendEmail(mailReqId, resendUser.Id, "resend_invite", "universal", new Dictionary<string, string> { { "[sender]", user.Name }, { "[organisation]", org.Name }, { "[link]", resetLink } });

                    if (emailSent)
                    {
                        generalService.WriteLogMessage("users", reqid, "Users.ResendInvitation", "Invitation resent successfully");
                        return true;
                    }
                    else
                        throw new Exception("143781");
                }
                catch (Exception e)
                {
                    generalService.WriteLogMessage("users", reqid, "Users.ResendInvitation", "Email sending error: " + e.Message);
                    throw new Exception("442100");
                }
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("users", reqid, "Users.ResendInvitation", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        private async Task<String> CreateResetToken(Guid userId)
        {
            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

            ResetTokens resetToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = utilsService.GenerateRandomString(settings.Value.ResetPasswordTokenLength),
                Expiry = DateTimeOffset.UtcNow.AddHours(1) // Token valid for 1 hour
            };

            await commonContext.ResetTokens.AddAsync(resetToken);
            await commonContext.SaveChangesAsync();

            return resetToken.Token;
        }

        /// <summary>
        /// Creates a new user in the specified organization and sends a password reset email to the user's email
        /// address.
        /// </summary>
        /// <remarks>The method generates a password reset token for the new user and sends a reset link
        /// to the provided email address. The reset token is valid for one hour. The method will throw exceptions if
        /// required configuration values are missing or if email sending fails.</remarks>
        /// <param name="user">An object containing the details of the user to create, including access level, email address, and name. The
        /// email address must not be null.</param>
        /// <param name="orgId">The unique identifier of the organization to which the new user will be added.</param>
        /// <returns>true if the user was created and the password reset email was sent successfully; otherwise, an exception is
        /// thrown.</returns>
        /// <exception cref="Exception">Thrown if there is an error generating the password reset token, sending the email, or if required
        /// configuration values are missing.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the email address of the user is null.</exception>
        private async Task<Boolean> CreateUser(UsersAdd user, Guid orgId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                await using var commonContext = await _commonContextFactory.CreateDbContextAsync();

                var org = await commonContext.Organisation.FirstOrDefaultAsync(a => a.Id == orgId) ?? throw new Exception("307849");

                var newUser = new User
                {
                    Access = user.Access,
                    Created = DateTime.UtcNow,
                    Email = user.Email.ToLower().Trim(),
                    Name = user.Name,
                    Password = "",
                    Organisation = orgId
                };

                await commonContext.User.AddAsync(newUser);
                await commonContext.SaveChangesAsync();

                // generate reset token and send reset email for new user
                String resetToken = await CreateResetToken(newUser.Id);

                var mailReqId = Guid.NewGuid().ToString();
                var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? throw new Exception("293080");
                var resetLink = $"{frontendBaseUrl}/register/{resetToken}";

                try
                {
                    Boolean emailSent = await emailService.PrepareSendEmail(mailReqId, newUser.Id, "create_user", "universal", new Dictionary<string, string> { { "[sender]", user.Name }, { "[organisation]", org.Name }, { "[link]", resetLink } });

                    if (emailSent)
                        return true;
                    else
                        throw new Exception("143781");
                }
                catch (Exception e)
                {
                    generalService.WriteLogMessage("api", reqid, "Users.CreateUser", "Email sending error: " + e.Message);
                    throw new Exception("442100");
                }
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "Users.CreateUser", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }
    }
}
