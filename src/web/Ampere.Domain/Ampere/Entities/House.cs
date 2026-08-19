using Ampere.Domain.Common;

namespace Ampere.Domain.Ampere.Entities;

/// <summary>Models a managed electrical house.</summary>
public sealed class House(string name) : EntityBase
{
    private readonly List<Room> _rooms = [];
    private readonly List<SonoffDevice> _devices = [];

    /// <summary>Gets or sets the house name.</summary>
    public string Name { get; set; } = ValidateName(name);

    /// <summary>Gets or sets the house address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets the configured rooms.</summary>
    public IReadOnlyList<Room> Rooms => _rooms;

    /// <summary>Gets the registered Sonoff devices.</summary>
    public IReadOnlyList<SonoffDevice> Devices => _devices;

    /// <summary>Adds a room to the house.</summary>
    public Room AddRoom(string roomName)
    {
        ValidateName(roomName);

        bool exists = _rooms.Any(
            room => string.Equals(
                room.Name,
                roomName,
                StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            throw new InvalidOperationException(
                "A room with the same name exists.");
        }

        Room room = new(roomName);
        _rooms.Add(room);
        return room;
    }

    /// <summary>Registers a Sonoff device.</summary>
    public SonoffDevice AddDevice(
        string nodeId,
        string macAddress)
    {
        SonoffDevice device = new(nodeId, macAddress);
        _devices.Add(device);
        return device;
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
