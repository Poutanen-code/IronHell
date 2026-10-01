# Dungeon Generation Final Research Audit

Final focused, read-only audit of unresolved MAngband 1.5.3 dungeon-generation mechanics. No runtime, JSON, schema, migration, or compendium changes were made by this audit.

## 1. Executive Summary

**Verified**: all 16 floor-trap offsets have executable trigger/effect branches in `cmd1.c :: hit_trap()`. Hidden traps are discovered by search or movement, then immediately triggered by movement.

**Verified**: type-9 vault records are normalized to runtime type 8 by `init1.c :: parse_v_info()`. They are gigantic greater-vault inputs, not dead runtime types.

**Verified**: monster allocation tables are built by `init2.c :: init_alloc()`. A race with nonzero rarity receives weight `100 / rarity`, grouped and ordered by native level.

**Verified**: nest/pit filters reject uniques and apply family predicates. They do not explicitly reject reproducing monsters; group/escort behavior is suppressed because nest/pit placement calls use `grp = FALSE`.

**Verified**: wilderness terrain generation requests `wild_info[Depth].type` monsters, uses terrain-derived `monster_level`, applies the shared allocation path, and uses terrain-specific hooks. Terrain determinism does not establish population determinism.

**Verified**: static levels are run-length encoded dungeon snapshots loaded from `server.level.<k>.<j>.<i>` save-directory files. The available distribution contains no default static level files; the loader is configuration-gated.

**Verified**: `ref-mangband/src/server/mdefines.h` defines `NASTY_MON = 50`, `GREAT_OBJ = 20`, `MIN_M_ALLOC_LEVEL = 14`, `MIN_M_ALLOC_TD = 4`, and `MIN_M_ALLOC_TN = 8`. The exact monster OOD roll is 1-in-50, the object/gold OOD roll is 1-in-20, and ordinary dungeon generation starts from 14 monster requests before its random and depth-dependent additions.

**Recommendation**: **READY TO FREEZE** for the MAngband source baseline. Remaining unknowns are configuration-dependent, distribution-dependent, or IronHell implementation gaps rather than missing core constants or allocation formulas.

## 2. Trap Semantics

### Trigger and selection chain

**Verified**: `cmd1.c :: search()` calls `pick_trap()` when a search discovers `FEAT_INVIS`, but does not trigger it immediately.

**Verified**: `cmd1.c :: move_player()` discovers `FEAT_INVIS`, calls `pick_trap()`, then calls `hit_trap()` immediately. Visible traps call `hit_trap()` directly on entry.

**Verified**: `object2.c :: pick_trap()` selects `FEAT_TRAP_HEAD + randint0(16)`. On quest levels and at `MAX_DEPTH - 1`, offset 0 is rerolled. Therefore selection is uniform over 16 offsets normally, and uniform over offsets 1..15 on quest/bottom levels.

**Verified**: `FEAT_INVIS` is a hidden marker before instantiation. It has no trap-specific effect until `pick_trap()` or an equivalent detection path converts it to a visible trap.

**Verified**: traps generally remain in place after activation. Offset 4 explicitly changes the feature to `FEAT_FLOOR`; offset 0 moves the player to the next level; other offsets do not clear the feature in `hit_trap()` and can therefore trigger again if the player re-enters.

### Complete offset table

