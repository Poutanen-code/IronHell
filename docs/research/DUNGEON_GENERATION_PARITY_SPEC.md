# Dungeon Generation Parity Specification

Behavioral contract for future IronHell dungeon-generation work. This document
specifies observable behavior, not implementation structure.

## Evidence and Status

- **Verified**: directly established by MAngband 1.5.3 source/data.
- **Derived**: calculated from verified behavior.
- **Observed**: directly observed in the current IronHell repository.
- **Inferred**: probable interpretation not directly proven.
- **Unresolved**: insufficient evidence or configuration-dependent behavior.

`Required` means ordinary parity behavior. `Conditional` means behavior depends
on configuration, level category, or source build. `Unresolved` must not be
silently implemented as a guessed rule.

**Source notation:** MAngband provenance uses repository-relative paths such as
`ref-mangband/src/server/generate.c :: cave_gen()` and
`ref-mangband/src/server/mdefines.h :: NASTY_MON`. Existing requirements that
use a shortened source filename refer to the same repository-relative source;
new requirements must use the full form.

## 1. Dungeon Lifecycle

### DG-LIFE-001 — Dispatch by depth

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: generate_cave()`.

**Given:** A level depth is requested.

**When:** Generation dispatch occurs.

**Then:** Depth zero uses `town_gen()`, negative depth uses `wilderness_gen()`,
and positive depth uses `cave_gen()`.

**Random behavior:** None in the dispatch branch.

**Parity notes:** The three paths are behaviorally distinct.

**Suggested test level:** Unit / Golden-seed.

### DG-LIFE-002 — Ordinary dungeon generation order

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`.

**Given:** A positive, non-static dungeon depth.

**When:** The level is generated.

**Then:** The observable order is: fill granite; roll destroyed level; build
rooms; set permanent boundaries; shuffle centers; build tunnels; place junction
doors; build streamers; process destruction; place stairs; select player start;
place monsters; request traps; request rubble; request room objects; request
general objects; request gold.

**Random behavior:** RNG consumption is order-sensitive.

**Parity notes:** Reordering stages can change legal placement and rating.

**Suggested test level:** Golden-seed.

### DG-LIFE-003 — Regeneration and autoscum

**Parity status:** Conditional

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: generate_cave()`.

**Given:** Object/monster list overflow or enabled dungeon autoscum.

**When:** A generated level is rejected.

**Then:** Depth objects and monsters are wiped and generation retries, with
feeling thresholds controlling autoscum acceptance.

**Random behavior:** A rejected attempt consumes RNG before retry; exact retry
stream continuity is parity-significant.

**Parity notes:** Town and wilderness are not autoscummed by this path.

**Suggested test level:** Integration / Golden-seed.

## 2. Room Generation

### DG-ROOM-001 — Room block grid

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`, `room[]`; constants `BLOCK_HGT = 11`,
`BLOCK_WID = 11`, `MAX_HGT = 66`, `MAX_WID = 198`.

**Given:** An ordinary positive dungeon level.

**When:** Room allocation begins.

**Then:** The reservation grid is 6 rows by 18 columns. A room reserves its
whole declared block footprint only after bounds and vacancy checks succeed.

**Random behavior:** Block coordinates are random per attempt.

**Parity notes:** Failed room attempts do not reserve blocks or record centers.

**Suggested test level:** Unit / Golden-seed.

### DG-ROOM-002 — Room attempt count and unusual selection

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`; `DUN_ROOMS = 50`, `DUN_UNUSUAL = 200`.

**Given:** A room attempt index from 0 through 49.

**When:** The room type is selected.

**Then:** The generator first rolls `randint0(200) < Depth`. On success it
rolls a second `randint0(200) < Depth` for the very-unusual group. The first
successful eligible specialized branch is attempted according to source order,
then the normal room fallback is attempted.

**Random behavior:** Integer comparison, not floating-point probability.

**Parity notes:** Depths above 200 make the comparison always true, subject to
room availability and footprint failure.

**Suggested test level:** Golden-seed.

### DG-ROOM-003 — Room family footprints

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: room[]` and `room_build()`.

**Given:** A room request.

**When:** The requested family is attempted.

**Then:** Minimum depths and block footprints are:

| Family | Minimum depth | Footprint |
|---|---:|---:|
| Simple | 1 | 1x3 |
| Overlapping | 1 | 1x3 |
| Cross | 3 | 1x3 |
| Large | 3 | 1x3 |
| Nest | 5 | 1x3 |
| Pit | 5 | 1x3 |
| Lesser vault | 5 | 2x3 |
| Greater vault | 10 | 4x6 |

**Random behavior:** Family-specific geometry consumes its own random rolls.

**Parity notes:** A family can be selected but fail because of depth, bounds, or
occupied blocks.

**Suggested test level:** Unit / Golden-seed.

### DG-ROOM-004 — Normal and overlapping rooms

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_type1()`, `build_type2()`.

**Given:** A successful type-1 or type-2 build.

**When:** Geometry is created.

**Then:** Type 1 creates a rectangular floor and outer wall with vertical
half-extents `1..4` and `1..3`, horizontal half-extents `1..11` and `1..11`.
Type 2 creates two independently rolled overlapping rectangles.

**Random behavior:** Type-1 lighting uses `Depth <= randint1(25)`; type-1 pillar
is 1-in-20, otherwise ragged edge is 1-in-50.

**Parity notes:** Floors receive room state through the room helpers.

**Suggested test level:** Golden-seed.

### DG-ROOM-005 — Cross and large rooms

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_type3()`, `build_type4()`.

**Given:** A successful cross or large-room build.

**When:** The family variant is rolled.

**Then:** Cross rooms use vertical half-height `3..4`, horizontal half-width
`3..11`, and a one-grid crossing arm. Large rooms use offsets `-4..4` and
`-11..11` with inner features.

**Random behavior:** Cross variant selection is one of four; large variant
selection is one of five.

**Parity notes:** Variants can add secret/locked doors, monsters, traps,
objects, stairs, pillars, or maze walls.

**Suggested test level:** Golden-seed.

### DG-ROOM-006 — Room centers and room state

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: room_build()`, `place_floor()`.

**Given:** A successful room build.

**When:** The room is committed.

**Then:** Its center is appended to the center list and all footprint blocks are
reserved. Floor cells created by room helpers receive `CAVE_ROOM`; vault cells
also receive `CAVE_ICKY`.

**Random behavior:** None after successful geometry decisions.

**Parity notes:** Center recording occurs even for nests, pits, and vaults.

**Suggested test level:** Unit.

### DG-ROOM-007 — Type-1 room variants

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: build_type1()`.

**Given:** A successful simple-room build.

**When:** Its internal variant rolls execute.

