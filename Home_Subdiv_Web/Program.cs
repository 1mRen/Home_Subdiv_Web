// Using necessary namespaces for the application
using Home_Subdiv_Web.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure the application's DbContext with the connection string from appsettings.json
// This sets up SQL Server as the database provider using the "Default" connection string
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Add services to the container for MVC functionality
// This enables controllers and views, which is used in MVC applications
builder.Services.AddControllersWithViews();

// Configure the authentication middleware to use cookies for user authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

var app = builder.Build();

// Configure the HTTP request pipeline for handling requests

// If the environment is not Development, configure error handling and HSTS (HTTP Strict Transport Security)
// In non-development environments, errors will be handled by the "/Home/Error" action
if (!app.Environment.IsDevelopment())
{
    // Use exception handler for production errors
    app.UseExceptionHandler("/Home/Error");

    // Enforce HTTPS and use HTTP Strict Transport Security (HSTS) for secure connections
    // HSTS ensures that the app is only accessed over HTTPS
    app.UseHsts();
}

// Force HTTPS redirection for all HTTP requests
app.UseHttpsRedirection();

// Serve static files (e.g., images, CSS, JS files) from the wwwroot directory
app.UseStaticFiles();

// Set up routing for handling HTTP requests and mapping them to the appropriate controllers/actions
app.UseRouting();

// Add authentication middleware for handling user authentication (cookie-based)
app.UseAuthentication();

// Add authorization middleware to enforce access control (only authenticated users can access certain routes)
app.UseAuthorization();

// Configure the default route pattern for MVC
// When a request is made, the default controller is "Home" and the default action is "Index"
// Optionally, an "id" can be passed to the action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run the application
app.Run();
