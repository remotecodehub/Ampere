# Testing Architecture

## Scope

Web unit tests live under:

`tests/web/Ampere.UnitTests`

The test project targets .NET 10.

## Layout

```text
Ampere/
  tests/
    web/
      Ampere.UnitTests/
        Common/
          Fixtures/
          Mocks/
        Feature_Name/
          UnitTests.cs
```

## Common

`Common/Fixtures` contains reusable test
fixtures and lifecycle helpers.

`Common/Mocks` contains deterministic fakes
for application abstractions.

Do not put production implementations in
Common.

## Features

Each feature has its own directory.
Tests for MQTT belong under:

`tests/web/Ampere.UnitTests/MQTT`

Tests should exercise Application handlers,
Infrastructure services, repository boundaries,
and relevant runtime behavior without requiring
a production SQL database.

## Coverage

Coverlet collector produces Cobertura XML.
The MQTT feature is the coverage scope for
the current migration.

The CI job reports both line and branch
coverage in the GitHub Actions summary.
Both metrics must be at least 80 percent.

Coverage filters are stored in:

`coverlet.runsettings`

## Test dependencies

The test project uses:

- xUnit v3;
- Microsoft.NET.Test.Sdk;
- coverlet.collector;
- xunit.runner.visualstudio.

Package versions are kept current with the
repository's supported .NET 10 toolchain.

## Rules

Use explicit local types.
Do not use dynamic.
Do not use reflection for test injection.
Use primary constructors for classes that
have dependencies.
Propagate cancellation tokens.
Keep one type per file.
Use file-scoped namespaces.

Tests must be deterministic and must not
require an external MQTT broker.

When a runtime MQTT server is required,
use an ephemeral local port and dispose the
runtime after the test.
