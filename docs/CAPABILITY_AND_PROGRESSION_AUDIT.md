# Capability and Progression Audit

## Scope and method

This is a research-only audit of the capability, progression, item, artifact,
ego-item, and activation catalogs. No gameplay data was changed. The audit
inspected:

- `data/definitions/capabilities.json`
- `data/definitions/classes.json`
- `data/definitions/races.json`
- `data/definitions/artifacts.json`
- `data/definitions/items/armor.json`
- `data/definitions/items/weapons.json`
- `data/definitions/items/accessories.json`
- `data/definitions/ego_items.json`
- `data/definitions/items/item_affixes.json`
- `data/definitions/activations.json`

References were counted in two forms:

- `capability_ids`: canonical ID references in races, classes, and item catalogs.
- `flags`: artifact and ego-item source flags mapped through `source_flag` in
  `capabilities.json`.

The implementation check also inspected `src/IronHell.Core` and the schema
validation tests. “Reachable” below therefore means catalog-reachable unless
an implementation path is explicitly identified.

## Executive findings

1. `capabilities.json` contains 46 canonical capabilities. Every canonical
   capability has at least one catalog reference; there are no canonical
   capability definitions with zero references.
2. The canonical split is coherent for the current data: class capabilities
   use `native_identity`, bearer-granted race/item/artifact properties use
   `bearer_passive`, and item-destruction protection uses `item_self_passive`.
3. Eight referenced IDs are outside the canonical catalog and are accepted by
   an explicit legacy baseline in
   `tests/schema-validation/repository-contracts.test.js`:
   `searching`, `regeneration`, `stealth`, `res_sound`, `res_shard`,
   `res_chaos`, `res_nethr`, and `slay_undead`. These are compatibility
   structures, not resolved canonical capabilities.
4. All six classes and eleven races have capability arrays that resolve to the
   canonical catalog. The C# path currently only combines and deduplicates race
   and class IDs; it does not evaluate individual capability behavior.
5. There are 125 artifacts, 39 artifacts with the `ACTIVATE` flag, and 64
   artifacts with an `activation` field. All 52 activation IDs are referenced
   by at least one artifact, and no activation ID is orphaned. However, the
   current C# source contains no activation dispatcher or capability evaluator.
6. The activation catalog has a compatibility/legacy dependency on the
   MAngband flag-and-serial model. `ego_items.json` explicitly describes
   `ACTIVATE` as hardcoded per serial and inert in runtime, while the newer
   artifact activation indirection is validated only structurally.

## 1. Capability reachability matrix

Definition location for every row is
`data/definitions/capabilities.json`. The `ID refs` column lists distinct files
containing the canonical `capability_ids` value. The `flag refs` column lists
distinct files containing a matching `source_flag` in a `flags` array. A dash
means no reference of that form.

| Capability | Scope | Classification | ID refs | Flag refs |
|---|---|---|---|---|
| `back_stab` | `native_identity` | Identity Trait | classes | - |
| `beam` | `native_identity` | Identity Trait | classes | - |
| `bless_weapon` | `native_identity` | Identity Trait | classes | - |
| `bravery_30` | `native_identity` | Identity Trait | classes | - |
| `choose_spells` | `native_identity` | Identity Trait | classes | - |
| `cumber_glove` | `native_identity` | Identity Trait | classes | - |
| `extra_shot` | `native_identity` | Identity Trait | classes | - |
| `free_act` | `bearer_passive` | Active | races, accessories | artifacts, ego_items |
| `hold_life` | `bearer_passive` | Active | races, accessories | artifacts, ego_items |
| `hp_bonus` | `native_identity` | Identity Trait | classes | - |
| `pseudo_id_heavy` | `native_identity` | Identity Trait | classes | - |
| `pseudo_id_improv` | `native_identity` | Identity Trait | classes | - |
| `regen` | `bearer_passive` | Active | races | artifacts, ego_items |
| `res_blind` | `bearer_passive` | Active | races, armor | artifacts, ego_items |
| `res_dark` | `bearer_passive` | Active | races, armor, accessories | artifacts, ego_items |
| `res_lite` | `bearer_passive` | Active | races, armor, accessories | artifacts, ego_items |
| `res_pois` | `bearer_passive` | Active | races, armor, accessories | artifacts, ego_items |
| `see_invis` | `bearer_passive` | Active | races, accessories | artifacts, ego_items |
| `speed_bonus` | `native_identity` | Identity Trait | classes | - |
| `stealing_improv` | `native_identity` | Identity Trait | classes | - |
| `stealth_mode` | `native_identity` | Identity Trait | classes | - |
| `sust_con` | `bearer_passive` | Active | races, armor, accessories | artifacts, ego_items |
| `sust_dex` | `bearer_passive` | Active | races, armor, accessories | artifacts, ego_items |
| `sust_str` | `bearer_passive` | Active | races, armor, accessories | artifacts, ego_items |
| `zero_fail` | `native_identity` | Identity Trait | classes | - |
| `aggravate_monsters` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `feather` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `ignore_acid` | `item_self_passive` | Passive Item Property | armor, weapons, accessories | ego_items |
| `ignore_cold` | `item_self_passive` | Passive Item Property | armor, accessories | ego_items |
| `ignore_elec` | `item_self_passive` | Passive Item Property | armor, accessories | ego_items |
| `ignore_fire` | `item_self_passive` | Passive Item Property | armor, weapons, accessories | ego_items |
| `infravision` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `res_acid` | `bearer_passive` | Active | armor, accessories | artifacts, ego_items |
| `res_cold` | `bearer_passive` | Active | armor, accessories | artifacts, ego_items |
| `res_confu` | `bearer_passive` | Active | armor, accessories | artifacts, ego_items |
| `res_disen` | `bearer_passive` | Active | armor, accessories | artifacts, ego_items |
| `res_elec` | `bearer_passive` | Active | armor, accessories | artifacts, ego_items |
| `res_fear` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `res_fire` | `bearer_passive` | Active | armor, accessories | artifacts, ego_items |
| `res_nexus` | `bearer_passive` | Active | armor, accessories | artifacts, ego_items |
| `slow_digest` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `sust_chr` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `sust_int` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `sust_wis` | `bearer_passive` | Active | accessories | artifacts, ego_items |
| `teleportation` | `bearer_passive` | Active | accessories | ego_items |
| `telepathy` | `bearer_passive` | Active | accessories | artifacts, ego_items |

