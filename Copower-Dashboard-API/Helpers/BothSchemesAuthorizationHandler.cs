
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Authorization handler that ensures both authentication schemes succeed
/// </summary>
public sealed class BothSchemesAuthorizationHandler : AuthorizationHandler<BothSchemesRequirement>
{
    private readonly IAuthenticationService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Authorization handler constructor
    /// </summary>
    /// <param name="authService">Authorisation service</param>
    /// <param name="httpContextAccessor">Http context accessor</param>
    public BothSchemesAuthorizationHandler(
        IAuthenticationService authService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Authorization handler implementation
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="requirement">Requirement</param>
    /// <returns></returns>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BothSchemesRequirement requirement)
    {
        // Get HttpContext from resource or accessor
        var httpContext = context.Resource as HttpContext ?? _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            context.Fail();
            return;
        }

        // Authenticate against both schemes
        var resultA = await _authService.AuthenticateAsync(httpContext, requirement.SchemeA);
        var resultB = await _authService.AuthenticateAsync(httpContext, requirement.SchemeB);

        // Require both to succeed
        var bothSucceeded = resultA?.Succeeded == true && resultB?.Succeeded == true;
        if (!bothSucceeded)
        {
            // Don’t succeed; let the framework return a 401 challenge for the missing/invalid scheme(s).
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
