# Dungeon Generation Compendium — MAngband 1.5.3

This document is a research reference for the original MAngband 1.5.3 dungeon
generator. It is intentionally source-shaped and does not define an IronHell
runtime model.

## Evidence Classes

- **Verified**: directly stated or directly executable in the referenced source.
- **Observed**: visible in source/data, but descriptive rather than a complete semantic conclusion.
- **Derived**: calculated from verified constants or formulas.
- **Inferred**: a reasoned interpretation that is not directly guaranteed by the source.
- **Unresolved**: not established by the inspected evidence.

## 1. Source Map

| Area | Primary source | Relevant symbols | Evidence |
|---|---|---|---|
| Dispatch and dungeon generation | [generate.c](../ref-mangband/src/server/generate.c) | `generate_cave`, `cave_gen`, `town_gen`, `wilderness_gen` | Verified |
| Rooms and tunnels | [generate.c](../ref-mangband/src/server/generate.c) | `room_build`, `build_type1` through `build_type8`, `build_tunnel` | Verified |
| Vault data | [vault.txt](../ref-mangband/lib/edit/vault.txt) | `N`, `X`, `D` records; `build_vault` | Verified |
| Monsters | [monster2.c](../ref-mangband/src/server/monster2.c) | `get_mon_num`, `alloc_monster`, `place_monster_aux` | Verified |
| Objects and traps | [object2.c](../ref-mangband/src/server/object2.c) | `place_object`, `place_gold`, `place_trap`, `drop_near` | Verified |
| Features | [terrain.txt](../ref-mangband/lib/edit/terrain.txt) | feature records and glyphs | Verified |
| Quest/special-level predicates | [cave.c](../ref-mangband/src/server/cave.c) | `is_quest`, `is_quest_level` | Verified |
| Wilderness | [wilderness.c](../ref-mangband/src/server/wilderness.c) | `world_index`, `wilderness_gen`, `wild_add_monster` | Verified |
| Global dimensions and list limits | [defines.h](../ref-mangband/src/common/defines.h) | `MAX_HGT`, `MAX_WID`, `MAX_DEPTH`, `MAX_M_IDX`, `MAX_O_IDX` | Verified |

The checked vault source carries `V:1.5.3`. **Verified**: vault spacing is significant, and the source comments define the layout grammar used below.

## 2. Dungeon Lifecycle

### Entry point and dispatch

**Verified**: `generate_cave(player_type *p_ptr, int Depth, int auto_scum)` is the level-generation entry point. It resets generation globals, clears the allocated cave, sets `monster_level` and `object_level` to `Depth`, resets `rating` and `good_item_flag`, then dispatches as follows:

```text
Depth == 0  -> town_gen()
Depth < 0   -> wilderness_gen(Depth)
Depth > 0   -> cave_gen(Depth)
```

**Verified**: `alloc_dungeon_level()` is performed by the caller in the level entry path before generation; `dealloc_dungeon_level()` removes generated dungeon levels after players leave, except protected cases such as owned houses and special levels.

### Ordinary dungeon flow

**Verified**: `cave_gen()` executes this order:

1. Fill the entire `MAX_HGT` by `MAX_WID` grid with `FEAT_WALL_EXTRA`.
2. Roll for a destroyed level when `Depth > 10`, except quest levels.
3. Initialize a 6 by 18 block reservation grid using 11 by 11 blocks.
4. Attempt up to `DUN_ROOMS` room builds.
5. Replace the outer map boundary with `FEAT_PERM_SOLID`.
6. Shuffle room centers and connect them with a cyclic sequence of tunnels.
7. Try doors at recorded tunnel junctions.
8. Add magma and quartz streamers.
9. Apply destroyed-level edits when selected.
10. Place down stairs, then up stairs.
11. Select the random player starting location.
12. Place sleeping ordinary monsters.
13. Place traps, corridor rubble, room objects, general objects, and gold.

**Inferred**: the numbered order is gameplay-significant because later allocators inspect feature and `CAVE_ROOM` state created by earlier stages.

### Regeneration and scumming

**Verified**: generation can restart for object-list overflow, monster-list overflow, or configured auto-scumming. On restart, dungeon objects and monsters for the depth are wiped, lists may be compacted, and generation is retried up to 100 auto-scum attempts.

**Verified**: auto-scum rejects dungeon levels according to the feeling index, with stricter acceptable-feeling thresholds at depths 5, 10, 20, and 40.

## 3. Room Grid and Room Builders

### Block model

