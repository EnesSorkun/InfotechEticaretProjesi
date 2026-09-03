using System.Security.Claims;
using Eticaret.Data;
using Eticaret.Service.Abstract;
using Eticaret.Service.Concrete;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// MVC + API CONTROLLER SERVİSLERİ
// =====================================================

builder.Services.AddControllersWithViews();


// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =====================================================
// SESSION
// =====================================================

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".TeknoGrit.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    options.IdleTimeout = TimeSpan.FromDays(1);
    options.IOTimeout = TimeSpan.FromMinutes(10);
});


// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"));
});


// =====================================================
// GENERIC SERVICE
// =====================================================

builder.Services.AddScoped(
    typeof(IService<>),
    typeof(Service<>));


// =====================================================
// AUTHENTICATION
// =====================================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "TeknoGritAccount";
        options.Cookie.MaxAge = TimeSpan.FromDays(7);
        options.Cookie.IsEssential = true;
    });


// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization(options =>
{
    // Sadece Admin girebilir
    options.AddPolicy(
        "AdminPolicy",
        policy =>
            policy.RequireClaim(
                ClaimTypes.Role,
                "Admin"));

    // Admin veya normal kullanıcı girebilir
    options.AddPolicy(
        "UserPolicy",
        policy =>
            policy.RequireClaim(
                ClaimTypes.Role,
                "Admin",
                "Customer"));
});


var app = builder.Build();


// =====================================================
// SWAGGER
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// =====================================================
// HTTP REQUEST PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// =====================================================
// HTTPS
// =====================================================

app.UseHttpsRedirection();


// =====================================================
// ROUTING
// =====================================================

app.UseRouting();


// =====================================================
// SESSION
// =====================================================

app.UseSession();


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// STATIC FILES
// =====================================================

app.MapStaticAssets();


// =====================================================
// ADMIN ROUTE
// =====================================================

app.MapControllerRoute(
    name: "admin",
    pattern:
        "{area:exists}/{controller=Main}/{action=Index}/{id?}");


// =====================================================
// DEFAULT MVC ROUTE
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// =====================================================
// WEB API ROUTES
// =====================================================
//
// CategoriesApiController içerisinde:
//
// [ApiController]
// [Route("api/categories")]
//
// kullandığımız için Attribute Routing
// endpointlerini aktif eder.
//
// GET    /api/categories
// GET    /api/categories/{id}
// POST   /api/categories
// PUT    /api/categories/{id}
// DELETE /api/categories/{id}
//
// =====================================================

app.MapControllers();


// =====================================================
// APPLICATION START
// =====================================================

app.Run();