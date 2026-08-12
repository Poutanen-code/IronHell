# Weapons Compendium — MAngband (reference 1.5.3)

Source notes:
- Base weapon stats and allocation: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt)
- Ego-item enchantments: [reference-mangband-1_5_3/lib/edit/ego_item.txt](reference-mangband-1_5_3/lib/edit/ego_item.txt)
- Unique artifact weapons: [reference-mangband-1_5_3/lib/edit/artifact.txt](reference-mangband-1_5_3/lib/edit/artifact.txt)
- Melee attack resolution: [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c)
- Blow count and stat tables: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c), [tables.c](reference-mangband-1_5_3/src/server/tables.c)

---

## Summary / How to Read This Compendium

### Object Entry Fields
- `W: depth : rarity : weight : cost` — `weight` in tenths of pounds (120 = 12 lbs)
- `A: depth/rarity` — dungeon allocation (multiple entries = multiple level bands)
- `P: AC : damage_dice : to_hit : to_dam : to_AC` — base combat stats
- `F:` — item flags (built-in abilities)

### Damage Notation
`XdY` means roll X dice of Y sides. Average = `X * (Y+1) / 2`.

### Multipliers
Slays and brands apply as damage multipliers. Only the **best** multiplier applies — they do not stack.

---

## Part 1 — Base Weapons

### Edged Weapons (TV_SWORD, tval 23)

| Name | Damage | Weight | Depth | Cost | Notes |
|------|--------|--------|-------|------|-------|
| Broken Dagger | 1d1 | 0.5 lb | 0 | 1 | -2 to-hit, -4 to-dam |
| Dagger | 1d4 | 1.2 lb | 0 | 10 | Lightest useful blade; found at all depths |
| Main Gauche | 1d5 | 3 lb | 3 | 25 | Light, short guard blade |
| Rapier | 1d6 | 4 lb | 5 | 42 | Fast thrusting blade |
| Small Sword | 1d6 | 7.5 lb | 5 | 48 | Short cut-and-thrust |
| Short Sword | 1d7 | 8 lb | 5 | 90 | Versatile short blade |
| Sabre | 1d7 | 5 lb | 5 | 50 | Curved slashing blade |
| Cutlass | 1d7 | 11 lb | 5 | 85 | Heavier curved blade |
| Tulwar | 2d4 | 10 lb | 5 | 200 | Curved eastern sword |
| Long Sword | 2d5 | 13 lb | 10 | 300 | Standard warrior blade |
| Broad Sword | 2d5 | 15 lb | 10 | 255 | Heavy double-edged blade |
| Scimitar | 2d5 | 13 lb | 10 | 250 | Broad curved blade |
| Katana | 3d4 | 12 lb | 20 | 400 | Eastern curved sword |
| Bastard Sword | 3d4 | 14 lb | 15 | 350 | Hand-and-a-half sword |
| Two-Handed Sword | 3d6 | 20 lb | 30 | 775 | Massive two-handed blade |
| Executioner's Sword | 4d5 | 26 lb | 40 | 850 | Giant cleaving blade |
| Blade of Chaos | 6d5 | 18 lb | 70 | 4000 | RES_CHAOS built-in |
| Broken Sword | 1d2 | 3 lb | 0 | 2 | -2 to-hit, -4 to-dam |

**Notes on Edged Weapons:**
- The **Blade of Chaos** (sval 30) has RES_CHAOS built in due to its chaotic nature; very rare (depth 70, rarity 5 × appears 1/rarity_group).
- Two-handed swords require enough strength to wield without penalty (see Blow Count section).
- Priests receive a **-2 to-hit / -2 to-dam penalty** for using non-blessed edged weapons.

---

### Blunt Weapons (TV_HAFTED, tval 21)

| Name | Damage | Weight | Depth | Cost | Notes |
|------|--------|--------|-------|------|-------|
| Whip | 1d3 | 3 lb | 3 | 30 | Minimal damage |
| Mace | 2d4 | 12 lb | 5 | 130 | Basic crushing weapon |
| War Hammer | 3d3 | 12 lb | 5 | 225 | Heavy but low variance |
| Quarterstaff | 1d9 | 15 lb | 10 | 200 | High single-die variance; Mage-friendly |
| Ball-and-Chain | 2d4 | 15 lb | 20 | 200 | Weighted chain weapon |
| Morning Star | 2d6 | 15 lb | 10 | 396 | Spiked club |
| Flail | 2d6 | 15 lb | 10 | 353 | Chained weighted head |
| Lucerne Hammer | 2d5 | 12 lb | 10 | 376 | Polearm-style hammer |
| Lead-Filled Mace | 3d4 | 18 lb | 15 | 502 | Heavy crushing |
| Two-Handed Flail | 3d6 | 28 lb | 45 | 590 | Very heavy chain weapon |
| Mace of Disruption | 5d8 | 40 lb | 80 | 4300 | SLAY_UNDEAD built-in; enormous damage |

**Notes on Blunt Weapons:**
- **Priests** favour blunt weapons — no penalty for using hafted weapons. Paladins also have no penalty for hafted.
- The **Mace of Disruption** (sval 20) has SLAY_UNDEAD built in, dealing ×3 damage vs undead; found only at dungeon depth 80+.
- The **Quarterstaff** has a single 1d9 die with high average (5.0) and is the preferred Mage/Priest weapon for combat.

---

### Polearms and Axes (TV_POLEARM, tval 22)

| Name | Damage | Weight | Depth | Cost | Notes |
|------|--------|--------|-------|------|-------|
| Spear | 1d6 | 5 lb | 5 | 36 | Light polearm, can be thrown |
| Trident | 1d8 | 7 lb | 5 | 120 | Three-pronged spear |
| Awl-Pike | 1d8 | 16 lb | 10 | 340 | Long spiked pole |
| Beaked Axe | 2d6 | 18 lb | 15 | 408 | Single-headed axe |
| Glaive | 2d6 | 19 lb | 20 | 363 | Bladed polearm |
| Pike | 2d5 | 16 lb | 15 | 358 | Long infantry spear |
| Broad Axe | 2d6 | 16 lb | 15 | 304 | Wide-bladed axe |
| Halberd | 3d5 | 19 lb | 25 | 430 | Axe-spear combination |
| Lance | 2d8 | 30 lb | 10 | 230 | Cavalry weapon; heavy |
| Great Axe | 4d4 | 23 lb | 40 | 500 | Large two-handed axe |
| Battle Axe | 2d8 | 17 lb | 15 | 334 | Double-headed war axe |
| Scythe | 5d3 | 25 lb | 45 | 800 | Farming tool repurposed; high average |
| Lochaber Axe | 3d8 | 25 lb | 45 | 750 | Massive hooked axe |
| Scythe of Slicing | 8d4 | 25 lb | 60 | 3500 | Enchanted scythe; enormous dice |

**Notes on Polearms and Axes:**
- The **Scythe of Slicing** (sval 30) is the highest-dice non-artifact weapon in the game (8d4 = avg 20), found only at depth 60+.
- **Priests** take the -2/-2 penalty for polearms unless the weapon is BLESSED.
- Many Blessed artifact polearms are favoured by Paladins.

---

### Ranged Weapons (TV_BOW, tval 19)