**Verified**: blocks are `BLOCK_HGT = 11` and `BLOCK_WID = 11`. With `MAX_HGT = 66` and `MAX_WID = 198`, the ordinary generator has 6 rows and 18 columns, or 108 reservable blocks.

**Verified**: each room type declares required block extents and a minimum depth. A build fails when the requested blocks are out of bounds or already reserved. Successful rooms record their center and reserve every covered block.

| Type | Name | Block footprint | Minimum depth | Evidence |
|---:|---|---|---:|---|
| 1 | Simple rectangular | 1 x 3 | 1 | Verified |
| 2 | Overlapping rectangles | 1 x 3 | 1 | Verified |
| 3 | Crossed room | 1 x 3 | 3 | Verified |
| 4 | Large room with features | 1 x 3 | 3 | Verified |
| 5 | Monster nest | 1 x 3 | 5 | Verified |
| 6 | Monster pit | 1 x 3 | 5 | Verified |
| 7 | Lesser vault | 2 x 3 | 5 | Verified |
| 8 | Greater vault | 4 x 6 | 10 | Verified |

### Type 1: simple room

**Verified**: the room is lit when `Depth <= randint1(25)`. Its vertical half extents are independently selected as north `1..4` and south `1..3`; its horizontal half extents are independently selected as west `1..11` and east `1..11`. The floor is marked `CAVE_ROOM`, and an outer wall is placed.

**Verified**: a one-in-20 roll replaces a grid pattern with inner walls for a pillar room. Otherwise, a one-in-50 roll creates a ragged inner-wall edge.

### Type 2: overlapping room

**Verified**: two independently sized rectangles overlap around the same center. The first uses vertical offsets `1..4` and `1..3`, horizontal offsets `1..11` and `1..10`; the second uses vertical offsets `1..3` and `1..4`, horizontal offsets `1..10` and `1..11`.

### Type 3: cross room

**Verified**: the vertical arm has half-height `3..4`, the horizontal arm has half-width `3..11`, and both arms have a fixed half-width/half-height of one for the crossing arm. One of four feature variants is selected: a solid center pillar, an inner treasure vault, a mostly open cross with occasional pinches/secret doors, or a center plus/pillar variant.

**Verified**: the type-3 treasure variant places one special-origin object, 3..4 nearby sleeping monster attempts, and 2..4 traps in a central region.

### Type 4: large room

**Verified**: the outer room spans offsets `-4..4` vertically and `-11..11` horizontally, with an inner wall offset by two grids. One of five variants is selected: a secret-door room with one monster; a locked treasure room; inner pillars and optional rooms; a checkerboard maze; or four small inner rooms.

**Verified**: the locked treasure variant has an 80% object versus 20% random stairs decision, 3..5 guarding monsters, and 3..5 traps. The maze variant places 1..3 monsters on each side, 1..3 traps on each side, and three nearby object/gold attempts. The four-room variant places 3..4 central objects/gold attempts and four groups of 1..4 monster attempts.

### Type 5: monster nest

**Verified**: nests use a large room with an inner room and one secret door. The nest family roll is `randint1(Depth)`: values below 30 select jelly, 30..49 animal, and 50 or higher undead. The allocation hook rejects unsuitable families, uniques, and reproducing monsters according to the helper predicates.

**Verified**: the generator samples 64 candidate races at `Depth + 10`; any failed sample aborts the nest. Otherwise it fills the inner 5 by 19 region with sampled races, adds 10 to `rating`, and may set `good_item_flag` at depths through 40.

### Type 6: monster pit

**Verified**: pits use the same large/inner room shape and one secret door. The family roll is `randint1(Depth)`: orc below 20, troll 20..39, giant 40..59, dragon 60..79, and demon otherwise. Dragon pits select one of five single breath types or a multi-hued mask.

**Verified**: the generator samples 16 races at `Depth + 10`, sorts by race level, retains every other entry to obtain eight races, and places them in a fixed symmetric pit pattern. Pits add 10 to `rating` and may set `good_item_flag` at depths through 40.

## 4. Tunnel Generation and Connectivity

**Verified**: room centers are randomly shuffled. The generator starts at the last center and calls `build_tunnel()` for each center in shuffled order, connecting the current center to the previous one. The final connection closes the sequence into a cycle.

**Verified**: `build_tunnel()` starts with a direction toward its target. On each step there is a 30% direction-correction roll; within that event there is a 10% random-direction roll. The tunnel retries direction choices that would leave the map.

