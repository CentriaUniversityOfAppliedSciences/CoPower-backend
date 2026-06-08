
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Requirement that ensures both authentication schemes (API key and JWT Bearer) are used
/// </summary>
public sealed class BothSchemesRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Authentication scheme A (e.g., "ApiKey")
    /// </summary>
    public string SchemeA { get; }
    /// <summary>
    /// Authentication scheme B (e.g., "Bearer")
    /// </summary>
    public string SchemeB { get; }

    /// <summary>
    /// Allows specifying two authentication schemes that must both succeed
    /// </summary>
    /// <param name="schemeA"></param>
    /// <param name="schemeB"></param>
    public BothSchemesRequirement(string schemeA, string schemeB)
    {
        SchemeA = schemeA;
        SchemeB = schemeB;
    }
}
