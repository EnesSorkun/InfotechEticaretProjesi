using System.Security.Claims;
using Eticaret.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();


// Database
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));


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