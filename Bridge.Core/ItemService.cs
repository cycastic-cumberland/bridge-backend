using Bridge.Core.Dtos;
using Bridge.Domain;
using Bridge.Domain.Entities;
using Bridge.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bridge.Core;

public class ItemService : ConfigurableService<ItemConfigurations>, IEphemeralCleaner<Item>
{
    private readonly RoomService _roomService;
    private readonly IStorageService _storageService;

    public ItemService(IAppDbContext dbContext,
        IOptions<ItemConfigurations> configurations,
        RoomService roomService,
        IStorageService storageService)
        : base(dbContext, configurations)
    {
        _roomService = roomService;
        _storageService = storageService;
    }

    public async Task<UploadPreSignedDto> GetPreSignedUploadUrlAsync(Guid roomId, string fileName, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        room.ExpiredAt = now.AddMinutes(_roomService.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        var extension = Path.GetExtension(fileName);
        var item = new Item
        {
            RoomId = room.Id,
            FileName = fileName,
            StorageKey = $"{Guid.NewGuid()}{extension}",
            CreatedAt = now,
            ExpiredAt = now.AddMinutes(Configurations.ItemExpirationMinutes ?? 10)
        };

        DbContext.Items.Add(item);
        await DbContext.SaveChangesAsync(cancellationToken);
        var url = await _storageService.GetPreSignedUrlAsync(item.StorageKey,
            item.FileName,
            true,
            DateTimeOffset.UtcNow.AddMinutes(Configurations.PreSignedUploadUrlExpirationMinutes ?? 5));
        return new(item.Id, url);
    }

    public async Task<Page<ItemDto>> GetLatestItemsAsync(Guid roomId, PaginatedRequest request, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        room.ExpiredAt = now.AddMinutes(_roomService.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await DbContext.SaveChangesAsync(cancellationToken);

        var itemQuery = DbContext.Rooms
            .Where(r => r.Id == roomId && r.ExpiredAt >= now)
            .SelectMany(r => r.Items)
            .Where(i => i.ExpiredAt >= now && i.IsReady)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new ItemDto
            {
                Id = i.Id,
                RoomId = i.RoomId,
                FileName = i.FileName,
                CreatedAt = i.CreatedAt,
                ExpiredAt = i.ExpiredAt
            });
        return await itemQuery.Materialize(request, cancellationToken);
    }

    public async Task MakeReadyAsync(Guid roomId, long itemId, CancellationToken cancellationToken)
    {
        var item = await DbContext.Items
            .Where(i => i.Id == itemId && i.RoomId == roomId)
            .GetAsync(cancellationToken);
        if (item.IsReady)
        {
            return;
        }
        item.IsReady = true;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetPreSignedDownloadUrlAsync(Guid roomId, long itemId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var item = await DbContext.Items
            .Where(i => i.Id == itemId && i.RoomId == roomId && i.ExpiredAt >= now)
            .Include(i => i.Room)
            .GetAsync(cancellationToken);
        item.ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(Configurations.ItemExpirationMinutes ?? 10);
        item.Room.ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(_roomService.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await DbContext.SaveChangesAsync(cancellationToken);
        return await _storageService.GetPreSignedUrlAsync(item.StorageKey, item.FileName, false,
            now.AddMinutes(Configurations.PreSignDownloadUrlExpirationMinutes ?? 5));
    }

    public async Task CleanUpAsync(CancellationToken cancellationToken)
    {
        await using var txn = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var itemQuery = DbContext.Items
            .Where(i => i.ExpiredAt < now);
        var storageKeys = await itemQuery
            .Select(i => i.StorageKey)
            .ToListAsync(cancellationToken);
        await itemQuery.ExecuteDeleteAsync(cancellationToken);
        foreach (var chunk in storageKeys.Chunk(1000))
        {
            await _storageService.DeleteObjectsAsync(chunk, cancellationToken);
        }
        await txn.CommitAsync(cancellationToken);
    }
}