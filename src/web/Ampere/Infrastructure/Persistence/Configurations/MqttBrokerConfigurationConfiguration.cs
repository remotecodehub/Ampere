using Ampere.Infrastructure.MQTT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ampere.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for MQTT broker configuration entity.</summary>
public sealed class MqttBrokerConfigurationConfiguration : IEntityTypeConfiguration<MqttBrokerConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<MqttBrokerConfigurationEntity> builder)
    {
        builder.ToTable("MqttBrokerConfigurations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BindAddress).HasMaxLength(255);
        builder.Property(x => x.Port).IsRequired();
        builder.Property(x => x.StartOnBoot).IsRequired();
        builder.Property(x => x.UseTls).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
