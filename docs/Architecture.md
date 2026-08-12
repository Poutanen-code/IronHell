# IronHell Architecture
**Status:** Draft

## Purpose
IronHell is a server-authoritative, online cooperative 2D fantasy roguelike. The initial goal is faithful MAngband 1.5.3 feature parity before modernization or new gameplay.

Priorities, in order:

1. Verified parity behavior
2. Solo-developer maintainability
3. Deterministic, testable rules
4. Multiplayer correctness
5. Simplicity
6. Performance and future scale

## Technology Baseline
- Godot 4.x with C#
- JSON for static game definitions
- SQLite for persistent runtime state
- MessagePack as a network serialization candidate
- xUnit for automated tests
- Git for version control

Transport, server host, authentication, and release integrations remain provisional until needed and validated by small prototypes.

## Architecture
```text
Godot Clients
     |
 commands / authoritative updates
     |
IronHell.Server
     |
IronHell.Application
     |
IronHell.Core
   /       JSON       SQLite
Definitions Persistence
```

IronHell begins as a modular monolith: one server process, one database, and clear internal boundaries. Do not introduce microservices or distributed infrastructure without measured need.

## Core Rules

### Server Authority
The server owns and validates movement, time and energy, combat, monsters, items, inventory, progression, visibility, dungeon state, and persistence.

Clients send intentions such as `Move`, `Attack`, or `UseItem`. They never set authoritative position, health, damage, experience, or inventory.

### Godot-Independent Core
`IronHell.Core` contains deterministic domain rules and must not reference Godot, networking, files, databases, rendering, UI, or wall-clock APIs.

Core behavior must run through ordinary .NET unit tests without launching Godot.

### Explicit Simulation Time
Simulation time is server-controlled and independent of frame rate, Godot callbacks, wall-clock time, and client input timing. The exact MAngband energy and action-ordering model must be verified before the main simulation loop is finalized.

### Data-Driven Definitions
Static content is loaded from validated JSON. Examples include races, classes, monsters, item kinds, spells, dungeon features, stores, flags, and progression tables.

Definitions describe content types; runtime entities describe instances. For example, `ItemInstance` references an `ItemKindId`.

Use stable machine IDs, never display names, for references. Unknown references, duplicate IDs, invalid ranges, and unknown flags must fail startup with clear errors.

### Determinism
Given the same initial state, ordered commands, definitions, configuration, and random seed, the core should produce the same result. Randomness must be injected and reproducible.

## Project Boundaries
- **IronHell.Core:** domain model and simulation rules
- **IronHell.Application:** use cases, command handling, and orchestration
- **IronHell.Data:** JSON loading, validation, and definition catalogs
- **IronHell.Persistence:** SQLite, migrations, and repositories
- **IronHell.Protocol:** explicit versioned network DTOs
- **IronHell.Server:** sessions, authoritative simulation host, saving, and diagnostics
- **IronHell.Client:** Godot presentation, input, audio, and networking adapter
- **Tests:** fast unit tests and focused boundary integration tests

Dependencies point inward toward `IronHell.Core`. Circular references are forbidden.

Forbidden dependencies include:

```text
Core -> Godot
Core -> SQLite
Core -> Protocol
Core -> Server
Core -> Client
```

## Data Ownership
- **JSON:** human-editable static definitions
- **SQLite:** durable mutable character and world state
- **Server memory:** active authoritative simulation state
- **Client memory:** non-authoritative presentation state

Game-definition, database, save, and protocol formats must be explicitly versioned. Persistent schema changes require migrations.

## Commands and Results
Commands request actions; they do not guarantee success.

```text
Command
-> validation and authorization
-> simulation
-> state change
-> explicit result/events
-> relevant client updates
```

Expected gameplay rejection uses explicit results such as `MovementBlocked` or `NotEnoughEnergy`. Invalid definitions fail startup. Unexpected programming or infrastructure failures use exceptions.

## Persistence
SQLite is the initial persistence store. Persistence must support migrations, transactions, constraints, backups, safe shutdown, crash-aware saving, and save/load round-trip tests.

Domain objects do not execute SQL. Godot Nodes, scenes, and visual state are never persisted as authoritative game state.

## Networking
Transport and server-host choices remain open. Protocol messages must be explicit DTOs separate from domain entities and must be versioned, validated, authorized, and size-limited.

Correctness and debuggability take priority over bandwidth optimization.

## Testing
Fast deterministic unit tests are the default for rules and validators. Use real integration tests only at boundaries such as SQLite, serialization, files, and networking. Godot visuals are primarily checked manually.

Behavior-changing work is incomplete until relevant tests pass. Parity-sensitive tests must record whether expected behavior is verified, inferred, IronHell-specific, or unresolved.

## Initial Milestones
1. Load and validate race/class definitions.
2. Create a character entirely from definitions.
3. Save, restart, load, and verify the character.
4. Add a headless floor, movement, energy, and visibility.
5. Add a thin Godot presentation adapter.
6. Add a combat vertical slice.
7. Connect two clients to one authoritative server.
8. Continue in small slices toward verified feature parity.

## Open Decisions
- Canonical MAngband 1.5.3 reference source
- Exact Godot release
- Energy, timing, and command ordering
- Headless Godot versus standalone .NET server
- Network transport and protocol version policy
- Initial tested player count
- Save frequency and persistent-floor lifecycle
- Authentication and account model
- Licensing and content-reuse boundaries
- Steam integration

Resolve these only when needed, using a small prototype or Architecture Decision Record.

## Success Criteria
The architecture succeeds when:

- Core rules run without Godot.
- Static content is validated and data-driven.
- Character and world state survive restart.
- Verified gameplay behavior is reproducible through tests.
- Multiple clients share one authoritative world.
- One developer can understand, test, deploy, and maintain the system.

### Simulation and Presentation Separation

Gameplay simulation is authoritative and deterministic.

Presentation is a visual interpretation of simulation results.

Core must never depend on:
- Rendering
- Animation
- Audio
- Frame rate
- Delta time
- Physics callbacks

Clients may animate, interpolate, and decorate state changes, but presentation must never alter gameplay outcomes.

Modernization should occur primarily in presentation while preserving simulation behavior.