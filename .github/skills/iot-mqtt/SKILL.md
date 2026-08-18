# Skill: iot-mqtt

## Use when

Use for embedded devices, telemetry and
MQTT integration.

## Architecture

Treat MQTT as transport infrastructure.
Do not make MQTT topics the Domain model.

Separate device payloads from application
contracts.
Validate telemetry at the integration
boundary.

Measurement data must identify the device,
power point and measurement time when the
protocol requires those values.

Design for intermittent connectivity.
Prefer durable local processing where the
hardware supports it.

## Safety

Do not infer electrical safety guarantees
from software measurements.
Keep hardware limits explicit.

## Code

Keep adapters isolated from Domain.
Propagate cancellation where async APIs
support it.
Dispose MQTT connections correctly.
Document public contracts in en-US XML.