| Name | Weight | Depth | Cost | Notes |
|------|--------|-------|------|-------|
| Sling | 0.5 lb | 1 | 5 | Fires pebbles and iron shots |
| Short Bow | 3 lb | 3 | 50 | Fires arrows |
| Long Bow | 4 lb | 10 | 120 | Fires arrows; stronger than short bow |
| Light Crossbow | 11 lb | 15 | 140 | Fires bolts |
| Heavy Crossbow | 20 lb | 30 | 300 | Fires bolts; highest raw ranged damage |

**Ammunition Compatibility:**
- Sling → Rounded Pebbles, Iron Shots
- Short Bow / Long Bow → Arrows, Seeker Arrows
- Light Crossbow / Heavy Crossbow → Bolts, Seeker Bolts

---

### Missiles and Ammunition (TV_SHOT/ARROW/BOLT)

| Name | Damage | Weight | Depth | Cost | Launcher |
|------|--------|--------|-------|------|---------|
| Rounded Pebble | 1d2 | 0.4 lb | 0 | 1 | Sling |
| Iron Shot | 1d4 | 0.5 lb | 3 | 2 | Sling |
| Arrow | 1d4 | 0.2 lb | 3 | 1 | Bow |
| Seeker Arrow | 4d4 | 0.2 lb | 55 | 20 | Bow |
| Bolt | 1d5 | 0.3 lb | 3 | 2 | Crossbow |
| Seeker Bolt | 4d5 | 0.3 lb | 65 | 25 | Crossbow |

**Seeker ammunition** is heavy-duty ammunition with greatly increased dice, found only in deeper dungeon levels (depth 55–65+).

---

### Digging Tools (TV_DIGGING, tval 20)

Digging tools double as weapons but are primarily used to tunnel through rock walls. They all have the TUNNEL flag.

| Name | Damage | Weight | Depth | Tunnel | Cost |
|------|--------|--------|-------|--------|------|
| Shovel | 1d2 | 6 lb | 1 | +1 | 10 |
| Gnomish Shovel | 1d2 | 6 lb | 20 | +2 | 100 |
| Dwarven Shovel | 1d3 | 12 lb | 40 | +3 | 200 |
| Pick | 1d3 | 15 lb | 5 | +1 | 50 |
| Orcish Pick | 1d3 | 18 lb | 30 | +2 | 300 |
| Dwarven Pick | 1d4 | 20 lb | 50 | +3 | 600 |
| Mattock | 1d8 | 25 lb | 50 | +3 | 700 |

The **Mattock** is unusual in having 1d8 damage, making it a situationally effective weapon while also being a capable tunneling tool.

---

## Part 2 — How Hitting and Damage Works

### To-Hit Resolution (Melee)

**Source:** `test_hit_norm()` in [cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L51)

1. Roll d100. If result < 5: **always miss**. If result < 10: **always hit** (5% each).
2. Otherwise, check `randint0(chance) >= (AC × 3 / 4)`:
   - `chance = skill_thn + (total_to_hit × BTH_PLUS_ADJ)`
   - `total_to_hit = player.to_h + weapon.to_h`
   - `skill_thn` scales with class and level
   - Invisible targets: `chance = (chance + 1) / 2` (halved)
3. If the roll succeeds: **hit**; otherwise: **miss**.

### To-Hit Resolution (Ranged)

**Source:** `test_hit_fire()` in cmd1.c

Identical formula to melee, except invisible targets also halve the chance.

### Number of Blows per Round

