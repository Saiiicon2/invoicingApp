using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Utilities.Automapper;
using PointOfSale.Utilities.Extensions;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Render (and many container platforms) provide the listening port via PORT.
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(portEnv) && int.TryParse(portEnv, out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Access/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });


builder.Services.AddDbContext<POINTOFSALEContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        // Render commonly provides a Postgres URL as DATABASE_URL.
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl)
            && Uri.TryCreate(databaseUrl, UriKind.Absolute, out var databaseUri)
            && (databaseUri.Scheme.Equals("postgres", StringComparison.OrdinalIgnoreCase)
                || databaseUri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase)))
        {
            var userInfoParts = databaseUri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfoParts[0]);
            var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
            var databaseName = databaseUri.AbsolutePath.Trim('/');

            var port = databaseUri.Port > 0 ? databaseUri.Port : 5432;

            // Basic conversion for Npgsql.
            // Render Postgres typically requires SSL.
            connectionString = $"Host={databaseUri.Host};Port={port};Database={databaseName};Username={username};Password={password};Ssl Mode=Require;Trust Server Certificate=true;";
        }
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Missing connection string 'ConnectionStrings:DefaultConnection' (or DATABASE_URL for Postgres).");
    }

    // Seamless switch: set DB_PROVIDER=postgres (recommended) or just provide a Postgres-style connection string.
    var dbProvider = builder.Configuration["DB_PROVIDER"] ?? Environment.GetEnvironmentVariable("DB_PROVIDER");
    var shouldUsePostgres = string.Equals(dbProvider, "postgres", StringComparison.OrdinalIgnoreCase)
        || string.Equals(dbProvider, "postgresql", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase);

    if (shouldUsePostgres)
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
}); 

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleItemRepository, SaleItemRepository>();

builder.Services.AddScoped<ITypeDocumentSaleService, TypeDocumentSaleService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IDashBoardService, DashBoardService>();
builder.Services.AddScoped<IMenuService, MenuService>();

var context = new CustomAssemblyLoadContext();
try
{
    var wkhtmltoxFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "libwkhtmltox.dll"
        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? "libwkhtmltox.dylib"
            : "libwkhtmltox.so";

    var wkhtmltoxPath = Path.Combine(AppContext.BaseDirectory, "Utilities", "LibraryPDF", wkhtmltoxFileName);

    if (File.Exists(wkhtmltoxPath))
    {
        context.LoadUnmanagedLibrary(wkhtmltoxPath);
    }
    else
    {
        // Don't crash the whole app on startup; PDF generation will fail if used.
        Console.Error.WriteLine($"wkhtmltox native library not found at '{wkhtmltoxPath}'.");
    }
}
catch (Exception ex)
{
    // Don't crash the whole app on startup; PDF generation will fail if used.
    Console.Error.WriteLine($"Failed to load wkhtmltox native library: {ex}");
}
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));


var app = builder.Build();

// Optional database bootstrap.
// Preferred: run EF Core migrations automatically (DB_AUTO_MIGRATE=true).
// Fallback: schema create without migrations (DB_AUTO_CREATE=true) for simple/dev scenarios.
var autoMigrate = builder.Configuration["DB_AUTO_MIGRATE"];
var autoCreate = builder.Configuration["DB_AUTO_CREATE"];
if (string.Equals(autoMigrate, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<POINTOFSALEContext>();
    db.Database.Migrate();
}
else if (string.Equals(autoCreate, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<POINTOFSALEContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.Run();
