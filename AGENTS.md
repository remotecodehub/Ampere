# Ampere Agent Instructions

## Mission

Ampere is a local-first IoT platform for
measuring electrical energy consumption
by power point.

The platform has three stages:

1. REST API, Blazor Interactive UI,
   controllers and MQTT broker support.
2. .NET MAUI mobile application.
3. Raspberry Pi OS customization for a
   local installation cloud.

Normal operation must not require a
private external cloud.

## Architecture

The repository uses separate projects for
DDD layers. Do not recreate the former
single-project DDD layout.

The web solution contains:

- `Ampere` presentation;
- `Ampere.Application` application;
- `Ampere.Domain` domain;
- `Ampere.Infrastructure` infrastructure;
- `Ampere.Composition` composition root.

The mobile solution follows the same layer
project model. Mobile is not the current
feature focus, but its boundaries must be
preserved.

Dependency direction is:

Presentation
    -> Composition
    -> Application
    -> Domain

Infrastructure implements Application
abstractions and may depend on Application
and Domain.

Composition is the only DI composition root.

## Repository and database

Application services consume
`IRepository<TEntity>` and `IUnitOfWork`.

A Service must never inject `DbContext`.
A Service must never use `DbSet<T>`.
A Service must never reference EF Core.
A Service must never execute SQL directly.

`DbContext` may only be injected into the
Infrastructure Repository or a dedicated
Infrastructure repository whose abstraction
is defined outside Infrastructure.

Repositories do not call SaveChangesAsync.
The Unit of Work owns persistence changes.
Transactional Mediator requests use the
transaction pipeline.

Do not introduce a direct database shortcut
to solve a Service problem.

## Mediator

Use the `Mediator` package by
`martinothamar/Mediator`.

`Mediator.Net` is forbidden and must not be
introduced or retained in source projects.

Use source-generated handlers and pipelines.
Use `IRequest<TResponse>` for requests.
Use `IStreamRequest<TResponse>` for streams.
Use `IRequestHandler` and
`IStreamRequestHandler` as appropriate.

Do not use reflection to discover handlers.
DI registration must use compile-time known
assemblies and types.

## C# rules

- Use .NET 10.0.
- Follow .NET 10 best practices.
- Never use `dynamic`.
- Never use implicit local types.
- Always use explicit local types.
- Always use primary constructors.
- Never inject types through reflection.
- Use file-scoped namespaces.
- Keep one type per source file.
- Keep source lines at 45 characters
  or fewer.
- Prefer async and await.
- Propagate CancellationToken.
- Dispose owned resources.
- Use IDisposable or IAsyncDisposable
  where appropriate.

Code must not introduce compiler,
analyzer, or avoidable build warnings.
Never suppress diagnostics just to hide a
problem.

## MQTT

MQTTnet integration belongs in Infrastructure.

Use the public strongly typed MQTTnet API.
Do not inspect MQTTnet assemblies with
reflection.
Do not use `dynamic` with MQTTnet.
Do not resolve MQTTnet types by reflection.

The broker runtime may be singleton state,
but Services that consume repositories must
remain compatible with scoped repository and
DbContext lifetimes.

Use a singleton runtime state object when
long-lived MQTT server state is required.
The feature Service itself may be scoped and
may inject `IRepository<TEntity>`.

Persist broker configuration so a restart does
not require reconfiguration.

## Blazor and MudBlazor

Blazor pages are presentation only.
Business logic belongs in Application.

Use only MudBlazor components for page UI.
Do not introduce another component library.

MQTT pages belong under:

`Components/Pages/MQTT`

A page must be developed in this order:

1. create the Razor page structure;
2. add the `@code { }` block;
3. add a local `<style>` block only when
   styling is actually required.

Do not create code-behind files for these
pages.
Do not create separate CSS files for these
pages.

Styles, when necessary, stay below the
`@code { }` block.

Dialogs must use MudBlazor dialog APIs.

## Web API

Controllers must remain thin.
They dispatch Mediator requests and map
application results to HTTP responses.

Every possible endpoint response must use
`ProducesResponseType<T>(StatusCode)` or the
non-generic equivalent when no body exists.

Stream endpoints must consume a
`IStreamRequest<T>` through Mediator.

## Documentation

Document public and relevant internal APIs
with XML documentation in en-US.
Fill all applicable XML documentation fields.

## Testing

Web unit tests live under:

`tests/web/Ampere.UnitTests`

Feature tests are organized as:

```text
Common/
    Fixtures/
    Mocks/
Feature_Name/
    UnitTests.cs
```

Shared test fixtures and fakes belong under
`Common`. Feature-specific tests belong under
the feature directory.

Tests must use explicit local types and the
same C# rules as production code.

MQTT changes must include unit tests for
Application and Infrastructure behavior.

Coverage is measured with
`coverlet.collector` and must reach at least
80% line and branch coverage for the feature
under test.

The CI test job must publish the coverage
summary and enforce the 80% threshold.

## Git

Do not create branches unless requested.
Keep changes focused.
Do not rewrite unrelated code.
Do not merge draft pull requests.
