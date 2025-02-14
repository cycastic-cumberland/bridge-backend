using Bridge.Domain.Interfaces;

namespace Bridge.Domain.Entities;

public class Item : ICreatable
{
    public long Id { get; set; }
    
    public required Guid RoomId { get; set; }

    public Room Room { get; set; } = null!;
    
    public required string FileName { get; set; }
    
    public required string StorageKey { get; set; }
    
    public bool IsReady { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }
}