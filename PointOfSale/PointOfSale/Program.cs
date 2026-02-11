using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
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
var doSeed = builder.Configuration["DB_SEED"];
if (string.Equals(autoMigrate, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<POINTOFSALEContext>();
    db.Database.Migrate();

    // Always ensure core navigation + numbering exists so the app is usable.
    EnsureCoreNavigationAndCorrelatives(db);

    if (string.Equals(doSeed, "true", StringComparison.OrdinalIgnoreCase))
    {
        SeedDatabase(db);
    }
}
else if (string.Equals(autoCreate, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<POINTOFSALEContext>();
    db.Database.EnsureCreated();

    // Always ensure core navigation + numbering exists so the app is usable.
    EnsureCoreNavigationAndCorrelatives(db);

    if (string.Equals(doSeed, "true", StringComparison.OrdinalIgnoreCase))
    {
        SeedDatabase(db);
    }
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

static void SeedDatabase(POINTOFSALEContext db)
{
    // Always ensure core menus/role links/correlatives exist.
    EnsureCoreNavigationAndCorrelatives(db);

    // Minimal bootstrap so you can log in on a brand-new database.
    // Set these on Render (or locally) to control the seeded credentials.
    var seedEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL");
    var seedPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
    var seedName = Environment.GetEnvironmentVariable("SEED_ADMIN_NAME") ?? "Admin";

    var hasSeedUserCreds = !string.IsNullOrWhiteSpace(seedEmail) && !string.IsNullOrWhiteSpace(seedPassword);
    if (!hasSeedUserCreds)
    {
        // Still seed roles/menus so the app is navigable even if you created users manually.
        Console.Error.WriteLine("DB_SEED=true but SEED_ADMIN_EMAIL/SEED_ADMIN_PASSWORD not set; will seed roles/menus only.");
    }

    // Ensure at least an Admin role exists (case-insensitive match).
    var adminRole = db.Rols.FirstOrDefault(r =>
        r.Description != null && r.Description.Trim().ToLower() == "admin");
    if (adminRole == null)
    {
        adminRole = new Rol { Description = "Admin", IsActive = true };
        db.Rols.Add(adminRole);
        db.SaveChanges();
    }

    if (hasSeedUserCreds)
    {
        // Create/update the seeded admin user.
        var existingSeededUser = db.Users.FirstOrDefault(u => u.Email == seedEmail);
        if (existingSeededUser == null)
        {
            db.Users.Add(new User
            {
                Name = seedName,
                Email = seedEmail,
                Password = seedPassword,
                IdRol = adminRole.IdRol,
                IsActive = true,
                Photo = Array.Empty<byte>()
            });
            db.SaveChanges();
        }
        else
        {
            var changed = false;
            if (existingSeededUser.IdRol != adminRole.IdRol)
            {
                existingSeededUser.IdRol = adminRole.IdRol;
                changed = true;
            }
            if (existingSeededUser.Photo == null)
            {
                existingSeededUser.Photo = Array.Empty<byte>();
                changed = true;
            }
            if (changed)
            {
                db.SaveChanges();
            }
        }
    }

    // Menu + RolMenu seeding is handled by EnsureCoreNavigationAndCorrelatives.
}

static void EnsureCoreNavigationAndCorrelatives(POINTOFSALEContext db)
{
    // Ensure at least an Admin role exists.
    var adminRole = db.Rols.FirstOrDefault(r =>
        r.Description != null && r.Description.Trim().ToLower() == "admin");
    if (adminRole == null)
    {
        adminRole = new Rol { Description = "Admin", IsActive = true };
        db.Rols.Add(adminRole);
        db.SaveChanges();
    }

    // Ensure numbering starts at INV400 for a fresh DB.
    EnsureCorrelative(db, management: "Sale", minimumLastNumber: 399);
    // Optional: reserve a correlativo for quotes (not currently used by code).
    EnsureCorrelative(db, management: "Quote", minimumLastNumber: 399);

    // Seed sidebar menus (only when there are no menus yet).
    if (!db.Menus.Any())
    {
        var adminParent = new Menu { Description = "Admin", Icon = "mdi mdi-view-dashboard-outline", IsActive = true };
        var inventoryParent = new Menu { Description = "Inventory", Icon = "mdi mdi-package-variant-closed", IsActive = true };
        var salesParent = new Menu { Description = "Sales", Icon = "mdi mdi-shopping", IsActive = true };
        var reportsParent = new Menu { Description = "Reports", Icon = "mdi mdi-chart-bar", IsActive = true };

        db.Menus.AddRange(adminParent, inventoryParent, salesParent, reportsParent);
        db.SaveChanges();

        adminParent.IdMenuParent = adminParent.IdMenu;
        inventoryParent.IdMenuParent = inventoryParent.IdMenu;
        salesParent.IdMenuParent = salesParent.IdMenu;
        reportsParent.IdMenuParent = reportsParent.IdMenu;
        db.SaveChanges();

        var childMenus = new List<Menu>
        {
            new Menu { Description = "Dashboard", IdMenuParent = adminParent.IdMenu, Controller = "Admin", PageAction = "Dashboard", IsActive = true },
            new Menu { Description = "Users", IdMenuParent = adminParent.IdMenu, Controller = "Admin", PageAction = "Users", IsActive = true },

            new Menu { Description = "Categories", IdMenuParent = inventoryParent.IdMenu, Controller = "Inventory", PageAction = "Categories", IsActive = true },
            new Menu { Description = "Products", IdMenuParent = inventoryParent.IdMenu, Controller = "Inventory", PageAction = "Products", IsActive = true },

            new Menu { Description = "New sale", IdMenuParent = salesParent.IdMenu, Controller = "Sales", PageAction = "NewSale", IsActive = true },
            new Menu { Description = "Sales history", IdMenuParent = salesParent.IdMenu, Controller = "Sales", PageAction = "SalesHistory", IsActive = true },

            new Menu { Description = "Sales report", IdMenuParent = reportsParent.IdMenu, Controller = "Reports", PageAction = "SalesReport", IsActive = true },
        };

        db.Menus.AddRange(childMenus);
        db.SaveChanges();

        foreach (var menu in childMenus)
        {
            db.RolMenus.Add(new RolMenu
            {
                IdRol = adminRole.IdRol,
                IdMenu = menu.IdMenu,
                IsActive = true
            });
        }
        db.SaveChanges();
    }
    else
    {
        var adminMenuPairs = new (string controller, string action)[]
        {
            ("Admin", "Dashboard"),
            ("Admin", "Users"),
            ("Inventory", "Categories"),
            ("Inventory", "Products"),
            ("Sales", "NewSale"),
            ("Sales", "SalesHistory"),
            ("Reports", "SalesReport"),
        };

        foreach (var (controller, action) in adminMenuPairs)
        {
            var menu = db.Menus.FirstOrDefault(m => m.Controller == controller && m.PageAction == action);
            if (menu == null)
            {
                continue;
            }

            var exists = db.RolMenus.Any(rm => rm.IdRol == adminRole.IdRol && rm.IdMenu == menu.IdMenu);
            if (!exists)
            {
                db.RolMenus.Add(new RolMenu
                {
                    IdRol = adminRole.IdRol,
                    IdMenu = menu.IdMenu,
                    IsActive = true
                });
            }
        }

        db.SaveChanges();
    }
}

static void EnsureCorrelative(POINTOFSALEContext db, string management, int minimumLastNumber)
{
    // Postgres column is 'timestamp without time zone' so we must write DateTimeKind.Unspecified.
    static DateTime PgNow() => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    var correlative = db.CorrelativeNumbers.FirstOrDefault(c => c.Management == management);
    if (correlative == null)
    {
        correlative = new CorrelativeNumber
        {
            Management = management,
            LastNumber = minimumLastNumber,
            QuantityDigits = 3,
            DateUpdate = PgNow()
        };
        db.CorrelativeNumbers.Add(correlative);
        db.SaveChanges();
        return;
    }

    var current = correlative.LastNumber ?? 0;
    if (current < minimumLastNumber)
    {
        correlative.LastNumber = minimumLastNumber;
        correlative.DateUpdate = PgNow();
        if (correlative.QuantityDigits == null) correlative.QuantityDigits = 3;
        db.CorrelativeNumbers.Update(correlative);
        db.SaveChanges();
    }
}
