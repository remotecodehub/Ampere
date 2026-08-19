namespace Ampere.Domain.Common;

/// <summary>
/// Provides common persistence metadata.
/// </summary>
public abstract class EntityBase(
    DateTimeOffset? createdAt = null) : IEntityBase
{
    /// <inheritdoc />
    public string Id { get; set; } =
        Guid.CreateVersion7().ToString();

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; } =
        createdAt ?? DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string CreatedBy { get; set; } =
        string.Empty;

    /// <inheritdoc />
    public DateTimeOffset UpdatedAt { get; set; } =
        createdAt ?? DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string UpdatedBy { get; set; } =
        string.Empty;
}
