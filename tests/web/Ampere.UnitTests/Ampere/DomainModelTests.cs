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
}
