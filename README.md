# Ampere

Ampere is a local-first IoT platform
for electrical energy measurement.

The solution measures consumption by
power point and provides a local-first
alternative to mandatory private cloud
infrastructure.

## Development stages

1. Web API, Blazor Interactive and MQTT.
2. .NET MAUI mobile application.
3. Raspberry Pi OS local cloud image.

## Architecture

See `AGENTS.md` for mandatory agent rules.
See `docs/architecture.md` for the
repository architecture.
See `.github/skills/` for focused skills.

## Technology

- .NET 10.0
- Blazor Interactive
- ASP.NET Core Controllers
- .NET MAUI
- MQTT
- Raspberry Pi OS
- Domain-Driven Design

The architecture is intentionally
single-project at first. Application,
Domain and Infrastructure remain folders
inside each application project.
