using System.Threading.Tasks;
using ImageSharingWithModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithModel.DAL;

public class ApplicationDbInitializer
{
    private readonly ApplicationDbContext db;
    private readonly ILogger<ApplicationDbInitializer> logger;

    public ApplicationDbInitializer(ApplicationDbContext db, ILogger<ApplicationDbInitializer> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public async Task SeedDatabase()
    {
        // Ensure that the database has been migrated (tables created).
        logger.LogDebug("Migrate the database...");
        await db.Database.MigrateAsync();

        logger.LogDebug("Clearing the database...");
        db.RemoveRange(db.Images);
        db.RemoveRange(db.Tags);
        db.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        logger.LogDebug("Initializing the database...");
        var sanjeet = new User { Username = "sanjeet", ADA = false };
        await db.Users.AddAsync(sanjeet);
        var sanjeetADA = new User { Username = "sanjeetADA", ADA = true };
        await db.Users.AddAsync(sanjeetADA);

        var portrait = new Tag { Name = "portrait" };
        await db.Tags.AddAsync(portrait);
        var architecture = new Tag { Name = "architecture" };
        await db.Tags.AddAsync(architecture);

        await db.SaveChangesAsync();
    }
}