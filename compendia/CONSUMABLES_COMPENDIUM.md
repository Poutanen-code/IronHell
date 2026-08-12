# Consumables Compendium

_Reference for the MAngband 1.5.3 source (`lib/edit/object.txt`) and IronHell implementation._

---

## How to Read This Document

| Column | Meaning |
|--------|---------|
| **N** | MAngband object serial number (`N:<n>:<name>`) |
| **tval** | Item type value: 80 = food/mushroom, 39 = light, 77 = flask |
| **sval** | Item sub-value |
| **pval** | Modifier value (nutrition for food, turns of light for lights) |
| **Dungeon Level** | Earliest floor (`A:<level>`) |
| **Rarity** | Allocation rarity (`A:<level>/<rarity>`) — higher = rarer |
| **Weight** | Weight in tenth-pounds |
| **Cost** | Base gold value |

Allocation lines (`A:`) may have multiple depth/rarity pairs, meaning the item appears at several dungeon levels with differing frequencies.

---

## 1. Mushrooms (tval 80, sval 0–19)

Mushrooms are hazardous forageables found throughout the dungeon. Unlike potions, mushrooms are **identified on sight** (`EASY_KNOW`). All have `pval 500` (nourishment value).

| N | Name | Depth | Rarity | Wt | Cost | Effect |
|---|------|-------|--------|----|------|--------|
| 1 | Mushroom of Blindness | 5 | 5/1 | 1 | 0 | Blindness 199+1d200 turns (RES_BLIND negates) |
| 2 | Mushroom of Paranoia | 5 | 5/1 | 1 | 0 | Fear 9+1d10 turns (RES_FEAR negates) |
| 3 | Mushroom of Confusion | 5 | 5/1 | 1 | 0 | Confusion 9+1d10 turns (RES_CONFU negates) |
| 4 | Mushroom of Hallucination | 10 | 10/1 | 1 | 0 | Hallucination 249+1d250 turns (RES_CHAOS negates) |
| 5 | Mushroom of Cure Poison | 10 | 10/1 | 1 | 60 | Neutralizes poison |
| 6 | Mushroom of Cure Blindness | 10 | 10/1 | 1 | 50 | Cures blindness |
| 7 | Mushroom of Cure Paranoia | 10 | 10/1 | 1 | 25 | Removes fear |
| 8 | Mushroom of Cure Confusion | 10 | 10/1 | 1 | 50 | Cures confusion |
| 9 | Mushroom of Weakness | 10 | 10/1 | 1 | 0 | 6d6 damage + drains STR (SUST_STR negates drain) |
| 10 | Mushroom of Unhealth | 15 | 15/1 | 1 | 50 | 10d10 damage + drains CON (SUST_CON negates drain); throwable for 10d10 |
| 11 | Mushroom of Restore Constitution | 20 | 20/1 | 1 | 350 | Restores CON to maximum |
| 12 | Mushroom of Restoring | 20 | 20/8 + 30/4 + 40/1 | 1 | 1000 | Restores all stats to maximum |
| 13 | Mushroom of Stupidity | 15 | 15/1 | 1 | 0 | 8d8 damage + drains INT (SUST_INT negates drain) |
| 14 | Mushroom of Naivety | 15 | 15/1 | 1 | 0 | 8d8 damage + drains WIS (SUST_WIS negates drain) |
| 15 | Mushroom of Poison | 5 | 5/1 × 2 | 1 | 0 | Adds 9+1d10 to poison counter (RES_POIS negates) |
| 16 | Mushroom of Sickness | 10 | 10/1 | 1 | 0 | 6d6 damage + drains CON (SUST_CON negates drain) |
| 17 | Mushroom of Paralysis | 20 | 20/1 | 1 | 0 | Paralysis 9+1d10 turns (FREE_ACT negates) |
| 18 | Mushroom of Restore Strength | 20 | 20/1 | 1 | 350 | Restores STR to maximum |
| 19 | Mushroom of Disease | 20 | 20/1 | 1 | 50 | 10d10 damage + drains STR (SUST_STR negates drain); throwable for 10d10 |
| 20 | Mushroom of Cure Serious Wounds | 15 | 15/1 | 1 | 75 | Heals 4d8 HP |

### Mushroom Design Notes
- Mushrooms with negative effects (1–4, 9, 13–17, 19) have cost 0 — they are worthless to sell.
- Mushroom of Restoring (N:12) has three allocation tiers (depth 20, 30, 40) making it progressively rarer at deeper levels but more commonly found overall.
- Mushrooms of Unhealth (N:10) and Disease (N:19) can be **thrown** as weapons (`P:0:10d10`).
- The "Cure" mushrooms (5–8) are always beneficial and safe to eat.

---

## 2. Normal Food (tval 80, sval 20–39, 42+)

Standard food items provide nourishment. They are **always identified** (`EASY_KNOW`).

| N | Name | Depth | Rarity | Wt | Cost | Nutrition (pval) | Effect |
|---|------|-------|--------|----|------|------------------|--------|
| 21 | Ration of Food | 0 | 0/1 + 5/1 + 10/1 | 10 | 3 | 5000 | Full meal |
| 22 | Hard Biscuit | 0 | — | 2 | 1 | 500 | Light snack |
| 23 | Strip of Beef Jerky | 0 | — | 2 | 2 | 1500 | Moderate nourishment |
| 24 | Slime Mold | 1 | 1/1 | 5 | 2 | 3000 | Good nourishment |
| 25 | Piece of Elvish Waybread | 5 | 5/1 + 10/1 + 20/1 | 3 | 10 | 7500 | Full meal + heals 4d8 + neutralizes poison |
| 26 | Pint of Fine Ale | 0 | — | 5 | 1 | 500 | Light nourishment |
| 27 | Pint of Fine Wine | 0 | — | 10 | 2 | 1000 | Moderate nourishment |

### MAngband Extra Food (tval 80, Mangband additions, N:522–527)

These town-grown vegetables are exclusive to MAngband, available from the general store. Each heals 1d4 HP in addition to providing nourishment.

| N | Name | Depth | Wt | Cost | Nutrition (pval) | Heal |
|---|------|-------|----|------|------------------|------|
| 522 | Potato | 0 | 2 | 1 | 1500 | 1d4 HP |
| 523 | Head of Cabbage | 0 | 3 | 1 | 1500 | 1d4 HP |
| 524 | Carrot | 0 | 1 | 2 | 500 | 1d4 HP |
| 525 | Beet | 0 | 1 | 2 | 750 | 1d4 HP |
| 526 | Squash | 0 | 3 | 1 | 2000 | 1d4 HP |
| 527 | Ear of Corn | 0 | 2 | 2 | 1500 | 1d4 HP |

### Food Design Notes
- **Elvish Waybread** (N:25) is the premier food — heals AND nourishes, with three allocation depths ensuring it remains relevant throughout.
- **Rations of Food** are the baseline survival item — highest pval (5000) means a single ration fully satisfies hunger.
- **Ale and Wine** provide modest nourishment but no other effect.
- MAngband vegetables have no `EASY_KNOW` flag (not yet obvious to the player until eaten), unlike standard food.

---

## 3. Light Sources (tval 39)

Light sources are equipped in the **light** slot. They grant illumination radius. Some consume fuel; others are permanent. All identified on sight (`EASY_KNOW` where applicable).

| N | Name | Depth | Rarity | Wt | Cost | Fuel (pval) | Radius | Permanent? | Notes |
|---|------|-------|--------|----|------|-------------|--------|------------|-------|
| 346 | Wooden Torch | 1 | 1/1 | 30 | 2 | 4000 | 1 sq | No | Can be fueled from another torch; max 5000 turns |
| 347 | Brass Lantern | 3 | 3/1 | 50 | 35 | 7500 | 2 sq | No | Can be refueled with Flask of Oil; max 15000 turns; IGNORE_FIRE |
| 529 | Dwarven Lantern | 20 | 20/20 | 10 | 2000 | 10000 | 2 sq | **Yes** | Permanent light; IGNORE_ACID/ELEC/FIRE/COLD; rarity 20 (common) |
| 530 | Feanorian Lamp | 40 | 40/10 | 10 | 6000 | 10000 | 3 sq | **Yes** | Permanent light; IGNORE_ACID/ELEC/FIRE/COLD; rarity 10 (uncommon) |

### Artifact Light Sources

| Artifact | Base type | Depth | Effect | Activation |
|----------|-----------|-------|--------|------------|
| Phial of Galadriel | Phial (tval 39, sval 4) | 5 | Permanent light radius 3 | ILLUMINATION (10+1d10 recharge): lights up room |
| Star of Elendil | Star (tval 39, sval 5) | 30 | Permanent light + SEE_INVIS | MAGIC_MAP (50+1d50): maps entire level |
| Arkenstone of Thrain | Arkenstone (tval 39, sval 6) | 50 | Permanent light + SEE_INVIS + HOLD_LIFE + RES_LITE + RES_DARK | DETECT (30+1d30): detect all on level |
| Palantir of Westernesse | Palantir (tval 39, sval 7) | 75 | INT/WIS/SEARCH/INFRA/SEE_INVIS/TELEPATHY + downsides | CLAIRVOYANCE (50+1d50): total level knowledge |

### Flask of Oil (tval 77)

