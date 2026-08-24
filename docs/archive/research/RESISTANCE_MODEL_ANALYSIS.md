# Resistance Model Analysis

Date: 2026-08-03  
Scope: G-02 resistance representation verification

## Objective

Determine whether resistances belong to capabilities, statuses, actions, or multiple layers.

## Evidence Basis

- Player resistance and immunity state fields: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2249)
- Permanent resist and immunity flags from equipment and native sources: [reference-mangband-1_5_3/src/server/spells2.c](reference-mangband-1_5_3/src/server/spells2.c#L514), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L649)
- Temporary oppose timers: [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L970), [reference-mangband-1_5_3/src/server/dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c#L1417)
- Damage formulas for elemental channels: [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L995)
- Item protection flags from destruction channels: [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L790)
- Monster AI inference from resist and oppose and immunity channels: [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L105)
- Object-level descriptive split between immunity, resistance, and ignore flags: [reference-mangband-1_5_3/src/server/obj-info.c](reference-mangband-1_5_3/src/server/obj-info.c#L770)

## Terminology in MAngband Runtime

- Resist: permanent passive reduction channel on player state, usually from race and equipment.
- Oppose: temporary status timer for elemental channels.
- Immunity: permanent complete negation for base elements.
- Ignore: item-self protection from elemental destruction.

## Runtime Behavior by Channel

## 1. Resist

Runtime behavior:
- Resist channels are boolean flags on player state.
- For elemental damage, resist reduces damage to about one third with integer rounding logic.

Calculation semantics:
- Acid example: if resist_acid then dam = (dam + 2) / 3.
- Similar pattern for elec, fire, and cold.

Sources:
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L995)
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1020)
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1043)
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1066)

Architectural implication:
- Resist is a capability-layer concern, not an action payload.

## 2. Oppose

Runtime behavior:
- Oppose channels are timed status values on player state.
- They decay each turn.
- They use the same reduction transform as elemental resist.

Calculation semantics:
- Oppose stacks multiplicatively with resist via repeated thirding.
- Elemental with both resist and oppose yields approximately one ninth of original damage.

Sources:
- [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L970)
- [reference-mangband-1_5_3/src/server/dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c#L1417)
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L995)

Architectural implication:
- Oppose belongs to status layer.
- It should not be flattened into permanent capability grants.

## 3. Immunity

Runtime behavior:
- Immunity flags fully short-circuit elemental damage.

Calculation semantics:
- If immune for elemental type, damage function returns before reduction logic.

Sources:
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L995)
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1043)

Architectural implication:
- Immunity is a high-priority capability channel, separate from timed statuses.

## 4. Item Protection Ignore

Runtime behavior:
- Ignore flags protect items from destruction checks under elemental effects.
- This is item-self behavior, not bearer damage resistance.

Calculation semantics:
- Destroy checks return false when matching ignore flag exists.

Sources:
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L790)
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L922)

Architectural implication:
- Ignore belongs to item-self capability scope.
- It must not be merged with bearer resistance semantics.

## 5. Resist Stacking

Observed stacking patterns:

1. Resist plus oppose on elemental channels stack multiplicatively through repeated reduction steps.
2. Immunity overrides both resist and oppose.
3. Non-elemental resist channels are mostly independent booleans used for saves or special effect checks.

Sources:
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L995)
- [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L105)

## Layer Placement Conclusion

- Capabilities layer:
  - Permanent resist flags
  - Immunity flags
  - Item ignore flags
- Status layer:
  - Oppose timers
- Actions layer:
  - Delivers elemental damage or status applications, but does not own resistance state

Final determination:
- Resistances are a multi-layer concern and cannot be represented correctly in a single channel.

## Safe and Blocked Decisions

Safe now:
1. Keep permanent resist and temporary oppose as separate model concepts.
2. Keep item ignore separate from bearer resist.
3. Preserve immunity as explicit capability, not as numeric maximum resist.

Still blocked:
1. Any attempt to collapse resist, oppose, and ignore into one unified attribute type.
2. Any action-schema design that assumes resistance is only action metadata.
