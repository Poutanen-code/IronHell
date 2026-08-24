# Store System Design

Status: Phase 1 Planning (Documentation only)
Scope: Canonical gameplay-definition design for stores
Non-goals:
- Runtime store implementation
- Godot integration
- Gameplay Architecture V2 redesign
- Schema implementation

Authority:
- Gameplay Architecture V2
- MAngband 1.5.3 store expectations
- Existing repository definitions
- docs/references and legacy/rewrite/terrain_features.json (reference source)

---

## 1. Purpose

Define canonical store definitions for `stores.json` and establish required data dimensions for inventory generation, turnover, and pricing.

This document is data design only.

---

## 2. Canonical Model: StoreDefinition

Proposed canonical definition object:

```json
{
  "id": "string",
  "name": "string",
  "store_type": "general_store|armory|weapon_smith|temple|alchemy_shop|magic_shop|black_market|home",
  "terrain_feature_id": "string",
  "town_symbol": "string",
  "inventory_profile_id": "string",
  "turnover_profile_id": "string",
  "pricing_profile_id": "string",
  "access_tags": ["string"],
  "provenance_status": "verified|inferred|unresolved"
}
```

Field intent:
- `store_type`: Stable gameplay identity.
- `terrain_feature_id`: Link to town feature tile.
- `inventory_profile_id`: Item-family generation rule set.
- `turnover_profile_id`: Restock and replacement timing and counts.
- `pricing_profile_id`: Buy/sell multipliers and constraints.
- `access_tags`: Access restrictions (for example, `town_only`, `owner_only`).

---

## 3. Required Store Definitions

Canonical store set:
- General Store
- Armory
- Weapon Smith
- Temple
- Alchemy Shop
- Magic Shop
- Black Market
- Home

Reference terrain mappings (verified from source file):
- General Store -> `shop_general_store`
- Armory -> `shop_armory`
- Weapon Smith -> `shop_weapon_smith`
- Temple -> `shop_temple`
- Alchemy Shop -> `shop_alchemy`
- Magic Shop -> `shop_magic`
- Black Market -> `shop_black_market`
- Home -> `shop_home`

---

## 4. Inventory Generation Requirements

### 4.1 Verified requirements

- Each store requires a distinct inventory identity tied to `shopType` in legacy features.
- Core store differentiation is part of parity behavior and must remain explicit in data.

### 4.2 Inferred requirements

- `inventory_profile_id` should reference:
  - Allowed item domains (for example, armor, weapons, consumables, magical devices).
  - Quality and depth weighting.
  - Stock size target range.
  - Special inclusion/exclusion rules.
- Black Market requires a distinct profile with broader, high-value, and potentially rare stock behavior.
- Home requires a non-commercial storage-oriented profile rather than normal generation.

### 4.3 Unresolved requirements

- Exact weighted item allocation tables per store type.
- Exact rare-item and out-of-depth rules by store.
- Final cross-file linkage format to item catalogs.

---

## 5. Turnover Requirements

### 5.1 Verified requirements

- Store inventories change over time in MAngband-like behavior.

### 5.2 Inferred requirements

`turnover_profile_id` should define:
- Restock interval units.
- Number of slots rotated per interval.
- Rules for preserving or replacing high-value inventory.
- Distinct turnover curves by store type.

### 5.3 Unresolved requirements

- Canonical time base for turnover in deterministic server turns.
- Black Market turnover constraints and anti-exploit controls.
- Home turnover behavior (expected to be no turnover) as a strict data rule.

---

## 6. Pricing Data Requirements

### 6.1 Verified requirements

- Store behavior requires explicit pricing modifiers and buy/sell asymmetry.
- Black Market pricing differs materially from regular stores.

### 6.2 Inferred requirements

`pricing_profile_id` should include:
- Base buy multiplier.
- Base sell multiplier.
- Minimum and maximum price clamps.
- Charisma or social modifier hooks.
- Special rules for cursed/unknown items.

### 6.3 Unresolved requirements

- Exact per-store multiplier values for parity.
- Haggling policy and whether represented as data, policy, or both.
- Multi-player economy interaction constraints.

---

## 7. Initial Store Characterization

| Store | Inventory profile focus | Turnover profile expectation | Pricing profile expectation | Status |
|---|---|---|---|---|
| General Store | Basic supplies and light utility | Moderate | Baseline | inferred |
| Armory | Defensive gear and armor | Moderate | Baseline to slightly higher | inferred |
| Weapon Smith | Weapons and combat gear | Moderate | Baseline to slightly higher | inferred |
| Temple | Holy and recovery items | Moderate | Baseline | inferred |
| Alchemy Shop | Potions and alchemical consumables | Moderate to high | Baseline | inferred |
| Magic Shop | Scrolls, books, devices, magical gear | Moderate | Higher than baseline | inferred |
| Black Market | Broad high-tier inventory | High and selective | Significantly higher | inferred |
| Home | Player storage, not commerce | None or near-none | Non-commercial | inferred |

---

## 8. Deferred to Later Phases

Not included in Phase 1:
- Runtime restock engine
- Runtime price computation
- NPC/storekeeper behavior
- Persistence implementation
