# IronHell Implementation Roadmap

Gameplay Architecture V2 normalization is complete. Development now focuses on deterministic runtime implementation with MAngband 1.5.3 parity.

## Phase 1: CharacterRoller

**Objective:** Implement deterministic stat, HP, physical-characteristic, and background rolling.

**Dependencies:** Character creation specification, verified MAngband source behavior, and an injectable random-source abstraction.

**Acceptance Criteria:** Seeded rolls are reproducible; all ranges and roll branches are tested; random consumption order is documented.

**Parity Risks:** Incorrect distributions, ordering, bounds, or random consumption.

## Phase 2: CharacterCreationService

**Objective:** Assemble a valid character from input, rolled values, race/class rules, history, starting equipment, and resources.

**Dependencies:** CharacterRoller, race/class definitions, character creation specification, and item definitions.

**Acceptance Criteria:** Valid inputs produce deterministic characters; invalid inputs are rejected; starting inventory, gold, history, and initial resources are tested.

**Parity Risks:** Incorrect starting equipment, resource defaults, history handling, or validation order.

## Phase 3: Character Runtime Model

**Objective:** Define authoritative mutable character state without duplicating catalog ownership.

**Dependencies:** CharacterCreationService and Gameplay Architecture V2 ownership rules.

**Acceptance Criteria:** State represents identity, base stats, resources, inventory, equipment, statuses, and progression with clear ownership boundaries.

**Parity Risks:** Mixing definitions with runtime state or storing derived values as competing truth.

## Phase 4: StatCalculator

**Objective:** Calculate derived statistics from base character state and modifiers.

**Dependencies:** Character runtime model, item affixes, and verified MAngband stat formulas.

**Acceptance Criteria:** Derived values are deterministic, composable, and covered by focused tests for caps, ordering, and modifiers.

**Parity Risks:** Wrong legacy formulas, modifier precedence, caps, or rounding.

## Phase 5: CapabilityResolver

**Objective:** Resolve native identity, bearer, and item-self capabilities while keeping resistances distinct.

**Dependencies:** Character model, race/class definitions, equipment model, capability catalog, and resistance ownership rules.

**Acceptance Criteria:** Scoped capabilities resolve without accidental propagation; resistance channels remain separate; precedence is tested.

**Parity Risks:** Flattening scopes, treating resistance as capability, or losing policy hooks.

## Phase 6: Equipment Runtime

**Objective:** Implement inventory, equipment slots, stacking, charges, activations, and equipment transitions.

**Dependencies:** Character model, item definitions, StatCalculator, and CapabilityResolver.

**Acceptance Criteria:** Equip, unequip, stack, split, charge, and activation operations preserve valid state and update resolution inputs deterministically.

**Parity Risks:** Slot rules, stack behavior, device economies, activation eligibility, and item-self effects.

## Phase 7: ResolvedActorState

**Objective:** Produce the complete combat-ready actor projection.

**Dependencies:** StatCalculator, CapabilityResolver, Equipment Runtime, and timed status model.

**Acceptance Criteria:** A stable projection contains derived stats, capabilities, resistances, statuses, and available actions with no duplicated effects.

**Parity Risks:** Stale projections, incorrect precedence, or duplicated ownership.

## Phase 8: Combat Foundation

**Objective:** Implement deterministic action validation, targeting, damage, resistance, status application, and basic melee/ranged behavior.

**Dependencies:** ResolvedActorState, canonical actions, energy rules, and MAngband combat evidence.

**Acceptance Criteria:** Basic combat flows pass deterministic behavior tests for targeting, damage, mitigation, status duration, and turn cost.

**Parity Risks:** Damage formulas, slay/kill/brand precedence, immunity, timing, targeting, and status interlocks.
