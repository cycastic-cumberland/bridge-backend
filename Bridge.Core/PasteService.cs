using Bridge.Core.Dtos;
using Bridge.Domain;
using Bridge.Domain.Entities;
using Bridge.Domain.Exceptions;
using Bridge.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bridge.Core;

public class PasteService : ConfigurableService<PasteConfigurations>, IEphemeralCleaner<Paste>
{
    private readonly RoomService _roomService;

    public PasteService(IAppDbContext dbContext, IOptions<PasteConfigurations> configurations, RoomService roomService)
        : base(dbContext, configurations)
    {
        _roomService = roomService;
    }

    public async Task CreatePasteAsync(Guid roomId, string content, CancellationToken cancellationToken)
    {
        if (content.Length > (Configurations.PasteExpirationMinutes ?? 8192))
        {
            throw new BadRequestException("Content length is over the preset limit.");
        }
        var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
        var truncatedContent = content.Truncate(Paste.TruncatedLength, out var truncated);
        var now = DateTimeOffset.UtcNow;
        DbContext.Pastes.Add(new()
        {
            RoomId = room.Id,
            Content = truncated ? content : string.Empty,
            TruncatedContent = truncatedContent,
            Truncated = truncated,
            CreatedAt = now,
            ExpiredAt = now.AddMinutes(Configurations.PasteExpirationMinutes ?? 5)
        });
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasteDto> GetPasteAsync(Guid roomId, long pasteId, bool truncate, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
        room.ExpiredAt = room.ExpiredAt
                .AddMinutes(_roomService.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await DbContext.SaveChangesAsync(cancellationToken);
        var pasteQuery = DbContext.Pastes
            .Where(p => p.RoomId == roomId && p.Id == pasteId && p.ExpiredAt >= now);
        IQueryable<PasteDto> finalQuery;
        if (truncate)
        {
            finalQuery = pasteQuery.Select(p => new PasteDto
            {
                Id = p.Id,
                RoomId = p.RoomId,
                Content = p.TruncatedContent,
                CreatedAt = p.CreatedAt,
                ExpiredAt = p.ExpiredAt
            });
        }
        else
        {
            finalQuery = pasteQuery.Select(p => new PasteDto
            {
                Id = p.Id,
                RoomId = p.RoomId,
                Content = p.Truncated ? p.Content : p.TruncatedContent,
                CreatedAt = p.CreatedAt,
                ExpiredAt = p.ExpiredAt
            });
        }

        return await finalQuery.GetAsync(cancellationToken);
    }
    
    public async Task<Page<PasteDto>> GetLatestPastesAsync(Guid roomId,
        PaginatedRequest request,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.GetRoomAsync(roomId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        room.ExpiredAt = now.AddMinutes(_roomService.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await DbContext.SaveChangesAsync(cancellationToken);

        var pasteQuery = DbContext.Rooms
            .Where(r => r.Id == roomId && r.ExpiredAt >= now)
            .SelectMany(r => r.Pastes)
            .Where(p => p.ExpiredAt >= now)
            .OrderByDescending(p => p.ExpiredAt)
            .Select(p => new PasteDto
            {
                Id = p.Id,
                RoomId = p.RoomId,
                Content = p.TruncatedContent,
                CreatedAt = p.CreatedAt,
                ExpiredAt = p.ExpiredAt
            });
        return await pasteQuery.Materialize(request, cancellationToken);
    }

    public async Task CleanUpAsync(CancellationToken cancellationToken)
    {
        await using var txn = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var pasteQuery = DbContext.Pastes
            .Where(i => i.ExpiredAt < now);
        await pasteQuery.ExecuteDeleteAsync(cancellationToken);
        await txn.CommitAsync(cancellationToken);
    }
}