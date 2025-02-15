using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bridge.Infrastructure.Data.Configurations;

internal readonly struct PasteConfigurations : IEntityTypeConfiguration<Paste>
{
    public void Configure(EntityTypeBuilder<Paste> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.TruncatedContent).HasMaxLength(Paste.TruncatedLength);
        builder.HasIndex(p => p.RoomId);
        builder.HasIndex(p => p.ExpiredAt);
    }
}