| Offset | Feature | Effect | Restrictions / defenses | Resulting terrain | Classification | Provenance |
|---:|---|---|---|---|---|---|
| 0 | Trap door | `2d8` damage unless `feather_fall`; sets `new_level_flag`, `LEVEL_RAND`, and increments depth | Ignored on wilderness and player quest levels; not selected on global quest/bottom generation | Player leaves current grid; trap grid is not explicitly cleared | Verified | `cmd1.c :: hit_trap()`; `object2.c :: pick_trap()` |
| 1 | Pit | `2d6` damage unless `feather_fall` | `feather_fall` avoids damage | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 2 | Spiked pit | Base `2d6`; 50% chance doubles damage and applies cut `randint1(dam)` | `feather_fall` avoids damage/spikes | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 3 | Poisonous spiked pit | Base `2d6`; 50% spike chance doubles damage, applies cut, then doubles damage again for poison amount unless poison resistance/opposition | `feather_fall`; poison resistance/opposition | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 4 | Summoning trap | Clears to `FEAT_FLOOR`; summons `2 + randint1(3)` monsters with `summon_specific(..., Depth, 0)` | Summon success depends on normal summon rules | `FEAT_FLOOR`; no repeat trigger | Verified | `cmd1.c :: hit_trap()` |
| 5 | Teleport trap | `teleport_player(p_ptr, 100)` | Normal teleport restrictions in teleport implementation | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 6 | Fire trap | `damroll(4, 6)` through `fire_dam()` | Fire resistance/immune behavior in `fire_dam()` | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 7 | Acid trap | `damroll(4, 6)` through `acid_dam()` | Acid resistance/immune behavior in `acid_dam()` | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 8 | Slow dart | Hit check power 125; on hit `1d4` and slow `20 + randint0(20)` | `check_hit()`; no separate saving throw | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 9 | Strength dart | Hit check power 125; on hit `1d4` and `do_dec_stat(A_STR)` | `check_hit()` | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 10 | Dexterity dart | Hit check power 125; on hit `1d4` and `do_dec_stat(A_DEX)` | `check_hit()` | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 11 | Constitution dart | Hit check power 125; on hit `1d4` and `do_dec_stat(A_CON)` | `check_hit()` | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 12 | Blindness gas | Blindness `25 + randint0(50)` unless `resist_blind` | Resistance prevents status | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 13 | Confusion gas | Confusion `10 + randint0(20)` unless `resist_conf` | Resistance prevents status | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 14 | Poison gas | Poison `10 + randint0(20)` unless poison resistance/opposition | Resistance/opposition prevents status | Trap remains | Verified | `cmd1.c :: hit_trap()` |
| 15 | Paralyzing mist | Paralysis `5 + randint0(10)` unless `free_act` | Free action prevents status | Trap remains | Verified | `cmd1.c :: hit_trap()` |

**Verified**: offset 0 is the trap door because `cmd1.c :: hit_trap()` handles `FEAT_TRAP_HEAD + 0x00` with level transition logic.

**Verified**: trap-door rejection occurs in two places with different scopes. `object2.c :: pick_trap()` rejects offset 0 for global quest depth and bottom depth. `cmd1.c :: hit_trap()` additionally ignores the effect for wilderness and player-specific quest levels.

**Derived**: rerolling offset 0 changes the final selection distribution from 1/16 each to 1/15 for offsets 1..15 on quest/bottom levels.

**Verified**: no trap branch in `hit_trap()` reselects itself. Offset 4 clears itself; offset 0 transitions away; all others retain their feature.

**Unresolved**: the exact movement/discovery behavior for every non-player actor is not established by the inspected `hit_trap()` path. The shown path is player-specific.

## 3. Type-9 Vault Investigation

### Source records

The seven type-9 records are:

| Serial | Name | Rating | Rows | Columns |
|---:|---|---:|---:|---:|
| 120 | Roundabout Three | 30 | 20 | 97 |
| 122 | Cyclone | 40 | 23 | 91 |
| 123 | Concentricity | 20 | 19 | 76 |
| 124 | a version with the Pattern | 50 | 41 | 82 |
| 125 | Greater Vault (Titanic) | 50 | 43 | 99 |
| 126 | Greater Vault (Un Named I) | 50 | 45 | 81 |
| 127 | *Greater Vault* (Divided) | 50 | 45 | 81 |

**Verified**: `init1.c :: parse_v_info()` accepts type 9 up to 99 columns and 66 rows, then immediately executes `if (v_ptr->typ == 9) v_ptr->typ = 8`.

### Answers A-E

**A. What does vault type 9 mean?**

**Verified**: in the available executable source, type 9 is an input encoding for a gigantic greater vault. The parser normalizes it to runtime type 8 after applying the larger size limit.

**B. Which function selects type-9 records?**

