# Canonical Gameplay Model Research

Date: 2026-08-03  
Scope: Viability evaluation of Source -> Policy -> Actions -> Statuses model using MAngband 1.5.3 evidence

## Research Question

Can MAngband gameplay systems be represented more faithfully by:

Source -> Policy -> Actions -> Statuses

than by:

Source -> Actions -> Statuses

## Evidence Inputs

- Spell and prayer command policy and lifecycle: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L43), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1607)
- Spell and prayer payload dispatch tables and switches: [reference-mangband-1_5_3/src/server/x-spell.c](reference-mangband-1_5_3/src/server/x-spell.c#L245), [reference-mangband-1_5_3/src/server/x-spell.c](reference-mangband-1_5_3/src/server/x-spell.c#L878)
- Monster AI spell policy and spell effects: [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L1620)
- Devices and consumables policy and payload: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1558)
- Activations policy and payload: [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L175)
- Trap trigger and payload: [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L1298), [reference-mangband-1_5_3/src/server/tables.c](reference-mangband-1_5_3/src/server/tables.c#L2184)

## Source Category Evaluation

## 1. Spells

### Policy Evidence

- Class realm and tval gate.
- Per-spell level, mana, fail rate, experience metadata.
- Learn, forget, remember lifecycle logic.
- Cast legality and mana checks before payload.

Evidence: [reference-mangband-1_5_3/lib/edit/p_class.txt](reference-mangband-1_5_3/lib/edit/p_class.txt#L20), [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L500), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1650)

### Action Evidence

- Direct payload switch dispatch for bolts, balls, buffs, teleports, map effects, summons, and terrain changes.

Evidence: [reference-mangband-1_5_3/src/server/x-spell.c](reference-mangband-1_5_3/src/server/x-spell.c#L878)

### Verdict

- Source -> Policy -> Actions -> Statuses is strongly supported.
- Source -> Actions -> Statuses loses lifecycle fidelity.

## 2. Monsters

### Policy Evidence

- Spell choice is AI-mediated with context-sensitive constraints.
- Monster capabilities and tactic decisions are pre-dispatch policy.

Evidence: [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L1620)

### Action Evidence

- Selected monster spells dispatch bolts, balls, debuffs, summons, and terrain effects.

Evidence: [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L1706)

### Verdict

- Model is supported with mandatory explicit policy layer.

## 3. Devices (Rods, Wands, Staves)

### Policy Evidence

- Rod timeout and stack availability checks.
- Wand and staff charge management and depletion behavior.
- Item identity and known-status feedback coupling.

Evidence: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L2391), [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L2242), [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1958)

### Action Evidence

- Payload dispatch overlaps with spell payloads for damage, detection, teleport, control, and terrain effects.

Evidence: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1558)

### Verdict

- Model supported; policy carries device-economy semantics absent from action layer.

## 4. Consumables

### Policy Evidence

- Consumption source checks and inventory effects.
- Side effects include negative status outcomes for bad consumables.

Evidence: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L648)

### Action Evidence

- Potions and scrolls produce healing, curing, teleport, mapping, enchanting, and buffs.

Evidence: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L801)

### Verdict

- Model supported; consumables show lighter policy than spells or devices.

## 5. Activations

### Policy Evidence

- Activation cooldown and item constraints.
- Equipment or artifact identity preconditions.

Evidence: [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L175)

### Action Evidence

- Effects include resistances, detection, lighting, healing, summoning, and damage.

Evidence: [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L468)

### Verdict

- Model supported with small but explicit policy gate.

## 6. Traps

### Policy Evidence

- Trigger logic based on chest trap flags and trap context.
- Damage and status outcomes may be partly unavoidable once triggered.

Evidence: [reference-mangband-1_5_3/src/server/tables.c](reference-mangband-1_5_3/src/server/tables.c#L2184), [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L1298)

### Action Evidence

- Effects include poison, blind, confusion, paralysis, stat loss, summon, teleport, and explosions.

Evidence: [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L1320)

### Verdict

- Model supported; trap policy is trigger-centric and distinct from caster-driven policy.

## Overall Assessment

## Model Success Conditions

The Source -> Policy -> Actions -> Statuses model succeeds when:

1. Actions remain pure payload definitions.
2. Policy remains source-bound and separate from payload definitions.
3. Status mutations are centralized and reusable.
4. Source-specific economies (mana, charges, cooldowns, AI decisions, trap triggers) are modeled independently.

## Model Failure Conditions

The model fails if:

1. Spell lifecycle rules are pushed into generic action payloads.
2. Monster AI selection logic is flattened into source-less action calls.
3. Device charge and timeout semantics are treated as generic status effects.
4. Trap triggers are represented as normal user-cast actions.

## G-13 Integration

G-13 Canonical Effect Inventory is viable as an Actions registry, but must be consumed through source-specific Policy adapters.

## Research Conclusion

- Recommendation: adopt Source -> Policy -> Actions -> Statuses as canonical architecture for parity-focused modernization.
- Confidence: high for structural fit, medium for complete effect enumeration pending full spell switch extraction and full activation catalog cross-check.

