# Definitions Organisation Guidelines

> Scope: all JSON files under `data/definitions/`.  
> These rules apply to both humans and AI agents adding new game content.

---

## 1. Preferred Folder Structure

```
data/definitions/
  capabilities.json          ← passive bearer/item grants (boolean and scaled)
  statuses.json              ← timed conditions placed on creatures
  effects.json               ← named reusable effect instances (catalog)
  activations.json           ← artifact activation definitions
  ego_items.json             ← ego item templates (MAngband flags + modifiers)
  artifacts.json             ← unique artifact definitions
  races.json                 ← player race definitions
  classes.json               ← player class definitions
  flavors.json               ← unidentified item flavors
  race_class_rules.json      ← race/class combination rules
  monster_compendium.json    ← all monster definitions (single large file)
  items/
    accessories.json         ← rings and amulets
    armor.json               ← body armor
    weapons.json             ← melee and ranged weapons
    potions.json             ← drinkable one-use items
    scrolls.json             ← readable one-use items
    rods.json                ← rechargeable devices
    wands.json               ← charge-based targeted devices
    staves.json              ← charge-based area devices
    consumables.json         ← food, mushrooms, lantern oil
    chests.json              ← chest templates
    item_affixes.json        ← affix type catalog
    item_capabilities.json   ← item-scope capability definitions
```

---

## 2. File Splitting Rules

### 2.1 Split only when a single file becomes unwieldy

A definition file should be split when it reaches **more than ~500 entries** AND the entries have clearly distinct subtypes with no cross-references between subtypes.

Do **not** split proactively. A single file covering all rings and amulets is correct. A file with 50 entries does not need a folder.

### 2.2 Split on type boundary, not on alphabet or depth range

Acceptable split: `armor.json` vs `accessories.json` (different tval categories, different schema).  
Not acceptable: `armor_light.json` + `armor_heavy.json` (same schema, arbitrary grouping).

### 2.3 Monster definitions stay in a single file

`monster_compendium.json` contains all 616+ monsters in one file. This is intentional — it preserves serial-order lookup and avoids split-range bugs. Do not split it into folders by monster type or depth.

### 2.4 The `items/` folder exists because items have many distinct tval categories

The `items/` folder groups the seven structural item types (armor, weapons, accessories, devices, consumables, chests). These map to separate schemas. Do not create sub-folders inside `items/`.

---

## 3. Rules for Introducing New Folders

A new folder under `data/definitions/` is appropriate only when **all three** conditions are met:

1. The content requires a **new schema** (not covered by any existing schema).
2. There are **at least two** distinct files to place in the folder.
3. The content cannot be expressed as additional entries in an existing file.

**Example that meets the bar**: spells — if player spells are added as a new content type with their own schema and multiple spell-book files.  
**Example that does not meet the bar**: adding traps — a `traps.json` at the top level is correct; a `traps/` folder with one file is not.

Never create a folder to hold a single file.

---

## 4. File Naming Rules

- All filenames are `snake_case.json`.
- Filename matches the primary JSON array key: `potions.json` contains `{ "potions": [...] }`.
- Schema files mirror definition paths: `data/schemas/items/potions.schema.json` ↔ `data/definitions/items/potions.json`.
- Do not prefix with category (`item_potions.json` is wrong; `potions.json` is correct).

---

## 5. Schema Co-location Rule

Every definition file must have a corresponding schema file:

```
data/definitions/X.json        → data/schemas/X.schema.json
data/definitions/items/X.json  → data/schemas/items/X.schema.json
```

Schema files are required before data is committed. Adding a definition file without a schema file is a validation error.

---

## 6. AI Agent Guidance

### When adding new items to an existing category

Place the item in the correct existing file. Do not create a new file for a single item or a small group.

```
New ring → data/definitions/items/accessories.json
New potion → data/definitions/items/potions.json
New monster → data/definitions/monster_compendium.json
```

### When adding a genuinely new content type

1. Check that no existing file can express the content.
2. Create one file at the appropriate level (top-level or `items/`).
3. Create a matching schema file.
4. Register the file in `validate.test.js`.
5. Do not create a folder unless you have at least two files.

### Effect IDs

Use only effect_ids listed in `data/schemas/items/common_item.schema.json#/$defs/EffectId.enum`. Do not invent new effect_ids without updating the schema and adding a test.

### Capability IDs

Reference only capability IDs that exist in `data/definitions/capabilities.json` or `data/definitions/items/item_capabilities.json`. Do not reference a capability ID that is not in either file.

### Status IDs

Reference only status_ids that exist in `data/definitions/statuses.json`. Do not invent status_ids.

### Condition format

Effect conditions must be typed objects. Never use the legacy string format:

```jsonc
// WRONG — legacy string format (rejected by schema)
"condition": "unless:res_blind"

// CORRECT — typed object
"condition": { "requires_absence": "res_blind" }
"condition": { "forced": true }
"condition": { "on_next_melee_hit": true }
```

Valid condition keys: `requires_absence`, `forced`, `on_next_melee_hit`.  
The value of `requires_absence` must be a known capability ID.

### Accessories

Rings and amulets do **not** use `effects[]`. Passive grants belong in `capability_ids[]`. If the grant is numeric (stat bonus), use `affixes[]`. Do not add `effects[]` to any accessory.

---

## 7. Known Architectural Limitations

The following data patterns are intentionally not resolved in the current file structure. They are documented here so future contributors do not re-introduce workarounds.

### L-01 · Sustain-suppressed stat drain on cursed accessories

`ring_of_weakness` and `ring_of_stupidity` apply a stat penalty via `affixes[]` but provide no data-level way to express "this affix is suppressed if the bearer has the corresponding sustain capability." The suppression is currently a runtime hardcoded behaviour.

Do not attempt to model this in `effects[]` using phantom effect_ids (`reduce_stat_str`, etc.). These were present and have been removed. The architectural resolution (extending `affixes[]` with an optional condition, or migrating to the `grants[]` model from the architecture review) is tracked in `GAMEPLAY_ARCHITECTURE_REVIEW.md §F-08`.

### L-02 · Undocumented affix types in accessories

Several rings and amulets use affix types not listed in `item_affixes.json`:
- `fire_resistance`, `ice_resistance`, `acid_resistance`, `lightning_resistance` (percentage values)
- `fire_damage_percent`, `ice_damage_percent`, `lightning_damage_percent`
- `spell_power`, `attack_power`, `critical_chance`, `dodge_chance`
- `hp_regen`, `mana_regen`, `mana`, `poison_damage`, `poison_resistance`

These are IronHell-specific affix types not in the MAngband reference. They are valid but not yet formally catalogued in `item_affixes.json`. This does not require immediate action but should be catalogued before the affixes system is locked.

### L-03 · `on_next_melee_hit` trigger is architecturally distinct

The `on_next_melee_hit` condition (used by Scroll of Monster Confusion) is semantically different from resistance checks (`requires_absence`). It is a deferred trigger, not a guard. It is currently accepted by the schema as a condition variant, but the runtime handling differs from the other two condition types. See `GAMEPLAY_ARCHITECTURE_REVIEW.md §F-07` for context.