**Verified**: no function selects `typ == 9` at runtime. `generate.c :: build_type8()` selects entries whose parsed `v_ptr->typ == 8`; after parsing, original type-9 records satisfy that condition.

**C. Can type-9 vaults appear during normal `cave_gen()`?**

**Verified**: yes, if the records are loaded and selected by `build_type8()`. They are runtime type-8 entries after parsing and therefore can be selected by ordinary greater-vault generation at depth 10 or deeper.

**D. Are they used for towns, quests, Ironman, special/static levels, or another system?**

**Unresolved**: no separate type-9 consumer was found. Their direct consumer is ordinary greater-vault selection after normalization. No evidence assigns them specifically to town, quest, Ironman, or static-level generation.

**E. Are they dead/legacy data?**

**Verified**: they are not dead in the available parser/generator path. They are normalized and eligible as type-8 records. Their historical naming or origin is outside the executable semantics.

## 4. Monster Allocation Specification

### Allocation-table construction

**Verified**: `init2.c :: init_alloc()` scans monster races from index 1 through `z_info->r_max - 2`, excluding the ghost endpoint. Races with `r_ptr->rarity == 0` are excluded.

**Verified**: each eligible race creates one `alloc_entry` with:

```text
index = race index
level = r_ptr->level
prob1 = 100 / r_ptr->rarity
prob2 = prob1
prob3 = prob1
```

Entries are grouped by native level using cumulative `num[]` offsets and ordered by level.

**Derived**: rarity is converted to an integer weight, not used directly as a probability. Lower rarity values produce larger weights through integer division.

### Probability lifecycle

**Verified**: `prob1` is the immutable base weight produced by `init_alloc()`.

**Verified**: `get_mon_num_prep()` copies `prob1` to `prob2` when the active hook accepts the race, otherwise sets `prob2 = 0`.

**Verified**: `get_mon_num()` sets `prob3 = 0` for entries outside the requested effective level or excluded by town, `FORCE_DEPTH`, or wilderness-unique rules. Accepted entries receive `prob3 = prob2`.

**Verified**: selection sums `prob3` into `total`, samples `randint0(total)`, and scans cumulative weights. A zero-probability entry remains in the table but contributes nothing.

### Selection and OOD pseudocode

**Verified**: the source-equivalent algorithm is:

```text
function prepare_monster_table(hook):
    for entry in alloc_race_table:
        if hook is absent or hook(entry.race_index):
            entry.prob2 = entry.prob1
        else:
            entry.prob2 = 0

function get_monster(level):
    if ironman and (level == 0 or check_special_level(level)):
        return none
    if more_towns and check_special_level(level):
        return none

    if level == 0 and town_count > cfg_max_townies:
        return none

    if level > 0:
        if randint0(NASTY_MON) == 0:
            level += min(level / 4 + 2, 5)
        if randint0(NASTY_MON) == 0:
            level += min(level / 4 + 2, 5)

    for entry in alloc_race_table:
        entry.prob3 = 0
        if entry.level > level:
            stop scanning
        if level > 0 and entry.level <= 0:
            continue
        race = entry.race
        if race has FORCE_DEPTH and race.level > level:
            continue
        if race is UNIQUE and level < 0:
            continue
        entry.prob3 = entry.prob2

    total = sum(entry.prob3)
    if total <= 0:
        return none

    candidate = weighted_pick(prob3)
    p = randint0(100)

    if p < 60:
        second = weighted_pick(prob3)
        keep candidate with smaller abs(native_level)

    if p < 10:
        third = weighted_pick(prob3)
        keep candidate with smaller abs(native_level)

    return selected race
```

**Verified**: the two `NASTY_MON` calls are independent source-level rolls. The second boost uses the already-boosted level.

**Verified**: `ref-mangband/src/server/mdefines.h` defines `NASTY_MON = 50`. Each successful OOD roll therefore has probability 1/50, and the two calls are independent source-level rolls.

**Verified**: `FORCE_DEPTH` is an allocation-level ceiling: a race whose native level exceeds the effective level is excluded. This does not prevent ordinary races from being out of depth.

