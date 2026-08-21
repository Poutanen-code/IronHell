# Repository Integrity Audit

## Executive Summary

This is a research-only audit of the 31 JSON files under
`data/definitions`, their 31 schema files under `data/schemas`, and the two
schema-validation test files. No gameplay, runtime, targeting, or activation
files were modified.

### Overall result

- **Canonical reference failures:** none found for `action_id`, `status_id`,
  `activation_id`, `race_id`, or `class_id`.
- **Canonical capability failures:** none in race/class references or canonical
  item references. Eight legacy item capability IDs are outside the canonical
  catalogs and pass only through the explicit compatibility baseline in
  `repository-contracts.test.js`.
- **Resistance failures:** none for the resistance references checked by the
  current data model. Fourteen resistance definitions have no direct
  `resistance_id` consumer; several are intentionally legacy, timed, or
  reserved semantic records.
- **Activation failures:** none. All 52 activation definitions are referenced
  by artifacts when the artifact `activation` field is used.
- **Reachability findings:** `ParalyzeControl` is the only unreferenced action;
  six statuses and 14 resistance definitions have no external direct consumer.
  These are findings, not automatically deletions.
- **Schema coverage:** incomplete. Four definition catalogs have no matching
  same-path schema, three schemas have no same-path definition, and the main
  validation test currently imports missing `effects.json` and
  `effects.schema.json` files.
- **Test execution status:** the validation suite could not start because
  `data/schemas/effects.schema.json` is missing. Static scans and source review
  were used for the findings below.

## Reference Integrity

### Reference-domain results

The scan traversed all definition JSON files and excluded each catalog's own
definition records when measuring external reachability.

| Domain | Definitions | External reachability | Unknown references | Result |
|---|---:|---:|---:|---|
| `action_id` | 42 | 132 references | 0 | Closed; one unreferenced action |
| `status_id` | 26 | 35 references | 0 | Closed; six unreferenced statuses |
| `capability_id` | 46 canonical | 75 canonical references | 10 legacy IDs | Compatibility baseline required |
| `resistance_id` | 30 | 16 direct references | 0 | Direct references resolve |
| `activation_id` | 52 | artifact `activation` fields | 0 | Closed; no orphan activation |
| `race_id` | 11 | 11 external references | 0 | Closed |
| `class_id` | 6 | 6 external references | 0 | Closed |

### `action_id`

All observed action references resolve to the 42 IDs in
`data/definitions/actions.json`. The only definition without an external
reference is `ParalyzeControl`, whose declared source family is
`monster_spell`/`trap`. It may be reserved for those producers, but no current
definition references it.

### `status_id`

All observed status references resolve to `data/definitions/statuses.json`.
The six status definitions without an external direct reference are:

| Status | Classification | Finding |
|---|---|---|
| `haste` | Active | Required status and likely runtime target; no direct JSON consumer found |
| `slowed` | Active | Defined control status; no direct JSON consumer found |
| `resist_fire` | Compatibility | Legacy alias for oppose-fire semantics |
| `resist_cold` | Compatibility | Legacy alias for oppose-cold semantics |
| `stunned` | Active | Defined control status; no direct JSON consumer found |
| `cut` | Active | Defined damage/debuff status; no direct JSON consumer found |

“Unreferenced” here means no direct `status_id` field in the definition data;
it does not prove that the runtime cannot create the status.

### `capability_id`

The canonical catalog contains 46 IDs. Ten item references are not in the
canonical catalog or the separate item capability catalog:

| ID | Referencing files | Classification |
|---|---|---|
| `searching` | accessories | Compatibility |
| `regeneration` | accessories, armor | Compatibility alias candidate for `regen` |
| `stealth` | armor | Compatibility |
| `res_sound` | armor | Compatibility; resistance definition exists |
| `res_shard` | armor | Compatibility; resistance definition exists |
| `res_chaos` | armor, weapons | Compatibility; resistance definition exists |
| `res_nethr` | armor | Compatibility; resistance definition exists |
| `slay_undead` | weapons | Compatibility; no canonical capability definition |

These references are explicitly allowed by the legacy baseline in
`tests/schema-validation/repository-contracts.test.js`. They are not fully
resolved through a canonical capability definition and should remain visible as
compatibility debt.

Artifact and ego-item flags form a second capability reachability mechanism:
their legacy flags map to canonical `source_flag` values in
`capabilities.json`. This mapping is structurally present, but no current C#
capability evaluator was found.

### `resistance_id`

All direct resistance references resolve. The 14 definitions with no direct
consumer are:

