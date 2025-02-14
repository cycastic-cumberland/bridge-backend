using Bridge.Domain.Entities;
using Bridge.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;

namespace Bridge.Core;

public class RoomService : ConfigurableService<RoomConfigurations>
{
    private readonly IStorageService _storageService;
    private readonly IUrlGenerator _urlGenerator;
    
    public RoomService(IAppDbContext dbContext,
        IOptions<RoomConfigurations> configurations,
        IStorageService storageService,
        IUrlGenerator urlGenerator)
        : base(dbContext, configurations)
    {
        _storageService = storageService;
        _urlGenerator = urlGenerator;
    }

    public async Task<Guid> CreateRoomAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var room = new Room
        {
            Id = Guid.NewGuid(),
            CreatedAt = now,
            ExpiredAt = now.AddMinutes(Configurations.RoomCreationExpirationMinutes ?? 60)
        };
        DbContext.Rooms.Add(room);
        await DbContext.SaveChangesAsync(cancellationToken);
        return room.Id;
    }

    internal Task<Room> GetRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var query = GetRoomQuery(roomId);
        return query.GetAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken)
    {
        return GetRoomQuery(roomId).AnyAsync(cancellationToken);
    }

    private IQueryable<Room> GetRoomQuery(Guid roomId)
    {
        var now = DateTimeOffset.UtcNow;
        var query = DbContext.Rooms.Where(r => r.Id == roomId && r.ExpiredAt >= now);
        return query;
    }

    public async Task CleanRoomsAsync(CancellationToken cancellationToken)
    {
        await using var txn = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var roomQuery = DbContext.Rooms
            .Where(r => r.ExpiredAt < now);
        var itemQuery = roomQuery.SelectMany(r => r.Items);
        var storageKeys = await itemQuery
            .Select(i => i.StorageKey)
            .ToListAsync(cancellationToken);
        await itemQuery.ExecuteDeleteAsync(cancellationToken);
        await roomQuery.ExecuteDeleteAsync(cancellationToken);
        foreach (var chunk in storageKeys.Chunk(1000))
        {
            await _storageService.DeleteObjectsAsync(chunk, cancellationToken);
        }
        await txn.CommitAsync(cancellationToken);
    }
}