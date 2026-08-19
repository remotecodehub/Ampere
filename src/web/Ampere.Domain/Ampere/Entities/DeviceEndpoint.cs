using Ampere.Domain.Ampere.Enums;
using Ampere.Domain.Common;

namespace Ampere.Domain.Ampere.Entities;

/// <summary>Models an addressable Sonoff endpoint.</summary>
public sealed class DeviceEndpoint(
    string name,
    EndpointKind kind,
    string sonoffId) : EntityBase
{
    /// <summary>Gets or sets the endpoint name.</summary>
    public string Name { get; set; } = name;

    /// <summary>Gets the endpoint kind.</summary>
    public EndpointKind Kind { get; } = kind;

    /// <summary>Gets the owning Sonoff identifier.</summary>
    public string SonoffId { get; } = sonoffId;

    /// <summary>Gets or sets the relay state.</summary>
    public bool RelayState { get; private set; }

    /// <summary>Changes the relay state.</summary>
    public void SetRelayState(bool state)
    {
        RelayState = state;
    }
}
