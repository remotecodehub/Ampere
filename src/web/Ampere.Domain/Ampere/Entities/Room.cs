using Ampere.Domain.Common;
using Ampere.Domain.Ampere.Enums;

namespace Ampere.Domain.Ampere.Entities;

/// <summary>Models a room in a managed house.</summary>
public sealed class Room(string name) : EntityBase
{
    private readonly List<DeviceEndpoint> _endpoints = [];

    /// <summary>Gets or sets the room name.</summary>
    public string Name { get; set; } = ValidateName(name);

    /// <summary>Gets the room endpoints.</summary>
    public IReadOnlyList<DeviceEndpoint> Endpoints => _endpoints;

    /// <summary>Adds a Sonoff endpoint.</summary>
    public DeviceEndpoint AddEndpoint(
        string endpointName,
        EndpointKind kind,
        string sonoffId)
    {
        ValidateName(endpointName);

        if (string.IsNullOrWhiteSpace(sonoffId))
        {
            throw new ArgumentException(
                "A Sonoff identifier is required.",
                nameof(sonoffId));
        }

        bool exists = _endpoints.Any(
            endpoint => string.Equals(
                endpoint.Name,
                endpointName,
                StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            throw new InvalidOperationException(
                "An endpoint with the same name exists.");
        }

        DeviceEndpoint endpoint = new(
            endpointName,
            kind,
            sonoffId);
        _endpoints.Add(endpoint);
        return endpoint;
    }

    private static string ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "A name is required.",
                nameof(value));
        }

        return value.Trim();
    }
}
