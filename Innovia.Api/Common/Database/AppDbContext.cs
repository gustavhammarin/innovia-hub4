using Innovia.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Common.Database;

public class AppDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceType> ResourceTypes => Set<ResourceType>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Booking>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId);
            b.HasOne<Resource>().WithMany().HasForeignKey(x => x.ResourceId);
        });

        builder.Entity<Resource>(r =>
        {
            r.HasKey(x => x.Id);
            r.HasOne<ResourceType>().WithMany().HasForeignKey(x => x.ResourceTypeId);
        });
    }
}