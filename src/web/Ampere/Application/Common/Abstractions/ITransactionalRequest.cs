using Mediator.Net.Contracts;

namespace Ampere.Application.Common.Abstractions;

/// <summary>
/// Marks a Mediator request as transactional.
/// </summary>
public interface ITransactionalRequest : IRequest
{
}
