# Project architecture

## Solution model

Ampere now uses one project per DDD layer.

The web solution is `Ampere.Web.slnx`.

Its projects are:

- `Ampere`: Presentation.
- `Ampere.Application`: Application.
- `Ampere.Domain`: Domain.
- `Ampere.Infrastructure`: Infrastructure.
- `Ampere.Composition`: DI composition.

The mobile solution is `Ampere.App.slnx`.
It follows the same project-per-layer model.

Mobile is present as architectural
foundation but is not the current focus.

## Dependency direction

```text
                 Composition
                 /    |     \
                /     |      \
               v      v       v
      Presentation  Application  Infrastructure
          |             |             |
          |             v             v
          +----------> Domain <-------+
```

The practical dependency rules are:

- Presentation may reference Application.
- Presentation may reference Composition
  for startup composition.
- Application may reference Domain.
- Infrastructure may reference Application
  and Domain.
- Composition may reference all application
  layers required to compose services.
- Domain must not reference other layers.
- Application must not reference Infrastructure.
- Infrastructure must not reference Presentation.
- Infrastructure must not reference Composition.

## Responsibilities

### Presentation

Contains UI and transport concerns.

For web this includes:

- Blazor components;
- controllers;
- startup and hosting.

Controllers do not contain business rules.
Controllers do not access EF Core directly.

### Application

Contains use cases and application contracts.

Features are organized with:

- Abstractions;
- Commands;
- Handlers;
- Queries;
- Requests;
- Responses;
- Validators.

Application defines abstractions for
infrastructure capabilities.

### Domain

Contains business rules and domain concepts.

Features may contain:

- Entities;
- Enums;
- Events;
- Models.

Domain is independent of external technology.

### Infrastructure

Contains implementations of Application
abstractions and external integrations.

EF Core, database providers, MQTT libraries,
ASP.NET Identity infrastructure and other
external dependencies belong here unless a
specific architectural rule says otherwise.

### Composition

Contains the composition root.

Dependency injection registrations belong
here. Registrations must be explicit and
compile-time based.

Do not move service registration into
Application, Domain or Infrastructure.

## Persistence

The persistence flow remains:

```text
Presentation
    -> Mediator.NET
    -> Application Handler
    -> Feature Service
    -> IRepository<TEntity>
    -> Infrastructure Repository
    -> DbContext
    -> EF Core
    -> SQL database
```

`IRepository<TEntity>` belongs to
`Ampere.Application`.
Its implementation belongs to
`Ampere.Infrastructure`.

Feature Services are the consumers of the
repository abstraction.
They must not inject or use `DbContext`.

`IUnitOfWork` belongs to Application and its
implementation belongs to Infrastructure.
Repositories do not call `SaveChangesAsync`.

Transactional Mediator.NET requests are
coordinated by the transaction middleware in
Infrastructure.

## Shared domain base

Persisted entities implement `IEntityBase`.
Domain entities should inherit `EntityBase`
when possible.

The base contract contains:

- `Id`;
- `CreatedAt`;
- `CreatedBy`;
- `UpdatedAt`;
- `UpdatedBy`.

Identifiers use `Guid.CreateVersion7()`.

ASP.NET Identity entities may implement the
contract directly because Identity already
provides their framework base classes.

## Feature isolation

Do not place infrastructure-specific models
in Domain or Application.

When an external package requires a model,
keep that model in Infrastructure and expose
an Application abstraction when the feature
needs it.

The Application project must not reference
NuGet packages merely to access an
Infrastructure implementation.

## Mobile

The mobile application follows the same
layered project structure:

- Presentation;
- Application;
- Domain;
- Infrastructure;
- Composition.

Platform-specific MAUI code remains inside
the Presentation project or the appropriate
platform boundary.

Mobile services must follow the same DDD
dependency rules as the web application.

## IoT and OS

`src/iot` is reserved for embedded device
software.

`src/os` is reserved for Raspberry Pi OS
customization.

Neither area should introduce dependencies
from web Presentation into device software.

Integration contracts must be explicit.
