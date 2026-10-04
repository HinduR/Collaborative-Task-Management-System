using BoardTaskService.API.Extension;
using BoardTaskService.Application.Extension;
using BoardTaskService.Infrastructure.Extension;
using BoardTaskService.Infrastructure.Realtime;
using Microsoft.OpenApi.Models;
using Shared.Authorisation.Infrastructure.Extensions;
using Shared.Authorization.Middleware;
using Shared.Grpc.Extension;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.ConfigureDBContext(builder.Configuration);
builder.Services.ConfigureRequiredServices(builder.Configuration);
builder.ConfigureLoggerService(builder.Services);
builder.Services.ConfigureExceptionHandler();
builder.Services.ConfigureHandlers();

builder.Services.AddSignalR();

builder.Services.ConfigureAuthorization();
builder.Services.AddSharedGrpc(builder.Configuration);
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "BoardTask Service API",
            Version = "v1"
        });
    options.EnableAnnotations();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "BoardTask Service API v1");
    });
}
app.UseCors();
if (app.Environment.IsProduction())
{
    app.Services.InitializeSeedData();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();

app.MapHub<BoardHub>("/hubs/board");
app.MapControllers();

app.Run();
