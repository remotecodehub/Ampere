# Skill: maui

## Use when

Use for the .NET MAUI application.

## Rules

Use .NET 10.0.
Keep presentation in MAUI components.
Use the same Application, Domain and
Infrastructure boundaries as the web app.

Organize application behavior by feature.
Keep platform-specific code under
`Platforms` when required.

Do not put server-only concerns in the
mobile presentation layer.

The app communicates with the local Ampere
server through explicit contracts.

Respect offline and local-first goals.
Do not make public cloud access mandatory.

Propagate cancellation.
Dispose owned resources.
Document relevant APIs in en-US XML.
