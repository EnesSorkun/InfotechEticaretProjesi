using System.Security.Claims;
using Eticaret.Data;
using Eticaret.Service.Abstract;
using Eticaret.Service.Concrete;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".TeknoGrit.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.IOTimeout = TimeSpan.FromMinutes(10);
});


// Database
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));


// Authentication
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(x =>
    {
        x.LoginPath = "/Account/SignIn";

        x.AccessDeniedPath = "/Account/AccessDenied";

        x.Cookie.Name = "TeknoGritAccount";

        x.Cookie.MaxAge =
            TimeSpan.FromDays(7);

        x.Cookie.IsEssential = true;
    });


// Authorization
builder.Services.AddAuthorization(x =>
{
    // Sadece Admin girebilir
    x.AddPolicy(
        "AdminPolicy",
        policy =>
            policy.RequireClaim(
                ClaimTypes.Role,
                "Admin"));


    // Admin veya normal kullanıcı girebilir
    x.AddPolicy(
        "UserPolicy",
        policy =>
            policy.RequireClaim(
                ClaimTypes.Role,
                "Admin",
                "Customer"));
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();
app.UseSession(); // session kullan


// Önce Authentication
app.UseAuthentication();


// Sonra Authorization
app.UseAuthorization();


app.MapStaticAssets();


app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Main}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();