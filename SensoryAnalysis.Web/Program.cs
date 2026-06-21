using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (HostBuilderContext context,
    IServiceProvider services,
    LoggerConfiguration configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITestManagerService, TestManagerService>();
builder.Services.AddScoped<ITestServiceFactory, TestServiceFactory>();
builder.Services.AddScoped<ITestRepository, SqlServerRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string? connection = builder.Configuration.GetConnectionString("Default");
    if (connection is null)
    {
        throw new Exception("Unable to get connection string");
    }
    options.UseSqlServer(connection);
});
var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();
RotativaConfiguration.Setup("wwwroot");
app.Run();