- Legacy/compatibility resistances: `res_sound`, `res_shard`, `res_nethr`,
  `res_chaos`.
- Timed oppose records: `oppose_acid`, `oppose_elec`, `oppose_fire`,
  `oppose_cold`, `oppose_pois`, `oppose_elemental_bundle`.
- Immunity records: `imm_acid`, `imm_elec`, `imm_fire`, `imm_cold`.

The timed oppose records are linked to statuses through their own
`status_id` fields, so absence of a direct `resistance_id` consumer is not by
itself a broken reference. The immunity records are currently reserved or
legacy-looking because no capability or item definition directly selects them.

### `activation_id`

`activations.json` contains 52 unique activation IDs. Every ID is used by at
least one artifact through the artifact `activation` property. There are no
unknown activation references and no orphan activation definitions.

There are 125 artifacts, 64 with an `activation` field, and 39 with the
`ACTIVATE` flag. Twenty-five artifacts have an activation field without the
flag; zero artifacts have the flag without an activation field. This asymmetric
contract is structurally accepted but should be clarified as either intentional
compatibility metadata or data drift.

### `race_id` and `class_id`

All 44 `race_class_rules` records resolve to known race and class IDs. There are
11 races and six classes, but only 44 of the 66 possible race/class pairs are
present. The absent pairs are:

```text
elf/priest, elf/paladin, hobbit/mage, hobbit/priest, hobbit/paladin,
gnome/ranger, gnome/paladin, dwarf/mage, dwarf/rogue, dwarf/ranger,
half_orc/mage, half_orc/ranger, half_orc/paladin, half_troll/mage,
half_troll/rogue, half_troll/ranger, half_troll/paladin, dunadan/rogue,
high_elf/rogue, high_elf/paladin, kobold/mage, kobold/paladin
```

This is a sparse compatibility/content model, not an invalid reference.

## Contract Integrity

### Required parameters and closed action contracts

`actions.json` defines 42 actions. Every action declares a closed parameter
contract, unique parameter IDs, and validation metadata requiring declared
parameters, rejecting unknown parameters, and enforcing declared value types.

The repository contract test checks action references for:

- unknown `action_id` values;
- undeclared parameter keys; and
- missing required parameters.

The current static data contains no observed violation. However, the validation
suite could not execute because its top-level imports require the missing
`data/schemas/effects.schema.json`.

### Enum values and value domains

The catalog schemas define enums for action categories, parameter value types,
damage types, shapes, status semantics, resistance kinds/channels, and other
closed domains. Catalog-level schema validation and focused negative tests cover
many invalid enum cases.

The repository-wide action contract test does **not** validate every referenced
parameter value against the action parameter's `allowed_values`. The repository
memory also records that this gap is known. Therefore, an invalid enum token in
a validly shaped action reference can escape the cross-file contract test.

### Reference domains

Reference domains are unevenly enforced:

- `action_id`: broad repository-wide check.
- `status_id`: effects, activations, resistances, and selected item catalogs
  have checks; not every arbitrary status-bearing structure is covered by one
  generic walker.
- `capability_id`: repository-wide check with an explicit legacy allowlist;
  item capability catalogs are treated as a second known domain.
- `resistance_id`: capability-to-resistance checks and oppose-to-status checks
  exist.
- `activation_id`: artifact references are checked.
- `race_id`/`class_id`: race/class rule schema checks shape, but the validation
  suite does not appear to enforce their cross-file existence.

### Duplicate semantics

The following are compatibility structures or duplicate semantic channels:

1. `regen` and `regeneration` represent the same apparent concept with
   different names.
2. Canonical `capability_ids`, artifact flags, and ego-item flags can encode
   overlapping passive mechanics.
3. `resist_*` legacy status aliases coexist with canonical `oppose_*` statuses.
4. Named artifact activations coexist with ego-item metadata describing
   serial-hardcoded activation behavior.
5. Canonical `resistance_id` records coexist with legacy item capability IDs
   such as `res_sound` and `res_chaos`.

## Reachability

### Active

- All externally referenced action, status, race, and class IDs that are in
  their canonical catalogs.
- All 52 named activation records, because each is referenced by an artifact.
- Canonical capabilities reached by race/class `capability_ids`, item
  capability arrays, or mapped artifact/ego source flags.
- Directly referenced resistance records and timed oppose records linked to
  known statuses.

### Compatibility

- The eight legacy item capability IDs listed in the Reference Integrity
  section.
