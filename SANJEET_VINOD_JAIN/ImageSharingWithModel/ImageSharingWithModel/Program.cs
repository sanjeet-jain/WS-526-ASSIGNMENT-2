using ImageSharingWithModel.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

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

// TODO-DONE add database context & enable saving data in the log (not for production use!)
var connectionString = builder.Configuration["Data:ConnectionStrings:ImageSharingDB"];
builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseSqlServer(connectionString); });

// Replacement for database error page
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
/*
 * Configure logging to go the console (local testing only!).
 */
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var app = builder.Build();

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
        "default",
        "{controller=Home}/{action=Index}/{id?}");
});

/* 
 * TODO-DONE Seed the database: We need to manually inject the dependencies of the initalizer.
 * EF services are scoped to a request, so we must create a temporary scope for its injection.
 * More on dependency injection: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
 * More on DbContext lifetime: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
 */
using (var serviceScope = app.Services.CreateScope())
{
    var serviceProvider = serviceScope.ServiceProvider;
    var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbInitializer>>();

    await new ApplicationDbInitializer(db, logger).SeedDatabase();
}

/*
 * Finally, run the application!
 */
app.Run();