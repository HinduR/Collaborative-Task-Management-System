using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using IdentityService.API.Extension;
using IdentityService.API.Grpc;
using IdentityService.Application.Extension;
using IdentityService.Infrastructure.Extension;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Shared.Authorisation.Infrastructure.Extensions;
using Shared.Authorization.Middleware;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// Authentication
// ---------------------------------------------------------

string publicKeyPem =
    builder.Configuration["JwtConfig:PublicKey"]
    ?? throw new InvalidOperationException(
        "JwtConfig:PublicKey is not configured.");

RSA publicKeyRsa = RSA.Create();

publicKeyRsa.ImportFromPem(
    NormalizePem(publicKeyPem));

RsaSecurityKey publicSecurityKey =
    new(publicKeyRsa)
    {
        CryptoProviderFactory =
            new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
    };

builder.Services
    .AddAuthentication(options =>
    {
        // Normal REST APIs authenticate using the JWT
        // forwarded by the API Gateway.
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        // Unauthenticated REST APIs should return 401.
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        // Google middleware stores its temporary
        // authenticated principal in a cookie.
        options.DefaultSignInScheme =
            CookieAuthenticationDefaults
                .AuthenticationScheme;
    })
  .AddJwtBearer(
    JwtBearerDefaults.AuthenticationScheme,
    options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = publicSecurityKey,

                ValidateIssuer = true,
                ValidIssuer =
                    builder.Configuration[
                        "JwtConfig:Issuer"],

                ValidateAudience = true,
                ValidAudience =
                    builder.Configuration[
                        "JwtConfig:Audience"],

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidAlgorithms =
                [
                    SecurityAlgorithms.RsaSha256
                ],

                NameClaimType =
                    JwtRegisteredClaimNames.UniqueName,

                RoleClaimType = "role"
            };

        options.Events =
            new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity
                        is not ClaimsIdentity identity)
                    {
                        return Task.CompletedTask;
                    }

                    string? userId =
                        identity.FindFirst(
                            JwtRegisteredClaimNames.Sub)
                        ?.Value;

                    if (!string.IsNullOrWhiteSpace(userId) &&
                        !identity.HasClaim(
                            claim =>
                                claim.Type ==
                                ClaimTypes.NameIdentifier))
                    {
                        identity.AddClaim(
                            new Claim(
                                ClaimTypes.NameIdentifier,
                                userId));
                    }

                    return Task.CompletedTask;
                }
            };
    })
    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.Cookie.Name =
                "round-table-google-auth";

            options.Cookie.HttpOnly = true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromMinutes(10);

            options.SlidingExpiration = false;
        })
    .AddGoogle(
        GoogleDefaults.AuthenticationScheme,
        options =>
        {
            // Google writes its result into the
            // temporary Google cookie.
            options.SignInScheme =
                CookieAuthenticationDefaults
                    .AuthenticationScheme;

            options.ClientId =
                builder.Configuration[
                    "Authentication:Google:ClientId"]
                ?? throw new InvalidOperationException(
                    "Google ClientId is not configured.");

            options.ClientSecret =
                builder.Configuration[
                    "Authentication:Google:ClientSecret"]
                ?? throw new InvalidOperationException(
                    "Google ClientSecret is not configured.");

            // Public Gateway callback:
            // https://localhost:7079/signin-google
            //
            // YARP forwards this path to Identity Service.
            options.CallbackPath = "/signin-google";

            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");

            options.SaveTokens = false;

            options.Events.OnRemoteFailure = context =>
            {
                context.Response.Redirect(
                    "/api/identity/auth/google/failure");

                context.HandleResponse();

                return Task.CompletedTask;
            };
        });

// ASP.NET authorization services.
builder.Services.AddAuthorization();

// IUserContext and custom IAuthorizationService.
builder.Services.ConfigureAuthorization();

// ---------------------------------------------------------
// Forwarded headers
// ---------------------------------------------------------

builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto |
            ForwardedHeaders.XForwardedHost;

        // Locally running YARP Gateway.
        options.KnownProxies.Add(
            IPAddress.Loopback);

        options.KnownProxies.Add(
            IPAddress.IPv6Loopback);
    });

// ---------------------------------------------------------
// Application services
// ---------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

builder.Services.ConfigureDBContext(
    builder.Configuration);

builder.Services.ConfigureHandlers();

builder.Services.ConfigureRequiredServices();

builder.Services.ConfigureCryptographyServices();

builder.ConfigureLoggerService(
    builder.Services);

builder.Services.ConfigureExceptionHandler();

// Do not call ConfigureJwtAuthentication here.
// JWT authentication is configured above.

// ---------------------------------------------------------
// Kestrel
// ---------------------------------------------------------

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Internal REST endpoint used by API Gateway.
    serverOptions.ListenLocalhost(
        5039,
        listenOptions =>
        {
            listenOptions.Protocols =
                HttpProtocols.Http1;
        });

    // Internal gRPC endpoint.
    serverOptions.ListenLocalhost(
        5040,
        listenOptions =>
        {
            listenOptions.Protocols =
                HttpProtocols.Http2;
        });
});

// ---------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------

WebApplication app = builder.Build();

if (app.Environment.IsProduction())
{
    app.Services.InitializeSeedData();
}

// Must execute before authentication so Google sees
// the original public Gateway host and HTTPS scheme.
app.UseForwardedHeaders();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Validates the Bearer JWT and creates HttpContext.User.
app.UseAuthentication();

// Copies claims from HttpContext.User into IUserContext.
app.UseMiddleware<UserContextMiddleware>();

// Executes authorization policies and attributes.
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<IdentityUserGrpcService>();

app.Run();

static string NormalizePem(
    string pem)
{
    return pem
        .Replace("\\n", "\n")
        .Trim();
}