**Verified**: unique eligibility is not fully enforced in `get_mon_num()`. The source contains a disabled `cur_num >= max_num` check. Effective unique restrictions are enforced by `place_monster_one() :: allow_unique_level()` and current/max counters.

**Verified**: selection is with replacement during the initial and candidate comparison rolls. No table entries are removed between rolls.

### Worked examples

**Derived — normal same-depth allocation**: with effective level `L`, all entries with native level `<= L` and nonzero `prob2` contribute `prob3 = 100 / rarity`. A race with rarity 5 contributes weight 20; a race with rarity 20 contributes weight 5. The first pick is proportional to those weights, followed by the source's 60%/10% candidate comparison rolls.

**Derived — one successful OOD boost**: when the first `randint0(50)` roll succeeds at level `L`, effective level becomes `L + min(L / 4 + 2, 5)`. The second independent 1-in-50 roll may fail, after which allocation includes all eligible races up to the boosted level, subject to `FORCE_DEPTH`.

**Derived — unique/FORCE_DEPTH rejection**: a unique race can remain in the weighted table if its level is eligible, but `place_monster_one()` rejects it when `allow_unique_level()` is false or `cur_num >= max_num`. A `FORCE_DEPTH` race is excluded before weighted selection whenever its native level exceeds effective allocation level.

## 5. Nest and Pit Allocation

### Family predicates

**Verified**: `generate.c` family hooks are:

| Family | Predicate |
|---|---|
| Jelly nest | Non-unique and symbol in `i`, `j`, `m`, `,` |
| Animal nest | Non-unique and `RF3_ANIMAL` |
| Undead nest | Non-unique and `RF3_UNDEAD` |
| Orc pit | Non-unique and symbol `o` |
| Troll pit | Non-unique and symbol `T` |
| Giant pit | Non-unique and symbol `P` |
| Dragon pit | Non-unique, symbol `d` or `D`, and `flags4 == vault_aux_dragon_mask4` |
| Demon pit | Non-unique and symbol `U` |

**Verified**: these hooks do not explicitly reject `RF2_MULTIPLY`. They also do not directly inspect rarity or `FORCE_DEPTH`; rarity remains the allocation weight and `FORCE_DEPTH` is enforced inside `get_mon_num()`.

### Nest algorithm

**Verified**: `build_type5()` installs one family hook, calls `get_mon_num_prep()`, samples 64 races using `get_mon_num(Depth + 10)`, and aborts the nest if any sample returns zero. It then restores the hook and fills the inner region using those sampled race IDs.

**Verified**: nest placement calls `place_monster_aux(..., FALSE, FALSE)`. This suppresses FRIENDS and ESCORT/ESCORTS expansion for those placements.

**Derived**: normal rarity weights, effective level `Depth + 10`, `FORCE_DEPTH`, and all ordinary allocation-table filters still affect the 64 samples.

### Pit algorithm

**Verified**: `build_type6()` installs the selected family hook and samples 16 races using `get_mon_num(Depth + 10)`. Any failed sample aborts the pit.

**Verified**: the 16 samples are bubble-sorted by native level, then entries at indexes 0, 2, 4, 6, 8, 10, 12, and 14 are retained as the eight strength tiers.

**Verified**: the eight retained races are placed in a fixed spatial pattern using `place_monster_aux(..., FALSE, FALSE)`. Group and escort expansion is therefore suppressed.

**Unresolved**: failed individual placement attempts inside the fixed nest/pit pattern are not retried by the room builder. The source ignores the return value of those placement calls.

## 6. Wilderness Monster Population

**Verified**: `wilderness_gen_hack()` sets `monster_level = terrain.monst_lev` after `init_terrain()`.

**Verified**: after terrain generation, `wilderness_gen()` executes:

```text
for (i = 0; i < wild_info[Depth].type; i++)
    wild_add_monster(Depth)
```

This occurs once for daytime and once for nighttime branches, with the same count expression.