**Verified**: ordinary granite is carved through. Permanent boundaries, permanent vault edges, and solid room walls are avoided. Outer room walls are pierced only when the next grid is legal; piercing locations are recorded, and adjacent outer-wall grids are made solid to prevent adjacent entrances.

**Verified**: tunnel grids and wall piercings are queued to avoid placing doors on an intersecting corridor or reusing an entrance. A 25% roll controls a door at a room entrance piercing. A loop guard stops after 2,000 iterations.

**Verified**: after tunnels are carved, every recorded junction is checked in four neighboring positions. A candidate must not be a room grid, must not be rock, and must have a possible doorway; then a 90% roll places a random door.

**Observed**: the source comments explicitly acknowledge that a tunnel can leave and re-enter one room, and that a set of rooms can remain connected only within the set. The cyclic connection strategy reduces but does not formally eliminate disconnected-room or self-loop anomalies.

**Unresolved**: the build is single-cell wide in the inspected default build; the conditional `WIDE_CORRIDORS` path exists but its enabled-build status is not established here.

## 5. Doors and Stairs

### Door states

**Verified**: `place_random_door()` selects from this 1,000-point distribution:

| Roll | Result |
|---:|---|
| 0..299 | Open door |
| 300..399 | Broken door |
| 400..599 | Secret door |
| 600..899 | Closed door |
| 900..998 | Locked door, strength 1..7 |
| 999 | Stuck door, strength 8..15 |

**Verified**: room-specific treasure rooms use secret doors and locked doors directly; tunnel junctions use the random-door distribution.

**Unresolved**: the exact parity contract for door strength, bashing, opening, and conversion of secret/locked/stuck states belongs to movement code and is not fully specified by generation alone.

### Stairs

**Verified**: ordinary dungeon generation places 3..4 down stairs and 1..2 up stairs, with up to 3,000 random location attempts per stair. Candidate locations must be naked floor and initially require three adjacent walls; the wall requirement is reduced after failed batches.

**Verified**: town stairs are down stairs. Quest levels and the maximum reachable depth use up stairs regardless of the requested feature. Ordinary up/down stair coordinates are saved as level-entry locations.

**Verified**: random stairs used inside a room follow `place_random_stairs()`; town chooses down, quest/max depth chooses up, and ordinary dungeon choices are randomized.

## 6. Vaults

### Selection and placement

**Verified**: type 7 and type 8 rooms randomly scan `v_info[]` until a vault of the requested type is selected. Type 7 is depth-gated at 5 and type 8 at 10 by the room table. The selected vault adds its `rat` value to `rating` and may set `good_item_flag`.

**Verified**: the room builder supplies the vault dimensions to `build_vault()`; the room block reservation provides the enclosing placement bounds. The source data documents maximum nominal sizes of 33x22 for lesser vaults and 66x44 for greater vaults.

### File grammar

**Verified**: `vault.txt` uses `N:<serial>:<name>`, `X:<type>:<rating>:<rows>:<columns>`, and padded `D:` layout lines. The file version is `V:1.5.3`.

| Glyph | Meaning | Evidence |
|---|---|---|
| `%` | Outside/connection area | Verified |
| `#` | Granite | Verified |
| `X` | Impenetrable rock | Verified |
| `*` | Treasure or trap | Verified |
| `+` | Secret door | Verified |
| `^` | Trap | Verified |
| `&` | Monster up to 5 levels OOD | Verified |
| `@` | Monster up to 11 levels OOD | Verified |
| `9` | Monster up to 9 and treasure up to 7 levels OOD | Verified |
| `8` | Monster up to 40 and treasure up to 20 levels OOD | Verified |
| `,` | Monster up to 3 and/or treasure up to 7 levels OOD | Verified |

**Verified**: `build_vault()` interprets layout characters, marks vault grids with room/icky state, and invokes monster, object, gold, trap, and door placement helpers for encoded content. The exact overwrite behavior for every glyph is implemented in `build_vault()` and should be treated as authoritative over prose summaries.

**Unresolved**: a complete per-vault inventory of all serials and ratings was not reproduced here; the raw file is the source of truth and should be parsed before statistical claims about individual vault frequency.

## 7. Traps and Objects

### General allocation

**Verified**: `alloc_object()` repeatedly samples random naked floor until it finds a grid matching `ALLOC_SET_CORR`, `ALLOC_SET_ROOM`, or `ALLOC_SET_BOTH`, then dispatches to rubble, trap, gold, or object placement.

**Verified**: ordinary generation requests `randint1(k)` traps and corridor rubble, where `k = clamp(Depth / 3, 2, 10)` using integer division.

