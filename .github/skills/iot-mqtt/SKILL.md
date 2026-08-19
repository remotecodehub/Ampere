# Skill: iot-mqtt

## Use when

Use for MQTT broker, devices, telemetry and
MQTT integration.

## Architecture

Treat MQTT as transport infrastructure.
Do not make MQTTnet types part of Domain or
Application contracts.

MQTTnet integration belongs in Infrastructure.
Application exposes typed feature contracts.

Feature Services may inject IRepository<T>
and IUnitOfWork. They must not inject or use
DbContext.

Long-lived MQTT server state belongs in a
singleton runtime component. A feature Service
that uses repositories remains scoped.

Persist broker configuration through the
repository so configuration survives process
restarts and power failures.

## MQTTnet

Use the public strongly typed MQTTnet API.
Never use dynamic.
Never inspect MQTTnet assemblies through
reflection.
Never resolve MQTTnet types by reflection.

Do not use assembly scanning to discover
MQTTnet handlers or services.

## Mediator

Use martinothamar/Mediator.
Mediator.Net is forbidden.

Use typed requests, handlers and stream
requests. Live MQTT data must flow through
`IStreamRequest<T>` and
`IStreamRequestHandler<TRequest,TResponse>`.

## Web

MQTT pages belong under
`Components/Pages/MQTT`.

Use only MudBlazor components.
Do not create code-behind files.
Keep page code inside `@code { }`.
Keep page styles below `@code` in a local
`<style>` block when styles are necessary.

## Safety

Do not infer electrical safety guarantees
from software measurements.
Keep hardware limits explicit.

## Code

Use .NET 10.
Use primary constructors.
Use explicit local types.
Use file-scoped namespaces.
Keep one type per source file.
Keep lines at 45 characters or fewer.
Propagate cancellation.
Dispose MQTT connections correctly.
Document public contracts in en-US XML.

## Tests

MQTT changes require unit tests.
Tests must not require an external broker.
Use ephemeral local ports for broker runtime
unit tests.
Maintain at least 80 percent line and branch
coverage for the MQTT feature.
