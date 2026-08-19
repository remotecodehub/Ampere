using Ampere.Domain.Common;

namespace Ampere.Domain.Ampere.Entities;

/// <summary>Stores one electrical telemetry sample.</summary>
public sealed class EnergyReading(
    string sonoffId,
    string endpointId,
    DateTimeOffset measuredAt,
    decimal voltage,
    decimal current,
    decimal power,
    decimal energyWh) : EntityBase
{
    /// <summary>Gets the Sonoff identifier.</summary>
    public string SonoffId { get; } = sonoffId;

    /// <summary>Gets the endpoint identifier.</summary>
    public string EndpointId { get; } = endpointId;

    /// <summary>Gets the measurement timestamp.</summary>
    public DateTimeOffset MeasuredAt { get; } = measuredAt;

    /// <summary>Gets the voltage in volts.</summary>
    public decimal Voltage { get; } = Validate(voltage);

    /// <summary>Gets the current in amperes.</summary>
    public decimal Current { get; } = Validate(current);

    /// <summary>Gets the power in watts.</summary>
    public decimal Power { get; } = Validate(power);

    /// <summary>Gets the cumulative energy in Wh.</summary>
    public decimal EnergyWh { get; } = Validate(energyWh);

    /// <summary>Calculates monthly consumption in Wh.</summary>
    public static decimal CalculateMonthlyWh(
        IEnumerable<EnergyReading> readings,
        int year,
        int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(month));
        }

        Dictionary<string, List<EnergyReading>> groups =
            readings
                .Where(reading =>
                    reading.MeasuredAt.Year == year &&
                    reading.MeasuredAt.Month == month)
                .GroupBy(reading => reading.EndpointId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(item => item.MeasuredAt)
                        .ToList());

        decimal total = 0;
        foreach (List<EnergyReading> group in groups.Values)
        {
            EnergyReading? previous = null;
            foreach (EnergyReading current in group)
            {
                if (previous is null)
                {
                    previous = current;
                    continue;
                }

                decimal delta = current.EnergyWh >= previous.EnergyWh
                    ? current.EnergyWh - previous.EnergyWh
                    : current.EnergyWh;
                total += delta;
                previous = current;
            }
        }

        return total;
    }

    private static decimal Validate(decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value));
        }

        return value;
    }
}
