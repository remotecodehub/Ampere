# Skill: web-api

## Use when

Use for REST controllers and web API work.

## Rules

Keep controllers thin.
Dispatch use cases to Application.
Keep business rules out of controllers.

For every endpoint, declare every possible
HTTP response with
`ProducesResponseType<T>(StatusCode)`.

Use explicit request and response
contracts. Do not expose Domain entities
as accidental transport contracts.

Propagate `CancellationToken` from the
controller into the application boundary.

Prefer async controller actions.

## Blazor

Components are presentation concerns.
Do not put persistence or domain rules in
components.

## Validation

Build the web project.
Run API tests when available.