### Classification result

- **Identity Trait:** 17 canonical class capabilities. They describe class
  identity or class progression behavior and are not item-borne.
- **Passive Item Property:** 4 canonical `item_self_passive` capabilities:
  `ignore_acid`, `ignore_cold`, `ignore_elec`, and `ignore_fire`.
- **Active:** 25 canonical bearer-passive capabilities, including race and
  equipment grants. “Active” here means referenced and intended to contribute
  to the bearer/item model; it does not claim that a C# runtime handler exists.
- **Reserved:** none in the canonical catalog.
- **Candidate Obsolete:** none among canonical definitions. The legacy IDs in
  the next section are candidates for eventual consolidation, but are not
  canonical definitions and should remain compatibility findings for this
  audit.

## 2. Capability scope audit

### `native_identity`

All 17 capabilities using this scope are granted only by classes. This matches
their semantics: spell selection, class combat modifiers, pseudo-identification,
and class-specific magic rules belong to the character's class identity.

### `bearer_passive`

All 25 capabilities using this scope are granted by races and/or equipment
representing a property applied to the character: resistances, sustains,
perception, regeneration, curses, and survival abilities. Artifact and ego-item
flags map to the same bearer semantics.

### `item_self_passive`

All four `ignore_*` capabilities are correctly scoped to the item itself. Their
descriptions state that the item cannot be destroyed by an element; they do not
grant the bearer immunity to that element.

### Scope risks

1. Legacy `RES_*` and ability flags in artifacts/ego items are represented as
   source flags, while ordinary items use canonical `capability_ids`. This is
   a compatibility structure with two encodings for the same conceptual layer.
2. `LITE`, `STEALTH`, `SEARCH`, and similar legacy flags are not all represented
   by canonical capabilities in the inspected catalog. The distinction between
   a bearer passive and a numeric `affix` remains important; the two should not
   be collapsed.
3. `item_affixes.json` contains numeric stat/property modifiers only. It does
   not define capability reachability, so it is not a substitute for
   `capability_ids` or artifact source flags.

## 3. Class and race progression

### Class grants

| Class | Capability IDs | Resolution |
|---|---|---|
| Warrior | `bravery_30`, `pseudo_id_heavy`, `pseudo_id_improv` | canonical |
| Mage | `cumber_glove`, `zero_fail`, `beam`, `choose_spells`, `hp_bonus` | canonical |
| Priest | `bless_weapon`, `zero_fail`, `pseudo_id_improv` | canonical |
| Rogue | `cumber_glove`, `choose_spells`, `pseudo_id_heavy`, `pseudo_id_improv`, `back_stab`, `stealing_improv`, `speed_bonus`, `stealth_mode` | canonical |
| Ranger | `cumber_glove`, `extra_shot`, `choose_spells`, `pseudo_id_improv` | canonical |
| Paladin | `pseudo_id_heavy`, `pseudo_id_improv` | canonical |

The class catalog contains 25 grant occurrences and 17 distinct canonical
capability IDs. `CharacterFactory` provides the current implementation path:
it concatenates race and class capability IDs, removes duplicates while
preserving order, and stores the result in `CharacterState.CapabilityIds`.
Tests cover representative class/race combinations and deduplication.