**Then:** The floor rectangle uses north offset `randint1(4)`, south offset
`randint1(3)`, west offset `randint1(11)`, and east offset `randint1(11)`.
The floor and surrounding room area are written by `place_floor()` and the
outer wall is placed by `place_wall()`. Lighting is enabled when
`Depth <= randint1(25)`. A 1-in-20 pillar branch replaces alternating eligible
floor cells with inner wall. Only if that branch fails is the 1-in-50 ragged-edge
branch tested; the two variants are mutually exclusive.

**Random behavior:** Geometry, lighting, pillar, and ragged-edge rolls occur in
source order.

**Parity notes:** Failed room placement is handled by the enclosing
`room_build()` checks, not by a partial-room rollback.

**Suggested test level:** Golden-seed.

### DG-ROOM-008 — Type-2 overlapping room geometry

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: build_type2()`.

**Given:** A successful overlapping-room build.

**When:** The two rectangles are generated.

**Then:** Rectangle A uses north/south offsets `1..4`/`1..3` and west/east
offsets `1..11`/`1..10`. Rectangle B uses north/south offsets `1..3`/`1..4`
and west/east offsets `1..10`/`1..11`. Both share one center and are written
through the cross-room helper, so their floors overlap and share room state.

**Random behavior:** One lighting decision is shared by both rectangles:
`Depth <= randint1(25)`.

**Parity notes:** This is one reserved room footprint and one recorded center,
not two independently reserved rooms.

**Suggested test level:** Golden-seed.

### DG-ROOM-009 — Type-3 cross variants

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: build_type3()`.

**Given:** A successful cross-room build.

**When:** The `randint0(4)` variant is selected.

**Then:** Variant 0 leaves the base cross structure. Variant 1 fills the center
crossing with inner wall. Variant 2 builds an inner treasure vault, places one
secret door, one special object, 3..4 nearby monster attempts, and 2..4 nearby
trap attempts. Variant 3 may pinch the cross shut, optionally add four secret
doors, add a central plus of inner walls, or add a central pillar through its
nested 1-in-3 decisions.

**Random behavior:** Variant-3 nested decisions are evaluated in source order.

**Parity notes:** Monster, object, trap, and door attempts may fail without
invalidating the room.

**Suggested test level:** Golden-seed.

### DG-ROOM-010 — Type-4 large-room variants

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: build_type4()`.

**Given:** A successful large-room build.

**When:** `randint1(5)` selects the internal variant.

**Then:** Variant 1 adds one secret door and one monster. Variant 2 adds one
secret door, a 3x3 inner room with one locked door, 3..5 monsters, an 80%
object/20% random-stair choice, and 3..5 traps. Variant 3 adds a central 3x3
inner pillar, may add two more pillars, and has a 1-in-3 chance of adding inner
rooms with secret doors, 1..2 monsters per side, and independent 1-in-3 object
attempts. Variant 4 creates a checkerboard inner maze, places 1..3 monsters on
each side, 1..3 traps on each side, and three nearby object/gold attempts.
Variant 5 creates four inner rooms, chooses one of two four-door layouts,
places 2..3 central object/gold attempts, and makes four groups of 1..4
monster attempts.

**Random behavior:** Every listed range and nested branch consumes RNG in source
order.

**Parity notes:** Secret/locked doors, stairs, monsters, objects, and traps are
independent placement effects; failed placement does not invalidate the room.

**Suggested test level:** Golden-seed.

## 3. Room Connectivity and Tunnels

### DG-CONN-001 — Cyclic center connections

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`.

**Given:** A nonempty room center list.

**When:** Connectivity is built.

**Then:** Centers are randomly shuffled, the last center is used as the initial
previous center, and each center is connected to the previous center, closing a
cycle.

**Random behavior:** Center shuffle consumes RNG before tunnel RNG.

**Parity notes:** The source comments acknowledge possible self-reentry and
room subsets that are not globally connected.

**Suggested test level:** Golden-seed.

### DG-TUN-001 — Tunnel direction and carving

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_tunnel()`; `DUN_TUN_RND = 10`,
`DUN_TUN_CHG = 30`, `DUN_TUN_CON = 15`.

**Given:** Two room centers.

**When:** A tunnel is built.

**Then:** The tunnel starts toward its target. Each step has a 30% direction
correction opportunity; within that branch, a 10% random-direction opportunity.
Out-of-bounds choices are corrected. Ordinary granite is carved; permanent and
solid walls are avoided.

**Random behavior:** Direction rolls occur per tunnel step in source order.

**Parity notes:** The loop has a 2,000-iteration guard.

**Suggested test level:** Golden-seed.

### DG-TUN-002 — Room-wall penetration and doors

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_tunnel()`, `try_door()`; `DUN_TUN_PEN = 25`,
`DUN_TUN_JCT = 90`.

**Given:** A tunnel reaches an outer room wall or recorded junction.

**When:** Entrance/junction processing occurs.

**Then:** Legal outer-wall penetrations are recorded; adjacent outer walls are
made solid to prevent adjacent entrances. Entrance door opportunities use 25%.
Junction candidates use `possible_doorway()` and a 90% door roll.

**Random behavior:** Entrance and junction rolls are separate.

**Parity notes:** Corridors are normally one cell wide; `WIDE_CORRIDORS` is
conditional on the source build.

**Suggested test level:** Golden-seed.

## 4. Doors

### DG-DOOR-001 — Random door distribution

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: place_random_door()`.

**Given:** A random door placement opportunity.

**When:** The 0..999 source roll is evaluated.

**Then:** Results are open 0..299, broken 300..399, secret 400..599, closed
600..899, locked 900..998 with strength 1..7, and stuck 999 with strength 8..15.

**Random behavior:** One 1,000-value selection followed by strength rolls where
applicable.

**Parity notes:** This is distinct from vault secret doors and room-specific
locked doors.

**Suggested test level:** Unit / Statistical.

### DG-DOOR-002 — Vault and room doors

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_vault()`, `build_type3()`, `build_type4()`.

**Given:** A vault `+` glyph or room-specific door variant.

**When:** The feature is built.

**Then:** A secret door is placed for `+` and room secret-door calls. Locked
room variants call locked-door placement directly; they do not use the random
junction distribution.

**Random behavior:** The room chooses a door position separately.

**Parity notes:** Door generation categories must remain distinct.

**Suggested test level:** Unit / Golden-seed.

## 5. Stairs

