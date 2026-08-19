using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ampere.UnitTests.Common;

/// <summary>Tests database context persistence behaviors.</summary>
public sealed class AmpereDbContextTests
{
    [Fact]
    public async Task SaveChanges_HandlesIdentifiersAndSoftDelete()
    {
        await using AmpereDbContext context = CreateContext();
        User user = new("user@example.com");
        Role role = new("Operator");
        string userId = user.Id;
        string roleId = role.Id;
        context.Users.Add(user);
        context.Roles.Add(role);

        await context.SaveChangesAsync();

        Assert.NotEqual(userId, user.Id);
        Assert.NotEqual(roleId, role.Id);

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
        User? persisted = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == user.Id);
        Assert.NotNull(persisted);
        Assert.True(persisted.IsDeleted);
    }

    [Fact]
    public async Task SaveChangesOverloads_ApplyPersistenceHooks()
    {
        await using AmpereDbContext context = CreateContext();

        Assert.Equal(0, context.SaveChanges());
        Assert.Equal(0, context.SaveChanges(true));
        Assert.Equal(0, await context.SaveChangesAsync(
            CancellationToken.None));
        Assert.Equal(0, await context.SaveChangesAsync(
            true, CancellationToken.None));
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
