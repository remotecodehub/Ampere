# Architecture

## Purpose

Ampere is a local-first IoT platform.
It measures electrical consumption by
power point and exposes the data to web
and mobile clients.

## Solution areas

- `src/app`: .NET MAUI application.
- `src/web`: Blazor and REST API.
- `src/iot`: embedded IoT software.
- `src/os`: Raspberry Pi OS work.
- `tests`: tests for each area.

The current repository may still contain
only part of this target tree. Agents must
extend the existing structure without
breaking working code.

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

MAUI may also contain platform-specific
code under `Platforms`.

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

## Composition

Dependency injection composition belongs
in `Composition`. Keep registration out
of domain code.

## Web

Controllers expose REST endpoints.
Business logic belongs in Application or
Domain, not controllers.

Every possible endpoint response must be
listed with `ProducesResponseType<T>` and
its HTTP status code.

Blazor components are presentation only.

## IoT

IoT software is isolated from the web and
mobile application. Device protocols and
measurement logic must have explicit
boundaries.

MQTT is an integration transport, not a
Domain model.

## Local-first

Normal operation must not require a
private external cloud. Local networking,
data storage, authentication and device
communication must remain viable without
an internet service.

## Evolution

The first stage favors a single project.
Future work may split Application, Domain
and Infrastructure into projects when a
real dependency boundary justifies it.