| N | Name | Depth | Rarity | Wt | Cost | Notes |
|---|------|-------|--------|----|------|-------|
| 348 | Flask of Oil | 1 | 1/1 | 10 | 3 | Refuels Brass Lantern (+7500 turns, max 15000). Thrown as a lit flask: 2d6 fire damage. |

### Light Source Design Notes
- **Torches** are the first light source — 1-square radius, cheap, common, but consume quickly (4000 turns).
- **Brass Lanterns** are the middle tier — 2-square radius, refuelable with oil flasks.
- **Dwarven Lanterns** (MAngband addition) are the first permanent light source — found from depth 20, extremely common (rarity 20), weight only 10, cost 2000gp.
- **Feanorian Lamps** (MAngband addition) are the best standard light — 3-square radius, permanent, depth 40, rarity 10, cost 6000gp.
- Artifact lights (Phial, Star, Arkenstone) are the pinnacle — each has an activation power.
- The Palantir has powerful vision abilities but imposes AGGRAVATE and DRAIN_EXP — a cursed convenience.
- Flasks of oil can also be thrown as incendiary weapons (2d6 fire, treated as lit on impact).

---

## 4. Potions (tval 75)

Potions are the primary consumable. They have **randomised "flavor" colors** — the player must identify each potion type. Effects are immediate on quaffing.

### How to Read Potion Entries

- **sval** — MAngband internal sub-value
- **pval** — carried over as a modifier (most potions: unused/0)
- **Depth/Rarity** — allocation tiers from the `A:` lines
- **Cost** — base gold value (0 = harmful, worth nothing)

---

### 4.1 Nourishment Potions

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 222 | Slime Mold Juice | 2 | 0 | 0/1 | 2 | Nourishment only (+400 food). Always icky green. |
| 223 | Apple Juice | 1 | 0 | 0/1 | 1 | Nourishment only (+250 food). Always light brown. |
| 224 | Water | 0 | 0 | 0/1 | 1 | Nourishment only (+200 food). Always clear. |
| 255 | Salt Water | 5 | 0 | 0/1 | 0 | Vomiting: sets food to near-starvation + clears all poison + paralysis 4 turns (bypasses FREE_ACT!) |

### 4.2 Harmful Potions

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 226 | Potion of Weakness | 16 | 3 | 3/1 | 0 | Drains STR (SUST_STR negates) |
| 229 | Potion of Stupidity | 17 | 20 | 20/1 | 0 | Drains INT (SUST_INT negates) |
| 232 | Potion of Naivety | 18 | 20 | 20/1 | 0 | Drains WIS (SUST_WIS negates) |
| 235 | Potion of Ugliness | 21 | 20 | 20/1 | 0 | Drains CHR (SUST_CHR negates) |
| 238 | Potion of Clumsiness | 19 | 5 | 5/1 | 0 | Drains DEX (SUST_DEX negates) |
| 239 | Potion of Sickliness | 20 | 10 | 10/1 | 0 | Drains CON (SUST_CON negates) |
| 245 | Potion of Sleep | 11 | 0 | 0/1 | 0 | Paralysis 3+1d4 turns (FREE_ACT negates) |
| 246 | Potion of Blindness | 7 | 0 | 0/1 | 0 | Blindness 99+1d100 turns (RES_BLIND negates) |
| 247 | Potion of Confusion | 9 | 0 | 0/1 | 0 | Confusion 14+1d20 turns (RES_CONFU negates); pval 50 nourishment |
| 248 | Potion of Poison | 6 | 3 | 3/1 | 0 | Adds 9+1d15 to poison counter (RES_POIS negates) |
| 250 | Potion of Slowness | 4 | 1 | 1/1 | 0 | Speed −10 for 15+1d25 turns; pval 50 nourishment |
| 254 | Potion of Lose Memories | 13 | 10 | 10/1 | 0 | Drains 25% of experience (HOLD_LIFE negates) |

### 4.3 Healing Potions

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 237 | Potion of Cure Light Wounds | 34 | 0 | 0/1 + 1/1 + 3/1 | 15 | Heals 15 HP; 20 pts cuts/confusion; cures blindness; pval 50 nourishment |
| 240 | Potion of Cure Serious Wounds | 35 | 3 | 3/1 | 40 | Heals 1d6+19 HP; heals all cuts; cures blindness + confusion; pval 100 nourishment |
| 241 | Potion of Cure Critical Wounds | 36 | 5 | 5/1 | 100 | Heals 1d6+24 HP; cures stunning/cuts/poison/blindness/confusion; pval 100 nourishment |
| 242 | Potion of Healing | 37 | 15 | 15/1 + 60/1 | 300 | Heals 300 HP; cures stunning/cuts/poison/blindness/confusion; pval 200 nourishment |

### 4.4 Combat Potions

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 249 | Potion of Speed | 29 | 1 | 1/1 + 40/1 | 75 | Speed +10 for 15+1d25 turns; already hasted: +5 turns only |
| 257 | Potion of Heroism | 32 | 1 | 1/1 | 35 | Heals 10 HP; removes fear; +12 to-hit + RES_FEAR for 25+1d25 turns |
| 258 | Potion of Berserk Strength | 33 | 3 | 3/1 | 100 | Heals 30 HP; removes fear; +12 to-hit + RES_FEAR + **−10 AC** for 25+1d25 turns |
| 259 | Potion of Boldness | 28 | 1 | 1/1 | 10 | Removes fear only |
| 261 | Potion of Resist Heat | 30 | 1 | 1/1 | 30 | Temporary RES_FIRE for 10+1d10 turns (stacks with permanent resistance) |
| 262 | Potion of Resist Cold | 31 | 1 | 1/1 | 30 | Temporary RES_COLD for 10+1d10 turns (stacks with permanent resistance) |
| 263 | Potion of Detect Invisible | 25 | 3 | 3/1 | 50 | SEE_INVIS for 12+1d12 turns |
| 264 | Potion of Slow Poison | 26 | 1 | 1/1 | 25 | Halves current poison counter |
| 265 | Potion of Neutralize Poison | 27 | 5 | 5/1 | 75 | Sets poison counter to 0 |
| 267 | Potion of Infravision | 24 | 3 | 3/1 | 20 | Infravision +50 ft for 100+1d100 turns |

### 4.5 Restoration Potions

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 227 | Potion of Restore Strength | 42 | 25 | 25/1 | 300 | Restores STR to maximum if below maximum |
| 230 | Potion of Restore Intelligence | 43 | 25 | 25/1 | 300 | Restores INT to maximum if below maximum |
| 233 | Potion of Restore Wisdom | 44 | 25 | 25/1 | 300 | Restores WIS to maximum if below maximum |
| 236 | Potion of Restore Charisma | 47 | 20 | 20/1 | 300 | Restores CHR to maximum if below maximum |
| 252 | Potion of Restore Dexterity | 45 | 25 | 25/1 | 300 | Restores DEX to maximum if below maximum |
| 253 | Potion of Restore Constitution | 46 | 25 | 25/1 | 300 | Restores CON to maximum if below maximum |
| 260 | Potion of Restore Life Levels | 41 | 40 | 40/1 | 400 | Restores experience to maximum if below maximum |
| 266 | Potion of Restore Mana | 40 | 25 | 25/1 + 70/1 | 350 | Restores mana points to maximum if below maximum |

### 4.6 Stat-Increasing Potions (Permanent)

These potions **permanently** raise the stat by 1 (first acting as the corresponding Restore potion if stat is drained):

| N | Name | sval | Depth | Rarity | Cost | Stat raised |
|---|------|------|-------|--------|------|-------------|
| 225 | Potion of Strength | 48 | 30 | 30/1 | 8000 | STR +1 permanent |
| 228 | Potion of Intelligence | 49 | 30 | 30/1 | 8000 | INT +1 permanent |
| 231 | Potion of Wisdom | 50 | 30 | 30/1 | 8000 | WIS +1 permanent |
| 234 | Potion of Charisma | 53 | 20 | 20/1 | 1000 | CHR +1 permanent |
| 243 | Potion of Constitution | 52 | 30 | 30/1 | 8000 | CON +1 permanent |
| 244 | Potion of Experience | 59 | 65 | 65/1 | 25000 | +100000 XP or half current XP + 10, whichever is less |
| 251 | Potion of Dexterity | 51 | 30 | 30/1 | 8000 | DEX +1 permanent |

### 4.7 Utility Potions

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 256 | Potion of Enlightenment | 56 | 25 | 25/1 | 800 | Lights entire dungeon level + full magic map |

### Potion Design Notes

- **Stat-Increasing Potions** (STR/INT/WIS/CON/DEX) are the most valuable single-use items in the game (8000gp base, depth 30). They bypass the stat cap by first restoring then boosting.
- **Potion of Experience** (65gp 25000) only appears at great depth (65) and is among the rarest potions. It grants up to 100,000 XP but is capped at half current XP+10 to prevent exploitation at low levels.
- **Berserk Strength** vs **Heroism**: Berserk heals more and is cheaper, but imposes −10 AC — risky against hard-hitting enemies.
- **Speed** potions have two allocation depths (1 and 40) — they can appear early but become commoner at deep levels.
- **Salt Water** is the most dangerous "food" — it bypasses FREE_ACT for paralysis, starves the character, but does clear all poison. A desperate anti-poison measure.

---

## 5. Scrolls (tval 70)

