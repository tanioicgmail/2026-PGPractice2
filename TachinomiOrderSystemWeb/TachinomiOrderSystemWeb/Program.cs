using TOSWeb.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register DatabaseManager as a singleton and initialize database
var dbManager = new DatabaseManager();
dbManager.InitializeDatabase();
builder.Services.AddSingleton(dbManager);

// Add MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable serving static files from wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configure MVC routing (default is HomeController / Index action)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
