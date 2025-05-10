using Home_Sbdv.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Home_Sbdv.Repositories;
using Home_Sbdv.Services;
using Home_Sbdv.Entities;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Home_Sbdv.Services.Implementations;
using Home_Sbdv.Services.Interfaces;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IVisitorPassService, Home_Sbdv.Services.Implementations.VisitorPassService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IContactDirectoryService, ContactDirectoryService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IFacilityReservationService, FacilityReservationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


// Configure cookie authentication with improved security settings
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Register AppDbContext with MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

// Configure antiforgery token security
builder.Services.AddAntiforgery(options => {
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.HeaderName = "X-CSRF-TOKEN";
});

// Add logging
builder.Services.AddLogging();
var app = builder.Build();

// Seed admin, staff, and homeowner users
await SeedDefaultUsers(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Adding security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add this line to serve files from SecureFiles directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "SecureFiles")),
    RequestPath = "/SecureFiles"
});

// Add this line to serve files from Uploads directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/Uploads"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

async Task SeedDefaultUsers(WebApplication app)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<AppDbContext>();

            // Admin
            var adminExists = await dbContext.Users.AnyAsync(u =>
                u.Email.ToLower() == "admin@sbdv.com" ||
                u.Username.ToLower() == "admin");
            if (!adminExists)
            {
                var password = "Admin@Sbdv2025!";
                string passwordHash = HashPassword(password);
                var admin = new Users
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@sbdv.com",
                    Username = "admin",
                    Password = passwordHash,
                    Role = "admin",
                    Address = "123 Admin Street",
                    Gender = "male",
                    OwnershipStatus = "owned",
                    ContactNumber = "1234567890",
                    CreatedAt = DateTime.UtcNow,
                    EmailVerified = true
                };
                dbContext.Users.Add(admin);
                await dbContext.SaveChangesAsync();
            }

            // Staff
            var staffExists = await dbContext.Users.AnyAsync(u =>
                u.Email.ToLower() == "staff@sbdv.com" ||
                u.Username.ToLower() == "staff");
            if (!staffExists)
            {
                var password = "Staff@Sbdv2025!";
                string passwordHash = HashPassword(password);
                var staff = new Users
                {
                    FirstName = "Default",
                    LastName = "Staff",
                    Email = "staff@sbdv.com",
                    Username = "staff",
                    Password = passwordHash,
                    Role = "staff",
                    Address = "456 Staff Lane",
                    Gender = "female",
                    OwnershipStatus = "rented",
                    ContactNumber = "2345678901",
                    CreatedAt = DateTime.UtcNow,
                    EmailVerified = true
                };
                dbContext.Users.Add(staff);
                await dbContext.SaveChangesAsync();
            }

            // Homeowner
            var homeownerExists = await dbContext.Users.AnyAsync(u =>
                u.Email.ToLower() == "homeowner@sbdv.com" ||
                u.Username.ToLower() == "homeowner");
            if (!homeownerExists)
            {
                var password = "Homeowner@Sbdv2025!";
                string passwordHash = HashPassword(password);
                var homeowner = new Users
                {
                    FirstName = "Default",
                    LastName = "Homeowner",
                    Email = "homeowner@sbdv.com",
                    Username = "homeowner",
                    Password = passwordHash,
                    Role = "homeowner",
                    Address = "789 Homeowner Ave",
                    Gender = "others",
                    OwnershipStatus = "owned",
                    ContactNumber = "3456789012",
                    CreatedAt = DateTime.UtcNow,
                    EmailVerified = true
                };
                dbContext.Users.Add(homeowner);
                await dbContext.SaveChangesAsync();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding default users: {ex.Message}");
    }
}

// Helper method to hash passwords using BCrypt
string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}
