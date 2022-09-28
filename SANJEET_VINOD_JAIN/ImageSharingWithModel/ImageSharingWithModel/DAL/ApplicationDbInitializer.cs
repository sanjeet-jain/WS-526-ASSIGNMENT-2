using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using ImageSharingWithModel.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ImageSharingWithModel.DAL
{
    public class ApplicationDbInitializer 
    {
        private ApplicationDbContext db;
        private ILogger<ApplicationDbInitializer> logger;
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
            User jfk = new User { Username = "jfk", ADA = false };
            await db.Users.AddAsync(jfk);
            User nixon = new User { Username = "nixon", ADA = false };
            await db.Users.AddAsync(nixon);

            Tag portrait = new Tag { Name = "portrait" };
            await db.Tags.AddAsync(portrait);
            Tag architecture = new Tag { Name = "architecture" };
            await db.Tags.AddAsync(architecture);

            await db.SaveChangesAsync();

        }
    }
}