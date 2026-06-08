
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

/// <summary>
/// API Key Authentication Options
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Default Scheme Name
    /// </summary>
    public const string DefaultScheme = "ApiKey";
    /// <summary>
    /// Scheme Name
    /// </summary>
    public string Scheme => DefaultScheme;
    /// <summary>
    /// API Key Header Name
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "X-CoPower-API";
    /// <summary>
    /// Expected API Key
    /// </summary>
    public string? ExpectedApiKey { get; set; } = null;
}

/// <summary>
/// API Key Authentication Handler
/// </summary>
public sealed class ApiKeyAuthenticationHandler
    : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    /// <summary>
    /// API Key Authentication Handler Constructor
    /// </summary>
    /// <param name="options">Options</param>
    /// <param name="logger">Logger</param>
    /// <param name="encoder">Encoder</param>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    /// <summary>
    /// Handle Authenticate Async
    /// </summary>
    /// <returns></returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Read header
        if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var providedKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing API Key header."));
        }

        // Validate against configured key(s)
        var expectedKey =  Options.ExpectedApiKey;
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key not configured on server."));
        }

        if (!string.Equals(providedKey.ToString(), expectedKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));
        }

        // Build authenticated identity
        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
        var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationOptions.DefaultScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.DefaultScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
