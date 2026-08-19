using Ampere.Infrastructure.Common.UnitOfWork;
using Ampere.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ampere.UnitTests.Common;

/// <summary>Tests database unit-of-work transactions.</summary>
public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task UnitOfWork_CommitsAndRollsBackTransactions()
    {
        await using SqliteConnection connection =
            new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<AmpereDbContext> options =
            new DbContextOptionsBuilder<AmpereDbContext>()
                .UseSqlite(connection)
                .Options;
        await using AmpereDbContext context =
            new(options);
        await context.Database.EnsureCreatedAsync();
        await using UnitOfWork unitOfWork =
            new(context);

        await unitOfWork.CommitTransactionAsync(
            CancellationToken.None);
        await unitOfWork.RollbackTransactionAsync(
            CancellationToken.None);
        Assert.Equal(0, await unitOfWork.SaveChangesAsync(
            CancellationToken.None));

        await unitOfWork.BeginTransactionAsync(
            CancellationToken.None);
        await unitOfWork.CommitTransactionAsync(
            CancellationToken.None);

        await unitOfWork.BeginTransactionAsync(
            CancellationToken.None);
        await unitOfWork.RollbackTransactionAsync(
            CancellationToken.None);
    }

    [Fact]
    public async Task UnitOfWork_SecondBegin_DoesNotReplaceTransaction()
    {
        await using SqliteConnection connection =
            new("Data Source=:memory:");
        await connection.OpenAsync();
        DbContextOptions<AmpereDbContext> options =
            new DbContextOptionsBuilder<AmpereDbContext>()
                .UseSqlite(connection)
                .Options;
        await using AmpereDbContext context =
            new(options);
        await context.Database.EnsureCreatedAsync();
        await using UnitOfWork unitOfWork =
            new(context);

        await unitOfWork.BeginTransactionAsync(
            CancellationToken.None);
        await unitOfWork.BeginTransactionAsync(
            CancellationToken.None);
        await unitOfWork.RollbackTransactionAsync(
            CancellationToken.None);
    }
}