Scrolls have random "flavor" (adjective+color descriptions). Reading requires light and the ability to read. Effects are listed in order of MAngband serial number.

### 5.1 Utility Scrolls

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 173 | Scroll of Enchant Weapon To-Hit | 17 | 15 | 15/1 | 125 | Attempts to enchant weapon's to-hit bonus +1 |
| 174 | Scroll of Enchant Weapon To-Dam | 18 | 15 | 15/1 | 125 | Attempts to enchant weapon's to-dam bonus +1 |
| 175 | Scroll of Enchant Armor | 16 | 15 | 15/1 | 125 | Attempts to enchant armor's AC bonus +1 |
| 176 | Scroll of Identify | 12 | 1 | 1/1 + 5/1 + 10/1 + 30/1 | 50 | Reveals all normal powers of one object (can abort) |
| 177 | Scroll of *Identify* | 13 | 25–70/1 | 1000 | Reveals ALL powers (including hidden) and implants permanently in memory |
| 180 | Remove Curse | 14 | 10 | 10/1 | 100 | Removes all ordinary curses from equipped items (heavy/permanent unaffected) |
| 191 | *Remove Curse* | 15 | 50 | 50/2 + 95/1 | 8000 | Removes ordinary AND heavy curses (permanent unaffected) |
| 192 | Treasure Detection | 26 | 0 | 0/1 | 15 | Detects all treasure in the area |
| 193 | Object Detection | 27 | 0 | 0/1 | 15 | Detects all objects in the area |
| 194 | Trap Detection | 28 | 5 | 5/1 + 10/1 | 35 | Detects all traps in the area |
| 197 | Door/Stair Location | 29 | 5 | 5/1 + 10/1 + 15/1 | 35 | Detects all stairs and doors in the area |
| 201 | Detect Invisible | 30 | 1 | 1/1 | 15 | Detects all invisible monsters in the area |
| 189 | Magic Mapping | 25 | 5 | 5/1 | 40 | Maps area centred on player |
| 206 | Recharging | 22 | 40 | 40/1 | 200 | Recharges wand or staff; chance of failure destroys charges |
| 210 | Satisfy Hunger | 32 | 5 | 5/1 | 10 | Sets food to "satisfied" level |
| 217 | Blessing | 33 | 1 | 1/1 | 15 | +5 AC + +10 to-hit for 6+1d12 turns |
| 218 | Holy Chant | 34 | 10 | 10/1 | 40 | +5 AC + +10 to-hit for 12+1d24 turns |
| 219 | Holy Prayer | 35 | 25 | 25/1 | 80 | +5 AC + +10 to-hit for 24+1d48 turns |
| 220 | Word of Recall | 11 | 5 | 5/1 | 150 | Invokes recall to/from town; second scroll cancels |

### 5.2 Light and Darkness Scrolls

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 181 | Light | 24 | 0 | 0/1 + 3/1 + 10/1 | 15 | Lights 2-sq radius + entire room; 2d8 damage to light-sensitive creatures |
| 208 | Darkness | 0 | 1 | 1/1 | 0 | Darkens 3-sq + room; blinds 3+1d5 turns (RES_BLIND negates) |

### 5.3 Teleportation Scrolls

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 185 | Phase Door | 8 | 1 | 1/1 | 15 | Teleports up to 10 squares randomly |
| 186 | Teleportation | 9 | 10 | 10/1 | 40 | Teleports up to 100 squares randomly |
| 187 | Teleport Level | 10 | 20 | 20/1 | 50 | Teleports 1 level up or down (random); always down from town, always up from floor 99/100 unless bosses killed; no effect with Ironman option |

### 5.4 Monster Control Scrolls

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 184 | Summon Monster | 4 | 1 | 1/1 | 0 | Summons 1d3 monsters at current depth; group/escort rules apply |
| 188 | Monster Confusion | 36 | 5 | 5/1 | 30 | Enchants hands to glow red; next melee attack attempts to confuse monster |
| 200 | Mass Banishment | 45 | 50 | 50/4 + 95/4 | 1000 | Removes all non-unique monsters within 20 squares; 1d3 damage per monster removed |
| 202 | Aggravate Monster | 1 | 5 | 5/1 | 0 | Wakes all monsters within 40 squares; hastes all visible monsters |
| 207 | Banishment | 44 | 40 | 40/4 + 80/2 | 750 | Removes all monsters of chosen symbol; uniques unaffected; 1d4 damage per removed |
| 211 | Dispel Undead | 42 | 40 | 40/1 | 200 | 60 damage to all undead monsters in line of sight |
| 216 | Summon Undead | 5 | 15 | 15/1 | 0 | Summons 1d3 non-unique undead monsters |

### 5.5 Enchantment and Curse Scrolls

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 212 | *Enchant Weapon* | 21 | 50 | 50/1 | 500 | Enchants weapon to-hit AND to-dam 1d3 times each |
| 213 | Curse Weapon | 3 | 50 | 50/1 | 0 | Curses wielded weapon: removes special powers, sets both bonuses to −2d5; artifact 50% resist |
| 214 | *Enchant Armor* | 20 | 50 | 50/1 × 2 | 500 | Enchants armor 2+1d3 times |
| 215 | Curse Armor | 2 | 50 | 50/1 | 0 | Curses body armor: removes powers, base AC → 0, magic bonus → −2d5; artifact 50% resist |

### 5.6 Warding Scrolls

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 190 | Rune of Protection | 38 | 60 | 60/4 + 90/1 | 500 | Inscribes glyph of warding under player; monsters can't move onto glyph (level/550 break chance) |
| 203 | Trap Creation | 7 | 10 | 10/1 | 0 | Creates traps on all empty squares within 1-square radius |
| 204 | Trap/Door Destruction | 39 | 10 | 10/1 | 50 | Destroys all traps and doors in 1-square radius |
| 209 | Protection from Evil | 37 | 30 | 30/1 | 50 | Grants protection from evil for 1d25 + 3×character_level turns |

### 5.7 Rare / Special Scrolls

| N | Name | sval | Depth | Rarity | Cost | Effect |
|---|------|------|-------|--------|------|--------|
| 178 | Scroll of Life | 48 | 20 | 20/1 | 500 | Restores ghost adjacent to player to life; or restores experience to maximum if no ghost adjacent |
| 179 | Artifact Creation | 6 | 99 | — | 1,000,000 | Creates an artifact (MAngband server-only; not spawned normally in shops) |
| 182 | House Creation | 49 | 99 | 99/40 | 1,000,000 | MAngband multiplayer: creates house from House Foundation Stones |
| 198 | Acquirement | 46 | 20 | 20/8 | 100,000 | Creates 1 good object near the player |
| 199 | *Acquirement* | 47 | 60 | 60/16 | 200,000 | Creates 1+1d2 good objects near the player |
| 221 | *Destruction* | 41 | 40 | 40/1 | 250 | Destroys 15-sq radius (random walls/floor); removes all monsters + non-artifact objects; blinds 10+1d10 turns unless RES_BLIND or RES_LITE |

### Scroll Design Notes

