# Skill: testing

## Use when

Use when creating or changing tests.

## Layout

Web unit tests live under:

`tests/web/Ampere.UnitTests`

Organize tests by feature. Shared fixtures and
fakes belong under `Common/Fixtures` and
`Common/Mocks`.

## Rules

Test behavior, not implementation details.
Keep tests isolated and deterministic.

Use explicit local types.
Do not use dynamic.
Do not inject types through reflection.
Use primary constructors when dependencies
exist.
Use file-scoped namespaces.
Keep one type per file.

Async tests must await operations.
Propagate cancellation when the API exposes
it.

MQTT tests must not require an external broker.
Use an ephemeral local port for runtime tests.
Always dispose broker runtime state.

Do not weaken production code merely to make
a test pass.

## Coverage

Use `coverlet.collector` with Cobertura.
The feature under development must reach at
least 80 percent line and branch coverage.

The CI job must publish the coverage summary
and fail the coverage gate when either metric
is below 80 percent.

## Package policy

Keep test dependencies on supported current
versions for the .NET 10 toolchain.
