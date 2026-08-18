# Coding standards

## C# structure

- Use file-scoped namespaces.
- Keep one data type per file.
- Keep lines at 45 characters or less.
- Prefer small cohesive types.
- Avoid unrelated refactors.

## Async

Use async and await whenever supported.
Propagate `CancellationToken` through all
async application boundaries.
Do not discard cancellation silently.

Use `IAsyncDisposable` when asynchronous
resource cleanup is required.
Use `IDisposable` when synchronous cleanup
is sufficient.

## DDD

Keep business invariants in Domain.
Keep use-case orchestration in Application.
Keep adapters and external integrations in
Infrastructure.
Keep presentation concerns in Components
or Controllers.

## Dependencies

Dependencies must point inward.
Do not make Domain depend on framework or
transport details unless explicitly
justified by the architecture.

## Documentation

Use XML documentation in en-US.
Document public APIs and relevant internal
contracts.
Fill every applicable XML field, including
`summary`, `param`, `returns`, `remarks`,
`exception` and `typeparam` when relevant.

Do not invent documentation.
Describe actual behavior and constraints.
