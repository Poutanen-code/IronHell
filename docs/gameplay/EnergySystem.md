# Energy System

Status: Partially Resolved

## Purpose

Document how MAngband 1.5.3 schedules actions, grants energy, determines turn order, and resolves simultaneous activity.

Energy rules affect:

- Movement
- Combat
- Spellcasting
- Resting
- Monster AI
- Multiplayer fairness
- Deterministic replay

This document serves as the primary reference for IronHell energy implementation.

---

## Classification

### Verified

- MAngband uses an energy-based action system rather than strict alternating turns. Actors accumulate energy and may only act when they possess sufficient energy. 【1-97ad53】【2-451991】

- Energy availability is checked before actors are processed. Historical bug discussions explicitly reference actors requiring sufficient energy before actions may occur. 【1-97ad53】

- MAngband 1.5.3 includes an optional `energy_buildup` feature allowing players to accumulate up to twice the normal amount of energy while standing still. This option was restored in the 1.5.3 release. 【3-6d94c8】

- MAngband 1.5.3 introduced "partial blows per round" derived from newer Angband versions. Individual melee blows consume a fraction of a full turn's energy instead of unleashing all blows at once after waiting for an entire turn. 【2-451991】【3-6d94c8】

- MAngband operates as a real-time multiplayer game driven by a server frame loop. Server FPS is configurable and defaults to 75 FPS in the shipped configuration. 【4-47f8bc】【5-3f901e】

### Observed

- Energy accumulation appears continuous relative to the server simulation loop rather than being synchronized to player input timing. 【5-3f901e】【4-47f8bc】

- Faster actors act more frequently because energy gain is speed-dependent, consistent with Angband-derived mechanics discussed throughout MAngband development notes. 【2-451991】

### Derived

- For IronHell, energy should be represented as an integer resource held by each actor.

- Action execution should consume energy rather than advancing a global turn counter.

- The simulation should process actors according to available energy, not according to frame order.

- Energy handling belongs entirely inside IronHell.Core.

### Inferred

These remain informed assumptions until verified from source:

- Base actor speed is likely inherited from Angband conventions where speed modifies energy acquisition rather than reducing action cost.

- A "full turn" energy value exists and represents the baseline cost for common actions such as movement.

- Movement and most standard actions likely consume one full-turn worth of energy.

- Turn ordering is likely stable and deterministic based on actor processing order when multiple actors qualify simultaneously.

### IronHell-Specific

None.

### Unresolved

The exact numeric values remain to be confirmed from MAngband 1.5.3 source:

- Full-turn energy amount.
- Exact speed-to-energy conversion table.
- Exact player base speed.
- Exact monster base speed.
- Precise movement energy cost.
- Precise spellcasting energy costs.
- Tie-breaking rules when several actors become eligible in the same tick.
- Resting energy behaviour.
- Interaction with stun/slowness/haste.
- Exact implementation of energy_buildup limits.

---

## Sources

### Primary Sources

- MAngband 1.5.3 release notes.
- MAngband issue #1323 (Partial blows per round).
- MAngband issue #653 (Energy processing discussion).
- MAngband server configuration documentation.

### Reliability

Current confidence:

- High confidence:
  - Energy-gated actions.
  - Partial blows.
  - Energy buildup option.
  - Frame-driven server simulation.

- Medium confidence:
  - General Angband-derived energy model.

- Low confidence:
  - Exact numerical constants.

---

## Research Answers

### 1. How much energy is gained per tick?

Status: Unresolved.

Energy gain is speed-dependent, but the exact formula and lookup table require direct source verification. Available public documentation does not expose the precise calculation. 【2-451991】【1-97ad53】

### 2. What energy threshold enables action?

Status: Partially Resolved.

A full-turn energy threshold exists and actors must possess sufficient energy before acting. The exact numeric threshold is not yet verified. 【1-97ad53】【2-451991】

### 3. Does movement cost equal attack cost?

Status: Partially Resolved.

Historically, standard attacks consumed a full turn. MAngband 1.5.3 introduces partial blows, allowing individual melee attacks to spend only part of a full turn's energy budget. Therefore attack cost is no longer always equivalent to a movement action. 【2-451991】【3-6d94c8】

### 4. How is speed applied?

Status: Inferred.

Speed appears to affect energy accumulation rate rather than directly reducing action cost. Exact formulas remain unresolved.

### 5. How are ties resolved?

Status: Unresolved.

No source found yet describing simultaneous eligibility handling or stable ordering guarantees.

### 6. How closely does MAngband differ from Angband?

Status: Partially Resolved.

MAngband largely retains the Angband energy model but modifies it for multiplayer operation. MAngband 1.5.3 additionally includes:

- Partial blows per round.
- Energy buildup option.
- Real-time server simulation.

These are known deviations or extensions from earlier behaviour. 【2-451991】【3-6d94c8】

---

## Candidate Test Cases

### Energy Threshold

Given:
- Actor below required energy.

Verify:
- No action occurs.

### Energy Availability

Given:
- Actor reaches required energy.

Verify:
- Exactly one eligible action may execute.

### Partial Blows

Given:
- Actor with multiple blows per round.

Verify:
- Individual blows consume partial energy rather than executing all blows together.

### Energy Buildup

Given:
- Actor remains idle.

Verify:
- Energy never exceeds documented buildup limit.

### Deterministic Ordering

Given:
- Fixed seed.
- Fixed actor list.
- Fixed commands.

Verify:
- Identical replay outcome.

---

## Planned Acceptance Criteria

Before EnergySystem becomes "Verified":

- Exact MAngband 1.5.3 source files identified.
- Energy gain formula documented.
- Speed table documented.
- Full-turn energy value documented.
- Tie-breaking documented.
- Idle energy buildup documented.
- Deterministic replay tests implemented.

## Architectural Acceptance Criteria

The Energy System design succeeds when:

- Energy scheduling is implemented entirely in Core.
- Action legality does not depend on animation state.
- Simulation results are reproducible without Godot.
- Clients can change animation timing without affecting gameplay.
- Multiplayer synchronization transmits state changes, not animation state.
- Presentation can be modernized without changing simulation behavior.