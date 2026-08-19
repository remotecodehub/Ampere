# Development stages

## Stage 1: Web

Build the local server experience:

- ASP.NET Core REST API.
- Blazor Interactive frontend.
- Controllers for app clients.
- MQTT broker (server) for devices.
- Local persistence and services.

The server is the local authority for
installed devices and measurements.

## Stage 2: Mobile

Build the .NET MAUI application.

The app discovers or connects to the
local Ampere server and displays energy
consumption for configured points.

The app must remain usable without a
mandatory public cloud.

## Stage 3: Raspberry Pi OS

Build the local appliance environment.

The Raspberry Pi installation provides
the services required by the solution,
including the web server, data storage,
MQTT and local networking.

## Cross-stage contract

Device telemetry must have stable,
versioned contracts.

The web API must expose contracts suitable
for the mobile client.

Avoid coupling UI models directly to device
protocol payloads.
