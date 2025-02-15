using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bridge.Infrastructure.Data.Configurations;

internal readonly struct ItemConfigurations : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FileName).HasMaxLength(128);
        builder.Property(p => p.StorageKey).HasMaxLength(296);
        builder.HasIndex(p => p.RoomId);
        builder.HasIndex(p => p.ExpiredAt);
    }
}