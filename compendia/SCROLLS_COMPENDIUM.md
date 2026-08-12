**Scrolls Compendium — MAngband (reference 1.5.3)**

Source notes:
- Scroll implementations: reference-mangband-1_5_3/src/server/use-obj.c (`read_scroll()`)
- Scroll constants: reference-mangband-1_5_3/src/server/mdefines.h
- Object metadata/dungeon allocation: reference-mangband-1_5_3/lib/edit/object.txt ("Scrolls" section)
- Store stock: reference-mangband-1_5_3/src/server/init2.c (Temple and Alchemy shop entries)

How to read these entries:
- Target: self/directional/area; Effect: implementation call and numeric args; Store availability: counts from `store_table` / `STORE_CHOICES` (32).

Temple store (static slots): notable scrolls and counts
- `SV_SCROLL_REMOVE_CURSE` — 1/32 (3.125%)
- `SV_SCROLL_BLESSING` — 1/32 (3.125%)
- `SV_SCROLL_HOLY_CHANT` — 1/32 (3.125%)
- `SV_SCROLL_LIFE` — 2/32 (6.25%)
(Temple also stocks prayer books in the same store block.)

Alchemy shop (static slots): notable scroll counts
- `SV_SCROLL_IDENTIFY` — 4/32 (12.5%)
- `SV_SCROLL_ENCHANT_WEAPON_TO_HIT` — 2/32 (6.25%)
- `SV_SCROLL_ENCHANT_WEAPON_TO_DAM` — 2/32 (6.25%)
- `SV_SCROLL_ENCHANT_ARMOR` — 2/32 (6.25%)
- `SV_SCROLL_PHASE_DOOR` — 3/32 (9.375%)
- `SV_SCROLL_WORD_OF_RECALL` — 4/32 (12.5%)
- `SV_SCROLL_LIGHT` — 1/32 (3.125%)
- `SV_SCROLL_MAPPING` — 1/32 (3.125%)
- `SV_SCROLL_MONSTER_CONFUSION` — 1/32 (3.125%)
- `SV_SCROLL_DETECT_DOOR` — 1/32 (3.125%)
- `SV_SCROLL_DETECT_INVIS` — 1/32 (3.125%)
- `SV_SCROLL_RECHARGING` — 1/32 (3.125%)
- `SV_SCROLL_SATISFY_HUNGER` — 1/32 (3.125%)
- `SV_SCROLL_ENCHANT_WEAPON_TO_HIT` (additional slot) — included above

Scroll list (from `read_scroll()`):

- `SV_SCROLL_DARKNESS` (0)
  - Target: area/self
  - Effect: `unlite_area(p_ptr, 10, 3)` and possible blindness
  - Stores: not listed in Temple/Alchemy static blocks (0/32)

- `SV_SCROLL_AGGRAVATE_MONSTER` (1)
  - Target: none
  - Effect: `aggravate_monsters(p_ptr, 0)`
  - Stores: 0/32

- `SV_SCROLL_CURSE_ARMOR` (2) / `SV_SCROLL_CURSE_WEAPON` (3)
  - Target: self
  - Effect: `curse_armor()` / `curse_weapon()`
  - Stores: 0/32

- `SV_SCROLL_SUMMON_MONSTER` (4) / `SV_SCROLL_SUMMON_UNDEAD` (5)
  - Target: none
  - Effect: `summon_specific()` (1..3 summons)
  - Stores: 0/32

- `SV_SCROLL_TRAP_CREATION` (7)
  - Target: area
  - Effect: `trap_creation()`
  - Stores: 0/32

- `SV_SCROLL_PHASE_DOOR` (8)
  - Target: self
  - Effect: `teleport_player(p_ptr, 10)`
  - Stores: Alchemy: 3/32 (9.375%)

- `SV_SCROLL_TELEPORT` (9)
  - Target: self
  - Effect: `teleport_player(p_ptr, 100)`
  - Stores: 0/32

- `SV_SCROLL_TELEPORT_LEVEL` (10)
  - Target: self
  - Effect: `teleport_player_level()`
  - Stores: 0/32 (Ironman variants may differ)

- `SV_SCROLL_WORD_OF_RECALL` (11)
  - Target: self
  - Effect: `set_recall(p_ptr, o_ptr)`
  - Stores: Alchemy: 4/32 (12.5%)

- `SV_SCROLL_IDENTIFY` (12) / `SV_SCROLL_STAR_IDENTIFY` (13)
  - Target: self / item
  - Effect: `ident_spell()` / `identify_fully()` (may not consume scroll on abort)
  - Stores: Alchemy: `IDENTIFY` 4/32 (12.5%); Temple not (0/32) except special cases.

- `SV_SCROLL_REMOVE_CURSE` (14) / `SV_SCROLL_STAR_REMOVE_CURSE` (15)
  - Target: self
  - Effect: `remove_curse()` / `remove_all_curse()`
  - Stores: Temple: `REMOVE_CURSE` 1/32 (3.125%); Alchemy: 0/32

- `SV_SCROLL_ENCHANT_ARMOR` (16) / `SV_SCROLL_ENCHANT_WEAPON_TO_HIT` (17) / `SV_SCROLL_ENCHANT_WEAPON_TO_DAM` (18)
  - Target: item
  - Effect: `enchant_spell(...)` (see code for parameters)
  - Stores: Alchemy: each of the three appears twice across the Alchemy block (2/32 each where shown), plus earlier duplicates for weapon enchant in the block (see counts above).

