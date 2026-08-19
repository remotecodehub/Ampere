# Architecture

## Purpose

Ampere is a local-first IoT platform.
It measures electrical consumption by
power point and exposes data to web and
mobile clients.

## Solution areas

- `src/app`: .NET MAUI application.
- `src/web`: Blazor, SignalR and REST API.
- `src/iot`: embedded IoT software.
- `src/os`: Raspberry Pi OS work.
- `tests/app`: mobile tests.
- `tests/web`: web tests.

## DDD projects

Each .NET application uses one project per
DDD layer. The presentation project is
separate from Application, Domain,
Infrastructure and Composition.

Web projects:

- `Ampere`
- `Ampere.Application`
- `Ampere.Domain`
- `Ampere.Infrastructure`
- `Ampere.Composition`

The MAUI application follows the same
layer-project model.

## Dependency direction

Presentation consumes Application and the
Composition root. Application depends on
Domain. Infrastructure implements
Application abstractions and may depend on
Domain. Composition wires all implementations.

## Application

Organize code by feature. A feature may
contain:

- `Abstractions`
- `Commands`
- `Handlers`
- `Queries`
- `Requests`
- `Responses`
- `Validators`

Handlers invoke feature Services.
Services consume `IRepository<TEntity>` and
`IUnitOfWork` when persistence is required.

Services must never depend on DbContext,
DbSet<T>, EF Core or SQL.

## Mediator

Use `Mediator` from martinothamar/Mediator.
`Mediator.Net` is not allowed.

Use source-generated request handlers and
stream handlers. Do not discover handlers
through reflection.

## Persistence flow

The persistence flow is:

Presentation
    -> Mediator
    -> Application Handler
    -> IService
    -> IRepository<TEntity>
    -> Infrastructure Repository
    -> DbContext
    -> EF Core
    -> SQL database

`IRepository<TEntity>` is defined in
Application and implemented in Infrastructure.

Repositories do not call SaveChangesAsync.
`IUnitOfWork` owns persistence changes.
Transaction middleware coordinates
transactional requests.

## MQTT runtime

MQTTnet belongs in Infrastructure.
The broker runtime may be singleton state,
but the feature Service that consumes the
repository is scoped.

Broker configuration is persisted through the
repository. A hosted startup service creates
a scope and starts the broker when
`StartOnBoot` is enabled.

MQTT messages are exposed to the Application
through typed contracts. Live messages use
Mediator stream requests.

## Entities

Persisted application entities implement
`IEntityBase`. Domain entities should inherit
`EntityBase` when possible.

`EntityBase` provides:

- `Id`
- `CreatedAt`
- `CreatedBy`
- `UpdatedAt`
- `UpdatedBy`

New identifiers use `Guid.CreateVersion7()`.
ASP.NET Identity entities implement the
interface directly because their framework
base classes are fixed.

## Blazor

Each feature have their pages belong under
`Components/Pages/FEATURE_NAME`.

Use only MudBlazor components.
Do not create code-behind or external CSS
for these pages. Keep page code inside the
Razor `@code` block and local styles below it
when required.

## Web API

Controllers expose REST endpoints and remain
thin. They dispatch Mediator requests.

Every possible endpoint response must be
listed with `ProducesResponseType<T>` or its
non-generic equivalent.

Stream endpoints consume Mediator
`IStreamRequest<T>` messages.

## Testing

Web unit tests live under
`tests/web/Ampere.UnitTests`.

Tests are organized by feature, with shared
fixtures and fakes under `Common`.

MQTT tests use no external broker. Runtime
tests use ephemeral ports.

Coverage for the MQTT executable feature
scope must be at least 80 percent for both
lines and branches.

## Local-first

Normal operation must not require a
private external cloud. Local networking,
data storage, authentication and device
communication remain viable without an
internet service.