**Verified**: `wild_add_monster()` installs a terrain-specific allocation hook, prepares `prob2`, finds a naked location, calls `get_mon_num(monster_level)`, and places the result through `place_monster_aux(..., FALSE, TRUE)`.

**Verified**: because `monster_level` is terrain-derived and positive, the shared OOD rolls in `get_mon_num()` still apply unless configuration/build constants or later filters prevent them.

**Verified**: terrain hooks restrict symbols/species by biome. Wilderness placement does not use the ordinary `alloc_monster()` distance-from-player requirement; it selects any naked location after up to 50 search attempts in `wild_add_monster()`.

**Verified**: wilderness unique exclusion in `get_mon_num()` is keyed to `level < 0`. Since wilderness terrain assigns a positive `monster_level`, that particular exclusion does not fire for `wild_add_monster()`. Placement still applies `allow_unique_level()` and unique counters.

**Verified**: `place_monster_aux(..., FALSE, TRUE)` permits FRIENDS and ESCORT behavior for wilderness leaders. `MULTIPLY` is a later runtime reproduction behavior, not an initial population modifier.

**Derived**: terrain layout is seeded through `Rand_quick` and `seed_town`, but monster population also consumes RNG during race selection, placement, groups, and escorts. Deterministic terrain therefore does not imply deterministic population unless the complete RNG state and call order are preserved.

**Verified**: wilderness monsters are retained in shared monster lists and are not wiped by `dealloc_dungeon_level()` because that function only wipes monsters for positive depths. Persistence across unloading is therefore state/save dependent rather than ordinary dungeon regeneration.

**Verified**: the initial wilderness request count is exactly the numeric value of `wild_info[Depth].type`; the available source does not expose the enum values in the inspected header set, so a terrain-name-to-count table remains unavailable. This does not prevent specifying the call chain or count expression.

## 7. Static/Special Levels

### Engine capability

**Verified**: `load2.c :: rd_dungeon()` reads a `dungeon_level` section containing depth, dimensions, generation turn, player count, stair coordinates, run-length/binary feature rows, and info rows. It allocates the cave if needed and restores terrain/features and grid flags.

**Verified**: `load2.c :: rd_dungeon_special()` searches the save directory for `server.level.<k>.<j>.<i>` with `k = 0`, `j = 0`, and `i` from 0 through `MAX_DEPTH - 1` in the available loop.

**Verified**: static snapshots can contain terrain/features, grid flags, stair coordinates, and generation metadata. Objects and monsters are loaded through later save sections or normal level-save handling, not by the feature reader itself.

**Verified**: save logic includes allocated special levels in the `dungeon_levels` section when `check_special_level(i)` is true and the cave exists.

### Default configuration

**Verified**: `rd_dungeon_special()` returns without scanning files unless `cfg_ironman` or `cfg_more_towns` is enabled.

### Distributed content

**Unresolved**: no `server.level.*` static level files were found in the inspected source/edit tree. The default distribution content cannot be proven beyond the available repository snapshot.

**Verified**: static level loading is separate from ordinary `generate_cave()` generation. The exact behavior of autoscum/destruction after loading a static snapshot is not established; the loader restores a level rather than invoking `cave_gen()`.

## 8. Generation Constants

