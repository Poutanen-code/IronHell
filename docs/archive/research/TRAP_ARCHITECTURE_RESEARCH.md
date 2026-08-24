# Trap Architecture Research

Date: 2026-08-03  
Scope: G-07 and G-08 trap trigger, payload, and world-state semantics

## Objective

Determine whether traps fit the same canonical model as spells and devices.

## Evidence Basis

- Chest trap table and semantics: [reference-mangband-1_5_3/src/server/tables.c](reference-mangband-1_5_3/src/server/tables.c#L2305)
- Chest trap runtime and open and disarm flows: [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L369)
- Floor trap runtime effects: [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L889)
- Trap placement and instantiation model: [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L3829)
- Trap world mutation by effects: [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1641)
- Natural trap allocation in level generation: [reference-mangband-1_5_3/src/server/generate.c](reference-mangband-1_5_3/src/server/generate.c#L713)

## 1. Trigger Model

## 1.1 Floor traps

Trigger semantics:
- Traps are initially FEAT_INVIS and are instantiated to typed traps by discovery or activation.
- Trigger is movement into trap cell; runtime resolves to one of FEAT_TRAP_HEAD + 0x00..0x0F.

Evidence:
- [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L3829)
- [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L4170)
- [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L916)

## 1.2 Chest traps

Trigger semantics:
- Chest trap set is indexed by chest pval through chest_traps[] table.
- Open and disarm actions can trigger trap; failed disarm can set it off.

Evidence:
- [reference-mangband-1_5_3/src/server/tables.c](reference-mangband-1_5_3/src/server/tables.c#L2311)
- [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L1004)
- [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L1083)

## 2. Payload Model

## 2.1 Damage payloads

- Pit, spiked pit, fire, acid, and chest explosion channels produce direct damage.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L960)
- [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L1070)
- [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L432)

## 2.2 Status payloads

- Poison, paralysis, blind, confusion, slow, and stat loss effects are trap payloads with resistance checks.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L1150)
- [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L400)

## 2.3 Summon payloads

- Floor and chest trap channels can summon monsters.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L1047)
- [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L420)

## 2.4 Item and attribute payloads

- Chest needles can reduce stats.
- Some floor darts reduce attributes.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L384)
- [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L1102)

## 3. World Effects

## 3.1 Treasure and lock interactions

- Chest explosion destroys chest contents and clears trap state.
- Disarming a chest trap also removes lock by negative pval convention.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd2.c](reference-mangband-1_5_3/src/server/cmd2.c#L435)
- [reference-mangband-1_5_3/src/server/tables.c](reference-mangband-1_5_3/src/server/tables.c#L2309)

## 3.2 Trap removal and mutation

- Destroy trap effects remove trap features and can unlock secret-door transformations.
- Trap creation effects introduce new hazards onto floor.

Evidence:
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1641)
- [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1900)

## 3.3 Spatial transition effects

- Trap door trap can trigger level transition semantics.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L916)

## 4. Semantic Classification

| Trap concern | Classification | Rationale |
|---|---|---|
| Trigger semantics | Policy | Trigger depends on world context, discoverability, and interaction path.
| Action semantics | Action | Damage and status payloads align with shared action families.
| World-state semantics | Policy plus world mutation | Chest lock, content destruction, and feature transitions exceed pure payload.

## Conclusion

Traps fit Source -> Policy -> Actions -> Statuses only when trap trigger and world mutation are treated as first-class policy channels.  
Trap payloads can share action vocabulary, but trap triggers and chest lifecycle semantics are not safely projectable as pure actions.
