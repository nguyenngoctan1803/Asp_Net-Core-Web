using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using DoAn2VADT.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Web.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

builder.Services.AddNotyf(config => { config.DurationInSeconds = 10; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("Db");
    opts.UseSqlServer(connectionString);
}
    );
builder.Services.AddSession();
/*builder.Services.AddSession(cfg =>
{
    cfg.Cookie.Name = DoAn2VADT.Shared.Const.CARTSESSION;
    cfg.IdleTimeout = new TimeSpan(0, 30, 0);
    cfg.Cookie.HttpOnly = true;
    cfg.Cookie.IsEssential = true;
});*/


/*builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = "/Account/Login";
    option.LogoutPath = "/Account/Logout";
    option.AccessDeniedPath = "/Account/Login";
});*/
builder.Services.AddDistributedMemoryCache();           


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseNotyf();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