### DG-STAIR-001 — Ordinary stairs

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`, `alloc_stairs()`.

**Given:** An ordinary positive dungeon level.

**When:** Stairs are allocated.

**Then:** The generator requests `rand_range(3,4)` down stairs and
`rand_range(1,2)` up stairs. Each attempts naked floor cells with at least three
adjacent walls, up to 3,000 attempts before reducing the wall requirement.

**Random behavior:** Requested counts are inclusive random ranges.

**Parity notes:** Requested stair count is not guaranteed successful stair count.

**Suggested test level:** Golden-seed.

### DG-STAIR-002 — Town, quest, and bottom stairs

**Parity status:** Conditional

**Evidence:** Verified

**Source:** `generate.c :: alloc_stairs()`, `place_random_stairs()`.

**Given:** Town, quest, or bottom depth.

**When:** A stair is requested.

**Then:** Town uses down stairs. Quest and `MAX_DEPTH - 1` use up stairs.
Ordinary random stairs choose according to the source branch.

**Random behavior:** Ordinary random-stair choice is separate from ordinary
stair counts.

**Parity notes:** Quest detection is player-state-dependent in `is_quest()`.

**Suggested test level:** Integration.

## 6. Vaults

### DG-VAULT-001 — Vault catalog and eligibility

**Parity status:** Required

**Evidence:** Verified

**Source:** `vault.txt`; `init1.c :: parse_v_info()`; `generate.c :: build_type7()`,
`build_type8()`.

**Given:** A positive dungeon depth.

**When:** A vault room is selected.

**Then:** Type 7 requires depth 5; type 8 requires depth 10. The source catalog
contains 149 records: 67 type 7, 75 type 8, and 7 source type 9.

**Random behavior:** A random `v_info[]` record is repeatedly selected until the
requested runtime type matches.

**Parity notes:** Source type 9 is normalized to runtime type 8, with 99-column
and 66-row validation before normalization. Runtime type-9 selection does not
exist.

**Suggested test level:** Unit / Golden-seed.

### DG-VAULT-002 — Vault cells and glyph order

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_vault()`.

**Given:** An eligible vault coordinate and layout cell.

**When:** The layout is interpreted.

**Then:** Every non-space cell first becomes `FEAT_FLOOR` with `CAVE_ROOM |
CAVE_ICKY`. Glyph processing then applies the individual behavior below.

**Random behavior:** Glyph-specific rolls occur in source traversal order.

**Parity notes:** `9`, `8`, and `,` perform sequential attempts on the same cell;
coexistence is not guaranteed.

**Suggested test level:** Golden-seed.

| Glyph | Required behavior |
|---|---|
| `%` | `FEAT_WALL_OUTER` |
| `#` | `FEAT_WALL_INNER` |
| `X` | `FEAT_PERM_INNER` |
| `*` | 75% object, otherwise invisible trap |
| `+` | Secret door |
| `^` | Invisible trap |
| `&` | Sleeping/group-enabled monster at `Depth + 5` |
| `@` | Sleeping/group-enabled monster at `Depth + 11` |
| `9` | Monster at `Depth + 9`, then object at `Depth + 7` |
| `8` | Monster at `Depth + 40`, then good/great object at `Depth + 20` |
| `,` | Independent 50% monster at `Depth + 3` and 50% object at `Depth + 7` |
| space | No cell processing |

## 7. Monster Nests

### DG-NEST-001 — Nest family and candidate sampling

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_type5()`, `vault_aux_jelly()`,
`vault_aux_animal()`, `vault_aux_undead()`.

**Given:** A type-5 room at depth at least 5.

**When:** The nest family is selected.

**Then:** `randint1(Depth)` selects jelly below 30, animal from 30 through 49,
and undead at 50 or higher. A temporary allocation hook is prepared and 64
candidate races are sampled with `get_mon_num(Depth + 10)`.

**Random behavior:** Candidate samples use replacement and ordinary allocation
weights/filters.

**Parity notes:** Any zero candidate aborts the nest. Family hooks reject
uniques; they do not explicitly reject `RF2_MULTIPLY`. Placement uses
`grp = FALSE`, suppressing FRIENDS and ESCORT/ESCORTS expansion.

**Suggested test level:** Golden-seed.

### DG-NEST-002 — Nest predicates

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: vault_aux_jelly()`, `vault_aux_animal()`,
`vault_aux_undead()`.

**Given:** A race considered for a nest.

**When:** The active family hook evaluates it.

**Then:** Jelly requires non-unique symbol `i`, `j`, `m`, or `,`; animal requires
non-unique `RF3_ANIMAL`; undead requires non-unique `RF3_UNDEAD`.

**Random behavior:** No additional family-hook RNG.

**Parity notes:** Rarity remains allocation weight; `FORCE_DEPTH` is enforced by
`get_mon_num()`.

**Suggested test level:** Unit.

## 8. Monster Pits

### DG-PIT-001 — Pit candidate selection

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_type6()`, `vault_aux_orc()`,
`vault_aux_troll()`, `vault_aux_giant()`, `vault_aux_dragon()`,
`vault_aux_demon()`.

**Given:** A type-6 room at depth at least 5.

**When:** The pit family is selected.

**Then:** The family thresholds are orc below 20, troll 20..39, giant 40..59,
dragon 60..79, demon 80 or higher. Six dragon masks are possible: acid,
electric, fire, cold, poison, and multi-hued. Sixteen candidates are sampled at
`Depth + 10`, then sorted by native level; indexes 0,2,4,6,8,10,12,14 become
eight tiers.

**Random behavior:** Candidate selection uses replacement and ordinary rarity
weights, OOD rolls, and `FORCE_DEPTH` filtering.

**Parity notes:** Any failed candidate aborts the pit. Fixed-pattern placement
uses `grp = FALSE`; placement return values are ignored and not retried by the
pit builder.

**Suggested test level:** Golden-seed.

### DG-PIT-002 — Pit predicates

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: vault_aux_orc()`, `vault_aux_troll()`,
`vault_aux_giant()`, `vault_aux_dragon()`, `vault_aux_demon()`.

**Given:** A candidate race.

**When:** The family filter evaluates it.

**Then:** Orc requires non-unique symbol `o`; troll `T`; giant `P`; dragon symbol
`d` or `D` and exact `flags4` equality to the selected breath mask; demon
symbol `U`.

**Random behavior:** Dragon mask selection is a separate six-way roll.

**Parity notes:** Breeder flags are not explicitly filtered.

**Suggested test level:** Unit.

## 9. Monster Allocation

### DG-MON-001 — Allocation table construction

**Parity status:** Required

**Evidence:** Verified

**Source:** `init2.c :: init_alloc()`; `mdefines.h`.

**Given:** A monster race with `rarity != 0` and a non-ghost race index.

**When:** Allocation tables initialize.

**Then:** One entry is created with native `level`, `prob1 = 100 / rarity`,
`prob2 = prob1`, and `prob3 = prob1`, using integer division. Entries are
ordered by native level.

**Random behavior:** None during table construction.

**Parity notes:** Rarity is an integer weight, not a direct percentage.

**Suggested test level:** Unit.