**Verified**: `place_trap()` requires an in-bounds naked grid and changes its feature to `FEAT_INVIS`. Trap selection is deferred to `pick_trap()` when the invisible trap is instantiated.

**Verified**: ordinary generation requests `randnor(9, 3)` room objects, `randnor(3, 3)` general objects, and `randnor(3, 3)` gold attempts. Negative normal-distribution results are possible at the call boundary and must be checked against the exact runtime implementation before assuming they become zero.

**Verified**: `place_object()` selects an object kind using the object allocation table, applies magic, and may create artifacts. `place_gold()` selects a gold kind from `object_level`, then computes its amount from the selected base value and random terms.

**Verified**: vault helper placement uses clean floor rather than merely naked floor. `vault_objects()` makes up to 11 local attempts per requested item and uses a 75% object versus 25% gold decision. Vault trap helpers make up to six local location attempts per requested trap.

**Verified**: object generation has separate good/great and artifact paths in `apply_magic()`, but ordinary `cave_gen()` allocation requests are not marked good or great. Vault glyph semantics can explicitly raise `object_level` and `monster_level` above the dungeon depth.

**Unresolved**: exact object-kind weights, rarity behavior, artifact preservation, and quality distributions require joining `object2.c`, `object.txt`, artifact and ego data, and the full allocation-table initialization path. This document does not infer those distributions from item counts.

### 7.1 Object Quality Generation

#### Good and great item chances

**Verified**: `apply_magic()` computes a good-item percentage as
`min(lev + 10, 75)`. The great-item percentage is then
`min(goodChance / 2, 20)`. These are percentage inputs to the source's
`magik()` rolls; they are not independent unconditional rolls because the
great roll is evaluated only after the item is classified as good.

| Object level | Good chance | Great chance |
|---:|---:|---:|
| 1 | 11% | 5% |
| 20 | 30% | 15% |
| 65 and above | 75% | 20% |

**Derived**: this makes equipment quality depth-sensitive, with caps at 75%
for good and 20% for great.

#### Out-of-depth object generation

**Verified**: `get_obj_num()` has a `1 / GREAT_OBJ` opportunity when its input
level is positive. On success it replaces the effective allocation level with:

```text
1 + (level * MAX_DEPTH / randint1(MAX_DEPTH))
```

The result is then used to select from the object allocation table. This is an
allocation-depth boost, not a guarantee that an object above the dungeon depth
will be selected.

#### Artifact creation stages

**Verified**: `place_object()` first has a special-artifact path: it calls
`make_artifact_special()` with a 1-in-1000 roll for ordinary objects and a
1-in-10 roll when `good` is true. If that path does not succeed, it selects a
base kind and then calls `apply_magic()`.

**Verified**: within `apply_magic()`, excellent/great power normally allows one
normal-artifact attempt, while `great == TRUE` forces four attempts. Normal
artifact creation is handled by `make_artifact()`, which may also attempt an
optional random artifact under `RANDART` and `cfg_random_artifacts`. Ego-item
application occurs later in the same function after base magic and artifact
handling. The effective order is therefore conditional rather than a single
universal linear chain:

```text
special artifact attempt in place_object()
-> base object selection
-> good/great power in apply_magic()
-> normal artifact or optional randart attempt
-> base-type magic
-> ego-item application
```

**Verified**: `acquirement()` calls `place_object()` with `good = TRUE` and
`great = TRUE`, so its ordinary path uses the four-attempt forced-great
artifact opportunity unless the special-artifact path succeeds first.

#### Rating contributions from objects

**Verified**: artifact application adds 10 rating, adds another 10 when the
artifact cost exceeds 50,000, and sets `good_item_flag = TRUE`. `place_object()`
also adds the object's native-level delta to rating when a non-cursed,
non-broken object is out of depth and the rating was unchanged by that object.

**Verified**: dragon scale mail adds 30 rating in its armor-magic path. Ego
items add their configured `rating` value in `apply_magic()`.

**Unresolved**: the supplied source excerpt does not establish a separate
generic rating increment for every special artifact or every good object; the
artifact, out-of-depth, dragon-scale-mail, and ego paths above are the verified
contributors.

#### Enchantment distribution

**Verified**: enchantment helpers call `m_bonus(max, level)`. `m_bonus()` moves
the mean bonus toward `max` as level increases, chooses a standard deviation
of approximately one quarter of `max`, samples with `randnor()`, and clamps
the result to `[0, max]`.

