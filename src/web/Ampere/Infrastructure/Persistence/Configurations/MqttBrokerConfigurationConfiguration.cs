using Ampere.Infrastructure.MQTT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ampere.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the MQTT broker settings entity.
/// </summary>
public sealed class MqttBrokerConfigurationConfiguration
    : IEntityTypeConfiguration<
        MqttBrokerConfigurationEntity>
{
    /// <summary>
    /// Configures the MQTT broker entity.
    /// </summary>
    /// <param name="builder">
    /// The entity type builder.
    /// </param>
    public void Configure(
        EntityTypeBuilder<MqttBrokerConfigurationEntity>
            builder)
    {
        builder.ToTable("MqttBrokerConfigurations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasMaxLength(450)
            .IsRequired();
        builder.Property(x => x.BindAddress)
            .HasMaxLength(255);
        builder.Property(x => x.Port).IsRequired();
        builder.Property(x => x.StartOnBoot)
            .IsRequired();
        builder.Property(x => x.UseTls).IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450)
            .IsRequired();
        builder.Property(x => x.UpdatedAt)
            .IsRequired();
        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(450)
            .IsRequired();
    }
}
