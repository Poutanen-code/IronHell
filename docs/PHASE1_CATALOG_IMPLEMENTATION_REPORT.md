# Phase 1 Catalog Implementation Report

Status: Implemented (production JSON catalogs, not design documents)
Scope: Terrain, traps, stores, vaults canonical gameplay-definition catalogs

Authority:
- docs/GAMEPLAY_ARCHITECTURE_V2.md
- docs/IronHell Implementation roadmap/TERRAIN_SYSTEM_DESIGN.md
- docs/IronHell Implementation roadmap/STORE_SYSTEM_DESIGN.md
- docs/IronHell Implementation roadmap/VAULT_SYSTEM_DESIGN.md
- docs/references and legacy/rewrite/terrain_features.json
- docs/references and legacy/rewrite/vault_templates.json
- docs/references and legacy/rewrite/vaults.json (prior partial canonical migration, reused as source for 10 selected vaults)
- MAngband 1.5.3 (terrain.txt / vault.txt derived reference data, as captured in the rewrite reference files above)

---

## 1. Files Created

### Data catalogs
- `data/definitions/terrain_definitions.json` — 92 terrain definitions, full parity with `terrain_features.json` (indices 0-135, all 92 populated entries).
- `data/definitions/traps.json` — 17 trap definitions (all floor/rune/gas/dart/pit/elemental traps from the terrain feature set). `glyph_of_warding` was intentionally excluded (see Unresolved Questions).
- `data/definitions/stores.json` — 8 canonical store definitions (General Store, Armory, Weapon Smith, Temple, Alchemy Shop, Magic Shop, Black Market, Home).
- `data/definitions/vaults.json` — 10 vault definitions: 5 starter, 3 greater, 2 special.

### Schemas
- `data/schemas/terrain_definitions.schema.json`
- `data/schemas/traps.schema.json`
- `data/schemas/stores.schema.json`
- `data/schemas/vaults.schema.json`

All schemas follow existing repository conventions: draft 2020-12, `$ref` into `data/schemas/common.schema.json` for `Id` and `ProvenanceStatus`, `additionalProperties: false` everywhere, `schema_version` const `1`.

### Validation
- `tests/schema-validation/phase1-catalogs.test.js` — new, self-contained AJV + cross-reference test suite covering all four catalogs (17 tests, all passing).
- `package.json` — added `test:phase1-catalogs` script to run the new suite independently (`node --test tests/schema-validation/phase1-catalogs.test.js`).

No runtime code, no Godot code, no sprite/art paths, and no new architecture documents were introduced. `symbol`/`color` fields are legacy ASCII/palette parity tokens (glyph + color-code characters), not art asset references.

---

## 2. Migration Decisions

### Terrain (`terrain_definitions.json`)
- Categorized all 92 `terrain_features.json` entries into the 13 required `terrain_category` values (floor, wall, permanent_wall, door, secret_door, stair, trap, vein, shop, water, vegetation, wilderness, special). `vegetation` was added as a distinct category from `wilderness` to separate living/plant features (grass, crops, trees, logs) from bare-ground wilderness terrain (packed dirt, loose dirt, mud), per the task's explicit category list.
- Added two attributes not present in the legacy source, both marked with reasoned defaults rather than fabricated precision:
  - `supports_lighting`: `true` for dungeon-context terrain (floor/wall/door/trap/vein/stair/secret_door/most special), `false` for town/wilderness/vegetation/water terrain that isn't subject to the dungeon darkness model.
  - `sound_attenuation` (`none`/`partial`/`full`): `full` for solid blocking terrain (granite/permanent walls, veins, secret doors), `partial` for closed doors, water, and natural obstructions (trees/log), `none` for floor, open doors, stairs, shops, and traps.
- `destroyable` was retained from the design doc's canonical model (static definition flag, not runtime state) to distinguish granite walls/veins/rubble (destroyable) from permanent walls and doors-as-terrain (not destroyable in the strict "terrain removal" sense).
- `appears_as` was set to `null` for `door_closed_base` even though the legacy source self-referenced (`appearsAsFeatureId: "door_closed_base"` pointing at itself); a self-alias carries no disguise information.

### Traps (`traps.json`)
- Reconciled the task's requested field names (`trigger_behavior`, `detection_behavior`, `disarm_behavior`, `trap_category`) with the richer model proposed in `TRAP_SYSTEM_DESIGN.md` by nesting `trigger_type`/`description` inside `trigger_behavior`, `detection_model`/two boolean flags inside `detection_behavior`, and `disarm_model`/`skill` inside `disarm_behavior`. `effect_type` and `visibility_model` were kept as top-level fields since they are independently useful and were part of the authoritative design model.
- Added `trap_category` as a grouping enum (`pit`, `dart`, `gas`, `rune`, `elemental_spot`, `transition`, `concealed_generic`) not present verbatim in the source, to satisfy the task's explicit requirement; grouping was derived directly from the `trapType` naming families in `terrain_features.json`.
- `source_feature_id` links every trap back to its terrain definition; cross-referential integrity is enforced by `phase1-catalogs.test.js`.
- All 17 traps use `disarm_behavior.skill: "disarming"`, matching the `disarming` skill name already used in `data/definitions/character/classes.json`.

### Stores (`stores.json`)
- Modeled the 8 canonical MAngband store types with `terrain_feature_id` cross-references into `terrain_definitions.json`.
- `inventory_profile_id`, `turnover_profile_id`, and `pricing_profile_id` are explicit placeholder identifiers (e.g. `general_store_inventory_profile_placeholder`) as requested. These are not yet backed by real profile catalogs — that is Phase 2 scope per `STORE_SYSTEM_DESIGN.md` section 8.
- All stores marked `provenance_status: "inferred"` because the store type/terrain linkage is verified, but the placeholder profile references are not yet resolved to real inventory/turnover/pricing data.

