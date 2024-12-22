using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using PetSoLive.Data.Repositories;
using PetSoLive.Infrastructure.Repositories;
using SmtpSettings = PetSoLive.Core.Entities.SmtpSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();  // Add MVC support



// .env dosyasını yükleyin
Env.Load();  // .env dosyasındaki çevresel değişkenleri yükler

// SMTP ayarlarını .env dosyasından alın
var smtpSettings = new SmtpSettings
{
    Host = Environment.GetEnvironmentVariable("SMTP_HOST"),
    Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")),
    Username = Environment.GetEnvironmentVariable("SMTP_USERNAME"),
    Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD"),
    FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL"),
    EnableSsl = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL"), out var enableSsl) && enableSsl

};

// SMTP ayarlarını DI container'a ekleyin
builder.Services.AddSingleton(smtpSettings);

// Veritabanı bağlantı dizesini al
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DATABASE_CONNECTION_STRING environment variable is not set.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("PetSoLive.Data"));
});

// Add session configuration
builder.Services.AddDistributedMemoryCache(); // Add memory cache for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;  // Cookie is only accessible via HTTP, not JavaScript
    options.Cookie.IsEssential = true; // Ensure session cookie is not ignored by browsers
});

// Register Repositories and Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IRepository<Assistance>, AssistanceRepository>();
builder.Services.AddScoped<IAssistanceService, AssistanceService>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<IAdoptionRepository, AdoptionRepository>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
//added
builder.Services.AddScoped<IAdoptionRepository, AdoptionRepository>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<IPetOwnerRepository, PetOwnerRepository>();
//added for mail
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPetOwnerService, PetOwnerService>();
builder.Services.AddScoped<IAdoptionRequestRepository, AdoptionRequestRepository>();




// Add authentication and authorization services (cookie-based authentication)
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";  // Path to login page for unauthorized access
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Adjust expiration as needed
        options.SlidingExpiration = true; // Extend cookie expiration with activity
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ensure session middleware comes before authentication middleware
app.UseSession(); // Enable session middleware
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();  // Enable authorization middleware

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
