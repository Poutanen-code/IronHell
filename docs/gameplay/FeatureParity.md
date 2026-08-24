# IronHell Feature Parity

**Status:** Draft inventory  
**Reference target:** MAngband 1.5.3  
**Purpose:** Track verified scope; this file is not proof that listed behavior is understood or implemented.

## Status Values

- `Unknown` — not yet researched
- `Documented` — expected behavior and source recorded
- `Tested` — executable parity tests exist
- `Partial` — some behavior implemented
- `Implemented` — implementation passes current parity tests
- `Deferred` — intentionally outside the current milestone
- `Excluded` — intentionally not planned, with rationale recorded

## Evidence Values

- `Verified` — confirmed from the canonical 1.5.3 source or data
- `Observed` — reproduced by running the original version
- `Derived` — transformed from verified material with documented steps
- `Inferred` — plausible but not confirmed
- `IronHell` — deliberate IronHell-specific behavior
- `Unresolved` — conflicting or missing evidence

## Rules

- Do not mark a feature `Documented` without a source reference.
- Do not mark a feature `Implemented` without automated tests where practical.
- Do not invent behavior to fill gaps; mark it `Unresolved`.
- Record known original bugs separately and decide whether parity requires preserving them.
- Separate gameplay parity from presentation and modernization.
- Link detailed rules to `GameRules.md`; do not duplicate large specifications here.

## Canonical Reference

- Repository/archive: `TBD`
- Version/tag/commit: `MAngband 1.5.3 — exact reference TBD`
- Authoritative data directory: `TBD`
- Build/runtime environment used for observation: `TBD`
- License and attribution review: `TBD`

## Parity Inventory