**Source:** `xtra1.c` + `blows_table` in [tables.c](reference-mangband-1_5_3/src/server/tables.c#L2032)

The engine converts STR and DEX stat indices into a table lookup:

1. `str_index = adj_str_blow[STR_ind] × att_multiply / div`
   - `div = max(class_min_weight, weapon_weight_in_tenths)`
2. `dex_index = adj_dex_blow[DEX_ind]` (0–11)
3. `num_blow = blows_table[str_index][dex_index]` (result: 1–6)
4. Capped at class max blows, then `+bonus_blows` (from BLOWS flag)

**Class Multipliers and Limits:**

| Class | att_multiply | min_weight | max_blows |
|-------|-------------|-----------|-----------|
| Warrior | 5 | 30 | 6 |
| Mage | 2 | 40 | 4 |
| Priest | 3 | 35 | 5 |
| Rogue | 3 | 30 | 5 |
| Ranger | 4 | 35 | 5 |
| Paladin | 4 | 30 | 5 |

**Blows Table** (`blows_table[P][D]`, P = str index 0–11, D = dex index 0–11):

```
P\D:  0  1  2  3  4  5  6  7  8  9 10 11
 0 :  1  1  1  1  1  1  2  2  2  2  2  3
 1 :  1  1  1  1  2  2  3  3  3  4  4  4
 2 :  1  1  2  2  3  3  4  4  4  5  5  5
 3 :  1  2  2  3  3  4  4  4  5  5  5  5
 4 :  1  2  2  3  3  4  4  5  5  5  5  5
 5 :  2  2  3  3  4  4  5  5  5  5  5  6
 6 :  2  2  3  3  4  4  5  5  5  5  5  6
 7 :  2  3  3  4  4  4  5  5  5  5  5  6
 8 :  3  3  3  4  4  4  5  5  5  5  6  6
 9 :  3  3  4  4  4  4  5  5  5  5  6  6
10 :  3  3  4  4  4  4  5  5  5  6  6  6
11+:  3  3  4  4  4  4  5  5  6  6  6  6
```

A **Warrior** with maximum STR and DEX can achieve 6 base blows. The BLOWS ego/artifact flag adds bonus blows on top of this cap.

**Heavy Weapon Penalty:** If player's strength-hold value < `weapon.weight / 10`, then `-2 × (hold - weight/10)` is applied to both to-hit (displayed and actual), and the weapon is marked "hard to wield."

### Critical Hits (Melee)

**Source:** `critical_norm()` in cmd1.c

Critical hit chance: roll `randint1(5000) <= i` where `i = weapon_weight + (total_to_hit × 5) + (level × 3)`

On a critical, roll `k = weapon_weight + randint1(650)`:

| k range | Message | Damage Formula |
|---------|---------|----------------|
| < 400 | "It was a good hit!" | `2 × dam + 5` |
| 400–699 | "It was a great hit!" | `2 × dam + 10` |
| 700–899 | "It was a superb hit!" | `3 × dam + 15` |
| 900–1299 | "It was a *GREAT* hit!" | `3 × dam + 20` |
| ≥ 1300 | "It was a *SUPERB* hit!" | `(7 × dam) / 2 + 25` |

Heavier weapons give more consistent criticals (lower k-threshold for better tiers). High to-hit bonuses and high character level dramatically increase critical frequency.

### Critical Hits (Ranged)

**Source:** `critical_shot()` in cmd1.c

`i = weight + (total_to_hit × 4) + (level × 2)` — slightly harder than melee.

| k range | Message | Damage Formula |
|---------|---------|----------------|
| < 500 | "It was a good hit!" | `2 × dam + 5` |
| 500–999 | "It was a great hit!" | `2 × dam + 10` |
| ≥ 1000 | "It was a superb hit!" | `3 × dam + 15` |

### Damage Calculation Summary

For one blow:

```
base_damage = damroll(weapon.dd, weapon.ds)
base_damage = apply_slays_and_brands(base_damage, monster)  [× multiplier]
base_damage = critical_norm(weight, to_h, base_damage)      [if crit]
base_damage += weapon.to_d
base_damage += player.to_d
total_damage = max(0, base_damage)
```

### Slay and Brand Damage Multipliers

**Source:** `tot_dam_aux()` in cmd1.c. Only the **highest** multiplier applies.

| Flag | vs. Target Type | Multiplier |
|------|----------------|-----------|
| SLAY_ANIMAL | Animals (RF3_ANIMAL) | ×2 |
| SLAY_EVIL | Evil creatures (RF3_EVIL) | ×2 |
| SLAY_UNDEAD | Undead (RF3_UNDEAD) | ×3 |
| SLAY_DEMON | Demons (RF3_DEMON) | ×3 |
| SLAY_ORC | Orcs (RF3_ORC) | ×3 |
| SLAY_TROLL | Trolls (RF3_TROLL) | ×3 |
| SLAY_GIANT | Giants (RF3_GIANT) | ×3 |
| SLAY_DRAGON | Dragons (RF3_DRAGON) | ×3 |
| KILL_DRAGON | Dragons | ×5 |
| KILL_DEMON | Demons | ×5 |
| KILL_UNDEAD | Undead | ×5 |
| BRAND_ACID | Non-immune to acid | ×3 |
| BRAND_ELEC | Non-immune to electricity | ×3 |
| BRAND_FIRE | Non-immune to fire | ×3 |
| BRAND_COLD | Non-immune to cold | ×3 |
| BRAND_POIS | Non-immune to poison | ×3 |

KILL_* variants ("*Slay*" in game text) deal 5× damage and appear only on powerful weapons. Brand multipliers do **not** apply if the monster is immune to that element.

### Special Combat Effects

- **IMPACT:** If a single blow deals >50 damage, triggers `earthquake(radius 10)` centred on player.
- **Priest icky weapon:** Using a non-BLESSED edged weapon (TV_SWORD) or polearm (TV_POLEARM) as a Priest applies -2/-2 to both to-hit/to-dam and marks the weapon "icky."
- **Backstab (Rogue):** Rogues with CF_BACK_STAB attacking sleeping monsters multiply damage by `(3 + level/40)`; against fleeing monsters: `×1.5`.
- **Bare-hands:** Deals 1 point of damage (ghosts deal `level` damage; fruit bats deal `level/5 + 1`).

---

## Part 3 — Magical (Ego) Weapons

Ego weapons are randomly enchanted ordinary weapons found in the dungeon. They can apply to any sword (tval 21), polearm (tval 22), or hafted weapon (tval 23) unless otherwise noted. Cursed egos have `LIGHT_CURSE` or `HEAVY_CURSE`.

### Combat-Enhancing Egos

**`(Holy Avenger)`** (N:64) — Rating 30, depth 0, rarity 12
- `+WIS`, `SLAY_EVIL ×2`, `SLAY_UNDEAD ×3`, `SLAY_DEMON ×3`, `SEE_INVIS`, `BLESSED`, `RES_FEAR`
- Bonuses: +6 to-hit, +6 to-dam, +4 AC, +4 pval
- Grants an extra random sustain (xtra=1)
- Priests can use Holy Avengers as edged weapons without the icky penalty (BLESSED overrides)

**`(Defender)`** (N:65) — Rating 25, depth 0, rarity 12
- `STEALTH`, `FREE_ACT`, `SEE_INVIS`, `FEATHER`, `REGEN`, `RES_ACID/ELEC/FIRE/COLD`, `IGNORE_ACID/ELEC/FIRE/COLD`
- Bonuses: +4/+4, +8 AC, +4 pval
- Defensively oriented; provides full four-element resistance

**`(Blessed)`** (N:66) — Rating 20, depth 0, rarity 10
- `+WIS`, `BLESSED`
- Bonuses: +3 pval
- Allows Priests to use edged/polearm weapons without penalty

**`of Gondolin`** (N:67) — Rating 30, depth 30, rarity 20
- `SLAY_DEMON ×3`, `SLAY_ORC ×3`, `SLAY_TROLL ×3`, `SLAY_DRAGON ×3`, `LITE`, `RES_DARK`, `SEE_INVIS`, `FREE_ACT`, `IGNORE_ACID/FIRE`
- Bonuses: +7/+7; grants extra random ability (xtra=3)

**`of Westernesse`** (N:68) — Rating 20, depth 0, rarity 10
- `+STR`, `+DEX`, `+CON`, `SLAY_ORC ×3`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `FREE_ACT`, `SEE_INVIS`
- Bonuses: +5/+5, +2 pval

**`of Extra Attacks`** (N:69) — Rating 20, depth 0, rarity 10
- `BLOWS` (+pval extra blows per round)
- Bonuses: +2 pval (up to +2 extra blows; stacks with class maximum)
- One of the most valuable ego types for Warriors

**`of Fury`** (N:70) — Rating 30, depth 40, rarity 20
- `+STR`, `BLOWS`, `AGGRAVATE`, `RES_FEAR`
- Bonuses: +10/+10, +2 pval
- Restricted to heavier weapon svals (2H swords, most polearms/hafted)
- AGGRAVATE makes all nearby monsters aware of player; powerful but noisy

---

### Elemental Brand Egos

All apply to any melee weapon (sval 0–99 of tval 21/22/23).

| Ego | N | Flags | Rating | Rarity |
|-----|---|-------|--------|--------|
| of Acid | 72 | `BRAND_ACID`, `RES_ACID`, `IGNORE_ACID` | 20 | 10 |
| of Lightning | 73 | `BRAND_ELEC`, `RES_ELEC`, `IGNORE_ELEC` | 20 | 10 |
| of Flame | 74 | `BRAND_FIRE`, `RES_FIRE`, `IGNORE_FIRE` | 15 | 8 |
| of Frost | 75 | `BRAND_COLD`, `RES_COLD`, `IGNORE_COLD` | 15 | 8 |
| of Venom | 76 | `BRAND_POIS` | 15 | 8 |

Elemental brands deal ×3 damage against non-immune monsters and also protect the weapon from being destroyed by that element. The wielder also gains the matching resistance.

---

### Slay Egos (single slay)

These apply to any melee weapon.

| Ego | N | Flag | Multiplier | Rating | Rarity |
|-----|---|------|-----------|--------|--------|
| of Slay Animal | 80 | `SLAY_ANIMAL` | ×2 | 18 | 6 |
| of Slay Evil | 81 | `SLAY_EVIL` | ×2 | 18 | 6 |
| of Slay Undead | 82 | `SLAY_UNDEAD` | ×3 | 18 | 6 |
| of Slay Demon | 83 | `SLAY_DEMON` | ×3 | 14 | 6 |
| of Slay Orc | 84 | `SLAY_ORC` | ×3 | 10 | 6 |
| of Slay Troll | 85 | `SLAY_TROLL` | ×3 | 10 | 6 |
| of Slay Giant | 86 | `SLAY_GIANT` | ×3 | 14 | 6 |
| of Slay Dragon | 87 | `SLAY_DRAGON` | ×3 | 18 | 6 |

---

### *Slay* Egos (enhanced slays, with stat bonuses)

These are rarer and more powerful than plain slay egos. They add pval stat bonuses and secondary abilities.

| Ego | N | Flags | Extras | Rating | Rarity |
|-----|---|-------|--------|--------|--------|
| of *Slay Animal* | 88 | `SLAY_ANIMAL ×2`, `INT` | `SLOW_DIGEST`, +2 pval | 20 | 20 |
| of *Slay Evil* | 89 | `SLAY_EVIL ×2`, `WIS` | `BLESSED`, +2 pval | 20 | 20 |
| of *Slay Undead* | 90 | `KILL_UNDEAD ×5`, `WIS` | `SEE_INVIS`, +2 pval | 24 | 20 |
| of *Slay Demon* | 91 | `KILL_DEMON ×5`, `INT` | `RES_FIRE`, +2 pval | 16 | 20 |
| of *Slay Orc* | 92 | `SLAY_ORC ×3`, `DEX` | `SUST_DEX`, +2 pval | 14 | 20 |
| of *Slay Troll* | 93 | `SLAY_TROLL ×3`, `STR` | `REGEN`, +2 pval | 14 | 20 |
| of *Slay Giant* | 94 | `SLAY_GIANT ×3`, `STR` | `SUST_STR`, +2 pval | 16 | 20 |
| of *Slay Dragon* | 95 | `KILL_DRAGON ×5`, `CON` | `RES_FEAR`, +2 pval | 24 | 20 |

Note: `*Slay Undead*`, `*Slay Demon*`, and `*Slay Dragon*` use the KILL_* flag (×5 multiplier) — the most powerful slay variants in the game.

---

### Cursed Weapon Egos

**`of Morgul`** (N:102) — depth 0, rarity 5
- `SEE_INVIS`, `AGGRAVATE`, `HOLD_LIFE`, `DRAIN_EXP`, `SLAY_UNDEAD`, `BRAND_POIS`
- `HEAVY_CURSE | LIGHT_CURSE`
- These are deceptively attractive (SLAY_UNDEAD and BRAND_POIS are genuinely useful) but DRAIN_EXP and AGGRAVATE make them traps

---

### Ranged Weapon Egos

| Ego | N | Launcher | Flags | Bonuses |
|-----|---|----------|-------|---------|
| of Accuracy | 104 | Any | — | Up to +15 to-hit |
| of Power | 105 | Any | — | Up to +15 to-dam |
| of Extra Might | 108 | Any | `MIGHT` | +1 pval (multiplier), +5/+10 |
| of Extra Shots | 109 | Any | `SHOTS` | +1 pval (shots/round), +10/+5 |
| of Lothlorien | 106 | Short/Long Bow | `DEX, MIGHT, FREE_ACT, IGNORE_ACID/FIRE` | +2 pval, depth 50 |
| of the Haradrim | 107 | Heavy Crossbow | `MIGHT, SHOTS, IGNORE_ACID/FIRE` | +1 pval, depth 50 |
| of Buckland | 110 | Sling | `DEX, SHOTS, MIGHT, IGNORE_ACID/FIRE` | +2 pval, depth 40 |
| of the Nazgul | 111 | Any | `LIGHT_CURSE, DRAIN_EXP, SEE_INVIS` | Cursed |

**MIGHT** increases the ranged damage multiplier by pval. **SHOTS** increases the number of shots per round by pval.

---

### Ammunition Egos

| Ego | N | Flag | Applies To |
|-----|---|------|-----------|
| of Slay Animal | 112 | `SLAY_ANIMAL ×2` | All ammo |
| of Slay Evil | 113 | `SLAY_EVIL ×2` | Non-seeker ammo |
| of Slay Undead | 114 | `SLAY_UNDEAD ×3` | All ammo |
| of Slay Demon | 115 | `SLAY_DEMON ×3` | All ammo |
| of Slay Giant | 118 | `SLAY_GIANT ×3` | All ammo |
| of Slay Dragon | 119 | `SLAY_DRAGON ×3` | All ammo |
| of Holy Might | 120 | `SLAY_EVIL ×2, SLAY_DEMON ×3, SLAY_UNDEAD ×3, IGNORE_FIRE/ACID` | Seeker+ ammo (sval 2+), depth 40 |
| of Acid | 116 | `BRAND_ACID ×3, IGNORE_ACID` | All ammo |
| of Lightning | 117 | `BRAND_ELEC ×3, IGNORE_ELEC` | All ammo |
| of Flame | 122 | `BRAND_FIRE ×3, IGNORE_FIRE` | All ammo |
| of Frost | 123 | `BRAND_COLD ×3, IGNORE_COLD` | All ammo |
| of Venom | 121 | `BRAND_POIS ×3` | Non-seeker ammo |
| of Wounding | 124 | — | All ammo; +5/+5 to-hit/dam bonus |
| of Backbiting | 125 | `LIGHT_CURSE` | All ammo; cursed |

**of Holy Might** is the premier combat ammunition, combining evil/demon/undead slays on seeker-tier ammo. Found only at depth 40+, rarity 15.

---

## Part 4 — Unique Weapon Artifacts

Unique artifacts appear at most once per game. Each entry shows: base item type (damage dice), to-hit/to-dam bonuses, depth, rarity, and notable flags.

### Swords (TV_SWORD)

#### Daggers and Short Blades

**Main Gauche of Maedhros** (N:64) — `I:23:5` 2d5, `P:+12/+15`, depth 15, rarity 30
- `+INT, +DEX, +SPEED (+3 pval)`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `FREE_ACT`, `SEE_INVIS`

**Dagger 'Angrist'** (N:65) — `I:23:4` 2d4, `P:+10/+15/+5AC`, depth 20, rarity 80
- `+DEX, +SPEED (+4 pval)`, `BRAND_ACID`, `SLAY_EVIL ×2`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `FREE_ACT`, `RES_ACID`, `RES_DARK`, `SUST_DEX`
- The famous Silmarillion dagger; carved from the meteoric iron of Angrist

**Dagger 'Narthanc'** (N:66) — `I:23:4` 2d4, `P:+4/+6`, depth 4, rarity 3
- `BRAND_FIRE`, `RES_FIRE`, Activate: FIRE1 (8 turns)

**Dagger 'Nimthanc'** (N:67) — `I:23:4` 2d4, `P:+4/+6`, depth 4, rarity 3
- `BRAND_COLD`, `RES_COLD`, Activate: FROST1 (7 turns)

**Dagger 'Dethanc'** (N:68) — `I:23:4` 2d4, `P:+4/+6`, depth 4, rarity 3
- `BRAND_ELEC`, `RES_ELEC`, Activate: LIGHTNING BOLT (6 turns)

**Dagger of Rilia** (N:69) — `I:23:4` 2d4, `P:+4/+3`, depth 5, rarity 40
- `SLAY_ORC ×3`, `BRAND_POIS`, `RES_POIS`, `RES_DISEN`, Activate: STINKING_CLOUD (4 turns)

**Dagger 'Belangil'** (N:70) — `I:23:4` 2d4, `P:+6/+9`, depth 10, rarity 40
- `+DEX (+2 pval)`, `BRAND_COLD`, `RES_COLD`, `SEE_INVIS`, `SLOW_DIGEST`, `REGEN`, Activate: FROST2 (5 turns)

**Rapier 'Forasgil'** (N:86) — `I:23:7` 1d6, `P:+12/+19`, depth 15, rarity 8
- `SLAY_ANIMAL ×2`, `BRAND_COLD`, `RES_COLD`, `RES_LITE`, `LITE`

**Sabre 'Careth Asdriag'** (N:87) — `I:23:11` 1d7, `P:+6/+8`, depth 15, rarity 8
- `BLOWS (+1 pval)`, `SLAY_DRAGON ×3`, `SLAY_ANIMAL ×2`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `SLAY_ORC ×3`

**Small Sword 'Sting'** (N:88) — `I:23:8` 1d6, `P:+7/+8`, depth 20, rarity 15
- `+STR, +DEX, +CON, BLOWS, +SPEED (+2 pval)`, `RES_FEAR`, `SLAY_EVIL ×2`, `SLAY_UNDEAD ×3`, `SLAY_ORC ×3`, `SLAY_ANIMAL ×2`, `FREE_ACT`, `RES_LITE`, `LITE`, `SEE_INVIS`
- Bilbo Baggins' blade; exceptional for its size

**Short Sword 'Dagmor'** (N:90) — `I:23:10` 1d7, `P:+3/+7`, depth 20, rarity 8
- `BLOWS (+2 pval)`, `BRAND_POIS`, `SLAY_ANIMAL ×2`, `SLOW_DIGEST`, `REGEN`

**Cutlass 'Gondricam'** (N:79) — `I:23:12` 1d7, `P:+10/+11`, depth 20, rarity 8
- `+DEX, STEALTH (+3 pval)`, `RES_ACID/ELEC/FIRE/COLD`, `FEATHER`, `SEE_INVIS`, `REGEN`

#### Broad and Long Swords

**Scimitar 'Haradekket'** (N:89) — `I:23:18` 2d5, `P:+9/+11`, depth 20, rarity 15
- `+DEX, BLOWS (+2 pval)`, `SLAY_ANIMAL ×2`, `SLAY_EVIL ×2`, `SLAY_UNDEAD ×3`, `SEE_INVIS`

**Broad Sword 'Glamdring'** (N:73) — `I:23:16` 2d5, `P:+10/+15`, depth 20, rarity 20
- `SEARCH, BLESSED, LITE`, `SLAY_EVIL ×2`, `BRAND_FIRE`, `SLAY_ORC ×3`, `SLAY_DEMON ×3`, `RES_FIRE`, `RES_LITE`, `SLOW_DIGEST`
- Gandalf's "Beater"; a classic Tolkien sword

**Broad Sword 'Aeglin'** (N:74) — `I:23:16` 2d5, `P:+12/+16`, depth 20, rarity 30
- `SEARCH, BLESSED, LITE`, `SLAY_ORC ×3`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `BRAND_ELEC`, `RES_ELEC`, `RES_BLIND`, `SLOW_DIGEST`

**Broad Sword 'Orcrist'** (N:75) — `I:23:16` 2d5, `P:+10/+15`, depth 20, rarity 20
- `SEARCH, BLESSED, LITE`, `SLAY_EVIL ×2`, `BRAND_COLD`, `SLAY_ORC ×3`, `SLAY_DRAGON ×3`, `RES_COLD`, `RES_DARK`, `SLOW_DIGEST`
- The Elven "Biter"

**Broad Sword 'Arunruth'** (N:72) — `I:23:16` 3d5, `P:+20/+12`, depth 20, rarity 45
- `+DEX (+4 pval)`, `SLAY_DEMON ×3`, `SLAY_ORC ×3`, `FREE_ACT`, `RES_COLD`, `FEATHER`, `SLOW_DIGEST`, Activate: FROST4 (50 turns)
- Thingol's "King's Ire"; remarkable 3d5 damage on a Broad Sword base

**Long Sword 'Elvagil'** (N:85) — `I:23:17` 2d5, `P:+2/+7`, depth 20, rarity 8
- `+DEX, +CHR, STEALTH (+2 pval)`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `FEATHER`, `SEE_INVIS`

**Long Sword 'Anguirel'** (N:84) — `I:23:17` 2d5, `P:+8/+12`, depth 20, rarity 30
- `+STR, +CON, +SPEED, AGGRAVATE (+2 pval)`, `SLAY_EVIL ×2`, `BRAND_POIS`, `SLAY_DEMON ×3`, `FREE_ACT`, `RES_ELEC`, `RES_LITE`, `RES_DARK`, `LITE`, `SEE_INVIS`
- Eol's second blade; AGGRAVATE is a drawback

**Long Sword 'Anduril'** (N:83) — `I:23:17` 3d5, `P:+10/+15/+10AC`, depth 20, rarity 40
- `+STR, +DEX, RES_FEAR, FREE_ACT, BLESSED (+4 pval)`, `SLAY_EVIL ×2`, `BRAND_FIRE`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `SLAY_UNDEAD ×3`, `RES_FIRE`, `RES_DISEN`, `SUST_STR/DEX`, `SEE_INVIS`, Activate: FIRE2 (40 turns)
- Aragorn's "Flame of the West"; 3d5 on a Long Sword base

**Long Sword 'Ringil'** (N:82) — `I:23:17` 4d5, `P:+22/+25`, depth 20, rarity 120
- `+SPEED, RES_FEAR, BLESSED (+10 pval)`, `SLAY_EVIL ×2`, `BRAND_COLD`, `SLAY_UNDEAD ×3`, `KILL_DEMON ×5`, `SLAY_TROLL ×3`, `FREE_ACT`, `RES_COLD`, `RES_LITE`, `LITE`, `SEE_INVIS`, `SLOW_DIGEST`, `REGEN`, Activate: FROST3 (40 turns)
- Fingolfin's legendary blade; 4d5 Long Sword is among the very best swords in the game; extremely rare

#### Katanas and Executioner Blades

**Katana 'Aglarang'** (N:81) — `I:23:20` 8d4, `P:+0/+0`, depth 30, rarity 25
- `+DEX, +SPEED, SUST_DEX (+5 pval)`
- Remarkably high base damage (8d4 = avg 20) with no combat bonuses; speed alone makes it appealing

**Executioner's Sword 'Crisdurian'** (N:80) — `I:23:28` 4d5, `P:+18/+19`, depth 40, rarity 25
- `SLAY_DRAGON ×3`, `SLAY_EVIL ×2`, `SLAY_UNDEAD ×3`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `SLAY_ORC ×3`, `SEE_INVIS`
- All-purpose slayer; outstanding versus most major monster categories

#### Two-Handed Swords

**Two-Handed Sword 'Gurthang'** (N:76) — `I:23:25` 3d6, `P:+13/+17`, depth 30, rarity 30
- `+STR (+2 pval)`, `RES_FIRE`, `RES_POIS`, `BRAND_FIRE`, `BRAND_POIS`, `KILL_DRAGON ×5`, `FREE_ACT`, `SLOW_DIGEST`, `REGEN`
- Turin's legendary dragonbane; dual brands with KILL_DRAGON

**Two-Handed Sword 'Zarcuthra'** (N:77) — `I:23:25` 4d6, `P:+19/+21`, depth 30, rarity 180
- `+STR, +CHR, +INFRA, AGGRAVATE (+4 pval)`, KILL_DRAGON ×5`, `SLAY_ANIMAL ×2`, `SLAY_EVIL ×2`, `BRAND_FIRE`, `SLAY_UNDEAD ×3`, `SLAY_DEMON ×3`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `SLAY_ORC ×3`, `RES_FIRE`, `RES_CHAOS`, `FREE_ACT`, `SEE_INVIS`, `AGGRAVATE`
- Possibly the most damaging sword in the game; extraordinarily rare (rarity 180)

**Two-Handed Sword 'Mormegil'** (N:78) — `I:23:25` 3d6, `P:-15/-15/-10AC`, depth 30, rarity 15
- `BRAND_POIS`, `KILL_DRAGON ×5`, `SLAY_UNDEAD ×3`, `+SPEED`, `SEE_INVIS`, `HOLD_LIFE`, `AGGRAVATE`, `DRAIN_EXP`
- `HEAVY_CURSE | LIGHT_CURSE` — The Black Sword; seductive but deeply cursed

**Bastard Sword 'Calris'** (N:71) — `I:23:21` 5d4, `P:-20/+20`, depth 30, rarity 15
- `+CON (+5 pval)`, `KILL_DRAGON ×5`, `SLAY_EVIL ×2`, `SLAY_DEMON ×3`, `SLAY_TROLL ×3`, `RES_DISEN`, `AGGRAVATE`
- `HEAVY_CURSE | LIGHT_CURSE` — Cursed; the huge -20 to-hit makes it a trap despite 5d4 damage

**Blade of Chaos 'Doomcaller'** (N:91) — `I:23:30` 6d5, `P:+18/+28/-50AC`, depth 70, rarity 25
- `KILL_DRAGON ×5`, `SLAY_ANIMAL ×2`, `SLAY_EVIL ×2`, `BRAND_COLD`, `SLAY_TROLL ×3`, `SLAY_DEMON ×3`, `FREE_ACT`, `RES_ACID/ELEC/FIRE/COLD/CHAOS`, `SEE_INVIS`, `TELEPATHY`, `AGGRAVATE`
- Extreme damage with -50 AC as drawback; the wielder "falls under the shadow of death"

---

### Polearms (TV_POLEARM)

**Spear 'Nimloth'** (N:99) — `I:22:2` 1d6, `P:+11/+13`, depth 15, rarity 12
- `STEALTH, +SPEED, BLESSED (+3 pval)`, `BRAND_COLD`, `SLAY_UNDEAD ×3`, `RES_COLD`, `SEE_INVIS`

**Spear 'Aeglos'** (N:97) — `I:22:2` 3d6, `P:+15/+25/+5AC`, depth 15, rarity 45
- `+WIS, +DEX, RES_FEAR, BLESSED (+4 pval)`, `BRAND_COLD`, `RES_COLD`, `SLAY_EVIL ×2`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `KILL_UNDEAD ×5`, `FREE_ACT`, `SLOW_DIGEST`, Activate: FROST3 (35 turns)
- Gil-galad's "Snow-point"; remarkably powerful spear

**Spear of Orome** (N:98) — `I:22:2` 4d6, `P:+15/+15`, depth 15, rarity 45
- `+INT, +SPEED, +INFRA, BLESSED (+4 pval)`, `BRAND_FIRE`, `SLAY_GIANT ×3`, `SLAY_ANIMAL ×2`, `RES_FIRE`, `RES_LITE`, `FEATHER`, `LITE`, `SEE_INVIS`, Activate: STONE_TO_MUD (5 turns)
- The Vala's hunting spear; 4d6 on a Spear base is extraordinary

**Lance of Eorlingas** (N:100) — `I:22:20` 3d8, `P:+13/+21`, depth 20, rarity 23
- `+STR, +DEX, +SPEED, RES_FEAR (+2 pval)`, `SLAY_EVIL ×2`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `SEE_INVIS`

**Trident of Wrath** (N:107) — `I:22:5` 3d8, `P:+16/+18`, depth 15, rarity 35
- `+STR, +DEX, BLESSED (+2 pval)`, `BRAND_POIS`, `SLAY_EVIL ×2`, `KILL_UNDEAD ×5`, `RES_LITE`, `RES_DARK`, `SEE_INVIS`

**Trident of Ulmo** (N:108) — `I:22:5` 4d8, `P:+15/+19`, depth 30, rarity 90
- `+DEX, BLESSED (+4 pval)`, `SLAY_DRAGON ×3`, `SLAY_ANIMAL ×2`, `FREE_ACT`, `HOLD_LIFE`, `IM_ACID`, `RES_ACID`, `RES_NETHR`, `SEE_INVIS`, `SLOW_DIGEST`, `REGEN`, Activate: TELE_AWAY (50 turns)
- Ulmo's weapon; 4d8 on a Trident base; IM_ACID is rare and valuable

**Beaked Axe of Theoden** (N:93) — `I:22:10` 2d6, `P:+8/+10`, depth 20, rarity 15
- `+WIS, +CON, SLAY_DRAGON ×3, TELEPATHY, SLOW_DIGEST (+3 pval)`, Activate: DRAIN_LIFE2 (40 turns)

**Beaked Axe of Hurin** (N:110) — `I:22:10` 3d6, `P:+12/+15`, depth 20, rarity 15
- `KILL_DEMON ×5`, `+STR, +CON (+2 pval)`, `BRAND_ACID`, `RES_ACID`, `RES_DARK`, `RES_FIRE`, `LITE`, `SLAY_DRAGON ×3`, `SLAY_TROLL ×3`, Activate: BERSERKER (80 turns)
- Hurin's final stand weapon; Activate grants Berserk strength

**Glaive of Pain** (N:94) — `I:22:13` 9d6, `P:+0/+30`, depth 30, rarity 25
- `RES_FEAR` only; but 9d6 (avg 31.5) is the highest polearm damage dice

**Halberd 'Osondir'** (N:95) — `I:22:15` 3d5, `P:+6/+9`, depth 20, rarity 8
- `+CHR (+3 pval)`, `BRAND_FIRE`, `SLAY_UNDEAD ×3`, `SLAY_GIANT ×3`, `RES_FIRE`, `RES_SOUND`, `FEATHER`, `SEE_INVIS`

**Pike 'Til-i-arc'** (N:96) — `I:22:8` 2d5, `P:+10/+12/+10AC`, depth 20, rarity 15
- `+INT, SUST_INT, SLOW_DIGEST (+2 pval)`, `BRAND_COLD`, `BRAND_FIRE`, `SLAY_DEMON ×3`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `RES_FIRE`, `RES_COLD`

**Broad Axe 'Barukkheled'** (N:106) — `I:22:11` 2d6, `P:+13/+19`, depth 20, rarity 8
- `+CON (+3 pval)`, `SLAY_EVIL ×2`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, `SLAY_ORC ×3`, `SEE_INVIS`

**Battle Axe 'Lotharang'** (N:104) — `I:22:22` 2d8, `P:+4/+3`, depth 30, rarity 15
- `+STR, +DEX (+1 pval)`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, Activate: CURE_WOUNDS (3 turns)

**Battle Axe of Balli Stonehand** (N:103) — `I:22:22` 3d8, `P:+8/+11/+5AC`, depth 30, rarity 15
- `+STR, +CON, STEALTH (+3 pval)`, `SLAY_DEMON ×3`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `FREE_ACT`, `RES_ACID/ELEC/FIRE/COLD`, `RES_BLIND`, `FEATHER`, `SEE_INVIS`, `REGEN`

**Great Axe of Durin** (N:101) — `I:22:25` 4d4, `P:+10/+20/+15AC`, depth 30, rarity 90
- `+STR, +CON, TUNNEL, BRAND_ACID/FIRE, KILL_DRAGON ×5, SLAY_DEMON ×3, SLAY_TROLL ×3, SLAY_ORC ×3 (+3 pval)`, `FREE_ACT`, `RES_CONFU`, `RES_FEAR`, `RES_ACID/FIRE/LITE/DARK/CHAOS`
- The Dwarven king's axe; exceptional resistances and combat slays

**Great Axe of Eonwe** (N:102) — `I:22:25` 5d4, `P:+15/+18/+8AC`, depth 30, rarity 120
- All 6 stats +1, `RES_FEAR, BLESSED (+2 pval)`, `SLAY_EVIL ×2`, `BRAND_COLD`, `SLAY_UNDEAD ×3`, `KILL_DEMON ×5`, `SLAY_ORC ×3`, `FREE_ACT`, `IM_COLD`, `RES_COLD`, `SEE_INVIS`, Activate: MASS_BANISHMENT (1000 turns)
- Maia-level weapon; IM_COLD and MASS_BANISHMENT are extremely rare abilities

**Lochaber Axe 'Mundwine'** (N:105) — `I:22:28` 3d8, `P:+12/+17`, depth 30, rarity 8
- `SLAY_EVIL ×2`, `SLAY_ANIMAL ×2`, `SLAY_DEMON ×3`, `RES_ACID/ELEC/FIRE/COLD`

**Scythe 'Avavir'** (N:109) — `I:22:17` 5d3, `P:+8/+8/+10AC`, depth 40, rarity 8
- `+DEX, +CHR (+3 pval)`, `BRAND_COLD`, `BRAND_FIRE`, `FREE_ACT`, `RES_FIRE`, `RES_COLD`, `RES_LITE`, `LITE`, `SEE_INVIS`, Activate: WORD OF RECALL (200 turns)

**Spear of Melkor** (N:92) — `I:22:2` 4d6, `P:-12/+20`, depth 65, rarity 45
- `STEALTH, +WIS (-4 pval)`, `BRAND_POIS`, `RES_DARK`, `RES_BLIND`, `RES_LITE`, `RES_NETHR`
- `HEAVY_CURSE | LIGHT_CURSE` — Morgoth's original weapon; cursed and stat-draining

---

### Hafted/Blunt Weapons (TV_HAFTED)

**Flail 'Totila'** (N:112) — `I:21:13` 3d6, `P:+6/+8`, depth 20, rarity 8
- `STEALTH, SLAY_EVIL ×2 (+2 pval)`, `BRAND_FIRE`, `RES_FIRE`, `RES_CONFU`, Activate: CONFUSE (15 turns)

**Mace 'Taratol'** (N:116) — `I:21:5` 3d4, `P:+12/+12`, depth 20, rarity 15
- `KILL_DRAGON ×5`, `BRAND_ELEC`, `IM_ELEC`, `RES_ELEC`, Activate: HASTE (100+d100 turns)
- KILL_DRAGON on a Mace; valuable Activate

**Morning Star 'Bloodspike'** (N:114) — `I:21:12` 2d6, `P:+8/+22`, depth 20, rarity 30
- `+STR (+4 pval)`, `BRAND_POIS`, `SLAY_ANIMAL ×2`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `RES_NEXUS`, `SEE_INVIS`

**Morning Star 'Firestar'** (N:115) — `I:21:12` 2d6, `P:+5/+7/+2AC`, depth 20, rarity 15
- `BRAND_FIRE`, `RES_FIRE`, Activate: FIRE BALL 2 (20 turns)

**Quarterstaff 'Nar-i-vagil'** (N:118) — `I:21:3` 1d9, `P:+10/+20`, depth 20, rarity 18
- `+INT (+3 pval)`, `SLAY_ANIMAL ×2`, `BRAND_FIRE`, `RES_FIRE`

**Quarterstaff 'Eriril'** (N:119) — `I:21:3` 1d9, `P:+3/+5`, depth 20, rarity 18
- `+INT, +WIS (+4 pval)`, `SLAY_EVIL ×2`, `RES_LITE`, `LITE`, `SEE_INVIS`, Activate: IDENTIFY (10 turns)
- Utility artifact; Identify-on-demand is very useful

**Quarterstaff of Olorin** (N:120) — `I:21:3` 2d9, `P:+10/+13`, depth 30, rarity 105
- `+INT, +WIS, +CHR, KILL_DEMON ×5 (+4 pval)`, `SLAY_EVIL ×2`, `BRAND_FIRE`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `HOLD_LIFE`, `RES_FIRE`, `RES_NETHR`, `SEE_INVIS`, Activate: PROBE (20 turns)
- Gandalf's staff; 2d9 damage base is unique; one of the finest mage/priest weapons

**Lucerne Hammer 'Turmil'** (N:122) — `I:21:10` 2d5, `P:+10/+6/+8AC`, depth 20, rarity 15
- `+WIS, +INFRA (+4 pval)`, `BRAND_COLD`, `SLAY_ORC ×3`, `RES_COLD`, `RES_LITE`, `LITE`, `REGEN`, Activate: DRAIN_LIFE1 (40 turns)

**War Hammer of Aule** (N:117) — `I:21:8` 9d3, `P:+19/+21/+5AC`, depth 40, rarity 75
- `+WIS, TUNNEL (+4 pval)`, `RES_FEAR`, `KILL_DRAGON ×5`, `SLAY_EVIL ×2`, `BRAND_ACID`, `SLAY_UNDEAD ×3`, `SLAY_DEMON ×3`, `FREE_ACT`, `RES_ACID/ELEC/FIRE/COLD/NEXUS`, `SEE_INVIS`
- 9d3 on a War Hammer base; exceptional to-hit/to-dam; full elemental coverage

**Two-Handed Flail 'Thunderfist'** (N:113) — `I:21:18` 4d6, `P:+5/+18`, depth 45, rarity 38
- `+STR, +CON, RES_FEAR (+4 pval)`, `SLAY_ANIMAL ×2`, `BRAND_FIRE`, `BRAND_ELEC`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `RES_ELEC`, `RES_FIRE`, `RES_DARK`

**Mace of Disruption 'Deathwreaker'** (N:121) — `I:21:20` 7d8, `P:+18/+18`, depth 80, rarity 38
- `+STR, TUNNEL, AGGRAVATE (+6 pval)`, `SLAY_DRAGON ×3`, `SLAY_ANIMAL ×2`, `SLAY_EVIL ×2`, `KILL_UNDEAD ×5`, `BRAND_FIRE`, `IM_FIRE`, `RES_FIRE`, `RES_DARK`, `RES_CHAOS`, `RES_DISEN`
- 7d8 (avg 31.5) on the already-powerful Mace of Disruption base; IM_FIRE is exceptional but AGGRAVATE is punishing

**Mighty Hammer 'Grond'** (N:111) — `I:21:50` 9d9, `P:+5/+25/+10AC`, depth 100, rarity 1 (Morgoth's drop)
- `KILL_DRAGON ×5`, `SLAY_ANIMAL ×2`, `SLAY_EVIL ×2`, `KILL_UNDEAD ×5`, `KILL_DEMON ×5`, `SLAY_TROLL ×3`, `SLAY_ORC ×3`, `IMPACT`, `SEE_INVIS`, `TELEPATHY`, `AGGRAVATE`
- The Hammer of the Underworld; 9d9 (avg 45) is the highest in the game; dropped by Morgoth

**Whip of Gothmog** (N:123) — `I:21:2` 6d3, `P:+13/+15`, depth 60, rarity 25
- `+INT, +DEX, +WIS, AGGRAVATE (-3 pval)`, `HEAVY_CURSE | LIGHT_CURSE`, `BRAND_FIRE`, `IM_FIRE`, `RES_ELEC`, `RES_DARK`, `LITE`, `SLAY_ANIMAL ×2`, `KILL_DRAGON ×5`, `SLAY_TROLL ×3`, `SLAY_GIANT ×3`, Activate: FIRE3 (15 turns)
- The Balrog lord's weapon; cursed but with IM_FIRE and exceptional activation

---

## Part 5 — Weapon Selection by Class

### Warrior
- Best melee class; up to **6 blows/round** base (more with Extra Attacks)
- Can use any weapon effectively; Two-Handed Swords (3d6) and Executioner's Swords (4d5) preferred for raw damage
- Ego priority: `of Extra Attacks` > `(Holy Avenger)` > `of Gondolin` > `of Westernesse`
- Artifact targets: Anduril, Ringil, Zarcuthra, Grond

### Mage
- Only **4 blows/round** max, limited by high `min_weight` (40); light weapons preferred
- Daggers (1d4) and Rapiers (1d6) best for weight — no attack penalty below 40 tenths
- Artifact targets: Sting (adds STR/DEX/CON/SPEED), Dagmor, Forasgil, Angrist
- Most Mages prefer wands/staves over melee

### Priest
- **5 blows/round** max; **must** use blunt weapons or blessed edged weapons to avoid -2/-2 icky penalty
- Quarterstaff (1d9) is the classic; War Hammer for heavy combat
- Artifact targets: Olorin (2d9, best staff), Aule (9d3, supreme), Turmil, Eriril
- BLESSED weapons from egos allow edged weapon use without penalty

### Rogue
- **5 blows/round**; backstab multiplier: `×(3 + level/40)` vs sleeping, `×1.5` vs fleeing
- Light fast weapons maximise backstab potential: Dagger, Rapier, Main Gauche
- Artifact targets: Angrist, Gondricam (DEX+STEALTH), Dagmor (BLOWS+REGEN)
- CF_BACK_STAB ability makes sleeping-monster damage extraordinary

### Ranger
- **5 blows/round**, good att_multiply (4); effective in both melee and ranged
- Ranged emphasis: Long Bow ego `of Lothlorien` for best archery
- Melee: Long Swords / Katana
- Most powerful when combining ranged snipes with melee finisher

### Paladin
- **5 blows/round**, att_multiply 4; better than Priest at melee
- Blunt weapons preferred; BLESSED weapons available without penalty
- Heavy hitters: Mace of Disruption, Two-Handed Flail
- Artifact targets: Anduril (BLESSED+melee supremacy), Osondir, Aeglos

---

## Part 6 — Weapon Availability and Generation

### Dungeon Depth Reference

Weapons in the dungeon are placed based on their `A:` (allocation) entries. Format: `depth/rarity` where lower rarity = more common.

| Depth Band | Available Weapons |
|-----------|------------------|
| 0–5 | Daggers, Short Swords, Whips, Main Gauche, Spears |
| 5–15 | Most basic swords and hafted; Short/Long Bows |
| 15–30 | Two-handed swords, polearms; Bastard Sword, Battle Axe |
| 30–50 | Executioner's Sword, Great Axe, Scythe, Two-Handed Flail |
| 50–70 | Seeker Arrows/Bolts; Blade of Chaos, Scythe of Slicing |
| 70–90 | Mace of Disruption, deep ego weapons |
| 80–100 | Deathwreaker, Grond (Morgoth drop only) |

### Ego Item Generation Probability

Ego weapons are assigned during item generation. The `rating` field affects dungeon level "feeling" — higher-rated items make the dungeon feel more dangerous/rewarding. The `rarity` within the ego_item table determines how often a generated magical weapon gets this particular ego; lower rarity = more common.

Most common egos (rarity 6–8): `of Slay X` simple slays, elemental brands  
Uncommon egos (rarity 10–12): `(Holy Avenger)`, `(Defender)`, Extra Attacks  
Rare egos (rarity 20+): `*Slay*` enhanced slays, `of Fury`, `of Gondolin`

### Artifact Rarity Summary

| Rarity | Example Artifacts |
|--------|------------------|
| 3–8 | Elemental daggers (Narthanc/Nimthanc/Dethanc), Gondricam, Forasgil |
| 10–20 | Sting, Glamdring, Orcrist, Anduril, most combat axes |
| 25–45 | Ringil (120), Arunruth, Aeglos, Rilia, Totila |
| 80–120 | Zarcuthra (180), Durin (90), Aule (75), Olorin (105) |
| 1 | Grond (Morgoth only, always drops) |

---

## Quick Reference: Average Damage by Base Weapon

Sorted by `average base damage = dd × (ds+1) / 2`:

| Weapon | Dice | Avg Dam | Type |
|--------|------|---------|------|
| Scythe of Slicing | 8d4 | 20.0 | Polearm |
| Mace of Disruption | 5d8 | 22.5 | Hafted |
| Glaive of Pain (artifact) | 9d6 | 31.5 | Polearm |
| War Hammer of Aule (art.) | 9d3 | 18.0 | Hafted |
| Grond (artifact) | 9d9 | 45.0 | Hafted |
| Katana 'Aglarang' (art.) | 8d4 | 20.0 | Sword |
| Blade of Chaos | 6d5 | 18.0 | Sword |
| Lochaber Axe | 3d8 | 13.5 | Polearm |
| Two-Handed Flail | 3d6 | 10.5 | Hafted |
| Two-Handed Sword | 3d6 | 10.5 | Sword |
| Great Axe | 4d4 | 10.0 | Polearm |
| Executioner's Sword | 4d5 | 12.0 | Sword |
| Quarterstaff | 1d9 | 5.0 | Hafted |
| Scythe | 5d3 | 10.0 | Polearm |
| Morning Star | 2d6 | 7.0 | Hafted |
| Flail | 2d6 | 7.0 | Hafted |
| Long Sword | 2d5 | 6.0 | Sword |
| Broad Sword | 2d5 | 6.0 | Sword |
| Dagger | 1d4 | 2.5 | Sword |
| Short Bow + Arrow | — | — | Ranged (multiplied by bow mod) |

Note: Artifact weapons often have enhanced dice or very high to-dam bonuses that far exceed base weapon averages.
