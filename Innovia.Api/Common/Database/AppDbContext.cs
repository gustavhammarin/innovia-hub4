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
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();

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

        builder.Entity<AvailabilityRule>(a =>
        {
            a.HasKey(x => x.Id);
            a.HasOne<ResourceType>().WithMany().HasForeignKey(x => x.ResourceTypeId);
            a.HasIndex(x => new { x.ResourceTypeId, x.DayOfWeek }).IsUnique();
            a.Property(x => x.OpensAt).HasColumnType("time without time zone");
            a.Property(x => x.ClosesAt).HasColumnType("time without time zone");
        });
    }
}