| Name | Value | Subsystem | Classification |
|---|---:|---|---|
| `DUN_ROOMS` | 50 | Room attempts | Verified |
| `DUN_UNUSUAL` | 200 | Unusual-room roll | Verified |
| `DUN_DEST` | 15 | Destroyed levels | Verified |
| `DUN_TUN_RND` | 10 | Tunnel random direction | Verified |
| `DUN_TUN_CHG` | 30 | Tunnel direction correction | Verified |
| `DUN_TUN_CON` | 15 | Tunnel continuation | Verified |
| `DUN_TUN_PEN` | 25 | Entrance doors | Verified |
| `DUN_TUN_JCT` | 90 | Junction doors | Verified |
| `DUN_STR_DEN` | 5 | Streamer density | Verified |
| `DUN_STR_RNG` | 2 | Streamer width | Verified |
| `DUN_STR_MAG` | 3 | Magma streamer count | Verified |
| `DUN_STR_QUA` | 2 | Quartz streamer count | Verified |
| `DUN_STR_MC` | 90 | Magma treasure denominator | Verified |
| `DUN_STR_QC` | 40 | Quartz treasure denominator | Verified |
| `DUN_AMT_ROOM` | 9 | Room object mean | Verified |
| `DUN_AMT_ITEM` | 3 | General object mean | Verified |
| `DUN_AMT_GOLD` | 3 | General gold mean | Verified |
| `BLOCK_HGT/WID` | 11/11 | Room blocks | Verified |
| `CENT_MAX` | 100 | Room centers | Verified |
| `DOOR_MAX` | 400 | Tunnel junction candidates | Verified |
| `WALL_MAX` | 1000 | Tunnel wall piercings | Verified |
| `TUNN_MAX` | 1800 | Tunnel grids | Verified |
| `GROUP_MAX` | 32 | Monster groups | Verified |
| `MAX_SPECIAL_LEVELS` | 10 | Static levels | Verified |
| `MAX_DEPTH` | 128 | Dungeon bound | Verified |
| `MAX_M_IDX` | 32768 | Monster list | Verified |
| `MAX_O_IDX` | 32768 | Object list | Verified |
| `NASTY_MON` | 50 | 1-in-50 monster OOD roll | Verified |
| `GREAT_OBJ` | 20 | 1-in-20 object/gold OOD roll | Verified |
| `MIN_M_ALLOC_LEVEL` | 14 | Ordinary dungeon monster baseline | Verified |
| `MIN_M_ALLOC_TD` | 4 | Commented daytime town baseline | Verified |
| `MIN_M_ALLOC_TN` | 8 | Commented nighttime town baseline | Verified |

**Verified**: these values are defined in `ref-mangband/src/server/mdefines.h`. The town constants are currently used only in commented-out resident-generation loops in `town_gen()`.

## 9. Corrections to Existing Research

### Correction 1: type-9 vaults

**Previous claim**: type-9 purpose and runtime use were unresolved.

**Correct claim**: `init1.c :: parse_v_info()` converts type 9 to type 8 after allowing larger dimensions. Type-9 records are therefore eligible through `build_type8()` after parsing.

**Classification**: Verified.

**Source**: `ref-mangband/src/server/init1.c :: parse_v_info()`; `generate.c :: build_type8()`.

**Reason**: executable normalization is present.

### Correction 2: nest/pit breeder exclusion

**Previous claim**: nest/pit filters reject reproducing monsters.

**Correct claim**: family hooks explicitly reject uniques but do not inspect `RF2_MULTIPLY`. The allocation path may still reject a race for other reasons.

**Classification**: Verified.

**Source**: `generate.c :: vault_aux_jelly()`, `vault_aux_animal()`, `vault_aux_undead()`, `vault_aux_orc()`, `vault_aux_troll()`, `vault_aux_giant()`, `vault_aux_dragon()`, `vault_aux_demon()`.

**Reason**: executable predicates omit `RF2_MULTIPLY`.

### Correction 3: wilderness unique exclusion

**Previous claim**: wilderness allocation rejects unique races because `get_mon_num()` excludes uniques for negative levels.

**Correct claim**: `wild_add_monster()` passes positive terrain-derived `monster_level` to `get_mon_num()`, so the `level < 0` allocation exclusion does not apply there. Placement-level unique restrictions still apply.

**Classification**: Verified.

**Source**: `wilderness.c :: wilderness_gen_hack()`, `wild_add_monster()`; `monster2.c :: get_mon_num()`, `place_monster_one()`.

**Reason**: allocation level and world depth are different values in wilderness generation.

### Correction 4: exact OOD constants

**Previous claim**: exact OOD probabilities and `MIN_M_ALLOC_LEVEL` remained unavailable.

**Correct claim**: `mdefines.h` defines `NASTY_MON = 50`, `GREAT_OBJ = 20`, and `MIN_M_ALLOC_LEVEL = 14`. Town baselines are 4 daytime and 8 nighttime, although the current town allocation loops are commented out.

