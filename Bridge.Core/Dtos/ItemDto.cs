namespace Bridge.Core.Dtos;

public class ItemDto
{
    public long Id { get; set; }
    
    public Guid RoomId { get; set; }
    
    public required string FileName { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }
}