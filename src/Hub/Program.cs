using DotNetEnv;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Spike.Application;
using Spike.Hub;
using Spike.Hub.Filters;
using Spike.Hub.Hubs;
using Spike.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var envFilePath = Path.Combine(
    Directory.GetCurrentDirectory(),
    $".env.{builder.Environment.EnvironmentName.ToLower()}");
Env.Load(envFilePath);

builder.Configuration
    .AddJsonFile(Path.Join("config", "appsettings.json"), false)
    .AddJsonFile(Path.Join("config", $"appsettings.{builder.Environment.EnvironmentName}.json"), false)
    .AddEnvironmentVariables();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Services.AddSingleton(Log.Logger);
builder.Host.UseSerilog();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 100 * 1024 * 1024; // Allow larger messages
    
    options.AddFilter<HubExceptionsFilter>();
}).AddMessagePackProtocol();

var app = builder.Build();

app.UseRouting();
app.UseStaticFiles();

app.MapHub<CallsHub>("/hub/calls");

app.Run();