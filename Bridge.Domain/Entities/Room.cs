using Bridge.Domain.Interfaces;

namespace Bridge.Domain.Entities;

public class Room : ICreatable
{
    public Guid Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }

    public ICollection<Item> Items { get; set; } = [];
}