**Derived**: the distribution is bounded and depth-biased rather than uniform;
simple uniform bonus generation will not preserve the source distribution.

## 8. Monster Placement

**Verified**: ordinary generation requests `MIN_M_ALLOC_LEVEL + randint1(8) + k` sleeping monster attempts. With the source constant `MIN_M_ALLOC_LEVEL` and `k` formula, the count is a bounded attempt count, not a guaranteed live-monster count: placement can fail after 50 location trials or because allocation returns no legal race.

**Derived**: assuming the conventional `MIN_M_ALLOC_LEVEL = 14`, the request mean before placement failures is `14 + 4.5 + k = 18.5 + k`; it ranges from 20.5 at depths 1..8 through 28.5 at depths 30 and deeper. This is a request distribution, not an observed population average.

**Verified**: `alloc_monster()` samples a naked floor, requires minimum distance from every player on the depth, and calls `place_monster()` with sleeping and group-enabled placement. `place_monster()` obtains a race through `get_mon_num(monster_level)`.

**Verified**: `get_mon_num()` uses the prepared allocation table, depth limits, rarity weights, out-of-depth boosts, `FORCE_DEPTH`, town exclusions, and wilderness unique exclusions. It can perform one or two additional weighted selections and retains the harder race by depth comparison.

**Verified**: normal placement handles friends, escorts, groups, unique limits, reproducer limits, and race counters in `place_monster_aux()` and its callers. Nest and pit filters prepare a temporary allocation hook and restore the normal allocation table afterward.

**Verified**: nest samples are selected from `Depth + 10`; pit samples are selected from `Depth + 10` and sorted before the symmetric layout is filled. Vault-encoded monster glyphs have their own out-of-depth caps.

**Unresolved**: the exact probability of a placed group, escort composition, and final live count depends on the full `place_monster_aux()` implementation, current list capacity, occupied grids, and allocation table contents; no single closed-form population number is source-justified.

### 8.1 Additional Monster Placement Rules

#### Out-of-depth monster generation

**Verified**: `get_mon_num()` performs two separate `1 / NASTY_MON` rolls when
the requested level is positive. Each successful roll adds
`min(level / 4 + 2, 5)` to the effective selection level. The second roll uses
the level after the first boost, so both boosts may apply.

#### Unique and force-depth restrictions

**Verified**: `place_monster_one()` rejects a unique when
`allow_unique_level()` is false or when `cur_num >= max_num`. The helper only
allows a unique when at least one player on the depth has not recorded that
unique as killed. `get_mon_num()` additionally excludes uniques for negative
wilderness allocation levels.

**Verified**: a `FORCE_DEPTH` race is rejected when its native level is deeper
than the requested allocation depth. The source comments note a disabled
unique check in `get_mon_num()`; the effective unique enforcement occurs in
placement, so allocation selection and placement rejection must remain
distinct in a parity implementation.

#### Groups and escorts

**Verified**: `place_monster_aux()` places the leader first. Only when its
`grp` argument is true does it process `FRIENDS` and `ESCORT` behavior.
`place_monster_group()` starts with `randint1(13)`, adjusts group size down for
monsters deeper than the dungeon and up for easier monsters, clamps the result
to at least one and at most `GROUP_MAX = 32`, and expands breadth-first through
adjacent grids.

**Derived**: holding other conditions equal, the group-size adjustment biases
weaker monsters toward larger groups and stronger monsters toward smaller
groups. It is not a fixed group-size table.

**Verified**: escort selection requires the same display symbol, level no
greater than the leader, a non-unique race, and a different race index. The
source tries up to 50 scattered locations. Escort monsters can trigger group
placement when the escort has `FRIENDS` or the leader has `ESCORTS`.

#### Reproducing monsters

**Verified**: `multiply_monster()` refuses unique races, makes up to 18
adjacent scatter attempts, copies the reproducer's race, and calls placement
with `grp = FALSE`. Thus reproduction does not initiate friend groups or
escorts through that call.

## 9. Level Feelings and Rating

**Verified**: `generate_cave()` starts each attempt with `rating = 0` and `good_item_flag = FALSE`. Rating is increased by special vault ratings and by monster pits/nests; other increases occur in object-generation code.

**Verified**: rating maps to feelings as follows, before special-feeling and town overrides:

| Rating | Feeling index |
|---:|---:|
| `>100` | 2 |
| `>80` | 3 |
| `>60` | 4 |
| `>40` | 5 |
| `>30` | 6 |
| `>20` | 7 |
| `>10` | 8 |
| `>0` | 9 |
| `<=0` | 10 |

