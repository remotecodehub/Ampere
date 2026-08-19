using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;

namespace Ampere.Application.Common.Pipeline.Validation;


/// <summary>Provides extension methods for adding validation to Mediator.Net pipelines.</summary>
public static class ValidationMiddlewareExtensions
{
    /// <summary>Adds the validation middleware to a Mediator.Net pipeline.</summary>
    /// <typeparam name="TContext">The pipeline context type.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    public static void UseValidation<TContext>(this IPipeConfigurator<TContext> configurator)
        where TContext : IContext<IMessage>
    {
        configurator.AddPipeSpecification(
            new ValidationMiddleware<TContext>(configurator.DependencyScope.Resolve<IMessageValidator>()));
    }
}
