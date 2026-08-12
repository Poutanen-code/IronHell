# IronHell Systems Compendium

> Reference document comparing MAngband 1.5.3 game mechanics to IronHell's implementation.
> Source files: `reference-mangband-1_5_3/src/server/dungeon.c`, `cmd1.c`, `xtra1.c`, `spells1.c`, `pathfind.c`.

---

## Table of Contents

1. [Turn & Energy System](#1-turn--energy-system)
2. [Per-Turn Periodic Processing](#2-per-turn-periodic-processing)
3. [World Processing (Every 50 Turns)](#3-world-processing-every-50-turns)
4. [Auto-Retaliate](#4-auto-retaliate)
5. [Movement](#5-movement)
6. [Running](#6-running)
7. [Interruptions (disturb)](#7-interruptions-disturb)
8. [Searching](#8-searching)
9. [Skills Reference](#9-skills-reference)
10. [Saving Throws](#10-saving-throws)
11. [IronHell Status Summary](#13-IronHell-status-summary)
15. [Stealth & Monster Wake Mechanics](#15-stealth--monster-wake-mechanics)
16. [Ego Item & Artifact Enrichment](#16-ego-item--artifact-enrichment)

---

## 1. Turn & Energy System

### MAngband: How It Works

MAngband uses a **real-time server loop** (`dungeon()` in `dungeon.c`) that ticks at approximately **50 frames per second** (FPS). Each frame runs the following sequence:

```
1. Check for player deaths → player_death()
2. Deallocate empty dungeon levels
3. Handle pending level entries (generate cave, place player)
4. process_various()    — server saves, unique timers, store restocks, day/night cycle
5. For each player: process_player_end()   — consume energy, execute buffered commands
6. For each player: process_player_begin() — award energy for next frame
7. For each player: process_world()        — every 50 turns: ambient, monster spawn
8. process_monsters()  — all monsters take their turns
9. regen_monsters()    — every 100 turns (scaled by time bubble)
```

**Important:** `process_player_end()` runs *before* `process_player_begin()`. A player uses energy from the previous frame before receiving new energy.

---

### Energy Accrual — `process_player_begin()`

```c
energy = extract_energy[p_ptr->pspeed];       // lookup table: speed → raw energy
energy *= (time_factor(p_ptr) / 100.0);       // time bubble scaling
if (in_town && p_ptr->running)
    energy *= (RUNNING_FACTOR / 100.0);        // town running bonus
p_ptr->energy += energy;

// Cap energy at level_speed(depth) unless ENERGY_BUILDUP option is set
if (p_ptr->energy > level_speed(depth))
    p_ptr->energy = level_speed(depth);

// Special overrides
if (p_ptr->resting)    p_ptr->energy = 0;     // resting: no built-up energy
if (p_ptr->paralyzed || p_ptr->stun >= 100)
    p_ptr->energy = 0;                         // paralysis/heavy stun: no energy
```

**`extract_energy[]`** — lookup table indexed by player speed (0–199).
- Speed 110 (normal) → ~20 energy per frame.
- Speed > 110 (haste) → more energy → acts more often relative to dungeon threshold.

**`level_speed(depth)`** — the energy threshold required to take one full action.
- At depth 0 (town): relatively low threshold.
- Deeper dungeons: higher threshold, so monsters act *faster* relative to the player unless the player gains speed.

**`time_factor(p_ptr)`** — MAngband's "time bubble" system. Slows server time around a single player to reduce perceived lag when alone; returns 100 (normal) in multiplayer.

---

### Energy Consumption — `process_player_end()`

```c
process_player_commands(p_ptr);   // drain energy for each buffered command

// Auto-melee: if not busy, energy still available, and no fear/confusion
blow_energy = level_speed(depth) / p_ptr->num_blow;
if (p_ptr->energy >= blow_energy
    && !p_ptr->confused && !p_ptr->afraid
    && !p_ptr->run_request && !cq_len)
{
    auto_retaliate(p_ptr);
}

// Running: consume level_speed worth of energy to take one run step
if (p_ptr->energy >= level_speed(depth)
    && (p_ptr->running || p_ptr->run_request))
{
    run_step(p_ptr, 0);
}

// Periodic processing (see §2 below)
```

**Cost of a standard action** = `level_speed(depth)` energy.
**Cost of one melee blow** = `level_speed(depth) / num_blow`.

This means a character with 3 blows per round can deal all 3 blows sequentially without waiting a full action period between blows — they each cost one-third of a full action.

---

### Key Constants

| Symbol | Typical Value | Meaning |
|--------|---------------|---------|
| `FPS` | ~50 | Server frames per second |
| `level_speed(0)` | 1000 | Energy threshold at town depth |
| `level_speed(n)` | `1000 + n*10` (approx.) | Threshold increases with depth |
| `extract_energy[110]` | ~20 | Energy per frame at normal speed |
| `RUNNING_FACTOR` | 150 | Town running multiplier (150% speed) |

---

## 2. Per-Turn Periodic Processing

### MAngband

Inside `process_player_end()`, there is a **periodic block** that triggers when a counter crosses `level_speed(depth) / 1000`, scaled by the time bubble. This effectively fires once every ~1000 energy units of real time, making its period depth-independent in wall-clock terms.

The following effects fire on this periodic tick:

| Effect | Condition |
|--------|-----------|
| Fade monster/player detect | Always |
| Poison DoT | `p_ptr->poisoned > 0` → `take_hit(1, "poison")` |
| Cut DoT | `p_ptr->cut > 0` → 1–3 HP depending on cut severity |
| HP regeneration | Base regen; doubled if resting or searching; **blocked** if poisoned or deeply cut |
| Mana regeneration | Similar to HP regen; doubled if resting |
| Status timer countdown | blind, paralyzed, confused, afraid, fast, slow, stun, invuln, protevil, poisoned, cut, etc. |
| Light fuel consumption | Decrements `p_ptr->cur_lite.pval` |
| Experience drain (10% chance) | If `p_ptr->exp_drain` flag set |
| Equipment recharge | `timeout--` for each rechargeable item |
| Rod recharge | Separate recharge logic for rods |
| Inventory sensing (pseudo-id) | Sense unknown items |
| Word of Recall countdown | `p_ptr->word_recall--` |

**Food digestion** fires on a *slower* sub-timer: every `(level_speed / 1000) * 10` energy units — 10× slower than the standard periodic tick.

---

## 3. World Processing (Every 50 Turns)

### MAngband — `process_world()` (called each frame for each player)

`process_world()` internally checks if the game turn counter has hit a multiple of 50. When it does:

- **Ambient sound**: every `10 * TOWN_DAWN / 4` turns (depth-0 uses town sound set; depth > 0 uses dungeon/wilderness sound set).
- **Day/Night toggle**: every `10 * TOWN_DAWN / 2` turns.
- **Monster generation**: `randint0(MAX_M_ALLOC_CHANCE) == 0` → `alloc_monster()` — one random monster wanders onto the level.
- **Monster regen**: handled separately in `regen_monsters()`, called every 100 turns, scaled by time bubble.

---

## 4. Auto-Retaliate

### MAngband

If the following conditions all hold at the end of a frame, the player attacks the monster sharing their tile (or an adjacent monster, depending on the melee target logic):

- `p_ptr->energy >= blow_energy` (has enough energy for at least one blow)
- `!p_ptr->confused && !p_ptr->afraid` (not incapacitated)
- `!p_ptr->run_request` (not trying to start running)
- `cq_len == 0` (command queue is empty — player is idle)

This auto-retaliate system is essential in MAngband because the server runs without waiting for player input. If a monster steps onto the player's tile and the player sends no command, the server automatically hits back.

---

## 5. Movement

### MAngband — `move_player()` in `cmd1.c`

Each call to `move_player()` costs `level_speed(depth)` energy (one full action).

**On each move step, in order:**

1. Check for wilderness boundary crossing (depth ≤ 0).
2. If destination has a **player**: attempt PvP attack, or swap positions if walking into each other.
3. If destination has a **monster**: call `py_attack()`.
4. If destination is an **impassable tile**: disturb, print obstacle message.
5. If destination is a **shop door**: disturb, enter store.
6. Otherwise — **normal movement**:
   a. Update player position in `cave[][]`.
   b. Run `verify_panel()` — scroll map if needed.
   c. Trigger `PU_VIEW | PU_LITE | PU_FLOW | PU_DISTANCE` updates.
   d. **Spontaneous search**: if `skill_fos >= 50` OR `randint0(50 - skill_fos) == 0` → call `search()`.
   e. **Continuous search**: if `p_ptr->searching` is set → call `search()`.
   f. Handle floor items: call `carry()` if item present.
   g. Discover invisible traps: if `FEAT_INVIS` on tile → `pick_trap()` + `disturb()`.
   h. Trigger visible traps: if `FEAT_TRAP_HEAD..FEAT_TRAP_TAIL` → `hit_trap()` + `disturb()`.

**Spontaneous search probability** formula:
- If `skill_fos >= 50`: always search on every step.
- Otherwise: `1 / (50 - skill_fos)` probability per step.
- Example: `skill_fos = 25` → 1-in-25 chance per step.

---

## 6. Running

### MAngband

Running is a **sustained movement mode** where the player automatically advances one tile per available `level_speed` of energy, without sending individual move commands.

#### Initialization — `run_init(p_ptr, dir)` in `cmd1.c`

Called when the player sends a "run" command in direction `dir`. Sets up the running algorithm's state:

- `p_ptr->running = TRUE`
- Records `find_current` (direction), `find_prevdir`, `find_openarea`, `find_breakleft`, `find_breakright`

The algorithm examines grids on both sides of the target direction to determine if the player is in a **corridor** (enclosed) or **open area**, affecting corner-following behaviour.

#### Stepping — `run_step(p_ptr, dir)` in `cmd1.c`

Called when `p_ptr->energy >= level_speed(depth)` and running is active. Advances one step and checks for stop conditions.

**Pathfind mode (`running_withpathfind`)**: click-to-move. Uses `p_ptr->pf_result[]` — a pre-computed array of direction chars produced by `findpath()` in `pathfind.c`. Each call pops the next direction from `pf_result[pf_result_index]`.

#### Running Algorithm Summary

The running algorithm (`run_step` + `area_affect`) works in two modes:

**Corridor mode** (enclosed on both sides):
- Follow the only open direction automatically.
- At a T-junction (more than one new open direction): **stop**.
- At a corner: cut the corner diagonally if the outer cells are both blocked; otherwise go straight and re-evaluate next step.

**Open area mode** (at least one open side):
- Run straight.
- Stop before entering an enclosed space (like reaching a doorway from an open room).
- Stop if a wall opens up on the side being tracked (`find_breakleft` / `find_breakright`).

#### Town Running

In town (`dun_depth == 0`) while running:
```c
energy *= (RUNNING_FACTOR / 100.0);  // RUNNING_FACTOR = 150 → 50% faster
```
Town running moves at 1.5× speed. The dungeon does not apply this multiplier.

#### Run Request

`p_ptr->run_request` is set by the client before the server processes the frame. This lets `process_player_end()` detect "player wants to start running" even before `p_ptr->running` is set, and prevents auto-retaliate from triggering during the transition.

---

## 7. Interruptions (disturb)

### MAngband — `disturb(p_ptr, int stop_search, int unused)`

```c
void disturb(player_type *p_ptr, int stop_search, int unused)
{
    p_ptr->running = FALSE;
    p_ptr->running_withpathfind = FALSE;
    if (stop_search) p_ptr->searching = FALSE;
    // cancel any pending repeat commands
}
```

`disturb()` **always** cancels running and pathfind-running.  
`stop_search = 1` also cancels continuous searching mode.

#### All Disturb Call Sites

| Trigger | `stop_search` | Source |
|---------|---------------|--------|
| Player takes damage (`take_hit`) | 1 | `spells1.c` |
| Player hits a wall / obstacle | 0 (or 1) | `cmd1.c:move_player` |
| Player bumps another player | 1 | `cmd1.c:move_player` |
| Player steps on invisible trap | 0 | `cmd1.c:move_player` |
| Player steps on visible trap | 0 | `cmd1.c:move_player` |
| Player picks up gold | 0 | `cmd1.c:carry` |
| Player picks up an item | 0 | `cmd1.c:carry` |
| Search finds trap | 0 | `cmd1.c:search` |
| Search finds secret door | 0 | `cmd1.c:search` |
| Search finds chest trap | 0 | `cmd1.c:search` |
| Map panel scrolls | 0 | `cmd1.c:move_player` (DISTURB_PANEL option) |
| Player switches places with another player | 1 | `cmd1.c:move_player` |
| Player leaves wilderness tile | 0 | `cmd1.c:move_player` |
| Player enters a store | 0 | `cmd1.c:move_player` |
| Inventory sensing (pseudo-id) notices something | 0 | `process_player_end` (DISTURB_MINOR option) |
| Player faints from hunger | 1 | `process_player_end` |
| Pack overflow (item drops) | 1 | `inven_carry` |
| Word of Recall activates | 1 | `process_player_end` |

---

## 8. Searching

### MAngband — `search(p_ptr)` in `cmd1.c`

**Two modes:**
1. **Active search command** (`do_cmd_search`): Player explicitly searches, costs one full action.
2. **Passive searching** (`p_ptr->searching = TRUE`): Called automatically on every move step (continuous mode, toggled by `S` key).
3. **Spontaneous search**: Called on move when `skill_fos >= 50` OR random chance fires (see §5).

#### Mechanic

```c
chance = p_ptr->skill_srh;
if (p_ptr->blind || no_lite(p_ptr)) chance /= 10;   // nearly blind
if (p_ptr->confused || p_ptr->image)  chance /= 10;  // confused

for y in (py-1)..(py+1):
    for x in (px-1)..(px+1):                 // 3×3 grid = 9 checks
        if randint0(100) < chance:
            if FEAT_INVIS:
                pick_trap(y, x)
                msg "You have found a trap."
                disturb(p_ptr, 0, 0)
            elif FEAT_SECRET:
                place_closed_door(y, x)
                msg "You have found a secret door."
                disturb(p_ptr, 0, 0)
                gain_exp(1)
            elif chest with unknown traps:
                object_known(chest)
                msg "You have discovered a trap on the chest!"
                disturb(p_ptr, 0, 0)
```

- **`skill_srh`** is the raw search skill (0–100+, see §9).
- Each of the 9 adjacent cells is checked **independently** every search call.
- A high search skill (e.g. 60) gives a 60% chance per cell per search, making thorough sweeping very likely.
- Conditions that halve search efficiency: blindness, no light, confusion, hallucination.
- Finding something **always calls `disturb()`**, which stops running/searching mode.
- Finding a secret door grants 1 XP.

---

## 9. Skills Reference

### MAngband — Derived Skills (`calc_bonuses()` in `xtra1.c`)

All 8 player skills are computed from:
- **Race base** (`rp_ptr->r_X`)
- **Class base** (`cp_ptr->c_X`)
- **Level scaling**: `(race_base + class_base) * player_level / 50`
- **Equipment bonuses** (for stealth and search)

| Field | Display Name | Display Divisor | Primary Use |
|-------|-------------|-----------------|-------------|
| `skill_thn` | Fighting | /12 | Melee to-hit roll |
| `skill_thb` | Shooting | /12 | Ranged to-hit roll |
| `skill_sav` | Saving Throw | /6 | Resist monster spells & effects |
| `skill_stl` | Stealth | /1 | Reduces noise; determines monster alertness (range 1–30) |
| `skill_fos` | Perception | /6 | Spontaneous searching on move; recognise monster lore |
| `skill_srh` | Searching | /6 | Active/passive trap and door detection |
| `skill_dis` | Disarming | /8 | Trap disarm rolls |
| `skill_dev` | Magic Devices | /6 | Wand, staff, rod activation |

**Equipment interactions:**
- `TR1_STEALTH` flag: `skill_stl += item.bonus`
- `TR1_SEARCH` flag: `skill_srh += item.bonus * 5`
- `to_h` bonus on weapons: `skill_thn += to_h * BTH_PLUS_ADJ`

**Display formula:** Skill value shown in character screen = raw value / display divisor. E.g. `skill_sav = 60` → shown as 10.

---

## 10. Saving Throws

### MAngband — Patterns from `spells1.c`

MAngband uses four distinct saving throw formulas, ordered from easiest to hardest for the player to resist:

#### Pattern 1 — Simple Percentile (easiest)
```c
if (randint0(100) < p_ptr->skill_sav) { resisted }
```
Used for: confusion from monster spells, blindness from monster spells, fear from some sources, nexus teleport-level, nexus stat-scramble.

A player with `skill_sav = 60` has 60% resistance.

#### Pattern 2 — vs. Monster Power (moderate)
```c
if (randint0(40 + power * 2) < p_ptr->skill_sav) { resisted }
```
Used for: polymorph stat-scramble (`apply_morph` case 1).

Against a `power = 20` monster: range is `randint0(80)`, need `< skill_sav`. A player with `skill_sav = 60` resists 75% of the time vs power-20.

#### Pattern 3 — vs. Monster Power (hard)
```c
if (randint0(10 + power * 4) < p_ptr->skill_sav) { resisted }
```
Used for: fruit bat polymorph, high-level stat drain.

Against `power = 20`: range is `randint0(90)`. Player with `skill_sav = 60` resists ~67% of the time.

#### Pattern 4 — vs. Damage (very hard)
```c
if (randint0(100 + dam * 6) < p_ptr->skill_sav) { resisted }
```
Used for: stat drain from nether/chaos/time damage effects.

Against `dam = 20`: range is `randint0(220)`. Player with `skill_sav = 60` resists only ~27% of the time.

#### Additional: Level Drain
```c
if (randint1(127) > p_ptr->lev) { level drained }
```
Level drain is **not** resisted by saving throw. It uses a direct level comparison: at level 40, 68% chance of being drained per hit.

#### Saving Throw Summary Table

| Formula | Effective vs. | Player Resist % (`skill_sav=60`) |
|---------|--------------|----------------------------------|
| `rand0(100) < sav` | Simple | 60% |
| `rand0(40 + pow*2) < sav` | Power-10 monster | 75% |
| `rand0(10 + pow*4) < sav` | Power-10 monster | ~86% |
| `rand0(100 + dam*6) < sav` | Dam-10 | ~50% |

---

---

## 11. IronHell Status Summary

This section tracks how each MAngband system maps to the current IronHell implementation.

| System | MAngband | IronHell | Status |
|--------|---------|----------|--------|
| **Server loop** | Real-time 50 FPS, frame-based | Turn-based via `advanceWorldTick()`, event-driven | Different by design (single-player); functionally equivalent |
| **Energy model** | `extract_energy[pspeed]` table; `level_speed(depth)` threshold; depth scales monster speed | `entity.speed` → energy accrual each tick; `levelSpeed(depth)` threshold scales action cost | **Implemented** — `levelSpeed()` in `energy.ts`; `monsterAI.ts` uses depth-scaled action cost |
| **Time bubble** | `time_factor()` scales energy per frame; slows server for lone player | N/A (single-player only) | Not needed |
| **Action cost** | `level_speed(depth)` energy per move/action | `levelSpeed(depth)` used in `monsterAI.ts` Phase 2 | **Implemented** |
| **Blow energy model** | Multiple blows per turn via `blow_energy = level_speed / num_blow` | `blowEnergy(depth, numBlows)` in `energy.ts` | **Implemented** |
| **Periodic processing** | Fires at `level_speed / 1000` energy intervals: DoT, regen, status countdown, food | DoT and status tick per world tick; regen blocked by poison/deep cut; exp drain, light fuel, equip recharge | **Implemented** |
| **HP regen blocked** | Blocked if poisoned or deeply cut (`cut >= 100`) | `applyResourceRegeneration()` blocks HP regen when poisoned or `cut >= 100` | **Implemented** — `progression.ts` |
| **Experience drain** | 10% chance per periodic tick if `exp_drain` flag set; drains `1 + exp/rand1(100k)` | `rollExperienceDrain()` in `progression.ts`; wired into `processCharacterTick` | **Implemented** |
| **Light fuel consumption** | Decrements `cur_lite.pval` each periodic tick | `consumeLightFuel()` in `progression.ts`; wired into `processCharacterTick` | **Implemented** |
| **Equipment recharge** | `timeout--` for each rechargeable item each periodic tick | `tickEquipmentRecharge()` in `progression.ts`; wired into `processCharacterTick` | **Implemented** |
| **World tick (every 50t)** | Ambient sound, monster spawn, day/night | `advanceWorldTickWithNarrative` every 50 turns; ambient sound, monster events | Matches |
| **Auto-retaliate** | Fires when player is idle and has blow energy | Not implemented (player turn-based; no idle auto-attack) | Not needed for turn-based design |
| **Movement** | `move_player()` — attack on contact, pick up items, hit traps | `movePlayer()` — similar structure | Implemented |
| **Spontaneous search on move** | `skill_fos >= 50` → always; else `1/(50-fos)` chance | `shouldAutoSearchThisTurn(fos, rng)` in `searching.ts` | **Implemented** |
| **Running** | `run_init`/`run_step`; corridor following algorithm; `RUNNING_FACTOR` in town | `initRunState`, `getRunStep`, `disturbCharacter` in `running.ts`; corridor / open-area detection; corner following; junction stops; Shift+direction in `useGameInput.ts`; 150 ms loop in `useDungeonLoop.ts`; `startRunning`/`stopRunning`/`advanceRunStep` store actions in `worldActions.ts` | **Implemented** |
| **disturb() — stop running** | Immediate on damage, trap find, item pickup, panel scroll | `disturbCharacter(char, stopSearch)` in `running.ts`; wired in `playerActions.ts` (damage, trap trigger), `worldActions.ts` (monster damage, search finds traps/doors) | **Implemented** |
| **Search skill** | `skill_srh`; 9-cell check; `rand0(100) < chance` per cell; blind/dark/confused penalties | `trySearchForHidden(skill, _, rng, blind?, confused?)` in `searching.ts`; radius 1 (3×3); flat `rand0(100) < skill` with /10 penalties | **Implemented** |
| **8 skills** | `thn`, `thb`, `sav`, `stl`, `fos`, `srh`, `dis`, `dev` | `skills` object with `melee`, `ranged`, `stealth`, `perception`, `savingThrow`, `disarming`, `magicDevice` | **All 8 active** — `sav` via `trySavingThrow` in `blowEffects.ts`; `dis` via `tryDisarmTrap` in `dungeonFeatures.ts`; `dev` via `tryUseMagicDevice` in `deviceUse.ts` |
| **Saving throw (Pattern 1)** | `rand0(100) < skill_sav` | `trySavingThrow(sav, power, rng, 'simple')` — ignores power | **Implemented** |
| **Saving throw (Pattern 2)** | `rand0(40 + power*2) < skill_sav` | `trySavingThrow(sav, power, rng, 'vsPowerMod')` | **Implemented** |
| **Saving throw (Pattern 3)** | `rand0(10 + power*4) < skill_sav` | `trySavingThrow(sav, power, rng, 'vsPowerHard')` | **Implemented** |
| **Saving throw (Pattern 4)** | `rand0(100 + power*6) < skill_sav` | `trySavingThrow(sav, power, rng, 'vsDamage')` | **Implemented** |
| **Level drain** | `rand1(127) > level` | `tryLevelDrain(characterLevel, rng)` in `skillChecks.ts` | **Implemented** |

---

## Appendix A — `level_speed(depth)` Formula

From `dungeon.c`:

```c
u32b level_speed(int Depth)
{
    // Town and wilderness (Depth <= 0)
    if (Depth <= 0) return 1000;

    // Dungeon: scales up with depth
    return (1000 + Depth * 10);
}
```

At dungeon depth 50: `level_speed = 1500`. This means players and monsters need more energy per action. Since `extract_energy[speed]` gives a fixed amount per frame, and `level_speed` is the *cost* of acting, effectively **all entities act more slowly** on deeper levels unless they have higher speed (which would give more energy per frame via the lookup table).

This creates the MAngband feel of "faster" monsters at depth — a monster with `mspeed = 110` at depth 50 accumulates 20 energy/frame but needs 1500 to act, giving one action every 75 frames. A hasted monster (`mspeed = 120`) accumulates ~25 energy/frame and acts every 60 frames.

---

## Appendix B — Pathfinding

From `pathfind.c`:

`findpath(p_ptr, target_y, target_x)` uses a **modified flood-fill** (not true A*) over a `MAX_PF_RADIUS × MAX_PF_RADIUS` terrain grid centred on the player:

- Tiles not marked `CAVE_MARK` (unknown to player) are treated as **passable** (the player assumes they can move through unseen areas).
- Known traps are **avoided**.
- Result stored in `p_ptr->pf_result[]` as a string of direction digits (`'1'`–`'9'` numpad notation).
- `p_ptr->pf_result_index` tracks which step is next.
- `run_step()` reads one direction per call, advancing the index.

This gives click-to-move behaviour: the player clicks a destination, `findpath()` computes the path, and `run_step()` executes it one tile per energy period.

---

## 14. Equipment Flags & Wearable Mechanics

### 14.1 Boolean Equipment Flags

Flags extracted from equipment affixes during stat recalculation (`characterCreation.ts`). Each flag directly affects gameplay systems:

| Flag | Affix Source | Effect | System File |
|------|-------------|--------|-------------|
| `hasFreeAction` | `free_action` | Blocks paralysis & slow from monster blows | `blowEffectApplication.ts` |
| `hasFeatherFall` | `feather_fall` | Negates damage from pit traps (pit, pit_spiked, pit_poison) | `trapMechanics.ts` |
| `hasSlowDigest` | `slow_digest` | Halves hunger decay per tick (4 → 2) | `progression.ts` |
| `hasTelepathy` | `telepathy` | See monsters through walls within 20 tiles (Chebyshev) | `renderFrame.ts` |
| `hasAggravate` | `aggravate` | All monsters detect player regardless of stealth/distance | `stealthDetection.ts` → `monsterAI.ts` |
| `hasHoldLife` | `hold_life` | Prevents XP drain from monster blows | `blowEffectApplication.ts` |
| `hasLite` | `lite` | +1 light radius bonus | `characterCreation.ts` |
| `hasTeleportCurse` | `teleport_curse` | Tracked for random teleportation curse | `characterCreation.ts` |
| `hasRegenFlag` | `regeneration` | HP regen bonus (prior session) | `progression.ts` |
| `expDrain` | `exp_drain` | XP drain per tick (prior session) | `progression.ts` |

### 14.2 Caster Helm Penalty

Mages and Rangers suffer a **-5 mana penalty** when wearing a helm that lacks INT or WIS affixes. This simulates MAngband's encumbrance rule that heavy headgear disrupts magical concentration.

- System: `isCasterHelmPenalty()` in `classFeatures.ts`
- Applied during `recalculateCharacterStats` in `characterCreation.ts`
- Classes affected: `mage`, `ranger`
- Exempt helms: Any helm with `intelligence` or `wisdom` affix

### 14.3 Sustain Stats

When a monster blow drains a stat (LOSE_STR, etc.), the sustain system checks:
1. Racial sustain flags from the character's race
2. Equipment affixes (`sust_str`, `sust_dex`, etc.)

If sustained, the drain is blocked entirely. Implemented in `sustainStats.ts`, wired through `blowEffectApplication.ts`.

### 14.4 Curse Mechanics

Three tiers of curse strength, matching MAngband:

| Tier | Can Unequip? | Remove Curse | *Remove Curse* |
|------|-------------|-------------|----------------|
| `light` | No | ✅ Removes | ✅ Removes |
| `heavy` | No | ❌ Fails | ✅ Removes |
| `perma` | No | ❌ Fails | ❌ Fails |

- `tryRemoveCurse(item, isStarRemoval?)` in `itemMagic.ts`
- `getUnequipBlockReason()` returns tier-specific messages

### 14.5 Item Activations

55 activation effects mapped from MAngband artifacts and Dragon Scale Mail. System in `itemActivation.ts`:

| Activation Type | Examples | Effect |
|----------------|---------|--------|
| `damage` | Breathe Fire, FIRE1-3, COLD1-3 | Area damage with type + radius |
| `heal` | HEAL1-3 | Restore HP |
| `buff` | HASTE, BERSERK, PROT_EVIL | Temporary combat bonuses |
| `detect` | DETECT, MAP | Reveal monsters/map |
| `teleport` | PHASE, TELEPORT | Relocate character |
| `utility` | LIGHT, IDENTIFY | Special effects |

Functions: `canActivateItem()`, `getActivationDescription()`, `resolveItemActivation()`

---

## 15. Stealth & Monster Wake Mechanics

> **MAngband parity section.** All rules here map to MAngband 1.5.3 source files: `xtra1.c`, `cmd1.c`, `monster2.c`, `melee2.c`.

---

### 15.1 Movement Noise

Every step a player takes generates a noise value sent into the energy loop as the `noiseLevel` parameter of `advanceTurn()`. The noise depends solely on the character's `skills.stealth` rating.

**Formula — `computeMovementNoise(stealth)` in `stealthDetection.ts`:**
```
noise = ceil((30 - clamp(stealth, 0, 30)) / 7.5)   →  range 0–4
```

| Stealth | Noise per step | Notes |
|---------|---------------|-------|
| 0 | 4 | Fully armored, clumsy |
| 1–5 | 4 | Novice stealthy characters |
| 10 | 3 | Average rogue early game |
| 20 | 2 | Skilled rogue |
| 23–29 | 1 | High-stealth characters |
| 30 | 0 | Completely silent — near-impossible to wake sleeping monsters by movement alone |

MAngband source: `process_player()` in `cmd1.c` adds `p_ptr->skill_stl` dependent noise each step.

---

### 15.2 Rogue Stealth Mode

Toggled by the `T` key action (`toggleStealthMode` in `worldActions.ts`).

| Class | Stealth Multiplier | Speed Penalty | Notes |
|-------|--------------------|--------------|-------|
| Non-rogue | ×1.5 | −10 speed | Careful movement, available to all |
| **Rogue** | **×3** | **level-scaled** | MAngband exact: triples `skill_stl` |

**Rogue speed penalty scales with level** (`computeRogueStealthSpeedPenalty` in `stealthDetection.ts`):

| Level | Speed Penalty |
|-------|--------------|
| 1–4   | −10 |
| 5–9   | −8  |
| 10–14 | −6  |
| 15–19 | −4  |
| 20–24 | −2  |
| 25+   | 0 (free — no penalty) |

**MAngband source:** `do_cmd_toggle_search()` in `cmd1.c` — rogues receive a ×3 multiplier to their computed stealth. A rogue with base stealth 10 reaches 30 in stealth mode → **0 movement noise** (completely silent movement).

Example: Rogue with base `skills.stealth = 10` in stealth mode:
- Effective stealth = 10 × 3 = 30
- Movement noise = `ceil((30-30)/7.5)` = 0 — every step is silent

When stealth mode is **off**, stealth is divided back: `floor(effectiveStealth / multiplier)`.

---

### 15.3 Monster Detection Range

IronHell's `isPlayerDetectedByMonster()` in `stealthDetection.ts` provides a **complementary layer** to the noise wake system. A sleeping monster must first "hear" the player (noise roll) before `csleep` depletion begins — detection range gates whether any waking can occur at all.

Detection uses Chebyshev distance vs. a threshold derived from monster level and player stealth:
- Higher monster level → wider detection radius
- Higher player stealth → narrower detection radius
- `hasAggravate` on the player → always detected regardless of range

This is IronHell's addition on top of MAngband's pure noise system.

---

### 15.4 Gradual Monster Wake (csleep — MAngband Parity)

MAngband tracks sleeping monster wakefulness via a counter `m_ptr->csleep`. Each game turn there is a chance the monster "hears" the player, depleting csleep. When it reaches 0, the monster wakes.

**IronHell implementation** in `monsterAI.ts`:

1. Each tick, sleeping monsters check if the nearest player is within detection range.
2. If detected, a noise roll fires: `rng() < hearingChance + proximityBonus`
   - `hearingChance = min(MAX_HEARING_CHANCE, noiseLevel × NOISE_HEARING_MULTIPLIER)`
   - `proximityBonus`: extra chance when player is within 3 tiles
3. On a successful noise roll: `sleepDepth -= floor(SLEEP_DEPTH_DEPLETION_BASE / distance)`
   - Closer player = larger depletion per roll
4. When `sleepDepth` reaches 0: monster wakes (`aiState = 'chase'`), sleep status removed.

**Spawn sleep depth** for `forceSleep` monsters:
```
sleepDepth = flags.sleepRating ?? (level * 3 + 50)
```
A monster with no explicit `sleepRating` at level 25 starts with `sleepDepth = 125`.

**Key insight:** A careful rogue (stealth mode, stealth 30, noise=0) can walk past a sleeping monster without ever triggering a noise roll, never depleting csleep at all — true MAngband backstab setup.

---

### 15.5 Dragon Backstab (and Powerful Sleeping Monsters)

MAngband `melee2.c` allows a rogue to deal massive bonus damage against sleeping monsters that were not woken by the approaching player. This applies not only to dragons but to **any monster with `forceSleep: true`** (i.e. spawns asleep).

**Eligible monsters:**
- All monsters with `flags.forceSleep: true` in `monster_compendium.json`
- Examples: Ancient dragons (`D`), Deep Ones, Vampires, unique bosses that spawn in magical slumber

**Backstab condition:**
- Monster must be sleeping (`hasStatusKeyword('sleep')`)
- Monster's `sleepDepth` must equal its **original spawn depth** (never partially woken)
- Player must be a Rogue class (full bonus) — or non-rogue for a partial bonus

This creates the signature Rogue play pattern: sneak in stealth mode, maintain silence, deliver a devastating first strike before the dragon ever stirs.

> **Implementation note:** The full damage multiplier formula is tracked in `monsterAI.ts` / combat system. Backstab bonus applies only on the first attack if the monster was never woken.

---

### 15.6 NO_SLEEP Flag vs. FORCE_SLEEP Flag

These two flags are **independent** and can coexist on the same monster:

| Flag | Effect | Source |
|------|--------|--------|
| `noSleep` | Immune to **spell-based sleep** (Sleep Monster scroll/spell). MAngband `NO_SLEEP`. | `spellCasting.ts` → `isMonsterImmuneToSleep()` |
| `forceSleep` | Spawns **asleep** with a `sleepDepth` counter. Can still be woken by noise. | `createMonsterFromTemplate()` in `monster.ts` |

Ancient dragons have **both flags**: they spawn asleep (`forceSleep`) but cannot be put back to sleep once awoken (`noSleep`). This is correct MAngband behavior.

---

### 15.7 Action Noise Reference (Variation A)

MAngband only defines noise for **movement** (running generates noise; walking in stealth mode is silent). All non-movement action noise values are IronHell-specific. The `actionNoise(normal, stealth, isSneaking)` helper in `stealthDetection.ts` selects between the two columns.

| Action | Normal Noise | Stealth Noise | Notes |
|--------|-------------|---------------|-------|
| Rest / wait (R, 5, ,) | 0 | 0 | Standing still makes no sound |
| Search (s) | 1 | 0 | Stealthy searching is silent |
| Open door (move into door) | 2 | 1 | Careful opening reduces creak |
| Bash door (b) | 5 | 4 | Loud regardless — stealth barely helps |
| Disarm trap (D) | 2 | 0 | Expert hands leave no trace |
| Open/disarm chest (o / D) | 2 / 2 | 1 / 0 | Careful lid opening; lock picking is silent |
| Melee combat | 4 | 3 | Backstab → 0 (sleeping target, never woken) |
| Ranged attack | 3 | 2 | Backstab → 0 |
| Spell cast (success) | 3 | 2 | Muttered incantations vs. silent gestures |
| Spell cast (turn consumed, failed) | 1 | 0 | Muffled fumble |
| Pick up item (%, menu) | 1 | 0 | Stealthy grab is silent |
| Use consumable (quaff/read/use) | 1 | 0 | Sneaking characters are more careful |
| Equip / unequip (w / T) | 2 | 1 | Armor rattle, quieter in stealth |
| Mining / digging | base noise | max(1, ⌈base/2⌉) | Half noise in stealth; minimum 1 |

Town actions always produce 1 noise tick (irrelevant — no sleeping dungeon monsters).

---

### 15.8 Armor Weight Stealth Penalties

MAngband has no armor-type stealth penalty (only `TR1_STEALTH` flag items grant bonuses). IronHell adds weight-based penalties to `skills.stealth` via `getArmorStealthPenalty()` in `stealthDetection.ts`, applied during `recalculateCharacterStats()`.

**Cloth and leather weights never penalise stealth.** Mail and plate penalise based on the armor slot.

| Slot | Leather / Cloth | Mail | Plate |
|------|----------------|------|-------|
| Chest (body) | 0 | −3 | −6 |
| Boots (feet) | 0 | −2 | −4 |
| Gloves (hands) | 0 | −1 | −2 |
| Helmet (head) | 0 | −1 | −2 |
| Crown | 0 | 0 | −2 |
| Shield | 0 | −1 | −2 |
| Cloak | 0 | 0 | 0 |

**Design intent:** A rogue in Soft Leather Armor + Soft Leather Boots suffers **0 total penalty**. A warrior in Full Plate Mail + Metal Shod Boots suffers **−10 penalty** (chest −6, boots −4) before any +stealth affixes.

Penalties stack additively across all equipped armor slots. `skills.stealth` is clamped to `max(0, stealth + penalties)`.

---

## 16. Ego Item & Artifact Enrichment

### 15.1 Ego tval/sval Matching

Ego items match by checking the item's actual `tval` against the ego's `applicableTypes[].tval` and `sval` range. This prevents misapplication (e.g. boot ego on a cloak). Legacy items without `tval` fall back to broad category matching.

### 15.2 Ego Xtra Random Flags

MAngband ego_item.txt `xtra` field grants a random bonus flag:

| xtra | Pool | Flags |
|------|------|-------|
| 1 | Sustain | SUST_STR, SUST_DEX, SUST_CON, SUST_INT, SUST_WIS, SUST_CHR |
| 2 | Resistance | RES_BLIND, RES_CONFU, RES_SOUND, RES_SHARD, RES_NEXUS, RES_NETHR, RES_CHAOS, RES_DISEN, RES_LITE, RES_DARK |
| 3 | Ability | FREE_ACT, HOLD_LIFE, FEATHER, LITE, SEE_INVIS, TELEPATHY, SLOW_DIGEST, REGEN |

### 15.3 Artifact Substitution

Artifacts match by `tval` to the base item. Curse flags (LIGHT_CURSE, HEAVY_CURSE, PERMA_CURSE) are extracted and stored. Activations and recharge times are carried to the result item. 48 wearable artifacts added (125 total).
