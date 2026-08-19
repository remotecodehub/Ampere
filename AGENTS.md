# Ampere Agent Instructions

## Mission

Ampere is a local-first IoT platform for
measuring electrical energy consumption
by power point.

The platform has three stages:

1. REST API, Blazor Interactive UI,
   controllers and SignalR notifications.
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

Use source-generated typed requests,
commands, queries, notifications and streams.
Use `IRequest<TResponse>` for requests.
Use `IStreamRequest<TResponse>` for streams.
Use `IRequestHandler` and
`IStreamRequestHandler` as appropriate.
Use `INotification` for application events.

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

## REST and SignalR

REST is the transport for simple App-to-
Raspberry requests and resource operations.

SignalR is the transport for realtime app
notifications and live telemetry.

SignalR entrypoints remain in Presentation.
The SignalR service abstraction belongs to
Application/Common/Abstractions.
Its implementation belongs to
Infrastructure/Common/Hubs.

Controllers must remain thin and dispatch
Mediator messages. They must not contain
business rules or access Infrastructure.

SignalR messages use explicit DTOs with the
`Request` suffix for incoming transport data.
Command, query, stream and notification
responses are explicit application contracts.

Do not use `dynamic` or reflection with
SignalR. Use strongly typed application
contracts and compile-time DI wiring.

The Raspberry communicates with AmperEsp
Sonoff devices through the radio boundary.
Radio transport implementation belongs in
Infrastructure and must be hidden behind an
Application abstraction.

## Blazor and MudBlazor

Blazor pages are presentation only.
Business logic belongs in Application.

Use only MudBlazor components for page UI.
Do not introduce another component library.

Pages must be developed in this order:

1. create the Razor page structure;
2. add the `@code { }` block;
3. add a local `<style>` block only when
   styling is actually required.

Do not create code-behind files for pages.
Do not create separate CSS files for pages.

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

## Domain

Domain contains only business concepts and
rules. It must not reference ASP.NET Core,
SignalR, EF Core, Mediator or controllers.

Persisted Ampere entities inherit EntityBase
and model houses, rooms, Sonoff devices,
endpoints and energy telemetry.

Energy consumption rules must remain in the
Domain model or domain services.

## Testing

Web unit tests live under:

`tests/web/Ampere.UnitTests`

Feature tests are organized as:

```text
Common/
    Fixtures/
    Mocks/
    Stubs/
Feature_Name/
    UnitTests.cs
```

Tests must use explicit local types and the
same C# rules as production code.

**Every feature implementation must include
tests for every business rule introduced by
the feature.**

Every feature must achieve at least:

- 80 percent line coverage;
- 80 percent branch coverage.

The CI test job must publish the TRX test
summary and coverage summary and enforce the
80 percent threshold.

## IoT and radio

The Raspberry Pi is the gateway between
AmperEsp devices and the application.

AmperEsp Sonoff devices communicate with
the Raspberry through the radio boundary.

Radio protocols, serial ports and HC-12
specific code belong in Infrastructure or
`src/iot`, according to the deployment
boundary. Application and Domain must not
reference radio libraries.

## Git

Do not create branches unless requested.
Keep changes focused.
Do not rewrite unrelated code.
Do not merge draft pull requests.