### DG-MON-002 — Allocation filtering and selection

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/monster2.c :: get_mon_num_prep()`;
`get_mon_num()`.

**Given:** A prepared allocation table and requested level.

**When:** A race is selected.

**Then:** `prob2` is either `prob1` or zero after hook filtering. `prob3` is
zeroed for entries over effective level, town-incompatible entries,
`FORCE_DEPTH` violations, or wilderness-negative unique exclusion. The source
sums `prob3` and selects an initial candidate by cumulative weighted scan. It
then uses the same candidate table for comparison selections. A comparison
candidate replaces the current candidate only when its `abs(native_level)` is
strictly smaller than the current candidate's `abs(native_level)`; equality
keeps the current candidate.

**Random behavior:** Selection is with replacement. One `p = randint0(100)` roll
controls both branches: `p < 60` selects a second candidate, then `p < 10`
selects a third candidate. All candidates use the same `prob3` weights. The
comparison uses only absolute native level; requested effective level is not a
separate comparison operand.

**Parity notes:** Allocation eligibility is distinct from placement rejection.

**Suggested test level:** Unit / Golden-seed.

### DG-MON-003 — Monster OOD rolls

**Parity status:** Required

**Evidence:** Verified

**Source:** `monster2.c :: get_mon_num()`; `mdefines.h :: NASTY_MON`.

**Given:** A positive requested allocation level `L`.

**When:** OOD processing occurs.

**Then:** Two independent 1-in-50 rolls are evaluated. Each success adds
`min(L / 4 + 2, 5)` using integer division; the second calculation uses the
level after the first success.

**Random behavior:** Both rolls occur before table filtering.

**Parity notes:** OOD changes allocation level, not guaranteed final race level.

**Suggested test level:** Unit / Golden-seed.

### DG-MON-004 — Placement restrictions

**Parity status:** Required

**Evidence:** Verified

**Source:** `monster2.c :: place_monster_one()`, `allow_unique_level()`.

**Given:** A selected race and target cell.

**When:** Placement occurs.

**Then:** `FORCE_DEPTH` races deeper than the requested depth are rejected by
allocation. Unique placement is rejected when no eligible player remains or
`cur_num >= max_num`. A selected race can therefore fail after allocation.

**Random behavior:** Placement failure consumes the caller's location attempts
but does not reselect the race unless the caller explicitly does so.

**Parity notes:** The unique `cur_num/max_num` check in `get_mon_num()` is
commented out; placement is authoritative.

**Suggested test level:** Unit.

## 10. Ordinary Monster Population

### DG-MON-005 — Ordinary request count

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`; `mdefines.h :: MIN_M_ALLOC_LEVEL`.

**Given:** Positive depth `Depth`.

**When:** Ordinary monster requests are made.

**Then:** The request starts at `14 + randint1(8)`, then adds
`k = clamp(Depth / 3, 2, 10)`. Each request calls `alloc_monster()` and may fail.

**Random behavior:** `randint1(8)` is one through eight; `Depth / 3` is integer
division.

**Parity notes:** This is a request count, not final population. Each placement
has its own location and race-selection failures.

**Suggested test level:** Unit / Golden-seed.

**Derived examples:** Before placement failures and group expansion, the request
range is 16..23 at DL1, 18..25 at DL10, 21..28 at DL30, and 21..28 at DL50.

## 11. Groups and Escorts

### DG-GROUP-001 — FRIENDS group expansion

**Parity status:** Required

**Evidence:** Verified

**Source:** `monster2.c :: place_monster_aux()`, `place_monster_group()`;
`GROUP_MAX = 32`.

**Given:** A leader race with `RF1_FRIENDS` and `grp = TRUE`.

**When:** The leader is successfully placed.

**Then:** A group begins with the leader and expands breadth-first through
adjacent empty cells, up to 32 total entries. Base size is `randint1(13)`;
easier-than-depth races bias upward and deeper-than-depth races bias downward,
with minimum one.

**Random behavior:** Group size and placement locations consume RNG in source
order.

**Parity notes:** `grp = FALSE` suppresses group expansion even when FRIENDS is
present.

**Suggested test level:** Unit / Golden-seed.

### DG-GROUP-002 — Escort selection

**Parity status:** Required

**Evidence:** Verified

**Source:** `monster2.c :: place_monster_aux()`, `place_monster_okay()`.

**Given:** A leader with `RF1_ESCORT` and `grp = TRUE`.

**When:** Escort attempts run.

**Then:** Up to 50 location attempts are made. Candidates must share the
leader's symbol, have level no greater than the leader, be non-unique, and have
a different race index. `ESCORTS` or escort FRIENDS can trigger group expansion.

**Random behavior:** Candidate allocation and scatter positions consume RNG.

**Parity notes:** Escort processing is context-sensitive and absent when
`grp = FALSE`.

**Suggested test level:** Unit / Golden-seed.

## 12. Monster Drops

### DG-DROP-001 — Additive quantity flags

**Parity status:** Required

**Evidence:** Verified

**Source:** `xtra2.c :: monster_death()`.

**Given:** A monster dies.

**When:** Random drop quantity is calculated.

**Then:** `DROP_60` adds one on an independent 60% roll; `DROP_90` adds one on
an independent 90% roll; `DROP_1D2` through `DROP_4D2` add their dice results.
All applicable flags stack.

**Random behavior:** Roll order is 60%, 90%, then 1D2, 2D2, 3D2, 4D2.

**Parity notes:** This quantity excludes carried objects.

**Suggested test level:** Unit / Golden-seed.

### DG-DROP-002 — Gold/item branch and quality

**Parity status:** Required

**Evidence:** Verified

**Source:** `xtra2.c :: monster_death()`; `object2.c :: place_object()`.

**Given:** A generated drop and `ONLY_ITEM`/`ONLY_GOLD` state.

**When:** Each drop is created.

**Then:** Gold is disabled by ONLY_ITEM; objects are disabled by ONLY_GOLD. If
both are allowed, the source chooses gold versus item through its 50% branch.
Item drops receive `good` and `great` from `DROP_GOOD` and `DROP_GREAT`. The
item level is `(Depth + monster_native_level) / 2`.

**Random behavior:** Branch and object generation consume RNG in source order.

**Parity notes:** Carried objects use `drop_near()` separately; unique drops
receive race-name inscriptions.

**Suggested test level:** Unit / Golden-seed.

## 13. Object Generation

### DG-OBJ-001 — Object OOD and quality

**Parity status:** Required

**Evidence:** Verified

**Source:** `object2.c :: get_obj_num()`, `apply_magic()`, `mdefines.h`.

**Given:** A positive object allocation level.

**When:** Object allocation and magic are applied.

**Then:** `GREAT_OBJ = 20` gives a 1-in-20 allocation-depth boost. On success,
effective level becomes `1 + (level * MAX_DEPTH / randint1(MAX_DEPTH))`. Good
chance is `min(level + 10, 75)`; great chance is integer `min(goodChance / 2, 20)`
after a good classification.

