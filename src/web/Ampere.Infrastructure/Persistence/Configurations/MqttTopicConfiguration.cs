using Ampere.Infrastructure.MQTT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ampere.Infrastructure.Persistence.Configurations;

/// <summary>Configures persisted MQTT topics.</summary>
public sealed class MqttTopicConfiguration
    : IEntityTypeConfiguration<MqttTopicEntity>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<MqttTopicEntity> builder)
    {
        builder.ToTable("MqttTopics");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Name)
            .HasMaxLength(512)
            .IsRequired();
        builder.HasIndex(entity => entity.Name)
            .IsUnique();
        builder.Property(entity => entity.Description)
            .HasMaxLength(2048);
    }
}