**Verified**: `good_item_flag` overrides the result with feeling 1. Towns have feeling 0. In non-Ironman mode, a player cannot receive a new feeling until 1,000 game turns have passed since the previous one; this also disables auto-scumming for that generation attempt.

**Observed**: source comments call low rating “boring” and describe special feelings as rare/good-item signals. The numeric feeling index is the verified contract; the client-facing wording is outside this generator.

### 9.1 Additional Level-Feeling Contributors

**Verified**: when `place_monster_one()` accepts a monster deeper than the
current dungeon depth, it adds the depth difference to `rating` for an ordinary
monster and twice the difference for a unique:

```text
ordinary: rating += monster_level - dungeon_level
unique:   rating += 2 * (monster_level - dungeon_level)
```

**Verified**: monster pits and nests each add 10 rating. Both can additionally
set `good_item_flag` through depth-dependent random checks at depths through
40. Vaults add their data-file rating, artifacts add their artifact rating
increments, dragon scale mail adds 30, and ego items add their configured
rating.

**Observed**: the resulting feeling combines treasure-quality signals with
out-of-depth danger and special-room signals; it is not a treasure-only score.

## 10. Special Levels and Town

**Verified**: town generation is a separate `town_gen()` path and does not use ordinary dungeon rooms, tunnels, vault allocation, monster allocation, or level feelings. Town layout is retained from seeded town generation and is day/night illuminated by world processing.

**Verified**: quest levels are identified by `is_quest(Depth)` in generation and by player-specific `is_quest_level()` checks elsewhere. Destroyed-level generation is explicitly disabled for quest levels, and quest/max-depth stair rules force up stairs.

**Verified**: special static levels are represented by a configured list of depths loaded into `special_levels[]`; `check_special_level()` is used to prevent ordinary deallocation and to suppress ordinary monster generation in some allocation paths.

**Unresolved**: the inspected generation path does not itself enumerate the contents or layouts of every configured static/special level. Those layouts and configuration files must be joined before claiming parity for themed or hand-authored levels.

## 11. Wilderness Interaction

**Verified**: negative depths use a ring-indexed world coordinate system. The town is depth zero; `world_index()` maps `(world_x, world_y)` to negative depths, and `wild_info[]` stores radius, type, and flags.

**Verified**: `wilderness_gen()` creates boundary permanent walls, calls the seeded wilderness generator, applies day/night lighting, and initially adds residents with `wild_add_monster()`. Terrain type controls the monster hook, and monster placement still flows through the shared monster allocation and placement functions.

**Verified**: wilderness generation uses seeded terrain composition, hotspots, dwellings, crop/food placement, terrain bleeding with neighbors, and day/night behavior. Wilderness levels are not ordinary dungeon `cave_gen()` levels.

**Verified**: dungeon-level monster allocation rejects unique races for negative levels, while wilderness hooks select biome-appropriate races. The shared object and monster lists therefore cross the dungeon/wilderness boundary even though generation algorithms differ.

**Unresolved**: exact wilderness biome frequencies and all dwelling contents are outside ordinary dungeon parity and require separate statistical treatment.

### 11.1 Additional Wilderness Findings

#### Terrain families and monster levels

**Verified**: the wilderness terrain families are town, grassland, forest,
dense forest, swamp, lake, and wasteland. Ordinary wilderness terrain types
are selected by `determine_wilderness_type()`; town is depth zero and is not
selected by that ordinary terrain roll.

**Verified**: `init_terrain()` assigns these base monster levels, before the
shared monster allocation logic applies its own rules:

| Terrain | Base monster level |
|---|---:|
| Grassland | `1 + radius / 2` |
| Lake | `1 + radius / 2` |
| Forest | `5 + radius / 2` |
| Swamp | `12 + radius / 2` |
| Dense forest | `15 + radius / 2` |
| Wasteland | `20 + radius / 2` |

**Verified**: dense forest adds 10 to the base monster level at night, while
swamp doubles its base monster level at night.

#### Determinism and terrain bleeding

**Verified**: wilderness generation temporarily sets `Rand_quick = TRUE` and
seeds the simple RNG from `seed_town` and the wilderness depth, with additional
derived seeds for terrain reseeding and neighbor bleed operations. It restores
the previous RNG mode afterward.

**Derived**: for a fixed source seed, depth, world-coordinate state, build
options, and unchanged RNG consumption, wilderness generation is intended to
be repeatable. This is a conditional determinism claim, not proof that every
server history produces identical output.

