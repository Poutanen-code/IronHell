# Character Creation Specification

> Date: 2026-08-07
> Status: Authoritative — supersedes CHARACTER_CREATION_PREREQUISITES.md and CHARACTER_STATE_DEFINITION.md
> Sources: `birth.c`, `types.h`, `mdefines.h`, `defines.h`, `init1.c`, `object2.c`, `p_class.txt`, `p_race.txt`, `p_hist.txt`, `tables.c`, `xtra1.c`, `races.json`, `classes.json`, `stat_tables.json`
>
> Purpose: Complete implementable specification for CharacterFactory and CharacterState. All rules are sourced. No rules are invented.

---

## Table of Contents

1. [Definitions and Constants](#1-definitions-and-constants)
2. [Character Creation Algorithm](#2-character-creation-algorithm)
3. [CharacterState Definition](#3-characterstate-definition)
4. [Starting Equipment Resolution](#4-starting-equipment-resolution)
5. [History Chart Dependency Model](#5-history-chart-dependency-model)
6. [Derived Calculations Reference](#6-derived-calculations-reference)
7. [Remaining Blockers](#7-remaining-blockers)
8. [Document Supersession](#8-document-supersession)

---

## 1. Definitions and Constants

### 1.1 Inventory Layout

Source: `mdefines.h`

| Constant | Value | Meaning |
|---|---|---|
| `INVEN_PACK` | 23 | Number of backpack slots (indices 0–22) |
| `INVEN_WIELD` | 24 | First equipment slot (main hand weapon) |
| `INVEN_BOW` | 25 | Ranged weapon slot |
| `INVEN_LEFT` | 26 | Left ring |
| `INVEN_RIGHT` | 27 | Right ring |
| `INVEN_NECK` | 28 | Amulet |
| `INVEN_LITE` | 29 | Light source |
| `INVEN_BODY` | 30 | Body armor |
| `INVEN_OUTER` | 31 | Cloak |
| `INVEN_ARM` | 32 | Shield |
| `INVEN_HEAD` | 33 | Helmet |
| `INVEN_HANDS` | 34 | Gloves |
| `INVEN_FEET` | 35 | Boots |
| `INVEN_TOTAL` | 36 | Total slots (pack + equipment) |

### 1.2 Game Constants

Source: `mdefines.h`, `defines.h`

| Constant | Value | Meaning |
|---|---|---|
| `PY_MAX_LEVEL` | 50 | Maximum player level |
| `PY_MAX_SPELLS` | 64 | Maximum spells per realm |
| `A_MAX` | 6 | Number of stats (STR=0, INT=1, WIS=2, DEX=3, CON=4, CHR=5) |
| `PY_FOOD_FULL` | 10000 | Full food level |
| `PY_NORMAL_SPEED` | 110 | Base player speed |
| `MAX_START_ITEMS` | 6 | Maximum class-specific starting item types |
| `BTH_PLUS_ADJ` | 3 | To-hit bonus per +1 weapon enchantment |

### 1.3 Item Type Values (tval)

Source: `mdefines.h`

| tval | Item Category |
|---|---|
| 17 | Arrow |
| 19 | Bow |
| 20 | Digging tool |
| 21 | Hafted weapon (maces, clubs) |
| 22 | Polearm |
| 23 | Sword/dagger |
| 30 | Boots |
| 31 | Gloves |
| 32 | Helmet |
| 33 | Crown |
| 34 | Shield |
| 35 | Cloak |
| 36 | Soft armor |
| 37 | Hard armor |
| 38 | Dragon scale armor |
| 39 | Light source |
| 70 | Scroll |
| 75 | Potion |
| 80 | Food |

---

## 2. Character Creation Algorithm

Source: `birth.c: player_birth()`, `get_stats()`, `get_extra()`, `get_ahw()`, `get_history()`, `get_money()`, `player_outfit()`

### Step 1 — Input Validation

```
Inputs:
  race_id     : string (must exist in races.json)
  class_id    : string (must exist in classes.json)
  sex         : Male | Female
  stat_order  : int[6]  (values 0–5, no duplicates; player's stat priority)
  name        : string
  rng         : seeded random source

Validation:
  race/class combination must have a row in race_class_rules.json.
  stat_order values must be a permutation of [0,1,2,3,4,5].
```

**Classification: Verified** — `birth.c:1285–1295`

---

### Step 2 — Stat Rolling (`get_stats`)

Source: `birth.c: get_stats()`

**2a. Roll 18 dice in 6 groups of 3:**

```
Roll 18 independent d6 dice.
Group them sequentially: dice[0..2], dice[3..5], ..., dice[15..17].
raw_stat[i] = sum of group i dice.
```

**2b. Enforce quality constraints:**

Repeat the roll until ALL of the following are satisfied:

```
sum(raw_stat[0..5]) > 42
sum(raw_stat[0..5]) < 54
at least one raw_stat[i] >= 17
at least two raw_stat[i] >= 16
at least three raw_stat[i] >= 15
```

**Classification: Verified** — `birth.c:65–160`

**2c. Assign by priority:**

```
Sort raw_stat descending.
stat_max[stat_order[i]] = sorted_raw_stat[i] for i in 0..5
stat_cur = stat_max (copy)
```

The player's highest rolled value goes to their most important stat.

**Classification: Verified** — `birth.c:130–156`, `218`

**2d. Apply racial and class modifiers:**

```
For each stat i in 0..5:
  bonus = races[race_id].stat_modifiers[i] + classes[class_id].stat_modifiers[i]
  stat_max[i] = modify_stat_value(stat_max[i], bonus)
  stat_cur[i] = stat_max[i]   (maximize mode — standard for new characters)
```

`modify_stat_value(value, amount)`:
- Positive `amount`: for each step, if value < 18 add 1; if value >= 18 add 10.
- Negative `amount`: for each step, if value > 18+9 subtract 10; at 18+1..18+9 clamp to 18; if value <= 18 subtract 1; minimum 3.

**Classification: Verified** — `birth.c:225–244`, CHARACTER_COMPENDIUM §3

**2e. Compute stat indices:**

```
For each stat i in 0..5:
  if stat_cur[i] <= 18:        stat_ind[i] = stat_cur[i] - 3
  elif stat_cur[i] <= 18+219:  stat_ind[i] = 15 + (stat_cur[i] - 18) / 10
  else:                        stat_ind[i] = 37
```

Range: 0–37. These index into all `stat_tables.json` lookup arrays.

**Classification: Verified** — CHARACTER_COMPENDIUM §4

---

### Step 3 — HP Array Rolling (`get_extra`)

Source: `birth.c: get_extra()`

**3a. Compute hit die:**

```
hitdie = races[race_id].hit_die + classes[class_id].hit_die
```

**Classification: Verified** — `birth.c:265`

**3b. Pre-roll HP array (50 entries):**

```
player_hp[0] = hitdie
for i in 1..49:
  player_hp[i] = player_hp[i-1] + randint1(hitdie)   (1 to hitdie inclusive)

Repeat until:
  min_value = (49 × (hitdie - 1) × 3) / 8 + 50
  max_value = (49 × (hitdie - 1) × 5) / 8 + 50
  min_value <= player_hp[49] <= max_value
```

This constrains the level-50 total to roughly the middle 25%–75% of the theoretical range.

**Classification: Verified** — `birth.c:270–296`

**3c. Set initial progression fields:**

```
level    = 1
expfact  = races[race_id].exp_factor + classes[class_id].exp_factor
exp      = 0
max_exp  = 0
```

**Classification: Verified** — `birth.c:259–262`

---

### Step 4 — Physical Characteristics (`get_ahw`)

Source: `birth.c: get_ahw()`

```
age = races[race_id].age_base + randint1(races[race_id].age_mod)

if sex == Male:
  height = randnor(races[race_id].male_height, races[race_id].male_height_mod)
  weight = randnor(races[race_id].male_weight, races[race_id].male_weight_mod)
else:
  height = randnor(races[race_id].female_height, races[race_id].female_height_mod)
  weight = randnor(races[race_id].female_weight, races[race_id].female_weight_mod)
```

`randnor(mean, stddev)` produces a Gaussian-distributed integer.

**Classification: Verified** — `birth.c:442–455`

---

### Step 5 — Social Class and Background History (`get_history`)

Source: `birth.c: get_history()`, `p_hist.txt`, `p_race.txt`

**⚠ BLOCKED — history chart data not yet in IronHell definitions. See §5 and §7.**

```
social_class = randint1(4)   (initial value, 1–4)
chart = races[race_id].history_chart   (starting chart index — NOT IN races.json yet)

While chart != 0:
  roll = randint1(100)
  find first h_info entry where h_info[i].chart == chart AND roll <= h_info[i].roll
  append h_info[i].text to background_history (with gender substitution)
  social_class += h_info[i].bonus - 50
  chart = h_info[i].next

social_class = clamp(social_class, 1, 100)
```

**Classification: Verified** — `birth.c:307–412`

---

### Step 6 — Starting Gold (`get_money`)

Source: `birth.c: get_money()`

```
gold = (social_class × 6) + randint1(100) + 300

For each stat i in 0..5:
  if stat_cur[i] >= 18+50: gold -= 300
  elif stat_cur[i] >= 18+20: gold -= 200
  elif stat_cur[i] > 18: gold -= 150
  else: gold -= (stat_cur[i] - 8) × 10

if sex == Female: gold += 50

gold = max(gold, 100)
```

**Classification: Verified** — `birth.c:465–490`

**⚠ Partially blocked** — `social_class` is required; see §5.

---

### Step 7 — Starting Equipment (`player_outfit`)

Source: `birth.c: player_outfit()`

**See §4 for complete resolution model.**

All items go into the backpack (slots 0–22). **No items are automatically equipped.**

---

### Step 8 — Initial Resource State

Source: `birth.c: player_wipe()`

```
food    = PY_FOOD_FULL - 1 = 9999
energy  = 0   (unconfirmed — see §7.U4)

All spell_flags[64]  = 0
All spell_order[64]  = 99   (sentinel value for unlearned)
```

**Classification: food = Verified** (`birth.c:630`); energy = **Unresolved**

---

### Step 9 — Derived State (first `calc_bonuses` call)

Source: `xtra1.c: calc_bonuses()`

These fields are computed immediately after birth and recomputed whenever stats or equipment change. They are not set in `player_birth` directly; they are triggered by setting `p_ptr->update` flags.

```
pspeed    = PY_NORMAL_SPEED (110) + equipment modifiers (none at creation)
see_infra = races[race_id].infravision + equipment modifiers (none at creation)
```

**Skills:**
```
skill_dis = race.disarming + class.base_skills.disarming
          + class.skill_growth.disarming × level / 10
          + adj_dex_dis[stat_ind[DEX]] + adj_int_dis[stat_ind[INT]]

skill_dev = race.magic_device + class.base_skills.magic_device
          + class.skill_growth.magic_device × level / 10
          + adj_int_dev[stat_ind[INT]]

skill_sav = race.saving_throw + class.base_skills.saving_throw
          + class.skill_growth.saving_throw × level / 10
          + adj_wis_sav[stat_ind[WIS]]

skill_stl = race.stealth + class.base_skills.stealth
          + class.skill_growth.stealth × level / 10
          + 1   (flat unconditional bonus)
          capped at 30, minimum 0

skill_srh = race.searching + class.base_skills.searching
          + class.skill_growth.searching × level / 10

skill_fos = race.search_frequency + class.base_skills.search_frequency
          + class.skill_growth.search_frequency × level / 10

skill_thn = race.melee_to_hit + class.base_skills.melee_to_hit
          + class.skill_growth.melee_to_hit × level / 10

skill_thb = race.ranged_to_hit + class.base_skills.ranged_to_hit
          + class.skill_growth.ranged_to_hit × level / 10
```

All skill growth terms use integer division. At level 1, `growth × 1 / 10 = 0` for any growth ≤ 9. Warrior `melee_to_hit` growth is 45, yielding 4 at level 1.

**Classification: Verified** — `xtra1.c: calc_bonuses(): 2810–2825`

**Max HP:**
```
mhp = player_hp[level - 1] + adj_con_mhp[stat_ind[CON]] × level / 100
mhp = max(mhp, level + 1)

Mage class with HP_BONUS capability: mhp += level
```

At level 1: `mhp = player_hp[0] + adj_con_mhp[con_ind] / 100`. Since `player_hp[0] = hitdie` and `adj_con_mhp[con_ind] / 100` is 0–1 for typical stats, `mhp` is approximately `hitdie` but never simply equal. At CON 11 (ind=8): bonus = 0/100 = 0. At CON 18 (ind=15): 150/100 = 1. At CON 3 (ind=0): -250/100 = -2 (negative, meaning mhp < hitdie).

**Classification: Verified** — `xtra1.c: calc_hitpoints():1995–2020`

**Max Mana:**
```
if class has no spell_stat (Warrior): msp = 0

else:
  spell_stat_ind = stat_ind[class.spell_stat]
  levels         = max(level - class.first_spell_level + 1, 0)
  msp            = adj_mag_mana[spell_stat_ind] × levels / 100
  if msp > 0: msp += 1

  Cumber glove penalty (CUMBER_GLOVE capability, no free-act gloves):
    msp = msp × 3 / 4

  Cumber armor penalty:
    excess = total_worn_weight - class.spell_weight
    if excess > 0: msp -= excess / 10
    msp = max(msp, 0)
```

At level 1 with `first_spell_level = 1`: `levels = 1`. For Mage at INT 18 (ind=15): `msp = 150 × 1 / 100 = 1 → +1 = 2`. No equipment at creation, so no cumber penalties apply.

**Classification: Verified** — `xtra1.c: calc_mana():1838–1887`

**Blows per round:**
```
div = max(weapon_weight, class.min_weight)    (weapon_weight = 0 if unarmed)
P   = adj_str_blow[stat_ind[STR]] × class.attack_multiplier / div
P   = min(P, 11)

D   = adj_dex_blow[stat_ind[DEX]]
D   = min(D, 11)

num_blow = blows_table[P][D]
num_blow = min(num_blow, class.max_attacks)
num_blow = max(num_blow, 1)
```

At creation without a wielded weapon: `div = class.min_weight`, `P` tends to be low, resulting in 1 blow.

**Classification: Verified** — `xtra1.c: calc_bonuses():2960–2980`

---

## 3. CharacterState Definition

### 3.1 Immutable after creation

These fields are set once during creation and never change for the lifetime of the character.

| Field | Type | Derivation |
|---|---|---|
| `character_id` | Guid | System-generated |
| `name` | string | Player input |
| `race_id` | string | Player choice |
| `class_id` | string | Player choice |
| `sex` | enum | Player choice |
| `hitdie` | int | `race.hit_die + class.hit_die` |
| `expfact` | int | `race.exp_factor + class.exp_factor` |
| `player_hp[50]` | int[] | Pre-rolled, quality-constrained — Verified |
| `stat_order[6]` | int[] | Player's stat priority — Verified |
| `age` | int | Rolled from race distributions — Verified |
| `height` | int | Rolled from race distributions — Verified |
| `weight` | int | Rolled from race distributions — Verified |

### 3.2 Definition-derived identities

Set from definitions at creation; change only if race/class can somehow change (no such mechanic in MAngband).

| Field | Type | Source |
|---|---|---|
| `base_capability_ids` | string[] | `race.capability_ids ∪ class.capability_ids` |

### 3.3 Mutable stat fields

| Field | Type | Notes |
|---|---|---|
| `stat_max[6]` | int[6] | Rolled + modifiers. Potions increase up to 18/100. |
| `stat_cur[6]` | int[6] | Monster drain reduces; restoration potions restore to stat_max. |
| `stat_ind[6]` | int[6] | Derived cache; recomputed whenever stat_cur changes. |

### 3.4 Progression

| Field | Type | Initial Value |
|---|---|---|
| `level` | int | 1 |
| `exp` | long | 0 |
| `max_exp` | long | 0 |

### 3.5 Vital points

| Field | Type | Initial Value | Derivation |
|---|---|---|---|
| `mhp` | int | `player_hp[0] + adj_con_mhp[con_ind] / 100` (min 2) | Recomputed on level-up or CON change |
| `chp` | int | `= mhp` | Depleted in combat |
| `msp` | int | Per mana formula above | Recomputed on level-up or spell_stat change |
| `csp` | int | `= msp` | Spent on casting |

### 3.6 Social origins (Blocked)

| Field | Type | Status |
|---|---|---|
| `social_class` | int (1–100) | **Unresolved** — history chart data absent |
| `background_history[4]` | string[4] | **Unresolved** — flavor only; not required for simulation |

### 3.7 Resources

| Field | Type | Initial Value |
|---|---|---|
| `gold` | int | `(social_class × 6) + 1d100 + 300` − stat deductions (min 100) |
| `food` | int | 9999 (`PY_FOOD_FULL - 1`) |

### 3.8 Derived combat attributes (recomputed by calc_bonuses)

| Field | Initial Value | Recomputed When |
|---|---|---|
| `pspeed` | 110 | Equipment changes |
| `see_infra` | `race.infravision` | Equipment changes |
| `skill_dis` | formula above | Stats or equipment change |
| `skill_dev` | formula above | Stats or equipment change |
| `skill_sav` | formula above | Stats or equipment change |
| `skill_stl` | formula above | Stats or equipment change |
| `skill_srh` | formula above | Stats or equipment change |
| `skill_fos` | formula above | Stats or equipment change |
| `skill_thn` | formula above | Stats or equipment change |
| `skill_thb` | formula above | Stats or equipment change |
| `to_h, to_d, to_a, ac` | 0 | Equipment changes |
| `num_blow` | 1 (unarmed) | Stats or weapon changes |
| `num_fire` | 0 | Ranged weapon equipped |

### 3.9 Passive capability booleans (derived cache)

Computed from `base_capability_ids` plus equipment capability grants. **Source of truth is the capability ID set, not these booleans.** Stored as cache for simulation performance.

*Sustains:* `sustain_str`, `sustain_int`, `sustain_wis`, `sustain_dex`, `sustain_con`, `sustain_chr`

*Resistances:* `resist_acid`, `resist_elec`, `resist_fire`, `resist_cold`, `resist_pois`, `resist_conf`, `resist_sound`, `resist_lite`, `resist_dark`, `resist_chaos`, `resist_disen`, `resist_shard`, `resist_nexus`, `resist_blind`, `resist_neth`, `resist_fear`

*Immunities:* `immune_acid`, `immune_elec`, `immune_fire`, `immune_cold` (none from race/class at creation)

*Passive traits:* `free_act`, `hold_life`, `see_inv`, `regenerate`, `slow_digest`, `feather_fall`, `lite`

**Classification: Derived**

### 3.10 Timed conditions (all 0 at creation)

*Debuffs:* `fast`, `slow`, `blind`, `paralyzed`, `confused`, `afraid`, `image`, `poisoned`, `cut`, `stun`

*Buffs:* `hero`, `shero`, `blessed`, `shield`, `protevil`, `invuln`

*Timed senses:* `tim_invis`, `tim_infra`

*Elemental shields:* `oppose_acid`, `oppose_elec`, `oppose_fire`, `oppose_cold`, `oppose_pois`

*Word of Recall:* `word_recall` = 0

**Classification: Verified**

### 3.11 Spell state

| Field | Type | Initial Value |
|---|---|---|
| `spell_flags[64]` | byte[] | All 0 |
| `spell_order[64]` | byte[] | All 99 (unlearned sentinel) |
| `new_spells` | int | 0 |

### 3.12 Energy

| Field | Initial Value | Classification |
|---|---|---|
| `energy` | 0 | **Unresolved** — initial value at creation not confirmed from source |

### 3.13 Inventory

| Field | Type | Initial Value |
|---|---|---|
| `inventory[23]` | item[] | From starting equipment resolution (§4) |
| `total_weight` | int | Sum of starting item weights |

---

## 4. Starting Equipment Resolution

Source: `birth.c: player_outfit()`, `init1.c:1318–1346`, `object2.c: object_prep()`, `object2.c: inven_carry()`

### 4.1 Resolution Flow

```
For each class-specific E: entry (up to MAX_START_ITEMS = 6):
  k_idx = lookup_kind(entry.tval, entry.sval)     // resolve by tval+sval
  if k_idx == 0: skip (invalid item)
  object_prep(item, k_idx)                         // initialize from kind defaults
  item.number = rand_range(entry.min, entry.max)   // stack count
  item.origin = ORIGIN_BIRTH
  mark item as known and aware
  inven_carry(player, item)                        // place in backpack

(Hardcoded universal items, always added):
  Food rations: rand_range(3, 7) × 1 ration each
  Torches:      rand_range(3, 7) torches, each with rand_range(3,7) × 500 fuel
  Word of Recall scroll: 1
```

**Classification: Verified** — `birth.c:840–907`

### 4.2 Item Initialization from Kind

`object_prep(item, kind_index)` sets:

| Item Field | Value |
|---|---|
| `k_idx` | kind index |
| `tval`, `sval` | from kind definition |
| `pval` | from kind default pval |
| `number` | 1 (then overridden by min/max) |
| `weight` | from kind definition |
| `to_h`, `to_d`, `to_a` | from kind defaults (base enchantment) |
| `ac`, `dd`, `ds` | from kind defaults |
| `origin` | `ORIGIN_BIRTH` |

No random enchantments are applied to starting items. No ego-item or artifact rolls occur. Items use exact kind defaults.

**Classification: Verified** — `object2.c:129–174`

### 4.3 Inventory Placement

`inven_carry` places items into backpack slots 0–22:

1. First, attempt to **combine** with an existing similar item (same kind, same enchantments, stackable). If combinable: increase existing item's `number`, update `total_weight`.
2. If no combination: place in the **lowest empty slot** (0 through 22).

**Starting items are never automatically wielded or equipped.** `inven_carry` only places in pack slots (0–22). Equipment slots (24–35) are never written by `player_outfit`. The player must manually wield/wear items after creation.

**Classification: Verified** — `object2.c:4528–4600`, `birth.c: player_outfit`

### 4.4 IronHell Item Resolution Model

IronHell's `classes.json` uses string IDs (`starting_equipment_ids`) instead of `tval/sval` pairs. Resolution proceeds as:

```
For each id in classes[class_id].starting_equipment_ids:
  item_kind = ItemCatalog.Find(id)              // lookup by IronHell string ID
  if item_kind == null: fail startup (missing definition)
  item = CreateItemFromKind(item_kind, quantity=1, origin=Birth)
  inventory.CarryOrStack(item)
```

**Stack count:** The current `starting_equipment_ids` is a flat list with no quantity field. The MAngband `E:` lines specify `min:max` ranges (most are `1:1`; food and torches use random counts). This is a definition gap.

**Deviation from MAngband:** The hardcoded food, torch, and WoR items from `birth.c` are included in `starting_equipment_ids` in IronHell's `classes.json`, giving IronHell control over these items per-class. MAngband adds them universally regardless of class.

**Classification of gap: Derived** — `starting_equipment_ids` is present; quantity ranges are absent.

---

## 5. History Chart Dependency Model

Source: `birth.c: get_history()`, `p_hist.txt`, `p_race.txt`

### 5.1 Data Structure

Each history entry in `p_hist.txt` has the format:

```
N:chart:next:roll:bonus
D:text (with gender templates $u/$r/~h/~a)
```

| Field | Meaning |
|---|---|
| `chart` | Which chart this entry belongs to |
| `next` | Chart to proceed to after this entry (0 = end of chain) |
| `roll` | Maximum cumulative roll (1d100) that triggers this entry |
| `bonus` | Social class modifier: `social_class += bonus - 50` |
| `text` | Background text fragment; gender-substituted via template vars |

### 5.2 Gender Substitution Templates

| Template | Male | Female |
|---|---|---|
| `$u` | "You" | "She" / "He" (context-dependent) |
| `$r` | "Your" | "His" / "Her" |
| `~a` | "are" | "is" |
| `~h` | "have" | "has" |

**Classification: Verified** — `birth.c:351–385`, `p_hist.txt:29–35`

### 5.3 Algorithm

```
social_class = randint1(4)                        // 1–4 initial value
chart = races[race_id].history_chart              // starting chart for this race

while chart != 0:
  roll = randint1(100)
  entry = first h_info entry where entry.chart == chart AND roll <= entry.roll
  background_text += entry.text                   // append with gender substitution
  social_class += entry.bonus - 50
  chart = entry.next

social_class = clamp(social_class, 1, 100)

// Split background_text into 4 lines of ≤ 60 chars, breaking on spaces
background_history[0..3] = word_wrap(background_text, width=60, lines=4)
```

**Classification: Verified** — `birth.c:307–428`

### 5.4 Race Starting Charts

Source: `p_race.txt I:` lines (first field is `history` = starting chart)

| Race | Starting Chart | Chain |
|---|---|---|
| Human | 1 | 1→2→3→50→51→52→53→0 |
| Half-Elf | 4 | 4→1→2→3→50→51→52→53→0 |
| Elf | 5 | 5→6→9→54→55→56→0 |
| High-Elf | 7 | 7→8→9→54→55→56→0 |
| Hobbit | 10 | 10→11→3→50→51→52→53→0 |
| Gnome | 13 | 13→14→3→50→51→52→53→0 |
| Dwarf | 16 | 16→17→18→57→58→59→60→61→0 |
| Half-Orc | 19 | 19→20→2→3→50→51→52→53→0 |
| Half-Troll | 21 | 21→22→62→63→64→65→66→0 |
| Kobold | 23 | 23→24→25→26→67→68→69→0 |
| Dúnadan | 1 | Same as Human |

**Classification: Verified** — `p_race.txt I:` lines, `p_hist.txt` chart map header

### 5.5 Missing Data

`races.json` does not have a `history_chart` field. The starting chart for each race must be added.

`p_hist.txt` contains the complete set of 69 chart entries with text. This file exists in the reference source and has not been extracted into IronHell definitions.

---

## 6. Derived Calculations Reference

All lookup table values are in `data/definitions/stat_tables.json`.

### 6.1 XP to next level

```
xp_threshold(level) = player_exp[level - 1] × expfact / 100
```

`player_exp[50]` is in `stat_tables.json`. `expfact = race.exp_factor + class.exp_factor`.

### 6.2 Carry weight limit

```
carry_limit = adj_str_wgt[stat_ind[STR]] × 100   (in tenths-lb)
if total_weight > carry_limit / 2:
  speed_penalty = (total_weight - carry_limit / 2) / (carry_limit / 10)
  pspeed -= speed_penalty
```

### 6.3 Weapon hold (heavy weapon check)

```
hold = adj_str_hold[stat_ind[STR]]
if hold < weapon.weight / 10:
  to_h -= 2 × (hold - weapon.weight / 10)   // negative × negative = positive penalty
  heavy_wield = true
  num_blow = 1   // forced to 1, skip blow table
```

### 6.4 Spell failure rate

Spell failure is not part of CharacterState at creation (no spells known). Documented here for completeness.

```
chance = spell.fail_rate
chance -= 3 × (stat[spell_stat_ind] - spell.level_required)
chance -= adj_mag_stat[stat_ind[spell_stat]]
if class has ZERO_FAIL: min_chance = 0 else min_chance = adj_mag_fail[stat_ind[spell_stat]]
chance = clamp(chance, min_chance, 100)
```

### 6.5 Encumbrance penalty on mana

```
if class has spell_stat:
  total_worn_weight = sum of weights of worn body, head, shield, cloak, gloves, boots
  if total_worn_weight > class.spell_weight:
    msp -= (total_worn_weight - class.spell_weight) / 10
    msp = max(msp, 0)
```

`class.spell_weight` is now in `classes.json` (added 2026-08-06).

---

## 7. Remaining Blockers

| # | Field | Status | Resolution |
|---|---|---|---|
| B1 | `social_class` | **Open** | Extract `h_info[]` from `p_hist.txt` into `data/definitions/history_charts.json`; add `history_chart` field to `races.json` |
| B2 | `background_history[4]` | **Open** — flavor only | Unblocks naturally once B1 is resolved; not required for simulation |
| B3 | `gold` (exact value) | **Open** — partially blocked | Formula is Verified; blocked by `social_class` (B1). Workaround: use fixed `social_class = 50` for testing |
| B4 | `energy` (initial) | **Open** | Confirm initial value from `dungeon.c` or `xtra2.c` at player placement |
| B5 | `starting_equipment_ids` quantity | **Open** — definition gap | `starting_equipment_ids` has no min/max count per item. Add a `starting_equipment` structured field to `classes.json` with `id`, `min`, `max` |
| B6 | `hp_per_level` duplicate | **Open** — cleanup | `hp_per_level` is still in `classes.json` and schema as redundant duplicate of `hit_die`. Remove both field and schema entry. |
| B7 | `history_chart` field in `races.json` | **Open** — data missing | Add integer field; values: human=1, half_elf=4, elf=5, hobbit=10, gnome=13, dwarf=16, half_orc=19, half_troll=21, dunadan=1, high_elf=7, kobold=23 |
| B8 | `starting_equipment_ids` cross-reference | **Open** — validation | Schema does not validate IDs against item definitions. No item named `wooden_torch` exists yet in item definitions. |

---

## 8. Document Supersession

### 8.1 Superseded Documents

| Document | Status | Reason |
|---|---|---|
| `docs/CHARACTER_CREATION_PREREQUISITES.md` | **Superseded** | All research consolidated here. Prerequisites audit (Parts 1–2) is complete. Part 3 blocker tracking is superseded by §7. |
| `docs/CHARACTER_STATE_DEFINITION.md` | **Superseded** | CharacterState structure is consolidated in §3 of this document with additional detail. |

### 8.2 Recommendation

Archive both superseded documents by prepending `> Status: Superseded by CHARACTER_CREATION_SPECIFICATION.md` to their headers. Do not delete — they serve as provenance trails for research decisions.

### 8.3 Documents That Remain Authoritative

| Document | Still Needed For |
|---|---|
| `compendia/CHARACTER_COMPENDIUM.md` | Detailed gameplay rules reference (§3–§23 cover systems beyond CharacterFactory) |
| `docs/GAMEPLAY_ARCHITECTURE_V2.md` | Overall gameplay definition architecture |
| `docs/GAMEPLAY_ARCHITECTURE_DECISIONS.md` | ADR log for deferred architecture decisions |
| `docs/FINAL_ARCHITECTURE_READINESS_REPORT.md` | Pre-implementation readiness gate |
