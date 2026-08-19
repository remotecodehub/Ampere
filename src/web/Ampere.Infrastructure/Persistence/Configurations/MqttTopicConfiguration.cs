using Ampere.Domain.MQTT.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ampere.Infrastructure.Persistence.Configurations;

/// <summary>Configures persisted MQTT topics.</summary>
public sealed class MqttTopicConfiguration
    : IEntityTypeConfiguration<MqttTopic>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<MqttTopic> builder)
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
