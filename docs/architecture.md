# Architecture

## Purpose

Ampere is a local-first IoT platform.
It measures electrical consumption by
power point and exposes data to web and
mobile clients.

## Solution areas

- `src/app`: .NET MAUI application.
- `src/web`: Blazor and REST API.
- `src/iot`: embedded IoT software.
- `src/os`: Raspberry Pi OS work.
- `tests`: tests for each area.

The repository may still contain only part
of the target tree. Agents must extend the
existing structure without breaking code.

## Single-project DDD

Each .NET application starts as one
project. The project directory is the
presentation boundary and contains:

- `Application`
- `Composition`
- `Domain`
- `Infrastructure`

Web also contains:

- `Components`
- `Controllers`

MAUI may also contain platform code under
`Platforms`.

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

Handlers invoke feature services. Services
may use application abstractions such as
repositories and unit of work.

Services must not depend on DbContext,
DbSet<T>, EF Core, or SQL directly.

## Persistence flow

The persistence flow is:

Presentation
    -> Mediator.NET
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

`IUnitOfWork` coordinates persistence and
is implemented in Infrastructure. Transaction
middleware controls its transaction lifecycle.

A state-changing Mediator request can
implement `ITransactionalRequest`.
The middleware then begins a transaction,
executes the handler, saves changes, and
commits or rolls back.

## Entities

Persisted application entities implement
`IEntityBase`. Domain entities should
inherit `EntityBase` when possible.

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

## Domain

Organize domain code by feature. Use:

- `Entities`
- `Enums`
- `Events`
- `Models`

Domain code must not depend on UI or
external infrastructure.

## Infrastructure

Organize external implementations by
feature. Keep adapter models here when
they require external dependencies.

EF Core and external NuGet dependencies
must remain in Infrastructure.

## Composition

Dependency injection composition belongs
in `Composition`. Keep registration out
of domain code.

Use explicit registrations. Do not discover
or inject types through reflection.

## Web

Controllers expose REST endpoints.
Business logic belongs in Application or
Domain, not controllers.

Every possible endpoint response must be
listed with `ProducesResponseType<T>` and
its HTTP status code.

Blazor components are presentation only.

## IoT

IoT software is isolated from web and mobile.
Device protocols and measurement logic have
explicit boundaries.

MQTT is an integration transport, not a
Domain model.

## Local-first

Normal operation must not require a
private external cloud. Local networking,
data storage, authentication, and device
communication remain viable without an
internet service.

## Evolution

The first stage favors a single project.
Future work may split Application, Domain,
and Infrastructure when a real dependency
boundary justifies it.