**Random behavior:** OOD roll precedes weighted object selection; quality rolls
and artifact attempts follow source order.

**Parity notes:** Allocation level, successful object creation, and final item
quality are separate outcomes.

**Suggested test level:** Unit / Golden-seed.

### DG-OBJ-002 — Artifact and rating sequence

**Parity status:** Required

**Evidence:** Verified

**Source:** `object2.c :: place_object()`, `apply_magic()`,
`make_artifact_special()`, `make_artifact()`.

**Given:** An object placement request.

**When:** Artifact and magic processing occurs.

**Then:** Special artifact selection is attempted in `place_object()`. Normal
artifact attempts occur in `apply_magic()`: one for excellent power and four
when forced great. Optional randart processing is conditional on build/config.
Base magic and ego application follow the source path. Artifacts add rating,
expensive artifacts add an additional rating increment, ego items add their
configured rating, and Dragon Scale Mail adds 30.

**Random behavior:** Artifact attempts and quality rolls are source-order
sensitive.

**Parity notes:** Failed artifact attempts fall through to ordinary generation.

**Suggested test level:** Unit / Golden-seed.

### DG-OBJ-003 — Ordinary room, object, and gold requests

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: cave_gen()`;
`DUN_AMT_ROOM = 9`, `DUN_AMT_ITEM = 3`, `DUN_AMT_GOLD = 3`.

**Given:** An ordinary positive dungeon level after monsters, traps, and rubble
requests.

**When:** Ordinary object allocation runs.

**Then:** Room objects request `randnor(DUN_AMT_ROOM, 3)`, general objects
request `randnor(DUN_AMT_ITEM, 3)`, and gold requests
`randnor(DUN_AMT_GOLD, 3)`. Each request is passed to `alloc_object()` and may
fail to create a successful placement or object.

**Random behavior:** Each normal-distribution call and each allocation search
consumes RNG in source order. These are distribution parameters, not fixed
counts.

**Parity notes:** Requested counts must not be reported as successfully placed
counts. Negative results, if produced by the source normal generator, result in
zero loop iterations because the allocation loop condition is `k < num`.

**Suggested test level:** Unit / Golden-seed / Statistical.

### DG-LIFE-004 — Streamers, rubble, and player start

**Parity status:** Required

**Evidence:** Verified

**Source:** `ref-mangband/src/server/generate.c :: cave_gen()`,
`build_streamer()`, `place_rubble()`, `new_player_spot()`.

**Given:** Rooms and tunnels have been built.

**When:** The remaining ordinary level stages execute.

**Then:** Three magma streamers and two quartz streamers are built with
`DUN_STR_RNG = 2`, `DUN_STR_DEN = 5`, treasure denominators 90 and 40. After
destruction and stairs, the player start is selected from a legal naked floor
location. Corridor rubble requests `randint1(k)` placements using the same
`k = clamp(Depth / 3, 2, 10)` depth term.

**Random behavior:** Streamer paths, treasure rolls, player start selection,
and rubble locations consume RNG in source order.

**Parity notes:** Streamers and rubble are placement attempts, not guaranteed
successful entity counts. Player-start search must not place the player on an
occupied or forbidden cell.

**Suggested test level:** Golden-seed.

## 14. Traps

### DG-TRAP-001 — Requested trap positions

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`, `alloc_object()`, `object2.c :: place_trap()`.

**Given:** Positive dungeon depth.

**When:** Ordinary trap allocation runs.

**Then:** It requests `randint1(clamp(Depth / 3, 2, 10))` positions. Each requires
an in-bounds naked floor; failed requests do not become traps.

**Random behavior:** Request count and location search consume RNG.

**Parity notes:** Requested positions and successfully placed traps differ.

**Suggested test level:** Unit / Golden-seed.

### DG-TRAP-002 — Trap selection and activation

**Parity status:** Required

**Evidence:** Verified

**Source:** `object2.c :: pick_trap()`; `cmd1.c :: search()`, `move_player()`,
`hit_trap()`.

**Given:** An invisible trap is discovered or a visible trap is entered.

**When:** Discovery/activation occurs.

**Then:** Search converts `FEAT_INVIS` to a visible trap without activating it.
Movement converts and immediately activates an invisible trap. Normal selection
is uniform over 16 offsets; quest/bottom selection excludes trap-door offset 0
and is uniform over the remaining 15.

**Random behavior:** Trap selection occurs at discovery, before activation.

**Parity notes:** Offset 4 clears itself; offset 0 transitions away; the other
visible traps remain repeatable.

**Suggested test level:** Unit / Golden-seed.

## 15. Level Rating and Feelings

### DG-RATING-001 — Rating contributors

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: build_type5()`, `build_type6()`, `build_type7()`,
`build_type8()`, `object2.c :: apply_magic()`, `place_object()`,
`monster2.c :: place_monster_one()`.

**Given:** A level-generation event creates special content, OOD content, or
quality content.

**When:** Rating is updated.

**Then:** Nests and pits add 10; vaults add their `rat`; ordinary OOD monsters
add level delta; OOD uniques add twice level delta; artifacts add 10 and
expensive artifacts add another 10; ego items add configured rating; Dragon
Scale Mail adds 30; qualifying OOD objects add native-level delta.

**Random behavior:** Rating inherits every preceding selection/placement roll.

**Parity notes:** Rating is not a single treasure-only score.

**Suggested test level:** Unit / Golden-seed.

### DG-FEEL-001 — Feeling thresholds

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: generate_cave()`.

**Given:** Final rating and `good_item_flag`.

**When:** Feeling is calculated.

**Then:** Apply this exact ordered mapping: `rating > 100` gives feeling 2;
`rating > 80` gives 3; `rating > 60` gives 4; `rating > 40` gives 5;
`rating > 30` gives 6; `rating > 20` gives 7; `rating > 10` gives 8;
`rating > 0` gives 9; otherwise rating gives 10. After that mapping,
`good_item_flag` overrides the result to feeling 1. A town overrides the result
to feeling 0.

**Random behavior:** None after rating is complete.

**Parity notes:** Autoscum evaluates the post-override feeling. In non-Ironman
mode, the feeling cooldown can suppress the feeling and disables scumming for
that attempt; town generation has no feeling.

**Suggested test level:** Unit.

## 16. Destroyed Levels

### DG-DEST-001 — Destroyed-level eligibility

**Parity status:** Required

**Evidence:** Verified

**Source:** `generate.c :: cave_gen()`, `DUN_DEST = 15`.

**Given:** Positive depth greater than 10 and a non-quest level.

**When:** The destroyed-level roll occurs.

**Then:** A 1-in-15 roll enables destruction. Quest levels always disable it.
The selected level first undergoes normal room/tunnel/streamer generation, then
`destroy_level()` removes monsters/objects and rewrites cells in circular
regions with granite, quartz, magma, or floor.

