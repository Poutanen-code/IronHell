# Character Compendium

> Reference document covering character creation, races, classes, stat mechanics, experience progression, skills, death, and all related character systems.  
> Source files: `reference-mangband-1_5_3/src/server/birth.c`, `xtra1.c`, `xtra2.c`, `tables.c`, `mdefines.h`, `common/defines.h`, `data/races_and_classes.json`.

---

## Table of Contents

1. [Character Constants](#1-character-constants)
2. [The Six Stats](#2-the-six-stats)
3. [Stat Scale and Limits](#3-stat-scale-and-limits)
4. [Character Creation — Stat Rolling](#4-character-creation--stat-rolling)
5. [Starting Gold](#5-starting-gold)
6. [Social Class and Background History](#6-social-class-and-background-history)
7. [Age, Height, and Weight](#7-age-height-and-weight)
8. [Races](#8-races)
9. [Classes](#9-classes)
10. [Derived Skill Calculation](#10-derived-skill-calculation)
11. [Blows Per Round](#11-blows-per-round)
12. [Mana Calculation](#12-mana-calculation)
13. [Hitpoints — Rolling and Calculation](#13-hitpoints--rolling-and-calculation)
14. [Experience and Level Progression](#14-experience-and-level-progression)
15. [Character Titles](#15-character-titles)
16. [Speed and Encumbrance](#16-speed-and-encumbrance)
17. [Hunger and Food](#17-hunger-and-food)
18. [Status Conditions](#18-status-conditions)
19. [Light Radius](#19-light-radius)
20. [Resistances and Immunities](#20-resistances-and-immunities)
21. [Sustain Stats](#21-sustain-stats)
22. [Temporary Combat Buffs](#22-temporary-combat-buffs)
23. [Death, Ghosts, and Resurrection](#23-death-ghosts-and-resurrection)

---

## 1. Character Constants

From `common/defines.h` and `mdefines.h`:

| Constant | Value | Meaning |
|----------|-------|---------|
| `PY_MAX_LEVEL` | 50 | Maximum player level |
| `PY_MAX_EXP` | 99,999,999 | Maximum experience points |
| `A_MAX` | 6 | Number of stats (STR, INT, WIS, DEX, CON, CHR) |
| `MAX_RACES` | 11 | Total playable races |
| `MAX_CLASS` | 6 | Total playable classes |
| `PY_MAX_SPELLS` | 64 | Maximum spells per realm |
| `INVEN_TOTAL` | 36 | Total inventory + equipment slots |
| Stat absolute max | 18/220+ | Stat index 37 (the effective ceiling) |
| `BTH_PLUS_ADJ` | 3 | Bonus to-hit per +1 weapon to-hit enchantment |

---

## 2. The Six Stats

| Index | Name | Abbreviation | Primary Uses |
|-------|------|-------------|-------------|
| 0 | Strength | STR | Melee to-hit/damage, weapon weight limit, carry weight, blows per round, digging |
| 1 | Intelligence | INT | Mana (Mage/Ranger), disarming bonus, magic device bonus, spell failure |
| 2 | Wisdom | WIS | Mana (Priest/Paladin), saving throw bonus, spell failure |
| 3 | Dexterity | DEX | Melee to-hit/AC, disarming bonus, blows per round, ranged attacks |
| 4 | Constitution | CON | HP per level bonus, carry weight (minor) |
| 5 | Charisma | CHR | Store prices (minor); social class modifier |

Stats affect play through **index tables** (`stat_ind`), not linearly. The same stat number maps to a table index that drives lookup arrays for blows, mana, to-hit, etc. This makes high stats progressively more powerful.

---

## 3. Stat Scale and Limits

MAngband uses an Angband-style stat scale:

```
  3 (minimum)
  4, 5, 6, ..., 17
  18           ← "18 flat"
  18/01 (=19), 18/02, ..., 18/09
  18/10, 18/20, ..., 18/90, 18/100
  18/110, ..., 18/200, 18/210, 18/220+  ← hard cap
```

The stat display shows `18/XXX` for any stat above 18. For example, `18/100` represents +100 above 18. The **hard display ceiling** is `18/***` (18/220+, stat index 37).

**Stat display as shown on the character screen:**

```
3–18           → displayed literally
18/01–18/09    → "18/0X"
18/10–18/99    → "18/XX"
18/100–18/219  → "18/XXX"
18/220+        → "18/***"
```

**`modify_stat_value(value, amount)`** — the function that applies racial/class/item modifiers:

- Positive amounts: below 18 → +1 per step; at or above 18 → +10 per step.
- Negative amounts: above 18+10 → -10 per step; at 18+01–18+09 → clamp to 18; at 18 or below → -1 per step; minimum 3.

**Stat drain** (from monster attacks) reduces `stat_cur` but not `stat_max`. Restoration potions restore `stat_cur` to `stat_max`. Stat gain potions raise `stat_max` permanently, up to 18/100.

---

## 4. Character Creation — Stat Rolling

Source: `birth.c: get_stats()`

### Dice Formula

Each stat is rolled as:

```
stat[i] = 5 + 1d3 + 1d4 + 1d5   (per stat, from groups of 3 dice out of 18 total)
```

The 18 dice are rolled in 6 groups of 3, one group per stat. Total pool sum must satisfy `42 < total < 54`.

### Guaranteed Minimums

The roll repeats until these conditions are met — ensuring the result is always usable:

- **At least one stat ≥ 17**
- **At least two stats ≥ 16**
- **At least three stats ≥ 15**

### Stat Assignment

After rolling, the 6 values are **sorted descending** and assigned to stats in the **player's chosen priority order** (`stat_order[6]`). The highest rolled value goes to the stat the player considers most important.

### Racial/Class Adjustment

After assignment, each stat receives `race_bonus + class_bonus` via `modify_stat_value()`. In **maximize mode** (the standard option), this always grants the full bonus deterministically (`value + 10 per positive step`). In non-maximize mode, the adjustment uses some randomness.

### Stat Index Mapping

After full adjustment, the final stat value is converted to a `stat_ind` (0–37) used for all lookup tables:

```c
if (use <= 18)        ind = use - 3;           // ind 0..15
else if (use <= 18+219) ind = 15 + (use-18)/10; // ind 15..36
else                  ind = 37;                  // ind 37 = 18/220+
```

---

## 5. Starting Gold

Source: `birth.c: get_money()`

```
gold = (social_class × 6) + 1d100 + 300
```

Deductions per high stat:
- stat ≥ 18+50 → −300
- stat ≥ 18+20 → −200
- stat > 18 → −150
- stat 9–18 → −(stat − 8) × 10

Female characters receive a flat +50 gold bonus.

**Minimum:** 100 gold regardless of deductions.

At depth = 0 with level ≥ 5, characters who resurrect receive 100 gold as a restart stipend.

---

## 6. Social Class and Background History

Source: `birth.c: get_history()`

**Social class** (1–100) is computed by walking a race-specific history chart, rolling 1d100 at each node. Each node has a `bonus` field (offset from 50); the running `social_class` is modified by `+bonus − 50` at each step. Final value is clamped to 1–100.

Social class affects:
- Starting gold (`gold = sc × 6 + ...`)
- Displayed background text (4 lines, 60 chars each)

Background text is **gender-aware** — pronouns (you/he/she, your/his/her, are/is, have/has) adapt to the chosen sex.

---

## 7. Age, Height, and Weight

Source: `birth.c: get_ahw()`

```
age = race.b_age + 1d(race.m_age)
```

Height and weight use `randnor()` (normal distribution):

```
ht = randnor(base_height, height_mod)   // Gaussian around the race mean
wt = randnor(base_weight, weight_mod)
```

Race-specific values (from `data/races_and_classes.json`):

| Race | Base Age | Max Age Mod | Male Ht (in) | Male Wt (lb) |
|------|----------|-------------|-------------|------------|
| Human | 14 | +6 | 72±6 | 180±25 |
| Half-Elf | 24 | +16 | 66±6 | 130±15 |
| Elf | 75 | +75 | 60±4 | 100±6 |
| Hobbit | 21 | +12 | 36±3 | 60±3 |
| Gnome | 50 | +40 | 42±3 | 90±6 |
| Dwarf | 35 | +15 | 48±3 | 150±10 |
| Half-Orc | 11 | +4 | 66±1 | 150±5 |
| Half-Troll | 20 | +10 | 96±10 | 250±50 |
| Dúnadan | 50 | +20 | 82±5 | 190±20 |
| High-Elf | 100 | +30 | 90±10 | 190±20 |
| Kobold | 15 | +10 | 38±4 | 70±5 |

Female characters use separate (typically lower) height and weight base values.

---

## 8. Races

MAngband has **11 playable races**. All races grant racial flags that are always active (cannot be drained). Source: `data/races_and_classes.json`.

### Race Stat Modifiers

| Race | STR | INT | WIS | DEX | CON | CHR | HD Mod | XP Factor |
|------|-----|-----|-----|-----|-----|-----|--------|-----------|
| Human | 0 | 0 | 0 | 0 | 0 | 0 | +10 | ×1.00 |
| Half-Elf | 0 | +1 | −1 | +1 | −1 | +1 | +10 | ×1.10 |
| Elf | −1 | +2 | −1 | +1 | −2 | +1 | +9 | ×1.20 |
| Hobbit | −2 | +2 | +1 | +3 | +2 | +1 | +7 | ×1.10 |
| Gnome | −1 | +2 | 0 | +2 | +1 | −2 | +8 | ×1.25 |
| Dwarf | +2 | −3 | +2 | −2 | +2 | −3 | +11 | ×1.20 |
| Half-Orc | +2 | −1 | 0 | 0 | +1 | −4 | +10 | ×1.10 |
| Half-Troll | +4 | −4 | −2 | −4 | +3 | −6 | +12 | ×1.20 |
| Dúnadan | +1 | +2 | +2 | +2 | +3 | +2 | +10 | ×1.80 |
| High-Elf | +1 | +3 | −1 | +3 | +1 | +5 | +10 | ×2.00 |
| Kobold | −1 | −1 | 0 | +2 | +2 | −2 | +8 | ×1.15 |

**HD Mod** = racial hit die contribution (added to class HD to form `hitdie`).

### Race Skill Modifiers

Race contributes base values to all 8 skills. Higher values = better skill at level 1. Class growth rates (`x_*`) determine how skills improve with level.

| Race | Dis | Dev | Sav | Stl | Srh | Fos | Thn | Thb |
|------|-----|-----|-----|-----|-----|-----|-----|-----|
| Human | 0 | 0 | 0 | 0 | 10 | 10 | 0 | 0 |
| Half-Elf | 2 | 3 | 3 | 1 | 6 | 11 | −1 | 5 |
| Elf | 5 | 6 | 6 | 1 | 8 | 12 | −5 | 15 |
| Hobbit | 15 | 18 | 18 | 4 | 12 | 15 | −10 | 20 |
| Gnome | 10 | 12 | 12 | 3 | 6 | 13 | −8 | 12 |
| Dwarf | 2 | 9 | 9 | −1 | 7 | 10 | 15 | 0 |
| Half-Orc | −3 | −3 | −3 | −1 | 0 | 7 | 12 | −5 |
| Half-Troll | −5 | −8 | −8 | −2 | −1 | 5 | 20 | −10 |
| Dúnadan | 4 | 5 | 5 | 2 | 3 | 13 | 15 | 10 |
| High-Elf | 4 | 20 | 20 | 3 | 3 | 14 | 10 | 25 |
| Kobold | 10 | 5 | 0 | 4 | 15 | 15 | −5 | 10 |

*Note: `Stl` = Stealth; `Fos` = Search Frequency; `Srh` = Search Ability; `Thn` = Melee; `Thb` = Ranged.*

### Racial Flags and Abilities

| Race | Racial Flags | Innate Abilities |
|------|-------------|-----------------|
| Human | — | None |
| Half-Elf | `SUST_DEX` | Dexterity sustained |
| Elf | `SUST_DEX`, `RES_LITE` | DEX sustained; resists light |
| Hobbit | `HOLD_LIFE` | Experience drain resistant |
| Gnome | `FREE_ACT` | Immune to paralysis |
| Dwarf | `RES_BLIND` | Resists blindness |
| Half-Orc | `RES_DARK` | Resists darkness |
| Half-Troll | `SUST_STR`, `REGEN` | STR sustained; faster HP regen |
| Dúnadan | `SUST_CON` | CON sustained |
| High-Elf | `RES_LITE`, `SEE_INVIS` | Resists light; sees invisible |
| Kobold | `RES_POIS` | Resists poison |

### Racial Class Restrictions

| Race | Allowed Classes |
|------|----------------|
| Human | All 6 |
| Half-Elf | All 6 |
| Elf | Warrior, Mage, Rogue, Ranger |
| Hobbit | Warrior, Rogue, Ranger |
| Gnome | Warrior, Mage, Priest, Rogue |
| Dwarf | Warrior, Priest, Paladin |
| Half-Orc | Warrior, Priest, Rogue |
| Half-Troll | Warrior, Priest |
| Dúnadan | Warrior, Mage, Priest, Ranger, Paladin |
| High-Elf | Warrior, Mage, Priest, Ranger |
| Kobold | Warrior, Priest, Rogue, Ranger |

### Racial Infravision

| Race | Infravision Range (tiles) |
|------|--------------------------|
| Human | 0 (none) |
| Half-Elf | 2 |
| Elf | 3 |
| Half-Orc | 3 |
| Half-Troll | 3 |
| Gnome | 4 |
| Hobbit | 4 |
| High-Elf | 4 |
| Dwarf | 5 |
| Kobold | 5 |
| Dúnadan | 0 (none) |

---

## 9. Classes

MAngband has **6 playable classes**. The class contributes stat modifiers, base/growth skill values, a hit die bonus, a mana die per level, an experience factor, and a set of class flags.

### Class Stat Modifiers

| Class | STR | INT | WIS | DEX | CON | CHR | HD Bonus | Mana Die | XP Factor |
|-------|-----|-----|-----|-----|-----|-----|----------|----------|-----------|
| Warrior | +5 | −2 | −2 | +2 | +2 | −1 | +9 | 0 | +0 |
| Mage | −5 | +3 | 0 | +1 | −2 | +1 | 0 | +6 | +30 |
| Priest | −1 | −3 | +3 | −1 | 0 | +2 | +2 | +5 | +20 |
| Rogue | +2 | +1 | −2 | +3 | +1 | −1 | +6 | +3 | +25 |
| Ranger | +2 | +2 | 0 | +1 | +1 | +1 | +4 | +4 | +30 |
| Paladin | +3 | −3 | +1 | 0 | +2 | +2 | +6 | +4 | +35 |

**Total hit die = racial HD mod + class HD bonus.** For example, a Human Mage has hitdie = 10 + 0 = 10. A Half-Troll Warrior = 12 + 9 = 21.

### Class Base Skills (Level 1) and Growth

Skills are computed as: `base = race_base + class_base`, then add `class_growth × level / 10` per level (see §10).

| Class | Dis B/G | Dev B/G | Sav B/G | Stl B/G | Srh B/G | Fos B/G | Thn B/G | Thb B/G |
|-------|---------|---------|---------|---------|---------|---------|---------|---------|
| Warrior | 25/10 | 18/7 | 18/10 | 1/0 | 14/0 | 2/0 | 70/45 | 55/45 |
| Mage | 30/7 | 36/13 | 30/9 | 2/0 | 16/0 | 20/0 | 34/15 | 20/15 |
| Priest | 25/7 | 30/10 | 32/12 | 2/0 | 16/0 | 8/0 | 48/20 | 35/20 |
| Rogue | 45/15 | 32/10 | 28/10 | 5/0 | 32/0 | 24/0 | 60/40 | 66/30 |
| Ranger | 30/8 | 32/10 | 28/10 | 3/0 | 24/0 | 16/0 | 56/30 | 72/45 |
| Paladin | 20/7 | 24/10 | 25/11 | 1/0 | 12/0 | 2/0 | 68/35 | 40/30 |

*(B = base contribution from class at level 1; G = growth per 10 levels via `x_* × lev/10`)*

### Class Flags

| Flag | Classes | Effect |
|------|---------|--------|
| `BRAVERY_30` | Warrior | Immune to fear at level 30+ |
| `PSEUDO_ID_HEAVY` | Warrior, Paladin, Rogue | Better pseudo-identification of items |
| `PSEUDO_ID_IMPROV` | Warrior, Priest, Rogue, Ranger, Paladin | Pseudo-ID improves with level |
| `CUMBER_GLOVE` | Mage, Rogue, Ranger | Wearing non-Free-Act gloves reduces mana by 25% |
| `ZERO_FAIL` | Mage, Priest | Minimum spell failure rate can reach 0% |
| `BEAM` | Mage | Spells may be fired as beams at high level |
| `CHOOSE_SPELLS` | Mage, Rogue, Ranger | Can choose which spells to learn |
| `HP_BONUS` | Mage | Gains +1 HP per level (up to ~lvl 30) |
| `BLESS_WEAPON` | Priest | −2 to-hit/to-dam penalty for non-blessed edged weapons |
| `BACK_STAB` | Rogue | 3×+(level/40) damage multiplier vs sleeping monsters; 1.5× vs fleeing |
| `STEALING_IMPROV` | Rogue | Steal skill improves with level |
| `SPEED_BONUS` | Rogue | +1 speed every 15 levels starting at level 5 |
| `STEALTH_MODE` | Rogue | Toggle stealth mode: ×3 stealth skill but −10 speed |
| `EXTRA_SHOT` | Ranger | +1 bow shot at level 20, +1 more at level 40 |

### Spell Systems by Class

| Class | Spell Stat | Spell Book | First Spell Level | Max Attacks |
|-------|-----------|-----------|-------------------|-------------|
| Warrior | — | None | — | 6 |
| Mage | INT | Magic books | 1 | 4 |
| Priest | WIS | Prayer books | 1 | 4 |
| Rogue | INT | Magic books | 5 | 5 |
| Ranger | INT | Magic books | 3 | 5 |
| Paladin | WIS | Prayer books | 1 | 5 |

### Priest Weapon Restriction

Priests suffer **−2 to-hit / −2 to-dam** when wielding edged weapons (swords, polearms) that are not blessed. `BLESS_WEAPON` flag on an item removes this penalty.

### Class Equipment Encumbrance

Caster classes that wear heavy armor accumulate **cumber armor** penalty once total worn weight exceeds `spell_weight` (class-specific). Each 10 lbs over the limit reduces mana by 1. This affects Mages, Priests, Rangers, and Paladins.

### Class Titles

Each class has 10 titles at intervals of 5 levels (levels 1–5, 6–10, ..., 46–50):

| Level | Warrior | Mage | Priest | Rogue | Ranger | Paladin |
|-------|---------|------|--------|-------|--------|---------|
| 1–5 | Rookie | Novice | Believer | Vagabond | Runner | Gallant |
| 6–10 | Soldier | Apprentice | Acolyte | Cutpurse | Strider | Keeper |
| 11–15 | Swordsman | Trickster | Adept | Footpad | Scout | Protector |
| 16–20 | Swashbuckler | Illusionist | Evangelist | Robber | Courser | Defender |
| 21–25 | Veteran | Spellbinder | Priest | Burglar | Tracker | Warder |
| 26–30 | Myrmidon | Evoker | Curate | Filcher | Guide | Knight |
| 31–35 | Commando | Conjurer | Canon | Sharper | Explorer | Guardian |
| 36–40 | Champion | Warlock | Bishop | Rogue | Pathfinder | Chevalier |
| 41–45 | Hero | Sorcerer | Prophet | Thief | Ranger | Paladin |
| 46–50 | Lord | Arch-Mage | Patriarch | Master Thief | Ranger Lord | Paladin Lord |

At level 50+ (total winner), the title becomes **\*\*KING\*\*** or **\*\*QUEEN\*\*** regardless of class.  
A ghost character displays the title **"Ghost"**; a polymorph victim displays **"Fruitbat"**.

---

## 10. Derived Skill Calculation

Source: `xtra1.c: calc_bonuses()`

All 8 skills are computed fresh on each recalculation from:

```
skill = race_base + class_base
      + class_growth × player_level / 10      [integer division]
      + stat_table_bonus(relevant_stat)
      + equipment_bonus
```

### Skill Formula Details

| Skill | Stat Bonus Source | Equipment Bonus |
|-------|------------------|----------------|
| `skill_dis` | `adj_dex_dis[dex_ind] + adj_int_dis[int_ind]` | None |
| `skill_dev` | `adj_int_dev[int_ind]` | None |
| `skill_sav` | `adj_wis_sav[wis_ind]` | None |
| `skill_stl` | +1 flat (always) | `TR1_STEALTH: += item.pval` |
| `skill_srh` | None | `TR1_SEARCH: += item.pval × 5`; `bpval × 5` for base item |
| `skill_fos` | None | `TR1_SEARCH: += item.pval × 5` (same flag as srh) |
| `skill_thn` | None | Weapon `to_h × BTH_PLUS_ADJ` (added during combat, not here) |
| `skill_thb` | None | None |

### Skill Caps and Limits

- **Stealth** (`skill_stl`): hard capped at 30; minimum 0. In stealth mode (Rogue only): ×3 multiplier applied after cap.
- **Digging** (`skill_dig`): minimum 1; gains from weapon weight (`weapon.weight / 10`) and STR table.
- **Infravision** (`see_infra`): minimum 0; never negative.
- Other skills: no hard upper cap in the formula (practically bounded by max stat/level).

### Skill Display Divisors

Skills are shown on the character screen divided by a display factor:

| Skill | Display Divisor | Example: raw 60 → shown |
|-------|----------------|------------------------|
| Fighting (Thn) | 12 | 5 |
| Shooting (Thb) | 12 | 5 |
| Saving Throw | 6 | 10 |
| Stealth | 1 | 60 (capped at 30 raw) |
| Perception (Fos) | 6 | 10 |
| Searching (Srh) | 6 | 10 |
| Disarming | 8 | 7 |
| Magic Devices | 6 | 10 |

---

## 11. Blows Per Round

Source: `xtra1.c: calc_bonuses()`, `tables.c: blows_table`

Number of melee blows uses a 2D lookup table indexed by STR power (P) and DEX speed (D):

```
P = (adj_str_blow[str_ind] × class.att_multiply) / max(weapon_weight, class.min_weight)
D = adj_dex_blow[dex_ind]
P capped at 11; D capped at 11
num_blow = blows_table[P][D]
```

### Blows Table (excerpt — full 12×12 grid)

```
        D→  0   1   2   3   4   5   6   7   8   9  10  11 P ↓
 0      :   1   1   1   1   1   1   2   2   2   2   2   3
 1      :   1   1   1   1   2   2   3   3   3   4   4   4
 2      :   1   1   2   2   3   3   4   4   4   5   5   5
 3      :   1   2   2   3   3   4   4   4   5   5   5   5
 4      :   1   2   2   3   3   4   4   5   5   5   5   5
 5      :   2   2   3   3   4   4   5   5   5   5   5   6
 6      :   2   2   3   3   4   4   5   5   5   5   5   6
 7      :   2   3   3   4   4   4   5   5   5   5   5   6
 8      :   3   3   3   4   4   4   5   5   5   5   6   6
 9      :   3   3   4   4   4   4   5   5   5   5   6   6
10      :   3   3   4   4   4   4   5   5   5   6   6   6
11      :   3   3   4   4   4   4   5   5   6   6   6   6
```

### Per-Class Parameters

| Class | `att_multiply` | `min_weight` (tenths lb) | `max_attacks` |
|-------|---------------|--------------------------|---------------|
| Warrior | 5 | 30 | 6 |
| Mage | 2 | 40 | 4 |
| Priest | 3 | 35 | 4 |
| Rogue | 3 | 30 | 5 |
| Ranger | 4 | 35 | 5 |
| Paladin | 5 | 30 | 5 |

**After table lookup:**
1. Cap at `max_attacks`.
2. Add `extra_blows` (from equipment flags `TR1_BLOWS`).
3. Minimum 1 blow always.

### Heavy Weapon Penalty

If `adj_str_hold[str_ind] < weapon.weight / 10`, the weapon is too heavy:
- `to_h -= 2 × (hold - weapon_weight/10)` — severe accuracy penalty.
- `heavy_wield = TRUE` — no blow count lookup (stays at 1).

---

## 12. Mana Calculation

Source: `xtra1.c: calc_mana()`

Only applies to classes with a `spell_book`. Spell stat is INT (Mage, Rogue, Ranger) or WIS (Priest, Paladin).

```
levels = player_level - spell_first_level + 1   (min 0)
mana   = adj_mag_mana[spell_stat_ind] × levels / 100
if (mana > 0): mana += 1
```

### `adj_mag_mana` Table (selected values)

| Spell Stat | Mana Factor | Example: lvl 20 Mage |
|-----------|------------|---------------------|
| 3 | 0 | 0 mana |
| 10 | 80 | (80×20)/100 + 1 = 17 |
| 13 | 100 | (100×20)/100 + 1 = 21 |
| 18 | 150 | (150×20)/100 + 1 = 31 |
| 18/50 | 200 | (200×20)/100 + 1 = 41 |
| 18/100 | 400 | (400×20)/100 + 1 = 81 |
| 18/150 | 650 | (650×20)/100 + 1 = 131 |
| 18/220+ | 800 | (800×20)/100 + 1 = 161 |

The mana factor increases smoothly from 0 to 800, with a cap at 800 for 18/180+.

### Mana Penalties

**Cumber Glove** (Mage, Rogue, Ranger): Wearing gloves that lack `FREE_ACT` and have no positive DEX bonus → mana ×¾.

**Cumber Armor**: Total worn armor weight (body + head + shield + cloak + gloves + boots) above `class.spell_weight` → mana −1 per 10 lbs over limit. Mana is floored at 0.

---

## 13. Hitpoints — Rolling and Calculation

Source: `birth.c: get_extra()`, `xtra1.c: calc_hitpoints()`

### Hit Die

```
hitdie = race.r_mhp + class.c_mhp
```

The sum of racial and class HD contributions.

| Class contribution | HD |
|-------------------|----|
| Warrior | +9 |
| Mage | 0 |
| Priest | +2 |
| Rogue | +6 |
| Ranger | +4 |
| Paladin | +6 |

| Racial contribution | HD |
|--------------------|----|
| Human | +10 |
| Half-Elf | +10 |
| Elf | +9 |
| Hobbit | +7 |
| Gnome | +8 |
| Dwarf | +11 |
| Half-Orc | +10 |
| Half-Troll | +12 |
| Dúnadan | +10 |
| High-Elf | +10 |
| Kobold | +8 |

### HP Array Rolling

At character creation, a 50-element HP array is pre-rolled:

```
player_hp[0] = hitdie          (level 1 = full hitdie)
player_hp[i] = player_hp[i-1] + 1d(hitdie)   for i = 1..49
```

The roll repeats until the level-50 total falls within:

```
min_value = (49 × (hitdie - 1) × 3) / 8 + 50
max_value = (49 × (hitdie - 1) × 5) / 8 + 50
```

This guarantees final HP is roughly in the middle 25%–75% of the theoretical range.

### Runtime HP Calculation

```
mhp = player_hp[level - 1]
    + adj_con_mhp[con_ind] × level / 100   [CON bonus]
```

Minimum: `level + 1` HP always guaranteed.

**Modifiers:**
- Mage with `CF_HP_BONUS`: +level HP (extra survivability).
- Hero effect: +10 temporary HP.
- Berserk (Shero) effect: +30 temporary HP.

**Ghost and Fruit Bat override:** `mhp = level + 2` (very fragile ghost form).

---

## 14. Experience and Level Progression

Source: `tables.c: player_exp[]`, `xtra2.c: check_experience()`, `birth.c: get_extra()`

### Experience Factor

```
expfact = race.r_exp + class.c_exp
```

This is added together (not multiplied). The effective XP needed for each level threshold is:

```
xp_needed(level) = player_exp[level - 1] × expfact / 100
```

| Race | r_exp | Class | c_exp |
|------|-------|-------|-------|
| Human | 100 | Warrior | 0 |
| Half-Elf | 110 | Mage | 30 |
| Elf | 120 | Priest | 20 |
| Hobbit | 110 | Rogue | 25 |
| Gnome | 125 | Ranger | 30 |
| Dwarf | 120 | Paladin | 35 |
| Half-Orc | 110 | | |
| Half-Troll | 120 | | |
| Dúnadan | 180 | | |
| High-Elf | 200 | | |
| Kobold | 115 | | |

**Example — Human Warrior (expfact = 100):** Thresholds are the raw table values.  
**Example — High-Elf Paladin (expfact = 200 + 35 = 235):** Each threshold is 2.35× harder.

### Base Experience Table

The `player_exp[50]` array (0-indexed) gives the raw XP for each level boundary:

| Level | Raw XP | Level | Raw XP | Level | Raw XP |
|-------|--------|-------|--------|-------|--------|
| 1→2 | 10 | 11→12 | 650 | 21→22 | 6,800 |
| 2→3 | 25 | 12→13 | 850 | 22→23 | 8,400 |
| 3→4 | 45 | 13→14 | 1,100 | 23→24 | 10,200 |
| 4→5 | 70 | 14→15 | 1,400 | 24→25 | 12,500 |
| 5→6 | 100 | 15→16 | 1,800 | 25→26 | 17,500 |
| 6→7 | 140 | 16→17 | 2,300 | 26→27 | 25,000 |
| 7→8 | 200 | 17→18 | 2,900 | 27→28 | 35,000 |
| 8→9 | 280 | 18→19 | 3,600 | 28→29 | 50,000 |
| 9→10 | 380 | 19→20 | 4,400 | 29→30 | 75,000 |
| 10→11 | 500 | 20→21 | 5,400 | 30→31 | 100,000 |
| | | | | 31→32 | 150,000 |
| | | | | 32→33 | 200,000 |
| | | | | 33→34 | 275,000 |
| | | | | 34→35 | 350,000 |
| | | | | 35→36 | 450,000 |
| | | | | 36→37 | 550,000 |
| | | | | 37→38 | 700,000 |
| | | | | 38→39 | 850,000 |
| | | | | 39→40 | 1,000,000 |
| | | | | 40→41 | 1,250,000 |
| | | | | 41→42 | 1,500,000 |
| | | | | 42→43 | 1,800,000 |
| | | | | 43→44 | 2,100,000 |
| | | | | 44→45 | 2,400,000 |
| | | | | 45→46 | 2,700,000 |
| | | | | 46→47 | 3,000,000 |
| | | | | 47→48 | 3,500,000 |
| | | | | 48→49 | 4,000,000 |
| | | | | 49→50 | 4,500,000 |
| | | | | Max EXP | 5,000,000 (raw) |

*Absolute cap: `PY_MAX_EXP = 99,999,999`.*

### Level Up Behaviour

On reaching a new level (`check_experience()`):
- HP array recalculated (new random HP already pre-rolled at birth).
- Mana recalculated from new level.
- Skills recalculated (level term grows).
- Level broadcast to all players: "*Name* has attained level X."
- Multiples of 5 are logged to character history.

### Level Down (XP Loss)

If current XP drops below the threshold for the current level, the character **loses a level**:
- A level-down message is broadcast.
- All stats, HP, mana, and skills are recalculated.

### Max XP Tracking

`max_exp` is always `≥ exp`. When experience is gained while `exp < max_exp` (after drain), `max_exp` also rises, but only at 10% the rate of actual XP gain (`max_exp += amount / 10`). This prevents trivially restoring lost levels by grinding XP.

### Experience Drain

Per periodic tick (see SYSTEMS_COMPENDIUM §2) when the `exp_drain` flag is set (from cursed/drain items):

```
10% chance per tick:
  amount = 1 + (p_ptr->exp / randint1(100000))
  lose_exp(amount)
```

Characters with `HOLD_LIFE` (Hobbit racial, or equipment flag) are completely immune to XP drain from monster blows.

---

## 15. Character Titles

Titles change every 5 levels (10 titles total, one per 5-level band), determined by `c_text + cp_ptr->title[(lev-1)/5]`. See the title table in §9.

---

## 16. Speed and Encumbrance

Source: `xtra1.c: calc_bonuses()`

**Base speed:** 110 (normal). The `pspeed` field is used by the energy system (see SYSTEMS_COMPENDIUM §1).

### Speed Sources

| Source | Effect |
|--------|--------|
| Base | +110 |
| Fast spell | +10 |
| Slow spell | −10 |
| Ring/equipment `TR1_SPEED` | +pval |
| Hero/Shero | no speed change (HP/to-hit only) |
| Fruit Bat | +10 (always) |
| Rogue speed bonus | +1 per 15 levels from level 5 |
| Dungeon Master | +50 |

### Encumbrance Penalty

```
carry_limit  = adj_str_wgt[str_ind] × 100   (in tenths of pounds)
current_load = sum of all inventory weights

if (load > carry_limit / 2):
    speed -= (load - carry_limit/2) / (carry_limit / 10)
```

A character carrying half their weight limit experiences no penalty. Beyond that, each `carry_limit/10` lbs over half the limit costs 1 speed.

Gorged state (`food >= PY_FOOD_MAX = 15000`): −10 speed.

Searching mode active: −10 speed (Rogue stealth mode also −10, but ×3 stealth bonus).

**Speed is hard-capped between 0 and 199 after all modifiers.**

---

## 17. Hunger and Food

Source: `mdefines.h`, `xtra1.c: periodic processing`

### Food Thresholds

| Threshold | Value | Status |
|-----------|-------|--------|
| `PY_FOOD_MAX` | 15000 | Gorged (−10 speed) |
| `PY_FOOD_FULL` | 10000 | Full (normal) |
| `PY_FOOD_ALERT` | 2000 | Hungry (warning) |
| `PY_FOOD_WEAK` | 1000 | Weak |
| `PY_FOOD_FAINT` | 500 | Fainting / Starving |
| `PY_FOOD_STARVE` | 100 | Starving (extreme) |

Starting food: `PY_FOOD_FULL - 1` (just below full).

### Digestion Rate

Food decreases on the food sub-timer (10× slower than the standard periodic tick):

- Normal: −4 food per food-tick.
- `slow_digest` flag (from equipment or class): −2 food per food-tick (halved).

At `< PY_FOOD_FAINT`: character may faint (set paralyzed briefly), triggering `disturb(stop_search=1)`.

---

## 18. Status Conditions

### Cut (Bleeding)

Accumulated `cut` value. Reduces HP per periodic tick and blocks HP regeneration when `cut >= 100`.

| cut Value | Severity |
|-----------|----------|
| 1–10 | Minor cut (1 DoT/tick) |
| 11–25 | Light cut (1 DoT/tick) |
| 26–50 | Bad cut (2 DoT/tick) |
| 51–100 | Severe cut (2 DoT/tick) |
| 101–200 | Deep cut (3 DoT/tick); blocks regen |
| 201–1000 | Mortal wound (lethal rate); blocks regen |
| >1000 | Instant kill territory |

### Stun

Accumulated `stun` value from blows.

| stun Value | Effect |
|-----------|--------|
| 1–50 | Light stun: −5 to-hit, −5 to-dam |
| 51–100 | Heavy stun: −20 to-hit, −20 to-dam |
| ≥100 | **Heavy Stun**: no energy accrual (paralysis-equivalent) |

### Other Conditions

| Condition | Effect | Countered by |
|-----------|--------|-------------|
| **Blind** | Cannot see; search/sense halved (skill /10) | Cure Blindness, Potions |
| **Confused** | Random walk direction; search/attack penalties (skill /10) | Cure Confusion |
| **Afraid** | Cannot attack; flees; no auto-retaliate | Heroism, Berserker, Remove Fear |
| **Paralyzed** | No energy accrual; cannot act at all | Free Action (immunity), wears off |
| **Slow** | −10 speed | Wears off |
| **Fast** | +10 speed | Wears off |
| **Poisoned** | DoT 1 HP/tick; blocks HP regeneration | Antidotes, wears off |
| **Hallucinating (Image)** | Everything looks like random creatures/objects | Cured by Rest or Potions |
| **Invulnerable** | +100 to-AC; immune to most damage | Wears off |
| **Hero** | +12 to-hit, +10 HP, resist fear | Wears off |
| **Berserk (Shero)** | +24 to-hit, −10 AC, +30 HP, resist fear | Wears off |
| **Blessed** | +5 AC, +10 to-hit | Wears off |
| **Shield** | +50 AC | Wears off |

All timed conditions count down once per periodic tick. The `disturb()` mechanic (see SYSTEMS_COMPENDIUM §7) interrupts running and searching when conditions change significantly.

---

## 19. Light Radius

Source: `xtra1.c: calc_torch()`

```
cur_lite = p_ptr->lite       (equipment +LITE flags, base 0)
         + torch_bonus        (from equipped light source)
```

Light source bonuses:

| Item | Radius Bonus | Condition |
|------|-------------|-----------|
| Torch (sval 0) | +1 | Must have fuel (`pval > 0`) |
| Lantern (sval 1) | +2 | Must have fuel |
| Dwarven Lantern | +2 | Always (no fuel needed) |
| Feanorian Lantern | +3 | Always |
| Artifact Lite | +3 | Always |

`TR3_LITE` equipment flag: +1 radius per item bearing the flag.

Running with `VIEW_REDUCE_LITE` option active: radius capped at 1.

---

## 20. Resistances and Immunities

From equipment flags (`TR2_*`). Resistances halve damage from that element. Immunities prevent damage entirely.

**Resistances:** `RES_ACID`, `RES_ELEC`, `RES_FIRE`, `RES_COLD`, `RES_POIS`, `RES_FEAR`, `RES_LITE`, `RES_DARK`, `RES_BLIND`, `RES_CONFU`, `RES_SOUND`, `RES_SHARD`, `RES_NEXUS`, `RES_NETHR`, `RES_CHAOS`, `RES_DISEN`.

**Immunities:** `IM_ACID`, `IM_ELEC`, `IM_FIRE`, `IM_COLD`. Full immunity prevents all damage and secondary effects from that element.

**Racial resistances** (always on, not from equipment):

| Race | Built-in Resistance |
|------|-------------------|
| Elf / High-Elf | `RES_LITE` |
| Dwarf | `RES_BLIND` |
| Half-Orc | `RES_DARK` |
| Kobold | `RES_POIS` |
| Hobbit | `HOLD_LIFE` (not a standard resistance, blocks XP drain) |

See SYSTEMS_COMPENDIUM §13 and WEARABLES_COMPENDIUM for full equipment flag details.

---

## 21. Sustain Stats

Sustain flags (`TR2_SUST_*`) prevent stat drain from monster blows for the corresponding stat. Sources:
- Equipment affix flags
- Racial flags (see §8)

When a drain blow hits a sustained stat, the drain is fully blocked. Implemented in `sustainStats.ts`, wired through `blowEffectApplication.ts`.

---

## 22. Temporary Combat Buffs

| Buff | Source | to-hit | to-dam | to-AC | HP | Other |
|------|--------|--------|--------|-------|----|-------|
| Blessed | Spell | +10 | — | +5 | — | — |
| Hero | Spell | +12 | — | — | +10 | Resist fear |
| Berserk | Spell | +24 | — | −10 | +30 | Resist fear |
| Shield | Spell | — | — | +50 | — | — |
| Invulnerable | Spell | — | — | +100 | — | Near-total immunity |
| Fast | Spell | — | — | — | — | +10 speed |
| Slow | Monster | — | — | — | — | −10 speed |
| Light Stun | Damage | −5 | −5 | — | — | — |
| Heavy Stun | Damage | −20 | −20 | — | — | No energy |

---

## 23. Death, Ghosts, and Resurrection

Source: `xtra2.c: player_death()`, `resurrect_player()`

### Death Sequence

When `p_ptr->chp ≤ 0`, the server calls `player_death()`:

1. **Arena**: Cannot die in arena; instead lose consciousness and be evacuated (HP restored to `mhp - 1`).
2. **Dungeon Master**: `death = FALSE`; HP clamped to 0 — DMs are invulnerable.
3. **Ghost second death**: Ghost body fades permanently → `player_funeral()` (full deletion from server).
4. **Suicide / Winner**: Retirement message; artifacts released; `player_funeral()`.
5. **Normal death**:
   - All **unique monsters** the player killed (up to ~65% of max depth) **respawn** (`ressurect_uniques()`).
   - Character dump written.
   - **Items dropped on floor** (unless `NO_GHOST` / Ironman option — in which case items are destroyed and character is permanently deleted).
   - Player becomes a **Ghost**.

### Ghost State

After normal death:
- `mhp = lev + 2` (very fragile).
- All status conditions cleared (blind, paralyzed, confused, etc.).
- Food restored.
- `fruit_bat` set to 2.
- Teleported randomly.
- `ghost = 1` flag set.
- Character is **incorporeal** — can pass through walls.

The ghost must find a way to be resurrected (another player, a shrine, or server config).

### Resurrection

`resurrect_player()`:
- `ghost = 0` — corporeal again.
- **Lose 50% of both current and max XP**: `max_exp /= 2; exp /= 2`.
- Level rechecked (may drop levels due to XP loss).
- Small gold grant (100 gp) if in town and level ≥ 5.
- `lives++` counter incremented.

### Unique Monster Resurrection on Death

When a player dies, unique monsters they killed at depths within ~65% of their deepest-reached level (`max_dlv - max_dlv × 35%`) are brought back to life. This ensures the dungeon remains challenging for the next run. Morgoth (`RF1_DROP_CHOSEN`) is **never** resurrected by this mechanic.

### Ironman / No-Ghost Mode

With `cfg_ironman` or the `NO_GHOST` player option:
- **Permanent death** — no ghost phase.
- All items are destroyed (not dropped).
- Character is permanently deleted from the server.
- Death announcement includes "The brave hero..." prefix.

### Death Announcement Format

```
"<Name> the level <N> <Race> <Class> was killed by <source>."
```

Broadcast to all online players. High-score entry is added.

---

## Cross-References

For mechanics that overlap with other compendia:

| Topic | See Also |
|-------|---------|
| Skills (formulas, saving throws, search, stealth) | [SYSTEMS_COMPENDIUM.md](SYSTEMS_COMPENDIUM.md) §8–§10 |
| Melee hit resolution and critical hits | [SYSTEMS_COMPENDIUM.md](SYSTEMS_COMPENDIUM.md) §11–§12 |
| Energy system and turn processing | [SYSTEMS_COMPENDIUM.md](SYSTEMS_COMPENDIUM.md) §1–§2 |
| Periodic processing (DoT, regen, status ticks) | [SYSTEMS_COMPENDIUM.md](SYSTEMS_COMPENDIUM.md) §2 |
| Equipment flags (FREE_ACT, HOLD_LIFE, etc.) | [SYSTEMS_COMPENDIUM.md](SYSTEMS_COMPENDIUM.md) §14 |
| Ego items and artifacts | [SYSTEMS_COMPENDIUM.md](SYSTEMS_COMPENDIUM.md) §15, [WEARABLES_COMPENDIUM.md](WEARABLES_COMPENDIUM.md) |
| Spells and spell failure formula | [MAGIC_COMPENDIUM.md](MAGIC_COMPENDIUM.md) |
| Weapons and blow count in detail | [WEAPONS_COMPENDIUM.md](WEAPONS_COMPENDIUM.md) |
