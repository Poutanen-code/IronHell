# Shared Effect Matrix

Date: 2026-08-03  
Scope: Cross-source comparison of MAngband gameplay payloads

## Source Columns

- Mage spell
- Priest prayer
- Monster spell
- Potion or scroll
- Rod
- Wand
- Staff
- Activation
- Trap

Legend:
- Y: source contains this payload pattern
- V: source contains variant with notable policy differences
- N: no direct payload equivalent identified in Phase 1

## Matrix

| Effect | Mage | Priest | Monster | Potion/Scroll | Rod | Wand | Staff | Activation | Trap | Classification |
|---|---|---|---|---|---|---|---|---|---|---|
| Bolt damage | Y | Y | Y | N | V | Y | N | N | N | Payload Equivalent |
| Ball damage | Y | Y | Y | V | Y | Y | Y | V | N | Payload Equivalent |
| Healing hp | N | Y | N | Y | N | N | Y | Y | N | Policy Variant |
| Cure poison | Y | Y | N | Y | N | N | Y | N | N | Payload Equivalent |
| Remove fear | N | Y | N | Y | N | N | N | N | N | Payload Equivalent |
| Bless or hero | Y | Y | N | Y | N | N | N | N | N | Policy Variant |
| Resist elements | Y | Y | N | Y | Y | N | N | Y | N | Payload Equivalent |
| Teleport self | Y | Y | V | Y | Y | N | Y | Y | Y | Payload Equivalent |
| Teleport other | Y | Y | V | N | Y | Y | N | N | N | Payload Equivalent |
| Recall or level shift | Y | Y | N | Y | Y | N | N | N | N | Policy Variant |
| Detection suite | Y | Y | N | Y | Y | N | Y | N | N | Payload Equivalent |
| Identification | Y | N | N | Y | Y | N | N | Y | N | Payload Equivalent |
| Recharging | Y | Y | N | Y | N | N | N | N | N | Policy Variant |
| Enchant or brand | Y | Y | N | Y | N | N | N | N | N | Payload Equivalent |
| Terrain alteration | Y | Y | V | V | N | N | Y | Y | N | Policy Variant |
| Sleep control | Y | N | Y | N | Y | Y | Y | N | N | Payload Equivalent |
| Slow control | Y | N | Y | N | Y | Y | N | N | N | Payload Equivalent |
| Confuse control | Y | N | Y | N | N | Y | N | N | N | Payload Equivalent |
| Fear control | N | Y | Y | N | N | Y | N | N | N | Payload Equivalent |
| Dispel evil or undead | N | Y | N | N | N | N | Y | N | N | Payload Equivalent |
| Banishment | Y | Y | N | N | N | N | N | N | N | Policy Variant |
| Drain life | Y | Y | Y | N | Y | N | N | N | N | Payload Equivalent |
| Earthquake | Y | Y | Y | N | N | N | Y | Y | N | Payload Equivalent |
| Summoning | N | N | Y | N | N | N | Y | Y | N | Policy Variant |
| Stat drain or damage | N | N | Y | V | N | N | N | N | Y | Payload Equivalent |
| Paralysis | N | N | Y | V | N | N | N | N | Y | Payload Equivalent |
| Trap spawn or trigger | Y | N | N | V | N | N | N | N | Y | Source Specific |

## Supporting Evidence

- Spell and prayer payload dispatch: [reference-mangband-1_5_3/src/server/x-spell.c](reference-mangband-1_5_3/src/server/x-spell.c#L878)
- Monster spell dispatch and policy context: [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L1620)
- Device payload and resource policy (rod, wand, staff): [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1558)
- Activation dispatch: [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L175)
- Trap table and chest trap execution: [reference-mangband-1_5_3/src/server/tables.c](reference-mangband-1_5_3/src/server/tables.c#L2184), [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L1298)

## Observed Policy Divergences

1. Monster spells are selected by AI policy before payload dispatch; player spells are selected by user with class and book legality gates.
2. Device payloads have charge or timeout economies not shared by player spellcasting.
3. Traps can enforce unavoidable or proximity-based trigger semantics.
4. Some prayers include MAngband-specific projected heal variants not mirrored in standard items.

## Architectural Interpretation

- Shared payload coverage is strong enough to justify a canonical effect registry.
- Policy mechanisms are not interchangeable and require source-specific policy layers.
