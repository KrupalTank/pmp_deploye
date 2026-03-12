using DotNetEnv;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PlacementMentorshipPortal.Models;
using PlacementMentorshipPortal.Services;
using System;
using Hangfire;
using Hangfire.PostgreSql;


Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

// Configure DbContext using builder.Configuration (avoid BuildServiceProvider)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("dbcs")));

// Register the Gmail sender
builder.Services.AddTransient<GmailEmailSender>();
// after DbContext registration
builder.Services.AddScoped<SelectListService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index"; // Where to go if not logged in
        options.AccessDeniedPath = "/Home/Index"; // Where to go if role is wrong
    });


// 1. Add Hangfire Services
// 1. Add Hangfire Services
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    // FIX: Use "dbcs" to match your DbContext connection string
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("dbcs")));

// 2. Add the processing server
builder.Services.AddHangfireServer();

// 3. Add the Hangfire Dashboard (Optional: to see your jobs at /hangfire)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHangfireDashboard();
app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.Run();