**Random behavior:** Eligibility roll and each destruction cell use source RNG.

**Parity notes:** This is not equivalent to generating a blank destroyed map.

**Suggested test level:** Golden-seed.

## 17. Town, Quest, and Special Levels

### DG-TOWN-001 — Town generation

**Parity status:** Conditional

**Evidence:** Verified

**Source:** `generate.c :: town_gen()`, `town_gen_hack()`.

**Given:** Depth zero.

**When:** Town generation runs.

**Then:** The town uses seeded layout generation, boundary features, stores,
streets, stairs, and lighting; it does not use ordinary dungeon rooms,
tunnels, or vault selection. Town feeling is 0.

**Random behavior:** Town layout uses `Rand_quick = TRUE` and `seed_town`.

**Parity notes:** Resident loops using `MIN_M_ALLOC_TD/TN` are commented out in
the inspected source.

**Suggested test level:** Golden-seed / Integration.

### DG-QUEST-001 — Quest and bottom behavior

**Parity status:** Conditional

**Evidence:** Verified

**Source:** `cave.c :: is_quest()`, `generate.c :: cave_gen()`,
`alloc_stairs()`, `object2.c :: pick_trap()`.

**Given:** A player quest depth or depth `MAX_DEPTH - 1`.

**When:** Generation and trap/stair rules run.

**Then:** Destroyed-level generation is disabled; up-stair behavior is forced;
trap-door selection is excluded. Player-specific quest detection also ignores
trap-door activation.

**Random behavior:** Trap-door exclusion changes the selection denominator.

**Parity notes:** Quest detection can depend on which players are present.

**Suggested test level:** Integration.

### DG-QUEST-002 — Static level loading

**Parity status:** Conditional

**Evidence:** Verified

**Source:** `load2.c :: rd_dungeon()`, `rd_dungeon_special()`; `save.c` dungeon
level persistence.

**Given:** `cfg_ironman` or `cfg_more_towns` and a matching
`server.level.<k>.<j>.<i>` file.

**When:** Special levels load.

**Then:** Terrain/features, grid flags, stair coordinates, dimensions, and
metadata are restored. Static loading is separate from `cave_gen()`. Objects and
monsters are restored through save-state paths rather than generated by the
feature reader.

**Random behavior:** Loading itself does not generate the map.

**Parity notes:** No default static files were found in the inspected
repository snapshot.

**Suggested test level:** Integration.

## 18. Wilderness

### DG-WILD-001 — Wilderness population

**Parity status:** Required

**Evidence:** Verified

**Source:** `wilderness.c :: wilderness_gen_hack()`, `wilderness_gen()`,
`wild_add_monster()`; `monster2.c :: get_mon_num()`.

**Given:** A generated negative-depth wilderness level.

**When:** Initial residents are added.

**Then:** The source requests `wild_info[Depth].type` monsters, uses terrain-
derived `monster_level`, applies a terrain-specific allocation hook, calls
`get_mon_num(monster_level)`, and places through `place_monster_aux(..., FALSE,
TRUE)`.

**Random behavior:** Shared monster OOD rolls, weighted allocation, placement,
groups, and escorts consume RNG after seeded terrain generation.

**Parity notes:** Seeded geometry does not imply deterministic population.
Numeric wilderness type values remain unresolved in the available header set.

**Suggested test level:** Golden-seed / Statistical.

## 19. RNG and Determinism Contract

### DG-RNG-001 — Seeded reproducibility

**Parity status:** Required

**Evidence:** Derived

**Source:** Combined source behavior from `generate.c`, `monster2.c`,
`object2.c`, and `wilderness.c`.

**Given:** Identical definitions, world state, configuration, generation inputs,
and RNG seed/stream.

**When:** The same generation path runs.

**Then:** It produces identical observable map, placement, rating, and feeling
results, including failed attempts.

**Random behavior:** RNG call order is parity-significant for room selection,
tunnel wandering, doors, destruction, OOD rolls, weighted selection, candidate
comparison, objects, traps, groups, escorts, and vault contents.

**Parity notes:** Statistical similarity is insufficient where seeded replay is
feasible.

**Suggested test level:** Golden-seed.

## 20. IronHell Representation Notes

**Observed**: current IronHell definitions contain `SpawnPolicy` fields for
unique, questor, force-depth, force-max-hp, force-sleep, friends, escort, and
escorts. This is data representation, not proof of runtime behavior.

**Observed**: current IronHell catalogs include monster loot profiles, vault
layouts, and trap definitions. No corresponding ordinary dungeon-generation
runtime was established by the frozen research audit.

## 21. Unresolved and Conditional Requirements

- Numeric wilderness type values remain **Unresolved** and are not required to
  specify the population call chain, but they are required for exact population
  count tests.
- Static level distribution content remains **Conditional/Unresolved** because
  it depends on configuration and save-directory files.
- Non-player trap activation remains **Unresolved** because the verified trigger
  path is player-specific.
- `WIDE_CORRIDORS` remains **Conditional** on source build configuration.
- RANDART behavior remains **Conditional** on build/configuration flags.
- IronHell runtime consumers for all generation catalogs remain **Observed
  missing**, not MAngband behavioral requirements.

## 22. Parity Test Classification

**Exact deterministic tests** should cover table construction, integer weights,
filtering, trap offset selection, vault glyph order, rating formulas, and
feeling thresholds.

**Golden-seed tests** should cover complete levels, room/tunnel order, vault
contents, destroyed levels, nests, pits, groups, escorts, and wilderness maps.

**Statistical tests** should cover 1-in-50 monster OOD, 1-in-20 object OOD, door
distribution, trap distributions, and quality distributions only where exact
seed replay is unavailable or intentionally not required.

## 23. Specification Closure

The frozen MAngband source baseline is sufficiently specified for future
IronHell dungeon-generation requirements. Remaining conditional behavior must
be represented explicitly in tests/configuration rather than silently replaced
with defaults. No implementation architecture is prescribed by this document.

## Parity Test Matrix

Every requirement has a verification row. Exact deterministic tests are primary
where the RNG stream and state can be controlled; statistical tests supplement
them only for distributions.

