# Trap System Design

Status: Phase 1 Planning (Documentation only)
Scope: Canonical gameplay-definition design for traps
Non-goals:
- Runtime trap execution
- Godot integration
- Gameplay Architecture V2 redesign
- Schema implementation

Authority:
- Gameplay Architecture V2
- MAngband 1.5.3 trap semantics
- Existing repository definitions
- docs/references and legacy/rewrite/terrain_features.json (reference source)
- docs/TRAP_ARCHITECTURE_RESEARCH.md

---

## 1. Purpose

Define a canonical trap definition model for `traps.json` and classify known trap terrain references.

This is a data design document only.

---

## 2. Canonical Model: TrapDefinition

Proposed canonical definition object:

```json
{
  "id": "string",
  "name": "string",
  "source_feature_id": "string",
  "trigger_type": "on_enter|on_interact|on_open|on_disarm_failure|mixed",
  "effect_type": "damage|status|stat_drain|summon|teleport|level_transition|mixed",
  "visibility_model": "hidden_until_detected|visible|disguised",
  "disarm_model": "not_disarmable|skill_check|special_case",
  "detection_model": "search_skill|active_detection|proximity|none",
  "effect_tags": ["string"],
  "provenance_status": "verified|inferred|unresolved"
}
```

Field intent:
- `source_feature_id`: Link to terrain feature that hosts trap identity.
- `trigger_type`: How trap activation is entered.
- `effect_type`: Primary gameplay outcome family.
- `visibility_model`: Discovery and representation semantics.
- `disarm_model`: How disarm interaction applies.
- `detection_model`: How trap can be discovered before trigger.

---

## 3. Trap Inventory from terrain_features.json

Source basis: `docs/references and legacy/rewrite/terrain_features.json`.

Identified trap-linked terrain features:
- `invisible_trap`
- `trap_door`
- `pit`
- `spiked_pit`
- `poison_pit`
- `summon_rune`
- `teleport_rune`
- `fire_spot`
- `acid_spot`
- `dart_slow`
- `dart_strength`
- `dart_dexterity`
- `dart_constitution`
- `gas_blind`
- `gas_confuse`
- `gas_poison`
- `gas_sleep`

---

## 4. Trap Classification Matrix

| Trap feature id | Trigger type | Effect type | Visibility model | Disarm model | Detection model | Evidence status |
|---|---|---|---|---|---|---|
| invisible_trap | on_enter | mixed | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| trap_door | on_enter | level_transition | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| pit | on_enter | damage | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| spiked_pit | on_enter | damage | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| poison_pit | on_enter | mixed (damage, status) | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| summon_rune | on_enter | summon | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| teleport_rune | on_enter | teleport | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| fire_spot | on_enter | damage | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| acid_spot | on_enter | damage | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| dart_slow | on_enter | status | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| dart_strength | on_enter | stat_drain | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| dart_dexterity | on_enter | stat_drain | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| dart_constitution | on_enter | stat_drain | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| gas_blind | on_enter | status | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| gas_confuse | on_enter | status | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| gas_poison | on_enter | status | hidden_until_detected | skill_check | search_skill, active_detection | verified |
| gas_sleep | on_enter | status | hidden_until_detected | skill_check | search_skill, active_detection | verified |

---

## 5. Trigger, Visibility, Disarm, and Detection Models

### 5.1 Trigger type model

Verified:
- Floor traps are primarily movement-triggered (`on_enter`).
- Broader trap ecosystem includes chest trap triggers (`on_open`, `on_disarm_failure`) per MAngband evidence.

Inferred:
- Canonical `mixed` trigger type is needed for future traps with multiple activation paths.

Unresolved:
- Whether any floor trap in parity set requires non-enter triggers in canonical floor trap data.

### 5.2 Visibility model

Verified:
- Hidden trap representation exists (`invisible_trap`, aliasing to floor appearance).
- Discovery changes player knowledge and rendered feature identity.

Inferred:
- `disguised` visibility state may be useful for traps represented as benign symbols beyond standard hidden trap forms.

Unresolved:
- Exact knowledge-state fields needed to model per-player visibility in multiplayer parity.

### 5.3 Disarm model

Verified:
- Trap disarm is a skill-mediated gameplay path.

Inferred:
- Some traps may be effectively non-disarmable by design policy in future content.

Unresolved:
- Canonical expression for disarm difficulty and failure consequence scaling.

### 5.4 Detection model

Verified:
- Passive searching and active trap detection are distinct paths.

Inferred:
- Proximity-based sensing may be needed for certain future or special traps.

Unresolved:
- Final detection parameterization and interaction with race/class capabilities.

---

## 6. Relation to Gameplay Architecture V2

- Trap entries are Source-layer definitions.
- Trigger and discoverability are Policy concerns.
- Damage/status/summon/teleport outcomes project to Actions.
- Timed conditions from trap effects project to Statuses.

This preserves Source -> Policy -> Actions -> Statuses without redesign.

---

## 7. Deferred to Later Phases

Not included in this phase:
- Runtime trigger checks
- Trap probability tuning
- Disarm RNG formulas
- Multiplayer trap visibility state synchronization
