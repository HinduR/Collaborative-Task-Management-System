using ApiGateway.Extension;
using Shared.Grpc.Extension;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.yarp.json",
    optional: false,
    reloadOnChange: true);

builder.ConfigureLoggerService(builder.Services);

builder.Services.AddGatewayServices(builder.Configuration);

builder.Services.AddSharedGrpc(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy"));

WebApplication app = builder.Build();

app.UseExceptionHandler();

app.UseCors();

app.UseRateLimiter();

app.UseRequestTimeouts();

app.UseAuthentication();

app.UseAuthorization();

app.MapReverseProxy();

app.Run();
