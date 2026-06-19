using Rotativa.AspNetCore;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITestManagerService, TestManagerService>();
builder.Services.AddSingleton<ITestServiceFactory, TestServiceFactory>();
builder.Services.AddScoped<ITestRepository, JsonRepository>();
var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();
RotativaConfiguration.Setup("wwwroot");
app.Run();