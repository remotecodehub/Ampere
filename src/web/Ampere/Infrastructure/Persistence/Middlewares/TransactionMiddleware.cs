using Ampere.Application.Common.Abstractions;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;

namespace Ampere.Infrastructure.Persistence.Middlewares;

/// <summary>
/// Executes marked requests in a database transaction.
/// </summary>
/// <typeparam name="TContext">
/// The Mediator pipeline context type.
/// </typeparam>
public sealed class TransactionMiddleware<TContext>(
    IUnitOfWork unitOfWork) : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    /// <inheritdoc />
    public bool ShouldExecute(
        TContext context,
        CancellationToken cancellationToken)
    {
        return context.Message is ITransactionalRequest;
    }

    /// <inheritdoc />
    public Task BeforeExecute(
        TContext context,
        CancellationToken cancellationToken)
    {
        return unitOfWork.BeginTransactionAsync(
            cancellationToken);
    }

    /// <inheritdoc />
    public Task Execute(
        TContext context,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task AfterExecute(
        TContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(
                cancellationToken);
            await unitOfWork.CommitTransactionAsync(
                cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    public Task OnException(
        Exception ex,
        TContext context)
    {
        return unitOfWork.RollbackTransactionAsync(
            CancellationToken.None);
    }
}
