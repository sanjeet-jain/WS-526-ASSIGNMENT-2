using ImageSharingWithModel.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

/*
 * Add services to the container.
 */
builder.Services.AddControllersWithViews();

/*
 * Configure cookie policy to allow ADA saved in a cookie.
 */
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

/*
 * Configure logging to go the console (local testing only!).
 */
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// TODO add database context & enable saving data in the log (not for production use!)


// Replacement for database error page
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/*
 * Configure logging to go the console (local testing only!).
 */
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

// We will add authorization later.
// app.UseAuthorization();

/*
 * Redundant because it is default, but just to show that everything is configurable.
 */
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

/*
 * TODO Seed the database: We need to manually inject the dependencies of the initalizer.
 * EF services are scoped to a request, so we must create a temporary scope for its injection.
 * More on dependency injection: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
 * More on DbContext lifetime: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
 */


/*
 * Finally, run the application!
 */
app.Run();