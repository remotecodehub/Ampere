# Project architecture

## Solution model

Ampere uses one project per DDD layer.

The web solution is `Ampere.Web.slnx`.
Its projects are:

- `Ampere`: Presentation.
- `Ampere.Application`: Application.
- `Ampere.Domain`: Domain.
- `Ampere.Infrastructure`: Infrastructure.
- `Ampere.Composition`: DI composition.

The mobile solution is `Ampere.App.slnx`.
It follows the same project-per-layer model.

Mobile is an architectural foundation but
is not the current focus.

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

The practical rules are:

- Presentation may reference Application.
- Presentation may reference Composition
  for startup composition.
- Application may reference Domain.
- Infrastructure may reference Application
  and Domain.
- Composition may reference all layers.
- Domain must not reference other layers.
- Application must not reference
  Infrastructure.
- Infrastructure must not reference
  Presentation or Composition.

## Responsibilities

### Presentation

Contains UI and transport concerns.

For web this includes:

- Blazor components;
- controllers;
- hosting startup.

Controllers do not contain business rules.
Controllers do not access EF Core.

Razor pages use components only MudBlazor components.
Razor pages and components do not have code-behind files or
external CSS files.

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

Mediator uses source-generated typed requests,
handlers and stream handlers.

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

EF Core, database providers, MQTTnet, ASP.NET
Identity infrastructure and other external
dependencies belong here.

### Composition

Contains the composition root.

Dependency injection registrations belong
here. Registrations must be explicit and
compile-time based.

Do not use reflection to discover services.

## Persistence

The persistence flow is:

```text
Presentation
    -> Mediator
    -> Application Handler
    -> Feature Service
    -> IRepository<TEntity>
    -> Infrastructure Repository
    -> DbContext
    -> EF Core
    -> SQL database
```

`IRepository<TEntity>` belongs to Application.
Its implementation belongs to Infrastructure.

Feature Services consume the repository.
They must not inject or use `DbContext`.

If direct context access becomes unavoidable,
create a dedicated Infrastructure repository
whose Application abstraction hides the context.

`IUnitOfWork` belongs to Application and its
implementation belongs to Infrastructure.
Repositories do not call SaveChangesAsync.

Transactional requests are coordinated by
the transaction pipeline in Infrastructure.

## SignalR

SinalR is an Presentation dependency, but the
service abstraction (Interface) belongs to
`Application/Common/Abstractions` and its
implementation goes to `Infrastructure/Common/Hubs/`.

Signal configuration is persisted through
`IRepository<SignalRConfigurationEntity>`.

A hosted startup service creates a scope and
starts the broker when `StartOnBoot` is enabled.

Live messages are exposed through Mediator
`IStreamRequest<T>` and consumed by the REST
stream endpoint and Blazor live page.

No `dynamic` or reflection may be
used against any assemblies.

## Shared domain base

Persisted entities implement `IEntityBase`.
Domain entities should inherit `EntityBase`.

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

## Testing

Web unit tests live under:

`tests/web/Ampere.UnitTests`

Shared fixtures and fakes live under
`Common/Fixtures` and `Common/Mocks`.
Also, shared Stubs goes to `Common/Stubs`.
Feature tests live in feature directories.

Coverlet collector generates Cobertura XML.
All the feature scope requires
at least 80 percent line and branch coverage.

The CI job publishes the coverage report in
the GitHub Actions summary and fails below
that threshold.

## Mobile

The mobile application follows the same
layered project structure:

- Presentation;
- Application;
- Domain;
- Infrastructure;
- Composition.

Platform-specific MAUI code remains inside
the Presentation project or appropriate
platform boundary.

## IoT and OS

`src/iot` is reserved for embedded device
software (`AmperEsp`).

`src/os` is reserved for Raspberry Pi OS
customization (`AmperOS`).

Neither area should introduce dependencies
from web Presentation into device software.

Integration contracts must be explicit.

## Project States and Data Flow

### General system states overview

```mermaid
stateDiagram-v2
    [*] --> SystemBoot

    state SystemBoot {
        LoadDatabase: Load embedded devices and SQlite/SQL/NVS states
        InitSerial: Start serial port (HC-12)
        StartSignalR: Start SignalR Hub
        LoadDatabase --> InitSerial
        InitSerial --> StartSignalR
    }

    SystemBoot --> DiscoveryMode: No nodes registered / App request
    SystemBoot --> NormalPolling: Nodes loaded from DB

    state DiscoveryMode {
        BroadcastPairBeacon: Ampere sends Pairing Beacon
        ListenPairResponse: Listen for answer of new AmperEsp's
        RegisterNode: Stores AmperEsp at SQLite/DB
        BroadcastPairBeacon --> ListenPairResponse
        ListenPairResponse --> RegisterNode: Answer received
    }

    DiscoveryMode --> NormalPolling: Done Pairing / Timeout

    state NormalPolling {
        PollDevice: Send POLL(Node_ID)
        WaitTelemetry: Waits RF Answer
        ProcessData: Updates DB and Sends SignalR
        
        PollDevice --> WaitTelemetry
        WaitTelemetry --> ProcessData: ACK / Telemetry received
        WaitTelemetry --> NodeTimeout: No Answer (Retry Count++)
        NodeTimeout --> PollNextNode: Retry limit exceeded de retries (Mark Offline)
        ProcessData --> PollNextNode
    }

    state EmergencyPowerRecovery {
        ReadLastKnownState: Read pre-fall load status
        RestoreRelays: AmperEsp restores state through EEPROM/NVS
        SyncState: Ampere sync states via RF
    }

    NormalPolling --> EmergencyPowerRecovery: Restart after power-loss
    EmergencyPowerRecovery --> NormalPolling: Validated states

    state FirmwareUpdate {
        NotifyApp: Notifies AmpereApp (Progress)
        TriggerWifiOTA: Sends command 'ENTER_OTA' through HC-12
        DeployBinary: Temporary binary upload through HTTP/Wi-Fi
        RebootDevice: AmperEsp reinicia e desativa Wi-Fi
    }

    NormalPolling --> FirmwareUpdate: Comando de atualização vindo do AmpereApp
    FirmwareUpdate --> NormalPolling: Update concluído / Fail-safe Timeout
```

