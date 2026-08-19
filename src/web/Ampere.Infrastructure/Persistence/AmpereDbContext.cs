using System.Linq.Expressions;
using Ampere.Domain.Common;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.MQTT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ampere.Infrastructure.Persistence;

/// <summary>Represents the Ampere database context.</summary>
public sealed class AmpereDbContext(
    DbContextOptions<AmpereDbContext> options)
    : IdentityDbContext<User, Role, string>(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (IMutableEntityType entityType
            in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(
                entityType.ClrType))
            {
                continue;
            }

            ParameterExpression parameter =
                Expression.Parameter(
                    entityType.ClrType,
                    "entity");
            MemberExpression property =
                Expression.Property(
                    parameter,
                    nameof(ISoftDeletable.IsDeleted));
            LambdaExpression filter =
                Expression.Lambda(
                    Expression.Not(property),
                    parameter);

            builder.Entity(entityType.ClrType)
                .HasQueryFilter(filter);
        }

        builder.ApplyConfigurationsFromAssembly(
            typeof(AmpereDbContext).Assembly);
    }

    /// <inheritdoc />
    public override int SaveChanges(
        bool acceptAllChangesOnSuccess)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChanges(
            acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChanges();
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyEntityIdentifiers();
        ApplySoftDelete();
        return base.SaveChangesAsync(
            cancellationToken);
    }

    private void ApplyEntityIdentifiers()
    {
        foreach (EntityEntry<IEntityBase> entry
            in ChangeTracker.Entries<IEntityBase>())
        {
            if (entry.State != EntityState.Added)
            {
                continue;
            }

            if (entry.Entity is User
                || entry.Entity is Role)
            {
                entry.Entity.Id =
                    Guid.CreateVersion7().ToString();
            }
        }
    }

    private void ApplySoftDelete()
    {
        foreach (EntityEntry<ISoftDeletable> entry
            in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt =
                DateTimeOffset.UtcNow;
        }
    }

    /// <summary>Gets broker configurations.</summary>
    public DbSet<MqttBrokerConfigurationEntity>
        MqttBrokerConfigurations { get; set; } = null!;

    /// <summary>Gets configured MQTT topics.</summary>
    public DbSet<MqttTopicEntity> MqttTopics
    {
        get;
        set;
    } = null!;
}
