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

## Git

Do not create branches unless requested.
For this foundation task, changes go to
main in a focused commit.

Do not rewrite unrelated files.
Do not introduce dependencies without a
clear architectural reason.
