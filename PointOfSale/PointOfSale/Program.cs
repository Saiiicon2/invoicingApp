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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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
