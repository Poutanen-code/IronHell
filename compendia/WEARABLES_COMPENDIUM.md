# IronHell — Wearables Compendium

> Source: MAngband 1.5.3 reference files (`object.txt`, `ego_item.txt`, `artifact.txt`)
> All stats reference the source data format:
> - **AC** = base armor class bonus
> - **Dmg** = base combat damage dice (body-part collision)
> - **to-h / to-d / to-a** = enchantment pluses (to-hit / to-dam / to-AC)
> - **Depth** = minimum dungeon level where the item naturally appears (50 ft per level)
> - **Weight** = in pounds (source stores tenths-of-pounds)
> - **Cost** = base gold value
> - **pval** = magnitude of numeric bonuses (stat boosts, speed, stealth, etc.) — randomised at generation unless stated
> - **Flags** explained in the [Flag Glossary](#flag-glossary) at the end

---

## Table of Contents

1. [Boots](#1-boots)
2. [Gloves & Gauntlets](#2-gloves--gauntlets)
3. [Helmets & Caps](#3-helmets--caps)
4. [Crowns](#4-crowns)
5. [Shields](#5-shields)
6. [Cloaks](#6-cloaks)
7. [Body Armor — Soft / Leather](#7-body-armor--soft--leather)
8. [Body Armor — Hard Metal](#8-body-armor--hard-metal)
9. [Dragon Scale Mail](#9-dragon-scale-mail)
10. [Rings](#10-rings)
11. [Amulets](#11-amulets)
12. [Ego-Item Enchantments](#12-ego-item-enchantments)
13. [Named Artifacts](#13-named-artifacts)
14. [Flag Glossary](#flag-glossary)

---

## 1. Boots

Equipment slot: **feet**. Boots provide AC, may carry ego enchantments, and some activate.

### 1.1 Base Boots

| # | Name | AC | Dmg | Depth | Wt (lb) | Cost |
|---|------|----|-----|-------|---------|------|
| 91 | Pair of Soft Leather Boots | 2 | 1d1 | 3 | 2.0 | 7 |
| 92 | Pair of Hard Leather Boots | 3 | 1d1 | 5 | 4.0 | 12 |
| 93 | Pair of Metal Shod Boots | 6 | 1d1 | 20 | 8.0 | 50 |

### 1.2 Ego Boot Enchantments

| Ego Suffix | Rating | Depth | Extra AC | pval | Cost | Flags |
|------------|--------|-------|----------|------|------|-------|
| of Slow Descent | 7 | 0 | — | — | 250 | FEATHER |
| of Stealth | 16 | 0 | — | up to +3 | 500 | STEALTH |
| of Free Action | 15 | 0 | — | — | 1,000 | FREE_ACT |
| of Speed | 25 | 0 | — | up to +10 | 100,000 | SPEED |
| of Stability | 20 | 0 | — | — | 5,000 | RES_NEXUS, FEATHER |
| of Elvenkind | 30 | 60 | — | up to +5 | 200,000 | STEALTH, SPEED, FEATHER, IGNORE_ACID, IGNORE_FIRE *(Hard/Metal Shod only)* |
| of Slowness | 0 | 0 | — | −5 | 0 | SPEED, LIGHT_CURSE *(cursed)* |
| of Annoyance | 0 | 0 | — | −10 | 0 | SPEED, STEALTH, AGGRAVATE, LIGHT_CURSE *(cursed)* |

> **Elvenkind boots** require sval 2–3 (Hard Leather Boots or Metal Shod Boots).

### 1.3 Artifact Boots

#### The Pair of Hard Leather Boots of Feanor
- **Base:** Hard Leather Boots
- **P:** AC 3, 1d1, to-h 0, to-d 0, to-a +20
- **pval:** +15 SPEED
- **Depth:** 40 | **Rarity:** 120 | **Wt:** 4.0 lb | **Cost:** 300,000
- **Flags:** SPEED, RES_NEXUS, ACTIVATE (Haste, recharge 200), IGNORE_ALL *(implied by ACTIVATE haste)*
- *"The boots of Feanor, creator of the Silmarils and mightiest of the Eldar. Once sped him to do battle for Middle-Earth."*

#### The Pair of Soft Leather Boots 'Dal-i-thalion'
- **Base:** Soft Leather Boots
- **P:** AC 2, 1d1, +0/+0/+15
- **pval:** +5 DEX
- **Depth:** 10 | **Rarity:** 25 | **Wt:** 2.0 lb | **Cost:** 40,000
- **Flags:** DEX, FREE_ACT, ACTIVATE (Remove Fear/Poison, recharge 5), RES_NETHR, RES_CHAOS, RES_CONFU, SUST_CON

#### The Pair of Metal Shod Boots of Thror
- **Base:** Metal Shod Boots
- **P:** AC 6, 1d1, +0/+0/+20
- **pval:** +3 STR, CON, SPEED
- **Depth:** 30 | **Rarity:** 25 | **Wt:** 8.0 lb | **Cost:** 12,000
- **Flags:** STR, CON, SPEED, RES_FEAR
- *"Sturdy footwear as enduring as the Dwarven king-in-exile."*

#### The Pair of Soft Leather Boots of Wormtongue *(cursed)*
- **Base:** Soft Leather Boots
- **P:** AC 2, 1d1, to-h −8, to-d −8, to-a +0
- **pval:** +3 INT, DEX, STEALTH, SPEED; FEATHER
- **Depth:** 10 | **Rarity:** 15 | **Wt:** 2.0 lb | **Cost:** 17,000
- **Flags:** LIGHT_CURSE, ACTIVATE (Phase Door, recharge 20)
- *"Running shoes of the treacherous Grima Wormtongue — suited for a spy, or a coward."*

---

## 2. Gloves & Gauntlets

Equipment slot: **hands**. Heavy gloves impose combat penalties unless enchanted; leather gloves do not.

### 2.1 Base Gloves

| # | Name | AC | Dmg | to-h | to-d | Depth | Wt (lb) | Cost | Notes |
|---|------|----|-----|------|------|-------|---------|------|-------|
| 125 | Set of Leather Gloves | 1 | 0d0 | 0 | 0 | 1 | 0.5 | 3 | No melee penalty |
| 126 | Set of Gauntlets | 2 | 1d1 | 0 | 0 | 10 | 2.5 | 35 | |
| 127 | Set of Cesti | 5 | 1d1 | 0 | 0 | 50 | 4.0 | 100 | Heavy combat gloves |

> **Spellcasters note:** Heavy gloves (Gauntlets, Cesti) reduce spellcasting ability unless the ego suffix negates the penalty (e.g. *of Free Action* or *of Agility*).

### 2.2 Ego Glove Enchantments

| Ego Suffix | Rating | Depth | Bonus | pval | Cost | Flags |
|------------|--------|-------|-------|------|------|-------|
| of Free Action | 11 | 0 | — | — | 1,000 | FREE_ACT |
| of Slaying | 17 | 0 | up to +5 to-h/d | — | 1,500 | SHOW_MODS |
| of Agility | 14 | 0 | — | up to +5 | 1,000 | DEX |
| of Power | 22 | 0 | up to +5 to-h/d | up to +5 | 2,500 | STR |
| of Thievery | 22 | 40 | up to +8 to-h, +3 to-d | up to +5 | 5,000 | DEX, SEARCH, FEATHER, FREE_ACT *(Leather Gloves only)* |
| of Combat | 22 | 50 | up to +3 to-h, +8 to-d | up to +2 | 7,000 | STR, CON, AGGRAVATE *(Gauntlets/Cesti only)* |
| of Weakness | 0 | 0 | — | −10 | 0 | STR, LIGHT_CURSE *(cursed)* |
| of Clumsiness | 0 | 0 | — | −10 | 0 | DEX, LIGHT_CURSE *(cursed)* |

### 2.3 Artifact Gloves

#### The Gauntlets of Eol
- **Base:** Gauntlets
- **P:** AC 2, 1d1, to-h −5, to-d −5, to-a +15
- **pval:** +3 INT
- **Depth:** 55 | **Rarity:** 35 | **Wt:** 2.5 lb | **Cost:** 40,000
- **Flags:** FREE_ACT, FEATHER, RES_ELEC, RES_DARK, RES_POIS, AGGRAVATE, ACTIVATE (Mana Bolt, recharge 30)
- *"The iron-shod gauntlets of the Dark Elven smith Eol, tingling with magics he could channel in battle."*

#### The Set of Leather Gloves 'Cambeleg'
- **Base:** Leather Gloves
- **P:** AC 1, 0d0, to-h +8, to-d +8, to-a +15
- **pval:** +2 STR, CON
- **Depth:** 10 | **Rarity:** 6 | **Wt:** 0.5 lb | **Cost:** 36,000
- **Flags:** FREE_ACT, SHOW_MODS
- *"A hero's handgear that lends great prowess in battle."*

#### The Set of Leather Gloves 'Cammithrim'
- **Base:** Leather Gloves
- **P:** AC 1, 0d0, +0/+0/+10
- **Depth:** 10 | **Rarity:** 3 | **Wt:** 0.5 lb | **Cost:** 30,000
- **Flags:** FREE_ACT, RES_LITE, SUST_CON, LITE, ACTIVATE (Magic Missile, recharge 2)
- *"These gloves glow so brightly as to light the way and cast magical bolts with great frequency."*

#### The Set of Gauntlets 'Paurhach'
- **Base:** Gauntlets
- **P:** AC 2, 1d1, +0/+0/+15
- **Depth:** 10 | **Rarity:** 3 | **Wt:** 2.5 lb | **Cost:** 15,000
- **Flags:** RES_FIRE, REGEN, ACTIVATE (Fire Ball, recharge 8)
- *"A set of gauntlets that smoulder with unnatural heat."*

#### The Set of Gauntlets 'Paurnimmen'
- **Base:** Gauntlets
- **P:** AC 2, 1d1, +0/+0/+15
- **Depth:** 10 | **Rarity:** 3 | **Wt:** 2.5 lb | **Cost:** 13,000
- **Flags:** RES_COLD, SLOW_DIGEST, ACTIVATE (Frost Ball, recharge 7)
- *"Freezing with unnatural cold."*

#### The Set of Gauntlets 'Pauraegen'
- **Base:** Gauntlets
- **P:** AC 2, 1d1, +0/+0/+15
- **Depth:** 10 | **Rarity:** 3 | **Wt:** 2.5 lb | **Cost:** 11,000
- **Flags:** RES_ELEC, LITE, ACTIVATE (Lightning Bolt, recharge 6)
- *"Sparks crackle across its knuckleguards."*

#### The Set of Gauntlets 'Paurnen'
- **Base:** Gauntlets
- **P:** AC 2, 1d1, +0/+0/+15
- **Depth:** 10 | **Rarity:** 3 | **Wt:** 2.5 lb | **Cost:** 12,000
- **Flags:** RES_ACID, FEATHER, ACTIVATE (Acid Ball, recharge 5)
- *"Gives off a foul acrid odour yet remains untarnished."*

#### The Set of Gauntlets 'Camlost' *(cursed)*
- **Base:** Gauntlets
- **P:** AC 2, 1d1, to-h −12, to-d −12
- **pval:** −3 STR, DEX
- **Depth:** 10 | **Rarity:** 20 | **Wt:** 2.5 lb | **Cost:** 0
- **Flags:** RES_FIRE, RES_DISEN, FREE_ACT, DRAIN_EXP, AGGRAVATE, LIGHT_CURSE, HEAVY_CURSE
- *"Named after the empty hand of Beren that once clasped a Silmaril."*

#### The Set of Cesti of Fingolfin
- **Base:** Cesti
- **P:** AC 5, 1d1, to-h +10, to-d +10, to-a +20
- **pval:** +4 DEX
- **Depth:** 40 | **Rarity:** 15 | **Wt:** 4.0 lb | **Cost:** 110,000
- **Flags:** FREE_ACT, RES_ACID, ACTIVATE (Arrow, recharge 30), SHOW_MODS
- *"The hand-sheathing of Fingolfin, warrior-king of Elves and Men, who dealt Morgoth seven mighty wounds."*

---

## 3. Helmets & Caps

Equipment slot: **head** (sub-type: caps/helms). Helms provide AC and may carry ego enchantments; heavier helms impose spell-failure penalties unless enchanted.

### 3.1 Base Helmets

| # | Name | AC | Dmg | Depth | Wt (lb) | Cost | Notes |
|---|------|----|-----|-------|---------|------|-------|
| 94 | Hard Leather Cap | 2 | 0d0 | 3 | 1.5 | 12 | Light, no spellcast penalty |
| 95 | Metal Cap | 3 | 1d1 | 10 | 2.0 | 30 | |
| 96 | Iron Helm | 5 | 1d3 | 20 | 7.5 | 75 | Heavy |
| 97 | Steel Helm | 6 | 1d3 | 40 | 6.0 | 200 | Heavy |

### 3.2 Ego Helm Enchantments

*(Apply to both caps (tval 32) and crowns (tval 33) unless noted)*

| Ego Suffix | Rating | Depth | pval max | Cost | Flags |
|------------|--------|-------|----------|------|-------|
| of Intelligence | 13 | 0 | +2 INT | 500 | INT, SUST_INT |
| of Wisdom | 13 | 0 | +2 WIS | 500 | WIS, SUST_WIS |
| of Beauty | 8 | 0 | +4 CHR | 1,000 | CHR, SUST_CHR |
| of Seeing | 8 | 0 | +5 SEARCH | 2,000 | SEARCH, RES_BLIND, SEE_INVIS *(caps + crowns)* |
| of Infravision | 11 | 0 | +5 INFRA | 500 | INFRA *(caps only)* |
| of Light | 6 | 0 | — | 1,000 | LITE, RES_LITE *(caps only)* |
| of Telepathy | 20 | 0 | — | 50,000 | TELEPATHY *(caps + crowns)* |
| of Regeneration | 10 | 0 | — | 1,500 | REGEN *(caps + crowns)* |
| of Teleportation | 0 | 0 | — | 0 | TELEPORT, LIGHT_CURSE *(cursed, caps only)* |
| of Dullness | 0 | 0 | −5 pval | 0 | INT, WIS, CHR, LIGHT_CURSE *(cursed)* |
| of Sickliness | 0 | 0 | −5 pval | 0 | STR, DEX, CON, LIGHT_CURSE *(cursed, crowns only)* |

### 3.3 Artifact Helmets

#### The Metal Cap of Celebrimbor
- **Base:** Metal Cap
- **P:** AC 3, 1d1, +0/+0/+18
- **pval:** +3 INT, DEX, CHR, SEARCH
- **Depth:** 55 | **Rarity:** 12 | **Wt:** 2.0 lb | **Cost:** 45,000
- **Flags:** RES_FIRE, RES_ACID, RES_DISEN, RES_SHARD
- *"Forged by the greatest Noldorin smith of the Second Age; its enchantment will never be diminished."*

#### The Hard Leather Cap of Thranduil
- **Base:** Hard Leather Cap
- **P:** AC 2, 0d0, +0/+0/+10
- **pval:** +2 INT, WIS
- **Depth:** 20 | **Rarity:** 2 | **Wt:** 1.5 lb | **Cost:** 50,000
- **Flags:** RES_BLIND, TELEPATHY
- *"The hunting cap of King Thranduil, to whose ears come all the secrets of his forest domain."*

#### The Metal Cap of Thengel
- **Base:** Metal Cap
- **P:** AC 3, 1d1, +0/+0/+12
- **pval:** +3 WIS, CHR
- **Depth:** 10 | **Rarity:** 2 | **Wt:** 2.0 lb | **Cost:** 22,000
- **Flags:** RES_CONFU
- *"Embossed with scenes of valor in fine-engraved silver. Grants the wearer nobility and understanding."*

#### The Steel Helm of Hammerhand
- **Base:** Steel Helm
- **P:** AC 6, 1d3, +0/+0/+20
- **pval:** +3 STR, DEX, CON
- **Depth:** 20 | **Rarity:** 8 | **Wt:** 6.0 lb | **Cost:** 45,000
- **Flags:** SUST_STR, SUST_DEX, SUST_CON, AGGRAVATE, RES_ACID, RES_NEXUS, RES_COLD, RES_DARK
- *"As steady and as distinctive as the hero of the Westdike."*

#### The Iron Helm of Dor-Lomin
- **Base:** Iron Helm (sval 5)
- **P:** AC 5, 1d3, +0/+0/+20
- **pval:** +4 STR, DEX, CON
- **Depth:** 40 | **Rarity:** 20 | **Wt:** 7.5 lb | **Cost:** 300,000
- **Flags:** RES_FEAR, RES_ACID, RES_ELEC, RES_FIRE, RES_COLD, LITE, SEE_INVIS, TELEPATHY
- *"The legendary dragon helm of Turin Turambar, an object of dread to the servants of Morgoth."*

#### The Iron Helm 'Holhenneth'
- **Base:** Iron Helm
- **P:** AC 5, 1d3, +0/+0/+10
- **pval:** +2 INT, WIS, SEARCH
- **Depth:** 20 | **Rarity:** 5 | **Wt:** 7.5 lb | **Cost:** 100,000
- **Flags:** RES_BLIND, RES_CONFU, SEE_INVIS, ACTIVATE (Detect, recharge 55)
- *"A famous helm granting extraordinary powers of mind and awareness."*

#### The Iron Helm of Gorlim *(cursed)*
- **Base:** Iron Helm
- **P:** AC 5, 1d3, to-h +8, to-d +8, to-a +10
- **pval:** −5 INT, WIS, SEARCH
- **Depth:** 20 | **Rarity:** 5 | **Wt:** 7.5 lb | **Cost:** 1
- **Flags:** RES_FEAR, SEE_INVIS, FREE_ACT, AGGRAVATE, LIGHT_CURSE, HEAVY_CURSE
- *"A headpiece that betrayed a warrior when he most needed succor."*

---

## 4. Crowns

Equipment slot: **head** (sub-type: crowns). Crowns have AC 0 (they grant no base protection) but carry high enchantment potential.

### 4.1 Base Crowns

| # | Name | AC | Dmg | Depth | Wt (lb) | Cost | Notes |
|---|------|----|-----|-------|---------|------|-------|
| 98 | Iron Crown | 0 | 1d1 | 45 | 2.0 | 500 | |
| 99 | Golden Crown | 0 | 1d1 | 45 | 3.0 | 1,000 | IGNORE_ACID |
| 100 | Jewel Encrusted Crown | 0 | 1d1 | 50 | 4.0 | 2,000 | IGNORE_ACID |

### 4.2 Ego Crown Enchantments

*(Crowns share ego table with helms; crown-exclusive entries shown below)*

| Ego Suffix | Rating | pval max | Cost | Flags |
|------------|--------|----------|------|-------|
| of the Magi | 15 | +3 INT | 7,500 | INT, SUST_INT, RES_ACID/ELEC/FIRE/COLD, IGNORE_ALL; +1 extra random flag |
| of Might | 19 | +3 STR/DEX/CON | 7,500 | STR, DEX, CON, SUST_STR/DEX/CON, FREE_ACT, IGNORE_ALL |
| of Lordliness | 17 | +3 WIS/CHR | 7,500 | WIS, CHR, SUST_WIS/CHR, RES_FEAR, IGNORE_ALL; +1 extra random flag |
| of Serenity | 20 | — | 4,000 | RES_SOUND, RES_CONFU, RES_FEAR *(crowns only)* |
| of Night and Day | 18 | — | 4,000 | RES_LITE, RES_DARK, LITE, SEE_INVIS, RES_BLIND, IGNORE_ACID *(depth 35, crowns only)* |

### 4.3 Artifact Crowns

#### The Massive Iron Crown of Morgoth *(perma-cursed)*
- **Base:** Iron Crown (special sval 50)
- **P:** AC 0, 1d1, +0/+0/+0
- **pval:** +125 all stats, INFRA
- **Depth:** 100 | **Rarity:** 1 (unique drop from Morgoth) | **Wt:** 40.0 lb | **Cost:** 10,000,000
- **Flags:** STR/INT/WIS/DEX/CON/CHR/INFRA, RES_ACID/ELEC/FIRE/COLD/POIS/LITE/DARK/CONFU/NEXUS/NETHR, LITE, SEE_INVIS, TELEPATHY, RES_FEAR, LIGHT_CURSE, HEAVY_CURSE, PERMA_CURSE, INSTA_ART
- *"Containing much of the power of the mightiest Ainur; mounted with the two remaining Silmarils."*

#### The Iron Crown of Beruthiel *(cursed)*
- **Base:** Iron Crown
- **P:** AC 0, 1d1, +0/+0/+20
- **pval:** −5 STR, DEX, CON
- **Depth:** 40 | **Rarity:** 12 | **Wt:** 2.0 lb | **Cost:** 1
- **Flags:** FREE_ACT, SEE_INVIS, TELEPATHY, LIGHT_CURSE, HEAVY_CURSE
- *"The midnight-hued steel circlet of the sorceress-queen Beruthiel — grants extraordinary powers at a terrible physical cost."*

#### The Golden Crown of Gondor
- **Base:** Golden Crown
- **P:** AC 0, 1d1, +0/+0/+15
- **pval:** +3 STR, WIS, CON, SPEED
- **Depth:** 40 | **Rarity:** 40 | **Wt:** 3.0 lb | **Cost:** 100,000
- **Flags:** RES_COLD, RES_FIRE, RES_LITE, RES_BLIND, RES_CONFU, RES_SOUND, RES_CHAOS, LITE, SEE_INVIS, REGEN, ACTIVATE (Heal, recharge 250)
- *"The shining winged circlet brought by Elendil from dying Numenor, emblem of Gondor through an age of the world."*

#### The Jewel Encrusted Crown of Numenor
- **Base:** Jewel Encrusted Crown
- **P:** AC 0, 1d1, +0/+0/+18
- **pval:** +3 INT, DEX, CHR, SEARCH, SPEED
- **Depth:** 60 | **Rarity:** 30 | **Wt:** 4.0 lb | **Cost:** 50,000
- **Flags:** SEE_INVIS, FREE_ACT, RES_SHARD, RES_SOUND, RES_COLD, RES_LITE, RES_DARK, RES_BLIND, LITE
- *"A crown of massive gold set with wondrous jewels of thought and warding, worn by the kings of ancient Numenor."*

---

## 5. Shields

Equipment slot: **off-hand**. Shields provide AC and block attacks. They require one free hand (incompatible with two-handed weapons).

### 5.1 Base Shields

| # | Name | AC | Dmg | to-a | Depth | Wt (lb) | Cost | Notes |
|---|------|----|-----|------|-------|---------|------|-------|
| 122 | Shield of Deflection | 10 | 1d1 | +10 | 70 | 10.0 | 10,000 | IGNORE_ACID; special base |
| 128 | Small Leather Shield | 2 | 1d1 | 0 | 3 | 5.0 | 30 | |
| 129 | Large Leather Shield | 4 | 1d2 | 0 | 15 | 10.0 | 120 | |
| 130 | Small Metal Shield | 3 | 1d2 | 0 | 10 | 6.5 | 50 | |
| 131 | Large Metal Shield | 5 | 1d3 | 0 | 30 | 12.0 | 200 | |

### 5.2 Ego Shield Enchantments

| Ego Suffix | Rating | Depth | Extra AC | Cost | Flags |
|------------|--------|-------|----------|------|-------|
| of Resist Acid | 16 | 0 | — | 1,000 | RES_ACID, IGNORE_ACID |
| of Resist Lightning | 10 | 0 | — | 400 | RES_ELEC, IGNORE_ELEC |
| of Resist Fire | 14 | 0 | — | 800 | RES_FIRE, IGNORE_FIRE |
| of Resist Cold | 12 | 0 | — | 600 | RES_COLD, IGNORE_COLD |
| of Resistance | 20 | 0 | up to +10 | 12,500 | RES_ACID/ELEC/FIRE/COLD, IGNORE_ALL |
| of Elvenkind | 25 | 0 | up to +10 | 18,000 | STEALTH (+3), RES_ALL_FOUR, IGNORE_ALL |
| of Preservation | 25 | 60 | up to +20 | 24,000 | RES_DISEN, SUST_STR/CON/DEX, HOLD_LIFE, IGNORE_ALL |
| of Vulnerability | 0 | 0 | −50 | 0 | AGGRAVATE, LIGHT_CURSE *(cursed)* |

### 5.3 Artifact Shields

#### The Shield of Deflection of Gil-galad
- **Base:** Shield of Deflection
- **P:** AC 10, 1d3, +0/+0/+20
- **pval:** +5 WIS, CHR
- **Depth:** 70 | **Rarity:** 4 | **Wt:** 10.0 lb | **Cost:** 65,000
- **Flags:** LITE, RES_ELEC, RES_ACID, RES_DISEN, RES_DARK, SUST_WIS/DEX/CHR, ACTIVATE (Starlight, recharge 100)
- *"The legendary shield of Ereinion Gil-galad, who fought his way to the gates of the Dark Tower."*

#### The Small Metal Shield of Thorin
- **Base:** Small Metal Shield
- **P:** AC 3, 1d2, +0/+0/+25
- **pval:** +4 STR, CON
- **Depth:** 40 | **Rarity:** 6 | **Wt:** 6.5 lb | **Cost:** 60,000
- **Flags:** RES_FEAR, FREE_ACT, IM_ACID, RES_SOUND, RES_CHAOS
- *"Invoking the strength of Thorin, King under the Mountain — proof against the Element of Earth."*

#### The Large Leather Shield of Celegorm
- **Base:** Large Leather Shield
- **P:** AC 4, 1d2, +0/+0/+20
- **Depth:** 30 | **Rarity:** 3 | **Wt:** 10.0 lb | **Cost:** 12,000
- **Flags:** RES_ACID, RES_ELEC, RES_FIRE, RES_COLD, RES_LITE, RES_DARK
- *"Once protected Celegorm, Lord of Himlad; a mystic balance of the elements lies around it."*

#### The Large Metal Shield of Anarion
- **Base:** Large Metal Shield
- **P:** AC 5, 1d3, +0/+0/+20
- **Depth:** 40 | **Rarity:** 9 | **Wt:** 12.0 lb | **Cost:** 160,000
- **Flags:** RES_ACID, RES_ELEC, RES_FIRE, RES_COLD, SUST_STR/INT/WIS/DEX/CON/CHR (all six)
- *"The great metal-bound shield of Anarion, son of Elendil, whom Sauron found himself powerless to wither."*

---

## 6. Cloaks

Equipment slot: **back**. Cloaks provide AC and stealth bonuses. Shadow Cloaks are a special subtype with higher base AC.

### 6.1 Base Cloaks

| # | Name | AC | to-a | Depth | Wt (lb) | Cost | Notes |
|---|------|----|------|-------|---------|------|-------|
| 123 | Cloak | 1 | 0 | 1 | 1.0 | 3 | Common |
| 124 | Shadow Cloak | 6 | +4 | 60 | 0.5 | 4,000 | Rare; stealth-class item |

### 6.2 Ego Cloak Enchantments

| Ego Suffix | Rating | Depth | Bonus | pval | Cost | Flags |
|------------|--------|-------|-------|------|------|-------|
| of Protection | 10 | 0 | up to +10 AC | — | 1,500 | RES_SHARD, IGNORE_ALL |
| of Stealth | 10 | 0 | — | up to +3 | 500 | STEALTH |
| of Aman | 20 | 0 | up to +20 AC | up to +3 | 4,000 | STEALTH, IGNORE_ALL; +1 extra flag |
| of the Magi | 15 | 30 | up to +4 AC | up to +2 | 2,000 | INT, SUST_INT, STEALTH, IGNORE_ACID |
| of Enveloping | 0 | 0 | — | — | 0 | SHOW_MODS, LIGHT_CURSE, −to-h/d *(cursed)* |
| of Vulnerability | 0 | 0 | −50 AC | — | 0 | AGGRAVATE, LIGHT_CURSE *(cursed)* |
| of Irritation | 0 | 0 | — | — | 0 | AGGRAVATE, SHOW_MODS, LIGHT_CURSE, −to-h/d *(cursed)* |

### 6.3 Artifact Cloaks

#### The Cloak 'Colluin'
- **Base:** Cloak
- **P:** AC 1, +0/+0/+15
- **Depth:** 5 | **Rarity:** 45 | **Wt:** 1.0 lb | **Cost:** 50,000
- **Flags:** RES_ACID, RES_ELEC, RES_FIRE, RES_COLD, RES_POIS, ACTIVATE (Resistance, recharge 111)
- *"Worn by a hero from Valinor, a land utterly beyond the strife of Elements."*

#### The Cloak 'Holcolleth'
- **Base:** Cloak
- **P:** AC 1, +0/+0/+4
- **pval:** +2 INT, WIS, SPEED, STEALTH
- **Depth:** 5 | **Rarity:** 25 | **Wt:** 1.0 lb | **Cost:** 18,000
- **Flags:** RES_ACID, ACTIVATE (Sleep, recharge 55)
- *"An elven-grey mantle possessing great powers of tranquility and concealment."*

#### The Cloak of Thingol
- **Base:** Cloak
- **P:** AC 1, +0/+0/+18
- **pval:** +3 DEX, CHR
- **Depth:** 5 | **Rarity:** 50 | **Wt:** 1.0 lb | **Cost:** 35,000
- **Flags:** FREE_ACT, RES_ACID, RES_FIRE, RES_COLD, ACTIVATE (Recharge, recharge 70)
- *"Translucent pearly grey, with glowing elven-runes to restore magic."*

#### The Cloak of Thorongil
- **Base:** Cloak
- **P:** AC 1, +0/+0/+10
- **Depth:** 5 | **Rarity:** 10 | **Wt:** 1.0 lb | **Cost:** 8,000
- **Flags:** FREE_ACT, RES_ACID, RES_FEAR, SEE_INVIS
- *"Once worn by the young Aragorn, a keen-eyed captain of Gondor."*

#### The Cloak 'Colannon'
- **Base:** Cloak
- **P:** AC 1, +0/+0/+15
- **pval:** +3 STEALTH, SPEED
- **Depth:** 5 | **Rarity:** 20 | **Wt:** 1.0 lb | **Cost:** 20,000
- **Flags:** RES_NEXUS, ACTIVATE (Teleport, recharge 45)
- *"A crystal-blue cape of fine silk worn by a silent messenger of the forces of Law."*

#### The Shadow Cloak of Luthien
- **Base:** Shadow Cloak
- **P:** AC 6, +0/+0/+20
- **pval:** +2 INT, WIS, CHR, SPEED, STEALTH
- **Depth:** 40 | **Rarity:** 40 | **Wt:** 0.5 lb | **Cost:** 45,000
- **Flags:** RES_ACID, RES_FIRE, RES_COLD, ACTIVATE (Restore Life, recharge 250)
- *"The opaque midnight folds, inset with diamonds — a fragment of Luthien's power to restore."*

#### The Shadow Cloak of Tuor
- **Base:** Shadow Cloak
- **P:** AC 6, +0/+0/+12
- **pval:** +4 STEALTH, DEX
- **Depth:** 40 | **Rarity:** 40 | **Wt:** 0.5 lb | **Cost:** 35,000
- **Flags:** FREE_ACT, IM_ACID, RES_ACID, SEE_INVIS
- *"From the ruin of Gondolin did Tuor escape, mantled by sea-commanding Ulmo's kingly gift."*

---

## 7. Body Armor — Soft / Leather

Equipment slot: **body**. Soft armors impose little or no spell-failure penalty. Heavier leathers impose a small to-hit penalty.

### 7.1 Base Soft Armor (tval 36)

| # | Name | AC | Dmg | to-h | to-a | Depth | Wt (lb) | Cost |
|---|------|----|-----|------|------|-------|---------|------|
| 102 | Filthy Rag | 1 | 0d0 | 0 | −1 | 0 | 2.0 | 1 |
| 101 | Robe | 2 | 0d0 | 0 | 0 | 1 | 2.0 | 4 |
| 103 | Soft Leather armor | 4 | 0d0 | 0 | 0 | 3 | 8.0 | 18 |
| 104 | Soft Studded Leather | 5 | 1d1 | 0 | 0 | 3 | 9.0 | 35 |
| 105 | Hard Leather armor | 6 | 1d1 | −1 | 0 | 5 | 10.0 | 150 |
| 106 | Hard Studded Leather | 7 | 1d2 | −1 | 0 | 10 | 11.0 | 200 |
| 107 | Leather Scale Mail | 11 | 1d1 | −1 | 0 | 15 | 14.0 | 450 |

### 7.2 Ego Soft-Armor Enchantments

| Ego Suffix | Rating | Depth | Extra AC | pval | Cost | Flags |
|------------|--------|-------|----------|------|------|-------|
| of Resist Acid | 16 | 0 | — | — | 1,000 | RES_ACID, IGNORE_ACID |
| of Resist Lightning | 10 | 0 | — | — | 400 | RES_ELEC, IGNORE_ELEC |
| of Resist Fire | 14 | 0 | — | — | 800 | RES_FIRE, IGNORE_FIRE |
| of Resist Cold | 12 | 0 | — | — | 600 | RES_COLD, IGNORE_COLD |
| of Resistance | 20 | 0 | up to +10 | — | 12,500 | RES_ACID/ELEC/FIRE/COLD, IGNORE_ALL |
| of Elvenkind | 25 | 0 | up to +10 | up to +3 | 15,000 | STEALTH, RES_ALL_FOUR, IGNORE_ALL; +1 extra random flag |
| of Permanence | 30 | 0 | up to +20 | — | 30,000 | SUST_ALL, HOLD_LIFE, RES_ALL_FOUR, IGNORE_ALL *(Robes only)* |
| of Vulnerability | 0 | 0 | −50 | — | 0 | AGGRAVATE, LIGHT_CURSE *(cursed)* |

### 7.3 Artifact Light/Soft Armor

#### The Hard Leather armor of Himring
- **Base:** Hard Leather armor
- **P:** AC 6, 0d0, +0/+0/+15
- **Depth:** 50 | **Rarity:** 20 | **Wt:** 10.0 lb | **Cost:** 35,000
- **Flags:** RES_CHAOS, RES_NETHR, RES_POIS, ACTIVATE (Protect Evil, recharge 100+100)
- *"Contained within is the memory of unvanquished Himring, defiant fortress surrounded by the legions of Morgoth."*

#### The Soft Leather armor 'Hithlomir'
- **Base:** Soft Leather armor
- **P:** AC 4, 0d0, +0/+0/+20
- **pval:** +4 STEALTH
- **Depth:** 20 | **Rarity:** 3 | **Wt:** 8.0 lb | **Cost:** 45,000
- **Flags:** RES_ACID, RES_ELEC, RES_FIRE, RES_COLD, RES_DARK
- *"Familiar with the secret ways hidden in darkness — truly more than it appears."*

#### The Leather Scale Mail 'Thalkettoth'
- **Base:** Leather Scale Mail
- **P:** AC 11, 1d1, −1/+0/+25
- **pval:** +3 DEX, SPEED
- **Depth:** 20 | **Rarity:** 3 | **Wt:** 6.0 lb | **Cost:** 25,000
- **Flags:** RES_ACID, RES_SHARD
- *"An amazingly light tunic and skirt sewn with overlapping hardened leather scales."*

---

## 8. Body Armor — Hard Metal

Equipment slot: **body**. Heavy metal armors carry penalties to to-hit and impose spell-failure. They provide the highest base AC outside of Dragon Scale Mail.

### 8.1 Base Metal Armor (tval 37)

| # | Name | AC | Dmg | to-h | Depth | Wt (lb) | Cost | Notes |
|---|------|----|-----|------|-------|---------|------|-------|
| 108 | Metal Scale Mail | 13 | 1d4 | −2 | 25 | 25.0 | 550 | |
| 109 | Chain Mail | 14 | 1d4 | −2 | 25 | 22.0 | 750 | |
| 110 | Rusty Chain Mail | 14 | 1d4 | −5 | 25 | 20.0 | 550 | −8 to-ac; functionally cursed |
| 121 | Double Chain Mail | 16 | 1d4 | −2 | 30 | 25.0 | 850 | |
| 111 | Augmented Chain Mail | 16 | 1d4 | −2 | 30 | 27.0 | 900 | |
| 112 | Bar Chain Mail | 18 | 1d4 | −2 | 35 | 28.0 | 950 | |
| 113 | Metal Brigandine armor | 19 | 1d4 | −3 | 35 | 29.0 | 1,100 | |
| 114 | Partial Plate armor | 22 | 1d6 | −3 | 45 | 26.0 | 1,200 | |
| 115 | Metal Lamellar armor | 23 | 1d6 | −3 | 45 | 34.0 | 1,250 | |
| 116 | Full Plate armor | 25 | 2d4 | −3 | 45 | 38.0 | 1,350 | |
| 117 | Ribbed Plate armor | 28 | 2d4 | −3 | 50 | 38.0 | 1,500 | |
| 120 | Mithril Chain Mail | 28 | 1d4 | −1 | 55 | 15.0 | 7,000 | IGNORE_ACID |
| 119 | Mithril Plate Mail | 35 | 2d4 | −3 | 60 | 30.0 | 15,000 | IGNORE_ACID |
| 118 | Adamantite Plate Mail | 40 | 2d4 | −4 | 75 | 42.0 | 20,000 | IGNORE_ACID |

### 8.2 Ego Metal-Armor Enchantments

*(Same ego types as soft armor, plus one metal-only:)*

| Ego Suffix | Rating | Depth | Extra AC | pval | Cost | Flags | Restriction |
|------------|--------|-------|----------|------|------|-------|-------------|
| of Resist Acid | 16 | 0 | — | — | 1,000 | RES_ACID, IGNORE_ACID | Any |
| of Resist Lightning | 10 | 0 | — | — | 400 | RES_ELEC, IGNORE_ELEC | Any |
| of Resist Fire | 14 | 0 | — | — | 800 | RES_FIRE, IGNORE_FIRE | Any |
| of Resist Cold | 12 | 0 | — | — | 600 | RES_COLD, IGNORE_COLD | Any |
| of Resistance | 20 | 0 | up to +10 | — | 12,500 | RES_ALL_FOUR, IGNORE_ALL | Any |
| of Elvenkind | 25 | 0 | up to +10 | up to +3 | 15,000 | STEALTH, RES_ALL_FOUR, IGNORE_ALL | Any |
| (Dwarven) | 18 | 0 | up to +15 | up to +2 | 5,000 | STR, CON, INFRA, FREE_ACT, IGNORE_ACID/FIRE | sval 3–99 (non-rusty) |
| of Vulnerability | 0 | 0 | −50 | — | 0 | AGGRAVATE, LIGHT_CURSE | Any |

> **(Dwarven)** prefix: grants +STR, +CON, +INFRA, FREE_ACT. Appears only on hard metal armors (not Rusty Chain Mail).

### 8.3 Artifact Metal Armor

#### The Adamantite Plate Mail 'Soulkeeper'
- **Base:** Adamantite Plate Mail
- **P:** AC 40, 2d4, −4/+0/+20
- **pval:** +2 CON
- **Depth:** 75 | **Rarity:** 9 | **Wt:** 42.0 lb | **Cost:** 300,000
- **Flags:** HOLD_LIFE, SUST_CON, RES_ACID/COLD/DARK/NETHR/NEXUS/CHAOS/CONFU/FEAR, ACTIVATE (Heal, recharge 444)
- *"A suit of imperishable adamant, with unconquerable strength to endure evil and disruptive magics."*

#### The Full Plate armor of Isildur
- **Base:** Full Plate armor
- **P:** AC 25, 2d4, +0/+0/+25
- **pval:** +1 CON
- **Depth:** 30 | **Rarity:** 3 | **Wt:** 30.0 lb | **Cost:** 50,000
- **Flags:** RES_ACID/ELEC/FIRE/COLD/SOUND/CONFU/NEXUS
- *"A gleaming steel suit with runes of warding and stability deeply engraved into its surface."*

#### The Metal Brigandine armor of the Rohirrim
- **Base:** Metal Brigandine armor
- **P:** AC 19, 1d4, +0/+0/+15
- **pval:** +2 STR, DEX
- **Depth:** 30 | **Rarity:** 3 | **Wt:** 20.0 lb | **Cost:** 30,000
- **Flags:** RES_FEAR, RES_ACID/ELEC/FIRE/COLD/CONFU/SOUND
- *"Small metal plates over sturdy canvas, bearing scenes of hunting and war. The spirit of Eorl the Young around you."*

#### The Mithril Chain Mail 'Belegennon'
- **Base:** Mithril Chain Mail
- **P:** AC 28, 1d4, −1/+0/+20
- **pval:** +4 STEALTH
- **Depth:** 40 | **Rarity:** 3 | **Wt:** 15.0 lb | **Cost:** 105,000
- **Flags:** RES_ACID/ELEC/FIRE/COLD/POIS, ACTIVATE (Phase Door, recharge 2)
- *"Shimmers as though of pure silver. Stands untouched amidst the fury of the elements."*

#### The Mithril Plate Mail of Celeborn
- **Base:** Mithril Plate Mail
- **P:** AC 35, 2d4, −3/+0/+25
- **pval:** +4 STR, CHR
- **Depth:** 40 | **Rarity:** 3 | **Wt:** 25.0 lb | **Cost:** 150,000
- **Flags:** RES_ACID/ELEC/FIRE/COLD/DARK/DISEN, ACTIVATE (Banishment, recharge 500)
- *"A shimmering suit of true-silver, forged long ago by dwarven smiths of legend."*

#### The Chain Mail of Arvedui
- **Base:** Chain Mail
- **P:** AC 14, 1d4, −2/+0/+15
- **pval:** +2 STR, CHR
- **Depth:** 20 | **Rarity:** 3 | **Wt:** 22.0 lb | **Cost:** 32,000
- **Flags:** RES_ACID/ELEC/FIRE/COLD/SHARD/NEXUS
- *"You feel as strong and tall as Arvedui, last king of Arnor, as you put it on."*

#### The Augmented Chain Mail of Caspanion
- **Base:** Augmented Chain Mail
- **P:** AC 16, 1d4, −2/+0/+20
- **pval:** +3 INT, WIS, CON
- **Depth:** 25 | **Rarity:** 9 | **Wt:** 27.0 lb | **Cost:** 40,000
- **Flags:** RES_ACID/POIS/CONFU, ACTIVATE (Trap/Door Destruction, recharge 10)
- *"Strategically reinforced with a second layer of chain. No door can bar the wearer's path."*

---

## 9. Dragon Scale Mail

Equipment slot: **body**. Dragon Scale Mails (tval 38) are exceptional armor forged from the hides of powerful dragons. All base DSMs share the following traits:
- **AC:** 30 (Power DSM: 40)
- **Dmg:** 2d4
- **to-h:** −2 (Power DSM: −3)
- **Weight:** 20.0 lb
- **ACTIVATE** (breathe corresponding element)
- **IGNORE_ACID, IGNORE_ELEC, IGNORE_FIRE, IGNORE_COLD** (all immune to elemental destruction)

### 9.1 Dragon Scale Mail Types

| # | Name | Depth | Cost | Resistances Granted |
|---|------|-------|------|---------------------|
| 400 | Black Dragon Scale Mail | 50 | 40,000 | RES_ACID |
| 401 | Blue Dragon Scale Mail | 50 | 40,000 | RES_ELEC |
| 402 | White Dragon Scale Mail | 50 | 40,000 | RES_COLD |
| 403 | Red Dragon Scale Mail | 50 | 40,000 | RES_FIRE |
| 404 | Green Dragon Scale Mail | 60 | 60,000 | RES_POIS |
| 406 | Pseudo-Dragon Scale Mail | 65 | 60,000 | RES_LITE, RES_DARK |
| 408 | Bronze Dragon Scale Mail | 50 | 40,000 | RES_CONFU |
| 409 | Gold Dragon Scale Mail | 50 | 40,000 | RES_SOUND |
| 407 | Law Dragon Scale Mail | 80 | 80,000 | RES_SOUND, RES_SHARD |
| 410 | Chaos Dragon Scale Mail | 80 | 80,000 | RES_CHAOS, RES_DISEN |
| 405 | Multi-Hued Dragon Scale Mail | 90 | 150,000 | RES_ACID, RES_ELEC, RES_FIRE, RES_COLD, RES_POIS |
| 411 | Balance Dragon Scale Mail | 95 | 100,000 | RES_SOUND, RES_SHARD, RES_CHAOS, RES_DISEN |
| 412 | Power Dragon Scale Mail | 100 | 300,000 | **ALL** resists (acid/elec/fire/cold/pois/confu/disen/sound/shard/blind/lite/dark/nexus/nethr/chaos) |

> Power DSM has AC 40, −3 to-hit, unique in covering every resistance in the game.

### 9.2 Artifact Dragon Scale Mails

#### The Multi-Hued Dragon Scale Mail 'Razorback'
- **Base:** Multi-Hued DSM
- **P:** AC 30, 2d4, −4/+0/+25
- **Depth:** 90 | **Rarity:** 9 | **Wt:** 50.0 lb | **Cost:** 400,000
- **Flags:** FREE_ACT, IM_ELEC, RES_ELEC/ACID/FIRE/COLD/POIS/LITE/DARK, LITE, SEE_INVIS, AGGRAVATE, ACTIVATE (Star Ball, recharge 50)
- *"A massive suit deeply saturated with many colors. Throbs with angry energies and raw elemental might."*

#### The Power Dragon Scale Mail 'Bladeturner'
- **Base:** Power DSM
- **P:** AC 50, 2d4, −8/+0/+35
- **Depth:** 100 | **Rarity:** 16 | **Wt:** 60.0 lb | **Cost:** 500,000
- **Flags:** HOLD_LIFE, REGEN, ALL resists including fear, blindness, confusion, nether, nexus, chaos, disenchantment, shards, sound, ACTIVATE (Rage/Bless/Resist, recharge 400)
- *"A suit of adamant set with scales of every color, surrounded in a nimbus of perfectly mastered powers elemental and ethereal."*

#### The Balance Dragon Scale Mail 'Mediator'
- **Base:** Balance DSM
- **P:** AC 30, 2d4, −4/+0/+25
- **Depth:** 95 | **Rarity:** 12 | **Wt:** 50.0 lb | **Cost:** 400,000
- **Flags:** RES_CHAOS/DISEN/SHARD/SOUND/CONFU/NEXUS, AGGRAVATE, FREE_ACT, SLOW_DIGEST, REGEN, ACTIVATE (Star Ball, recharge 50)
- *"Even the mightiest wyrms of Law and Chaos fear the judgement of its wearer."*

---

## 10. Rings

Equipment slot: **fingers** (two slots available). Rings are **flavoured** at game start — their true identity is hidden until identified. Weight: 0.2 lb each.

> **Note on pval:** Rings with stat boosts (STR, DEX, etc.) and rings with to-hit/to-dam/to-ac bonuses have a *variable* pval assigned at generation. The base entries show the sign and flag; actual magnitude depends on generation level and luck.

### 10.1 Base Rings

| # | Name | Depth | Cost | pval | Flags | Notes |
|---|------|-------|------|------|-------|-------|
| 132 | Ring of Strength | 30 | 500 | varies | STR, SUST_STR | |
| 133 | Ring of Dexterity | 30 | 500 | varies | DEX, SUST_DEX | |
| 134 | Ring of Constitution | 30 | 500 | varies | CON, SUST_CON | |
| 135 | Ring of Intelligence | 30 | 500 | varies | INT, SUST_INT | |
| 136 | Ring of Speed | 75 | 100,000 | varies | SPEED | Most valuable generic ring |
| 137 | Ring of Searching | 5 | 250 | varies | SEARCH | |
| 138 | Ring of Teleportation | 5 | 0 | — | LIGHT_CURSE, TELEPORT | *Cursed* |
| 139 | Ring of Slow Digestion | 5 | 250 | — | SLOW_DIGEST | |
| 140 | Ring of Resist Fire | 10 | 250 | — | RES_FIRE, IGNORE_FIRE | |
| 141 | Ring of Resist Cold | 10 | 250 | — | RES_COLD, IGNORE_COLD | |
| 142 | Ring of Feather Falling | 5 | 200 | — | FEATHER | |
| 143 | Ring of Resist Poison | 40 | 16,000 | — | RES_POIS | |
| 144 | Ring of Free Action | 20 | 1,500 | — | FREE_ACT | |
| 145 | Ring of Weakness | 5 | 0 | −5 | LIGHT_CURSE, STR | *Cursed* |
| 146 | Ring of Flames | 50 | 3,000 | — | RES_FIRE, IGNORE_FIRE, ACTIVATE | +15 to-ac; ACTIVATE (Fire Ball) |
| 147 | Ring of Acid | 50 | 3,000 | — | RES_ACID, IGNORE_ACID, ACTIVATE | +15 to-ac; ACTIVATE (Acid Ball) |
| 148 | Ring of Ice | 50 | 3,000 | — | RES_COLD, IGNORE_COLD, ACTIVATE | +15 to-ac; ACTIVATE (Ice Ball) |
| 149 | Ring of Woe | 50 | 0 | −5 | LIGHT_CURSE, TELEPORT, WIS, CHR | *Cursed* |
| 150 | Ring of Stupidity | 5 | 0 | −5 | LIGHT_CURSE, INT | *Cursed* |
| 151 | Ring of Damage | 20 | 500 | varies | — | Boosts to-dam |
| 152 | Ring of Accuracy | 20 | 500 | varies | — | Boosts to-hit |
| 153 | Ring of Protection | 10 | 500 | varies | — | Boosts to-ac |
| 154 | Ring of Aggravate Monster | 5 | 0 | — | LIGHT_CURSE, AGGRAVATE | *Cursed* |
| 155 | Ring of See Invisible | 30 | 340 | — | SEE_INVIS | |
| 156 | Ring of Sustain Strength | 20 | 400 | — | SUST_STR | |
| 157 | Ring of Sustain Intelligence | 20 | 400 | — | SUST_INT | |
| 158 | Ring of Sustain Wisdom | 20 | 400 | — | SUST_WIS | |
| 159 | Ring of Sustain Constitution | 20 | 400 | — | SUST_CON | |
| 160 | Ring of Sustain Dexterity | 20 | 400 | — | SUST_DEX | |
| 161 | Ring of Sustain Charisma | 20 | 400 | — | SUST_CHR | |
| 162 | Ring of Slaying | 40 | 1,000 | varies | SHOW_MODS | Boosts to-hit and to-dam |

### 10.2 Artifact Rings

#### The Ring of Barahir
- **P:** +0/+0/+0
- **pval:** +1 all stats (STR/INT/WIS/DEX/CON/CHR) + STEALTH
- **Depth:** 50 | **Rarity:** 25 | **Cost:** 65,000
- **Flags:** RES_POIS, RES_DARK
- *"Twinned serpents with eyes of emerald meeting beneath a crown of flowers, an ancient treasure of Isildur's house."*

#### The Ring of Tulkas
- **pval:** +4 STR, DEX, CON
- **Depth:** 70 | **Rarity:** 50 | **Cost:** 150,000
- **Flags:** RES_FEAR, ACTIVATE (Haste, recharge 150)
- *"The treasure of Tulkas, most fleet and wrathful of the Valar."*

#### The Ring of Power 'Narya' *(one of the Three)*
- **P:** to-h +6, to-d +6
- **pval:** +1 all stats + SPEED
- **Depth:** 70 | **Rarity:** 60 | **Cost:** 100,000
- **Flags:** FREE_ACT, SEE_INVIS, SLOW_DIGEST, REGEN, SUST_STR/CON/WIS/CHR, IM_FIRE, RES_FIRE, RES_NETHR, RES_FEAR, ACTIVATE (Fire Ball III, recharge 20)
- *"The Ring of Fire, set with a ruby that glows like flame."*

#### The Ring of Power 'Nenya' *(one of the Three)*
- **P:** to-h +8, to-d +8
- **pval:** +2 all stats + SPEED
- **Depth:** 80 | **Rarity:** 70 | **Cost:** 200,000
- **Flags:** HOLD_LIFE, FREE_ACT, SEE_INVIS, FEATHER, REGEN, SUST_INT/WIS/CHR, IM_COLD, RES_COLD, RES_BLIND, TELEPATHY, ACTIVATE (Frost Ball V, recharge 20)
- *"The Ring of Adamant, with a pure white stone as centerpiece."*

#### The Ring of Power 'Vilya' *(one of the Three)*
- **P:** to-h +10, to-d +10
- **pval:** +3 all stats + SPEED
- **Depth:** 90 | **Rarity:** 80 | **Cost:** 300,000
- **Flags:** HOLD_LIFE, FREE_ACT, SEE_INVIS, FEATHER, SLOW_DIGEST, REGEN, SUST_STR/DEX/CON, IM_ELEC, RES_ELEC, RES_POIS, RES_DISEN, ACTIVATE (Lightning Ball II, recharge 20)
- *"The Ring of Sapphire, with clear blue gems that shine like stars — the mightiest of the Three."*

#### The Ring of Power 'The One Ring' *(perma-cursed)*
- **P:** to-h +15, to-d +15
- **pval:** +5 all stats + SPEED
- **Depth:** 100 | **Rarity:** 100 | **Cost:** 5,000,000
- **Flags:** LIGHT_CURSE, HEAVY_CURSE, PERMA_CURSE, AGGRAVATE, DRAIN_EXP, SEE_INVIS, REGEN, IM_FIRE/COLD/ELEC/ACID, RES_POIS/NETHR/BLIND/DISEN/FEAR, TELEPATHY, SUST_ALL, ACTIVATE (Bizarre)
- *"One Ring to rule them all … Made of massive gold, set with runes in the foul speech of Mordor."*

---

## 11. Amulets

Equipment slot: **neck** (one slot). Amulets are **flavoured** — identity hidden until identified. Weight: 0.3 lb each.

### 11.1 Base Amulets

| # | Name | Depth | Cost | pval | Flags | Notes |
|---|------|-------|------|------|-------|-------|
| 163 | Amulet of Wisdom | 30 | 500 | varies | WIS, SUST_WIS | |
| 164 | Amulet of Charisma | 30 | 500 | varies | CHR, SUST_CHR | |
| 165 | Amulet of Searching | 15 | 600 | varies | SEARCH | |
| 166 | Amulet of Teleportation | 10 | 0 | — | LIGHT_CURSE, TELEPORT | *Cursed* |
| 167 | Amulet of Slow Digestion | 15 | 200 | — | SLOW_DIGEST | |
| 168 | Amulet of Resist Acid | 10 | 300 | — | RES_ACID, IGNORE_ACID | |
| 169 | Amulet of Adornment | 10 | 20 | — | — | No special powers |
| 170 | Amulet of Sustenance | 60 | 20,000 | — | SUST_ALL, HOLD_LIFE, SLOW_DIGEST, IGNORE_ALL | All six sustains + life hold |
| 171 | Amulet of the Magi | 70 | 30,000 | varies | FREE_ACT, SEE_INVIS, SEARCH, SUST_INT, INT, RES_CONFU, IGNORE_ALL | +3 to-ac; best generic amulet |
| 172 | Amulet of DOOM | 50 | 0 | −5 | LIGHT_CURSE, all stats | *Cursed* — reduces all six stats |

### 11.2 Artifact Amulets

#### The Amulet of Carlammas
- **pval:** +2 CON
- **Depth:** 50 | **Rarity:** 10 | **Cost:** 60,000
- **Flags:** RES_FIRE, ACTIVATE (Protection from Evil, recharge 225)
- *"A fiery circle of bronze with mighty spells to ward off and banish evil."*

#### The Amulet of Ingwe
- **pval:** +3 INT, WIS, CHR, INFRA
- **Depth:** 65 | **Rarity:** 30 | **Cost:** 90,000
- **Flags:** SEE_INVIS, FREE_ACT, RES_ACID/COLD/ELEC, ACTIVATE (Dispel Evil, recharge 50)
- *"The ancient heirloom of Ingwe, high lord of the Vanyar, against whom nothing of evil could stand."*

#### The Necklace of the Dwarves
- **pval:** +3 STR, CON, INFRA
- **Depth:** 70 | **Rarity:** 50 | **Cost:** 75,000
- **Flags:** SEE_INVIS, FREE_ACT, REGEN, LITE, RES_FEAR
- *"The Nauglamir; a carcanet of gold set with a multitude of shining gems of Valinor, accenting a radiant Silmaril."*

#### The Elfstone 'Elessar'
- **P:** to-h +7, to-d +7, to-a +10
- **pval:** +2 STR, WIS, CHR, SPEED
- **Depth:** 60 | **Rarity:** 60 | **Cost:** 40,000
- **Flags:** RES_FEAR, RES_FIRE, RES_POIS, ACTIVATE (Heal, recharge 200)
- *"A green stone imbued with the power of Elvendom, fit to be borne by a true King of Men."*

#### The Jewel 'Evenstar'
- **Depth:** 40 | **Rarity:** 40 | **Cost:** 25,000
- **Flags:** HOLD_LIFE, SUST_CON/WIS/INT, RES_DARK, RES_COLD, ACTIVATE (Restore Life, recharge 150)
- *"A plain white jewel, given by Queen Arwen to Frodo Baggins before his return to the Shire."*

#### The Amulet of the Magi 'of Westernesse' *(Palantir)*
- **pval:** +2 INT, WIS, SEARCH, INFRA
- **Depth:** 75 | **Rarity:** 60 | **Wt:** 20.0 lb | **Cost:** 100,000
- **P:** 0, 10d10 (can be used as weapon)
- **Flags:** SEE_INVIS, TELEPATHY, RES_CHAOS, RES_BLIND, AGGRAVATE, DRAIN_EXP, ACTIVATE (Clairvoyance, recharge 50)
- *"A great globe with a heart of fire — provides sight of far places at a cost: those espied upon are aware of it."*

---

## 12. Ego-Item Enchantments

Ego items are named enchantments that appear on base items. The same ego name may apply to different item categories with different effects.

### 12.1 Body Armor Egos (tval 36–37)

| Name | Rating | Flags | Extra | Applies To |
|------|--------|-------|-------|-----------|
| of Resist Acid | 16 | RES_ACID, IGNORE_ACID | — | All body armor |
| of Resist Lightning | 10 | RES_ELEC, IGNORE_ELEC | — | All body armor |
| of Resist Fire | 14 | RES_FIRE, IGNORE_FIRE | — | All body armor |
| of Resist Cold | 12 | RES_COLD, IGNORE_COLD | — | All body armor |
| of Resistance | 20 | RES_ALL_FOUR, IGNORE_ALL | +AC up to 10 | All body armor |
| of Elvenkind | 25 | STEALTH(+3), RES_ALL_FOUR, IGNORE_ALL | +AC up to 10, +1 rand flag | All body armor |
| of Permanence | 30 | SUST_ALL, HOLD_LIFE, RES_ALL_FOUR, IGNORE_ALL | +AC up to 20 | Robes only |
| (Dwarven) | 18 | STR, CON, INFRA(+2), FREE_ACT, IGNORE_ACID/FIRE | +AC up to 15 | Heavy metal only |
| of Vulnerability | 0 | AGGRAVATE, LIGHT_CURSE | −50 AC | All |

### 12.2 Shield Egos (tval 34)

| Name | Rating | Flags | Extra |
|------|--------|-------|-------|
| of Resist Acid | 16 | RES_ACID, IGNORE_ACID | — |
| of Resist Lightning | 10 | RES_ELEC, IGNORE_ELEC | — |
| of Resist Fire | 14 | RES_FIRE, IGNORE_FIRE | — |
| of Resist Cold | 12 | RES_COLD, IGNORE_COLD | — |
| of Resistance | 20 | RES_ALL_FOUR, IGNORE_ALL | +AC up to 10 |
| of Elvenkind | 25 | STEALTH, RES_ALL_FOUR, IGNORE_ALL | +AC up to 10, +1 rand flag |
| of Preservation | 25 | RES_DISEN, SUST_STR/CON/DEX, HOLD_LIFE, IGNORE_ALL | +AC up to 20 |
| of Vulnerability | 0 | AGGRAVATE, LIGHT_CURSE | −50 AC |

### 12.3 Helm & Crown Egos (tval 32–33)

| Name | Rating | Flags | Extra | Applies To |
|------|--------|-------|-------|-----------|
| of Intelligence | 13 | INT, SUST_INT | +INT up to 2 | Helms |
| of Wisdom | 13 | WIS, SUST_WIS | +WIS up to 2 | Helms |
| of Beauty | 8 | CHR, SUST_CHR | +CHR up to 4 | Helms |
| of Seeing | 8 | SEARCH, RES_BLIND, SEE_INVIS | +SEARCH up to 5 | Helms + Crowns |
| of Infravision | 11 | INFRA | +INFRA up to 5 | Helms only |
| of Light | 6 | LITE, RES_LITE | — | Helms only |
| of Telepathy | 20 | TELEPATHY | — | Helms + Crowns |
| of Regeneration | 10 | REGEN | — | Helms + Crowns |
| of Teleportation | 0 | TELEPORT, LIGHT_CURSE | — | Helms only *(cursed)* |
| of the Magi | 15 | INT, SUST_INT, RES_ALL_FOUR, IGNORE_ALL | +INT up to 3; +1 rand flag | Crowns only |
| of Might | 19 | STR/DEX/CON + SUST_ALL_THREE, FREE_ACT, IGNORE_ALL | +3 all three | Crowns only |
| of Lordliness | 17 | WIS, CHR, SUST_WIS/CHR, RES_FEAR, IGNORE_ALL | +3 WIS/CHR; +1 rand flag | Crowns only |
| of Serenity | 20 | RES_SOUND, RES_CONFU, RES_FEAR | — | Crowns only |
| of Night and Day | 18 | RES_LITE/DARK, LITE, SEE_INVIS, RES_BLIND, IGNORE_ACID | — | Crowns only (depth 35) |
| of Dullness | 0 | INT, WIS, CHR, LIGHT_CURSE | −5 pval | *(cursed)* |
| of Sickliness | 0 | STR, DEX, CON, LIGHT_CURSE | −5 pval | Crowns only *(cursed)* |

### 12.4 Cloak Egos (tval 35)

| Name | Rating | Flags | Bonus |
|------|--------|-------|-------|
| of Protection | 10 | RES_SHARD, IGNORE_ALL | +AC up to 10 |
| of Stealth | 10 | STEALTH | +STEALTH up to 3 |
| of Aman | 20 | STEALTH, IGNORE_ALL | +AC up to 20, +STEALTH up to 3; +1 rand flag |
| of the Magi | 15 | INT, SUST_INT, STEALTH, IGNORE_ACID | +INT/STEALTH up to 2; +AC up to 4 |
| of Enveloping | 0 | SHOW_MODS, LIGHT_CURSE | Up to −10 to-h/d *(cursed)* |
| of Vulnerability | 0 | AGGRAVATE, LIGHT_CURSE | −50 AC *(cursed)* |
| of Irritation | 0 | AGGRAVATE, SHOW_MODS, LIGHT_CURSE | Up to −15 to-h/d *(cursed)* |

### 12.5 Glove Egos (tval 31)

| Name | Rating | Flags | Bonus | Restriction |
|------|--------|-------|-------|------------|
| of Free Action | 11 | FREE_ACT | — | All |
| of Slaying | 17 | SHOW_MODS | Up to +5 to-h/d | All |
| of Agility | 14 | DEX | +DEX up to 5 | All |
| of Power | 22 | STR | Up to +5 to-h/d, +5 STR | All |
| of Thievery | 22 | DEX, SEARCH, FEATHER, FREE_ACT | Up to +8 to-h/+3 to-d, +5 DEX | Leather Gloves only |
| of Combat | 22 | STR, CON, AGGRAVATE | Up to +3 to-h/+8 to-d, +2 STR/CON | Gauntlets/Cesti only |
| of Weakness | 0 | STR, LIGHT_CURSE | −10 STR *(cursed)* | All |
| of Clumsiness | 0 | DEX, LIGHT_CURSE | −10 DEX *(cursed)* | All |

### 12.6 Boot Egos (tval 30)

| Name | Rating | Flags | Bonus | Restriction |
|------|--------|-------|-------|------------|
| of Slow Descent | 7 | FEATHER | — | All |
| of Stealth | 16 | STEALTH | +STEALTH up to 3 | All |
| of Free Action | 15 | FREE_ACT | — | All |
| of Speed | 25 | SPEED | +SPEED up to 10 | All |
| of Stability | 20 | RES_NEXUS, FEATHER | — | All |
| of Elvenkind | 30 | STEALTH, SPEED, FEATHER, IGNORE_ACID/FIRE | +SPEED/STEALTH up to 5 | Hard/Metal Shod only |
| of Slowness | 0 | SPEED, LIGHT_CURSE | −5 SPEED *(cursed)* | All |
| of Annoyance | 0 | SPEED, STEALTH, AGGRAVATE, LIGHT_CURSE | −10 pval *(cursed)* | All |

---

## 13. Named Artifacts

Complete reference table for all wearable named artifacts in MAngband 1.5.3.

### 13.1 Special Wearables (Rings & Amulets)

| Artifact | Slot | AC | Key Bonuses | Key Flags | Depth |
|----------|------|----|-------------|-----------|-------|
| Ring of Barahir | Finger | — | +1 all stats/stealth | RES_POIS, RES_DARK | 50 |
| Ring of Tulkas | Finger | — | +4 STR/DEX/CON | RES_FEAR, ACTIVATE(Haste) | 70 |
| Ring 'Narya' | Finger | — | +1 all+SPEED, +6 to-h/d | IM_FIRE, RES_FIRE/NETHR/FEAR, ACTIVATE | 70 |
| Ring 'Nenya' | Finger | — | +2 all+SPEED, +8 to-h/d | IM_COLD, TELEPATHY, HOLD_LIFE, ACTIVATE | 80 |
| Ring 'Vilya' | Finger | — | +3 all+SPEED, +10 to-h/d | IM_ELEC, RES_DISEN/POIS, HOLD_LIFE, ACTIVATE | 90 |
| The One Ring | Finger | — | +5 all+SPEED, +15 to-h/d | All IMs, perma-cursed, AGGRAVATE, DRAIN_EXP | 100 |
| Amulet of Carlammas | Neck | — | +2 CON | RES_FIRE, ACTIVATE | 50 |
| Amulet of Ingwe | Neck | — | +3 INT/WIS/CHR/INFRA | FREE_ACT, SEE_INVIS, RES_ACID/COLD/ELEC, ACTIVATE | 65 |
| Necklace of the Dwarves | Neck | — | +3 STR/CON/INFRA | FREE_ACT, SEE_INVIS, REGEN, LITE, RES_FEAR | 70 |
| Elfstone 'Elessar' | Neck | +10 | +2 STR/WIS/CHR/SPEED, +7 to-h/d | RES_FEAR/FIRE/POIS, ACTIVATE | 60 |
| Jewel 'Evenstar' | Neck | — | — | HOLD_LIFE, SUST_CON/WIS/INT, RES_DARK/COLD, ACTIVATE | 40 |
| Palantir of Westernesse | Neck | — | +2 INT/WIS/SEARCH/INFRA | TELEPATHY, DRAIN_EXP, AGGRAVATE, ACTIVATE | 75 |

### 13.2 Armor Artifacts

| Artifact | Type | Base AC | to-a | Key Bonuses | Key Flags | Depth |
|----------|------|---------|------|-------------|-----------|-------|
| 'Razorback' | Multi-Hued DSM | 30 | +25 | — | IM_ELEC, 5×RES, LITE, SEE_INVIS, AGGRAVATE, ACTIVATE | 90 |
| 'Bladeturner' | Power DSM | 50 | +35 | — | ALL resists, HOLD_LIFE, REGEN, ACTIVATE | 100 |
| 'Mediator' | Balance DSM | 30 | +25 | — | RES_CHAOS/DISEN/SHARD/SOUND/CONFU/NEXUS, FREE_ACT, REGEN, ACTIVATE | 95 |
| 'Soulkeeper' | Adamantite PM | 40 | +20 | +2 CON | HOLD_LIFE, 8×RES, ACTIVATE(Heal) | 75 |
| of Isildur | Full Plate | 25 | +25 | +1 CON | 7×RES | 30 |
| of the Rohirrim | Metal Brigandine | 19 | +15 | +2 STR/DEX | RES_FEAR, 6×RES | 30 |
| 'Belegennon' | Mithril Chain | 28 | +20 | +4 STEALTH | 5×RES, ACTIVATE(Phase) | 40 |
| of Celeborn | Mithril Plate | 35 | +25 | +4 STR/CHR | 6×RES+DISEN, ACTIVATE(Banishment) | 40 |
| of Arvedui | Chain Mail | 14 | +15 | +2 STR/CHR | 6×RES | 20 |
| of Caspanion | Aug. Chain Mail | 16 | +20 | +3 INT/WIS/CON | RES_ACID/POIS/CONFU, ACTIVATE | 25 |
| of Himring | Hard Leather | 6 | +15 | — | RES_CHAOS/NETHR/POIS, ACTIVATE | 50 |
| 'Hithlomir' | Soft Leather | 4 | +20 | +4 STEALTH | RES_ACID/ELEC/FIRE/COLD/DARK | 20 |
| 'Thalkettoth' | Leather Scale | 11 | +25 | +3 DEX/SPEED | RES_ACID/SHARD | 20 |

### 13.3 Shield Artifacts

| Artifact | Type | Base AC | to-a | Key Bonuses | Key Flags | Depth |
|----------|------|---------|------|-------------|-----------|-------|
| of Gil-galad | Shield of Deflection | 10 | +20 | +5 WIS/CHR | LITE, RES_DISEN/ELEC/ACID/DARK, SUST×3, ACTIVATE | 70 |
| of Thorin | Small Metal Shield | 3 | +25 | +4 STR/CON | IM_ACID, RES_FEAR/SOUND/CHAOS, FREE_ACT | 40 |
| of Celegorm | Large Leather Shield | 4 | +20 | — | 6×RES | 30 |
| of Anarion | Large Metal Shield | 5 | +20 | — | 4×RES, SUST_ALL_SIX | 40 |

### 13.4 Helm & Crown Artifacts

| Artifact | Type | Base AC | to-a | Key Bonuses | Key Flags | Depth |
|----------|------|---------|------|-------------|-----------|-------|
| of Celebrimbor | Metal Cap | 3 | +18 | +3 INT/DEX/CHR/SEARCH | RES_FIRE/ACID/DISEN/SHARD | 55 |
| Crown of Morgoth | Iron Crown | 0 | — | +125 all stats | ALL resists, PERMA_CURSE, INSTA_ART | 100 |
| Crown of Beruthiel | Iron Crown | 0 | +20 | −5 STR/DEX/CON | TELEPATHY, SEE_INVIS, LIGHT/HEAVY_CURSE | 40 |
| Cap of Thranduil | Hard Leather Cap | 2 | +10 | +2 INT/WIS | RES_BLIND, TELEPATHY | 20 |
| Cap of Thengel | Metal Cap | 3 | +12 | +3 WIS/CHR | RES_CONFU | 10 |
| Helm of Hammerhand | Steel Helm | 6 | +20 | +3 STR/DEX/CON | SUST×3, AGGRAVATE, RES_ACID/NEXUS/COLD/DARK | 20 |
| Helm of Dor-Lomin | Iron Helm | 5 | +20 | +4 STR/DEX/CON | RES_FEAR, 4×RES, LITE, SEE_INVIS, TELEPATHY | 40 |
| 'Holhenneth' | Iron Helm | 5 | +10 | +2 INT/WIS/SEARCH | RES_BLIND/CONFU, SEE_INVIS, ACTIVATE(Detect) | 20 |
| Helm of Gorlim | Iron Helm | 5 | +10 | −5 INT/WIS/SEARCH | SEE_INVIS, FREE_ACT, AGGRAVATE, HEAVY_CURSE | 20 |
| Crown of Gondor | Golden Crown | 0 | +15 | +3 STR/WIS/CON/SPEED | 7×RES, LITE, SEE_INVIS, REGEN, ACTIVATE(Heal) | 40 |
| Crown of Numenor | Jewel Crown | 0 | +18 | +3 INT/DEX/CHR/SEARCH/SPEED | SEE_INVIS, FREE_ACT, 6×RES, LITE | 60 |

### 13.5 Cloak Artifacts

| Artifact | Type | Base AC | to-a | Key Bonuses | Key Flags | Depth |
|----------|------|---------|------|-------------|-----------|-------|
| 'Colluin' | Cloak | 1 | +15 | — | RES_ACID/ELEC/FIRE/COLD/POIS, ACTIVATE | 5 |
| 'Holcolleth' | Cloak | 1 | +4 | +2 INT/WIS/SPEED/STEALTH | RES_ACID, ACTIVATE(Sleep) | 5 |
| of Thingol | Cloak | 1 | +18 | +3 DEX/CHR | FREE_ACT, RES_ACID/FIRE/COLD, ACTIVATE | 5 |
| of Thorongil | Cloak | 1 | +10 | — | FREE_ACT, RES_ACID, RES_FEAR, SEE_INVIS | 5 |
| 'Colannon' | Cloak | 1 | +15 | +3 STEALTH/SPEED | RES_NEXUS, ACTIVATE(Teleport) | 5 |
| of Luthien | Shadow Cloak | 6 | +20 | +2 INT/WIS/CHR/SPEED/STEALTH | RES_ACID/FIRE/COLD, ACTIVATE | 40 |
| of Tuor | Shadow Cloak | 6 | +12 | +4 STEALTH/DEX | IM_ACID, RES_ACID, FREE_ACT, SEE_INVIS | 40 |

### 13.6 Glove Artifacts

| Artifact | Type | Base AC | to-h/d | Key Bonuses | Key Flags | Depth |
|----------|------|---------|--------|-------------|-----------|-------|
| Gauntlets of Eol | Gauntlets | 2 | −5/−5 to-a+15 | +3 INT | FREE_ACT, FEATHER, RES_ELEC/DARK/POIS, AGGRAVATE, ACTIVATE | 55 |
| 'Cambeleg' | Leather Gloves | 1 | +8/+8 to-a+15 | +2 STR/CON | FREE_ACT | 10 |
| 'Cammithrim' | Leather Gloves | 1 | — to-a+10 | — | FREE_ACT, RES_LITE, SUST_CON, LITE, ACTIVATE(Missile) | 10 |
| 'Paurhach' | Gauntlets | 2 | — to-a+15 | — | RES_FIRE, REGEN, ACTIVATE(Fire Ball) | 10 |
| 'Paurnimmen' | Gauntlets | 2 | — to-a+15 | — | RES_COLD, SLOW_DIGEST, ACTIVATE(Frost Ball) | 10 |
| 'Pauraegen' | Gauntlets | 2 | — to-a+15 | — | RES_ELEC, LITE, ACTIVATE(Lightning Bolt) | 10 |
| 'Paurnen' | Gauntlets | 2 | — to-a+15 | — | RES_ACID, FEATHER, ACTIVATE(Acid Ball) | 10 |
| 'Camlost' | Gauntlets | 2 | −12/−12 | −3 STR/DEX | RES_FIRE/DISEN, FREE_ACT, DRAIN_EXP, HEAVY_CURSE | 10 |
| of Fingolfin | Cesti | 5 | +10/+10 to-a+20 | +4 DEX | FREE_ACT, RES_ACID, ACTIVATE(Arrow) | 40 |

### 13.7 Boot Artifacts

| Artifact | Type | Base AC | to-a | Key Bonuses | Key Flags | Depth |
|----------|------|---------|------|-------------|-----------|-------|
| Boots of Feanor | Hard Leather | 3 | +20 | +15 SPEED | RES_NEXUS, ACTIVATE(Haste) | 40 |
| 'Dal-i-thalion' | Soft Leather | 2 | +15 | +5 DEX | FREE_ACT, SUST_CON, RES_NETHR/CHAOS/CONFU, ACTIVATE | 10 |
| Boots of Thror | Metal Shod | 6 | +20 | +3 STR/CON/SPEED | RES_FEAR | 30 |
| Boots of Wormtongue | Soft Leather | 2 | — | +3 INT/DEX/STEALTH/SPEED, FEATHER; −8 to-h/d | LIGHT_CURSE, ACTIVATE(Phase) | 10 |

---

## Flag Glossary

| Flag | Effect |
|------|--------|
| **STR / INT / WIS / DEX / CON / CHR** | Boosts the corresponding stat by pval (may be positive or negative) |
| **SPEED** | Boosts speed (energy per turn) by pval; +10 ≈ one extra action per 10 turns |
| **STEALTH** | Increases stealth skill; reduces monster detection range |
| **SEARCH** | Increases search (trap/door detection) skill |
| **INFRA** | Increases infravision range in feet |
| **BLOWS** | Grants additional melee attacks per turn |
| **SUST_STR / SUST_INT / …** | Prevents the corresponding stat from being drained |
| **HOLD_LIFE** | Prevents experience drain (partial protection) |
| **FREE_ACT** | Immunity to paralysis and most slow/hold effects |
| **SEE_INVIS** | Allows seeing invisible monsters |
| **TELEPATHY** | Senses all nearby monsters through walls |
| **SLOW_DIGEST** | Reduces hunger rate |
| **REGEN** | Regenerates HP and MP faster |
| **FEATHER** | Feather-fall (avoids falling damage from traps/pits) |
| **LITE** | Provides a permanent light radius (+1) |
| **TELEPORT** | Random teleportation trigger (cursed effect) |
| **AGGRAVATE** | Wakes all nearby monsters |
| **DRAIN_EXP** | Continuously drains experience |
| **LIGHT_CURSE** | Item is lightly cursed (can be removed by Remove Curse) |
| **HEAVY_CURSE** | Item is heavily cursed (requires *Remove Curse*) |
| **PERMA_CURSE** | Item cannot be uncursed under any circumstances |
| **SHOW_MODS** | Always displays to-hit/to-dam modifiers |
| **HIDE_TYPE** | Hides the pval bonus type label |
| **RES_ACID** | Resistance to acid (reduces damage by ~2/3, immune to item destruction) |
| **RES_ELEC** | Resistance to electricity |
| **RES_FIRE** | Resistance to fire |
| **RES_COLD** | Resistance to cold |
| **RES_POIS** | Resistance to poison |
| **RES_FEAR** | Immunity to magical fear |
| **RES_LITE** | Resistance to light damage |
| **RES_DARK** | Resistance to darkness damage |
| **RES_BLIND** | Resistance to blindness |
| **RES_CONFU** | Resistance to confusion |
| **RES_SOUND** | Resistance to sound/stunning |
| **RES_SHARD** | Resistance to shards (cuts) |
| **RES_NEXUS** | Resistance to nexus (stat/level shuffling) |
| **RES_NETHR** | Resistance to nether (life drain) |
| **RES_CHAOS** | Resistance to chaos (mutation/confusion) |
| **RES_DISEN** | Resistance to disenchantment (item bonus drain) |
| **IM_ACID / IM_ELEC / IM_FIRE / IM_COLD** | Full immunity to that element |
| **IGNORE_ACID / …** | Item itself cannot be destroyed by that element |
| **ACTIVATE** | Item can be activated for a special magical effect |
| **INSTA_ART** | Always generates as a specific named artifact |

---

*Compiled from MAngband 1.5.3 reference data: `lib/edit/object.txt`, `lib/edit/ego_item.txt`, `lib/edit/artifact.txt`.*

---

## Implementation Status

All items and mechanics from this compendium have been audited against the IronHell codebase. Summary:

### Data (Complete)
- **95 base wearable items** (54 armors + 41 accessories) — all implemented in `generatedWearables.ts`
- **56 wearable ego items** — all present in `ego_items.json`
- **60 wearable artifacts** — all present in `artifacts.json` (125 total)

### Mechanics (Implemented)
| Flag/Mechanic | System File | Status |
|---|---|---|
| FREE_ACT (blocks paralysis/slow) | `blowEffectApplication.ts` | ✅ Working |
| HOLD_LIFE (blocks XP drain) | `blowEffectApplication.ts` | ✅ Working |
| RES_FEAR (blocks fear) | `blowEffects.ts` | ✅ Working |
| FEATHER (negates pit damage) | `trapMechanics.ts` | ✅ Working |
| SLOW_DIGEST (halves hunger) | `progression.ts` | ✅ Working |
| TELEPATHY (see monsters) | `renderFrame.ts` | ✅ Working |
| AGGRAVATE (auto-detection) | `stealthDetection.ts` | ✅ Working |
| LITE (+1 lightRadius) | `characterCreation.ts` | ✅ Working |
| DRAIN_EXP (10% per-tick) | `progression.ts` / `storeHelpers.ts` | ✅ Working |
| TELEPORT curse (1% per-tick) | `progression.ts` / `storeHelpers.ts` | ✅ Working |
| IM_FIRE/COLD/ELEC/ACID | `resistances.ts` | ✅ Mapped + `applyDamageReduction` |
| SUST_STR/DEX/CON/INT/WIS/CHR | `sustainStats.ts` | ✅ Working |
| REGEN | `characterCreation.ts` | ✅ Working |
| Caster helm penalty | `classFeatures.ts` / `characterCreation.ts` | ✅ Working |
| Stealth from equipment | `characterCreation.ts` | ✅ Working |
| Search from equipment | `characterCreation.ts` | ✅ Working |
| Curse system (light/heavy/perma) | `itemMagic.ts` | ✅ Working |
| Flavored items (ring/amulet) | `itemMagic.ts` | ✅ Working |
| All 63 item activations | `itemActivation.ts` | ✅ Working |

### Mechanics (Deferred / Low Priority)
| Flag/Mechanic | Reason |
|---|---|
| IGNORE_ACID/ELEC/FIRE/COLD | Item destruction by element subsystem not yet ported |
| SHOW_MODS / HIDE_TYPE | Cosmetic display flags — no gameplay impact |
| BLOWS (extra attacks) | Weapon-only mechanic; wearable items don't carry this flag |