**Classification**: Verified.

**Source**: `monster2.c :: get_mon_num()`; `object2.c :: get_obj_num()`, `place_gold()`.

**Reason**: the newly added `ref-mangband/src/server/mdefines.h` contains the definitions.

## 10. Limited IronHell Mapping

| Mechanic | Current IronHell observation | Classification |
|---|---|---|
| 16 floor traps | Trap catalog exists, but no 16-offset feature allocator found | Observed / Unresolved |
| Type-9 vaults | 21 vault layouts exist; no runtime normalization/interpreter found | Observed / Unresolved |
| Monster allocation | Monster definitions and spawn fields exist; no allocation table found | Observed / Unresolved |
| Nest/pit filters | No corresponding generator/filter implementation found | No implementation found |
| Wilderness population | Terrain definitions/prose exist; no population generator found | Observed / Unresolved |
| Static levels | No static level loader/content counterpart found | No implementation found |
| Generation constants | No dungeon-generation constant module found | No implementation found |

## 11. Final Unresolved Questions

| Question | Classification | Reason unresolved | Parity impact | Blocks implementation? |
|---|---|---|---|---|
| Numeric wilderness type values | Unresolved | Enum/macros absent from available source snapshot | Medium | No |
| Full static-file distribution | Unresolved | No default `server.level.*` content in snapshot | Medium | No |
| Non-player trap triggering | Unresolved | Inspected trigger path is player-specific | Low | No |
| Static snapshot post-load autoscum/destruction | Unresolved | Loader and generation paths are separate; integration path unclear | Medium | No |

## 12. Research Freeze Recommendation

**READY TO FREEZE**.

The newly added `mdefines.h` resolves the exact monster OOD probability, object/
gold OOD probability, ordinary monster baseline, and town resident baselines.
Remaining unknowns are configuration-dependent, distribution-dependent, or
IronHell implementation gaps and do not block freezing the MAngband source
specification.

## 13. Proposed Final Compendium Patch

```markdown
### 6.3 Type-9 Vault Normalization

**Verified**: `init1.c :: parse_v_info()` accepts type-9 vault records under the
larger 99-column/66-row limit and then converts `v_ptr->typ` from 9 to 8.
Therefore type-9 records are runtime greater-vault entries selected by
`generate.c :: build_type8()`; no separate type-9 consumer exists.

### 7.4 Complete Floor-Trap Semantics

**Verified**: `cmd1.c :: hit_trap()` defines the 16 trap offsets. Offset 0 is a
trap door; offsets 1-3 are pits; offset 4 is a summoning trap; offset 5 is a
teleport trap; offsets 6-7 are fire/acid traps; offsets 8-11 are dart traps;
and offsets 12-15 are blindness, confusion, poison, and paralysis gas.

**Derived**: `pick_trap()` is uniform over 16 offsets normally and uniform over
15 offsets after excluding offset 0 on quest/bottom levels.

### 8.2 Allocation Table Semantics

**Verified**: `init2.c :: init_alloc()` creates one allocation entry per monster
with nonzero rarity, ordered by native level, with `prob1 = prob2 = prob3 =
100 / rarity`. `get_mon_num_prep()` filters entries by setting `prob2` to
`prob1` or zero. `get_mon_num()` derives `prob3`, sums it, and performs weighted
selection with replacement.

**Verified**: `mdefines.h` defines `NASTY_MON = 50`, `GREAT_OBJ = 20`, and
`MIN_M_ALLOC_LEVEL = 14`. The source therefore specifies 1-in-50 monster OOD,
1-in-20 object/gold OOD, and a 14-monster ordinary baseline.

### 8.3 Nest and Pit Filters

**Verified**: nest/pit hooks reject uniques and apply family predicates based on
symbol, `RF3_ANIMAL`, `RF3_UNDEAD`, or exact dragon `flags4` masks. They do not
explicitly reject `RF2_MULTIPLY`. Nest/pit placements pass `grp = FALSE`, so
FRIENDS and ESCORT/ESCORTS expansion is suppressed.

### 11.2 Wilderness Population

**Verified**: wilderness generation requests `wild_info[Depth].type` monsters,
sets `monster_level` from terrain, applies terrain allocation hooks, and calls
`get_mon_num(monster_level)`. Terrain seeding is deterministic under the
source seed protocol, but monster population also consumes selection,
placement, group, escort, and OOD RNG and is not thereby guaranteed deterministic.

### 14.3 Research Status

**Verified**: the matching build definitions for `NASTY_MON`, `GREAT_OBJ`, and
`MIN_M_ALLOC_LEVEL` are present in `ref-mangband/src/server/mdefines.h`; the
source specification is ready to freeze for these mechanics.
```

