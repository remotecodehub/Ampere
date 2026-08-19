using Ampere.Domain.Ampere.Enums;
using Ampere.Domain.Common;

namespace Ampere.Domain.Ampere.Entities;

/// <summary>Models an AmperEsp Sonoff controller.</summary>
public sealed class SonoffDevice(
    string nodeId,
    string macAddress) : EntityBase
{
    /// <summary>Gets the assigned radio node identifier.</summary>
    public string NodeId { get; } = Validate(
        nodeId,
        nameof(nodeId));

    /// <summary>Gets the device MAC address.</summary>
    public string MacAddress { get; } = Validate(
        macAddress,
        nameof(macAddress));

    /// <summary>Gets or sets the firmware version.</summary>
    public string FirmwareVersion { get; set; } = string.Empty;

    /// <summary>Gets the device status.</summary>
    public DeviceStatus Status { get; private set; }

    /// <summary>Marks the device as online.</summary>
    public void MarkOnline()
    {
        Status = DeviceStatus.Online;
    }

    /// <summary>Marks the device as offline.</summary>
    public void MarkOffline()
    {
        Status = DeviceStatus.Offline;
    }

    /// <summary>Marks the device as updating.</summary>
    public void MarkUpdating()
    {
        Status = DeviceStatus.Updating;
    }

    private static string Validate(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "A value is required.",
                parameterName);
        }

        return value.Trim();
    }
}