### Vaults (`vaults.json`)
- Reused the already-migrated canonical vault data in `docs/references and legacy/rewrite/vaults.json` (which had already converted `vault_templates.json` entries into the `id/rating/tags/dimensions/layout/legend` shape) rather than re-deriving from raw `vault_templates.json`.
- Selected the first 5 `starter` (type7) vaults, first 3 `greater` (type8) vaults, and first 2 `special` (type7 + special tag) vaults by template serial order: `vault_round_001`, `vault_octagon_002`, `vault_octagon_003`, `vault_square_004`, `vault_diagonal_005` (starter); `vault_huge_010`, `vault_large_011`, `vault_butterfly_012` (greater); `vault_planet_x_014`, `vault_turnabout_015` (special).
- Normalized generic legend terrain refs (`"granite_wall"`, `"permanent_wall"`, `"door"`) to concrete `terrain_definitions.json` ids (`granite_wall_basic`, `permanent_wall_basic`, `door_closed_base`) so vault legends are cross-referentially checkable. Monster/item/trap spawn refs (`vault_monster_spawn`, `vault_loot_spawn`, `vault_elite_spawn`, `trap_floor_marker`, `vault_marker_8`, `vault_marker_9`, `layout_padding`) remain descriptive placeholders since no monster/item spawn catalog exists yet.
- Dropped the nested `provenance: { status, source_file, source_template_index }` object from the prior migration in favor of a single flat `provenance_status` field, matching the canonical model specified in `VAULT_SYSTEM_DESIGN.md` section 2.

---

## 3. Unresolved MAngband / Design Questions

These are carried over from the design docs and were not resolved by this implementation pass (data was authored to be consistent, but the underlying parity questions remain open):

1. **`glyph_of_warding` classification** — `TERRAIN_SYSTEM_DESIGN.md` left it unresolved whether this is a `trap` or `special` terrain. It is modeled here as `terrain_category: "special"` in `terrain_definitions.json` and deliberately **excluded** from `traps.json` pending a decision; MAngband trap semantics for warding glyphs (player-created ward vs. monster trap) should be confirmed before adding a `TrapDefinition` for it.
2. **`pile_of_rubble` categorization** — modeled as `terrain_category: "wall"` with a `destructible` tag rather than a separate obstruction subtype; unresolved whether it needs its own category in a future non-parity pass.
3. **`drawbridge` categorization** — modeled as `terrain_category: "door"` with a `drawbridge` tag; unresolved whether it should become its own category once bridge/chasm terrain is designed.
4. **Vault legend markers `8`, `9`, `@`, `&`** — semantics remain unresolved per `VAULT_SYSTEM_DESIGN.md` section 5.3 (exact distinctions between elite spawn, generic spawn, and numbered special markers). Retained as placeholder refs (`vault_marker_8`, `vault_marker_9`, `vault_elite_spawn`, `vault_monster_spawn`) pending monster/item spawn catalog design.
5. **`supports_lighting` and `sound_attenuation` semantics** — these two fields were required by the task but are not present in any MAngband/legacy source; values were assigned by inference from terrain category and gameplay context (see Migration Decisions above) and should be validated against actual MAngband lighting/stealth behavior before being treated as parity-verified.
6. **Store profile catalogs** — `inventory_profile_id`/`turnover_profile_id`/`pricing_profile_id` are placeholders only; exact weighted item tables, restock cadence, and pricing multipliers per store type remain fully unresolved (tracked in `STORE_SYSTEM_DESIGN.md` sections 4-6).
7. **Remaining vault backlog** — only 10 of 149 `vault_templates.json` entries were promoted into `vaults.json` (this phase's required 5+3+2). Full parity migration of the remaining templates is out of scope for this phase.

---

## 4. Validation Registration

- Added `tests/schema-validation/phase1-catalogs.test.js`, compiled and passing (17/17 tests): schema conformance for all four catalogs, uniqueness checks, and cross-file reference integrity (`appears_as`, `source_feature_id`, `terrain_feature_id`, vault legend glyph coverage, vault tier counts).
- Added `npm run test:phase1-catalogs` script in `package.json` to run this suite in isolation.

### Pre-existing test infrastructure issue (not introduced by this change)

Running the pre-existing `npm test` (`tests/schema-validation/validate.test.js`) and `tests/schema-validation/repository-contracts.test.js` currently fails with `ENOENT` errors (e.g. `data/schemas/spells.schema.json`, `data/definitions/priest_prayers.json`) because those files were previously reorganized into `data/schemas/magic/` and `data/definitions/magic/` subfolders without updating the test file paths. This is a pre-existing repository drift issue unrelated to the Phase 1 catalog work in this report and was confirmed to exist before any changes in this pass. It is out of scope for this task and is flagged here rather than silently patched, since fixing the legacy suite's file paths touches many unrelated lines. The new `phase1-catalogs.test.js` suite is fully independent of this issue and passes cleanly.

---

## 5. Acceptance Criteria Checklist

- [x] Actual JSON catalogs created (`terrain_definitions.json`, `traps.json`, `stores.json`, `vaults.json`).
- [x] Actual schemas created (matching filenames under `data/schemas/`).
- [x] Validation updated (new dedicated test suite + npm script; catalogs registered and passing).
- [x] No runtime code.
- [x] No Godot code.
- [x] No sprite references (only legacy ASCII glyph/color parity tokens, consistent with existing repository conventions).
- [x] No new architecture documents (this report is an implementation/migration record, not an architecture document).
- [x] New Phase 1 catalog tests pass (17/17). Pre-existing unrelated test-path breakage documented above, not introduced by this work.
