using SensoryAnalysis.Contracts;
using SensoryAnalysis.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITestManagerService, TestManagerService>();
builder.Services.AddSingleton<ITestServiceFactory, TestServiceFactory>();
var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();
app.Run();