**Verified**: `bleed_with_neighbors()` and `wild_bleed_level()` copy seeded
terrain composition across selected neighboring edges using generated bleed
maps. **Observed**: this softens biome boundaries rather than treating each
wilderness level as an entirely isolated terrain patch.

## 12. Generation Constants

| Constant | Value | Meaning | Evidence |
|---|---:|---|---|
| `DUN_ROOMS` | 50 | Room attempts | Verified |
| `DUN_UNUSUAL` | 200 | Depth/chance threshold | Verified |
| `DUN_DEST` | 15 | Destroyed-level one-in-N roll | Verified |
| `DUN_TUN_RND` | 10% | Random direction within correction event | Verified |
| `DUN_TUN_CHG` | 30% | Direction correction event | Verified |
| `DUN_TUN_CON` | 15% | Extra tunneling/continuation roll | Verified |
| `DUN_TUN_PEN` | 25% | Door at room entrance | Verified |
| `DUN_TUN_JCT` | 90% | Door at junction candidate | Verified |
| `DUN_STR_DEN` | 5 | Streamer density | Verified |
| `DUN_STR_RNG` | 2 | Streamer spread | Verified |
| `DUN_STR_MAG` / `DUN_STR_QUA` | 3 / 2 | Magma/quartz streamers | Verified |
| `DUN_STR_MC` / `DUN_STR_QC` | 90 / 40 | Treasure roll denominators | Verified |
| `DUN_AMT_ROOM` | 9 | Mean room object requests | Verified |
| `DUN_AMT_ITEM` / `DUN_AMT_GOLD` | 3 / 3 | Mean general object/gold requests | Verified |
| `BLOCK_HGT` / `BLOCK_WID` | 11 / 11 | Generation block size | Verified |
| `MAX_HGT` / `MAX_WID` | 66 / 198 | Dungeon grid dimensions | Verified |
| `MAX_DEPTH` | 128 | Maximum depth bound used by generation | Verified |
| `MAX_M_IDX` / `MAX_O_IDX` | 32768 / 32768 | Monster/object list capacity | Verified |
| Tunnel loop guard | 2000 | Maximum tunnel iterations | Verified |
| Stair attempt limit | 3000 | Attempts per stair before relaxing wall count | Verified |
| Monster location attempts | 50 | Attempts per ordinary monster allocation | Verified |

## 13. Statistics and Derived Expectations

**Derived**: the room grid contains 108 reservable blocks, while 50 room attempts are made. Because room footprints consume 3, 6, or 24 blocks and attempts can collide, 50 is an attempt ceiling, not an average room count.

**Derived**: before collisions and depth gates, the unusual-room roll is `Depth / 200` for depths below 200. A second independent `Depth / 200` roll is required for the very unusual group containing vaults, nests, and pits. The actual vault frequency is lower and depth-dependent because room footprints, vault type availability, and failed placement matter.

**Derived**: ordinary stairs request 3..4 down plus 1..2 up, so the request range is 4..6 stairs per generated dungeon level. Quest/max-depth rules can change the feature type and saved entry coordinate.

**Derived**: ordinary trap and rubble request counts are uniform over `1..clamp(Depth / 3, 2, 10)`. At depths 1..8, the request range is 1..2; at depths 30 and above it is 1..10.

**Unresolved**: “average rooms”, “vault frequency”, final monster population, and final object population cannot be responsibly reported as measured statistics without running a fixed-seed compatible generator across depths. The formulas above are source-derived request statistics only.

## 13.1 Parity-Critical Behaviors

**Derived**: the following behaviors have unusually high parity sensitivity
because they alter map topology, population, quality, or level feeling:

- Depth-dependent unusual-room selection and destroyed-level generation.
- Cyclic room-center connectivity and tunnel direction correction.
- Vault glyph semantics and out-of-depth object/monster allocation.
- Good/great item rolls, artifact attempts, and enchantment distributions.
- Monster out-of-depth boosts, unique restrictions, groups, escorts, and reproduction.
- Nest/pit family selection and rating contributions.
- Rating calculations and the `good_item_flag` feeling override.
- Seeded wilderness generation, terrain bleeding, and day/night monster levels.

This is a research prioritization, not a claim that each behavior is already
represented in IronHell.

## 14. IronHell Relevance

Status meanings here are parity research status, not implementation quality: `Unknown` means no sufficient repository evidence; `Partial` means some source or data coverage exists; `High` means substantial source-shaped documentation or definitions exist but runtime parity is not proven; `Complete` requires verified executable parity, and is not claimed for these generation systems.