| Requirement ID | Test type | Seed | Primary assertion | Priority |
|---|---|---|---|---|
| DG-LIFE-001 | Unit | No | Depth dispatch | P0 |
| DG-LIFE-002 | Golden-seed | Yes | Exact lifecycle order/output | P0 |
| DG-LIFE-003 | Integration | Yes | Retry/autoscum | P1 |
| DG-LIFE-004 | Golden-seed | Yes | Streamers, rubble, start | P1 |
| DG-ROOM-001 | Unit | Optional | Footprint reservation | P0 |
| DG-ROOM-002 | Golden-seed | Yes | 50 attempts/unusual rolls | P0 |
| DG-ROOM-003 | Unit | No | Family metadata | P0 |
| DG-ROOM-004 | Golden-seed | Yes | Normal/overlap geometry | P1 |
| DG-ROOM-005 | Golden-seed | Yes | Cross/large base geometry | P1 |
| DG-ROOM-006 | Unit | No | Centers/flags | P0 |
| DG-ROOM-007 | Golden-seed | Yes | Type-1 variants | P1 |
| DG-ROOM-008 | Golden-seed | Yes | Type-2 overlap | P1 |
| DG-ROOM-009 | Golden-seed | Yes | Four Type-3 variants | P1 |
| DG-ROOM-010 | Golden-seed | Yes | Five Type-4 variants | P1 |
| DG-CONN-001 | Golden-seed | Yes | Cyclic center links | P0 |
| DG-TUN-001 | Golden-seed | Yes | Tunnel decisions | P0 |
| DG-TUN-002 | Golden-seed | Yes | Penetrations/junction doors | P1 |
| DG-DOOR-001 | Unit/Statistical | Optional | Door buckets | P1 |
| DG-DOOR-002 | Unit | No | Direct room/vault doors | P1 |
| DG-STAIR-001 | Golden-seed | Yes | Counts/retries | P0 |
| DG-STAIR-002 | Integration | Yes | Town/quest/bottom direction | P1 |
| DG-VAULT-001 | Unit/Golden-seed | Yes | Eligibility/normalization | P1 |
| DG-VAULT-002 | Unit/Golden-seed | Yes | Glyph order/failures | P0 |
| DG-NEST-001 | Golden-seed | Yes | 64 samples/abort | P1 |
| DG-NEST-002 | Unit | No | Family predicates | P1 |
| DG-PIT-001 | Golden-seed | Yes | 16 samples/sort/tiers | P1 |
| DG-PIT-002 | Unit | No | Pit predicates/masks | P1 |
| DG-MON-001 | Unit | No | Table construction | P0 |
| DG-MON-002 | Unit/Golden-seed | Yes | Same-p comparison/replacement | P0 |
| DG-MON-003 | Unit/Statistical | Optional | Forced 1-in-50 OOD | P0 |
| DG-MON-004 | Unit | No | Allocation vs placement rejection | P0 |
| DG-MON-005 | Unit/Golden-seed | Yes | Request formula/failures | P1 |
| DG-GROUP-001 | Unit/Golden-seed | Yes | BFS/cap/size | P1 |
| DG-GROUP-002 | Unit/Golden-seed | Yes | Escort restrictions | P1 |
| DG-DROP-001 | Unit/Golden-seed | Yes | Additive quantity/order | P1 |
| DG-DROP-002 | Unit/Golden-seed | Yes | Gold/item/quality | P1 |
| DG-OBJ-001 | Unit/Golden-seed | Yes | OOD/quality formulas | P0 |
| DG-OBJ-002 | Unit/Golden-seed | Yes | Artifact/order/rating | P1 |
| DG-OBJ-003 | Unit/Statistical | Optional | Request parameters/success | P1 |
| DG-TRAP-001 | Unit/Golden-seed | Yes | Requested vs placed | P0 |
| DG-TRAP-002 | Unit/Golden-seed | Yes | All offsets/activation | P0 |
| DG-RATING-001 | Unit/Golden-seed | Yes | Independent contributions | P1 |
| DG-FEEL-001 | Unit | No | Exact thresholds/precedence | P0 |
| DG-DEST-001 | Golden-seed | Yes | Eligibility/rewrite order | P1 |
| DG-TOWN-001 | Golden-seed/Integration | Yes | Seeded town/feeling 0 | P2 |
| DG-QUEST-001 | Integration | Yes | Quest/bottom restrictions | P1 |
| DG-QUEST-002 | Integration | No | Static snapshot load | P2 |
| DG-WILD-001 | Golden-seed/Statistical | Yes | Terrain/population separation | P2 |
| DG-RNG-001 | Golden-seed | Yes | Complete replay equality | P0 |

Required fixtures include canonical feature grids, source monster/object/vault
records, controlled allocation tables, quest/configuration state, and explicit
legal/illegal placement cells. Dependencies follow the generation order and
the source call chains represented by the requirement IDs.

## Golden-Seed Scenarios

Seeds are intentionally unspecified until repository conventions and reference
execution establish them.

| Scenario | Snapshot assertions | Requirements |
|---|---|---|
| GS-001 Basic shallow dungeon | Dimensions, grid hash, rooms, stairs, entities, rating/feeling | DG-LIFE-002, DG-ROOM-001, DG-STAIR-001 |
| GS-002 Mid-depth dungeon | Unusual rooms, object quality, rating | DG-ROOM-002, DG-OBJ-001, DG-RATING-001 |
| GS-003 Deep dungeon | OOD content, pit/nest/vault eligibility | DG-MON-003, DG-NEST-001, DG-PIT-001, DG-VAULT-001 |
| GS-004 Destroyed level | Rewritten cells and removed entities | DG-DEST-001 |
| GS-005 Lesser vault | Identity, layout, glyph results, rating | DG-VAULT-001/002 |
| GS-006 Greater/gigantic vault | Type-9 normalization and sequential glyph results | DG-VAULT-001/002 |
| GS-007 Monster nest | Family, candidate IDs, positions, rating | DG-NEST-001/002 |
| GS-008 Monster pit | Mask, candidates, sorted tiers, positions | DG-PIT-001/002 |
| GS-009 Quest/bottom | Stairs, trap domain, destruction exclusion | DG-QUEST-001, DG-TRAP-002 |
| GS-010 Wilderness | Terrain hash separately from population/RNG state | DG-WILD-001, DG-RNG-001 |
| GS-011 Town | Seeded layout, stairs, feeling 0 | DG-TOWN-001 |

Snapshots should include dimensions, canonical grid/terrain hash, room
centers/count, stairs, vault identity, monster IDs/positions, object
IDs/positions, traps, rating, feeling, and RNG final state/call count when that
boundary is part of the fixture. Expected values are not invented here.

## Behavioral Implementation Slices

These are behavioral slices only. They do not prescribe classes, interfaces,
dependency injection, or project restructuring.

### Slice 1 — Deterministic allocation boundary

**Behavioral scope:** Seeded decisions, allocation tables, weighted selection,
and OOD outcomes.

**Parity requirements:** DG-RNG-001, DG-MON-001/002/003.

**Tests first:** Allocation-table units and forced OOD tests.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`.

**Out of scope:** Map features, persistence, Godot, networking.

**Completion criteria:** Controlled inputs reproduce allocation decisions and
RNG-sensitive outcomes.

### Slice 2 — Grid, rooms, and room selection

**Behavioral scope:** Features, room flags, reservation, simple/overlap/cross/
large geometry, variants, and failed reservations.

**Parity requirements:** DG-ROOM-001 through DG-ROOM-010.

**Tests first:** Room units and GS-001 geometry snapshots.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`.

