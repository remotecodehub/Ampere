using Ampere.Application.Common.Abstractions;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;

namespace Ampere.Infrastructure.Persistence.Middlewares;

/// <summary>
/// Configures transactional Mediator pipelines.
/// </summary>
public static class TransactionMiddlewareExtensions
{
    /// <summary>
    /// Adds the transaction middleware.
    /// </summary>
    /// <typeparam name="TContext">
    /// The Mediator pipeline context type.
    /// </typeparam>
    /// <param name="configurator">
    /// The pipeline configurator.
    /// </param>
    public static void UseTransaction<TContext>(
        this IPipeConfigurator<TContext> configurator)
        where TContext : IContext<IMessage>
    {
        IUnitOfWork unitOfWork = configurator
            .DependencyScope
            .Resolve<IUnitOfWork>();

        configurator.AddPipeSpecification(
            new TransactionMiddleware<TContext>(
                unitOfWork));
    }
}
