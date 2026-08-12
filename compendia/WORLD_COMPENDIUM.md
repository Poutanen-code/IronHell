# IronHell World Compendium

> Reference document detailing MAngband 1.5.3 world generation mechanics for IronHell's implementation.
> Source files: `reference-mangband-1_5_3/src/server/generate.c`, `wilderness.c`, `store.c`, `dungeon.c`, `init2.c`, `mdefines.h`, `common/defines.h`.

---

## Table of Contents

1. [World Structure Overview](#1-world-structure-overview)
2. [Wilderness Coordinate System](#2-wilderness-coordinate-system)
3. [Wilderness Terrain Types](#3-wilderness-terrain-types)
4. [Terrain Type Determination](#4-terrain-type-determination)
5. [Terrain Composition Parameters](#5-terrain-composition-parameters)
6. [Wilderness Generation Procedure](#6-wilderness-generation-procedure)
7. [Terrain Bleeding (Border Blending)](#7-terrain-bleeding-border-blending)
8. [Hotspots](#8-hotspots)
9. [Wilderness Dwellings](#9-wilderness-dwellings)
10. [Dwelling Contents](#10-dwelling-contents)
11. [Crops and Gardens](#11-crops-and-gardens)
12. [Day/Night Cycle](#12-daynight-cycle)
13. [Wilderness Monster Spawning](#13-wilderness-monster-spawning)
14. [Town Generation](#14-town-generation)
15. [Town Layout](#15-town-layout)
16. [Dungeon Generation](#16-dungeon-generation)
17. [Room Types](#17-room-types)
18. [Tunnel Generation](#18-tunnel-generation)
19. [Streamers](#19-streamers)
20. [Destroyed Levels](#20-destroyed-levels)
21. [Stairs and Player Placement](#21-stairs-and-player-placement)
22. [Dungeon Object and Monster Placement](#22-dungeon-object-and-monster-placement)
23. [Door Placement Probabilities](#23-door-placement-probabilities)
24. [Level Rating and Feelings](#24-level-rating-and-feelings)
25. [Stores: Structure and Constants](#25-stores-structure-and-constants)
26. [Store Inventory Tables](#26-store-inventory-tables)
27. [Store Maintenance and Restocking](#27-store-maintenance-and-restocking)
28. [Store Item Creation](#28-store-item-creation)
29. [Mass Production and Discounts](#29-mass-production-and-discounts)
30. [Store Pricing](#30-store-pricing)
31. [Player-Owned Shops](#31-player-owned-shops)
32. [IronHell Status Summary](#32-ironhell-status-summary)

---

## 1. World Structure Overview

MAngband organizes the entire world into a single `cave[Depth]` array indexed by a depth integer:

| Depth | Meaning |
|-------|---------|
| `0` | Town level |
| `1` — `127` | Dungeon levels (deeper = harder) |
| `-1` — `-4095` | Wilderness tiles (negative index) |

**Grid dimensions** are uniform across all levels:
- `MAX_HGT = 66` rows
- `MAX_WID = 198` columns

**Maximum dungeon depth:** `MAX_DEPTH = 128` (levels 1–127 are playable).  
**Maximum wilderness tiles:** `MAX_WILD = 4096`.

Generation is dispatched in `generate_cave()`:

```c
if (!Depth)          → town_gen()
else if (Depth < 0)  → wilderness_gen(Depth)
else                 → cave_gen(Depth)
```

Levels are generated on demand when a player enters them and deallocated when no longer occupied (unless a player-owned house exists on that level).

---

## 2. Wilderness Coordinate System

Wilderness tiles are addressed by a 2D world coordinate `(world_x, world_y)` with the town at `(0, 0)`. The internal negative depth index is computed via a **ring-based** formula:

```c
ring   = abs(world_x) + abs(world_y)
base   = 2 * ring * (ring - 1) + 1
offset = (world_x >= 0) ? (ring - world_y) : (3*ring + world_y)
idx    = -(base + offset)
```

**Ring layout** (ring number = Manhattan distance from town):

```
         [05]              Ring 2
     [12][01][06]          Ring 1 + 2
 [11][04][To][02][07]      Ring 1 (Town = 0)
     [10][03][08]
         [09]

 world_x: -2  -1   0   1   2
 world_y: ...visualized as rows going N (+y) / S (-y)
```

Cardinal neighbors are computed with `neighbor_index(Depth, DIR_NORTH/EAST/SOUTH/WEST)`.

The full wilderness is initialized recursively from `(0,0)` outward, storing `world_x`, `world_y`, and `radius = abs(x) + abs(y)` in each `wild_info[depth]` structure.

---

## 3. Wilderness Terrain Types

Each wilderness tile has exactly one terrain type, defined as:

| Constant | Value | Description |
|----------|-------|-------------|
| `WILD_LAKE` | 0 | Open water — mostly water tiles |
| `WILD_GRASSLAND` | 1 | Open plains — mostly grass |
| `WILD_FOREST` | 2 | Mixed forest — trees + grass |
| `WILD_WASTELAND` | 7 | Dangerous waste — sparse grass, evil trees |
| `WILD_UNDEFINED` | 8 | Not yet determined |
| `WILD_CLONE` | 9 | Copy terrain from a random neighbor |
| `WILD_TOWN` | 10 | Town tile (depth 0 only) |
| `WILD_DENSEFOREST` | 15 | Dense dark forest — heavy trees |
| `WILD_SWAMP` | 20 | Swamp — water + mud + trees |

The numeric values double as the **default monster spawn count** for that biome (see §13).

---

## 4. Terrain Type Determination

Terrain type is resolved lazily via `determine_wilderness_type(Depth)`. It uses the **simple (seeded) RNG** seeded with:

```c
Rand_value = seed_town + Depth * 600
```

This makes every tile's type **deterministic and reproducible** based on the server's `seed_town` value, regardless of the order players explore.

### Algorithm

1. If `type == WILD_UNDEFINED`: always sets `type = WILD_CLONE` on the first call (the `randint0(100) < 101` condition is always true).
2. If `type == WILD_CLONE` (second call, or in a loop): draws actual terrain from the sequence below.

**Terrain roll when resolving a clone:**

| Check | Probability | Result |
|-------|-------------|--------|
| `randint0(100) < 3` | 3% | WILD_WASTELAND |
| `randint0(100) < 5` | ~4.9% | WILD_DENSEFOREST |
| `randint0(100) < 40` | ~37%  | WILD_FOREST |
| `randint0(100) < 10` | ~9%   | WILD_SWAMP |
| `randint0(100) < 15` | ~12.8% | WILD_LAKE |
| else | ~33%  | WILD_GRASSLAND |

> Note: Because each `randint0(100)` draws a fresh number, the effective probabilities compound. The above are approximate.

3. When `type == WILD_CLONE` (normal case), the tile **copies a random neighbor's type recursively**. This produces large contiguous biome blobs — a forest tile copies its neighbor, which copied its neighbor, etc.

### Closed-Loop Detection

If clones form a circular reference (A→B→A), the loop is detected and broken by reseeding with:

```c
Rand_value = seed_town + total_loop_depth * 8973
```

Then a non-clone terrain is picked from the roll table above.

### Near-Town Restrictions (`radius <= 2`)

To keep the area around the town habitable, certain dangerous biomes are partially converted to grassland:

| Biome | At radius ≤ 2 |
|-------|--------------|
| WILD_WASTELAND | Always → WILD_GRASSLAND |
| WILD_DENSEFOREST | 80% → WILD_GRASSLAND |
| WILD_SWAMP | 50% → WILD_GRASSLAND |
| WILD_FOREST | 30% → WILD_GRASSLAND |

---

## 5. Terrain Composition Parameters

Each biome has numeric parameters that control the density of ground features, computed by `init_terrain()`. All values are out of **1000** (probabilities × 1000).

`terrain_spot()` applies them in order — later checks override earlier ones:

```
FEAT_DIRT (default)
→ if randint0(1000) < grass:    FEAT_GRASS
→ if randint0(1000) < tree:     FEAT_TREE
→ if randint0(1000) < eviltree: FEAT_EVIL_TREE
→ if randint0(1000) < water:    FEAT_WATER
→ if randint0(1000) < mud:      FEAT_MUD
```

### Parameter Table

| Biome | grass (÷1000) | tree (÷1000) | water (÷1000) | eviltree (÷1000) | dwelling | hotspots | monst_lev |
|-------|--------------|-------------|--------------|-----------------|----------|----------|-----------|
| WILD_GRASSLAND | 850–1049 | 0–14 | 0–9 | 0 | 100 | 0–5 | 1 + radius/2 |
| WILD_FOREST | 500–899 | 100–299 | 0–19 | 0 | 37 | 0–9 | 5 + radius/2 |
| WILD_DENSEFOREST | 850–949 | 600–749 | 0–14 | 5–14 | 8 | 4–18 | 15 + radius/2 (+10 night) |
| WILD_SWAMP | 0–899 | 0–499 | 300–749 | 0 | 8 | 4–18 | 12 + radius/2 (×2 night) |
| WILD_LAKE | 0–899 | 0–399 | 996–999 | 0 | 25 | 4–18 | 1 + radius/2 |
| WILD_WASTELAND | 0–99 | 0 | 0 | 0–3 | 0 | 4–18 | 20 + radius/2 |

> `grass` values > 1000 are effectively 100% grass coverage.  
> `dwelling` is the base density used in dwelling placement (see §9).  
> `hotspots` is the number of feature hotspots added per level (see §8).

---

## 6. Wilderness Generation Procedure

`wilderness_gen(Depth)` runs in this order:

1. **Boundary walls:** All four edges are set to `FEAT_PERM_CLEAR` (invisible permanent walls — keeps algorithms happy without blocking visuals).

2. **Terrain generation** (`wilderness_gen_hack`):
   - Seed simple RNG: `seed_town + Depth * 600`
   - Determine terrain type (§4)
   - Initialize terrain parameters (§5)
   - Fill entire interior (rows 1–64, cols 1–196) with `terrain_spot()` per tile
   - Blend boundaries with neighboring levels (`bleed_with_neighbors`, §7)
   - Reseed: `seed_town + Depth * 287 + 490836`
   - Add `terrain.hotspot` hotspots (§8)
   - Apply near-town dwelling multipliers:
     - radius 1: `dwelling × 100`
     - radius 2: `dwelling × 20`
     - radius 3: `dwelling × 3`
   - Optionally reserve a "park" plot at radius 1 (50% chance: 15–44 wide, 10–29 tall)
   - Add dwellings (§9)

3. **Lighting:**
   - Day: set `CAVE_GLOW` on all tiles
   - Night: remove `CAVE_GLOW` from non-room tiles

4. **Monster placement:** spawn `wild_info[Depth].type` monsters using terrain-appropriate filter (§13).

5. **Flag the level:** `WILD_F_GENERATED | WILD_F_INHABITED`.

On subsequent visits, terrain is rebuilt from the same seed (deterministic), but objects/monsters are *not* re-placed if already flagged.

---

## 7. Terrain Bleeding (Border Blending)

Adjacent wilderness tiles with different biomes are blended at their shared border using `bleed_with_neighbors()` and `wild_bleed_level()`.

### Bleed Decision

Whether a tile bleeds into its neighbor is determined by seeding with `seed_town + (Depth + neigh_idx) * 93754` and checking `randint0(2)`. The tile with the lower (more-negative) index absorbs the bleed from the tile with the higher (less-negative) index. Only tiles of different terrain types bleed.

### Bleedmap Generation (Fractal)

`wild_gen_bleedmap()` generates a 257-element array using **midpoint displacement**:

- Initial endpoints: random or specified values
- Noise magnitude per recursion step:
  - N/S borders: `±70 * span / 64`
  - E/W borders: `±25 * span / 64`
- Minimum bleed depth: **8 tiles** from any edge (enforced by clamping)
- If both endpoints are zero, a bump is added in the middle to ensure interesting shapes

The result controls how many tiles of the neighboring biome "leak" into this level at each column/row position, producing an organic, fractal-shaped border rather than a straight line.

### Shared Corner Points

Where three different biomes meet, corner bleed points are negotiated between the two neighbors and shared, preventing discontinuities at level corners.

---

## 8. Hotspots

After base terrain and bleed, a number of **hotspots** are stamped onto the level. Each is an approximately elliptical region (squashed 4:1 vertically) of contrasting terrain.

**Hotspot count:** `randint0(randint0(randint0(max_distance)))` — triple-random, so most hotspots are small, with rare large ones. The minimum radius is 3 tiles. The `hotspot` parameter in the terrain table is the maximum number of hotspots attempted.

**Hotspot terrain by biome:**

| Host biome | Hotspot type | Probability |
|------------|-------------|-------------|
| GRASSLAND | Pond (water=1000) | 50% |
| GRASSLAND | Glade (dense trees) | 50% |
| FOREST | Pond (water=1000) | 60% |
| FOREST | Clearing (sparse trees); if radius>8: 25% dwelling | 40% |
| DENSEFOREST | Nothing (skip) | 80% |
| DENSEFOREST | Pond (water=1000) | 14% |
| DENSEFOREST | Rare clearing; if radius>8: 50% dwelling | 6% |
| SWAMP | Pond (water=1000) | 40% |
| SWAMP | Mud pit (heavy mud) | 60% |
| LAKE | Island (GRASSLAND biome terrain) | 100% |
| WASTELAND / other | Evil tree patch (100–899/1000) | 100% |

---

## 9. Wilderness Dwellings

Dwellings are placed by repeatedly checking `randint0(1000) < terrain.dwelling`, decrementing by 50 each iteration until dwelling reaches 0:

```c
while (terrain.dwelling > 0) {
    if (randint0(1000) < terrain.dwelling)
        wild_add_dwelling(Depth, -1, -1);
    terrain.dwelling -= 50;
}
```

**Expected dwelling counts (base, before radius multiplier):**

| Biome | dwelling value | Expected count |
|-------|---------------|----------------|
| GRASSLAND | 100 | ~0.15 per level |
| FOREST | 37 | ~0.05 per level |
| LAKE | 25 | ~0.04 per level |
| DENSEFOREST | 8 | ~0.01 per level |
| SWAMP | 8 | ~0.01 per level |
| WASTELAND | 0 | Never |

**Radius multipliers** dramatically increase density near town:

| Radius | Multiplier | Grassland expected count |
|--------|------------|-------------------------|
| 1 | ×100 | ~15 dwellings |
| 2 | ×20 | ~3 dwellings |
| 3 | ×3 | ~0.45 dwellings |

### Dwelling Size

`wild_add_dwelling()` uses the **simple RNG** seeded consistently per dwelling:

**Large house** (50% chance):
- Width: `9 + randint0(10) + randint0(randint0(10))`
- Height: `6 + randint0(5) + randint0(randint0(5))`

**Normal house** (50% chance):
- Width: `3 + randint0(10)`
- Height: `3 + randint0(5)`

**Moat chance** (based on interior area = `(w-2) × (h-2)`):

| Area | Moat chance |
|------|-------------|
| ≥ 70 | 1/16 |
| ≥ 80 | 1/6 |
| ≥ 100 | 1/2 |
| ≥ 130 | 3/4 |

When a moat is present, the plot expands by +8 tiles in each dimension and a `FEAT_DRAWBRIDGE` path is placed from door to outside.

### Dwelling Type Probabilities

| Type | Base probability | Near radius 1 | Near radius 2 |
|------|-----------------|---------------|---------------|
| `WILD_LOG_CABIN` (log walls, `FEAT_LOGS`) | 60% | 80% → ROCK_HOME, then 90% → TOWN_HOME | 80% → TOWN_HOME |
| `WILD_ROCK_HOME` (granite walls) | ~37% | → mostly TOWN_HOME | → mostly TOWN_HOME |
| `WILD_PERM_HOME` (permanent walls) | ~3.2% | → TOWN_HOME | → TOWN_HOME |
| `WILD_TOWN_HOME` (for-sale house) | 0% (base) | ~90% of all | ~80% of all |
| `WILD_ARENA` (PvP arena) | if area≥70, 30% of non-moat large | rare | rare |

**Door locking probabilities by type:**

| Type | Locked |
|------|--------|
| LOG_CABIN | 33% locked (1–7) |
| ROCK_HOME | 60% locked (1–7) |
| PERM_HOME | 90% locked (1–7) |
| TOWN_HOME | `FEAT_HOME_HEAD` (player-owned door) |

**House price (TOWN_HOME):**
```
if area > 40: price += (area-40)^3 × 3
price += area^2 × 33
price += area × (900 + randint0(200))
```

---

## 10. Dwelling Contents

`wild_furnish_dwelling()` populates a dwelling's interior:

**Inhabited check:** 75% of all dwellings are inhabited.

### Log Cabin / Rock Home
- Farming: Log Cabin 50% chance, Rock Home 40% chance of adding a garden (§11) near the dwelling.
- No additional contents generated.

### Permanent Home (inhabited)
| Content | Probability | Amount |
|---------|-------------|--------|
| Owner present | 80% | 1 NPC monster (level = radius) |
| Food items | 80% | `randint0(randint0(20))` items |
| Gold | 40% | Level = `randint0(10)` |
| Objects | 50% | `randint0(randint0(10))` items at level `radius/2+1` |

### Permanent Home (uninhabited, 50% chance of takeover)
| Content | Probability | Amount |
|---------|-------------|--------|
| Gold | 40% | Level = `randint0(20)` |
| Objects | 50% | `randint0(randint0(10))` items |
| Food | 33% | `randint0(3)` items |
| Bones (skeletons) | Always | `randint0(20)` items at base level 10 |
| Monster invaders | Always | Fill every floor tile (level = `radius/2+1`) |

**Owner NPC species:** humanoids/players/giants (`hpP`).  
**Invader species:** orcs/trolls/persons/ogres/kobolds/brigands/monsters (`oTpOKbrm`), plus dark elven mages/priests/warriors.

> The RNG is seeded consistently before content generation, then restored — so dwelling contents are deterministic based on `seed_town` and the dwelling's coordinates.  
> If `WILD_F_GENERATED` is already set, no new objects are placed (prevents duplication on re-entry).

---

## 11. Crops and Gardens

Gardens are rectangular plots of loose dirt with alternating rows of crops.

### Garden Dimensions
- Width: `randint0(randint0(60)) + 15` (15–74 tiles)
- Height: `randint0(randint0(20)) + 7` (7–26 tiles)

### Garden Layout
- Outer ring: `FEAT_LOOSE_DIRT`
- Inner tiles alternate rows (horizontal or vertical orientation, 50/50):
  - Crop rows: `FEAT_CROP` — 40% chance of immediate food spawn on each tile
  - Dirt rows: `FEAT_LOOSE_DIRT`

### Crop Types
| Type | Produces |
|------|---------|
| `WILD_CROP_POTATO` | Potato |
| `WILD_CROP_CABBAGE` | Head of Cabbage |
| `WILD_CROP_CARROT` | Carrot |
| `WILD_CROP_BEET` | Beet |
| `WILD_CROP_SQUASH` | Squash |
| `WILD_CROP_CORN` | Ear of Corn |
| `WILD_CROP_MUSHROOM` | Random food (`lookup_kind(TV_FOOD, randint0(randint0(20)))`) |

Crop type is chosen randomly when a generic `FEAT_CROP` grows, or inherits from planted seed.

### Crop Growth
`wild_grow_crops()` is called every server tick for all wilderness levels:
- Chance per crop tile per tick: **1 in (10000 × cfg_fps)**
- If a monster/player is on the tile: skip
- If already generated (`WILD_F_GENERATED`) and fails `randint0(16) < 1`: skip (1/16 regen chance)

### Player Planting
`do_cmd_plant_seed()` — players can plant food items as seeds. Spends all energy. Converts TV_FOOD items to `FEAT_CROP_HEAD + crop_type` on the current tile.

---

## 12. Day/Night Cycle

| Constant | Value |
|----------|-------|
| `TOWN_DAWN` | 50,000 turns |
| Full cycle | 500,000 turns (`10 × TOWN_DAWN`) |

```c
IS_DAY   = (turn % 500000) <= 250000
IS_NIGHT = (turn % 500000) >  250000
```

Day/night transitions occur every **250,000 turns** (`TOWN_DAWN / 2 × 10`).

### Effects on Wilderness
| Condition | Effect |
|-----------|--------|
| Day | All tiles get `CAVE_GLOW` |
| Night | Non-room tiles lose `CAVE_GLOW` (darkness) |
| Night in DENSEFOREST | `monster_lev += 10` |
| Night in SWAMP | `monster_lev × 2` |

### Effects on Town
- Day: all tiles get `CAVE_GLOW` (fully lit town)
- Night: town is dark (no auto-lighting)

---

## 13. Wilderness Monster Spawning

Monsters are added once when the level is first generated (unless `WILD_F_INHABITED` is set).

**Number of monsters spawned:** `wild_info[Depth].type` (the terrain type integer value):

| Biome | Type value | Monsters spawned |
|-------|-----------|-----------------|
| WILD_LAKE | 0 | 0 (no monsters!) |
| WILD_GRASSLAND | 1 | 1 |
| WILD_FOREST | 2 | 2 |
| WILD_WASTELAND | 7 | 7 |
| WILD_TOWN | 10 | 10 |
| WILD_DENSEFOREST | 15 | 15 |
| WILD_SWAMP | 20 | 20 |

Each monster placement: up to 50 attempts to find a `cave_naked_bold()` tile. The selection hook is set to match the biome:

| Biome | Monster filter |
|-------|---------------|
| WILD_LAKE | No multipliers; animals OR humanoids (`ph`) |
| WILD_GRASSLAND | Animals, most humanoids/creatures, town monsters |
| WILD_FOREST | Snakes(`J`), wolves(`C`), beetles(`K`), felines(`f`), wood spiders, rangers, druids, forest wights/trolls, dark elven druids, mystics |
| WILD_SWAMP | Worms(`J`), worm masses(`w`), mushrooms(`,`), flies(`F`), ghosts(`G`), imps(`I`), liches(`L`), molds(`m`), quylthulgs(`Q`), roosters(`R`), snakes(`S`), vampires(`V`), wraiths(`W`), centipedes(`c`), small creatures, dark elven mages/lords/druids |
| WILD_DENSEFOREST | Forest trolls, Mirkwood spiders, forest wights, dark elven druids only |
| WILD_WASTELAND | All large/powerful monster letters (`ABCDEFHLMOPTUVWXYZdefghopqv`), town monsters |

Monster level = `terrain.monst_lev` (base, modified by radius and day/night).

---

## 14. Town Generation

Town is always at depth 0. `town_gen()` runs:

1. **Boundary walls:** All edges set to `FEAT_PERM_CLEAR` or `FEAT_PERM_SOLID` (server config `cfg_town_wall`).
2. **Town hack:** `town_gen_hack()` uses the **simple RNG** seeded with `seed_town` — fully deterministic.
3. **Lighting:** If daytime, all tiles get `CAVE_GLOW`.

### Base Terrain (town_gen_hack)
- Fill all interior tiles with `FEAT_DIRT`
- Each tile: random chance of `FEAT_TREE` (controlled by `cfg_max_trees` limit)
- Then 75% chance each remaining tile becomes `FEAT_GRASS`

### Streets
**3 horizontal streets** at `y = place*22 + 10` for place = 0, 1, 2:
- Full width: 5 tiles (±2 of center) = `FEAT_GRASS`
- Inner width: 3 tiles (±1 of center) = `FEAT_FLOOR`

**6 vertical streets** at `x = place*32 + 20` for place = 0–5:
- Full width: 5 tiles = `FEAT_GRASS`
- Inner width: 3 tiles = `FEAT_FLOOR`

---

## 15. Town Layout

The town is subdivided into a **6 rows × 12 columns** grid of building slots. Each slot becomes a building placed by `build_store(n, yy, xx)`.

**Building coordinates per slot:**
- `y0 = yy * 11 + 5`, `x0 = xx * 16 + 12`
- Building bounds vary by `±1–3` rows, `±1–5` columns from center

### Room Array

```
rooms[0..7]   = stores 0–6 (General Store, Armory, Weapon Shop, Temple,
                             Alchemist, Magic Shop, Black Market) + Tavern (7)
rooms[8..15]  = 8 × Pond (type 10)
rooms[16..67] = 52 × Purchasable House (type 13)
rooms[68..70] = 3 × Forest (type 12)
rooms[71]     = 1 × Dungeon Entrance (type 11)
```

**Placement:** Center 2-row × 4-column block (rows 2–3, cols 4–7) is filled first with the 8 main buildings (shuffled). The remaining 64 slots are filled with the remaining buildings (shuffled from rooms[8..71]).

### Building Types

| Type | Description |
|------|-------------|
| 0–6 | Shops: `FEAT_SHOP_HEAD + n` door, permanent walls |
| 7 | Tavern: hollow `FEAT_FLOOR` interior, `CAVE_ROOM|CAVE_GLOW|CAVE_ICKY`, player spawn point |
| 9 | Park: grass interior, up to 5 trees |
| 10 | Pond: all `FEAT_WATER`, corners replaced with `FEAT_DIRT` |
| 11 | Dungeon entrance: mix of floor/grass with `FEAT_MORE` staircase in center |
| 12 | Forest: grass base, tree density proportional to distance from center |
| 13 | Purchasable house: `FEAT_FLOOR` interior, `CAVE_ICKY`, `FEAT_HOME_HEAD` door, price = `area * 20 * (80 + randint1(40))` |
| 14 | Auction house: `FEAT_PERM_EXTRA` door |

**Black Market back room:** the door on the opposite wall gets `FEAT_SHOP_HEAD + 8` (currently disabled).

---

## 16. Dungeon Generation

`cave_gen(Depth)` generates dungeon levels 1–127.

### Initial State
- All tiles: `FEAT_WALL_EXTRA` (granite)
- **Destroyed level check:** if `Depth > 10` and `randint0(DUN_DEST) == 0` (1/15 chance), flag as destroyed

### Generation Constants

| Constant | Value | Meaning |
|----------|-------|---------|
| `DUN_ROOMS` | 50 | Room attempts per level |
| `DUN_UNUSUAL` | 200 | Level/chance threshold for unusual rooms |
| `DUN_DEST` | 15 | 1/chance of destroyed level |
| `DUN_TUN_RND` | 10 | % chance of random tunnel direction |
| `DUN_TUN_CHG` | 30 | % chance of direction change per step |
| `DUN_TUN_CON` | 15 | % chance of early tunnel termination |
| `DUN_TUN_PEN` | 25 | % chance of door at room entrance |
| `DUN_TUN_JCT` | 90 | % chance of door at tunnel junction |
| `DUN_STR_DEN` | 5 | Streamer density (tiles per step) |
| `DUN_STR_RNG` | 2 | Streamer width (spread radius) |
| `DUN_STR_MAG` | 3 | Number of magma streamers |
| `DUN_STR_MC` | 90 | 1/chance of treasure per magma tile |
| `DUN_STR_QUA` | 2 | Number of quartz streamers |
| `DUN_STR_QC` | 40 | 1/chance of treasure per quartz tile |
| `DUN_AMT_ROOM` | 9 | Mean objects placed in rooms |
| `DUN_AMT_ITEM` | 3 | Mean objects placed anywhere |
| `DUN_AMT_GOLD` | 3 | Mean gold placed anywhere |
| `MIN_M_ALLOC_LEVEL` | 14 | Minimum monsters on dungeon level |
| `BLOCK_HGT` | 11 | Room block height in tiles |
| `BLOCK_WID` | 11 | Room block width in tiles |

### Room Block Grid
The dungeon is divided into 11×11 blocks: `MAX_ROOMS_ROW = 6`, `MAX_ROOMS_COL = 18` → 108 total blocks.

### Generation Sequence

1. Fill with `FEAT_WALL_EXTRA`
2. Check destroyed flag
3. Attempt `DUN_ROOMS = 50` room placements:
   - Pick random block `(y, x)`
   - Optionally align: if `(x % 3) == 0`: `x++`; if `(x % 3) == 2`: `x--`
   - If destroyed: attempt Type 1 only
   - If unusual (`randint0(200) < Depth`):
     - If very unusual (`randint0(200) < Depth`):
       - `k < 10` (10%): Type 8 — Greater Vault (min depth 10)
       - `k < 25` (15%): Type 7 — Lesser Vault (min depth 5)
       - `k < 40` (15%): Type 6 — Monster Pit (min depth 5)
       - `k < 50` (10%): Type 5 — Monster Nest (min depth 5)
     - `k < 25` (25%): Type 4 — Large Room (min depth 3)
     - `k < 50` (25%): Type 3 — Cross Room (min depth 3)
     - `k < 100` (50%): Type 2 — Overlapping
   - Otherwise: Type 1 — Simple
4. Solid perma-walls on all four borders (`FEAT_PERM_SOLID`)
5. Scramble room center array randomly
6. Connect rooms in circular order with `build_tunnel()`
7. Place doors at intersections (`try_door`)
8. Add `DUN_STR_MAG = 3` magma streamers (1/90 treasure)
9. Add `DUN_STR_QUA = 2` quartz streamers (1/40 treasure)
10. If destroyed: `destroy_level()`
11. Stairs: `rand_range(3,4)` down-stairs, `rand_range(1,2)` up-stairs
12. Player spawn: random naked floor tile (no CAVE_ICKY)
13. Objects, gold, traps, rubble (§22)

---

## 17. Room Types

### Type 1 — Simple Rectangular
- Size: `y1 = yval − randint1(4)`, `y2 = yval + randint1(3)`, `x1 = xval − randint1(11)`, `x2 = xval + randint1(11)`
- Lit if `Depth ≤ randint1(25)`
- 1/20 chance: **pillar room** — inner walls at every even `(y, x)` pair
- 1/50 chance: **ragged edges** — inner walls on alternating perimeter positions

### Type 2 — Overlapping Rectangles
Two overlapping rooms, each up to 11 wide and 4 tall, sharing the same center. Creates L-, T-, or cross-like shapes.

### Type 3 — Cross Shaped
North-south arm (1–2 wide, 6–8 tall) crossing east-west arm (1–2 wide, 6–22 wide). Center intersection is always 3×3.

**Special center features (3/4 of Type 3 rooms):**

| Roll | Feature |
|------|---------|
| 0 (25%) | Nothing special |
| 1 (25%) | Solid 3×3 inner wall pillar |
| 2 (25%) | Inner treasure vault: inner wall ring, 1 secret door, 1 object, `randint0(2)+3` monsters, `randint0(3)+2` traps |
| 3 (25%) | Pinched center (1/3), plus-shaped wall (1/3), or center pillar (1/3); optionally sealed with secret doors |

### Type 4 — Large Room with Inner Features
Outer room: 9 rows × 23 cols fixed. Double-walled. Inner room: 5×19.

**Sub-types (equal probability):**

| Sub-type | Contents |
|----------|---------|
| 1 | Secret door + 1 monster |
| 2 | Secret door → inner room with locked door; `randint1(3)+2` monsters; 80% object / 20% stairs; `2+randint1(3)` traps |
| 3 | Large center pillar (3×3); possibly 2 flanking pillars; possibly side rooms with `randint1(2)` monsters and 1/3 chance object each |
| 4 | Checkerboard maze (inner walls at `(x+y) & 1`); 3 monsters left and right; 3+3 traps; 3 objects |
| 5 | Four quadrant rooms separated by a cross wall; secret doors into each; 2+ objects in center; 1–4 monsters per quadrant |

### Type 5 — Monster Nest
Double room, center inner room with secret door on one side.

**Nest type (roll = `randint1(Depth)`):**

| Roll | Nest | Requirement |
|------|------|-------------|
| < 30 | Jelly (`ijm,`) | Any depth |
| < 50 | Animal (RF3_ANIMAL) | Any depth |
| ≥ 50 | Undead (RF3_UNDEAD) | Any depth |

- Picks 64 candidate monsters at `Depth + 10`
- Places random selections across a 5×19 inner grid
- No unique monsters; adds `+10` rating

### Type 6 — Monster Pit
Same double room structure as nest.

**Pit type (roll = `randint1(Depth)`):**

| Roll | Pit |
|------|-----|
| < 20 | Orcs (`o`) |
| < 40 | Trolls (`T`) |
| < 60 | Giants (`P`) |
| < 80 | Dragons — one random breath type (acid/elec/fire/cold/poison/multi) |
| ≥ 80 | Demons (`U`) |

- Picks 16 monsters at `Depth + 10`, sorts by level, uses the 8 even-indexed ones
- Arranged in symmetric pattern (0=weakest outer ring, 7=strongest center):

```
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
0 1 1 2 2 3 3 4 5 5 4 3 3 2 2 1 1 0
0 1 1 2 2 3 3 4 6 7 6 4 3 3 2 2 1 1 0
0 1 1 2 2 3 3 4 5 5 4 3 3 2 2 1 1 0
0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
```
- No unique monsters; adds `+10` rating

### Types 7 & 8 — Vaults (from v_info.txt)

Vaults are pre-designed room templates loaded from `v_info.txt`. Type 7 = lesser vault (min depth 5), Type 8 = greater vault (min depth 10).

**Vault grid characters:**

| Char | Meaning |
|------|---------|
| (space) | Skip tile entirely |
| `%` | Outer granite wall |
| `#` | Inner granite wall |
| `X` | Permanent inner wall |
| `*` | 75% object, 25% trap |
| `+` | Secret door |
| `^` | Trap |
| `&` | Monster at `Depth + 5` |
| `@` | Monster at `Depth + 11` |
| `9` | Monster at `Depth + 9` + good object at `Depth + 7` |
| `8` | Monster at `Depth + 40` + great object at `Depth + 20` |
| `,` | 50% monster at `Depth + 3`; 50% object at `Depth + 7` |

All vault grids are marked `CAVE_ROOM | CAVE_ICKY`.

---

## 18. Tunnel Generation

`build_tunnel(row1, col1, row2, col2)` carves corridors between room centers.

### Direction Control
- Each step: `DUN_TUN_CHG = 30%` chance to recompute correct direction
  - Within that: `DUN_TUN_RND = 10%` chance to pick fully random direction instead
- If stuck at map edge: force-correct direction

### Wall Behavior
| Wall type encountered | Action |
|----------------------|--------|
| `FEAT_PERM_SOLID` | Never pierce (dungeon border) |
| `FEAT_PERM_OUTER` | Never pierce (vault edge) |
| `FEAT_WALL_SOLID` | Never pierce (corner guard) |
| `FEAT_WALL_OUTER` | Pierce (room entrance); record in wall array; convert nearby outer walls to solid to prevent adjacent piercings |
| `CAVE_ROOM` flag | Travel through freely |
| `FEAT_WALL_EXTRA` | Tunnel through; record in tunnel array |
| Floor/corridor | Accept; record as potential door location; `DUN_TUN_CON = 15%` early termination if tunnel has traveled >10 tiles |

### Post-Processing
- **Tunnel array** → all become `FEAT_FLOOR`
- **Wall array** → become `FEAT_FLOOR`, then `DUN_TUN_PEN = 25%` chance to place a door

---

## 19. Streamers

Streamers are veins of ore that run diagonally across the dungeon.

- Start at approximately `MAX_HGT/2 ± 10` rows, `MAX_WID/2 ± 15` cols
- Advance in a random cardinal direction each step
- At each step, paint `DUN_STR_DEN = 5` tiles within `DUN_STR_RNG = 2` tiles of current position
- Only converts `FEAT_WALL_EXTRA` through `FEAT_WALL_SOLID` (not floors, rooms, or permanent walls)
- `randint0(chance) == 0` → add known gold to the vein tile (feat += 4)

| Streamer | Count | Treasure chance |
|----------|-------|----------------|
| Magma (`FEAT_MAGMA`) | 3 | 1/90 per tile |
| Quartz (`FEAT_QUARTZ`) | 2 | 1/40 per tile |

---

## 20. Destroyed Levels

If a level is flagged for destruction (`randint0(15) == 0` at depth > 10, and not a quest level):

- `randint1(5)` epicenters are chosen
- Each epicenter affects a 16-tile radius circle
- Per tile in circle: delete monster and object, then:

| Roll (`randint0(200)`) | Feature placed |
|------------------------|---------------|
| < 20 (10%) | `FEAT_WALL_EXTRA` (granite) |
| < 70 (25%) | `FEAT_QUARTZ` |
| < 100 (15%) | `FEAT_MAGMA` |
| ≥ 100 (50%) | `FEAT_FLOOR` |

- `CAVE_ROOM` and `CAVE_ICKY` flags cleared; `CAVE_GLOW` cleared

Destroyed levels only allow Type 1 (simple) rooms during generation.

---

## 21. Stairs and Player Placement

### Stair Placement
`alloc_stairs(Depth, feat, num, walls)`:
- Requires a `cave_naked_bold()` tile with ≥ `walls` adjacent walls
- Up to 3000 attempts per stair; decrements `walls` requirement if no spot found
- Records `level_down_y/x[Depth]` (where players arriving from above land) and `level_up_y/x[Depth]`

**Stair counts:**

| Depth | Up stairs | Down stairs |
|-------|-----------|-------------|
| 0 (town) | 0 | 1 (always down) |
| Quest / MAX_DEPTH-1 | 1–2 (always up) | 0 |
| Normal | 1–2 up (`rand_range(1,2)`) | 3–4 down (`rand_range(3,4)`) |

**Random stair type** (non-quest, non-town): 50% up, 50% down.

### Player Spawn
`new_player_spot()`: random naked floor tile that is not `CAVE_ICKY`. Saved in `level_rand_y/x[Depth]`.

---

## 22. Dungeon Object and Monster Placement

After room and tunnel generation, objects and creatures are scattered:

**Scaling factor `k`:**
```c
k = Depth / 3
k = clamp(k, 2, 10)
```

| Content | Placement | Count |
|---------|-----------|-------|
| Monsters | Anywhere | `MIN_M_ALLOC_LEVEL(14) + randint1(8) + k` |
| Traps | Rooms + corridors | `randint1(k)` |
| Rubble | Corridors only | `randint1(k)` |
| Objects (rooms) | Rooms only | `randnor(DUN_AMT_ROOM=9, 3)` |
| Objects (general) | Anywhere | `randnor(DUN_AMT_ITEM=3, 3)` |
| Gold | Anywhere | `randnor(DUN_AMT_GOLD=3, 3)` |

`randnor(mean, std)` returns a normally-distributed value, so object counts vary naturally around the mean.

**Vault objects** (`vault_objects`): up to 11 attempts per object within ±2y/±3x of the vault center. 75% object, 25% gold.

---

## 23. Door Placement Probabilities

### `place_random_door()` (room piercings at tunnel entry, vault doors)

| Roll (`randint0(1000)`) | Door type |
|------------------------|-----------|
| < 300 | Open (`FEAT_OPEN`) |
| < 400 | Broken (`FEAT_BROKEN`) |
| < 600 | Secret (`FEAT_SECRET`) |
| < 900 | Closed (`FEAT_DOOR_HEAD + 0`) |
| < 999 | Locked strength 1–7 |
| 999 | Stuck (jammed) strength 8–15 |

### `place_closed_door()` (corridor junctions)

| Roll (`randint0(400)`) | Door type |
|-----------------------|-----------|
| < 300 (75%) | Closed |
| < 399 (24.75%) | Locked strength 1–7 |
| 399 (0.25%) | Stuck strength 8–15 |

### Junction Doors (`try_door`)
Only placed if:
1. `randint0(100) < DUN_TUN_JCT (90)`
2. Grid has ≥ 2 adjacent corridor grids AND is flanked by walls on one axis

---

## 24. Level Rating and Feelings

During generation, a `rating` counter is incremented by special features:

| Feature | Rating added |
|---------|-------------|
| Monster Nest (Type 5) | +10 |
| Monster Pit (Type 6) | +10 |
| Lesser Vault (Type 7) | vault's `rat` value |
| Greater Vault (Type 8) | vault's `rat` value |

The `good_item_flag` is set when vaults or pits appear on shallow levels.

**Feeling assigned from rating:**

| Rating | Feeling index |
|--------|--------------|
| > 100 | 2 (Superb) |
| > 80 | 3 |
| > 60 | 4 |
| > 40 | 5 |
| > 30 | 6 |
| > 20 | 7 |
| > 10 | 8 |
| > 0 | 9 |
| 0 | 10 (Boring) |
| `good_item_flag` | 1 (Special) |

Feeling is suppressed if fewer than 1000 game turns have passed since last feeling, or on town level.

**Auto-scum** (if enabled): regenerates the level if feeling exceeds thresholds by depth:
- Depth ≥ 40: retry if feeling > 5
- Depth ≥ 20: retry if feeling > 6
- Depth ≥ 10: retry if feeling > 7
- Depth ≥ 5: retry if feeling > 8
- Any depth: retry if feeling > 9
- Maximum 100 retries

---

## 25. Stores: Structure and Constants

### Store List

| Index | Store | Items Sold / Notes |
|-------|-------|-------------------|
| 0 | General Store | Food, torches, flasks, ammo, cloaks, shovels |
| 1 | Armory | All armor types, shields |
| 2 | Weapon Shop | Swords, polearms, hafted, bows, ammo |
| 3 | Temple | Hafted weapons, prayer books, scrolls, potions |
| 4 | Alchemist | Scrolls and potions only |
| 5 | Magic Shop | Magic books, rings, amulets, rods, staves, wands, scrolls, potions |
| 6 | Black Market | Random high-level items (special rules) |
| 7 | Tavern | Player spawn point; not a real store |
| 8 | Player-owned Shop | Player houses used as stores |

### Constants

| Constant | Value | Meaning |
|----------|-------|---------|
| `MAX_STORES` | 9 | Total store slots |
| `MAX_OWNERS` | 4 | Different shopkeeper personalities per store type |
| `STORE_INVEN_MAX` | 48 | Maximum item slots per store |
| `STORE_CHOICES` | 32 | Items in each store's preset selection table |
| `STORE_OBJ_LEVEL` | 7 | Magic application level for normal store items |
| `STORE_TURNOVER` | 9 | Items bought/sold per maintenance cycle |
| `STORE_MIN_KEEP` | 12 | Minimum item slots always kept filled |
| `STORE_MAX_KEEP` | 36 | Maximum item slots kept after turnover |
| `STORE_SHUFFLE` | 35 | 1/35 chance per cycle that owner changes |
| `STORE_TURNS` | 250 | Server turns between maintenance cycles |

---

## 26. Store Inventory Tables

Each normal store (0–5) has a hardcoded table of 32 tval/sval pairs. Examples:

**General Store (store 0):** Food rations ×5, biscuits, jerky ×2, wine, ale, torches ×4, lanterns ×2, oil flasks ×6, arrows ×2, shots, bolts ×2, shovel, pick, cloaks ×3.

**Armory (store 1):** Boots (soft/hard ×2 each), helms (leather cap ×2, metal cap, iron helm), robes ×2, leather armors ×4, studded leather ×2, scale mail ×2, metal scale, chain mail ×2, augmented chain, bar chain, double chain, brigandine, leather gloves ×2, gauntlets, shields (small leather ×2, large leather, small metal).

**Weapon Shop (store 2):** Daggers through bastard swords, spears/polearms, battle axe, whip, sling, bows, crossbow, ammo ×6.

**Temple (store 3):** Blunt weapons (whip to flail), prayer books (books 1–4 ×2–3 each), potions of cure wounds ×4, potions of boldness/heroism, scrolls of remove curse/protection.

**Alchemist (store 4):** Potions of cure light/serious/critical ×4–5, restore life levels, scrolls of teleportation/word of recall ×3, identify ×4.

**Magic Shop (store 5):** Arcane magic books (books 1–4 ×2–3 each), rings (searching, levitation, protection, open wounds), amulets, rods of trap/door detection, staff of light, wand of slow monsters, scrolls/potions.

The Black Market (store 6) does **not** use a preset table — it generates random items at level `30 + randint0(25)` (range 30–54).

---

## 27. Store Maintenance and Restocking

Stores are maintained every `STORE_TURNS = 250` server turns (in `process_various()` inside `dungeon()`):

```c
if (!(turn % (10 × 250))) {
    for each store 0..MAX_STORES-1: store_maint(n)
    if (randint0(35) == 0): store_shuffle(randint0(MAX_STORES-2))
}
```

### `store_maint(which)`

1. Skip Tavern (7) and Player Shop (8)
2. Skip if any player is currently in the store
3. Black Market: delete all items that are available in regular stores 0–5
4. **Deletion phase:**
   - Target = `current_stock − randint1(STORE_TURNOVER = 9)`
   - Clamp to `[STORE_MIN_KEEP=12, STORE_MAX_KEEP=36]`
   - Delete items randomly until target reached
5. **Replenishment phase:**
   - Target = `current_stock + randint1(STORE_TURNOVER = 9)`
   - Clamp to `[STORE_MIN_KEEP=12, STORE_MAX_KEEP=36]`
   - Create new items until target reached

### `store_shuffle(which)`
Changes the shopkeeper owner (picks a different one from the 4 available). Puts existing stock "on sale" (50% discount, 1/10 per item, `note = "on sale"`). Also resets insult counter and re-opens the store.

### When Store Empties (After Player Purchase)
If a player buys the last item, the store immediately:
1. 1/`STORE_SHUFFLE(35)` chance: `store_shuffle()` + announce "shopkeeper retires"
2. Otherwise: announce "new stock" and run `store_maint()` × 10

---

## 28. Store Item Creation

`store_create(st)` attempts to generate an item up to 4 times:

**Normal stores (0–5):**
- Pick index from the 32-item preset table
- Apply magic at level `rand_range(1, STORE_OBJ_LEVEL=7)` — no artifacts, no ego boost
- Reject: chests, worthless items (value ≤ 0)

**Black Market (store 6):**
- 1/25 chance: use normal random object path (rare)
- 24/25 chance: get random object at level `30 + randint0(25)`
- Apply magic with good-quality boost
- Reject: chests, items with value < 40, "crappy" items (already in regular stores without ego/bonus)

**Common to all:** if `TV_LITE`: set charges to 50% of max (`FUEL_TORCH/2` or `FUEL_LAMP/2`). Mark item as "known". Apply `mass_produce()`.

---

## 29. Mass Production and Discounts

`mass_produce()` determines pile size and discount for store items:

### Pile Size by Item Type

| Item type | Cost ≤ 5 | Cost ≤ 20/50/60 | Cost ≤ 100/240/500 |
|-----------|----------|-----------------|-------------------|
| Food, Flask, Lite | +`mass_roll(3,5)` | +`mass_roll(3,5)` | — |
| Potion, Scroll | — | +`mass_roll(3,5)` | +`mass_roll(1,5)` |
| Magic/Prayer Book | — | +`mass_roll(2,3)` | +`mass_roll(1,3)` |
| Armor, Weapon (no ego) | +`mass_roll(3,5)` | +`mass_roll(3,5)` | — |
| Ammo (shots/arrows/bolts) | +`mass_roll(5,5)` | +`mass_roll(5,5)` | +`mass_roll(5,5)` |

`mass_roll(n, max)` = sum of `n` rolls of `randint0(max)`.

**Final pile count:** `size − (size × discount / 100)`

### Discount Probabilities

| Probability | Discount |
|-------------|---------|
| 1/50 (2%) | 25% off |
| 1/300 (0.33%) | 50% off |
| 1/600 (0.17%) | 75% off |
| 1/1000 (0.1%) | 90% off |
| Otherwise | No discount |

Items with base cost < 5 never receive discounts.

---

## 30. Store Pricing

`price_item(p_ptr, o_ptr, greed, flip)`:

```
base_price = object_value()
factor = g_info[owner_race × p_max + player_race]    // racial adjustment 95–130
factor += adj_chr_gold[CHR_stat_index]                // charisma adjustment 80–130

if buying (flip=FALSE):
    adjust = 100 + (greed + factor) − 300    // minimum 100
    if Black Market: base × 3

if selling (flip=TRUE):
    adjust = 100 + 300 − (greed + factor)    // maximum 100
    if Black Market: base ÷ 3

final = (base × adjust + 50) / 100    // rounded
```

**Key behaviors:**
- Charisma and racial modifiers stack multiplicatively through the `adjust` factor
- Black Market sells items at 3× object value; buys at 1/3 value
- Price can never drop to zero (minimum 1 gold)
- Player-owned stores use `price × 3` as base, or asking price if higher

---

## 31. Player-Owned Shops

Houses purchased as `WILD_TOWN_HOME` (store type 8) can be used as player storefronts:
- Owner inscribes items with `"for sale <price>"` to list them
- Optional store name via `"store name <name>"` inscription
- Buyers pay price set in inscription (or `object_value × 3` if lower)
- Seller receives 90% of the listed/computed price when items sell
- Other players cannot enter a house while the owner is restocking (anti-exploit lock)
- Items displayed live from cave grid scan, not a fixed inventory

---

## 32. IronHell Status Summary

| System | MAngband Source | IronHell Status |
|--------|----------------|-----------------|
| Dungeon generation (rooms, tunnels) | `generate.c: cave_gen()` | **Implemented** — block-grid (DUN_ROOMS=50), wandering tunnels, 1–2 up-stairs/3–4 down-stairs (`dungeonGeneration.ts`) |
| Room type 1–8 | `generate.c: build_type1–8()` | **Implemented (types 1–6)** — simple, overlapping, cross, large, nest, pit (`dungeonRooms.ts`); types 7–8 (vaults) pending |
| Streamer veins | `generate.c: build_streamer()` | **Implemented** — quartz/magma/ore streamers (`dungeonFeatures.ts`) |
| Destroyed levels | `generate.c: destroy_level()` | **Implemented** — DUN_DEST_CHANCE=15, radius circles (`dungeonFeatures.ts`) |
| Stair placement | `generate.c: alloc_stairs()` | **Implemented** — 1–2 up, 3–4 down placed in room floors (`dungeonGeneration.ts`) |
| Level rating / feelings | `generate.c: cave_gen()` | **Implemented** — rating from nest/pit rooms, mapped to `levelFeelingMessage` (`dungeonGeneration.ts`) |
| Town generation | `generate.c: town_gen()` | **Implemented** — `createStarterTown()` with 7 shop types, owner pools, initial stock (`town.ts`) |
| Wilderness coordinate system | `wilderness.c: world_index()` | **Implemented** — WILD_MAX=128, WILD_CENTER=64, ring/depth system (`wildernessGeneration.ts`) |
| Wilderness terrain types | `wilderness.c: determine_wilderness_type()` | **Implemented** — 9 terrain types with ring-based distribution (`wildernessGeneration.ts`, `dungeon.ts`) |
| Terrain composition | `wilderness.c: init_terrain()` | **Implemented** — tile grid from terrain params with noise (`wildernessGeneration.ts`) |
| Terrain bleeding | `wilderness.c: bleed_with_neighbors()` | **Implemented** — fractal bleedmap + border stamping, `bleedWildernessLevel()` (`wildernessFeatures.ts`) |
| Hotspots | `wilderness.c: wild_add_hotspot()` | **Implemented** — triple-random count, 4:1 ellipse stamps per biome, `applyWildernessHotspots()` (`wildernessFeatures.ts`) |
| Wilderness dwellings | `wilderness.c: wild_add_dwelling()` | **Implemented** — log/rock/perm/town homes, moat, door locking, `placeDwellingsForLevel()` (`wildernessFeatures.ts`) |
| Dwelling contents | `wilderness.c: wild_furnish_dwelling()` | **Partial** — structural shell, moat, locked doors; NPC/item furnishing deferred (needs monster/item integration) |
| Crops / gardens | `wilderness.c: wild_add_garden()` | **Implemented** — alternating crop/dirt rows, adjacent to dwellings, `placeGardenNearDwelling()` (`wildernessFeatures.ts`) |
| Day/night cycle | `dungeon.c`, `wilderness.c: wild_apply_day/night()` | **Implemented** — TOWN_DAWN/DUSK, isDaytime(), monster level modifiers (`dayNightCycle.ts`) |
| Wilderness monster spawning | `wilderness.c: wild_add_monster()` | **Implemented** — depth/terrain/day-night level scaling (`dayNightCycle.ts`, `wildernessGeneration.ts`) |
| Store structure (8 shop types) | `store.c`, `init2.c` | **Implemented** — 7 shop types with SHOP_CONFIGS and owner pools (`town.ts`) |
| Store preset inventory tables | `init2.c: store_table[]` | **Implemented** — SHOP_STOCK_TABLE with classic starter items (`town.ts`) |
| Store maintenance / turnover | `store.c: store_maint()` | **Implemented** — STORE_TURNS=250, shouldRestockStore(), maintainShopInventory() (`town.ts`) |
| Store shuffle (owner change) | `store.c: store_shuffle()` | **Implemented** — STORE_SHUFFLE_CHANCE=35, shuffleStoreOwner() with price halving (`town.ts`) |
| Store item creation | `store.c: store_create()` | **Implemented** — createListing(), getListingPrice(), rarity-weighted picks (`town.ts`) |
| Mass production / discounts | `store.c: mass_produce()` | **Implemented** — applyMassProductionDiscount() with 25/50/75/90% tiers (`town.ts`) |
| Store pricing (CHA/racial/greed) | `store.c: price_item()` | **Implemented** — getAdjustedTownBuyPrice/SellPrice() with CHA factor (`town.ts`) |
| Black market rules | `store.c: black_market_crap()` | **Implemented** — BLACK_MARKET_MIN_VALUE=40gp, isBlackMarketJunk() filter (`town.ts`) |
| Player-owned shops | `store.c: display_inventory(st==8)` | Not implemented |
