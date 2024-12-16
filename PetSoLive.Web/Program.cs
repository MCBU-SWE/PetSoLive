using Microsoft.EntityFrameworkCore;
using PetSoLive.Business.Services;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using PetSoLive.Data;
using PetSoLive.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();  // Add MVC support

// Add session configuration
builder.Services.AddDistributedMemoryCache(); // Add memory cache for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;  // Cookie is only accessible via HTTP, not JavaScript
});

// Add ApplicationDbContext (EF Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("PetSoLive.Data")));

// Register Repositories and Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();


builder.Services.AddScoped<IRepository<Assistance>, AssistanceRepository>();
builder.Services.AddScoped<IAssistanceService, AssistanceService>();
builder.Services.AddScoped<IRepository<Adoption>, AdoptionRepository>();
builder.Services.AddScoped<IAdoptionService, AdoptionService>();


// Register PetService with DI container
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPetRepository, PetRepository>();


// Add authentication and authorization services (cookie-based authentication)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Extend session timeout as needed
    options.Cookie.HttpOnly = true; // Ensure session cookie is secure
    options.Cookie.IsEssential = true; // Ensure cookie is not ignored by browsers
});

builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Adjust expiration as needed
        options.SlidingExpiration = true; // Extend cookie expiration with activity
    });



builder.Services.AddAuthorization();

// Build the application
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

// Enable session middleware (must come before routing)
app.UseSession();

// Authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Ensure session middleware is also in the pipeline

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
