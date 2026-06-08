using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Copower_API.Services
{
    /// <summary>
    /// Services for User Controller
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticates a user based on the provided credentials.
        /// </summary>
        /// <param name="model">An object containing the user's authentication credentials. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an AuthUserModel representing
        /// the authenticated user if authentication is successful; otherwise, null.</returns>
        Task<AuthUserModel> Authenticate(AuthenticateModel model);

        /// <summary>
        /// Asynchronously verifies the authentication status of a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to check. Specify <see langword="null"/> to check the current
        /// authenticated user, if applicable.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AuthUserModel"/>
        /// representing the authenticated user if found; otherwise, <see langword="null"/>.</returns>
        Task<AuthUserModel?> Check(Guid? userId);

        /// <summary>
        /// Retrieves the authenticated user associated with the specified user identifier, if authentication is
        /// successful.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to authenticate. Cannot be null or empty.</param>
        /// <returns>A <see cref="Entities.User"/> object representing the authenticated user if authentication succeeds;
        /// otherwise, <see langword="null"/>.</returns>
        Task<Entities.User?> CheckAuth(Guid userId);

        /// <summary>
        /// Deletes the user with the specified unique identifier asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete. If null, the operation will not be performed.</param>
        /// <param name="password">Password of the user to verify the deletion</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result is <see langword="true"/> if the
        /// user was successfully deleted; otherwise, <see langword="false"/>.</returns>
        Task<bool> DeleteUser(Guid id, string password);

        /// <summary>
        /// Retrieves the user profile associated with the specified user identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose profile is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user profile model for the
        /// specified user.</returns>
        Task<UserProfileModel> Get(Guid userId);

        /// <summary>
        /// Resets the user's password using the specified reset information.
        /// </summary>
        /// <param name="model">An object containing the user's reset credentials and new password details. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the password
        /// was successfully reset; otherwise, <see langword="false"/>.</returns>
        Task<bool> ResetPassword(ResetPasswordModel model);

        /// <summary>
        /// Initiates the password reset process for the user associated with the specified email address.
        /// </summary>
        /// <param name="email">The email address of the user requesting a password reset. Cannot be null or empty.</param>
        /// <param name="language">The language code to use for the password reset communication (for example, "en" for English). Determines
        /// the language of the email sent to the user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the password
        /// reset process was successfully initiated; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> ForgotPassword(String email, String language);

        /// <summary>
        /// Updates the profile information for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update. Cannot be null.</param>
        /// <param name="user">An object containing the updated profile information for the user. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the update
        /// was successful; otherwise, <see langword="false"/>.</returns>
        Task<Boolean> UpdateUser(Guid userId, UserProfileUpdateModel user);

        /// <summary>
        /// Validates a password reset token and returns the associated user identifier if the token is valid.
        /// </summary>
        /// <param name="token">The password reset token to validate. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user identifier as a string
        /// if the token is valid; otherwise, null.</returns>
        Task<String> ValidateResetToken(string token);
    }
    /// <summary>
    /// Services for User Controller
    /// </summary>
    public partial class UserService(CommonContext commonContext, IConfiguration configuration, IGeneralService generalService, IUtilsService utilsService, IEmailService emailService, IOptions<Settings> settings) : IUserService
    {
        /// <inheritdoc/>
        async public Task<AuthUserModel> Authenticate(AuthenticateModel model)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                generalService.WriteLogMessage("api", reqid, "User.Authenticate", "New request");

                if (utilsService.CheckEmailValidity(model.Email) == false) // Check email
                {
                    generalService.WriteLogMessage("api", reqid, "User.Authenticate", "Invalid email > " + model.Email);
                    throw new Exception("239089");
                }

                var pwcheck = utilsService.CheckPasswordValidity(model.Password);
                if (pwcheck > 0) // Check password
                {
                    generalService.WriteLogMessage("api", reqid, "User.Authenticate", "Invalid password > E" + pwcheck + " > " + model.Email);
                    throw new Exception("239089");
                }

                var dbUser = await commonContext.User.SingleOrDefaultAsync(u => u.Email == model.Email.Trim() && u.Deleted == null && u.Registered != null) ?? throw new Exception("239089"); // Find user

                if (dbUser.Disabled == true) // User is disabled
                {
                    generalService.WriteLogMessage("api", reqid, "User.Authenticate", "User account is disabled > " + dbUser.Id);
                    throw new Exception("767551");
                }

                if (utilsService.PasswordVerify(model.Password, dbUser.Password) == false) // Verify password
                {
                    if (dbUser.Disabled == false)
                    {
                        dbUser.FailedLogins++; // Increment failed login counter
                        if (dbUser.FailedLogins >= 5) // Disable account if max failure count has been exceeded
                        {
                            dbUser.Disabled = true;
                            dbUser.FailedLogins = 0;
                        }
                        commonContext.User.Update(dbUser);
                        commonContext.SaveChanges();
                    }

                    generalService.WriteLogMessage("api", reqid, "User.Authenticate", "Password failure > " + dbUser.Id);
                    throw new Exception("239089");
                }
                else
                {
                    dbUser.LastLogin = DateTime.UtcNow;
                    dbUser.FailedLogins = 0;
                    commonContext.SaveChanges();
                }

                // Update last login
                dbUser.LastLogin = DateTime.UtcNow;
                commonContext.SaveChanges();

                /*if (dbUser.Access != "appadmin")
                {
                    if (dbUser.Organisation != null)
                    {
                        var dbOrg = await commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == dbUser.Organisation && o.Deleted == null) ?? throw new Exception("205907");
                    }
                    else
                    {
                        generalService.WriteLogMessage("api", reqid, "User.Authenticate", "No organisation > " + dbUser.Id);
                        throw new Exception("425051");
                    }
                }*/

                var validUser = new AuthUserModel
                {
                    Name = dbUser.Name ?? "",
                    Token = GenerateJWT(dbUser, reqid)
                };

                generalService.WriteLogMessage("api", reqid, "User.Authenticate", "Request success > " + dbUser.Id);
                return validUser; // authentication successful
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.Authenticate", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<AuthUserModel?> Check(Guid? userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                if (userId == null)
                    return null;

                var user = await utilsService.GetUserById((Guid)userId, "user");
                if (user != null)
                {
                    // Update last login
                    user.LastLogin = DateTime.UtcNow;
                    commonContext.SaveChanges();

                    var validUser = new AuthUserModel
                    {
                        Name = user.Name ?? "",
                        Token = GenerateJWT(user, reqid)
                    };

                    return validUser;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.Check", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Entities.User?> CheckAuth(Guid userId)
        {
            var user = await utilsService.GetUserById(userId, "user");
            if (user != null)
                return user;
            else
                return null;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteUser(Guid userId, string password)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                var user = await utilsService.GetUserById(userId, "user");
                if (user == null)
                    return false;

                if (utilsService.PasswordVerify(password, user.Password) == true) // Verify password
                {
                    user.Deleted = DateTimeOffset.UtcNow;
                    await commonContext.SaveChangesAsync();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.DeleteUser", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> ForgotPassword(String email, String language)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                var user = await utilsService.GetUserByEmail(email, "user") ?? throw new Exception("798710");

                // generate password reset token
                var resetTokenId = Guid.NewGuid();
                var expiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

                var tokenHandler = new JwtSecurityTokenHandler();
                var JWTsecret = configuration.GetSection("JWT");
                var secretValue = JWTsecret["Secret"];
                if (string.IsNullOrEmpty(secretValue))
                {
                    generalService.WriteLogMessage("api", reqid, "User.ForgotPassword", "JWT generation error");
                    throw new Exception("442100");
                }

                var key = Encoding.ASCII.GetBytes(secretValue);
                var tokenDescription = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                    [
                        new Claim("email", user.Email),
                        new Claim("purpose", "password_reset") // extra safety to distinguish from login token
                    ]),
                    Expires = DateTime.UtcNow.AddHours(settings.Value.ForgottenPasswordTokenExpire),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescription);
                var resetToken = tokenHandler.WriteToken(token);


                var resetTokenEntry = new ResetTokens
                {
                    Created = DateTime.UtcNow,
                    Id = resetTokenId,
                    UserId = user.Id,
                    Token = resetToken,
                    Expiry = expiry
                };

                commonContext.ResetTokens.Add(resetTokenEntry);
                await commonContext.SaveChangesAsync();

                var mailReqId = Guid.NewGuid().ToString();
                var resetLink = $"{settings.Value.ServerPath.Server}{settings.Value.ServerPath.Dashboard}reset-password?token={resetToken}";

                Boolean emailSent = await emailService.PrepareSendEmail(mailReqId, user.Id, "password_reset", language, new Dictionary<string, string> { { "<RESET_LINK>", resetLink } });

                if (emailSent)
                    return true;
                else
                    throw new Exception("143781");
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.ForgotPassword", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<UserProfileModel> Get(Guid userId)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                var user = await utilsService.GetUserById(userId, "user") ?? throw new Exception("239089");
                return new UserProfileModel
                {
                    Name = user.Name ?? "",
                    Email = user.Email,
                };
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.Get", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ResetPassword(ResetPasswordModel model)
        {
            var reqid = utilsService.GetRequestId();

            try
            {
                // Find the reset token
                var resetToken = await commonContext.ResetTokens.FirstOrDefaultAsync(a => a.Token == model.Token && a.Used == null && a.Expiry > DateTimeOffset.UtcNow) ?? throw new Exception("847399");

                // Verify the user exists
                var user = await utilsService.GetUserById(resetToken.UserId, "password_reset") ?? throw new Exception("847339");

                user.Password = utilsService.GeneratePasswordHash(model.NewPassword);
                user.Registered = DateTime.UtcNow;
                resetToken.Used = DateTime.UtcNow;
                await commonContext.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.ResetPassword", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Boolean> UpdateUser(Guid userId, UserProfileUpdateModel user)
        {
            var reqid = utilsService.GetRequestId();
            try
            {
                var existingUser = await utilsService.GetUserById(userId, "User.UpdateUser") ?? throw new Exception("355281A");
                if (utilsService.PasswordVerify(user.CurrentPassword, existingUser.Password) == false)
                    throw new Exception("355281B");

                if (String.IsNullOrWhiteSpace(user.NewPassword) == false)
                    existingUser.Password = utilsService.GeneratePasswordHash(user.NewPassword);

                existingUser.Email = user.Email;
                existingUser.Name = user.Name;
                existingUser.Updated = DateTime.UtcNow;
                await commonContext.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.UpdateUser", "Error occured > " + e.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(e.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<String> ValidateResetToken(string token)
        {
            var reqid = utilsService.GetRequestId();
            try
            {
                generalService.WriteLogMessage("api", reqid, "User.ValidateResetToken", $"Attempting to validate token.");

                // Find the reset token
                var resetToken = await commonContext.ResetTokens.FirstOrDefaultAsync(a => a.Token == token && a.Used == null && a.Expiry > DateTimeOffset.UtcNow) ?? throw new Exception("847399");

                // Verify the user exists
                var user = await commonContext.User.FirstOrDefaultAsync(u => u.Id == resetToken.UserId && u.Deleted == null && u.Registered == null) ?? throw new Exception("847339");

                generalService.WriteLogMessage("api", reqid, "User.ValidateResetToken", "Token successfully validated.");
                return user.Email;
            }
            catch (Exception ex)
            {
                if (ex.Message.Length > 6)
                {
                    generalService.WriteLogMessage("api", reqid, "User.ValidateResetToken", "Error occured > " + ex.Message);
                    throw new Exception("174361");
                }
                else
                    throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified user using application configuration settings.
        /// </summary>
        /// <remarks>The generated JWT includes claims for the user's identifier, organisation (if
        /// available), and access role. The token's audience, issuer, and expiration are determined by application
        /// configuration settings. Ensure that the configuration contains valid JWT settings before calling this
        /// method.</remarks>
        /// <param name="user">The user for whom the JWT is to be generated. The user's access level and identifier must not be null.</param>
        /// <param name="reqid">The unique request identifier used for logging and tracing the token generation process.</param>
        /// <returns>A string containing the generated JWT for the specified user.</returns>
        /// <exception cref="Exception">Thrown if the user's access level or identifier is null, or if the JWT secret is not configured.</exception>
        private string GenerateJWT(Entities.User user, string reqid)
        {
            if (user.Access == null)
                throw new Exception("873423");

            var tokenHandler = new JwtSecurityTokenHandler();
            var JWTsecret = configuration.GetSection("JWT");

            var secretValue = JWTsecret["Secret"];
            if (string.IsNullOrEmpty(secretValue) == true)
            {
                generalService.WriteLogMessage("api", reqid, "User.Authenticate", "JWT generation error");
                throw new Exception("442100");
            }

            int authExpireDays = (int)Math.Truncate(settings.Value.AuthTokenExpire);
            double authExpireHours = (settings.Value.AuthTokenExpire - authExpireDays) * 10;

            var key = Encoding.ASCII.GetBytes(secretValue);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Audience = JWTsecret["Audience"],
                Subject = new ClaimsIdentity(
                [
                    new Claim("name", user.Id.ToString()),
                    user.Organisation is not null ? new Claim("organisation", user.Organisation.ToString() ?? "") : null!,
                    new Claim("role", user.Organisation is not null ? user.Access : "none")
                ]),
                Expires = DateTime.UtcNow.AddDays(authExpireDays).AddHours(authExpireHours),
                Issuer = JWTsecret["Issuer"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescription);
            var jwt = tokenHandler.WriteToken(token); 

            return jwt;
        }
    }
}
