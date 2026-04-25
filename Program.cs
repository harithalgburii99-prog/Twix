using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Twix.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Railway provides PORT env var ───────────────────────────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Services ────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// SQLite — file path works locally; on Railway, use /data/twix.db (persistent volume)
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "twix.db";
builder.Services.AddDbContext<TwixDb>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath        = "/Account/Login";
        opt.LogoutPath       = "/Account/Logout";
        opt.AccessDeniedPath = "/Account/Login";
        opt.ExpireTimeSpan   = TimeSpan.FromDays(7);
        opt.SlidingExpiration= true;
        opt.Cookie.HttpOnly  = true;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ───────────────────────────────────────────────────────────────────
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Seed DB on startup ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TwixDb>();
    await DbSeeder.SeedAsync(db);
}

app.Run();