| Area | Feature | Status | Evidence | Source / Test | Notes |
|---|---|---:|---:|---|---|
| Character | Character creation flow | Unknown | Unresolved | TBD | |
| Character | Races and race restrictions | Partial | Inferred | Existing JSON; verify | Draft data exists |
| Character | Classes and class restrictions | Partial | Inferred | Existing JSON; verify | Draft data exists |
| Character | Stats and derived values | Unknown | Unresolved | TBD | |
| Character | Skills and skill growth | Unknown | Unresolved | TBD | |
| Character | Age, height, weight, history | Unknown | Unresolved | TBD | |
| Character | Starting equipment and gold | Unknown | Unresolved | TBD | |
| Character | Experience and leveling | Unknown | Unresolved | TBD | |
| Character | Titles | Unknown | Unresolved | TBD | |
| Time | Energy accumulation | Unknown | Unresolved | High priority | Blocks main loop |
| Time | Player and monster action order | Unknown | Unresolved | High priority | |
| Time | Idle, AFK, and disconnected behavior | Unknown | Unresolved | High priority | |
| Time | World behavior with no players | Unknown | Unresolved | TBD | |
| World | Town layout and behavior | Unknown | Unresolved | TBD | |
| World | Dungeon generation | Unknown | Unresolved | TBD | |
| World | Floor creation and lifecycle | Unknown | Unresolved | TBD | |
| World | Terrain and feature rules | Unknown | Unresolved | TBD | |
| World | Stairs, depth, and transitions | Unknown | Unresolved | TBD | |
| World | Lighting, vision, and line of sight | Unknown | Unresolved | TBD | |
| World | Traps, doors, tunneling, and searching | Unknown | Unresolved | TBD | |
| Movement | Movement and collision | Unknown | Unresolved | TBD | |
| Movement | Running, resting, and repeated commands | Unknown | Unresolved | TBD | |
| Combat | Melee attacks | Unknown | Unresolved | TBD | |
| Combat | Ranged attacks and ammunition | Unknown | Unresolved | TBD | |
| Combat | Armor, hit chance, and damage | Unknown | Unresolved | TBD | |
| Combat | Critical hits and special attacks | Unknown | Unresolved | TBD | |
| Combat | Player death and revival | Unknown | Unresolved | TBD | |
| Combat | PvP rules, if any | Unknown | Unresolved | TBD | Determine exact scope |
| Effects | Resistances, immunities, and sustains | Unknown | Unresolved | TBD | |
| Effects | Buffs, debuffs, and timed effects | Unknown | Unresolved | TBD | |
| Effects | Hunger, poison, cuts, and stun | Unknown | Unresolved | TBD | |
| Magic | Spell books and spell access | Unknown | Unresolved | TBD | |
| Magic | Learning, casting, failure, and mana | Unknown | Unresolved | TBD | |
| Magic | Spell targeting and effects | Unknown | Unresolved | TBD | |
| Items | Base item kinds | Unknown | Unresolved | TBD | |
| Items | Item generation and quality | Unknown | Unresolved | TBD | |
| Items | Artifacts and ego items | Unknown | Unresolved | TBD | |
| Items | Identification and pseudo-identification | Unknown | Unresolved | TBD | |
| Items | Inventory, equipment, stacking, and weight | Unknown | Unresolved | TBD | |
| Items | Pickup, drop, destroy, give, and trade | Unknown | Unresolved | TBD | |
| Items | Charges, fuel, activation, and recharging | Unknown | Unresolved | TBD | |
| Monsters | Monster definitions and spawning | Unknown | Unresolved | TBD | |
| Monsters | Movement, perception, and targeting | Unknown | Unresolved | TBD | |
| Monsters | Attacks, spells, and special abilities | Unknown | Unresolved | TBD | |
| Monsters | Sleep, fear, reproduction, and status | Unknown | Unresolved | TBD | |
| Monsters | Unique monsters and persistence | Unknown | Unresolved | TBD | |
| Multiplayer | Connections and sessions | Unknown | Unresolved | TBD | |
| Multiplayer | Shared world and floor occupancy | Unknown | Unresolved | TBD | |
| Multiplayer | Player visibility and updates | Unknown | Unresolved | TBD | |
| Multiplayer | Parties and party experience | Unknown | Unresolved | TBD | |
| Multiplayer | Chat and social commands | Unknown | Unresolved | TBD | |
| Multiplayer | Trading and item ownership | Unknown | Unresolved | TBD | |
| Multiplayer | Disconnect and reconnect behavior | Unknown | Unresolved | TBD | |
| Multiplayer | Simultaneous commands and conflicts | Unknown | Unresolved | TBD | |
| Persistence | Character save/load | Unknown | Unresolved | TBD | IronHell uses SQLite |
| Persistence | Inventory and equipment persistence | Unknown | Unresolved | TBD | |
| Persistence | World and floor persistence | Unknown | Unresolved | TBD | |
| Persistence | Houses and ownership | Unknown | Unresolved | TBD | Verify presence and behavior |
| Persistence | Unique and artifact state | Unknown | Unresolved | TBD | |
| Economy | Stores and services | Unknown | Unresolved | TBD | |
| Economy | Buying, selling, pricing, and stock | Unknown | Unresolved | TBD | |
| Economy | Gold and player economy | Unknown | Unresolved | TBD | |
| UI | Character and inventory information | Unknown | Unresolved | TBD | Presentation may differ |
| UI | Map symbols, colors, and messages | Unknown | Unresolved | TBD | Separate semantic parity from visuals |
| UI | Commands, keybindings, and targeting | Unknown | Unresolved | TBD | Modern UI may be layered later |
| Admin | Server configuration | Unknown | Unresolved | TBD | |
| Admin | Operator and moderation commands | Unknown | Unresolved | TBD | |
| Admin | Logging, shutdown, and recovery | Unknown | Unresolved | TBD | IronHell implementation may differ |
| Endgame | Winning condition | Unknown | Unresolved | TBD | |
| Endgame | Retirement, scoring, and high scores | Unknown | Unresolved | TBD | |

## Known Deviations

Record deliberate deviations only after a decision is made.

| ID | Original behavior | IronHell behavior | Reason | ADR |
|---|---|---|---|---|
| None | | | | |

## Known Original Bugs

| ID | Behavior | Evidence | Preserve? | Decision |
|---|---|---|---:|---|
| None recorded | | | TBD | |

## Current Priority

1. Select and record the canonical MAngband 1.5.3 source.
2. Verify existing race/class JSON against it.
3. Research energy, timing, and action ordering.
4. Document character creation rules.
5. Convert verified rules into fast deterministic tests.
6. Implement the first character create/save/reload vertical slice.

## Definition of Feature Parity

IronHell reaches initial feature parity when:

- Every in-scope inventory row is `Implemented` or has an approved deviation.
- Gameplay-critical behavior has documented evidence.
- Automated parity tests cover deterministic rules where practical.
- Multiplayer behavior is validated with at least two clients.
- Persistent state survives restart according to documented rules.
- Remaining differences are listed explicitly rather than hidden.
