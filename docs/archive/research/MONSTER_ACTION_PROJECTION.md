# Monster Action Projection

Date: 2026-08-03  
Scope: G-03 and G-04 monster blow and spell projection research

## Objective

Determine whether monster behaviors can share canonical action vocabulary and where policy separation is mandatory.

## Evidence Basis

- Monster data grammar B and S lines: [reference-mangband-1_5_3/lib/edit/monster.txt](reference-mangband-1_5_3/lib/edit/monster.txt#L11)
- Parser for blows, spell flags, and 1_IN_X frequency: [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c#L2929)
- Blow method and blow effect constants: [reference-mangband-1_5_3/src/server/mdefines.h](reference-mangband-1_5_3/src/server/mdefines.h#L2234)
- Blow runtime dispatch and side effects: [reference-mangband-1_5_3/src/server/melee1.c](reference-mangband-1_5_3/src/server/melee1.c#L243)
- Spell selection policy and effectiveness filtering: [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L47)
- Spell family projection labels: [reference-mangband-1_5_3/src/server/monster1.c](reference-mangband-1_5_3/src/server/monster1.c#L125)

Classification:
- Direct Action Candidate
- Action + Policy
- Not Safely Projectable

## 1. Blow System Projection

## 1.1 Blow Representation

Original representation:
- B line defines method, effect, and damage dice.
- Up to MONSTER_BLOW_MAX blows.

Runtime behavior:
- Method controls messaging and auxiliary cut or stun profile.
- Effect controls damage, resource drains, inventory interactions, status effects, stat drains, and experience drains.

Sources:
- [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c#L3078)
- [reference-mangband-1_5_3/src/server/melee1.c](reference-mangband-1_5_3/src/server/melee1.c#L243)

## 1.2 Blow Categories and Projection

| Blow category | Original MAngband representation | Runtime behavior | Payload | Trigger conditions | Resistance and status interactions | Classification |
|---|---|---|---|---|---|---|
| Physical damage blows | RBM_HIT, RBM_CLAW, RBM_BITE with RBE_HURT | Armor-reduced melee hit | Direct damage, optional cut or stun | Melee reach and hit roll | AC, cut and stun status | Direct Action Candidate |
| Elemental contact blows | RBE_ACID, RBE_ELEC, RBE_FIRE, RBE_COLD | Calls elemental damage functions | Elemental damage with item destruction risk | Melee hit | resist, oppose, immunity, item ignore | Action + Policy |
| Poison and fear-style blows | RBE_POISON, RBE_TERRIFY | Damage plus status application checks | poison or fear status | Melee hit | poison resist and oppose, fear resist | Action + Policy |
| Confuse and paralyze blows | RBE_CONFUSE, RBE_PARALYZE | Damage plus control status | confusion or paralysis | Melee hit | free action, saves, resist conf | Action + Policy |
| Stat drain blows | RBE_LOSE_STR/DEX/CON/INT/WIS/CHR/ALL | Attribute reduction mutations | stat damage | Melee hit | sustain stats reduce impact | Action + Policy |
| Experience drain blows | RBE_EXP_10..80 | Experience loss behavior | xp drain | Melee hit | hold life mitigation | Action + Policy |
| Inventory or economy blows | RBE_EAT_GOLD, RBE_EAT_ITEM, RBE_EAT_FOOD, RBE_EAT_LITE, RBE_UN_POWER | Steal or drain inventory resources | inventory mutation | Melee hit and post-hit selection logic | dex and level safety checks, item type constraints | Not Safely Projectable |
| Disenchant and curse-like blows | RBE_UN_BONUS | Item disenchantment of bearer equipment | equipment mutation | Melee hit | resist disen mitigates | Action + Policy |
| Shatter blow | RBE_SHATTER | Strong physical plus terrain-adjacent effects in runtime family | damage and force-like impact | Melee hit | armor and status side effects | Action + Policy |

## 1.3 Blow Projection Findings

1. Payload overlap is high for direct damage and standard status applications.
2. Inventory and economy mutation blows are deeply policy-coupled and not safe as pure action payloads.
3. Method and effect channels should remain split in any canonical projection.

## 2. Spell System Projection

## 2.1 Spell Representation

Original representation:
- S:1_IN_X defines cast frequency.
- S flags mapped through RF4, RF5, RF6 spell families.

Runtime behavior:
- Spell set filtered by line-of-sight, range, intelligence behavior, and learned or cheated player resistance knowledge.
- One spell selected from remaining legal flags.

Sources:
- [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c#L3177)
- [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L416)

## 2.2 Spell Categories and Projection

| Spell category | Original representation | Runtime behavior | Payload | Trigger conditions | Resistance and status interactions | Classification |
|---|---|---|---|---|---|---|
| Innate ranged and boulder | RF4_ARROW_x, RF4_BOULDER | Bolt-like physical projections | physical projectile damage | range and LOS | armor, hit resolution | Direct Action Candidate |
| Breath attacks | RF4_BR_* | Damage capped by monster hp fractions | elemental or special AoE | range, LOS, smart filtering | resist, oppose, immunity, special saves | Action + Policy |
| Ball and bolt spells | RF5_BA_*, RF5_BO_* | Standard caster payload with monster scaling | elemental or typed damage | frequency and filter gates | resist and immunity channels | Action + Policy |
| Mind and curse spells | RF5_MIND_BLAST, RF5_BRAIN_SMASH, RF5_CAUSE_* | Direct spells with save checks and status outcomes | damage and statuses | direct path required | saving throw, resist blind or conf, free act | Action + Policy |
| Control spells | RF5_SCARE, RF5_BLIND, RF5_CONF, RF5_SLOW, RF5_HOLD | Status application payload | fear, blind, conf, slow, paralyze | direct path and spell selection policy | resist flags and free act | Action + Policy |
| Monster self-state spells | RF6_HASTE, RF6_HEAL, RF6_BLINK, RF6_TPORT | Self buff and relocation | speed, heal, teleport | own hp and tactical context | mostly no player resistance | Action + Policy |
| Player displacement spells | RF6_TELE_TO, RF6_TELE_AWAY, RF6_TELE_LEVEL | Forced movement or level move | displacement | direct path and tactical use | resist nexus and saving throw in level move | Action + Policy |
| Environment control spells | RF6_DARKNESS, RF6_TRAPS, RF6_FORGET | Darkness, trap creation, memory loss | world and status effects | direct path and tactical context | saving throws for forget; trap policy coupling | Action + Policy |
| Summon spells | RF6_S_* | Spawn entities by taxonomy | summon payload | tactical use, depth and family constraints | world occupancy and summon rules | Action + Policy |

## 2.3 Monster Spell Policy Layer

Policy elements that must remain source-specific:

1. Frequency gate from 1_IN_X contract.
2. Target legality through projectable path checks.
3. Smart filtering based on learned or cheated player defenses.
4. Desperation masking by monster hp and intelligence.

Sources:
- [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L47)
- [reference-mangband-1_5_3/src/server/melee2.c](reference-mangband-1_5_3/src/server/melee2.c#L460)

## Conclusion

Monsters can share canonical action vocabulary only for payloads.  
Monster behavior is not safely representable as payload-only actions because targeting, frequency, intelligence filtering, and inventory-coupled melee effects are policy-critical.

Safe projection boundary:
- Shared payload actions: yes.
- Shared policy model with spells and devices: no, requires monster-specific policy adapter.