**Out of scope:** Monsters, objects, persistence, rendering.

**Completion criteria:** All specified room variants reproduce geometry, flags,
branches, and placement attempts.

### Slice 3 — Connectivity, doors, stairs, and start

**Behavioral scope:** Center order, tunnels, doors, stairs, legal-cell retry,
and player start selection.

**Parity requirements:** DG-CONN-001, DG-TUN-001/002, DG-DOOR-001/002,
DG-STAIR-001/002, DG-LIFE-004.

**Tests first:** Topology, door, stair, and start-cell golden tests.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`.

**Out of scope:** Vault contents, monster allocation, object quality.

**Completion criteria:** Map topology and transition features match controlled
source behavior, including failed attempts.

### Slice 4 — Vault interpretation

**Behavioral scope:** Catalog eligibility, type-9 normalization, glyph order,
same-cell sequential attempts, and vault rating.

**Parity requirements:** DG-VAULT-001/002.

**Tests first:** Per-glyph fixtures and GS-005/GS-006.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`,
existing `data/definitions/environment/` observations.

**Out of scope:** Migrating missing vault records or static-level content.

**Completion criteria:** Every verified glyph has deterministic, failure-aware
observable behavior.

### Slice 5 — Monsters, nests, pits, groups, escorts

**Behavioral scope:** Ordinary population, placement rejection, special filters,
groups, escorts, and request counts.

**Parity requirements:** DG-MON-004/005, DG-NEST-001/002, DG-PIT-001/002,
DG-GROUP-001/002.

**Tests first:** Allocation/placement units and GS-007/GS-008.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`,
existing monster data through `src/IronHell.Data/`.

**Out of scope:** Combat, monster drops, wilderness regeneration.

**Completion criteria:** Candidate eligibility, failed placement, formation
contexts, and spatial arrangements are deterministic.

### Slice 6 — Objects, gold, drops, rating

**Behavioral scope:** Normal requests, OOD objects, quality, artifacts, ego
effects, drops, and rating contributions.

**Parity requirements:** DG-OBJ-001/002/003, DG-DROP-001/002,
DG-RATING-001.

**Tests first:** Quality/OOD units, drop-flag matrix, GS-002.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`,
existing item/monster catalogs through `src/IronHell.Data/`.

**Out of scope:** New loot schemas, catalog migration, economy behavior.

**Completion criteria:** Requested versus successful creation and all rating
effects match controlled source behavior.

### Slice 7 — Traps, feelings, destruction, retries

**Behavioral scope:** Trap effects, feeling precedence, autoscum, and destroyed
level rewriting.

**Parity requirements:** DG-TRAP-001/002, DG-FEEL-001, DG-DEST-001,
DG-LIFE-003.

**Tests first:** All 16 trap fixtures, feeling table, GS-004/GS-009.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`.

**Out of scope:** Trap catalog migration and player combat implementation.

**Completion criteria:** Exact effects, precedence, retry behavior, and rewrite
order are testable.

### Slice 8 — Town, quest, static, wilderness

**Behavioral scope:** Conditional town, quest/bottom, static snapshot, and
wilderness geometry/population paths.

**Parity requirements:** DG-TOWN-001, DG-QUEST-001/002, DG-WILD-001.

**Tests first:** GS-009 through GS-011 and static-load integration fixtures.

**Likely repository areas:** `src/IronHell.Core/`, `src/IronHell.Core.Tests/`,
`src/IronHell.Data/`, and existing persistence boundaries only where needed.

**Out of scope:** Static content creation, Godot presentation, networking.

**Completion criteria:** Conditional paths are explicit and do not silently use
ordinary dungeon behavior.

## Traceability Checklist

| Research source | Requirements | Planned verification |
|---|---|---|
| `ref-mangband/src/server/generate.c :: cave_gen()` | DG-LIFE-002, DG-ROOM-002, DG-LIFE-004, DG-DEST-001 | Golden-seed + integration |
| `ref-mangband/src/server/generate.c :: build_type1()` through `build_type4()` | DG-ROOM-004/005, DG-ROOM-007/008/009/010 | Unit + Golden-seed |
| `ref-mangband/src/server/generate.c :: build_tunnel()` | DG-CONN-001, DG-TUN-001/002 | Golden-seed |
| `ref-mangband/src/server/generate.c :: build_vault()` | DG-VAULT-002 | Unit + Golden-seed |
| `ref-mangband/src/server/monster2.c :: get_mon_num()` | DG-MON-001/002/003 | Unit + Golden-seed |
| `ref-mangband/src/server/generate.c :: build_type5()` | DG-NEST-001/002 | Unit + Golden-seed |
| `ref-mangband/src/server/generate.c :: build_type6()` | DG-PIT-001/002 | Unit + Golden-seed |
| `ref-mangband/src/server/xtra2.c :: monster_death()` | DG-DROP-001/002 | Unit + Golden-seed |
| `ref-mangband/src/server/object2.c :: place_object()` | DG-OBJ-001/002/003 | Unit + Golden-seed |
| `ref-mangband/src/server/object2.c :: pick_trap()` and `cmd1.c :: hit_trap()` | DG-TRAP-001/002 | Unit |
| `ref-mangband/src/server/generate.c :: generate_cave()` | DG-FEEL-001, DG-TOWN-001, DG-QUEST-001 | Unit + Integration |
| `ref-mangband/src/server/wilderness.c :: wilderness_gen()` | DG-WILD-001 | Golden-seed + Statistical |
| `ref-mangband/src/server/load2.c :: rd_dungeon_special()` | DG-QUEST-002 | Integration |

## Unresolved Requirements and Actions

| Requirement area | Status | Parity impact | Ordinary blocker? | Test blocker? | Action |
|---|---|---|---|---|---|
| Numeric wilderness type values | Unresolved | Medium | No | No | Conditional test |
| Non-player trap activation | Unresolved | Low | No | No | Defer |
| Distributed static-level content | Conditional/Unresolved | Medium | No | No | Conditional test |
| `WIDE_CORRIDORS` build option | Conditional | Medium | No | No | Conditional test |
| RANDART configuration | Conditional | Medium | No | No | Conditional test |
| IronHell runtime consumers | Observed missing | High for implementation | No | No for specification | Defer to implementation |

## Specification Freeze Decision

**READY TO FREEZE**

`DG-MON-002` is mechanically precise; type 1–4 room variants, feeling
thresholds/precedence, ordinary object/gold formulas, and core constants are
explicit; every requirement has a matrix row; golden-seed scenarios, behavioral
slices, and traceability are defined. Remaining unknowns are isolated as
conditional or non-blocking and do not require reopening MAngband source for
ordinary dungeon generation.
