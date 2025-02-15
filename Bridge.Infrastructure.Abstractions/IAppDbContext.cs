using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bridge.Infrastructure;

public interface IAppDbContext
{
    DbSet<Room> Rooms { get; }

    DbSet<Item> Items { get; }
    
    DbSet<Paste> Pastes { get; }
    
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}