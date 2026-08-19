using Ampere.Domain.Ampere.Entities;
using Ampere.Domain.Ampere.Enums;
using Xunit;

namespace Ampere.UnitTests.Ampere;

/// <summary>Tests the Ampere domain model.</summary>
public sealed class DomainModelTests
{
    [Fact]
    public void House_AddRoom_RejectsDuplicateNames()
    {
        House house = new("Home");

        house.AddRoom("Kitchen");

        Assert.Throws<InvalidOperationException>(
            () => house.AddRoom("kitchen"));
    }

    [Fact]
    public void House_AddDevice_RegistersNode()
    {
        House house = new("Home");

        SonoffDevice device = house.AddDevice(
            "node-01",
            "AA:BB:CC:DD:EE:01");

        Assert.Single(house.Devices);
        Assert.Equal("node-01", device.NodeId);
    }

    [Fact]
    public void House_AddDevice_RejectsDuplicateNode()
    {
        House house = new("Home");
        house.AddDevice("node-01", "mac-01");

        Assert.Throws<InvalidOperationException>(
            () => house.AddDevice(
                "NODE-01",
                "mac-02"));
    }

    [Fact]
    public void Sonoff_RejectsMissingIdentity()
    {
        Assert.Throws<ArgumentException>(
            () => new SonoffDevice("", "mac"));
        Assert.Throws<ArgumentException>(
            () => new SonoffDevice("node", ""));
    }

    [Fact]
    public void Room_AddEndpoint_AssignsSonoffAndKind()
    {
        Room room = new("Kitchen");

        DeviceEndpoint endpoint = room.AddEndpoint(
            "Coffee outlet",
            EndpointKind.Outlet,
            "sonoff-01");

        Assert.Equal("sonoff-01", endpoint.SonoffId);
        Assert.Equal(EndpointKind.Outlet, endpoint.Kind);
    }

    [Fact]
    public void Endpoint_SetRelayState_ChangesState()
    {
        DeviceEndpoint endpoint = new(
            "Light",
            EndpointKind.Switch,
            "sonoff-01");

        endpoint.SetRelayState(true);

        Assert.True(endpoint.RelayState);
    }

    [Fact]
    public void Sonoff_StatusMethods_UpdateState()
    {
        SonoffDevice device = new("node", "mac");

        device.MarkOnline();
        Assert.Equal(DeviceStatus.Online, device.Status);

        device.MarkUpdating();
        Assert.Equal(DeviceStatus.Updating, device.Status);

        device.MarkOffline();
        Assert.Equal(DeviceStatus.Offline, device.Status);
    }

    [Fact]
    public void EnergyReading_RejectsNegativeMeasurements()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EnergyReading(
                "node-01",
                "endpoint-01",
                DateTimeOffset.UtcNow,
                -1,
                1,
                100,
                10));
    }

    [Fact]
    public void MonthlyConsumption_SumsPositiveDeltas()
    {
        DateTimeOffset first =
            new(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset second = first.AddHours(1);
        DateTimeOffset third = second.AddHours(1);

        List<EnergyReading> readings =
        [
            new("node", "endpoint", first, 220, 1, 220, 100),
            new("node", "endpoint", second, 220, 1, 220, 160),
            new("node", "endpoint", third, 220, 1, 220, 250)
        ];

        decimal result = EnergyReading.CalculateMonthlyWh(
            readings,
            2026,
            8);

        Assert.Equal(150, result);
    }

    [Fact]
    public void MonthlyConsumption_HandlesMeterReset()
    {
        DateTimeOffset first =
            new(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset second = first.AddHours(1);

        List<EnergyReading> readings =
        [
            new("node", "endpoint", first, 220, 1, 220, 900),
            new("node", "endpoint", second, 220, 1, 220, 25)
        ];

        decimal result = EnergyReading.CalculateMonthlyWh(
            readings,
            2026,
            8);

        Assert.Equal(25, result);
    }

    [Fact]
    public void MonthlyConsumption_RejectsInvalidMonth()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EnergyReading.CalculateMonthlyWh(
                [],
                2026,
                13));
    }

    [Fact]
    public void MonthlyConsumption_IgnoresOtherMonths()
    {
        DateTimeOffset date =
            new(2026, 7, 31, 0, 0, 0, TimeSpan.Zero);
        EnergyReading reading = new(
            "node",
            "endpoint",
            date,
            220,
            1,
            220,
            100);

        decimal result = EnergyReading.CalculateMonthlyWh(
            [reading],
            2026,
            8);

        Assert.Equal(0, result);
    }
}
