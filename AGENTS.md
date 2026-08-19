# Ampere Agent Instructions

## Mission

Ampere is a local-first IoT platform for
measuring electrical energy consumption
by power point.

The platform has three main stages:

1. REST API, Blazor Interactive UI,
   controllers and MQTT broker support.
2. .NET MAUI mobile application.
3. Raspberry Pi OS customization for
a local installation cloud.

The system must not require a private
external cloud for normal operation.

## Repository rules

- Read the applicable skill before work.
- Preserve the repository architecture.
- Use .NET 10.0 for .NET projects.
- Follow current .NET 10 best practices.
- Prefer DDD boundaries.
- Keep the current single-project model.
- Treat each project root as presentation.
- Put Application, Domain and
  Infrastructure below that root.
- Keep one data type per source file.
- Use file-scoped namespaces.
- Keep source lines at 45 characters
  or fewer.
- Prefer async and await.
- Propagate CancellationToken.
- Dispose resources when appropriate.
- Use IDisposable or IAsyncDisposable
  when the implementation owns resources.

## C# language rules

- Never use dynamic.
- Never use implicit local typing.
- Declare local variables explicitly.
- Always use primary constructors.
- Do not inject types through reflection.
- Prefer compile-time dependency wiring.
- Use explicit generic type arguments when
  inference harms clarity.
- Follow current .NET 10 best practices.

Dependency injection must use explicit
registrations and known service types.
Do not discover or register services by
scanning assemblies or types through
reflection.

## Service and persistence flow

Feature handlers call feature services.
Feature services own application use cases
that require infrastructure services.

A feature service may inject:

- feature abstractions;
- IRepository<TEntity>;
- IUnitOfWork;
- other application abstractions.

A feature service must not inject DbContext.
A feature service must not access DbSet<T>.
A feature service must not reference EF Core.
A feature service must not access SQL directly.

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

IUnitOfWork is coordinated by the
transaction middleware. Repositories do
not call SaveChangesAsync().

Transactional Mediator requests implement
ITransactionalRequest. The transaction
middleware begins a transaction before the
handler, saves changes after the handler,
and commits or rolls back the transaction.

Queries must not open database transactions
unless a specific consistency requirement
justifies one.

## Entity persistence

Persisted application entities must
implement IEntityBase. Domain entities
should inherit EntityBase when possible.

EntityBase provides:

- Id;
- CreatedAt;
- CreatedBy;
- UpdatedAt;
- UpdatedBy.

Id values use Guid.CreateVersion7().
Identity entities may implement
IEntityBase directly because ASP.NET Identity
already provides their base classes.

## Build quality

Code must not introduce compiler warnings.
Code must not introduce analyzer warnings.
Code must not introduce avoidable build
messages.

Do not suppress warnings merely to hide a
problem.

If a warning or analyzer diagnostic is
caused by the change, fix the root cause.

Do not use pragma suppression unless the
suppression is technically required,
scoped to the smallest possible location,
and documented.

The final affected build must be clean.

## Application organization

Application code is organized by feature.
Each feature may contain:

- Abstractions
- Commands
- Handlers
- Queries
- Requests
- Responses
- Validators

Domain code is organized by feature.
Each feature may contain:

- Entities
- Enums
- Events
- Models

Infrastructure is also organized by
feature. External-library-specific models
belong there when they cannot belong in
Domain.

## Web API

The web project contains Controllers.
Every endpoint must declare all possible
HTTP responses with
ProducesResponseType<T>(StatusCode).

Controllers must remain thin. Business
rules belong to Application or Domain.

## Documentation

Document public and relevant internal APIs
with XML documentation in en-US.
Fill every applicable XML documentation
field.

Documentation must be factual and must
not invent behavior that is not implemented.

## Validation

Build affected .NET projects after changes.
Run tests when they exist and are relevant.
Do not hide build or test failures.

The build must finish without warnings or
avoidable informational diagnostics.

## Git

Do not create branches unless requested.
For this foundation task, changes go to
main in a focused commit.

Do not rewrite unrelated files.
Do not introduce dependencies without a
clear architectural reason.
