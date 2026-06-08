using Copower_API.Context;
using Copower_API.Helpers;
using Copower_API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using System.Security.Claims;
using System.Text;

// Version 0.6
var logDir = Path.Combine(AppContext.BaseDirectory, "log");

if (Directory.Exists(logDir) == false)
{
    Directory.CreateDirectory(logDir); // idempotent, works on Windows & Linux
    Console.WriteLine("Log directory created > " + logDir);
}

var logFile = Path.Combine(logDir, "log.log");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logFile,
        fileSizeLimitBytes: 10_000_000,
        retainedFileCountLimit: 60,
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
var CorsPolicyName = "CorsPolicy";
IWebHostEnvironment env = builder.Environment;
IServiceCollection services = builder.Services;
ConfigurationManager configuration = builder.Configuration;

services.Configure<RouteOptions>(options => options.LowercaseUrls = true); // Lowercase all routes

services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPath;
});

// Set up Serilog to log to file and console
builder.Host.UseSerilog();

services.AddCors(options =>
{
    options.AddPolicy(
        CorsPolicyName,
        policy =>
        {
            policy.WithOrigins("http://localhost:5100",
                               "http://localhost:8100",
                               "http://localhost:5173",
                               "https://copower.westeurope.cloudapp.azure.com") //local 
                .AllowAnyHeader()
                .WithMethods("DELETE", "GET", "POST", "PUT")
                .AllowCredentials();
        });
});

// Load configuration variables
builder.Configuration
    .AddJsonFile("env.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"env.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.
services.AddControllers();

services.AddDbContext<CommonContext>();
services.AddDbContext<CommondataContext>();
services.AddDbContext<Database1Context>();
services.AddDbContext<Database1APIContext>();
services.AddDbContext<Database2Context>();
services.AddDbContext<Database2APIContext>();

services.AddScoped<IAPIService, APIService>();
services.AddScoped<IDashboardService, DashboardService>();
services.AddScoped<IDBQueries, DBQueries>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IGeneralService, GeneralService>();
services.AddScoped<IMeasurementsService, MeasurementsService>();
services.AddScoped<IOrganisationService, OrganisationService>();
services.AddScoped<IPublicService, PublicService>();
services.AddScoped<ISensorService, SensorService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IUsersService, UsersService>();
services.AddScoped<IUtilsService, UtilsService>();

//builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IAuthorizationHandler, BothSchemesAuthorizationHandler>();

services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value != null && e.Key != "$" && e.Value.Errors.Count > 0)
            .Select(e => new
            {
                Error = e.Value!.Errors,
                Field = e.Key
            }).ToArray();

        return new BadRequestObjectResult(new
        {
            Errors = errors,
            Message = "Validation errors occurred."
        });
    };
});

//services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
services.AddMvc(options => options.EnableEndpointRouting = false);
var appSettingsSection = configuration.GetSection("AppSettings");
var JWTsecret = configuration.GetSection("JWT");
if (JWTsecret == null)
{
    Console.WriteLine("JWT section missing in the env");
    Environment.Exit(1);
}
if (JWTsecret["Secret"] == null || JWTsecret["Issuer"] == null || JWTsecret["Audience"] == null)
{
    Console.WriteLine("JWT section incomplete in the env");
    Environment.Exit(1);
}
services.Configure<AppSettings>(appSettingsSection);
services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
var appSettings = appSettingsSection.Get<AppSettings>();

if (appSettings == null)
{
    Console.WriteLine("AppSettings error");
    Environment.Exit(1);
}

var clientAPIkey = appSettings.ClientAPIKey;

// Authentication
// API Key
services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.DefaultScheme, options =>
        {
            options.ApiKeyHeaderName = "X-CoPower-API";
            options.ExpectedApiKey = clientAPIkey; // Set the expected API key from appsettings
        });

// JWT Bearer
services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                //Console.WriteLine("JWT Check");
                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                var userIdClaim = claimsIdentity?.FindFirst("name")?.Value;

                if (userIdClaim != null)
                {
                    var user = userService.CheckAuth(Guid.Parse(userIdClaim));
                    if (user == null)
                        context.Fail("Unauthorized");
                }
                else
                    context.Fail("Unauthorized");

                return Task.CompletedTask;
            }
        };
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidIssuer = JWTsecret["Issuer"],
            ValidAudience = JWTsecret["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JWTsecret["Secret"] ?? "error")),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

var test = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    ValidIssuer = JWTsecret["Issuer"],
    ValidAudience = JWTsecret["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JWTsecret["Secret"] ?? "error")),
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true
};

services.AddAuthentication(options =>
    {
        // Make a single "default" that delegates to the right scheme based on the request
        options.DefaultScheme = "ApiKeyAndJwt";
        options.DefaultChallengeScheme = "ApiKeyAndJwt";
    })
    .AddPolicyScheme("ApiKeyAndJwt", "API Key and JWT", policy =>
    {
        policy.ForwardDefaultSelector = context =>
        {
            var hasApiKey = context.Request.Headers.ContainsKey("X-CoPower-API");
            var authHeader = context.Request.Headers.Authorization.ToString();
            var isBearer = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true;

            if (isBearer && hasApiKey)
            {
                // If both are present, we can choose either one.
                // Here we choose JWT Bearer by default.
                return JwtBearerDefaults.AuthenticationScheme;
            }
            return "error"; // No default scheme, let the framework decide based on the request
        };
    });

// Authorization policy (optional; you can use [Authorize] without a named policy too)
services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyAndJwt", policy =>
    {
        // This policy does NOT just list schemes (which is OR). It adds a custom requirement.
        policy.AddRequirements(new BothSchemesRequirement(ApiKeyAuthenticationOptions.DefaultScheme, JwtBearerDefaults.AuthenticationScheme));
    });

    // Require specifically API Key
    options.AddPolicy("ApiKey", policy =>
    {
        policy.AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme);
        policy.RequireAuthenticatedUser();
    });
});