## 14. Research Closure Matrix

| Subsystem | Confidence | Remaining unknown | Parity blocker? |
|---|---|---|---|
| Dungeon lifecycle | High | Static/config integration | No |
| Rooms | High | Build-option variants | No |
| Room selection | High | Missing build constants only affect population | No |
| Connectivity | High | Enabled wide-corridor option | No |
| Tunnels | High | Build-option variants | No |
| Doors | High | Movement semantics | No |
| Stairs | High | Static-level integration | No |
| Vault selection | High | None material | No |
| Vault interpreter | High | IronHell counterpart absent | No for MAngband specification |
| Vault catalog | High | Source type-9 historical purpose | No |
| Nests | High | Candidate data statistics | No |
| Pits | High | Candidate data statistics | No |
| Monster allocation | High | Runtime integration and numeric wilderness enum values | No for source specification |
| Monster placement | High | Runtime integration | No for source specification |
| Monster groups | High | None material | No |
| Escorts | High | None material | No |
| Monster drops | High | IronHell executor | No for source specification |
| Object allocation | High | Runtime integration | No for source specification |
| Object quality | High | Build/config variants | No |
| Artifacts | High | Optional RANDART configuration | No |
| Traps | High | Non-player trigger scope | No |
| Level rating | High | Complete allocation constants | Medium |
| Level feelings | High | Client wording | No |
| Destroyed levels | High | None material | No |
| Town | High | Configuration variants | No |
| Quest levels | High | Player-state integration | No |
| Static levels | High | Distributed file content | No |
| Wilderness geometry | High | Missing enum values | No |
| Wilderness population | High | Numeric wilderness enum values and runtime integration | No for source specification |

## Research Closure

**Files inspected**: `DUNGEON_GENERATION_COMPENDIUM.md`,
`docs/research/DUNGEON_GENERATION_GAP_AUDIT.md`, MAngband `generate.c`,
`monster2.c`, `object2.c`, `cmd1.c`, `cave.c`, `init1.c`, `init2.c`,
`load2.c`, `save.c`, `wilderness.c`, `vault.txt`, relevant headers, and
current IronHell monster, vault, trap, and definition-loading sources.

**Commands/tools run**: read-only workspace search, source reads, PowerShell
symbol searches, and PowerShell extraction of type-9 vault records. No tests
were run.

**Files changed**: `docs/research/DUNGEON_GENERATION_FINAL_RESEARCH_AUDIT.md` only.

**New Verified findings**: 16 trap effects, type-9 normalization, allocation
entry construction, probability lifecycle, nest/pit predicates, wilderness
population call chain, and static snapshot format.

**New Observed findings**: no direct IronHell counterparts for the focused
mechanics were found; current definitions are partial catalog representations.

**New Derived findings**: quest/bottom trap selection is uniform over 15 allowed
offsets; deterministic terrain does not imply deterministic population.

**New Inferred findings**: none promoted beyond the classifications above.

**Remaining Unresolved findings**: numeric wilderness enum values, some
build/configuration variants, non-player trap triggering, static distribution
content, and IronHell runtime consumers.

**Corrections to previous research**: 4.

**High-impact parity blockers**: absent IronHell allocation/generation
consumers remain implementation gaps. No missing MAngband generation constant
now blocks the source specification.
