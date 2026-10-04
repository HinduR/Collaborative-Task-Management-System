using Microsoft.AspNetCore.Builder;
using MetadataService.Infrastructure.Extension;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MetadataService.Application.Extension;
using MetadataService.API.GrpcServices;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.ConfigureLoggerService(builder.Services);
builder.Services.ConfigureExceptionHandler();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureRequiredServices();
builder.Services.ConfigureDBContext(builder.Configuration);
builder.Services.ConfigureHandlers();
builder.Services.ConfigureRedis(builder.Configuration);


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // REST controllers
    serverOptions.ListenAnyIP(5278, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    // Internal gRPC endpoint
    serverOptions.ListenAnyIP(5279, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();         
    });
});

builder.Services.AddGrpc();

var app = builder.Build();
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (app.Environment.IsProduction())
{
    app.Services.InitializeSeedData();
}
app.MapGrpcService<MetadataGrpcService>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
