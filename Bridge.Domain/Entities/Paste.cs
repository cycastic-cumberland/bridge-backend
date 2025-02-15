using Bridge.Domain.Interfaces;

namespace Bridge.Domain.Entities;

public class Paste : IEphemeral
{
    public const int TruncatedLength = 32;
    
    public long Id { get; set; }
    
    public required Guid RoomId { get; set; }

    public Room Room { get; set; } = null!;
    
    public required string Content { get; set; }
    
    public required string TruncatedContent { get; set; }
    
    public required bool Truncated { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiredAt { get; set; }
}