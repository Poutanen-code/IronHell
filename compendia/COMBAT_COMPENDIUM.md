# Combat Compendium

Reference documentation for IronHell's combat systems, compared against MAngband 1.5.3 and FAangband sources.

---

## Table of Contents

1. [Multiple Attacks (Blows Per Round)](#1-multiple-attacks-blows-per-round)
2. [Critical Hits (Melee)](#2-critical-hits)
3. [Two-Handed Weapons and Shield Use](#3-two-handed-weapons-and-shield-use)
4. [Dual Wielding](#4-dual-wielding)
5. [Ranged Critical Hits (`critical_shot`)](#5-ranged-critical-hits-critical_shot)
6. [Ranged Multiple Shots (`num_fire`)](#6-ranged-multiple-shots-num_fire)
7. [Hit Resolution](#7-hit-resolution)
8. [Implementation Status Summary](#8-implementation-status-summary)

---

## 1. Multiple Attacks (Blows Per Round)

### MAngband Model

MAngband calculates the number of melee blows per round from a 2D lookup table (`blows_table[12][12]`). The two table indices come from:

- **Strength index** = `str_blow_index[STR]` — maps raw strength to a table row
- **Dexterity index** = `dex_blow_index[DEX]` — maps raw dexterity to a table column

Before the lookup a per-class parameter controls how much strength and weapon weight affect the result:

```
p = floor(str_blow_index[STR] * class_mult / max(class_minDiv, weapon_weight))
d = dex_blow_index[DEX]
blows = min(class_numBlows, blows_table[p][d])
```

Class parameters (from `cmd1.c`):

| Class   | mult | minDiv | numBlows |
|---------|------|--------|----------|
| Warrior | 5    | 30     | 6        |
| Mage    | 2    | 40     | 4        |
| Priest  | 3    | 35     | 5        |
| Rogue   | 3    | 30     | 5        |
| Ranger  | 4    | 35     | 5        |
| Paladin | 4    | 30     | 5        |

**Heavy weapon penalty** (`heavy_wield`): if `weight > str_adj_wgt[STR] * 3`, MAngband sets `to_h -= 15` and forces blows to 1. IronHell has the blows table but not the heavy_wield cap (known gap).

### IronHell Implementation

`calculateBlowsPerRound` in `src/game/systems/character/statTables.ts` implements the MAngband formula exactly. Called inside `recalculateCharacterStats` (`characterCreation.ts`) and the result stored in `derivedStats.blowsPerRound`.

**Display**: CharacterSheetDialog shows "Blows: N/rnd" in the Combat section.

**Heavy weapon penalty** (`heavy_wield`): if `weight > str_adj_wgt[STR] * 3`, MAngband sets `to_h -= 15` and forces blows to 1. IronHell has the blows table but not the heavy_wield cap (known gap).

---

## 2. Critical Hits

### MAngband `critical_norm` (cmd1.c:119)

```
i = weight + to_h * 5 + level * 3
critRoll = rand(5000) + 1
if critRoll > i: no crit
else:
  k = weight + rand(650) + 1
  apply tier based on k
```

Tier table (melee):

| k threshold | damage mult | flat bonus | name     |
|-------------|-------------|------------|----------|
| < 400       | ×2          | +5         | good     |
| < 700       | ×2          | +10        | great    |
| < 900       | ×3          | +15        | superb   |
| < 1300      | ×3          | +20        | \*GREAT\* |
| ≥ 1300      | ×3.5        | +25        | \*SUPERB\* |

The tier name is logged in MAngband as a separate message: `"It was a good hit!"`, `"It was a great hit!"`, etc.

### IronHell Implementation

`classicCriticalNorm` in `src/game/systems/combat/combat.ts` implements the formula correctly including the `weight + to_h*5 + level*3` chance roll. Returns `{ damage, isCritical, critTier: CritTier | null }`.

`applyclassicStyleCritical` (used by the non-MAngband tooltip code path) uses a simplified formula without `to_h` — known gap, not yet fixed.

**CritTier propagation** (implemented):
- `CritTier = 'good' | 'great' | 'superb' | 'hi_great' | 'hi_superb'` in `common.ts`
- `AttackResult.critTier: CritTier | null` tracked per action
- `resolvePlayerAttackAgainstMonster` returns best crit tier across all blows
- `describePlayerAttack` in `combatNarrative.ts` uses tier-specific messages

### FAangband Comparison

FAangband uses the same tier concept but implements crits with extra dice rolls rather than a flat multiplier + bonus. The 5-tier system is preserved. IronHell follows MAngband's multiplier approach.

---

## 3. Two-Handed Weapons and Shield Use

### MAngband

MAngband has no enforcement of two-handed weapon exclusivity. A player can wield a two-handed weapon and a shield simultaneously without penalty. The weight/strength system indirectly reduces blows and accuracy when using heavy weapons without sufficient strength.

### FAangband (Source: NickMcConnell/FAangband on GitHub)

FAangband adds two weapon object flags:
- `OF_TWO_HANDED_REQ` — weapon *requires* both hands (cannot use off-hand at all)
- `OF_TWO_HANDED_DES` — weapon *desires* both hands (can use off-hand but at a penalty)

When a shield is equipped with a two-handed desired weapon:
- Shield AC contribution is reduced to **⅔** of its normal value
- A `shield_on_back` state is tracked
- **Shield bash** is disabled when shield is on back
- Players get a message informing them of the reduced effectiveness

When a `TWO_HANDED_REQ` weapon is equipped:
- Shield is automatically moved to the "on back" / inaccessible state
- No AC from shield is applied

### IronHell Implementation

IronHell currently does **not** enforce two-handed weapon restrictions. Both hands can always be used freely. This is a known gap vs MAngband/FAangband — no shield penalty exists for two-handed wielding. Planned for future implementation.

---

## 4. Dual Wielding (Off-Hand Weapon)

### MAngband

MAngband does **not** implement dual wielding. The off-hand is exclusively for shields or goes empty.

### IronHell Custom System

IronHell implements dual wielding as a custom extension:
- Off-hand weapon (`equipment.offHand` slot) can hold a second weapon
- Off-hand attacks use **0.85× damage multiplier** (`styleMult = 0.85`)
- Off-hand attacks suffer a **−9 to-hit penalty** (applied via `attackStyle = 'secondary'`)
- Off-hand blows use the same blows-per-round count as main-hand
- No class-specific off-hand restrictions currently exist

Balancing rationale: the accuracy penalty and damage reduction make dual-wielding competitive with a shield only for highly dexterous characters; shields add defence while off-hand adds offence.

---

## 5. Ranged Critical Hits (`critical_shot`)

### MAngband (`critical_shot` in `cmd1.c`)

After a ranged hit is confirmed, `critical_shot` runs:

```c
i = weight + ((to_h + plus) * 4) + (lev * 2);   // note ×4 and lev×2 (vs melee ×5/×3)
if (randint1(5000) <= i)
{
    k = weight + randint1(500);                   // note 500 not 650
    if (k < 500)  { "good hit";   dam = 2*dam + 5;  }
    if (k < 1000) { "great hit";  dam = 2*dam + 10; }
    else          { "superb hit"; dam = 3*dam + 15; }
}
```

Differences vs `critical_norm` (melee):
- Hit bonus multiplier: `×4` vs melee `×5`
- Level multiplier: `×2` vs melee `×3`
- Random range for tier roll: 500 vs 650
- Only **3 tiers** (no hi_great / hi_superb)

**3-tier ranged crit table:**

| k threshold | damage formula | name    |
|-------------|----------------|---------|
| < 500       | `2d + 5`       | good    |
| < 1000      | `2d + 10`      | great   |
| ≥ 1000      | `3d + 15`      | superb  |

### IronHell Implementation

`applyclassicStyleCriticalShot` in `combat.ts` implements the formula exactly:
- `critPower = weight + rangedBonus×4 + level×2`
- `rangedBonus = floor((dex−10)/2) + floor(attackPower/25)` (approximation of `to_h`)
- Uses `RANGED_CRIT_TIERS` constant (3 tiers: good/great/superb)
- Returns `{ damage, isCritical, critTier: CritTier | null }` (added this session)

---

## 6. Ranged Multiple Shots (`num_fire`)

### MAngband (`xtra1.c` — `calc_bonuses`)

```c
p_ptr->num_fire = 1;   // default: 1 shot per round

// CF_EXTRA_SHOT flag (Rangers only) + using a bow (tval_ammo = TV_ARROW):
if (p_ptr->lev >= 20) p_ptr->num_fire++;
if (p_ptr->lev >= 40) p_ptr->num_fire++;

// Equipment extra_shots bonus (from items with xtra_shots flag):
p_ptr->num_fire += extra_shots;
if (p_ptr->num_fire < 1) p_ptr->num_fire = 1;
```

**Energy model**: each shot uses `energy / num_fire` of a turn (`cmd2.c`: `energy -= level_speed / thits`). With 2 shots, each shot costs half a turn — the player fires twice in the same time another player fires once.

**Important**: MAngband fires one projectile per command. Multiple `num_fire` means the player can issue the fire command more times per full turn (reduced energy per shot), not that one command fires multiple arrows.

### IronHell Implementation

IronHell models this as **all shots in one action** (analogous to how melee fires all blows in one action):

- `derivedStats.shotsPerRound` stores `num_fire` (computed in `recalculateCharacterStats`)
- Ranger with long/short bow: +1 shot at level ≥ 20, +1 shot at level ≥ 40
- Other classes or non-bow launchers: always 1 shot
- `resolvePlayerRangedAttackAgainstMonster` loops `shotsPerRound` times, accumulating damage
- Highest crit tier across all shots is returned as `bestCritTier`
- All shots miss → `isDodged: true`

**Display**: CharacterSheetDialog shows "Shots: N/rnd" in the Combat section.

---

## 7. Hit Resolution

### MAngband — `test_hit_norm()` in `cmd1.c`

Used for melee attacks against monsters and players.

```c
bool test_hit_norm(int chance, int ac, int vis)
{
    int k = randint0(100);

    // 5% always miss, 5% always hit (bypasses all other checks)
    if (k < 10) return (k < 5);

    // Zero or negative chance always misses
    if (chance <= 0) return FALSE;

    // Invisible target: halve effective chance (rounded up)
    if (!vis) chance = (chance + 1) / 2;

    // Roll: chance vs. 75% of target's AC
    return randint0(chance) >= (ac * 3 / 4);
}
```

**Attack quality** (`chance`) is computed as:
```
chance = skill_thn + (to_h_weapon + to_h_player) * BTH_PLUS_ADJ
```

**AC contribution**: Only 75% of AC is effective (`ac * 3 / 4`). This prevents high AC from ever being a complete immunity.

**Invisible target penalty**: Halves effective `chance`. A player with `chance = 100` has an effective `chance = 50` against an invisible enemy.

### Ranged — Same formula, different inputs

Same mechanic as `test_hit_norm()`, but `chance` is derived from `skill_thb` instead of `skill_thn`.

### IronHell Implementation

`classicTestHitNorm(chance, ac, rng, vis?)` in `combat.ts` — exact MAngband equivalent:
- `vis=false` halves effective chance (rounded up)
- 5% always-miss / 5% always-hit thresholds
- `rand0(chance) >= ac*3/4` main check

---

## 8. Implementation Status Summary

| Feature                            | MAngband | IronHell      | Gap / Note                                        |
|------------------------------------|----------|---------------|---------------------------------------------------|
| Blows per round (table)            | ✅       | ✅            | Heavy_wield cap not enforced                      |
| Blows displayed to player          | ✅       | ✅            | `derivedStats.blowsPerRound` → CharacterSheet     |
| Shots per round (num_fire)         | ✅       | ✅            | Rangers +1 at lev 20/40 with bows                 |
| Shots displayed to player          | ✅       | ✅            | `derivedStats.shotsPerRound` → CharacterSheet     |
| Melee crit tiers (5 tiers)         | ✅       | ✅            | critTier returned and shown in combat log         |
| Ranged crit tiers (3 tiers)        | ✅       | ✅            | critTier returned, shown in log + CharacterSheet  |
| Ranged crit% shown to player       | ✅       | ✅            | CharacterSheet "RCrit%" computed from ammo weight |
| Melee hit resolution               | ✅       | ✅            | `classicTestHitNorm` — exact formula              |
| Invisible target penalty           | ✅       | ✅            | `vis=false` halves effective chance               |
| Backstab (rogue)                   | ✅       | ✅            | 3×+lev/40 sleeping, 1.5× fleeing                 |
| Two-handed weapon restriction      | ❌       | ❌            | Neither enforces; FAangband does                  |
| Shield AC penalty (two-handed)     | ❌       | ❌            | FAangband reduces to ⅔                           |
| Dual wielding                      | ❌       | ✅ (custom)   | 0.85× damage, −9 to-hit                          |
| Heavy wield penalty                | ✅       | ❌            | Planned gap                                       |
| Equipment extra_shots              | ✅       | ❌            | Gear-based shot bonus not yet implemented         |