- `resist_fire` and `resist_cold` status aliases.
- Artifact/ego flag mappings and ego-item serial activation metadata.
- The 25 artifact activation fields lacking `ACTIVATE` flags, pending contract
  clarification.

### Reserved

- `ParalyzeControl` is a candidate reserved action for monster/trap producers;
  its source-family declaration explains why it has no current item/spell
  consumer.
- The four `imm_*` resistance records have no direct consumer and should be
  treated as reserved or legacy until a producer is identified.
- Sparse race/class pairs are absent content records, not broken references;
  their intended status is unresolved.

### Candidate Obsolete

- The six directly unreferenced active-looking statuses (`haste`, `slowed`,
  `stunned`, `cut`, plus the two aliases after compatibility review) are
  candidates for reachability review, not deletion.
- `regeneration` is a candidate obsolete spelling if `regen` is confirmed as
  the canonical ID.
- Legacy item capability IDs without a canonical capability or resistance
  consumer are candidates for consolidation after compatibility requirements
  are confirmed.

No record is marked safe to delete by this audit.

## Schema Coverage

### Definition/schema inventory

There are 31 definition JSON files and 31 schema JSON files, but the paths do
not form a complete one-to-one set:

**Definitions without a same-path schema:**

- `data/definitions/items/item_affixes.json`
- `data/definitions/mage_spells.json`
- `data/definitions/priest_prayers.json`

**Schemas without a same-path definition:**

- `data/schemas/common.schema.json` (shared schema)
- `data/schemas/items/common_item.schema.json` (shared item schema)
- `data/schemas/spells.schema.json` (shared spell schema)

The `effects.json` and `effects.schema.json` paths imported by
`tests/schema-validation/validate.test.js` are absent from the current
workspace. This prevents the test module from loading and is the most immediate
validation infrastructure failure.

### Existing detection coverage

The tests detect or partially detect:

- duplicate action, status, resistance, capability, spell book, spell,
  effect, and activation IDs;
- action contract closure, duplicate parameter IDs, missing required parameters,
  and unknown action references;
- selected invalid enum values and invalid schema shapes;
- unknown status references in effects, activations, and selected item files;
- unknown capability references, with an explicit legacy baseline;
- invalid capability resistance links;
- invalid oppose resistance status links;
- invalid artifact activation references and missing activation fields when the
  `ACTIVATE` flag is present;
- duplicate/removed `STARLIGHT` activation alias;
- selected item effect IDs, item condition shapes, and phantom accessory
  effects.

### Missing validation

The following gaps remain:

1. The suite cannot run until the missing effects definition/schema dependency
   is restored or the stale imports are corrected.
2. No generic test validates all definition files against their matching
   schemas. Classes, races, artifacts, ego items, armor, weapons, lights,
   chests, shops, and item affixes are not all loaded by `validate.test.js`.
3. `race_id` and `class_id` cross-file existence is not enforced for
   `race_class_rules.json`.
4. `activation` fields without `ACTIVATE` flags are not rejected or reported by
   tests.
5. Parameter instances are not checked against action `allowed_values` across
   every definition file.
6. There is no generic duplicate-ID/duplicate-name policy for all catalogs.
7. Canonical capability reachability is not distinguished from the explicit
   legacy allowlist by a failing test; legacy IDs remain accepted indefinitely.
8. Activation reachability is checked structurally, not against a runtime
   activation dispatcher.
9. Unreferenced actions, statuses, resistances, and capabilities are not
   reported as reachability findings by automated tests.

## Recommended Fixes

These are audit recommendations only. They are not implementation instructions
and no changes were made.

1. Restore or reconcile the missing effects definition/schema dependency so the
   validation suite can load and produce meaningful results.
2. Add a repository-wide schema loader that validates every definition file
   against its intended schema, excluding only documented shared schemas.
3. Add cross-file reference checks for race/class rules, all activation fields,
   all resistance links, and all item capability arrays.
4. Validate action parameter instances against declared value types and
   `allowed_values`, including every item and artifact source family.
5. Add a report-only reachability check for unreferenced definitions, with
   explicit classifications for compatibility and reserved records.
6. Replace the hard-coded legacy capability allowlist with an explicit
   compatibility catalog or a documented alias map, preserving current data
   behavior until a separate migration is approved.
7. Decide whether artifact `activation` implies `ACTIVATE`; encode that decision
   as a schema/test contract rather than leaving the current asymmetric rule
   implicit.
8. Keep activation and targeting semantics unchanged during any future
   validation work; this report does not recommend either migration.