- `SV_SCROLL_RECHARGING` (22)
  - Target: self / item
  - Effect: `recharge(p_ptr, 60)`
  - Stores: Alchemy: 1/32 (3.125%)

- `SV_SCROLL_LIGHT` (24)
  - Target: area
  - Effect: `lite_area(p_ptr, damroll(2,8), 2)`
  - Stores: Alchemy: 1/32 (3.125%); Temple: 0/32 (Temple stocks `LIFE`/blessings)

- `SV_SCROLL_MAPPING` (25)
  - Target: self
  - Effect: `map_area()`
  - Stores: Alchemy: 1/32 (3.125%)

- `SV_SCROLL_DETECT_GOLD` (26) / `SV_SCROLL_DETECT_ITEM` (27) / `SV_SCROLL_DETECT_TRAP` (28) / `SV_SCROLL_DETECT_DOOR` (29) / `SV_SCROLL_DETECT_INVIS` (30)
  - Target: self
  - Effect: `detect_treasure()` / `detect_objects_normal()` / `detect_trap()` / `detect_sdoor()` / `detect_invisible()`
  - Stores: Alchemy includes `DETECT_DOOR` 1/32 and `DETECT_INVIS` 1/32; Temple/Alchemy distribution for others is 0/32 or listed in other store blocks.

- `SV_SCROLL_SATISFY_HUNGER` (32)
  - Target: self
  - Effect: `set_food(p_ptr, PY_FOOD_MAX - 1)`
  - Stores: Alchemy: 1/32 (3.125%)

- `SV_SCROLL_BLESSING` (33) / `SV_SCROLL_HOLY_CHANT` (34) / `SV_SCROLL_HOLY_PRAYER` (35)
  - Target: self
  - Effect: `set_blessed(...)` with durations (small/medium/large)
  - Stores: Temple: `BLESSING` 1/32, `HOLY_CHANT` 1/32; `HOLY_PRAYER` not listed in Temple static block (0/32).

- `SV_SCROLL_MONSTER_CONFUSION` (36)
  - Target: self
  - Effect: set player's `confusing` flag (temporary)
  - Stores: Alchemy: 1/32 (3.125%);

- `SV_SCROLL_PROTECTION_FROM_EVIL` (37)
  - Target: self
  - Effect: `set_protevil(p_ptr, p_ptr->protevil + randint1(25) + 3 * p_ptr->lev)`
  - Stores: 0/32 in Alchemy/Temple static blocks

- `SV_SCROLL_RUNE_OF_PROTECTION` (38)
  - Target: self
  - Effect: `warding_glyph()`
  - Stores: 0/32

- `SV_SCROLL_TRAP_DOOR_DESTRUCTION` (39)
  - Target: self / touch
  - Effect: `destroy_doors_touch()`
  - Stores: 0/32

- `SV_SCROLL_STAR_DESTRUCTION` (41)
  - Target: area
  - Effect: `destroy_area(Depth, py, px, 15, TRUE)`
  - Stores: 0/32

- `SV_SCROLL_DISPEL_UNDEAD` (42) / `SV_SCROLL_BANISHMENT` (44) / `SV_SCROLL_MASS_BANISHMENT` (45)
  - Target: area / none
  - Effect: `dispel_undead()` / `banishment()` / `mass_banishment()`
  - Stores: 0/32 in Alchemy/Temple static blocks for some of these; check `object.txt` and dungeon allocation.

- `SV_SCROLL_ACQUIREMENT` (46) / `SV_SCROLL_STAR_ACQUIREMENT` (47)
  - Target: area
  - Effect: `acquirement()` (1 or more items)
  - Stores: 0/32 in Alchemy/Temple static blocks

- `SV_SCROLL_LIFE` (48) — MAngband-specific
  - Target: self
  - Effect: `restore_level()` + `do_scroll_life()` (DM/variant-specific)
  - Stores: Temple: 2/32 (6.25%)

- `SV_SCROLL_CREATE_ARTIFACT` (6) / `SV_SCROLL_CREATE_HOUSE` (49) — DM-restricted
  - Target: self
  - Effect: `create_artifact()` / `create_house()` (may be restricted to DM or server flags)
  - Stores: not sold (0/32)

Notes on reading/usage:
- `read_scroll()` returns whether the scroll was used up; some effects may not consume the scroll if aborted (e.g., `identify` may set `used_up = FALSE`). There is no single failure% formula like `spell_chance()` for spells — usage failures are conditional or precondition-based.
- Dungeon allocations (`A:`) and object `W:` levels are authoritative in `lib/edit/object.txt` (Scrolls section). Use that file for spawn depths and rarities.

References:
- `read_scroll()` in: reference-mangband-1_5_3/src/server/use-obj.c (lines ~700-1150)
- Scroll object data: reference-mangband-1_5_3/lib/edit/object.txt (Scrolls section)
- Store table: reference-mangband-1_5_3/src/server/init2.c (Temple and Alchemy blocks)

If you want an explicit per-scroll table with `store_name,occurrences,percent` for every store block I can generate it by parsing `init2.c` and adding a CSV to `compendia/scrolls_store_table.csv`.
