using Mediator.Net.Contracts;

namespace Ampere.Application.Common.Responses;


/// <summary>Represents a generic application response with success state, optional data, and errors.</summary>
/// <typeparam name="T">The response data type.</typeparam>
/// <param name="Succeeded">Indicates whether the operation succeeded.</param>
/// <param name="Data">The operation result when successful; otherwise <see langword="null"/>.</param>
/// <param name="Errors">The errors returned by the operation.</param>
public sealed record Response<T>(bool Succeeded, T? Data, IReadOnlyCollection<string> Errors) : IResponse;

/// <summary>Factory class for creating <see cref="Response{T}"/> instances.</summary>
public static class Response
{
    /// <summary>Creates a successful response containing the supplied data.</summary>
    public static Response<T> Success<T>(T data) => new(true, data, Array.Empty<string>());

    /// <summary>Creates a failed response containing the supplied errors.</summary>
    public static Response<T> Failure<T>(IEnumerable<string> errors) => new(false, default, errors.ToArray());

    /// <summary>Creates a failed response containing the supplied errors.</summary>
    public static Response<T> Failure<T>(params string[] errors) => new(false, default, errors);
}
