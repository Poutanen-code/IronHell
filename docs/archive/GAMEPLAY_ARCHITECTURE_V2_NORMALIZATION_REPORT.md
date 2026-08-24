# Gameplay Architecture V2 Normalization Report

Status: Complete
Authority: MAngband 1.5.3 runtime evidence and canonical IronHell catalogs

## 1. Final Architecture

The normalized model is:

`Sources -> Policies -> Actions -> Statuses`

Capabilities represent binary bearer or item-self rules. Resistances own channel-based mitigation taxonomy. Policies own runtime formulas, probability gates, turn processing, combat precedence, and source-specific behavior. Actions represent invocable effects and compositions.

## 2. Ownership Conclusions

- Affixes own numeric additive item modifiers.
- Capabilities own binary rule-gating state, including sustains, free action, hold life, telepathy, impact, and related flags.
- Resistances own permanent resist, timed oppose, immunity, and item-self ignore channels.
- Statuses own timed counters and temporary active state.
- Policies own slay/kill/brand resolution, immunity checks, passive curses, cooldowns, and other runtime rules.
- Actions own effect execution; activation records compose actions and do not own runtime state.

Capability/resistance and legacy status aliases were normalized so compatibility references do not become competing semantic definitions. `MIGHT`, `SHOTS`, and `BLOWS` remain active affix semantics. `IMPACT`, `AGGRAVATE`, `TELEPORT`, and `DRAIN_EXP` retain explicit policy implications.

## 3. Normalization Outcomes

- Duplicate semantic descriptions were removed from item ego metadata.
- Resistance ownership was normalized to the resistance catalog.
- Timed oppose naming was separated from permanent resistance taxonomy.
- Affix aliases were identified and canonical ownership retained.
- Activation payloads were aligned with the action catalog.
- Catalog dependency relationships and ownership boundaries were made explicit.

## 4. Validation Results

- Broken references: PASS.
- Ownership consistency: PASS.
- Dependency graph: PASS.
- Semantic normalization: PASS.
- Required parameter, enum, structured amount, and duration contract checks: PASS for the migrated activation records.

The five V2 validation reports are consolidated here because their work is complete. This archive is evidence, not active design guidance.

## 5. Final Runtime Implications

The normalized catalogs now support implementation of:

- equipment flag aggregation;
- derived stat calculation;
- capability and resistance resolution;
- timed status interlocks;
- combat slay/kill/brand policy;
- passive curse scheduling;
- activation execution and cooldown policy.

Catalog redesign should not resume unless new source evidence disproves one of these conclusions.
