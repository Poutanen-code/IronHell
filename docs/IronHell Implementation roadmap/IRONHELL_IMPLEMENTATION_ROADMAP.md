# IronHell Implementation Roadmap

Status: Planning Document
Authority:
- Gameplay Architecture V2
- MAngband 1.5.3
- Repository audits completed during Architecture V2 stabilization

---

# Vision

IronHell aims to become a faithful MAngband 1.5.3 implementation using:

- Godot
- C#
- Deterministic server-authoritative simulation
- Data-driven gameplay definitions
- JSON content catalogs
- SQLite persistence

Priority order:

1. Verify MAngband behavior
2. Create canonical data definitions
3. Validate repository integrity
4. Implement deterministic simulation
5. Reach feature parity
6. Modernize only after parity

---

# Current State Assessment

## Completed

### Architecture

- Gameplay Architecture V2 established
- Source → Policy → Actions → Statuses model established
- Capability model established
- Resistance model established
- Canonical action catalog largely stabilized

### Research

Completed audits:

- Targeting Model Research
- Targeting Audit
- Policy Duration Audit
- Capability Audit
- Contract Audit
- Repository Integrity Audit
- Progression Audit

### Data

Major catalogs exist:

- Races
- Classes
- Race/Class rules
- Capabilities
- Resistances
- Statuses
- Actions
- Mage spells
- Priest prayers
- Consumables
- Potions
- Rods
- Wands
- Staves
- Scrolls
- Artifacts
- Activations
- Ego items

### Core Runtime

Implemented:

- CharacterFactory
- CharacterState
- Definition loading foundations

---

# Phase 0 — Repository Health

Goal:
Make repository internally trustworthy.

## Tasks

### Validation Infrastructure

- Repair missing schema dependencies
- Resolve missing effects schema references
- Ensure validation suite executes

### Repository Validation

Validate:

- action_id references
- status_id references
- capability_id references
- resistance_id references
- activation_id references
- race_id references
- class_id references

### Contract Validation

Validate:

- Required parameters
- Enum values
- Closed contracts
- Allowed value domains

### Remaining Drift

Resolve:

- InspectEntity probing contract mismatch
- Compatibility alias documentation
- Validation coverage gaps

## Exit Criteria

- Validation suite green
- No contract violations
- No broken references

---

# Phase 1 — Definition Completion

Goal:
Complete all major gameplay data domains.

## Terrain System

Create canonical:

terrain_definitions.json

Based partly on:

- docs/references and legacy/rewrite/terrain_features.json

Represent:

- Floors
- Walls
- Doors
- Secret doors
- Permanent walls
- Veins
- Stairs
- Wilderness terrain
- Shops
- Traps

### Research Needed

- MAngband cave feature behavior
- LOS blocking behavior
- Terrain destruction behavior

---

## Trap System

Create:

traps.json

Represent:

- Trap doors
- Pits
- Poison pits
- Dart traps
- Sleep gas
- Blindness gas
- Confusion gas
- Summoning traps
- Teleport traps

---

## Vault System

Create:

vaults.json

Source:

- docs/references and legacy/rewrite/vault_templates.json

Add:

- Metadata
- Rating
- Tags
- Placement rules
- Symbol legends

Research:

- MAngband vault generation logic
- Vault rarity and placement

---

## Store System

Create:

stores.json

Represent:

- General Store
- Armory
- Weapon Smith
- Temple
- Alchemist
- Magic Shop
- Black Market
- Home

Research:

- Inventory generation
- Turnover logic
- Pricing modifiers

## Exit Criteria

All major gameplay definition domains exist.

---

# Phase 2 — Core Simulation Engine

Goal:
Create deterministic simulation foundation.

## Systems

Implement:

- GameState
- WorldState
- DungeonState
- Turn Scheduler
- Energy Model
- RNG Service

### Research

- MAngband energy ordering
- Action scheduling
- Tick advancement

## Exit Criteria

Player can exist in a dungeon and process turns.

---

# Phase 3 — Map and Visibility

Goal:
Playable dungeon navigation.

## Systems

Implement:

- Dungeon grid
- Terrain placement
- Movement
- Collision
- Field of View
- Line of Sight
- Memory map

### Research

