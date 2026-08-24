# Terrain System Design

Status: Phase 1 Planning (Documentation only)
Scope: Canonical gameplay-definition design for terrain
Non-goals:
- Runtime implementation
- Godot integration
- Gameplay Architecture V2 redesign
- Schema implementation

Authority:
- Gameplay Architecture V2
- MAngband 1.5.3 behavior expectations
- Existing repository definitions
- docs/references and legacy/rewrite/terrain_features.json (reference source)

---

## 1. Purpose

Define a canonical terrain definition model suitable for `terrain_definitions.json` in a parity-first workflow.

This document specifies data model design only.

---

## 2. Canonical Model: TerrainDefinition

Proposed canonical definition object:

```json
{
  "id": "string",
  "name": "string",
  "symbol": "string",
  "color": "string",
  "terrain_category": "floor|wall|permanent_wall|door|secret_door|trap|stair|shop|vein|water|wilderness|special",
  "walkable": true,
  "blocks_los": false,
  "destroyable": true,
  "feature_tags": ["string"],
  "appears_as": "string|null",
  "provenance_status": "verified|inferred|unresolved"
}
```

Field intent:
- `id`: Stable canonical identity.
- `name`: Display and authoring name.
- `symbol`: Primary glyph representation.
- `color`: Legacy palette token from source references.
- `terrain_category`: Primary gameplay category.
- `walkable`: Traversal availability.
- `blocks_los`: Line-of-sight blocking behavior.
- `destroyable`: Whether gameplay effects can transform or destroy this terrain type.
- `feature_tags`: Secondary traits, such as `door_locked` or `contains_hidden_treasure`.
- `appears_as`: Canonical alias target used for disguised features.
- `provenance_status`: Evidence confidence marker.

---

## 3. Terrain Category Taxonomy

Canonical categories and intent:
- `floor`: Generic traversable floor cells.
- `wall`: Non-permanent blocking structural walls.
- `permanent_wall`: Hard-blocking walls intended to be non-destructible.
- `door`: Open, closed, locked, jammed, and home-door variants.
- `secret_door`: Hidden door represented as wall until discovered.
- `trap`: Terrain feature that hosts trap trigger semantics.
- `stair`: Up/down level transition terrain.
- `shop`: Town shop entry tiles and home entry tile.
- `vein`: Diggable mineral structures, including hidden/known treasure states.
- `water`: Liquid terrain behavior class.
- `wilderness`: Outdoor terrain and vegetation classes.
- `special`: Non-core but gameplay-significant map features.

---

## 4. Classification of Existing terrain_features Entries

Source basis: `docs/references and legacy/rewrite/terrain_features.json`.

### 4.1 Floor

Verified:
- `open_floor`
- `packed_dirt`
- `grass`
- `crops`
- `loose_dirt`
- `mud`

Inferred:
- `darkness` (legacy placeholder feature; not a normal traversable floor)

Unresolved:
- Whether `darkness` should remain a terrain feature in canonical data or move to map state metadata.

### 4.2 Wall

Verified:
- `granite_wall_basic`
- `granite_wall_inner`
- `granite_wall_outer`
- `granite_wall_solid`
- `pile_of_rubble`

Inferred:
- `arena_entrance` as wall-like special blocker
- `log` and `tree`/`dark_tree` as wall-like blockers in wilderness context

Unresolved:
- Whether `pile_of_rubble` should be `wall` or a separate destructible obstacle subtype via tags.

### 4.3 Permanent Wall

Verified:
- `permanent_wall_basic`
- `permanent_wall_inner`
- `permanent_wall_outer`
- `permanent_wall_solid`
- `wilderness_border_wall`

### 4.4 Door

Verified:
- `open_door`
- `broken_door`
- `door_closed_base`
- `locked_door_1` .. `locked_door_7`
- `jammed_door_0` .. `jammed_door_7`
- `opened_home_door`
- `home_door_strength_1` .. `home_door_strength_8`
- `drawbridge`

Inferred:
- `drawbridge` as door-adjacent transition feature via `feature_tags`

Unresolved:
- Whether drawbridge should be a separate category in future post-parity taxonomy.

### 4.5 Secret Door

Verified:
- `secret_door`

### 4.6 Trap

Verified:
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

Inferred:
- `glyph_of_warding` as special trap-like terrain in placement and interaction terms

Unresolved:
- Whether `glyph_of_warding` is categorized under `trap` or `special` in final parity lock.

### 4.7 Stair

Verified:
- `up_staircase`
- `down_staircase`

### 4.8 Shop

Verified:
- `shop_general_store`
- `shop_armory`
- `shop_weapon_smith`
- `shop_temple`
- `shop_alchemy`
- `shop_magic`
- `shop_black_market`
- `shop_home`

### 4.9 Vein

Verified:
- `magma_vein`
- `quartz_vein`
- `magma_vein_treasure_hidden`
- `quartz_vein_treasure_hidden`
- `magma_vein_treasure_known`
- `quartz_vein_treasure_known`

### 4.10 Water

Verified:
- `water`

### 4.11 Wilderness

Verified:
- `grass`
- `crops`
- `potato_crops`
- `cabbage_crops`
- `carrot_crops`
- `beet_crops`
- `squash_crops`
- `corn_crops`
- `mushroom_field`
- `tree`
- `dark_tree`
- `log`

Inferred:
- `packed_dirt` and `mud` as wilderness-capable depending on map generator context

### 4.12 Special

Verified:
- `arena_entrance`

Inferred:
- `glyph_of_warding`
- `darkness`

Unresolved:
- Boundary between `special` terrain and non-terrain map state markers.

---

## 5. Canonical Tag Recommendations

Recommended `feature_tags` examples:
- `town_only`
- `dungeon_only`
- `blocks_projectiles`
- `door_open`
- `door_closed`
- `door_locked`
- `door_jammed`
- `secret`
- `trap_hidden`
- `trap_visible`
- `vein_contains_treasure_hidden`
- `vein_contains_treasure_known`
- `shop_entry`
- `stair_up`
- `stair_down`
- `wilderness_feature`
- `destructible`
- `indestructible`

Verified:
- Need for alias/disguise tags (`appears_as`) is demonstrated by secret doors, hidden treasure veins, and hidden traps.

Inferred:
- Explicit tags will reduce overloaded interpretation from symbol-only representation.

Unresolved:
- Final canonical tag list should be frozen only after trap, store, and dungeon-generation design docs converge.

---

## 6. Provenance Use Rules

Use `provenance_status` per entry:
- `verified`: Directly evidenced in MAngband behavior or clearly represented in current authoritative references.
- `inferred`: Strongly suggested by current references but not yet parity-proven.
- `unresolved`: Known required terrain concept without sufficient confirmation.

---

## 7. Deferred to Later Phases

Not part of this document:
- Runtime movement rules
- LOS algorithm implementation
- Terrain mutation engine behavior
- Map generation implementation details

This preserves Phase 1 as definition infrastructure only.