services.AddSignalR();
services.AddScoped<IUserService, UserService>();

builder.Logging.AddDebug(); // Enable debug logging
builder.Logging.AddConsole(); // Enable console logging
services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<SecuritySchemesTransformer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Server running in development");
    app.MapOpenApi(); // Enable OpenAPI support
    /*app.UseSwaggerUI(options => // Configure Swagger UI to use this document
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CoPower v1");
        options.RoutePrefix = "swagger"; // UI at /swagger
    }); // Enable Swagger UI*/
    //app.UseDeveloperExceptionPage();
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "Centria CoPower API v1";
        opt.SortOperationsByMethod()
            .SortTagsAlphabetically()
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        /*opt.AddPreferredSecuritySchemes("Apikey").AddApiKeyAuthentication("Apikey", apiKey =>
        {
            apiKey.Value = "apikey";
        });*/
    });

    Console.WriteLine("Development settings set");
}

if (appSettings.LoggingActive == true)
    app.UseHttpLogging();

//dataContext.Database.Migrate();

using (var scope = app.Services.CreateAsyncScope())
{
    var commonContext = scope.ServiceProvider.GetRequiredService<CommonContext>();
    commonContext.Database.EnsureCreated();

    var copowerContext = scope.ServiceProvider.GetRequiredService<Database1Context>();
    commonContext.Database.EnsureCreated();
}

app.Use(async (context, next) =>
{
    if (appSettings.BasePath.Length > 0)
    {
        if (context.Request.Path.StartsWithSegments(appSettings.BasePath, out var remainingPath))
        {
            context.Request.Path = remainingPath;
        }
    }
    await next();
});

app.UseHsts();
app.UseHttpsRedirection();

//app.UseMiddleware<HeaderValidationMiddleware>(appSettings.ClientAPIKey);

app.UseRouting();

app.UseCors(CorsPolicyName);

//app.UseMiddleware<HeaderValidationMiddleware>(appSettings.ClientAPIKey);

//app.UseMvc();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<HeaderValidationMiddleware>(appSettings.ClientAPIKey);

//app.UseCors(CorsPolicyName);

app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Check request for API key
/// </summary>
/// <param name="next">Next</param>
/// <param name="apikey">API key</param>
public class HeaderValidationMiddleware(RequestDelegate next, string apikey)
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Check request header
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path == null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing required path");
            return;
        }

        // Allow Scalar UI and OpenAPI JSON without API key
        if (path.StartsWith("/scalar") || path.StartsWith("/openapi"))
        {
            await _next(context);
            return;
        }

        // Validate API key for other endpoints
        if (!context.Request.Headers.TryGetValue("X-CoPower-API", out StringValues headerValue) ||
            !string.Equals(headerValue.ToString(), apikey, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized; // 401 Unauthorized
            await context.Response.WriteAsync("Missing or invalid API key");
            return;
        }
        await _next(context);
    }
}

internal sealed class SecuritySchemesTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider = authenticationSchemeProvider;

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(a => a.Name == "Bearer"))
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            };

            document.Components ??= new OpenApiComponents();

            document.AddComponent("Bearer", bearerScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            };

            foreach (var path in document.Paths.Values)
            {
                if (path.Operations == null) continue;
                foreach (var operation in path.Operations.Values)
                {
                    operation.Security ??= [];
                    operation.Security.Add(securityRequirement);
                }
            }
        }

        // Add API Key
        if (authenticationSchemes.Any(a => a.Name == "ApiKey"))
        {
            var apikeyScheme = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "X-CoPower-API",
                Scheme = ApiKeyAuthenticationOptions.DefaultScheme,
                Type = SecuritySchemeType.ApiKey
            };

            document.Components ??= new OpenApiComponents();

            document.AddComponent("APIKey", apikeyScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("API Key", document)] = []
            };

            foreach (var path in document.Paths.Values)
            {
                if (path.Operations == null) continue;
                foreach (var operation in path.Operations.Values)
                {
                    operation.Security ??= [];
                    operation.Security.Add(securityRequirement);
                }
            }
        }
    }
}