- **Identify** has four allocation tiers (depths 1, 5, 10, 30) — remains common throughout the game.
- **Word of Recall** is the primary town-return mechanism; cost 150gp makes it the most practical for deep dives.
- **Acquirement** (rarity 8 at depth 20) is surprisingly accessible early but very valuable; *Acquirement* (rarity 16 at depth 60) is proportionally rarer relative to depth.
- **Artifact Creation** and **House Creation** scrolls are effectively server admin items — cost 1,000,000gp and have no allocation line (can't drop randomly from *Artifact Creation*).
- **Rune of Protection** becomes more common at depth 90 (rarity 1) — essential for late-game pit-fighting.
- Cursed scrolls (Curse Weapon, Curse Armor, Aggravate, Darkness, Trap Creation, Summon Undead, Summon Monster) all have cost 0.

---

## 6. Chests (tval 7)

Chests are containers found in the dungeon that hold generated loot. They may be trapped.

| N | Name | Depth | Rarity | Wt | Cost | Trap Damage |
|---|------|-------|--------|----|------|-------------|
| 338 | Small Wooden Chest | 5 | 5/1 | 250 | 20 | 2d3 |
| 339 | Large Wooden Chest | 15 | 15/1 | 500 | 60 | 2d5 |
| 340 | Small Iron Chest | 25 | 25/1 | 300 | 100 | 2d4 |
| 341 | Large Iron Chest | 35 | 35/1 | 1000 | 150 | 2d6 |
| 342 | Small Steel Chest | 45 | 45/1 | 500 | 200 | 2d4 |
| 343 | Large Steel Chest | 55 | 55/1 | 1000 | 250 | 2d6 |
| 344 | Ruined Chest | 0 | 75/1 | 250 | 0 | — (empty, already opened) |

**Chest Mechanics:**
- Chests may contain items scaled to chest depth.
- Chests may be trapped (poison needles, explosion, gas traps, summons).
- The `D` key attempts to disarm a chest before opening.
- The `P:` line defines trap dice — bigger chests have more dangerous traps.

---

## 7. Iron Spikes (tval 5)

| N | Name | Depth | Rarity | Wt | Cost | Effect |
|---|------|-------|--------|----|------|--------|
| 345 | Iron Spike | 1 | 1/1 | 10 | 1 | Jamming a door: each of first 7 spikes increases door bash resistance. Stackable. |

---

## 8. Unique Items (Artifacts)

The following is a full listing of all MAngband 1.5.3 artifacts by category, with stats and notable properties. All artifacts are immune to acid, electricity, fire, and cold destruction (`IGNORE_ACID/ELEC/FIRE/COLD` implicit).

### 8.1 Special Artifacts — Light Sources and Jewelry

| # | Name | Base | Depth | Rare | Wt | Cost | pval | Stats | Resistances | Flags | Activation |
|---|------|------|-------|------|----|------|------|-------|-------------|-------|------------|
| 1 | Phial of Galadriel | Phial | 5 | 5 | 10 | 10,000 | — | — | — | INSTA_ART | ILLUMINATION (10+10) |
| 2 | Star of Elendil | Star | 30 | 25 | 5 | 30,000 | — | — | — | SEE_INVIS, INSTA_ART | MAGIC_MAP (50+50) |
| 3 | Arkenstone of Thrain | Arkenstone | 50 | 50 | 5 | 50,000 | — | — | RES_LITE, RES_DARK | SEE_INVIS, HOLD_LIFE, INSTA_ART | DETECT (30+30) |
| 7 | Palantir of Westernesse | Palantir | 75 | 60 | 200 | 100,000 | +2 INT/WIS | — | RES_CHAOS, RES_BLIND | SEE_INVIS, TELEPATHY, AGGRAVATE, DRAIN_EXP, INSTA_ART | CLAIRVOYANCE (50+50) |
| 4 | Amulet of Carlammas | Amulet | 50 | 10 | 3 | 60,000 | +2 CON | — | RES_FIRE | INSTA_ART | PROT_EVIL (225+225) |
| 5 | Amulet of Ingwe | Amulet | 65 | 30 | 3 | 90,000 | +3 INT/WIS/CHR, +INFRA | RES_ACID, RES_COLD, RES_ELEC | — | SEE_INVIS, FREE_ACT, INSTA_ART | DISP_EVIL (50+50) |
| 6 | Necklace of the Dwarves | Necklace | 70 | 50 | 3 | 75,000 | +3 STR/CON, +INFRA | — | RES_FEAR | SEE_INVIS, FREE_ACT, REGEN, LITE, INSTA_ART | — |
| 8 | Ring of Barahir | Ring | 50 | 25 | 2 | 65,000 | +1 all stats, +STEALTH | RES_POIS, RES_DARK | — | INSTA_ART | — |
| 9 | Ring of Tulkas | Ring | 70 | 50 | 2 | 150,000 | +4 STR/DEX/CON | — | RES_FEAR | INSTA_ART | HASTE2 (150+150) |
| 10 | Ring of Power 'Narya' | Ring | 70 | 60 | 2 | 100,000 | +1 all stats, +SPEED | IM_FIRE, RES_FIRE, RES_NETHR, RES_FEAR | FREE_ACT, SEE_INVIS, SLOW_DIGEST, REGEN, sustains, INSTA_ART | FIRE3 (20+20) |
| 11 | Ring of Power 'Nenya' | Ring | 80 | 70 | 2 | 200,000 | +2 all stats, +SPEED, +8/+8 | IM_COLD, RES_COLD, RES_BLIND | HOLD_LIFE, FREE_ACT, SEE_INVIS, FEATHER, REGEN, TELEPATHY, INSTA_ART | FROST5 (20+20) |
| 12 | Ring of Power 'Vilya' | Ring | 90 | 80 | 2 | 300,000 | +3 all stats, +SPEED, +10/+10 | IM_ELEC, RES_ELEC, RES_POIS, RES_DISEN | HOLD_LIFE, FREE_ACT, SEE_INVIS, FEATHER, SLOW_DIGEST, REGEN, sustains, INSTA_ART | ELEC2 (20+20) |
| 13 | Ring of Power 'The One Ring' | Ring | 100 | 100 | 2 | 5,000,000 | +5 all stats, +SPEED, +15/+15 | IM_FIRE, IM_COLD, IM_ELEC, IM_ACID, RES_POIS, RES_NETHR, RES_BLIND, RES_DISEN, RES_FEAR | SEE_INVIS, REGEN, TELEPATHY, AGGRAVATE, DRAIN_EXP, LIGHT_CURSE, HEAVY_CURSE, PERMA_CURSE, INSTA_ART | BIZZARE (30+30) |
| 14 | Elfstone 'Elessar' | Elfstone | 60 | 60 | 3 | 40,000 | +2 STR/WIS/CHR, +SPEED, +7/+7/+10 | RES_FEAR, RES_FIRE, RES_POIS | INSTA_ART | HEAL1 (200+0) |
| 15 | Jewel 'Evenstar' | Jewel | 40 | 40 | 3 | 25,000 | — | RES_DARK, RES_COLD | HOLD_LIFE, SUST_CON, SUST_WIS, SUST_INT, INSTA_ART | RESTORE_LIFE (150+0) |

### 8.2 Artifact Armor — Dragon Scale Mails

| # | Name | Depth | Rare | AC | Wt | Cost | Flags | Activation |
|---|------|-------|------|----|----|------|-------|------------|
| 16 | 'Razorback' (MHDSM) | 90 | 9 | 30+25 | 500 | 400,000 | FREE_ACT, IM_ELEC, all elem res, RES_LITE/DARK, LITE, SEE_INVIS, **AGGRAVATE** | STAR_BALL (50+0) |
| 17 | 'Bladeturner' (PDSM) | 100 | 16 | 50+35 | 600 | 500,000 | HOLD_LIFE, REGEN, all elem res, all other res | RAGE_BLESS_RESIST (400+0) |
| 18 | 'Mediator' (BDSM) | 95 | 12 | 30+25 | 500 | 400,000 | RES_CHAOS/DISEN/SHARD/SOUND/CONFU/NEXUS, FREE_ACT, SLOW_DIGEST, REGEN, **AGGRAVATE** | STAR_BALL (50+0) |

### 8.3 Artifact Armor — Heavy Armor

| # | Name | Base | Depth | Rare | AC | Cost | Stats | Resistances | Flags | Activation |
|---|------|------|-------|------|----|------|-------|-------------|-------|------------|
| 19 | 'Soulkeeper' | Adamantite Plate | 75 | 9 | 40+20 | 300,000 | +2 CON, SUST_CON | RES_ACID, RES_COLD, RES_DARK, RES_NETHR, RES_NEXUS, RES_CHAOS, RES_CONFU, RES_FEAR | HOLD_LIFE | HEAL2 (444+0) |
| 20 | of Isildur | Full Plate | 30 | 3 | 25+25 | 50,000 | +1 CON | RES_ACID, RES_ELEC, RES_FIRE, RES_COLD, RES_SOUND, RES_CONFU, RES_NEXUS | — | — |
| 21 | of the Rohirrim | Metal Brigandine | 30 | 3 | 19+15 | 30,000 | +2 STR/DEX | RES_ACID/ELEC/FIRE/COLD/CONFU/SOUND, RES_FEAR | — | — |
| 22 | 'Belegennon' | Mithril Chain | 40 | 3 | 28+20 | 105,000 | +4 STEALTH | RES_ACID/ELEC/FIRE/COLD/POIS | — | PHASE (2+0) |
| 23 | of Celeborn | Mithril Plate | 40 | 3 | 35+25 | 150,000 | +4 STR/CHR | RES_ACID/ELEC/FIRE/COLD/DARK/DISEN | — | BANISHMENT (500+0) |
| 24 | of Arvedui | Chain Mail | 20 | 3 | 14+15 | 32,000 | +2 STR/CHR | RES_ACID/ELEC/FIRE/COLD/SHARD/NEXUS | — | — |
| 25 | of Caspanion | Augmented Chain | 25 | 9 | 16+20 | 40,000 | +3 INT/WIS/CON | RES_ACID/POIS/CONFU | — | TRAP_DOOR_DEST (10+0) |

### 8.4 Artifact Armor — Light Armor

| # | Name | Base | Depth | Rare | AC | Cost | Stats | Resistances | Flags |
|---|------|------|-------|------|----|------|-------|-------------|-------|
| 26 | of Himring | Hard Leather | 50 | 20 | 6+15 | 35,000 | — | RES_CHAOS, RES_NETHR, RES_POIS | ACTIVATE: PROT_EVIL (100+100) |
| 27 | 'Hithlomir' | Soft Leather | 20 | 3 | 4+20 | 45,000 | +4 STEALTH | RES_ACID/ELEC/FIRE/COLD/DARK | — |
| 28 | 'Thalkettoth' | Leather Scale | 20 | 3 | 11+25 | 25,000 | +3 DEX, +SPEED | RES_ACID, RES_SHARD | — |

### 8.5 Artifact Shields

| # | Name | Base | Depth | Rare | AC | Cost | Stats | Resistances | Flags | Activation |
|---|------|------|-------|------|----|------|-------|-------------|-------|------------|
| 29 | of Gil-galad | Shield of Deflection | 70 | 4 | 10+20 | 65,000 | +5 WIS/CHR | RES_ELEC/ACID/DISEN/DARK | LITE, SEE_INVIS, SUST_WIS/DEX/CHR | STARLIGHT (100+0) |
| 30 | of Thorin | Small Metal Shield | 40 | 6 | 3+25 | 60,000 | +4 STR/CON | IM_ACID, RES_SOUND, RES_CHAOS, RES_FEAR | FREE_ACT | — |
| 31 | of Celegorm | Large Leather Shield | 30 | 3 | 4+20 | 12,000 | — | RES_ACID/ELEC/FIRE/COLD/LITE/DARK | — | — |
| 32 | of Anarion | Large Metal Shield | 40 | 9 | 5+20 | 160,000 | — | RES_ACID/ELEC/FIRE/COLD | SUST_STR/INT/WIS/DEX/CON/CHR | — |

### 8.6 Artifact Helms and Crowns

| # | Name | Base | Depth | Rare | AC | Cost | Stats | Resistances | Flags | Activation |
|---|------|-------|------|----|------|-------|-------------|-------|------------|------|
| 33 | of Celebrimbor | Metal Cap | 55 | 12 | 3+18 | 45,000 | +3 INT/DEX/CHR, +SEARCH | RES_FIRE/ACID/DISEN/SHARD | — | — |
| 34 | Crown of Morgoth | Massive Iron Crown | 100 | 1 | 0+0 | 10,000,000 | +125 all stats, +INFRA | RES_ACID/ELEC/FIRE/COLD/POIS/LITE/DARK/CONFU/NEXUS/NETHR | LITE, SEE_INVIS, TELEPATHY, RES_FEAR, PERMA_CURSE | — |
| 35 | Crown of Beruthiel | Iron Crown | 40 | 12 | 0+20 | 1 | −5 all stats | — | FREE_ACT, SEE_INVIS, TELEPATHY, HEAVY_CURSE | — |
| 36 | of Thranduil | Hard Leather Cap | 20 | 2 | 2+10 | 50,000 | +2 INT/WIS | RES_BLIND | TELEPATHY | — |
| 37 | of Thengel | Metal Cap | 10 | 2 | 3+12 | 22,000 | +3 WIS/CHR | RES_CONFU | — | — |
| 38 | of Hammerhand | Steel Helm | 20 | 8 | 6+20 | 45,000 | +3 STR/DEX/CON | RES_ACID/NEXUS/COLD/DARK | SUST_STR/DEX/CON, **AGGRAVATE** | — |
| 39 | of Dor-Lomin | Iron Helm | 40 | 20 | 5+20 | 300,000 | +4 STR/DEX/CON | RES_ACID/ELEC/FIRE/COLD, RES_FEAR | LITE, SEE_INVIS, TELEPATHY | — |
| 40 | 'Holhenneth' | Iron Helm | 20 | 5 | 5+10 | 100,000 | +2 INT/WIS, +SEARCH | RES_BLIND, RES_CONFU | SEE_INVIS | DETECT (55+55) |
| 41 | of Gorlim | Iron Helm | 20 | 5 | 5+10 | 1 | −5 all stats, +8/+8 | RES_FEAR | SEE_INVIS, FREE_ACT, **AGGRAVATE**, HEAVY_CURSE | — |
| 42 | Crown of Gondor | Golden Crown | 40 | 40 | 0+15 | 100,000 | +3 STR/WIS/CON, +SPEED | RES_COLD/FIRE/LITE/BLIND/CONFU/SOUND/CHAOS | LITE, SEE_INVIS, REGEN | HEAL1 (250+0) |
| 43 | Crown of Numenor | Jewel Enc. Crown | 60 | 30 | 0+18 | 50,000 | +3 INT/DEX/CHR, +SEARCH, +SPEED | RES_SHARD/SOUND/COLD/LITE/DARK/BLIND | SEE_INVIS, FREE_ACT, LITE | — |

### 8.7 Artifact Cloaks

| # | Name | Base | Depth | Rare | AC | Cost | Stats | Resistances | Flags | Activation |
|---|------|------|-------|------|----|------|-------|-------------|-------|------------|
| 44 | 'Colluin' | Cloak | 5 | 45 | 1+15 | 50,000 | — | RES_ACID/ELEC/FIRE/COLD/POIS | — | RESIST (111+0) |
| 45 | 'Holcolleth' | Cloak | 5 | 25 | 1+4 | 18,000 | +2 INT/WIS/SPEED/STEALTH | RES_ACID | — | SLEEP (55+0) |
| 46 | of Thingol | Cloak | 5 | 50 | 1+18 | 35,000 | +3 DEX/CHR | RES_ACID/FIRE/COLD | FREE_ACT | RECHARGE1 (70+0) |
| 47 | of Thorongil | Cloak | 5 | 10 | 1+10 | 8,000 | — | RES_ACID, RES_FEAR | FREE_ACT, SEE_INVIS | — |
| 48 | 'Colannon' | Cloak | 5 | 20 | 1+15 | 20,000 | +3 STEALTH/SPEED | RES_NEXUS | — | TELEPORT (45+0) |
| 49 | of Luthien | Shadow Cloak | 40 | 40 | 6+20 | 45,000 | +2 INT/WIS/CHR/SPEED/STEALTH | RES_ACID/FIRE/COLD | — | RESTORE_LIFE (250+0) |
| 50 | of Tuor | Shadow Cloak | 40 | 40 | 6+12 | 35,000 | +4 DEX/STEALTH | RES_ACID | FREE_ACT, IM_ACID, SEE_INVIS | — |

### 8.8 Artifact Gloves and Gauntlets

| # | Name | Base | Depth | Rare | AC | Cost | Stats | Resistances | Flags | Activation |
|---|------|------|-------|------|----|------|-------|-------------|-------|------------|
| 51 | of Eol | Gauntlets | 55 | 35 | 2+15, −5/−5 | 40,000 | +3 INT | RES_ELEC, RES_DARK, RES_POIS | FREE_ACT, FEATHER, **AGGRAVATE** | MANA_BOLT (30+30) |
| 52 | 'Cambeleg' | Leather Gloves | 10 | 6 | 1+15, +8/+8 | 36,000 | +2 STR/CON | — | FREE_ACT, SHOW_MODS | — |
| 53 | 'Cammithrim' | Leather Gloves | 10 | 3 | 1+10 | 30,000 | — | RES_LITE | FREE_ACT, SUST_CON, LITE | MISSILE (2+0) |
| 54 | 'Paurhach' | Gauntlets | 10 | 3 | 2+15 | 15,000 | — | RES_FIRE | REGEN | FIRE1 (8+8) |
| 55 | 'Paurnimmen' | Gauntlets | 10 | 3 | 2+15 | 13,000 | — | RES_COLD | SLOW_DIGEST | FROST1 (7+7) |
| 56 | 'Pauraegen' | Gauntlets | 10 | 3 | 2+15 | 11,000 | — | RES_ELEC | LITE | LIGHTNING_BOLT (6+6) |
| 57 | 'Paurnen' | Gauntlets | 10 | 3 | 2+15 | 12,000 | — | RES_ACID | FEATHER | ACID1 (5+5) |
| 58 | 'Camlost' | Gauntlets | 10 | 20 | 2+0, −12/−12 | 0 | +1 STR/DEX | RES_FIRE, RES_DISEN | FREE_ACT, DRAIN_EXP, **AGGRAVATE**, HEAVY_CURSE | — |
| 59 | of Fingolfin | Cesti | 40 | 15 | 5+20, +10/+10 | 110,000 | +4 DEX | RES_ACID | FREE_ACT | ARROW (30+30) |

### 8.9 Artifact Boots

| # | Name | Base | Depth | Rare | AC | Cost | Stats | Resistances | Flags | Activation |
|---|------|------|-------|------|----|------|-------|-------------|-------|------------|
| 60 | of Feanor | Hard Leather Boots | 40 | 120 | 3+20 | 300,000 | +15 SPEED | RES_NEXUS | — | HASTE1 (200+0) |
| 61 | 'Dal-i-thalion' | Soft Leather Boots | 10 | 25 | 2+15 | 40,000 | +5 DEX, SUST_CON | RES_NETHR, RES_CHAOS, RES_CONFU | FREE_ACT | REM_FEAR_POIS (5+0) |
| 62 | of Thror | Metal Shod Boots | 30 | 25 | 6+20 | 12,000 | +3 STR/CON/SPEED | RES_FEAR | — | — |
| 63 | of Wormtongue | Soft Leather Boots | 10 | 15 | 2+0, −8/−8 | 17,000 | +3 INT/DEX/STEALTH/SPEED, FEATHER | — | LIGHT_CURSE | PHASE (20+0) |

### 8.10 Artifact Weapons — Swords

| # | Name | Base | Depth | Rare | Dice | +Hit/+Dam | Cost | Stats | Flags | Activation |
|---|------|------|-------|------|------|-----------|------|-------|-------|------------|
| 64 | of Maedhros | Main Gauche | 15 | 30 | 2d5 | +12/+15 | 20,000 | +3 INT/DEX/SPEED | SLAY_TROLL, SLAY_GIANT, FREE_ACT, SEE_INVIS | — |
| 65 | 'Angrist' | Dagger | 20 | 80 | 2d4 | +10/+15+5AC | 100,000 | +4 DEX/SPEED | BRAND_ACID, SLAY_EVIL/TROLL/ORC, FREE_ACT, SUST_DEX, RES_ACID/DARK | — |
| 66 | 'Narthanc' | Dagger | 4 | 3 | 2d4 | +4/+6 | 12,000 | — | BRAND_FIRE, RES_FIRE | FIRE1 (8+8) |
| 67 | 'Nimthanc' | Dagger | 4 | 3 | 2d4 | +4/+6 | 11,000 | — | BRAND_COLD, RES_COLD | FROST1 (7+7) |
| 68 | 'Dethanc' | Dagger | 4 | 3 | 2d4 | +4/+6 | 13,000 | — | BRAND_ELEC, RES_ELEC | LIGHTNING_BOLT (6+6) |
| 69 | of Rilia | Dagger | 5 | 40 | 2d4 | +4/+3 | 15,000 | — | SLAY_ORC, BRAND_POIS, RES_POIS/DISEN | STINKING_CLOUD (4+4) |
| 70 | 'Belangil' | Dagger | 10 | 40 | 2d4 | +6/+9 | 40,000 | +2 DEX | BRAND_COLD, SEE_INVIS, SLOW_DIGEST, REGEN, RES_COLD | FROST2 (5+5) |
| 71 | 'Calris' | Bastard Sword | 30 | 15 | 5d4 | −20/+20 | 100,000 | +5 CON | KILL_DRAGON, SLAY_EVIL/DEMON/TROLL, RES_DISEN, **AGGRAVATE**, HEAVY_CURSE | — |
| 72 | 'Arunruth' | Broad Sword | 20 | 45 | 3d5 | +20/+12 | 50,000 | +4 DEX | SLAY_DEMON/ORC, FREE_ACT, RES_COLD, FEATHER, SLOW_DIGEST | FROST4 (50+0) |
| 73 | 'Glamdring' | Broad Sword | 20 | 20 | 2d5 | +10/+15 | 40,000 | +1 SEARCH | SLAY_EVIL, BRAND_FIRE, SLAY_ORC/DEMON, LITE, RES_FIRE/LITE, SLOW_DIGEST, BLESSED | — |
| 74 | 'Aeglin' | Broad Sword | 20 | 30 | 2d5 | +12/+16 | 45,000 | +1 SEARCH | SLAY_ORC/TROLL/GIANT, BRAND_ELEC, LITE, RES_ELEC/BLIND, SLOW_DIGEST, BLESSED | — |
| 75 | 'Orcrist' | Broad Sword | 20 | 20 | 2d5 | +10/+15 | 40,000 | +3 SEARCH | SLAY_EVIL, BRAND_COLD, SLAY_ORC/DRAGON, LITE, RES_COLD/DARK, SLOW_DIGEST, BLESSED | — |
| 76 | 'Gurthang' | Two-Handed Sword | 30 | 30 | 3d6 | +13/+17 | 100,000 | +2 STR | BRAND_FIRE, BRAND_POIS, KILL_DRAGON, FREE_ACT, SLOW_DIGEST, REGEN, RES_FIRE/POIS | — |
| 77 | 'Zarcuthra' | Two-Handed Sword | 30 | 180 | 4d6 | +19/+21 | 200,000 | +4 STR/CHR, +INFRA | KILL_DRAGON, SLAY_ANIMAL/EVIL, BRAND_FIRE, SLAY_UNDEAD/DEMON/TROLL/GIANT/ORC, RES_FIRE/CHAOS, FREE_ACT, SEE_INVIS, **AGGRAVATE** | — |
| 78 | 'Mormegil' | Two-Handed Sword | 30 | 15 | 3d6 | −15/−15/−10AC | 10,000 | +SPEED | BRAND_POIS, KILL_DRAGON, SLAY_UNDEAD, SEE_INVIS, HOLD_LIFE, **AGGRAVATE**, DRAIN_EXP, HEAVY_CURSE | — |
| 79 | 'Gondricam' | Cutlass | 20 | 8 | 1d7 | +10/+11 | 28,000 | +3 DEX/STEALTH | RES_ACID/ELEC/FIRE/COLD, FEATHER, SEE_INVIS, REGEN | — |
| 80 | 'Crisdurian' | Executioner's Sword | 40 | 25 | 4d5 | +18/+19 | 100,000 | — | SLAY_DRAGON/EVIL/UNDEAD/TROLL/GIANT/ORC, SEE_INVIS | — |
| 81 | 'Aglarang' | Katana | 30 | 25 | 8d4 | +0/+0 | 40,000 | +5 DEX/SPEED | SUST_DEX | — |
| 82 | 'Ringil' | Long Sword | 20 | 120 | 4d5 | +22/+25 | 300,000 | +10 SPEED | SLAY_EVIL, BRAND_COLD, SLAY_UNDEAD, KILL_DEMON, SLAY_TROLL, FREE_ACT, RES_COLD/LITE, LITE, SEE_INVIS, SLOW_DIGEST, REGEN, RES_FEAR, BLESSED | FROST3 (40+0) |
| 83 | 'Anduril' | Long Sword | 20 | 40 | 3d5 | +10/+15/+10AC | 80,000 | +4 STR/DEX | SLAY_EVIL, BRAND_FIRE, SLAY_TROLL/ORC/UNDEAD, RES_FIRE/DISEN, SUST_STR/DEX, SEE_INVIS, FREE_ACT, BLESSED, RES_FEAR | FIRE2 (40+0) |
| 84 | 'Anguirel' | Long Sword | 20 | 30 | 2d5 | +8/+12 | 40,000 | +2 STR/CON, +SPEED | SLAY_EVIL, BRAND_POIS, SLAY_DEMON, FREE_ACT, RES_ELEC/LITE/DARK, LITE, SEE_INVIS, **AGGRAVATE** | — |
| 85 | 'Elvagil' | Long Sword | 20 | 8 | 2d5 | +2/+7 | 30,000 | +2 DEX/CHR, +STEALTH | SLAY_TROLL/ORC, FEATHER, SEE_INVIS | — |
| 86 | 'Forasgil' | Rapier | 15 | 8 | 1d6 | +12/+19 | 15,000 | — | SLAY_ANIMAL, BRAND_COLD, RES_COLD/LITE, LITE | — |
| 87 | 'Careth Asdriag' | Sabre | 15 | 8 | 1d7 | +6/+8 | 25,000 | +1 BLOWS | SLAY_DRAGON/ANIMAL/TROLL/GIANT/ORC | — |
| 88 | 'Sting' | Small Sword | 20 | 15 | 1d6 | +7/+8 | 100,000 | +2 STR/DEX/CON, +BLOWS/SPEED | SLAY_EVIL/UNDEAD/ORC/ANIMAL, FREE_ACT, RES_LITE, LITE, SEE_INVIS, RES_FEAR | — |
| 89 | 'Haradekket' | Scimitar | 20 | 15 | 2d5 | +9/+11 | 30,000 | +2 DEX, +BLOWS | SLAY_ANIMAL/EVIL/UNDEAD, SEE_INVIS | — |
| 90 | 'Dagmor' | Short Sword | 20 | 8 | 1d7 | +3/+7 | 15,000 | +BLOWS | BRAND_POIS, SLAY_ANIMAL, SLOW_DIGEST, REGEN | — |
| 91 | 'Doomcaller' | Blade of Chaos | 70 | 25 | 6d5 | +18/+28/−50AC | 200,000 | — | KILL_DRAGON, SLAY_ANIMAL/EVIL, BRAND_COLD, SLAY_TROLL/DEMON, FREE_ACT, RES_ACID/ELEC/FIRE/COLD/CHAOS, SEE_INVIS, TELEPATHY, **AGGRAVATE** | — |
| 135 | 'Narsil' (broken) | Broken Sword | 10 | 4 | 3d2 | +6/+10 | 2,000 | +2 STR/DEX, +BLOWS | SLAY_ORC/TROLL, RES_FIRE, BLESSED | — |
| 136 | of Eowyn | Bastard Sword | 30 | 100 | 4d4 | +12/+16 | 120,000 | +4 STR/CHR, +STEALTH/SPEED | SLAY_EVIL, KILL_UNDEAD, SLAY_GIANT/ANIMAL, RES_NETHR/FEAR/DARK/COLD | — |
| 133 | of Azaghal | Main Gauche | 18 | 30 | 2d5 | +12/+14 | 50,000 | — | KILL_DRAGON, IM_FIRE, RES_ACID, RES_FEAR | — |

### 8.11 Artifact Weapons — Polearms

| # | Name | Base | Depth | Rare | Dice | +Hit/+Dam | Cost | Stats | Flags | Activation |
|---|------|------|-------|------|------|-----------|------|-------|-------|------------|
| 92 | of Melkor | Spear | 65 | 45 | 4d6 | −12/+20 | 100,000 | +STEALTH/WIS | BRAND_POIS, RES_DARK/BLIND/LITE/NETHR, DRAIN_EXP, LIGHT_CURSE/HEAVY_CURSE, **AGGRAVATE** | — |
| 93 | of Theoden | Beaked Axe | 20 | 15 | 2d6 | +8/+10 | 40,000 | +3 WIS/CON | SLAY_DRAGON, TELEPATHY, SLOW_DIGEST | DRAIN_LIFE2 (40+0) |
| 94 | of Pain | Glaive | 30 | 25 | 9d6 | +0/+30 | 50,000 | — | RES_FEAR | — |
| 95 | 'Osondir' | Halberd | 20 | 8 | 3d5 | +6/+9 | 22,000 | +3 CHR | BRAND_FIRE, SLAY_UNDEAD/GIANT, RES_FIRE/SOUND, FEATHER, SEE_INVIS | — |
| 96 | 'Til-i-arc' | Pike | 20 | 15 | 2d5 | +10/+12/+10AC | 32,000 | +2 INT, SUST_INT | BRAND_COLD/FIRE, SLAY_DEMON/TROLL/GIANT, RES_FIRE/COLD, SLOW_DIGEST | — |
| 97 | 'Aeglos' | Spear | 15 | 45 | 3d6 | +15/+25/+5AC | 140,000 | +4 WIS/DEX | BRAND_COLD, SLAY_EVIL/TROLL/ORC, KILL_UNDEAD, FREE_ACT, SLOW_DIGEST, RES_COLD, RES_FEAR, BLESSED | FROST3 (35+0) |
| 98 | of Orome | Spear | 15 | 45 | 4d6 | +15/+15 | 60,000 | +4 INT/SPEED, +INFRA | BRAND_FIRE, SLAY_GIANT/ANIMAL, RES_FIRE/LITE, FEATHER, LITE, SEE_INVIS, BLESSED | STONE_TO_MUD (5+0) |
| 99 | 'Nimloth' | Spear | 15 | 12 | 1d6 | +11/+13 | 30,000 | +3 STEALTH/SPEED | BRAND_COLD, SLAY_UNDEAD, RES_COLD, SEE_INVIS, BLESSED | — |
| 100 | of Eorlingas | Lance | 20 | 23 | 3d8 | +13/+21 | 55,000 | +2 STR/DEX/SPEED | SLAY_EVIL/TROLL/ORC, SEE_INVIS, RES_FEAR | — |
| 101 | of Durin | Great Axe | 30 | 90 | 4d4 | +10/+20/+15AC | 150,000 | +3 STR/CON, +TUNNEL | BRAND_ACID/FIRE, KILL_DRAGON, SLAY_DEMON/TROLL/ORC, FREE_ACT, RES_CONFU/FEAR/ACID/FIRE/LITE/DARK/CHAOS | — |
| 102 | of Eonwe | Great Axe | 30 | 120 | 5d4 | +15/+18/+8AC | 200,000 | +2 all stats | SLAY_EVIL, BRAND_COLD, SLAY_UNDEAD, KILL_DEMON, SLAY_ORC, FREE_ACT, IM_COLD, RES_COLD, SEE_INVIS, BLESSED, RES_FEAR | MASS_BANISHMENT (1000+0) |
| 103 | of Balli Stonehand | Battle Axe | 30 | 15 | 3d8 | +8/+11/+5AC | 90,000 | +3 STR/CON/STEALTH | SLAY_DEMON/TROLL/ORC, FREE_ACT, RES_ACID/ELEC/FIRE/COLD/BLIND, FEATHER, SEE_INVIS, REGEN | — |
| 104 | 'Lotharang' | Battle Axe | 30 | 15 | 2d8 | +4/+3 | 21,000 | +1 STR/DEX | SLAY_TROLL/ORC | CURE_WOUNDS (3+3) |
| 105 | 'Mundwine' | Lochaber Axe | 30 | 8 | 3d8 | +12/+17 | 30,000 | — | SLAY_EVIL/ANIMAL/DEMON, RES_ACID/ELEC/FIRE/COLD | — |
| 106 | 'Barukkheled' | Broad Axe | 20 | 8 | 2d6 | +13/+19 | 50,000 | +3 CON | SLAY_EVIL/TROLL/GIANT/ORC, SEE_INVIS | — |
| 107 | of Wrath | Trident | 15 | 35 | 3d8 | +16/+18 | 90,000 | +2 STR/DEX | BRAND_POIS, SLAY_EVIL, KILL_UNDEAD, RES_LITE/DARK, SEE_INVIS, BLESSED | — |
| 108 | of Ulmo | Trident | 30 | 90 | 4d8 | +15/+19 | 120,000 | +4 DEX | SLAY_DRAGON/ANIMAL, FREE_ACT, HOLD_LIFE, IM_ACID, RES_ACID/NETHR, SEE_INVIS, SLOW_DIGEST, REGEN, BLESSED | TELE_AWAY (50+0) |
| 109 | 'Avavir' | Scythe | 40 | 8 | 5d3 | +8/+8/+10AC | 18,000 | +3 DEX/CHR | BRAND_COLD/FIRE, FREE_ACT, RES_FIRE/COLD/LITE, LITE, SEE_INVIS | WOR (200+0) |
| 110 | of Hurin | Beaked Axe | 20 | 15 | 3d6 | +12/+15 | 90,000 | +2 STR/CON | BRAND_ACID, KILL_DEMON, SLAY_DRAGON/TROLL, RES_ACID/DARK/FIRE, LITE | BERSERKER (80+80) |

### 8.12 Artifact Weapons — Blunt Weapons

| # | Name | Base | Depth | Rare | Dice | +Hit/+Dam | Cost | Stats | Flags | Activation |
|---|------|------|-------|------|------|-----------|------|-------|-------|------------|
| 111 | 'Grond' | Mighty Hammer | 100 | 1 | 9d9 | +5/+25/+10AC | 500,000 | — | KILL_DRAGON, SLAY_ANIMAL/EVIL, IMPACT, KILL_UNDEAD/DEMON, SLAY_TROLL/ORC, SEE_INVIS, TELEPATHY, **AGGRAVATE**, INSTA_ART | — |
| 112 | 'Totila' | Flail | 20 | 8 | 3d6 | +6/+8 | 55,000 | +2 STEALTH | SLAY_EVIL, BRAND_FIRE, RES_FIRE/CONFU | CONFUSE (15+0) |
| 113 | 'Thunderfist' | Two-Handed Flail | 45 | 38 | 4d6 | +5/+18 | 160,000 | +4 STR/CON | SLAY_ANIMAL, BRAND_FIRE/ELEC, SLAY_TROLL/ORC, RES_ELEC/FIRE/DARK, RES_FEAR | — |
| 114 | 'Bloodspike' | Morning Star | 20 | 30 | 2d6 | +8/+22 | 30,000 | +4 STR | BRAND_POIS, SLAY_ANIMAL/TROLL/ORC, RES_NEXUS, SEE_INVIS | — |
| 115 | 'Firestar' | Morning Star | 20 | 15 | 2d6 | +5/+7/+2AC | 35,000 | — | BRAND_FIRE, RES_FIRE | FIRE2 (20+0) |
| 116 | 'Taratol' | Mace | 20 | 15 | 3d4 | +12/+12 | 50,000 | — | KILL_DRAGON, BRAND_ELEC, IM_ELEC, RES_ELEC | HASTE1 (100+100) |
| 117 | of Aule | War Hammer | 40 | 75 | 9d3 | +19/+21/+5AC | 250,000 | +4 WIS, +TUNNEL | KILL_DRAGON, SLAY_EVIL, BRAND_ACID, SLAY_UNDEAD/DEMON, FREE_ACT, RES_ACID/ELEC/FIRE/COLD/NEXUS, SEE_INVIS | — |
| 118 | 'Nar-i-vagil' | Quarterstaff | 20 | 18 | 1d9 | +10/+20 | 70,000 | +3 INT | SLAY_ANIMAL, BRAND_FIRE, RES_FIRE | — |
| 119 | 'Eriril' | Quarterstaff | 20 | 18 | 1d9 | +3/+5 | 20,000 | +4 INT/WIS | SLAY_EVIL, RES_LITE, LITE, SEE_INVIS | IDENTIFY (10+0) |
| 120 | of Olorin | Quarterstaff | 30 | 105 | 2d9 | +10/+13 | 130,000 | +4 INT/WIS/CHR | KILL_DEMON, SLAY_EVIL, BRAND_FIRE, SLAY_TROLL/ORC, HOLD_LIFE, RES_FIRE/NETHR, SEE_INVIS | PROBE (20+0) |
| 121 | 'Deathwreaker' | Mace of Disruption | 80 | 38 | 7d8 | +18/+18 | 400,000 | +6 STR, +TUNNEL | SLAY_DRAGON/ANIMAL/EVIL, KILL_UNDEAD, BRAND_FIRE, IM_FIRE, RES_FIRE/DARK/CHAOS/DISEN, **AGGRAVATE** | — |
| 122 | 'Turmil' | Lucerne Hammer | 20 | 15 | 2d5 | +10/+6/+8AC | 30,000 | +4 WIS/INFRA | BRAND_COLD, SLAY_ORC, RES_COLD/LITE, LITE, REGEN | DRAIN_LIFE1 (40+0) |
| 123 | of Gothmog | Whip | 60 | 25 | 6d3 | +13/+15 | 25,000 | +INT/DEX/WIS | BRAND_FIRE, IM_FIRE, RES_ELEC/DARK, SLAY_ANIMAL, KILL_DRAGON, SLAY_TROLL/GIANT, LITE, **AGGRAVATE**, HEAVY_CURSE | FIRE3 (15+0) |
| 132 | of Fundin Bluecloak | Ball-and-Chain | 65 | 100 | 4d4 | +13/+17/+10AC | 60,000 | +4 STR/WIS/SPEED | SLAY_EVIL/UNDEAD, RES_FIRE/ELEC/NETHR, HOLD_LIFE, LITE | DISP_EVIL (100+100) |

### 8.13 Artifact Weapons — Missile Weapons

| # | Name | Base | Depth | Rare | +Hit/+Dam | Cost | Stats | Flags | Activation |
|---|------|------|-------|------|-----------|------|-------|-------|------------|
| 124 | 'Belthronding' | Long Bow | 40 | 20 | +20/+22 | 35,000 | +1 DEX/SPEED/STEALTH, +SHOTS | RES_DISEN | — |
| 125 | of Bard | Long Bow | 30 | 20 | +17/+19 | 20,000 | +2 DEX/SPEED, +MIGHT | FREE_ACT | — |
| 126 | 'Cubragol' | Light Crossbow | 50 | 25 | +10/+14 | 50,000 | +10 SPEED | RES_FIRE | FIREBRAND (999+0): gives bolts fire brand permanently |
| 127 | of Umbar | Heavy Crossbow | 60 | 20 | +18/+18, 4d1 | 35,000 | +2 STR/CON, +MIGHT | RES_LITE/DARK/BLIND/ELEC, **AGGRAVATE** | ARROW (20+20) |
| 128 | of Amrod | Short Bow | 25 | 10 | +12/+15 | 9,000 | +2 STR/CON, +MIGHT | RES_FIRE/ELEC/COLD, REGEN | — |
| 129 | of Amras | Short Bow | 25 | 10 | +12/+15 | 9,000 | +1 INT/WIS/DEX, +SHOTS/MIGHT/SPEED | RES_FIRE/ELEC/COLD, SLOW_DIGEST | — |

### 8.14 Artifact Digging Tools

| # | Name | Base | Depth | Rare | Dice | +Hit/+Dam | Cost | Stats | Flags | Activation |
|---|------|------|-------|------|------|-----------|------|-------|-------|------------|
| 130 | of Nain | Mattock | 60 | 8 | 2d8 | +12/+18 | 30,000 | +6 TUNNEL/INFRA/SEARCH/STR | SLAY_ORC/TROLL/GIANT/DRAGON, RES_DARK/DISEN | STONE_TO_MUD (2+0) |
| 131 | of Erebor | Dwarven Pick | 55 | 5 | 3d4 | +5/+20 | 30,000 | +5 STR/CON/TUNNEL, SUST_STR | SLAY_ORC/TROLL/DEMON, BRAND_ACID, RES_CHAOS/LITE/DARK, LITE | — |

### 8.15 MAngband-Exclusive Items (N:515–521)

These items are unique to MAngband (not in vanilla Angband) and appear as randomly-generated magic items, not named artifacts.

| N | Name | Type | Depth | Rarity | Stats/Flags | Notes |
|---|------|------|-------|--------|-------------|-------|
| 515 | Amulet of the Moon | Amulet | 40 | 40/8 + 60/4 | SEE_INVIS, INFRA, ACTIVATE | MAngband multiplayer utility amulet |
| 516 | Amulet of Terken | Amulet | 40 | 40/8 + 60/4 | SEE_INVIS, FREE_ACT, SEARCH | IGNORE all elements |
| 517 | Amulet of Speed | Amulet | 30 | 30/16 | SPEED (pval variable), HIDE_TYPE | Speed amulet; very common (rarity 16) |
| 518 | Orcish Shield | Shield | 40 | 40/4 | +STR, +CON, 5+5AC, 1d3 | IGNORE all elements; unusual: grants stats |
| 519 | Kolla (cloak) | Cloak | 65 | 65/4 | +STR/CON/DEX, SUST_STR/CON/DEX, 3+4AC | All-sustain cloak; deep and rare |
| 520 | Witan Boots | Boots | 60 | 60/1 | +STEALTH, 8+5AC | IGNORE all elements; heavy armor boots |
| 521 | Elven Gloves | Gloves | 20 | 20/16 | REGEN, 1AC | Very common; regeneration gloves |

---

## 9. Artifact Activation Reference

All artifact activations and their recharge times. Format: `ACTIVATION (base_recharge + dice_recharge)`.

| Activation | Effect | Example Artifact |
|------------|--------|-----------------|
| ILLUMINATION | Lights up room | Phial of Galadriel |
| MAGIC_MAP | Maps entire level | Star of Elendil |
| DETECT | Detects all creatures, objects, traps | Arkenstone, Holhenneth |
| PROT_EVIL | Protection from evil (duration: 1d225+225) | Amulet of Carlammas, Himring |
| DISP_EVIL | Dispels all evil creatures in sight | Amulet of Ingwe |
| CLAIRVOYANCE | Full level knowledge (map + detects) | Palantir |
| HASTE1 | Speed +10 for 20+1d20 turns | Boots of Feanor, Taratol |
| HASTE2 | Speed +10 for 75+1d75 turns | Ring of Tulkas |
| FIRE1/2/3 | Fire bolt/ball of various power | Narthanc, Anduril, Narya |
| FROST1/2/3/4/5 | Ice bolt/ball of various power | Nimthanc, Nenya |
| ELEC2 | Lightning ball | Vilya |
| HEAL1 | Heals 500 HP | Crown of Gondor, Elessar |
| HEAL2 | Heals 1000 HP | Soulkeeper |
| RESTORE_LIFE | Restores experience | Luthien, Evenstar |
| PHASE | Phase door (teleport ~10 sq) | Belegennon |
| TELEPORT | Teleport (~100 sq) | Colannon |
| BANISHMENT | Remove all monsters of one type from level | Celeborn |
| BERSERKER | Berserk mode | Shield of the Haradrim, Hurin |
| MASS_BANISHMENT | Remove all non-unique monsters from level | Eonwe |
| STARLIGHT | Starlight beams in all 8 directions | Shield of Gil-galad |
| STAR_BALL | Ball lightning in all 8 directions | Razorback, Mediator |
| RAGE_BLESS_RESIST | Berserk + Bless + Resist all | Bladeturner |
| DRAIN_LIFE1/2 | Drain life from target | Turmil, Theoden |
| CONFUSE | Confuse target monster | Totila |
| RESIST | Resist all elements (temporary) | Colluin |
| SLEEP | Sleep all non-unique monsters near player | Holcolleth |
| RECHARGE1 | Recharge a wand/staff | Thingol |
| FIRE1/FROST1/LIGHTNING_BOLT/ACID1 | Elemental bolt | Elemental gauntlet quartet |
| MISSILE | Magic missile | Cammithrim |
| MANA_BOLT | Mana bolt | Gauntlets of Eol |
| STONE_TO_MUD | Tunnels through rock | Orome, Nain |
| BIZZARE | One Ring effect — unique | The One Ring |
| WOR | Word of Recall | Avavir |
| IDENTIFY | Identify one item | Eriril |
| PROBE | Probe all monsters on level | Olorin |
| ARROW | Shoots arrows | Fingolfin, Umbar |
| FIREBRAND | Brands all ammo with fire permanently | Cubragol |
| TELE_AWAY | Teleport target away | Ulmo |
| CURE_WOUNDS | Cure wounds (minor heal) | Lotharang |
| REM_FEAR_POIS | Remove fear and poison | Dal-i-thalion |
| TRAP_DOOR_DEST | Destroy traps and doors | Caspanion |
| DISP_EVIL | Dispel evil | Fundin Bluecloak |
| BERSERKER | Berserk strength | Hurin, Haradrim |

---

## 10. Implementation Notes for IronHell

### Consumables Priority Tiers

**Tier 1 — Already Implemented (see POTIONS_COMPENDIUM, SCROLLS_COMPENDIUM):**
- All potions, scrolls, wands, staffs, rods

**Tier 2 — To Implement:**
- Mushrooms (tval 80, sval 0–19): beneficial mushrooms (5–8, 11–12, 18, 20) are straightforward; harmful ones require debuff system
- Normal food: Rations, Elvish Waybread, vegetables — nourishment system
- Light sources: Torch (fuel), Brass Lantern (refuelable), Dwarven Lantern (permanent), Feanorian Lamp (permanent 3-sq)
- Flask of Oil: dual use (refuel lantern or throw as fire)

**Tier 3 — Future:**
- Chests: container + loot generation + traps
- Iron spikes: door jamming mechanic
- Artifact items: unique named items for late-game content

### Hunger / Nutrition Thresholds (MAngband reference)

| State | pval threshold | Effect |
|-------|---------------|--------|
| Gorged | > 20000 | Slow, minor damage |
| Full | 10000–20000 | No hunger message |
| Satisfied | 2000–10000 | Normal |
| Hungry | 1000–2000 | "You are getting hungry." |
| Weak | 500–1000 | "You are getting weak from hunger!" |
| Fainting | 1–500 | Random fainting (paralysis) |
| Starving | 0 | HP drain each turn |

### Light Source Mechanics

- **Torches and lanterns** track fuel (`pval`). Fuel decrements each turn player is active.
- At 0 fuel: light source goes out; character is in darkness.
- Torches can combine: `merge_torch()` adds fuel up to 5000 cap.
- Oil flasks add 7500 to lantern fuel, capped at 15000.
- **Dwarven Lantern and Feanorian Lamp** use `pval 10000` as a marker but are **permanent** — fuel never decrements.
- **Illumination radius**: Torch = 1 sq, Brass/Dwarven Lantern = 2 sq, Feanorian Lamp = 3 sq.
