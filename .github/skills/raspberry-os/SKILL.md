# Skill: raspberry-os

## Use when

Use for `src/os` and Raspberry Pi appliance
work.

## Goals

Provide a reproducible local Ampere host.
The host should provide the services needed
by the installed solution without a mandatory
public cloud.

Keep OS customization separate from domain
logic.

Prefer deterministic provisioning.
Document system dependencies and service
boundaries.

Do not assume internet availability after
installation.

When scripts or services are added, document
configuration, startup and failure behavior.

Security-sensitive configuration must not
be committed as secrets.