### Setup, Discovery and Pairing Data Flow

```mermaid
sequenceDiagram
    autonumber
    participant App as AmpereApp (Mobile)
    participant Server as Ampere (Raspberry Pi)
    participant RF as HC-12 (Meio Físico)
    participant Node as AmperEsp (Sonoff Unpaired)

    Note over Node: Boot at Unpaired Mode (Passive Listening)
    App->>Server: POST /api/v1/discovery/start
    Server-->>App: 200 OK (Modo Pareamento Ativo)

    loop DiscoveryWindow (Ex: 600 segundos)
        Server->>RF: TX Broadcast: [CMD:DISCOVER_BEACON]
        Note over RF,Node: All nodes in range litens
        Node->>Node: Generates rando delay (0-500ms) to avoid colision
        Node->>RF: TX Response: [RESP:PAIR_REQ | MAC: AA:BB:CC:DD:EE:01]
        
        Server->>Server: Validates packet e gera uniqueID (Ex: Node_01)
        Server->>RF: TX Direct: [CMD:PAIR_ACK | MAC: AA:BB:CC:... | ASSIGNED_ID: 1]
        Node->>Node: Grava ASSIGNED_ID na EEPROM/NVS
        Node->>RF: TX Confirm: [RESP:PAIR_CONFIRM | ID: 1]

        Server->>Server: Stores AmperEsp no SQLite/SQL (Status: Active)
        Server->>App: SignalR Notify: NodeRegistered(Node_01)
    end

    Server->>App: SignalR Notify: DiscoveryFinished()
```

### Normal Operation: Telemetry Polling and Command Sending

```mermaid 
sequenceDiagram
autonumber
participant App as AmpereApp (Mobile)
participant Server as Ampere (Raspberry Pi)
participant RF as HC-12 (Half-Duplex)
participant Node1 as AmperEsp (ID: 01)
participant Node2 as AmperEsp (ID: 02)

rect rgb(240, 240, 240)
    Note over Server, Node2: Rapid Polling Cycle (Continuous Scan)
    Server->>RF: TX: [POLL | ID: 01]
    Note over RF: Only Node 01 processes
    Node1->>RF: TX: [DATA | ID: 01 | V: 221.5 | A: 1.2 | W: 265 | Relay: ON]
    Server->>Server: Log Telemetry to Local DB
    Server->>App: SignalR: TelemetryUpdated(Node_01, 265W)

    Server->>RF: TX: [POLL | ID: 02]
    Node2->>RF: TX: [DATA | ID: 02 | V: 220.0 | A: 0.0 | W: 0 | Relay: OFF]
    Server->>Server: Log Telemetry to Local DB
    Server->>App: SignalR: TelemetryUpdated(Node_02, 0W)
end

rect rgb(220, 240, 255)
    Note over App, Node1: User Command Interruption
    App->>Server: POST /api/v1/nodes/1/relay (Command: TOGGLE)
    Note over Server: Queue command for next RF turn
    Server->>RF: TX: [CMD_SET | ID: 01 | RELAY: OFF]
    Node1->>Node1: Change Relay pin state
    Node1->>RF: TX: [ACK_SET | ID: 01 | RELAY: OFF | CurrentW: 0]
    Server->>Server: Update Database
    Server->>App: SignalR: RelayStateChanged(Node_01, OFF)
end
```

### Firmware Update (OTA Transition State)

```mermaid
sequenceDiagram
    autonumber
    participant App as AmpereApp
    participant Server as Ampere (Raspberry)
    participant RF as HC-12
    participant Node as AmperEsp (Sonoff)

App->>Server: POST /api/nodes/1/firmware-update (File: firmware.bin)
Server->>App: SignalR: UpdateProgress(Node_01, 5% - "Starting Wi-Fi AP")

Server->>Server: Activates temporary Wi-Fi AP on Raspberry (SSID: AmpereOTA)

Server->>RF: TX: [CMD_OTA_MODE | ID: 01 | SSID: AmpereOTA | Pass: Secret123]
Node->>RF: TX: [ACK_OTA | ID: 01]

Note over Node: AmperEsp turns on Wi-Fi radio and connects to Raspberry AP

Node->>Server: HTTP GET /ota/firmware.bin (Via Wi-Fi)

loop Download and Flash Writing
Server->>Node: Sends binary chunks via HTTP Stream
Server->>App: SignalR: UpdateProgress(Node_01, percentage)
end

Node->>Node: Writes to Flash and validates MD5 Checksum
Node->>Server: HTTP POST /ota/status (Success)

Note over Node: AmperEsp reboots and disables Wi-Fi
Server->>Server: Disables temporary Wi-Fi AP

Server->>RF: TX: [POLL | ID: 01]
Node->>RF: TX: [DATA | ID: 01 | FW_Ver: 2.0.0]
Server->>App: SignalR: UpdateProgress(Node_01, 100% - "Successfully Completed")
``` 
