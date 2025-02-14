using Bridge.Domain.Entities;
using Bridge.Domain.Interfaces;
using Bridge.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Infrastructure.Data;

public class PostgresAppDbContext : DbContext, IAppDbContext
{
    public DbSet<Room> Rooms { get; set; }

    public DbSet<Item> Items { get; set; }
    
    public PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Configure<Room, RoomConfigurations>(modelBuilder);
        Configure<Item, ItemConfigurations>(modelBuilder);
    }

    protected static void Configure<T, TConfig>(ModelBuilder modelBuilder)
        where TConfig : IEntityTypeConfiguration<T>, new()
        where T : class
    {
        new TConfig().Configure(modelBuilder.Entity<T>());
    }
    
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var createdEntries = ChangeTracker.Entries<ICreatable>().Where(e => e.State == EntityState.Added);
        foreach (var entry in createdEntries)
        {
            if (entry.Entity.CreatedAt == default)
            {
                entry.Entity.CreatedAt = now;
            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}