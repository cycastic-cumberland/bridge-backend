using Bridge.Domain.Interfaces;

namespace Bridge.Domain.Entities;

public class Room : IEphemeral
{
    public Guid Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }

    public ICollection<Item> Items { get; set; } = [];

    public ICollection<Paste> Pastes { get; set; } = [];
}