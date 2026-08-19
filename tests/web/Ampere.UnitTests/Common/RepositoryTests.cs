using Ampere.Domain.MQTT.Entities;
using Ampere.Infrastructure.Common.Repository;
using Ampere.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ampere.UnitTests.Common;

/// <summary>Tests the generic EF Core repository.</summary>
public sealed class RepositoryTests
{
    [Fact]
    public async Task Repository_SupportsQueryAndMutationOperations()
    {
        await using AmpereDbContext context = CreateContext();
        Repository<MqttTopic> repository = new(context);
        MqttTopic first = new() { Name = "energy/main" };
        MqttTopic second = new() { Name = "energy/secondary" };

        await repository.AddAsync(first, CancellationToken.None);
        await repository.AddAsync(second, CancellationToken.None);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        MqttTopic? byId = await repository.GetByIdAsync(
            first.Id, [], CancellationToken.None);
        MqttTopic? tracked = await repository.GetTrackedByIdAsync(
            first.Id, [], CancellationToken.None);
        MqttTopic? match = await repository.FirstOrDefaultAsync(
            topic => topic.Name == "energy/secondary",
            [], CancellationToken.None);
        IReadOnlyList<MqttTopic> filtered =
            await repository.ListAsync(
                topic => topic.Enabled, [],
                CancellationToken.None);
        IReadOnlyList<MqttTopic> all =
            await repository.ListAsync(
                null, [], CancellationToken.None);

        Assert.NotNull(byId);
        Assert.NotNull(tracked);
        Assert.NotNull(match);
        Assert.Equal(2, filtered.Count);
        Assert.Equal(2, all.Count);
        Assert.True(await repository.ExistsAsync(
            topic => topic.Name == "energy/main",
            CancellationToken.None));
        Assert.False(await repository.ExistsAsync(
            topic => topic.Name == "missing",
            CancellationToken.None));

        first.Description = "Updated";
        repository.Update(first);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        repository.Remove(second);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.Single(await repository.ListAsync(
            null, [], CancellationToken.None));

        MqttTopic replacement = new()
        {
            Name = "energy/replacement"
        };
        await repository.AddAsync(replacement, CancellationToken.None);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        repository.RemoveRange([first, replacement]);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(await repository.ListAsync(null, [], CancellationToken.None));
    }

    private static AmpereDbContext CreateContext()
    {
        DbContextOptions<AmpereDbContext> options =
            new DbContextOptionsBuilder<AmpereDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        return new AmpereDbContext(options);
    }
}
