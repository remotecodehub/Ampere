namespace Ampere.Domain.Ampere.Enums;

/// <summary>Defines a Sonoff device status.</summary>
public enum DeviceStatus
{
    /// <summary>The device is not reachable.</summary>
    Offline,

    /// <summary>The device is reachable.</summary>
    Online,

    /// <summary>The device is being updated.</summary>
    Updating
}