### Race grants

| Race | Capability IDs | Resolution |
|---|---|---|
| Human | - | no capability |
| Half-Elf | `sust_dex` | canonical |
| Elf | `sust_dex`, `res_lite` | canonical |
| Hobbit | `hold_life` | canonical |
| Gnome | `free_act` | canonical |
| Dwarf | `res_blind` | canonical |
| Half-Orc | `res_dark` | canonical |
| Half-Troll | `sust_str`, `regen` | canonical |
| Dunadan | `sust_con` | canonical |
| High-Elf | `res_lite`, `see_invis` | canonical |
| Kobold | `res_pois` | canonical |

The race catalog contains 14 grant occurrences and 11 races. All race/class
capability IDs have a catalog resolution path. The unresolved part is runtime
behavior: the current C# implementation exposes IDs but does not apply
resistance, sustain, regeneration, or class-specific effects.

## 4. Artifact and activation audit

### Activation resolution

`activations.json` contains 52 unique activation IDs. Every one is referenced
by at least one artifact, so there are:

- **Missing activation references:** none.
- **Unreachable/orphaned activation definitions:** none at the catalog level.
- **Duplicate activation IDs:** none.
- **Legacy STARLIGHT reference:** none; the canonical ID is `STAR_LIGHT`, and
  the validation suite explicitly protects that alias cleanup.

The artifact set has 125 entries. Only 39 have the `ACTIVATE` flag, but 64 have
an `activation` field. This mismatch is a finding: 25 artifacts carry an
activation field without the `ACTIVATE` marker. The schema and current tests
permit this because they require activation fields for flagged artifacts but
do not require the reverse implication. Whether those 25 fields are intended
compatibility metadata or data drift is unresolved.

### Legacy activation dependencies

- `data/definitions/ego_items.json` documents the legacy MAngband contract:
  `ACTIVATE` means an effect is hardcoded per serial and is marked inert in
  runtime. This is a serial/flag dependency rather than a canonical activation
  resolver.
- `data/definitions/artifacts.json` uses a newer named `activation` indirection
  layer, but the current C# source has no activation execution path.
- Activation payloads use effect IDs and free-form effect parameters. Structural
  schema validation confirms shape and selected enums, but no current runtime
  dispatcher was found to consume these payloads.

## 5. Missing references, duplicated concepts, and runtime dependencies

### Missing or compatibility-only references

The following IDs occur in item catalogs but do not exist in
`capabilities.json`:

| ID | Referencing files | Finding |
|---|---|---|
| `searching` | accessories | legacy capability baseline |
| `regeneration` | accessories, armor | legacy spelling/alias for canonical `regen` |
| `stealth` | armor | legacy item capability |
| `res_sound` | armor | legacy resistance not in canonical catalog |
| `res_shard` | armor | legacy resistance not in canonical catalog |
| `res_chaos` | armor, weapons | legacy resistance not in canonical catalog |
| `res_nethr` | armor | legacy resistance not in canonical catalog |
| `slay_undead` | weapons | legacy slay capability not in canonical catalog |

These references pass only because the repository contract test has an explicit
legacy baseline. They are not resolved through a canonical capability
definition, scope, or implementation path.

### Duplicated or overlapping concepts

- `regen` versus `regeneration` is a direct naming duplication.
- Canonical capability IDs and MAngband-style `flags` encode the same passive
  concepts in different item families. This is intentional compatibility
  layering but creates two reachability mechanisms.
- Artifact activations use named catalog records, while ego-item metadata still
  describes serial-hardcoded activation behavior.
- Numeric item modifiers belong in `affixes`; boolean/property mechanics belong
  in `capability_ids` or mapped source flags. Existing accessory tests protect
  against duplicating capability concepts in `effects[]`.

### Unresolved runtime dependencies

1. `CharacterFactory` resolves and deduplicates IDs but does not implement the
   behavior behind the IDs.
2. No C# capability evaluator was found for class traits, racial abilities,
   passive item properties, resistance layers, sustains, or curses.
3. No C# activation dispatcher was found for `activations.json` effect payloads.
4. Artifact flag-to-capability mapping is represented in data, but the runtime
   path that turns artifact flags into bearer/item state is not present in the
   inspected Core project.
5. Activation reachability is therefore structural, not behavioral. All 52
   records are named and referenced, but “resolves correctly” beyond JSON/schema
   resolution remains unverified.

## Validation evidence

The existing validation suite covers JSON schemas, capability reference
resolution with the explicit legacy baseline, artifact activation existence,
required activation fields, duplicate activation IDs, the `STAR_LIGHT` alias,
and accessory phantom-effect checks. It does not currently prove that each
capability or activation has an executable runtime handler.

This document records findings only; no migration, activation migration, or
gameplay modification is proposed or performed.