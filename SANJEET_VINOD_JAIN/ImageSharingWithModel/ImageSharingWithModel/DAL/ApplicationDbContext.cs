using ImageSharingWithModel.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageSharingWithModel.DAL;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Image> Images { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Tag> Tags { get; set; }
}