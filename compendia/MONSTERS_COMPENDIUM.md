# Monsters Compendium — MAngband (reference 1.5.3)

> Source files:
> - Monster data definitions: [reference-mangband-1_5_3/lib/edit/monster.txt](reference-mangband-1_5_3/lib/edit/monster.txt)
> - Monster flag parsing (blow methods, effects, race flags, spell flags): [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c)
> - Monster memory / lore display: [reference-mangband-1_5_3/src/server/monster1.c](reference-mangband-1_5_3/src/server/monster1.c)
> - Monster lifecycle (spawn, delete, compact): [reference-mangband-1_5_3/src/server/monster2.c](reference-mangband-1_5_3/src/server/monster2.c)
> - Physical melee attacks: [reference-mangband-1_5_3/src/server/melee1.c](reference-mangband-1_5_3/src/server/melee1.c)
> - Spell casting, breath attacks, AI: [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c)
> - Global constants: [reference-mangband-1_5_3/src/common/defines.h](reference-mangband-1_5_3/src/common/defines.h), [reference-mangband-1_5_3/src/server/mdefines.h](reference-mangband-1_5_3/src/server/mdefines.h)

---

## Table of Contents

1. [Monster Data Format (monster.txt)](#1-monster-data-format-monstertxt)
2. [Monster Symbols and Colors](#2-monster-symbols-and-colors)
3. [Speed System](#3-speed-system)
4. [Hit Points](#4-hit-points)
5. [Armor Class and Combat](#5-armor-class-and-combat)
6. [Melee Attacks — Methods and Effects](#6-melee-attacks--methods-and-effects)
7. [Spell System — Frequency, Types, and Damage](#7-spell-system--frequency-types-and-damage)
8. [Breath Attacks](#8-breath-attacks)
9. [Bolt and Ball Spells](#9-bolt-and-ball-spells)
10. [Status and Utility Spells](#10-status-and-utility-spells)
11. [Summoning Spells](#11-summoning-spells)
12. [Monster Race Flags — Flags1 (Identity and Drops)](#12-monster-race-flags--flags1-identity-and-drops)
13. [Monster Race Flags — Flags2 (Behavior and AI)](#13-monster-race-flags--flags2-behavior-and-ai)
14. [Monster Race Flags — Flags3 (Species, Immunities, Vulnerabilities)](#14-monster-race-flags--flags3-species-immunities-vulnerabilities)
15. [Drop System](#15-drop-system)
16. [AI and Smart Casting](#16-ai-and-smart-casting)
17. [Monster Movement and Pathfinding](#17-monster-movement-and-pathfinding)
18. [Depth, Rarity, and Experience](#18-depth-rarity-and-experience)
19. [Uniques](#19-uniques)
20. [Questors](#20-questors)
21. [Monster Categories by Symbol](#21-monster-categories-by-symbol)
22. [Representative Monster Roster by Depth Tier](#22-representative-monster-roster-by-depth-tier)
23. [IronHell Status Summary](#23-ironhell-status-summary)

---

## 1. Monster Data Format (monster.txt)

Each monster entry in `monster.txt` uses colon-delimited fields:

```
N:<serial>:<name>        # unique ID and display name
G:<symbol>:<color>       # map glyph and color
I:<speed>:<hp_dice>:<vision>:<ac>:<alertness>
W:<depth>:<rarity>:<unused>:<experience>
B:<method>:<effect>:<damage>     # up to 4 blow lines
S:1_IN_<X>               # spell frequency: casts 1 spell every X turns
S:<SPELL> | <SPELL> | ...        # spell list (multiple S: lines allowed)
F:<FLAG> | <FLAG> | ...          # flags (multiple F: lines allowed)
D:<description text>             # flavor text (multiple D: lines allowed)
```

**Field-by-field:**

| Line | Fields | Meaning |
|------|--------|---------|
| `I:` | speed : hp_dice : vision : ac : alertness | Speed (110 normal), HP as `NdM`, vision in tens-of-feet, armor class, alertness 0-255 (0=ever vigilant, 255=prefers to ignore) |
| `W:` | depth : rarity : unused : experience | Native dungeon depth, generation rarity, always 0, XP for kill |
| `B:` | method : effect : damage | Physical attack — method of delivery, damage effect, `XdY` dice (see §6) |
| `S:` | frequency or spell list | First S: line is `1_IN_X`; subsequent S: lines list spell flags |
| `F:` | pipe-delimited flags | Race flags from flags1-3, behavior flags, immunities, drops |

---

## 2. Monster Symbols and Colors

Every monster has a single ASCII character (`G:` symbol) and color. Symbols loosely group monsters by type.

### Symbol → Monster Type Map

| Symbol | Type | Examples |
|--------|------|----------|
| `@` | Player | (entry 0, special) |
| `a` | Ant | Soldier ant, Giant black ant, Giant silver ant |
| `A` | Angel | Angel |
| `b` | Bat | Fruit bat, Giant brown bat, Doombat |
| `B` | Bird | Nighthawk, Roc |
| `c` | Centipede | Giant yellow centipede, Stegocentipede |
| `C` | Canine | Scruffy little dog, Warg, Wolf chieftain |
| `d` | Young/Baby dragon | Young white dragon, Baby multi-hued dragon |
| `D` | Ancient/Mature dragon | Great ice wyrm, Smaug, Glaurung, Dracolich |
| `e` | Eye | Floating eye, Radiation eye, Beholder hive-mother |
| `E` | Elemental | Air spirit, Water elemental, Quaker |
| `f` | Feline | Scrawny cat, Wild cat, Tiger, Displacer beast |
| `F` | Dragon fly | Giant white dragon fly, Giant bronze dragon fly |
| `g` | Golem | Flesh golem, Pukelman, Bone golem, Bronze golem |
| `G` | Ghost | Poltergeist, Green glutton ghost, Dreadmaster |
| `h` | Hobbit/Elf/Dwarf humanoid | Smeagol, Mind flayer, Dark elven druid |
| `H` | Hybrid | White harpy, Griffon, Gorgimaera |
| `i` | Icky thing | White icky thing, Blue icky thing |
| `I` | Insect | Giant flea, Neekerbreeker, Giant firefly |
| `j` | Jelly | White jelly, Green jelly, Ochre jelly |
| `J` | Snake | Large brown snake, Rattlesnake, Copperhead snake |
| `k` | Kobold | Small kobold, Kobold archer, Kobold shaman |
| `K` | Beetle | Killer brown beetle, Killer fire beetle |
| `l` | Louse | Giant white louse, Giant black louse |
| `L` | Lich | Lich, Master lich, Dread lich |
| `m` | Mold | Grey mold, Brown mold, Death mold |
| `M` | Multi-headed | 5-headed hydra, 11-headed hydra |
| `n` | Naga | Black naga, Green naga, Spirit naga |
| `N` | (unused in base set) | |
| `o` | Orc | Snaga, Cave orc, Uruk-hai |
| `O` | Ogre | Ogre, Cave ogre, Ogre mage |
| `p` | Human(oid) person | Novice warrior, Mage, Black knight, Sauron |
| `P` | Giant/Titan | Hill giant, Frost giant, Cyclops, Morgoth |
| `q` | Quadruped | Mumak |
| `r` | Rodent | Giant white mouse, Giant white rat, Wererat |
| `R` | Reptile/Amphibian | Rock lizard, Cave lizard, Giant green frog, Greater basilisk |
| `s` | Skeleton | Skeleton kobold, Skeleton human |
| `S` | Spider | Cave spider, Mirkwood spider, Shelob, Aranea |
| `t` | Townsperson | Filthy street urchin, Mean-looking mercenary |
| `T` | Troll | Stone troll, Troll priest, Troll chieftain |
| `u` | Minor demon | Manes, Homunculus |
| `U` | Major demon | Marilith, Lesser Balrog, Balrog of Moria, Gothmog |
| `v` | Vortex | Shardstorm, Storm of Unmagic |
| `V` | Vampire | Vampire, Elder vampire |
| `w` | Worm mass | White worm mass, Red worm mass, Wereworm |
| `W` | Wight/Wraith | White wraith, Ringwraith (Nazgûl) |
| `y` | Yeek | Blue yeek, Boldor King of the Yeeks |
| `z` | Zombie/Mummy | Zombified human, Ghoul, Ghast, Greater mummy |
| `Z` | Hound (zephyr) | Light hound, Nexus hound, Multi-hued hound |
| `$` | Mimic (coin) | Creeping copper coins, Creeping adamantite coins |
| `,` | Mushroom patch | Grey mushroom patch, Magic mushroom patch |
| `.` | Lurker | Lurker |

### Colors

16 MAngband colors: `d`=Black, `D`=Dark Gray, `w`=White, `W`=Light Gray, `s`=Gray, `o`=Orange, `r`=Red, `R`=Light Red, `g`=Green, `G`=Light Green, `b`=Blue, `B`=Light Blue, `u`=Brown, `U`=Light Brown, `v`=Violet, `y`=Yellow.

Special display flags:
- **ATTR_CLEAR**: monster uses the color of the tile beneath it (camouflage).
- **ATTR_MULTI**: monster cycles through colors each turn (multi-hued).
- **CHAR_CLEAR**: monster uses the *character* of the tile beneath it (invisibility mimic).
- **CHAR_MULTI**: monster uses the character of objects (object mimic).

---

## 3. Speed System

Speed is stored in the `I:` line. **110 is normal speed** (same as a level 0 player with no bonuses).

| Speed | Relative | Examples |
|-------|----------|----------|
| 100 | Slow (−10) | Worm masses, Large snakes |
| 110 | Normal (±0) | Most humanoids, orcs, kobolds |
| 120 | Fast (+10) | Bats, spiders, unique warriors, Ancient dragons |
| 130 | Very Fast (+20) | Smeagol, Eol, Sauron, Balrog of Moria |
| 140 | Extremely Fast (+30) | Morgoth (140), Death mold (140) |

Every 10 speed points roughly doubles the creature's turn frequency relative to normal. A creature at speed 120 gets approximately twice as many turns as one at 110 over the same real-time period.

---

## 4. Hit Points

Hit points are specified as `NdM` dice in the `I:` line.

- **Normal monsters** roll `N` dice of `M` sides, giving HP in the range `[N .. N×M]` with average `N×(M+1)/2`.
- **FORCE_MAXHP** flag: monster always gets maximum HP (`N×M`). Used for uniques and bosses.

### Examples

| Monster | HP Dice | Average HP | Max HP | FORCE_MAXHP? |
|---------|---------|-----------|--------|---------------|
| Grey mold | 1d2 | 1.5 | 2 | No |
| Novice warrior | 9d4 | 22.5 | 36 | No |
| Smeagol | 20d20 | 210 | 400 | Yes → always 400 |
| Sauron | 105d100 | 5302 | **10,500** | Yes |
| Morgoth | 200d100 | 10,100 | **20,000** | Yes |

---

## 5. Armor Class and Combat

### Monster AC

Armor class is in the `I:` field. Higher AC makes the monster harder to hit in melee. Monster AC ranges from **1** (mushroom patches, weak jellies) to **170** (bone golem, bronze golem).

### Monster Attack Roll (melee1.c)

When a monster attacks a player:
```
power = base_power_for_effect  (see table below)
attack_quality = power + (monster_level * 3)
hit = attack_quality > (player_AC * 3 / 4)
```
5% auto-miss, 5% auto-hit on every swing (d100 < 10 check).

### Attack Power by Effect

| Effect | Power | Notes |
|--------|-------|-------|
| HURT | 60 | Standard physical damage |
| SHATTER | 60 | Physical + earthquake |
| ACID, FIRE, COLD, ELEC | 0–10 | Elemental always hits or nearly always |
| POISON | 5 | Low power, relies on secondary effect |
| PARALYZE | 2 | Very low power — hard to land vs high AC |
| BLIND, CONFUSE, TERRIFY | 2–10 | Status attacks |
| EAT_GOLD, EAT_ITEM, EAT_FOOD, EAT_LITE | 5 | Thievery/consumption |
| UN_BONUS, UN_POWER | 15–20 | Disenchant/drain charges |
| LOSE_STR..LOSE_ALL | 0–2 | Stat drain bypasses AC easily |
| EXP_10..EXP_80 | 5 | Experience drain |
| HALLU | 10 | Hallucination |

### Critical Blows (melee1.c)

Monster melee crits determine **cuts** and **stuns** on the player:
- Damage must be ≥ 95% of maximum possible (`dice × sides`).
- Damage must also be ≥ 20 (or pass a d100 < damage check).
- Perfect damage (exact max) gives +1 crit level.
- Damage ≥ 20 may "super-charge" with 2% chance per additional level.
- Crit levels produce increasing stun/cut durations.

---

## 6. Melee Attacks — Methods and Effects

Each monster can have up to **4 physical attacks** (`B:` lines).

### Attack Methods

| Method | Description | Causes Cut? | Causes Stun? |
|--------|-------------|:-----------:|:------------:|
| HIT | Standard melee hit | Yes | Yes |
| TOUCH | Touch attack | No | No |
| PUNCH | Punching | No | Yes |
| KICK | Kicking | No | Yes |
| CLAW | Clawing | Yes | No |
| BITE | Biting | Yes | No |
| STING | Stinging | No | No |
| BUTT | Head-butting | No | Yes |
| CRUSH | Crushing/constricting | No | Yes |
| ENGULF | Engulfing | No | No |
| CRAWL | Crawling on player | No | No |
| DROOL | Drooling | No | No |
| SPIT | Spitting | No | No |
| GAZE | Gaze attack | No | No |
| WAIL | Wailing | No | No |
| SPORE | Spore release | No | No |
| BEG | Begging for money | No | No |
| INSULT | Random insult text | No | No |
| MOAN | Moaning (e.g. Farmer Maggot) | No | No |

### Attack Effects

| Effect | Damage Type | Secondary Effect |
|--------|-------------|------------------|
| HURT | Physical | Raw HP damage |
| POISON | Physical + poison | Adds poisoned status |
| UN_BONUS | Physical | Disenchants a random equipped item (removes +hit/+dam/+ac) |
| UN_POWER | Physical | Drains charges from a random wand/staff/rod |
| EAT_GOLD | Physical | Steals gold, monster blinks away |
| EAT_ITEM | Physical | Steals an inventory item, monster blinks away |
| EAT_FOOD | Physical | Destroys food from inventory |
| EAT_LITE | Physical | Reduces light source fuel |
| ACID | Acid | Acid damage, may destroy armor/items |
| ELEC | Lightning | Lightning damage, may destroy rings/wands |
| FIRE | Fire | Fire damage, may destroy scrolls/staves/books |
| COLD | Cold | Cold damage, may destroy potions |
| BLIND | Physical | Blinds the player |
| CONFUSE | Physical | Confuses the player |
| TERRIFY | Physical | Frightens the player |
| PARALYZE | Physical | Paralyzes the player (extremely dangerous) |
| LOSE_STR | Physical | Drains Strength |
| LOSE_INT | Physical | Drains Intelligence |
| LOSE_WIS | Physical | Drains Wisdom |
| LOSE_DEX | Physical | Drains Dexterity |
| LOSE_CON | Physical | Drains Constitution |
| LOSE_CHR | Physical | Drains Charisma |
| LOSE_ALL | Physical | Drains all six stats |
| SHATTER | Physical | HP damage + earthquake effect (destroys walls) |
| EXP_10 | Physical | Drains 10d6 + (experience / 100) * MON_DRAIN_LIFE XP |
| EXP_20 | Physical | Drains 20d6 + (experience / 100) * MON_DRAIN_LIFE XP |
| EXP_40 | Physical | Drains 40d6 + (experience / 100) * MON_DRAIN_LIFE XP |
| EXP_80 | Physical | Drains 80d6 + (experience / 100) * MON_DRAIN_LIFE XP |
| HALLU | Physical | Causes hallucination |

---

## 7. Spell System — Frequency, Types, and Damage

### Spell Frequency

The first `S:` line declares `1_IN_X` — on each monster turn, there is a `100/X`% chance it attempts to cast a spell instead of moving/meleeing.

| Frequency | Cast Chance | Typical Users |
|-----------|-------------|---------------|
| 1_IN_2 | 50% | Sauron, Patriarch, Beholder hive-mother |
| 1_IN_3 | 33% | Morgoth, Mage, Ancient multi-hued dragon |
| 1_IN_4 | 25% | Most casters: Vampire, Dark elven druid |
| 1_IN_5 | 20% | Hound breathers, Hydras |
| 1_IN_6 | 17% | Lesser casters |
| 1_IN_8 | 12.5% | Occasional casters |
| 1_IN_9 | 11% | Rare casting |
| 1_IN_10..15 | 7-10% | Very rare casting |

When a monster casts, it picks one spell at random from its entire spell list. Confused monsters cannot cast.

### Spell Categories

Spells are grouped into three 32-bit flag words:

- **flags4 (RF4)**: Innate abilities — shrieks, arrows, breath weapons, boulders
- **flags5 (RF5)**: Magical spells — balls, bolts, curses, status effects
- **flags6 (RF6)**: Special abilities — haste, heal, teleport, summons

---

## 8. Breath Attacks

Breath weapons deal damage based on the monster's **current HP**, divided by a fraction, capped at a maximum. The radius is 2 for normal monsters, 3 for POWERFUL monsters.

### Breath Damage Table

| Breath | Flag | Damage Formula | Cap | Element |
|--------|------|---------------|-----|---------|
| BR_ACID | RF4 | hp / 3 | 1600 | Acid |
| BR_ELEC | RF4 | hp / 3 | 1600 | Lightning |
| BR_FIRE | RF4 | hp / 3 | 1600 | Fire |
| BR_COLD | RF4 | hp / 3 | 1600 | Frost |
| BR_POIS | RF4 | hp / 3 | 800 | Poison |
| BR_NETH | RF4 | hp / 6 | 550 | Nether |
| BR_LITE | RF4 | hp / 6 | 400 | Light |
| BR_DARK | RF4 | hp / 6 | 400 | Darkness |
| BR_CONF | RF4 | hp / 6 | 400 | Confusion |
| BR_SOUN | RF4 | hp / 6 | 500 | Sound |
| BR_CHAO | RF4 | hp / 6 | 500 | Chaos |
| BR_DISE | RF4 | hp / 6 | 500 | Disenchantment |
| BR_NEXU | RF4 | hp / 6 | 400 | Nexus |
| BR_TIME | RF4 | hp / 3 | 150 | Time |
| BR_INER | RF4 | hp / 6 | 200 | Inertia |
| BR_GRAV | RF4 | hp / 3 | 200 | Gravity |
| BR_SHAR | RF4 | hp / 6 | 500 | Shards |
| BR_PLAS | RF4 | hp / 6 | 150 | Plasma |
| BR_WALL | RF4 | hp / 6 | 200 | Force |

**Key insight:** The four basic elements (acid, elec, fire, cold) can do massive damage (up to 1600) from high-HP dragons, while exotic breaths have lower caps but often bypass common resistances.

---

## 9. Bolt and Ball Spells

### Ball Spells (area of effect, radius 2 for normal, 3 for POWERFUL)

| Spell | Flag | Damage Formula | Element |
|-------|------|---------------|---------|
| BA_ACID | RF5 | 1d(rlev×3) + 15 | Acid |
| BA_ELEC | RF5 | 1d(rlev×3/2) + 8 | Lightning |
| BA_FIRE | RF5 | 1d(rlev×7/2) + 10 | Fire |
| BA_COLD | RF5 | 1d(rlev×3/2) + 10 | Frost |
| BA_POIS | RF5 | 12d2 (= 12–24) | Poison |
| BA_NETH | RF5 | 50 + 10d10 + rlev | Nether |
| BA_WATE | RF5 | 1d(rlev×5/2) + 50 | Water |
| BA_MANA | RF5 | rlev×5 + 10d10 | Mana (unresistable) |
| BA_DARK | RF5 | rlev×5 + 10d10 | Darkness |

### Bolt Spells (single target, stopped by first monster in path)

| Spell | Flag | Damage Formula | Element |
|-------|------|---------------|---------|
| BO_ACID | RF5 | 7d8 + rlev/3 | Acid |
| BO_ELEC | RF5 | 4d8 + rlev/3 | Lightning |
| BO_FIRE | RF5 | 9d8 + rlev/3 | Fire |
| BO_COLD | RF5 | 6d8 + rlev/3 | Frost |
| BO_NETH | RF5 | 30 + 5d5 + rlev×3/2 | Nether |
| BO_WATE | RF5 | 10d10 + rlev | Water |
| BO_MANA | RF5 | 1d(rlev×7/2) + 50 | Mana |
| BO_PLAS | RF5 | 10 + 8d7 + rlev | Plasma |
| BO_ICEE | RF5 | 6d6 + rlev | Ice |
| MISSILE | RF5 | 2d6 + rlev/3 | Magic missile |

### Ranged Physical Attacks

| Spell | Flag | Damage | Notes |
|-------|------|--------|-------|
| ARROW_1 | RF4 | 1d6 | Light arrow |
| ARROW_2 | RF4 | 3d6 | Arrow volley |
| ARROW_3 | RF4 | 5d6 | Missile |
| ARROW_4 | RF4 | 7d6 | Heavy missiles |
| BOULDER | RF4 | (1 + rlev/7)d12 | Thrown boulder |
| SHRIEK | RF4 | 0 | Aggravates all monsters on level |

`rlev` = max(monster_native_depth, 1), i.e. the monster's effective level.

---

## 10. Status and Utility Spells

### Offensive Status Spells (all require line of sight, "direct")

| Spell | Flag | Effect | Resisted By |
|-------|------|--------|-------------|
| SCARE | RF5 | Fear for 4+d4 turns | resist_fear OR saving throw |
| BLIND | RF5 | Blindness for 12+d4 turns | resist_blind OR saving throw |
| CONF | RF5 | Confusion for 4+d4 turns | resist_conf OR saving throw |
| SLOW | RF5 | Slowed for 4+d4 turns | free_act OR saving throw |
| HOLD | RF5 | Paralysis for 4+d4 turns | free_act OR saving throw |
| FORGET | RF6 | Lose all item/map knowledge | Saving throw |
| DARKNESS | RF6 | Unlights area (radius 3) | — |
| TRAPS | RF6 | Creates traps around player | — |

### Curse Spells (damage + status, resisted by saving throw)

| Spell | Flag | Damage | Extra |
|-------|------|--------|-------|
| CAUSE_1 | RF5 | 3d8 | — |
| CAUSE_2 | RF5 | 8d8 | — |
| CAUSE_3 | RF5 | 10d15 | — |
| CAUSE_4 | RF5 | 15d15 | + bleeding (10d10 cut) |

### Psionic Attacks

| Spell | Flag | Damage | Extra |
|-------|------|--------|-------|
| MIND_BLAST | RF5 | 8d8 | + confusion (if not resist_conf) |
| BRAIN_SMASH | RF5 | 12d15 | + blind + confuse + paralyze + slow |
| DRAIN_MANA | RF5 | 1d(rlev/2)+1 mana drained | Monster heals 6× mana drained |

### Self-Buff Spells

| Spell | Flag | Effect |
|-------|------|--------|
| HASTE | RF6 | +10 speed (up to base+10 quickly, then +2 up to base+20) |
| HEAL | RF6 | Recovers rlev×6 HP, cancels fear |
| BLINK | RF6 | Short-range teleport (range 10) |
| TPORT | RF6 | Long-range teleport (range ~50) |

### Teleport Spells (target: player)

| Spell | Flag | Effect | Resisted By |
|-------|------|--------|-------------|
| TELE_TO | RF6 | Teleports player adjacent to monster | — |
| TELE_AWAY | RF6 | Teleports player away (range 100) | — |
| TELE_LEVEL | RF6 | Sends player up or down one dungeon level | resist_nexus OR saving throw |

---

## 11. Summoning Spells

| Spell | Flag | Count | What Is Summoned |
|-------|------|-------|------------------|
| S_KIN | RF6 | 6 | Monsters with same symbol as caster |
| S_MONSTER | RF6 | 1 | Any single monster |
| S_MONSTERS | RF6 | 8 | Any monsters |
| S_ANIMAL | RF6 | 6 | Animals |
| S_SPIDER | RF6 | 6 | Spiders |
| S_HOUND | RF6 | 6 | Hounds |
| S_HYDRA | RF6 | 6 | Hydras |
| S_ANGEL | RF6 | 1 | Angel |
| S_DEMON | RF6 | 1 | Demon |
| S_UNDEAD | RF6 | 1 | Undead |
| S_DRAGON | RF6 | 1 | Dragon |
| S_HI_UNDEAD | RF6 | 8 | Greater Undead |
| S_HI_DRAGON | RF6 | 8 | Ancient Dragons |
| S_HI_DEMON | RF6 | 8 | Greater Demons |
| S_WRAITH | RF6 | 8 | Ring Wraiths (Nazgûl) |
| S_UNIQUE | RF6 | 8 | Unique monsters |

Summoned monsters appear at the monster's level (`rlev`). The summon target squares are adjacent to the player's position.

---

## 12. Monster Race Flags — Flags1 (Identity and Drops)

### Identity Flags

| Flag | Effect |
|------|--------|
| UNIQUE | Only one can exist at a time; once killed, never re-generated (per player in MAngband). Uniques have a respawn timer of up to `COME_BACK_TIME_MAX` (600 turns) for multiplayer. |
| QUESTOR | Must be killed to complete a quest. Sauron (quest 1, depth 99) and Morgoth (quest 2, depth 100). |
| MALE | Pronoun "he/his" |
| FEMALE | Pronoun "she/her" |

### Display Flags

| Flag | Effect |
|------|--------|
| CHAR_CLEAR | Uses floor/terrain character as its symbol |
| CHAR_MULTI | Uses object characters (mimic) |
| ATTR_CLEAR | Uses floor/terrain color |
| ATTR_MULTI | Cycles through all 16 colors each turn |

### Generation Flags

| Flag | Effect |
|------|--------|
| FORCE_DEPTH | Never generated above native depth |
| FORCE_MAXHP | Always maximum HP (no dice roll) |
| FORCE_SLEEP | Starts asleep with a `sleepDepth` counter. Woken gradually by player noise (MAngband csleep parity). `sleepDepth = flags.sleepRating ?? (level × 3 + 50)`. A sleeping `forceSleep` monster that was never woken is eligible for Rogue backstab bonus. |
| FORCE_EXTRA | Reserved |

> **sleepRating** (optional in `monster_compendium.json` `flags` object): explicit override for the spawn sleep depth. If omitted, depth is calculated as `level × 3 + 50`. Higher values mean the monster is harder to wake.

### Group Flags

| Flag | Effect |
|------|--------|
| FRIEND | Appears with one similar companion |
| FRIENDS | Appears with a group of similar companions |
| ESCORT | Appears with an escort of related monsters |
| ESCORTS | Appears with many escorts |

### Drop Flags

| Flag | Effect |
|------|--------|
| ONLY_GOLD | Drops only gold, no items |
| ONLY_ITEM | Drops only items, no gold |
| DROP_60 | 60% chance of a single drop |
| DROP_90 | 90% chance of a single drop |
| DROP_1D2 | Drops 1d2 treasures |
| DROP_2D2 | Drops 2d2 treasures |
| DROP_3D2 | Drops 3d2 treasures |
| DROP_4D2 | Drops 4d2 treasures |
| DROP_GOOD | Drops are guaranteed "good" quality |
| DROP_GREAT | Drops are guaranteed "great" quality (artifact-possible) |
| DROP_USEFUL | Drops are biased toward useful items |
| DROP_CHOSEN | Drops use special selection tables (Morgoth only) |

Drop count flags stack: a monster with `DROP_90 | DROP_2D2` can drop up to 4+1 = 5 items. `ONLY_GOLD` and `ONLY_ITEM` restrict the type. Without either, drops can be gold or items.

---

## 13. Monster Race Flags — Flags2 (Behavior and AI)

### Intelligence

| Flag | Effect |
|------|--------|
| STUPID | Monster is too stupid to learn player resistances; halved smart-casting probability |
| SMART | Monster learns/remembers player resistances; casts intelligently; when low HP (<10%), prefers escape/heal spells |

### Mind Type

| Flag | Effect | Detect? |
|------|--------|---------|
| EMPTY_MIND | No mind at all (golems, molds, jellies) | Not detected by Detect Monsters |
| WEIRD_MIND | Alien mind (animals, insects) | Detected intermittently |
| *(neither)* | Normal mind | Always detected |

### Physical Properties

| Flag | Effect |
|------|--------|
| INVISIBLE | Requires See Invisible to perceive |
| COLD_BLOOD | Cannot be detected by infravision |
| MULTIPLY | Breeds/splits to produce copies (contributes to `num_repro` count) |
| REGENERATE | Recovers HP faster than normal |
| POWERFUL | Breaths have radius 3 instead of 2; stronger tunnel/bash |

### Movement Abilities

| Flag | Effect |
|------|--------|
| OPEN_DOOR | Can open closed doors |
| BASH_DOOR | Can bash down doors |
| PASS_WALL | Can move through walls (ghosts, ethereal creatures) |
| KILL_WALL | Destroys walls by moving through them (Morgoth, golems) |
| MOVE_BODY | Can push other monsters out of the way |
| KILL_BODY | Can destroy weaker monsters in its path |
| TAKE_ITEM | Can pick up items from the floor |
| KILL_ITEM | Destroys items on the floor |
| NEVER_BLOW | Never makes melee attacks |
| NEVER_MOVE | Stationary (mushrooms, molds, eyes) |

### Randomness

| Flag | Effect |
|------|--------|
| RAND_25 | 25% chance of moving randomly each turn |
| RAND_50 | 50% chance of moving randomly each turn |
| RAND_25 + RAND_50 | 75% random movement |
| WANDERER | Wanders the level (town NPCs) |

### Brain Flags (BRAIN_2..8)

Used internally for AI complexity levels; higher BRAIN flags indicate more sophisticated behavior patterns.

---

## 14. Monster Race Flags — Flags3 (Species, Immunities, Vulnerabilities)

### Species Types

| Flag | Slain By | Description |
|------|----------|-------------|
| ORC | Slay Orc (×3) | Orcs: Snaga, Cave orc, Uruk-hai |
| TROLL | Slay Troll (×3) | Trolls: Stone troll, Troll priest |
| GIANT | Slay Giant (×3) | Giants and ogres: Hill giant, Cyclops |
| DRAGON | Slay Dragon (×3), Kill Dragon (×5) | All dragons, hydras, wyrms |
| DEMON | Slay Demon (×3), Kill Demon (×5) | All demons, balrogs |
| UNDEAD | Slay Undead (×3), Kill Undead (×5) | Skeletons, zombies, ghosts, vampires, liches, wraiths |
| EVIL | Slay Evil (×2) | Most non-natural monsters |
| ANIMAL | Slay Animal (×2) | Natural creatures: cats, dogs, bats, spiders, beetles |

### Immunities (take 0 or 1/9 damage)

| Flag | Element |
|------|---------|
| IM_ACID | Acid |
| IM_ELEC | Lightning |
| IM_FIRE | Fire |
| IM_COLD | Cold/Frost |
| IM_POIS | Poison |
| IM_WATER | Water |

### Resistances (reduced but not zero damage)

| Flag | Element |
|------|---------|
| RES_NETH | Nether |
| RES_PLAS | Plasma |
| RES_NEXUS | Nexus |
| RES_DISE | Disenchantment |

### Vulnerabilities

| Flag | Effect |
|------|--------|
| HURT_LITE | Takes extra damage from light-based attacks; vulnerable to Scare Monster scroll |
| HURT_ROCK | Takes extra damage from Stone-to-Mud and rock-based attacks |
| HURT_FIRE | Takes extra damage from fire (rare on monsters) |
| HURT_COLD | Takes extra damage from cold (rare on monsters) |

### Status Immunities

| Flag | Effect |
|------|--------|
| NO_FEAR | Cannot be frightened |
| NO_STUN | Cannot be stunned |
| NO_CONF | Cannot be confused |
| NO_SLEEP | Cannot be put to sleep **by spells** (Sleep Monster, etc.). Does NOT prevent spawn sleep from `FORCE_SLEEP`. Ancient dragons typically have both flags: they spawn asleep but cannot be re-slept once awoken. |

> **Note:** `FORCE_SLEEP` and `NO_SLEEP` are fully independent. A monster can have both: it spawns asleep (via `forceSleep`) but resists any attempt to magically sleep it again (`noSleep`).

---

## 15. Drop System

When a monster is killed, the game determines drops from its flags:

1. **Count** the number of drops: combine DROP_60 (0-1), DROP_90 (0-1), DROP_1D2 (1-2), DROP_2D2 (2-4), DROP_3D2 (3-6), DROP_4D2 (4-8). Multiple flags stack additively.
2. **Type** each drop: if `ONLY_GOLD`, all drops are gold piles; if `ONLY_ITEM`, all are objects; otherwise random.
3. **Quality**: `DROP_GOOD` guarantees "good" quality (higher enchantment bonus). `DROP_GREAT` guarantees "great" quality (higher chance of ego items and artifacts). `DROP_CHOSEN` uses a special table (only Morgoth).
4. **Object level**: drops are generated at the monster's native depth. Items from deeper monsters are more powerful.

### Drop Examples

| Monster | Depth | Drop Flags | Typical Drops |
|---------|-------|------------|---------------|
| Novice warrior | 2 | DROP_60 | 0-1 random item or gold |
| Smeagol | 3 | DROP_90, DROP_GOOD, DROP_GREAT | 1 item, guaranteed great quality |
| Ancient multi-hued dragon | 43 | ONLY_ITEM, DROP_2D2, DROP_3D2, DROP_4D2 | 9-14 items at depth 43 |
| Sauron | 99 | ONLY_ITEM, DROP_2D2..4D2, DROP_GOOD, DROP_GREAT | 9-14 great items at depth 99 |
| Morgoth | 100 | ONLY_ITEM, DROP_1D2..4D2, DROP_GOOD, DROP_GREAT, DROP_CHOSEN | 10-16 chosen-quality items |

---

## 16. AI and Smart Casting

### Basic AI Flow (melee2.c)

Each monster turn:
1. If monster can see the player and is within `MAX_RANGE` and has a projectable path:
   - Roll against spell frequency (`1_IN_X`). If success → attempt spell.
2. Otherwise: move toward player (or wander if RAND flags).

### Smart Spell Selection

When `smart_learn` or `smart_cheat` is enabled:

- **SMART monsters** track which resistances the player has (acid, fire, cold, etc.) via a 32-bit `smart` field per monster instance.
- On each spell attempt, the monster calls `remove_bad_spells()` which probabilistically removes spells the player is resistant/immune to:
  - **Immune**: 100% chance to remove that element's spells (SMART monsters), 50% for non-SMART.
  - **Resist + Oppose**: 80% removal / 40%.
  - **Resist OR Oppose**: 30% removal / 15%.
- With 1% chance per turn, a monster "forgets" the player's resistances (resets `smart` to 0).

### Desperate Casting

SMART monsters at < 10% HP have a 50% chance to restrict their spell list to only "intelligent" spells:
- Teleport, Heal, Haste, Blink, Summon spells (RF4_INT_MASK, RF5_INT_MASK, RF6_INT_MASK).
- If no intelligent spells remain, the monster simply doesn't cast.

### Protection from Evil

When a player has Protection from Evil active, EVIL monsters' melee attacks are repelled if:
```
player_level >= monster_level AND (d100 + player_level) > 50
```

---

## 17. Monster Movement and Pathfinding

### Movement Priority

1. **NEVER_MOVE**: stationary, never moves (mushrooms, molds, many eyes).
2. **Spell attempt**: if in range and has spells, may cast instead of moving.
3. **Melee**: if adjacent to player, attack.
4. **Chase**: move toward player using simple 8-directional pathfinding.
5. **Random**: RAND_25/RAND_50 flags cause random movement some percentage of turns.

### Door Interaction

- **OPEN_DOOR**: monster opens closed doors normally (1 turn).
- **BASH_DOOR**: monster smashes doors (can destroy them). Strength check involved.
- Without either flag, doors completely block the monster.

### Wall Interaction

- **PASS_WALL**: phase through walls (ghosts, ethereal dragons). No wall destruction.
- **KILL_WALL**: destroy walls while moving through them (Morgoth, some golems). Leaves rubble.

### Terrain Navigation

- **MOVE_BODY**: can push weaker monsters out of the way to reach the player.
- **KILL_BODY**: can destroy weaker monsters blocking the path.

---

## 18. Depth, Rarity, and Experience

### Depth (W: line, first field)

The native dungeon level where the monster normally appears. Monsters can appear up to 5 levels above their native depth (out-of-depth generation). The `FORCE_DEPTH` flag prevents out-of-depth appearance.

| Depth Band | Typical Monsters |
|------------|-----------------|
| 0 (Town) | Townspeople, street urchins, mercenaries (serials 1-14) |
| 1-5 | Molds, centipedes, kobolds, novice adventurers, white worm masses |
| 6-10 | Orcs (Snaga), mushroom patches, bats, large snakes |
| 11-15 | Ogres, half-orcs, light/dark hounds, wargs, skeleton humans |
| 16-20 | Black orcs, trolls, druids, illusionists, cold/fire hounds |
| 21-25 | Dark elves, stone trolls, vampires, hydras, hill giants |
| 26-30 | Cave ogres, mind flayers, wraiths, frost giants, black knights |
| 31-40 | Master thieves, fire giants, patriarch, young dragons |
| 41-50 | Ancient dragons, elemental lords, master mystics, Balrog of Moria |
| 51-60 | Greater undead, elder vampires, time/nether hounds |
| 61-70 | Great wyrms, nightwings, bone golems |
| 71-80 | Greater titans, bronze golems, Maeglin |
| 81-90 | Kronos Lord of Titans |
| 91-99 | Sauron (depth 99) |
| 100 | Morgoth (depth 100) |

### Rarity (W: line, second field)

Higher rarity means the monster is less common. Rarity 1 = standard frequency. Rarity 2+ = proportionally rarer. Most monsters are rarity 1-3. Some boss-tier uniques are rarity 4-5.

### Experience (W: line, fourth field)

XP awarded on kill. Ranges from **0** (town NPCs like the urchin) to **60,000** (Morgoth).

| XP Range | Example Monsters |
|----------|------------------|
| 0-10 | Town monsters, grey mold, worm masses |
| 10-50 | Novice adventurers, basic orcs, kobolds |
| 50-200 | Named unique early bosses, ogres, trolls |
| 200-1000 | Vampires, mind flayers, black knights |
| 1000-5000 | Ancient dragons, elemental lords |
| 5000-15000 | Great wyrms, Dreadmaster, Dracolich |
| 15000-30000 | Ringwraith-tier uniques, Smaug, Glaurung |
| 30000-50000 | Balrog of Moria, Ar-Pharazôn, Gothmog, Sauron |
| 60000 | Morgoth |

---

## 19. Uniques

MAngband has approximately **80+ unique monsters**, each with the `UNIQUE` flag. Key rules:

1. **One at a time**: only one instance of a unique can exist on any level at any time.
2. **Per-player kill tracking**: in MAngband, each player tracks which uniques they have killed (`p_ptr->r_killed[]`). A unique only spawns on a level if at least one player on that level has NOT killed it.
3. **Respawn timer**: killed uniques may respawn after up to `COME_BACK_TIME_MAX` (600 turns).
4. **FORCE_MAXHP**: almost all uniques have this flag, ensuring consistent difficulty.
5. **DROP_GOOD / DROP_GREAT**: uniques always drop at least good-quality loot.

### Notable Uniques by Depth

| Depth | Unique | HP | Speed | Key Abilities |
|-------|--------|-----|-------|---------------|
| 2 | Grip / Fang (Farmer Maggot's dogs) | 25 | 120 | BITE melee |
| 3 | Smeagol | 400 | 130 | Invisible, steals gold, good+great drop |
| 5 | Bullroarer the Hobbit | 60 | 120 | Strong melee, 2d2 good items |
| 13 | Boldor, King of the Yeeks | 180 | 120 | SMART caster, summons kin+monsters |
| 14 | Ufthak of Cirith Ungol | 320 | 110 | 4× HIT melee, escort |
| 24 | Sangahyando of Umbar | 800 | 110 | SLOW, FORGET spells |
| 27 | Mim, Betrayer of Turin | 1100 | 120 | Full elemental immunity, heals, acid ball |
| 32 | Lokkak, the Ogre Chieftain | 1500 | 120 | Escort, 3× 6d6 melee |
| 43 | Quaker, Master of Earth | 2800 | 110 | SHATTER attack, KILL_WALL, acid ball |
| 43 | Ariel, Queen of Air | 2700 | 130 | RAND_25, elemental balls |
| 46 | Scatha the Worm | 2200 | 120 | Cold breath, SMART dragon |
| 48 | Smaug the Golden | 2400 | 120 | Fire breath, SMART dragon |
| 49 | Eol, the Dark Elf | 2400 | 130 | 1_IN_2 caster, massive spell list, summons |
| 50 | The Balrog of Moria | 3000 | 130 | Fire breath, escorts, summons undead+demon |
| 52 | Ji Indur Dawndeath | 3200 | 120 | 5 elemental balls, XP drain |
| 55 | Ar-Pharazôn the Golden | 4000 | 130 | 4×8d8 melee, summons |
| 56 | Dwar, Dog Lord of Waw | 4000 | 120 | Summons hounds+undead, fire+nether balls |
| 66 | Greater titan | 3800 | 120 | 4× confuse 12d12, boulder, summons |
| 69 | Polyphemus, the Blind Cyclops | 5300 | 130 | 11d10 SHATTER, water attacks |
| 70 | Glaurung, Father of Dragons | 7500 | 130 | Fire+poison breath, summons ancient dragons |
| 76 | Atlas, the Titan | 6000 | 120 | 13d13 SHATTER+confuse, KILL_WALL |
| 81 | Maeglin, Traitor of Gondolin | 6000 | 130 | Summons Wraith+HI_UNDEAD+HI_DRAGON+HI_DEMON+UNIQUE |
| 87 | Kronos, Lord of the Titans | 7000 | 120 | 4× breath, summons, 12d12 confuse |
| 99 | **Sauron, the Sorcerer** | **10,500** | **130** | 1_IN_2 caster, BA_MANA, S_UNIQUE, S_HI_DEMON/UNDEAD/DRAGON |
| 100 | **Morgoth, Lord of Darkness** | **20,000** | **140** | 20d10 SHATTER, LOSE_ALL, BA_MANA, full summon suite |

---

## 20. Questors

Two monsters have the `QUESTOR` flag:

### Sauron, the Sorcerer (serial 546)
- **Depth**: 99 | **Rarity**: 1 | **XP**: 50,000
- **Speed**: 130 | **HP**: 10,500 (FORCE_MAXHP)
- **AC**: 160 | **Vision**: 100 (10,000 ft)
- **Melee**: 4 attacks — 2× HIT:UN_BONUS:10d12, 2× HIT:UN_POWER:8d12
- **Spells (1_IN_2)**: TPORT, TELE_LEVEL, BLIND, SCARE, CAUSE_4, BO_ICEE, BO_MANA, BO_PLAS, BRAIN_SMASH, FORGET, BA_MANA, BA_FIRE, BA_WATE, BA_NETH, BA_DARK, S_MONSTERS, S_HI_DEMON, S_HI_UNDEAD, S_HI_DRAGON, S_WRAITH, S_UNIQUE
- **Immunities**: Fire, Cold, Lightning, Poison
- **Flags**: SMART, REGENERATE, MOVE_BODY, NO_CONF, NO_SLEEP, NO_FEAR, FORCE_DEPTH
- **Drops**: ONLY_ITEM, DROP_2D2+3D2+4D2, DROP_GOOD, DROP_GREAT (9-14 great items)
- Killing Sauron grants access to dungeon level 100 (Morgoth's level).

### Morgoth, Lord of Darkness (serial 547)
- **Depth**: 100 | **Rarity**: 1 | **XP**: 60,000
- **Speed**: 140 | **HP**: 20,000 (FORCE_MAXHP)
- **AC**: 150 | **Vision**: 100 (10,000 ft)
- **Melee**: 4 attacks — 2× HIT:SHATTER:20d10, 1× HIT:LOSE_ALL:10d12, 1× TOUCH:UN_POWER
- **Spells (1_IN_3)**: BA_MANA, BO_MANA, BA_NETH, BRAIN_SMASH, S_MONSTERS, S_UNIQUE, S_WRAITH, S_HI_UNDEAD, S_HI_DRAGON, S_HI_DEMON
- **Immunities**: ALL five elements (Acid, Fire, Cold, Lightning, Poison)
- **Flags**: SMART, KILL_WALL, MOVE_BODY, REGENERATE, NO_CONF, NO_SLEEP, NO_FEAR, FORCE_DEPTH
- **Drops**: ONLY_ITEM, DROP_1D2+2D2+3D2+4D2, DROP_GOOD, DROP_GREAT, DROP_CHOSEN (10-16 chosen items)
- His SHATTER attacks cause earthquakes that destroy surrounding walls.
- Killing Morgoth wins the game.

---

## 21. Monster Categories by Symbol

### Total Count

The monster file contains **616 monster entries** (serials 1–616, with serial 0 reserved for the player). This includes:
- ~14 town monsters (depth 0)
- ~80+ unique monsters
- 2 questors (Sauron, Morgoth)
- ~520 regular monsters

### Approximate Breakdown by Type

| Category | Count | Depth Range | Notes |
|----------|-------|-------------|-------|
| Townspeople (`t`) | 10 | 0 | Non-hostile or mild threat |
| Animals (`f,C,R,b,a,r,l,I,K`) | ~80 | 1-45 | ANIMAL flag, vulnerable to Slay Animal |
| Molds and Mushrooms (`m,,`) | ~15 | 1-47 | NEVER_MOVE, spore attacks |
| Jellies and Oozes (`j`) | ~15 | 2-30 | Elemental touches, often stationary |
| Worm Masses (`w`) | ~10 | 1-25 | MULTIPLY, low-level nuisance |
| Centipedes (`c`) | ~10 | 1-15 | Fast, multi-attack |
| Spiders (`S`) | ~15 | 2-48 | Webs, poison, FRIENDS groups |
| Snakes and Nagas (`J,n`) | ~12 | 1-28 | Crush attacks, poison bites |
| Kobolds (`k`) | ~8 | 1-5 | Early game humanoids, IM_POIS |
| Orcs (`o`) | ~10 | 6-30 | EVIL, ORC, groups, HURT_LITE |
| Humanoid NPCs (`p,h`) | ~60 | 2-100 | Adventurers, mages, dark elves, Sauron |
| Ogres (`O`) | ~8 | 13-32 | GIANT, heavy hitters |
| Trolls (`T`) | ~10 | 18-40 | TROLL, REGENERATE, HURT_LITE |
| Giants and Titans (`P`) | ~12 | 25-100 | GIANT, BOULDER, Morgoth |
| Undead (`s,z,G,W,L,V`) | ~60 | 5-99 | IM_COLD, IM_POIS, XP drain, NO_SLEEP |
| Demons (`u,U`) | ~20 | 7-70 | IM_FIRE, summoners |
| Dragons (`d,D`) | ~40 | 16-70 | Breath weapons, POWERFUL |
| Hydras (`M`) | ~8 | 20-55 | Multi-headed, element breath |
| Hounds (`Z`) | ~15 | 15-51 | FRIENDS packs, single-element breathers |
| Eyes (`e`) | ~10 | 1-67 | GAZE attacks, NEVER_MOVE or special |
| Golems (`g`) | ~10 | 14-75 | EMPTY_MIND, immunities |
| Elementals (`E`) | ~8 | 12-43 | PASS_WALL, elemental immunity |
| Vortexes (`v`) | ~5 | 37-53 | RAND_50, breath attacks |
| Mimics (`$,.`) | ~8 | 4-27 | CHAR_CLEAR, disguised as objects |

---

## 22. Representative Monster Roster by Depth Tier

### Depth 1-5 — The Shallows

| Serial | Name | Sym | Spd | HP | AC | Depth | XP | Attacks | Notable |
|--------|------|-----|-----|----|----|-------|----|---------|---------|
| 15 | Grey mold | m:s | 110 | 1d2 | 1 | 1 | 3 | SPORE:HURT:1d4 ×2 | NEVER_MOVE |
| 17 | Giant yellow centipede | c:y | 110 | 2d6 | 12 | 1 | 2 | BITE + STING | ANIMAL |
| 21 | Giant white mouse | r:w | 110 | 1d3 | 4 | 1 | 1 | BITE:1d2 | MULTIPLY |
| 24 | Small kobold | k:y | 110 | 2d7 | 16 | 1 | 5 | HIT:1d5 | IM_POIS, opens doors |
| 27 | Floating eye | e:o | 110 | 3d6 | 6 | 1 | 1 | GAZE:PARALYZE | NEVER_MOVE |
| 35 | Novice warrior | p:u | 110 | 9d4 | 16 | 2 | 6 | 2× HIT:1d7 | Opens/bashes doors |
| 38 | Novice mage | p:r | 110 | 6d4 | 6 | 2 | 6 | HIT:1d4 | Casts BLINK, BLIND, CONF, MISSILE |
| 46 | Grip (unique) | C:U | 120 | 25 | 30 | 2 | 30 | BITE:1d4 | UNIQUE, first boss |
| 52 | Smeagol (unique) | h:B | 130 | 400 | 12 | 3 | 50 | TOUCH:EAT_GOLD | INVISIBLE, great loot |

### Depth 6-15 — Dungeon Entrance

| Serial | Name | Sym | Spd | HP | AC | Depth | XP | Notable |
|--------|------|-----|-----|----|----|-------|----|---------|
| 96 | Snaga | o:U | 110 | 8d8 | 32 | 6 | 15 | ORC, FRIENDS, HURT_LITE |
| 98 | Cave orc | o:G | 110 | 11d9 | 32 | 7 | 20 | ORC, FRIENDS |
| 162 | Dark elven priest | h:b | 120 | 15d10 | 30 | 12 | 80 | Heals, blinds, casts CAUSE_2 |
| 164 | Skeleton human | s:w | 110 | 10d8 | 30 | 12 | 38 | UNDEAD, IM_COLD+POIS |
| 173 | Boldor (unique) | y:v | 120 | 180 | 24 | 13 | 200 | SMART, summons kin, escorts |
| 193 | Light hound | Z:o | 110 | 6d6 | 30 | 15 | 50 | FRIENDS pack, BR_LITE |

### Depth 16-30 — Mid Dungeon

| Serial | Name | Sym | Spd | HP | AC | Depth | XP | Notable |
|--------|------|-----|-----|----|----|-------|----|---------|
| 184 | Hill giant | P:U | 110 | 30d15 | 45 | 25 | 150 | BOULDER spell |
| 278 | Stone troll | T:W | 110 | 23d10 | 40 | 25 | 85 | TROLL, REGENERATE, HURT_LITE |
| 298 | Vampire | V:W | 110 | 25d12 | 45 | 27 | 175 | EXP_20 drain, TELE_TO, BRAIN, FORGET |
| 303 | Black knight | p:s | 120 | 30d10 | 70 | 28 | 240 | BLIND, SCARE, CAUSE_3 |
| 306 | Mind flayer | h:v | 110 | 15d10 | 60 | 28 | 200 | MIND_BLAST, BRAIN_SMASH, stat drain |

### Depth 31-50 — Deep Dungeon

| Serial | Name | Sym | Spd | HP | AC | Depth | XP | Notable |
|--------|------|-----|-----|----|----|-------|----|---------|
| 462 | Ancient multi-hued dragon | D:v | 120 | 2100 | 100 | 43 | 13000 | 5-element breath, SMART, massive drops |
| 466 | Quaker, Master of Earth | E:u | 110 | 2800 | 97 | 43 | 4500 | SHATTER, KILL_WALL, acid spells |
| 470 | Patriarch | p:G | 120 | 520 | 60 | 40 | 1800 | 1_IN_2, HEAL+CAUSE_3+CAUSE_4 |
| 475 | Smaug the Golden | D:R | 120 | 2400 | 130 | 48 | 23000 | Fire breath, SMART, great drops |
| 483 | Balrog of Moria | U:v | 130 | 3000 | 100 | 50 | 30000 | Fire breath, ESCORT+ESCORTS, summons |

### Depth 51-70 — The Abyss

| Serial | Name | Sym | Spd | HP | AC | Depth | XP | Notable |
|--------|------|-----|-----|----|----|-------|----|---------|
| 481 | Glaurung, Father of Dragons | D:R | 130 | 7500 | 140 | 70 | 50000 | Fire+poison breath, S_HI_DRAGON |
| 484 | Nightwing | W:D | 120 | 3600 | 120 | 61 | 10000 | BO_MANA, BA_NETH, BRAIN_SMASH |
| 485 | Nether hound | Z:G | 120 | 600 | 100 | 51 | 5000 | BR_NETH, FRIENDS pack |
| 591 | Beholder hive-mother | e:b | 120 | 3500 | 80 | 67 | 17000 | 1_IN_2, BA_DARK+ACID+FIRE, BRAIN_SMASH, S_KIN |
| 608 | Bone golem | g:D | 120 | 3500 | 170 | 68 | 23000 | UN_BONUS, CAUSE_4, S_UNDEAD |

### Depth 71-100 — Endgame

| Serial | Name | Sym | Spd | HP | AC | Depth | XP | Notable |
|--------|------|-----|-----|----|----|-------|----|---------|
| 586 | Atlas, the Titan | P:s | 120 | 6000 | 160 | 76 | 37000 | 13d13 SHATTER+confuse, KILL_WALL |
| 587 | Kronos, Lord of Titans | P:v | 120 | 7000 | 150 | 87 | 42000 | Multi-breath, summons, ESCORT |
| 595 | Maeglin, Traitor of Gondolin | h:D | 130 | 6000 | 120 | 81 | 35000 | S_WRAITH+HI_UNDEAD+HI_DRAGON+HI_DEMON+UNIQUE |
| 546 | **Sauron** | p:v | 130 | 10500 | 160 | 99 | 50000 | 1_IN_2 caster, all summons |
| 547 | **Morgoth** | P:D | 140 | 20000 | 150 | 100 | 60000 | 20d10 SHATTER, LOSE_ALL, BA_MANA, all immunities |

---

## 23. IronHell Status Summary

### What Should Be Adopted from MAngband

| System | MAngband Source | Priority | Notes |
|--------|---------------|----------|-------|
| Monster data format (NdM HP, speed, AC) | monster.txt | Core | Already partially modeled in `monster_compendium.json` |
| Symbol/color display system | monster.txt G: lines | Core | Map to sprite system or retain for ASCII fallback |
| Melee attack methods and effects | init1.c, melee1.c | Core | 24 methods × 29 effects → rich combat |
| Breath weapon damage (hp-based, capped) | melee2.c | Core | Key dragon/hound mechanic |
| Bolt and ball spell formulas | melee2.c | Core | Level-scaled damage |
| Status spells (fear, blind, confuse, paralyze, slow) | melee2.c | Core | Already partially in game |
| Summoning system | melee2.c | High | Critical for boss fights |
| Smart AI / resistance learning | melee2.c remove_bad_spells() | High | Makes monsters adaptive |
| Drop system (flags → count × quality) | monster1.c, melee1.c | Core | Already partially modeled |
| Unique tracking (per-player kills) | monster2.c | High | MAngband multiplayer feature |
| Species flags (ORC, TROLL, DRAGON, etc.) | init1.c flags3 | Core | Needed for slay/brand interactions |
| Immunity/vulnerability system | init1.c flags3 | Core | Already partially in game types |
| FORCE_MAXHP for bosses | monster2.c | Core | Ensures consistent boss difficulty |
| Alertness / FORCE_SLEEP | monster.txt I: line | Medium | Starting-asleep mechanic |
| MULTIPLY (breeding) | monster2.c | Medium | Classic nuisance mechanic |
| Group flags (FRIENDS, ESCORT) | monster2.c | High | Pack encounters |
| Protection from Evil | melee1.c | Medium | Priest class feature interaction |

### What Exists in `monster_compendium.json`

The current `monster_compendium.json` has a simplified format with `stats`, `abilities`, `ai_behavior`, `blows`, `loot_table`, `resistances`, `vulnerabilities`. This should be enriched with:
- Full MAngband flag sets (flags1-3, flags4-6 for spells)
- HP as `NdM` dice + FORCE_MAXHP flag
- Speed as integer (110-base system)
- All 4 blow slots with method/effect/dice
- Spell frequency and spell flag lists
- Native depth + rarity + experience values
- Symbol and color for map display
- Species type flags for slay/brand interaction

### Monster Data Pipeline

```
monster.txt (616 entries, MAngband reference)
    ↓ parse
monster_compendium.json (IronHell game data, curated subset)
    ↓ load
src/game/types/monster.ts (TypeScript interfaces)
    ↓ use
src/game/systems/ (combat, AI, spawning, drops)
```

---

## 24. Sleep, Stealth, and Backstab Mechanics

> **MAngband parity section.** Sources: `monster2.c` (csleep), `cmd1.c` (stealth mode), `melee2.c` (backstab).

### 24.1 Monster Sleep (csleep / sleepDepth)

In MAngband each monster has a `csleep` counter. IronHell maps this to `Monster.sleepDepth`.

**Spawning:**
- Monsters with `forceSleep: true` in `flags` spawn with `statusEffects` containing a Sleep entry.
- Their `sleepDepth` is set to `flags.sleepRating ?? (level × 3 + 50)`.
- Example: Ancient dragon (level 40) without explicit `sleepRating` → `sleepDepth = 170`.

**Wake depletion each tick (in `monsterAI.ts`):**
1. Check if nearest player is within detection range (`isPlayerDetectedByMonster`).
2. If detected: noise roll `rng() < noiseLevel × NOISE_HEARING_MULTIPLIER + proximityBonus`.
3. On success: `sleepDepth -= floor(100 / distance)`.
4. At `sleepDepth = 0`: monster wakes, `aiState = 'chase'`, sleep status removed.

**Key:** `noiseLevel` comes from `computeMovementNoise(stealth)` — a rogue with stealth 30 generates noise=0, so no wake roll ever fires.

### 24.2 Which Monsters Spawn Asleep

Monsters with `flags.forceSleep: true` in `monster_compendium.json`. Notable examples:

| Category | Examples | Typical sleepDepth | Notes |
|----------|---------|-------------------|-------|
| Ancient dragons (`D`) | Great ice wyrm, Smaug, Glaurung | 120–180 | Also have `noSleep` — cannot be re-slept |
| Deep-level undead | Elder vampires, Lich | 100–140 | May also have `noSleep` |
| Powerful uniques | Treasure-room bosses | Varies | `sleepRating` set explicitly in JSON |
| Certain demons | Greater demons (select) | 90–120 | — |

### 24.3 Backstab — Rogue Bonus vs Sleeping Monsters

A Rogue can deal massive bonus damage on the first strike against a sleeping monster that was **never woken** by the player's approach.

**Conditions for backstab:**
1. Attacker is Rogue class.
2. Target has Sleep status (`hasStatusKeyword('sleep')`).
3. Target's `sleepDepth` equals its **spawn sleep depth** (no depletion occurred — monster was never disturbed).

**Who is backstabbable:**
- Any monster with `flags.forceSleep: true` that was never disturbed.
- This includes dragons, powerful undead, high-level demons, and any other monster that spawns asleep.
- Monsters put to sleep by magic (Sleep Monster scroll/spell) are also eligible for conditions 1-2 (but condition 3 would not apply since they have no spawn sleepDepth).

**Rogue play pattern:**
1. Enter dungeon level.
2. Toggle stealth mode (`T` key) → ×3 stealth → movement noise drops to 0 at stl≥30.
3. Navigate to sleeping dragon without generating any noise.
4. First attack lands as backstab with full bonus.
5. Remaining attacks are normal (dragon is now awake).

### 24.4 NO_SLEEP Flag vs. FORCE_SLEEP Flag

These flags serve opposite purposes and can coexist:

| Flag | Applies to | Blocks |
|------|-----------|--------|
| `forceSleep` | Spawn | Nothing — causes spawn sleep |
| `noSleep` | Spell targeting | Blocks Sleep Monster / Sleep spells only |

Ancient dragons (`D` symbol) routinely carry both: they wake from spawn sleep normally but cannot be re-slept by magic once awake.