- MAX_SIGHT behavior
- LOS calculations
- Visibility updates

## Exit Criteria

Player movement and vision functioning.

---

# Phase 4 — Inventory and Equipment

Goal:
Items become usable.

## Systems

Implement:

- Inventory
- Equipment slots
- Pickup
- Drop
- Equip
- Unequip
- Stack handling

## Support

Use:

- Ego items
- Artifacts
- Affixes

## Exit Criteria

Equipment system fully operational.

---

# Phase 5 — Status, Capability and Resistance Engine

Goal:
Gameplay modifiers become functional.

## Statuses

Implement:

- Buffs
- Debuffs
- Timed counters

## Capabilities

Implement:

- bearer_passive
- native_identity
- item_self_passive

## Resistances

Implement:

- Resist
- Oppose
- Immunity
- Ignore

## Exit Criteria

Capabilities and resistances influence gameplay.

---

# Phase 6 — Action Execution System

Goal:
Canonical actions become executable.

## Execute

Actions:

- Damage
- Healing
- Teleport
- Detection
- Summoning
- Terrain modification
- Status application
- Branding
- Identification

## Exit Criteria

Action catalog executable.

---

# Phase 7 — Spell and Device System

Goal:
Magic becomes playable.

## Sources

Implement:

- Mage books
- Priest books
- Potions
- Scrolls
- Wands
- Rods
- Staves

## Runtime

Implement:

- Runtime formulas
- Targeting
- Cooldowns
- Charges

## Exit Criteria

All player spell sources functional.

---

# Phase 8 — Monsters

Goal:
Core roguelike gameplay loop.

## Systems

Implement:

- Monster definitions
- Monster instances
- AI
- Pathfinding
- Combat
- Summoning
- Fear
- Sleep
- Speed

## Research

- MAngband monster AI
- Monster spell selection
- Monster energy system

## Exit Criteria

Monster gameplay fully functional.

---

# Phase 9 — Combat

Goal:
Complete combat simulation.

## Systems

Implement:

- Melee
- Ranged
- Criticals
- Slays
- Brands
- Damage mitigation
- Death

## Data Sources

- Weapons
- Ego items
- Artifacts
- Resistances
- Capabilities

## Exit Criteria

Combat parity approaching MAngband.

---

# Phase 10 — Dungeon Generation

Goal:
Generate complete playable dungeons.

## Systems

Implement:

- Rooms
- Tunnels
- Streams
- Veins
- Traps
- Stairs
- Vaults

Use:

- terrain_definitions.json
- vaults.json

## Exit Criteria

Random dungeon generation complete.

---

# Phase 11 — Persistence

Goal:
Long-term gameplay.

## Systems

Implement:

- Character save
- Dungeon save
- Migrations
- Recovery

Use:

- SQLite

## Exit Criteria

Persistent gameplay operational.

---

# Phase 12 — Artifact Activations

Goal:
Artifact special behavior.

## Systems

Implement chain:

Artifact
→ Activation
→ Action Execution

## Research

Validate every activation against MAngband.

## Exit Criteria

All artifact activations functional.

---

# Phase 13 — Multiplayer Foundation

Goal:
First playable cooperative game.

## Systems

Implement:

- Sessions
- Commands
- Synchronization
- Visibility
- Shared dungeon

## Exit Criteria

Multiple players in same dungeon.

---

# Phase 14 — MAngband Parity Completion

Goal:
Close remaining gameplay gaps.

## Systems

Review:

- Parties
- Ghosts
- Recall
- Death handling
- Store edge cases
- Artifact edge cases
- Rare monster behavior

## Exit Criteria

Feature parity target reached.

---

# Deferred Until After Parity

Do NOT prioritize:

- Targeting redesign
- Activation redesign
- Modernized combat systems
- New content
- New classes
- New races
- New progression systems
- MMO features
- Crafting systems
- Housing systems
- Alternate advancement systems

These are post-parity concerns.

---

# Current Recommended Next Milestone

Phase 1:

Definition Completion

Priority order:

1. Terrain Definitions
2. Trap Definitions
3. Store Definitions
4. Vault Definitions
5. Monster Definitions Research

Reason:

These are the largest remaining gameplay-data gaps before simulation implementation begins.