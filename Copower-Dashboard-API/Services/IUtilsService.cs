using BC = BCrypt.Net.BCrypt;
using Copower_API.Context;
using Copower_API.Entities;
using Copower_API.Helpers;
using Copower_API.Models.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Copower_API.Services
{
    /// <summary>
    /// IUtilsService
    /// </summary>
    public interface IUtilsService
    {
        /// <summary>
        /// Calculates the total value by adding the standard VAT to the specified amount.
        /// </summary>
        /// <remarks>This method applies a standard VAT rate to the input value. Ensure that the input
        /// value is valid to avoid unexpected results.</remarks>
        /// <param name="value">The original amount to which VAT will be added. Must be a non-negative value.</param>
        /// <returns>The total value including VAT. Returns 0 if the input value is negative.</returns>
        double AddVATtoValue(double value);

        /// <summary>
        /// Determines whether the specified access level is granted.
        /// </summary>
        /// <param name="access">The access level to check. This value typically represents a permission or role name. Cannot be null.</param>
        /// <returns>true if the specified access level is granted; otherwise, false.</returns>
        Boolean CheckAccess(string access);

        /// <summary>
        /// Validates the authorization of the specified HTTP request and returns the associated user identifier.
        /// </summary>
        /// <param name="request">The HTTP request to check for valid authorization. Cannot be null.</param>
        /// <returns>A <see cref="Guid"/> representing the authorized user's unique identifier. Returns <see cref="Guid.Empty"/>
        /// if the request is not authorized.</returns>
        Guid CheckAuthorization(HttpRequest request);

        /// <summary>
        /// Determines whether the specified string is a valid email address format.
        /// </summary>
        /// <param name="input">The string to validate as an email address. Cannot be null.</param>
        /// <returns><see langword="true"/> if the input is a valid email address format; otherwise, <see langword="false"/>.</returns>
        Boolean CheckEmailValidity(String input);

        /// <summary>
        /// Check if user belongs to any organisation
        /// </summary>
        /// <param name="user">User data</param>
        /// <returns></returns>
        Boolean CheckIfHasOrganisation(User user);

        /// <summary>
        /// Checks the validity of the specified password and returns a code indicating the result.
        /// </summary>
        /// <remarks>The meaning of non-zero return codes depends on the implementation and may correspond
        /// to different password policy violations, such as insufficient length or missing character types.</remarks>
        /// <param name="input">The password string to validate. Cannot be null.</param>
        /// <returns>An integer code representing the result of the password validation. A value of 0 indicates the password is
        /// valid; other values indicate specific validation failures.</returns>
        int CheckPasswordValidity(String input);

        /// <summary>
        /// Determines whether the specified text input is valid according to the given length constraint.
        /// </summary>
        /// <param name="input">The text input to validate. Cannot be null.</param>
        /// <param name="length">The required length of the input. If 0, no length check is performed.</param>
        /// <returns>true if the input is valid based on the specified length; otherwise, false.</returns>
        Boolean CheckTextInput(String input, int length = 0);

        /// <summary>
        /// Determines whether the specified string is a valid universally unique identifier (UUID) in standard format.
        /// </summary>
        /// <param name="input">The string to validate as a UUID. Cannot be null.</param>
        /// <returns>true if the input string represents a valid UUID; otherwise, false.</returns>
        Boolean CheckUUID(Guid input);

        /// <summary>
        /// Generate a random string with the wanted length
        /// </summary>
        /// <param name="length">Length of the string to be generated</param>
        /// <returns>Generated random string</returns>
        String GenerateRandomString(int length);

        /// <summary>
        /// Retrieves the organisation associated with the specified organisation identifier.
        /// </summary>
        /// <param name="orgId">The unique identifier of the organisation to retrieve. Cannot be null or empty.</param>
        /// <param name="reqId">The unique identifier for the request, used for tracking or correlation purposes. Cannot be null or empty.</param>
        /// <param name="source">A string indicating the source of the request, such as the calling system or component. Cannot be null or
        /// empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Organisation"/>
        /// corresponding to the specified organisation identifier.</returns>
        Task<Organisation> GetOrganisation(Guid orgId, string reqId, string source);

        /// <summary>
        /// Retrieves the unique identifier associated with the current request.
        /// </summary>
        /// <returns>A string containing the unique request identifier. The value is guaranteed to be non-null and unique for
        /// each request context.</returns>
        string GetRequestId();

        /// <summary>
        /// Retrieves a user by the specified user identifier, with options to control request context and access to
        /// locked users.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to retrieve. If null, the method may return a default or current user,
        /// depending on implementation.</param>
        /// <param name="reqId">A unique identifier for the request, used for tracking or correlation purposes. Cannot be null or empty.</param>
        /// <param name="source">A string indicating the source of the request, such as the calling system or component. Cannot be null or
        /// empty.</param>
        /// <param name="lockedAllowed">true to allow retrieval of users who are in a locked state; otherwise, false to restrict results to unlocked
        /// users. The default is false.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user matching the specified
        /// criteria, or null if no such user exists.</returns>
        Task<User> GetUser(Guid? userId, string reqId, string source, Boolean lockedAllowed = false);

        /// <summary>
        /// Asynchronously retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve. Cannot be null or empty.</param>
        /// <param name="reqId">A unique identifier for the request, used for tracking or correlation purposes. Cannot be null or empty.</param>
        /// <param name="source">A string indicating the source of the request, such as the calling system or component. Cannot be null or
        /// empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user associated with the
        /// specified email address, or null if no user is found.</returns>
        Task<User?> GetUserByEmail(string email, string reqId, string source);

        /// <summary>
        /// Asynchronously retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve. Cannot be null or empty.</param>
        /// <param name="reqId">A unique identifier for the request, used for tracking or correlation purposes. Cannot be null or empty.</param>
        /// <param name="source">A string indicating the source of the request, such as user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user associated with the
        /// specified identifier, or null if no such user exists.</returns>
        Task<User?> GetUserById(Guid id, string reqId, string source);

        /// <summary>
        /// Retrieves a user by their unique identifier in a synchronous operation.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve. Cannot be null or empty.</param>
        /// <param name="reqId">A unique identifier for the request, used for tracking or correlation purposes. Cannot be null or empty.</param>
        /// <param name="source">A string indicating the source of the request, such as user</param>
        /// <returns>A <see cref="User"/> object representing the user with the specified identifier, or <see langword="null"/>
        /// if no matching user is found.</returns>
        Task<User?> GetUserByIdSync(Guid id, string reqId, string source);

        /// <summary>
        /// Generates a cryptographic hash of the specified password for secure storage or verification.
        /// </summary>
        /// <param name="password">The plain text password to hash. Cannot be null or empty.</param>
        /// <returns>A string containing the hashed representation of the password.</returns>
        String GeneratePasswordHash(string password);

        /// <summary>
        /// Verifies whether the specified password matches the provided hashed value.
        /// </summary>
        /// <remarks>Use this method to authenticate a user by comparing a plain text password to a
        /// previously hashed value. The method does not modify either input parameter.</remarks>
        /// <param name="password">The plain text password to verify. Cannot be null.</param>
        /// <param name="hash">The hashed password value to compare against. Cannot be null.</param>
        /// <returns><see langword="true"/> if the password matches the hash; otherwise, <see langword="false"/>.</returns>
        Boolean PasswordVerify(string password, string hash);
    }

    /// <summary>
    /// Provides utility methods for user authentication, authorization, input validation, and related operations within
    /// the application context.
    /// </summary>
    /// <remarks>This service centralizes common utility functions related to security, validation, and user
    /// management. It is intended to be used throughout the application wherever such operations are
    /// required.</remarks>
    /// <param name="commonContextFactory">The database context factory used for creating and managing database contexts.</param>
    /// <param name="generalService">A service for general-purpose operations such as logging and auxiliary tasks.</param>
    /// <param name="configuration">The application configuration provider used to access settings and secrets.</param>
    /// <param name="settings">The application settings options containing configuration values such as input constraints.</param>
    public partial class UtilsService(IDbContextFactory<CommonContext> commonContextFactory, IGeneralService generalService, IConfiguration configuration, IOptions<Settings> settings) : IUtilsService
    {
        private readonly IDbContextFactory<CommonContext> _commonContextFactory = commonContextFactory;

        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private static readonly string All = Lower + Upper + Digits;

        /// <inheritdoc/>
        public double AddVATtoValue(double value)
        {
            if (value < 0)
                return 0;

            return value + value * (settings.Value.VAT.Value / 100);
        }

        /// <inheritdoc/>
        public Boolean CheckAccess(string access)
        {
            switch (access)
            {
                case "user": case "admin": case "api": case "appadmin": { return true; }
            }
            return false;
        }

        /// <inheritdoc/>
        public Guid CheckAuthorization(HttpRequest request)
        {
            try
            {
                var token = request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
                if (token != null)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                    var userId = jsonToken?.Claims.First(claim => claim.Type == "name").Value;

                    if (userId != null)
                        return Guid.Parse(userId);
                    else
                        throw new Exception("288586");
                }
                else throw new Exception("288586");
            }
            catch (Exception)
            {
                throw new Exception("771047");
            }
        }

        /// <inheritdoc/>
        public Boolean CheckEmailValidity(String input)
        {
            if (input.Length > settings.Value.InputMax.Email)
                return false;

            if (String.IsNullOrWhiteSpace(input) == true)
                return false;

            Regex regex = EmailPatternRegex();
            return regex.IsMatch(input);
        }

        /// <inheritdoc/>
        public Boolean CheckIfHasOrganisation(User user)
        {
            if (user.Organisation != null)
            {
                return true;
            }
            else
            {
                throw new Exception("689010");
            }
        }

        /// <inheritdoc/>
        public int CheckPasswordValidity(String input)
        {
            if (String.IsNullOrWhiteSpace(input) == true)
                return 1;

            Regex patternLetterRegex = PasswordLetterPatternRegex();
            if (patternLetterRegex.IsMatch(input) == false)
                return 2;

            Regex patternNumberRegex = PasswordNumberPatternRegex();
            if (patternNumberRegex.IsMatch(input) == false)
                return 3;

            Regex patternSpecialRegex = PasswordSpecialSymbolRegex();
            if (patternSpecialRegex.IsMatch(input) == false)
                return 4;

            if ((input.Length < settings.Value.InputMax.Password_min) || (input.Length > settings.Value.InputMax.Password))
                return 5;

            return 0;
        }

        /// <inheritdoc/>
        public Boolean CheckTextInput(String input, int length = 0)
        {
            Boolean check = String.IsNullOrWhiteSpace(input) == false;

            if (check == false)
                return check;

            if ((length > 0) && (input.Length > length))
                return false;

            return check;
        }

        /// <inheritdoc/>
        public Boolean CheckUUID(Guid input)
        {
            if (input == Guid.Empty)
                return false;

            Regex regex = UUIDFormatRegex();
            return regex.IsMatch(input.ToString());
        }


        /// <inheritdoc/>
        public String GeneratePasswordHash(string password)
        {
            return BC.HashPassword(password);
        }

        /// <inheritdoc/>
        public String GenerateRandomString(int length)
        {
            var randomString = new char[length];

            // Ensure at least one of each required type
            randomString[0] = GetRandomChar(Lower);
            randomString[1] = GetRandomChar(Upper);
            randomString[2] = GetRandomChar(Digits);

            // Fill the rest randomly from all sets
            for (int i = 3; i < length; i++)
            {
                randomString[i] = GetRandomChar(All);
            }

            // Shuffle to avoid predictable positions
            Shuffle(randomString);

            return new(randomString);
        }

        /// <inheritdoc/>
        public string GenerateResetToken(string email, out DateTime expiry)
        {
            expiry = DateTime.UtcNow.AddHours(1);
            var claims = new[]
            {
                new Claim("email", email),
                new Claim("purpose", "password_reset")
            };

            var key = Encoding.ASCII.GetBytes(configuration["JWT:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <inheritdoc/>
        public async Task<Organisation> GetOrganisation(Guid orgId, string reqId, string source)
        {
            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
            var organisation = await commonContext.Organisation.FirstOrDefaultAsync(o => o.Id == orgId && o.Disabled == false && o.Deleted == null);
            if (organisation == null)
            {
                generalService.WriteLogMessage("api", reqId, source, "Invalid organisation > " + orgId);
                throw new Exception("270216");
            }
            return organisation;
        }

        /// <inheritdoc/>
        public string GetRequestId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <inheritdoc/>
        public async Task<User> GetUser(Guid? userId, string reqId, string source, Boolean lockedAllowed = false)
        {
            generalService.WriteLogMessage("utils", reqId, source, "Fetching user by Id #1 > " + userId);
            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
            if (userId == null)
            {
                generalService.WriteLogMessage("api", reqId, source, "Invalid user id > " + userId);
                throw new Exception("270215");
            }

            var user = await commonContext.User.FirstOrDefaultAsync(u => u.Id == userId && (u.Disabled == false || lockedAllowed == true) && u.Deleted == null);
            if (user == null)
            {
                generalService.WriteLogMessage("api", reqId, source, "Invalid user > " + userId);
                throw new Exception("786390");
            }

            return user;
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByEmail(string email, string reqId, string source)
        {
            generalService.WriteLogMessage("utils", reqId, source, "Fetching user by email > " + email);
            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
            if (source == "user")
            {
                return await commonContext.User.FirstOrDefaultAsync(u => u.Email == email && u.Deleted == null && u.Disabled == false && u.Registered != null);
            }
            else
                return await commonContext.User.FirstOrDefaultAsync(u => u.Email == email && u.Deleted == null && u.Disabled == false);
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserById(Guid id, string reqId, string source)
        {
            generalService.WriteLogMessage("utils", reqId, source, "Fetching user by Id #2 > " + id);
            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
            if (source == "user")
            {
                return await commonContext.User.FirstOrDefaultAsync(u => u.Id == id && u.Deleted == null && u.Disabled == false && u.Registered != null);
            }
            else
                return await commonContext.User.FirstOrDefaultAsync(u => u.Id == id && u.Deleted == null && u.Disabled == false);
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByIdSync(Guid id, string reqId, string source)
        {
            generalService.WriteLogMessage("utils", reqId, source, "Fetching user by Id #3 > " + id);
            await using var commonContext = await _commonContextFactory.CreateDbContextAsync();
            if (source == "user")
            {
                return await commonContext.User.FirstOrDefaultAsync(u => u.Id == id && u.Deleted == null && u.Disabled == false && u.Registered != null);
            }
            else
                return await commonContext.User.FirstOrDefaultAsync(u => u.Id == id && u.Deleted == null && u.Disabled == false);
        }

        /// <inheritdoc/>
        public Boolean PasswordVerify(string password, string hash)
        {
            return BC.Verify(password, hash);
        }

        [GeneratedRegex("[-A-Za-z0-9!#$%&'*+/=?^_`{|}~]+(?:\\.[-A-Za-z0-9!#$%&'*+/=?^_`{|}~]+)*@(?:[A-Za-z0-9](?:[-A-Za-z0-9]*[A-Za-z0-9])?\\.)+[A-Za-z0-9](?:[-A-Za-z0-9]*[A-Za-z0-9])?", RegexOptions.IgnoreCase, "fi-FI")]
        public static partial Regex EmailPatternRegex();
        [GeneratedRegex(@"[A-Za-z]")]
        public static partial Regex PasswordLetterPatternRegex();
        [GeneratedRegex(@"[0-9]")]
        public static partial Regex PasswordNumberPatternRegex();
        [GeneratedRegex(@"[#!\”$% &/ () =?@~`\\.\’;:+= ^*_ -]")]
        public static partial Regex PasswordSpecialSymbolRegex();
        [GeneratedRegex("[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}", RegexOptions.IgnoreCase, "fi-FI")]
        public static partial Regex UUIDFormatRegex();

        private static char GetRandomChar(string chars)
        {
            int index = RandomNumberGenerator.GetInt32(chars.Length);
            return chars[index];
        }

        private static void Shuffle(char[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}