| Subsystem | IronHell status | Repository evidence | Evidence |
|---|---|---|---|
| Dungeon lifecycle | Unknown | Feature parity lists dungeon generation as unresolved | Verified |
| Room builders | Partial | World Compendium prose; no dedicated runtime room catalog | Verified |
| Tunnel/connectivity | Partial | World Compendium prose; no executable parity evidence | Verified |
| Vault data | Partial | `data/definitions/vaults.json` exists, but is a selected/partial catalog | Verified |
| Vault interpreter | Unknown | No verified IronHell generator path found | Observed |
| Doors | Partial | Prose exists; no dedicated door-state/generation catalog | Verified |
| Stairs/transitions | Partial | Prose and transition action exist; generation policy remains unresolved | Verified |
| Traps | Research only / Partial | Trap research and catalogs exist, but placement/trigger parity is open | Verified |
| Object placement | Partial | Item catalogs and artifact/ego data exist; generation policy is not complete | Verified |
| Monster placement | Partial | Monster definitions are substantial; allocation policy is not complete | Verified |
| Level feelings | Unknown | No verified IronHell rating/feeling runtime found | Observed |
| Town generation | Unknown | Existing parity audit marks town layout unresolved | Verified |
| Wilderness generation | Research only / Partial | World Compendium documents behavior; terrain catalog coverage is incomplete | Verified |
| Special/quest levels | Research only | Quest definitions and static-level contents remain missing | Verified |

### 14.1 Monster Definition Mapping (IronHell Research)

**Observed**: the current IronHell monster definitions contain fields that are
structurally compatible with several MAngband generation and sensing flags:

| MAngband flag | Observed IronHell field |
|---|---|
| `RF1_UNIQUE` | `spawn_policy.unique` |
| `RF1_FORCE_MAXHP` | `spawn_policy.force_max_hp` |
| `RF1_FORCE_SLEEP` | `spawn_policy.force_sleep` |
| `RF1_ESCORT` | `spawn_policy.escort` |
| `RF2_MULTIPLY` | `capabilities.multiply` |
| `RF2_EMPTY_MIND` | `telepathy_profile.empty_mind` |
| `RF2_WEIRD_MIND` | `telepathy_profile.weird_mind` |

**Unresolved**: these field correspondences demonstrate repository evidence,
not complete behavioral parity. A one-to-one mapping for all `RF1` through
`RF7` flags, including `FRIENDS`, `ESCORTS`, `FORCE_DEPTH`, and allocation
policy interactions, requires a full dataset and runtime audit.

## 15. Unresolved Questions and Parity Risks

1. **Unresolved**: What exact source build options were enabled for the reference server, especially `WIDE_CORRIDORS`, `RANDART`, and debug or compatibility flags?
2. **Unresolved**: What is the exact `MIN_M_ALLOC_LEVEL` value in the selected build configuration, and are any runtime configuration overrides applied?
3. **Unresolved**: Does the full `build_vault()` implementation overwrite every pre-existing grid in the same way for `%`, `#`, `X`, `*`, and encoded glyphs?
4. **Unresolved**: What are the complete `vault.txt` serial count, rating distribution, and type-7/type-8 availability in the loaded raw table?
5. **Unresolved**: What exact object allocation probabilities and quality modifiers result from `object.txt`, artifacts, ego items, and allocation table initialization?
6. **Unresolved**: How do negative values from `randnor()` behave at the allocation call sites in the shipped runtime?
7. **Unresolved**: What final monster-count distribution results after failed placement, groups, escorts, unique limits, and list compaction?
8. **Unresolved**: Which configured special depths have hand-authored layouts, and how do those layouts interact with ordinary generation and persistence?
9. **Unresolved**: What client wording corresponds to feeling indexes 1..10, and which player-visible systems consume rating beyond auto-scumming?
10. **Unresolved**: Are all wilderness and dungeon generation calls made with a stable seed protocol across server restart and level regeneration?
11. **Unresolved**: Which IronHell source/data files are intended to become the authoritative parity boundary for rooms, features, doors, stairs, traps, and allocation policies?
12. **Unresolved**: Can fixed-seed differential tests be created without preserving legacy global RNG consumption order, or is behavioral parity sufficient for the refactor?

## 16. Research Boundary

**Verified**: this compendium changes documentation only. It does not claim an IronHell implementation, schema, migration, or runtime behavior. The original MAngband source and edit files remain the authority for any future parity implementation, with unresolved items above requiring explicit decisions before refactoring.
