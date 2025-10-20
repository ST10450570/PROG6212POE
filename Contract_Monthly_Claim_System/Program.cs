// Program.cs
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

// Add MVC services
builder.Services.AddControllersWithViews();

// Add HttpContextAccessor to access session/connection info in services
builder.Services.AddHttpContextAccessor();

// Register application services with Dependency Injection
// Scoped: A new instance is created for each web request.
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

// Singleton: A single instance is created for the application's lifetime.
// Good for services that are stateless or manage a shared state, like our in-memory store and encryption service.
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<IFileEncryptionService, FileEncryptionService>();


var app = builder.Build();

// --- Configure the HTTP request pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enables serving files from wwwroot

app.UseRouting();

app.UseAuthorization();

// Map the default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed the in-memory database with initial data
var dataStore = app.Services.GetRequiredService<InMemoryDataStore>();
dataStore.SeedInitialData();


app.Run();