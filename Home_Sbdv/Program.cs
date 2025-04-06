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

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventService, EventService>();
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

// Seed admin user
await SeedAdminUser(app);

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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

async Task SeedAdminUser(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();

        // Apply migrations if needed
        await dbContext.Database.MigrateAsync();

        // Check if admin already exists
        var adminExists = await dbContext.Users.AnyAsync(u =>
            u.Email.ToLower() == "admin@sbdv.com" ||
            u.Username.ToLower() == "admin");

        if (!adminExists)
        {
            // Create password hash
            var password = "Admin@Sbdv2025!";
            string passwordHash = HashPassword(password);

            // Create new admin user
            var admin = new Users
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@sbdv.com",
                Username = "admin",
                Password = passwordHash,
                Role = "admin",
                Address = "123 Admin Street",
                Gender = "Other",
                OwnershipStatus = "Own",
                ContactNumber = "1234567890",
                CreatedAt = DateTime.UtcNow,
                EmailVerified = true // Admin account is pre-verified
            };

            // Add to database
            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Admin user seeded successfully!");
        }
        else
        {
            Console.WriteLine("Admin user already exists.");
        }
    }
}

// Helper method to hash passwords (without access modifier)
string HashPassword(string password)
{
    // Create a new instance of the PBKDF2 algorithm
    using var pbkdf2 = new Rfc2898DeriveBytes(
        password,
        16, // 16 bytes salt size
        10000, // 10,000 iterations
        HashAlgorithmName.SHA256);

    var hash = pbkdf2.GetBytes(32); // 32 bytes hash size
    var salt = pbkdf2.Salt;

    // Combine salt and hash
    var hashBytes = new byte[48]; // 16 + 32 = 48 bytes
    Array.Copy(salt, 0, hashBytes, 0, 16);
    Array.Copy(hash, 0, hashBytes, 16, 32);

    // Convert to base64 string for storage
    return Convert.ToBase64String(hashBytes);
}