// Program.cs
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

builder.Services.AddControllersWithViews();

// Add HttpContextAccessor to access session/connection info in services
builder.Services.AddHttpContextAccessor();

// --- ADD THIS ---
// 1. Add session services to the dependency injection container
builder.Services.AddSession(options =>
{
    // You can configure session options here, e.g., timeout
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// --- END ADD ---


// Register application services with Dependency Injection
// Scoped: A new instance is created for each web request.
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

// Singleton: A single instance is created for the application's lifetime.
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<IFileEncryptionService, FileEncryptionService>();


var app = builder.Build();

// --- Configure the HTTP request pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enables serving files from wwwroot

app.UseRouting();

// --- ADD THIS ---
// 2. Enable the session middleware
// This MUST be called *after* UseRouting() and *before* UseAuthorization() and MapControllerRoute().
app.UseSession();
// --- END ADD ---

app.UseAuthorization();

// Map the default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed the in-memory database with initial data
var dataStore = app.Services.GetRequiredService<InMemoryDataStore>();
dataStore.SeedInitialData();


app.Run();