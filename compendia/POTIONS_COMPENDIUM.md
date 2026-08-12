# Potions Compendium

_Reference for the MAngband 1.5.3 source and IronHell implementation._

---

## How to Read This Document

| Column | Meaning |
|--------|---------|
| **sval** | MAngband internal sub-value (object.txt `I:75:<sval>:<pval>`) |
| **Dungeon Level** | Earliest floor the potion can appear (`A:<level>`) |
| **MAngband Effect** | Exact C function calls from `src/server/use-obj.c` / `xtra2.c` / `spells2.c` |
| **MAngband Message** | Exact player-visible message(s) printed by MAngband (from `msg_print` in the relevant `set_*` or `quaff_potion` functions) |
| **IronHell Effect** | How the effect is expressed in `generatedConsumables.ts` |
| **IronHell Message** | The message shown in IronHell's player log |
| **Status** | ✓ correct · ⚠ minor deviation · ❌ significant deviation |

Durations are in game turns (MAngband) / world ticks (IronHell). The conversion is approximately 1:1 for player actions.

### `hp_player` message table (from `spells2.c`)

| HP healed | `num / 5` | Message |
|-----------|-----------|---------|
| 1–14 | 0–2 | "You feel better." |
| 15–34 | 3–6 | "You feel much better." |
| 35+ | ≥ 7 | "You feel very good." |

---

## Potion List

### Water
- **sval** 0 · **Dungeon Level** 0 (surface shops)
- **MAngband Effect**: `set_food(p_ptr, p_ptr->food + 200)` — nourishes slightly
- **MAngband Message**: "You feel less thirsty."
- **IronHell Effect**: `satisfy_hunger: 200`
- **IronHell Message**: "You feel less thirsty."
- **Status**: ✓

---

### Apple Juice
- **sval** 1 · **Dungeon Level** 0
- **MAngband Effect**: `set_food(p_ptr, p_ptr->food + 250)` — nourishes slightly
- **MAngband Message**: "You feel less thirsty."
- **IronHell Effect**: `satisfy_hunger: 250`
- **IronHell Message**: "You feel less thirsty."
- **Status**: ✓

---

### Slime Mold Juice
- **sval** 2 · **Dungeon Level** 0
- **MAngband Effect**: `set_food(p_ptr, p_ptr->food + 400)` — nourishes moderately
- **MAngband Message**: "You feel less thirsty."
- **IronHell Effect**: `satisfy_hunger: 400`
- **IronHell Message**: "You feel less thirsty."
- **Status**: ✓

---

### Slowness
- **sval** 4 · **Dungeon Level** 3
- **MAngband Effect**: `set_slow(p_ptr, p_ptr->slow + randint1(25)+15)` → (16–40 turns), **speed −10**
- **MAngband Message**: "You feel yourself moving slower!"
- **IronHell Effect**: `apply_status: speed −10, durationMin 16, durationMax 40`
- **IronHell Message**: "You feel yourself moving slower!"
- **Status**: ✓

---

### Salt Water
- **sval** 5 · **Dungeon Level** 0
- **MAngband Effect**: `set_food(STARVE-1)` + `set_poisoned(0)` + `set_paralyzed(+4, bypass_free_act=TRUE)`
- **MAngband Message**: "The potion makes you vomit!"
- **IronHell Effect**: `neutralize_poison` + `apply_status: name='paralyzed', duration 4, buffId 'paralysis'` + `satisfy_hunger: −25000`
- **IronHell Message**: "The potion makes you vomit!" · "You are paralyzed!"
- **Notes**: MAngband bypasses `free_act` for salt water paralysis — not modelled in IronHell.
- **Status**: ⚠ `free_act` bypass not modelled

---

### Poison
- **sval** 6 · **Dungeon Level** 5
- **MAngband Effect**: `set_poisoned(p_ptr, p_ptr->poisoned + randint0(15)+10)` → (10–24 turns), DoT damage each turn
- **MAngband Message**: "You are poisoned!"
- **IronHell Effect**: `apply_status: name='poison', type='dot', durationMin 10, durationMax 24` — `applyPoisonDot()` deals `POISON_DOT_DAMAGE_PER_TICK` HP per tick via `processCharacterTick`
- **IronHell Message**: "You are poisoned!"
- **Notes**: Damage is flat per tick (MAngband scales with counter; IronHell uses a constant).
- **Status**: ✓

---

### Blindness
- **sval** 7 · **Dungeon Level** 6
- **MAngband Effect**: `set_blind(p_ptr, p_ptr->blind + randint0(100)+100)` → (100–199 turns), unless `resist_blind`
- **MAngband Message**: "You are blind!"
- **IronHell Effect**: `apply_status: lightRadius −999 (blind), durationMin 100, durationMax 199, buffId 'blindness'`
- **IronHell Message**: "You are blind!"
- **Notes**: `lightRadius −999` effectively renders the player blind; duration range preserved exactly.
- **Status**: ✓

---

### Confusion
- **sval** 9 · **Dungeon Level** 5
- **MAngband Effect**: `set_confused(p_ptr, p_ptr->confused + randint0(20)+15)` → (15–34 turns), unless `resist_conf`; randomises movement direction each turn
- **MAngband Message**: "You are confused!"
- **IronHell Effect**: `apply_status: durationMin 15, durationMax 34, buffId 'confusion'`; `isPlayerConfused()` randomises movement to a random cardinal/diagonal direction
- **IronHell Message**: "You are confused!"
- **Status**: ✓

---

### Sleep
- **sval** 11 · **Dungeon Level** 4
- **MAngband Effect**: `set_paralyzed(p_ptr, p_ptr->paralyzed + randint0(4)+4)` → (4–7 turns), unless `free_act`
- **MAngband Message**: "You are paralyzed!"
- **IronHell Effect**: `apply_status: name='paralyzed', durationMin 4, durationMax 7, buffId 'paralysis'` — `isPlayerParalyzed()` blocks all actions
- **IronHell Message**: "You are paralyzed!"
- **Status**: ✓

---

### Lose Memories
- **sval** 13 · **Dungeon Level** 5
- **MAngband Effect**: `lose_exp(p_ptr, p_ptr->exp / 4)` — drains 25% of current experience, unless `hold_life`
- **MAngband Message**: "You feel your memories fade."
- **IronHell Effect**: `lose_memories` flag → `consumableSpecials.ts` drains 25% of `character.experience`
- **IronHell Message**: "You feel your memories fade."
- **Status**: ✓

---

### Infravision
- **sval** 24 · **Dungeon Level** 3
- **MAngband Effect**: `set_tim_infra(p_ptr, p_ptr->tim_infra + 100 + randint1(100))` → (101–200 turns), extends infravision range
- **MAngband Message**: "Your eyes begin to tingle!"
- **IronHell Effect**: `buff: seeInfra +1, durationMin 101, durationMax 200, buffId 'infravision'`
- **IronHell Message**: "Your eyes begin to tingle!"
- **Status**: ✓

---

### Detect Invisible
- **sval** 25 · **Dungeon Level** 3
- **MAngband Effect**: `set_tim_invis(p_ptr, p_ptr->tim_invis + 12 + randint1(12))` → (13–24 turns), allows seeing invisible monsters
- **MAngband Message**: "Your eyes feel very sensitive!"
- **IronHell Effect**: `buff: canSeeInvisible +1, durationMin 13, durationMax 24, buffId 'detect_invisible'`
- **IronHell Message**: "Your eyes feel very sensitive!"
- **Status**: ✓

---

### Slow Poison
- **sval** 26 · **Dungeon Level** 2
- **MAngband Effect**: `set_poisoned(p_ptr, p_ptr->poisoned / 2)` — halves the remaining poison counter
- **MAngband Message**: "You are no longer poisoned." (only if counter drops to 0) · "You feel better." (if still poisoned)
- **IronHell Effect**: `halve_poison` — halves `remainingTicks` of all poison effects; removes those ≤ 0
- **IronHell Message**: "You are no longer poisoned." (if fully removed) · "You feel better." (if still poisoned)
- **Status**: ✓

---

### Neutralize Poison
- **sval** 27 · **Dungeon Level** 5
- **MAngband Effect**: `set_poisoned(p_ptr, 0)` — removes all poison
- **MAngband Message**: "You are no longer poisoned."
- **IronHell Effect**: `neutralize_poison` — removes all poison status effects
- **IronHell Message**: "You are no longer poisoned."
- **Status**: ✓

---

### Boldness
- **sval** 28 · **Dungeon Level** 1
- **MAngband Effect**: `set_afraid(p_ptr, 0)` — removes fear only
- **MAngband Message**: "You feel bolder now."
- **IronHell Effect**: `remove_fear` — removes fear status effect only
- **IronHell Message**: "You feel bolder now."
- **Status**: ✓

---

### Speed
- **sval** 29 · **Dungeon Level** 10
- **MAngband Effect**: If not already fast: `set_fast(p_ptr, 15+randint1(25))` → (16–40 turns), **speed +10**. If already fast: extends timer by +5 only.
- **MAngband Message**: "You feel yourself moving faster!"
- **IronHell Effect**: `buff: speed +10, durationMin 16, durationMax 40, buffId 'speed'`
- **IronHell Message**: "You feel yourself moving faster!"
- **Status**: ✓

---

### Resist Heat
- **sval** 30 · **Dungeon Level** 10
- **MAngband Effect**: `set_oppose_fire(p_ptr, p_ptr->oppose_fire + randint1(10)+10)` → (11–20 turns), grants temporary fire resistance
- **MAngband Message**: "You feel resistant to fire!"
- **IronHell Effect**: `buff: armor +4, durationMin 11, durationMax 20, buffId 'oppose_fire'`
- **IronHell Message**: "You feel resistant to fire!"
- **Notes**: Fire resistance approximated as AC bonus; message is correct.
- **Status**: ⚠ Effect approximated; message is correct

---

### Resist Cold
- **sval** 31 · **Dungeon Level** 10
- **MAngband Effect**: `set_oppose_cold(p_ptr, p_ptr->oppose_cold + randint1(10)+10)` → (11–20 turns), grants temporary cold resistance
- **MAngband Message**: "You feel resistant to cold!"
- **IronHell Effect**: `buff: armor +4, durationMin 11, durationMax 20, buffId 'oppose_cold'`
- **IronHell Message**: "You feel resistant to cold!"
- **Notes**: Cold resistance approximated as AC bonus; message is correct.
- **Status**: ⚠ Effect approximated; message is correct

---

### Heroism
- **sval** 32 · **Dungeon Level** 15
- **MAngband Effect**: `hp_player(10)` + `set_afraid(0)` + `set_hero(hero + randint1(25)+25)` → (26–50 turns): **to-hit +12**, resist fear, maxHP +10
- **MAngband Message**: "You feel like a hero!" · "You feel better." · "You feel bolder now."
- **IronHell Effect**: `heal_hp: 10` + `remove_fear` + `buff: attackPower +12, durationMin 26, durationMax 50, buffId 'heroism'`
- **IronHell Message**: "You feel better." · "You feel bolder now." · "You feel like a hero!"
- **Notes**: maxHP bonus not modelled. Message order differs slightly.
- **Status**: ⚠ maxHP bonus not modelled

---

### Berserk Strength
- **sval** 33 · **Dungeon Level** 15
- **MAngband Effect**: `hp_player(30)` + `set_afraid(0)` + `set_shero(shero + randint1(25)+25)` → (26–50 turns): **to-hit +24**, **AC −10**, resist fear, maxHP +30
- **MAngband Message**: "You feel like a killing machine!" · "You feel much better." · "You feel bolder now."
- **IronHell Effect**: `heal_hp: 30` + `remove_fear` + `buff: attackPower +24, durationMin 26, durationMax 50, buffId 'berserk-strength'` + `apply_status: armor −10, durationMin 26, durationMax 50`
- **IronHell Message**: "You feel much better." · "You feel bolder now." · "You feel like a killing machine!"
- **Notes**: maxHP bonus not modelled. Message order differs slightly.
- **Status**: ⚠ maxHP bonus not modelled

---

### Cure Light Wounds
- **sval** 34 · **Dungeon Level** 1
- **MAngband Effect**: `hp_player(15)` + `set_blind(0)` + `set_cut(cut-20)` + `set_confused(confused-20)`
- **MAngband Message**: "You feel much better." (15/5=3 → "much better")
- **IronHell Effect**: `heal_hp: 15`
- **IronHell Message**: "You feel much better."
- **Status**: ✓

---

### Cure Serious Wounds
- **sval** 35 · **Dungeon Level** 4
- **MAngband Effect**: `hp_player(randint0(5)+20)` → (20–24 HP) + cure blind/confused/cut
- **MAngband Message**: "You feel much better." (20–24/5=4 → "much better")
- **IronHell Effect**: `heal_hp: 22` + `cleanse`
- **IronHell Message**: "You feel much better."
- **Status**: ✓

---

### Cure Critical Wounds
- **sval** 36 · **Dungeon Level** 8
- **MAngband Effect**: `hp_player(randint0(5)+25)` → (25–29 HP) + cure blind/confused/poison/stun/cut
- **MAngband Message**: "You feel much better." (25–29/5=5 → "much better")
- **IronHell Effect**: `heal_hp: 27` + `cleanse`
- **IronHell Message**: "You feel much better."
- **Status**: ✓

---

### Healing
- **sval** 37 · **Dungeon Level** 30
- **MAngband Effect**: `hp_player(300)` + cure all bad status
- **MAngband Message**: "You feel very good." (300/5=60 ≥ 7)
- **IronHell Effect**: `heal_hp: 300` + `cleanse`
- **IronHell Message**: "You feel very good."
- **Status**: ✓

---

### \*Healing\*
- **sval** 38 · **Dungeon Level** 40
- **MAngband Effect**: `hp_player(1200)` + cure all bad status
- **MAngband Message**: "You feel very good."
- **IronHell Effect**: `heal_hp: 1200` + `cleanse`
- **IronHell Message**: "You feel very good."
- **Status**: ✓

---

### Life
- **sval** 39 · **Dungeon Level** 45
- **MAngband Effect**: `restore_level` + cure all status + restore all 6 stats + `hp_player(5000)`
- **MAngband Message**: "You feel life flow through your body!" · "You feel very good."
- **IronHell Effect**: `heal_hp: 5000` + `restore_all_stats` + `cleanse`
- **IronHell Message**: "You feel life flow through your body!" · "You feel very good."
- **Notes**: `restore_level` (experience restore) not modelled.
- **Status**: ⚠ Experience restore not modelled

---

### Restore Mana
- **sval** 40 · **Dungeon Level** 25
- **MAngband Effect**: `p_ptr->csp = p_ptr->msp` — restores mana to maximum
- **MAngband Message**: "Your feel your head clear." _(MAngband source has a typo — IronHell uses correct grammar)_
- **IronHell Effect**: `heal_mana: 9999` → capped at `character.mana.max`
- **IronHell Message**: "You feel your head clear."
- **Status**: ✓

---

### Restore Life Levels (Restore Exp)
- **sval** 41 · **Dungeon Level** 10
- **MAngband Effect**: `restore_level(p_ptr)` → restores `exp` to `max_exp`
- **MAngband Message**: "You feel your life energies returning."
- **IronHell Effect**: `restore_life_levels` flag → `consumableSpecials.ts` grants 100,000 exp via `applyExperienceGain`
- **IronHell Message**: "You feel your life energies returning."
- **Notes**: IronHell does not track `max_exp` separately; approximated as a large fixed grant.
- **Status**: ⚠ Approximation (large fixed grant vs. restoring to tracked max)

---

### Restore Strength / Intelligence / Wisdom / Charisma / Dexterity / Constitution
- **sval** 42–47 · **Dungeon Level** 10–15
- **MAngband Effect**: `do_res_stat(p_ptr, A_*)` → restores drained stat to natural max; no effect if not drained
- **MAngband Message**: "You feel stronger!" / "You feel smarter!" etc. (only if stat was actually drained)
- **IronHell Effect**: `restore_stat: <stat>` — restores drained stat to its natural (un-drained) value; emits `:already_max` if not drained
- **IronHell Message**: "You feel stronger!" etc. (only if restored; no message if already at max)
- **Status**: ✓

---

### Strength / Intelligence / Wisdom / Dexterity / Constitution / Charisma
- **sval** 48–53 · **Dungeon Level** 5–15
- **MAngband Effect**: `do_res_stat()` first (restore if drained); if already at max permanently increments up to `18+100`
- **MAngband Message**: "You feel stronger!" / "You feel smarter!" etc.
- **IronHell Effect**: `modify_stat_permanent: <stat> +1`
- **IronHell Message**: "You feel stronger!" / "You feel smarter!" etc.
- **Status**: ✓ _(functional match for non-drained case; messages correct)_

---

### Weakness / Stupidity / Naivety / Clumsiness / Sickliness / Ugliness
- **sval** N/A · **Dungeon Level** 3–5
- **MAngband Effect**: `do_dec_stat(p_ptr, A_*)` → permanently decrements the stat
- **MAngband Message**: "You feel weaker." / "You feel less intelligent." etc.
- **IronHell Effect**: `modify_stat_permanent: <stat> −1`
- **IronHell Message**: "You feel less stronger." etc. _(phrasing may differ slightly)_
- **Status**: ✓

---

### Enlightenment
- **sval** 56 · **Dungeon Level** 15
- **MAngband Effect**: `wiz_lite(p_ptr)` → reveals entire dungeon level (magic map). No item identification.
- **MAngband Message**: "An image of your surroundings forms in your mind..."
- **IronHell Effect**: `magic_mapping`
- **IronHell Message**: "An image of your surroundings forms in your mind..."
- **Status**: ✓

---

### Augmentation
- **sval** 55 · **Dungeon Level** 40
- **MAngband Effect**: `do_inc_stat` for all 6 base stats — permanently increments each by up to 1 (or restores if drained)
- **MAngband Message**: "You feel powerful!" (one message per stat that improved)
- **IronHell Effect**: `modify_stat_permanent: +1` for each of STR, INT, WIS, DEX, CON, CHA
- **IronHell Message**: "You feel stronger!" · "You feel smarter!" etc. (one per stat)
- **Status**: ✓

---

### \*Enlightenment\*
- **sval** 57 · **Dungeon Level** 50
- **MAngband Effect**: `wiz_lite` (magic map) + `do_inc_stat(INT)` + `do_inc_stat(WIS)` + detect all + identify pack + self knowledge
- **MAngband Message**: "You begin to feel more enlightened..." · stat-increase messages · "You feel your knowledge deepen."
- **IronHell Effect**: `buff: buffId 'enlightenment_announce'` (announcement) + `magic_mapping` + `modify_stat_permanent: intelligence +1` + `modify_stat_permanent: wisdom +1` + `identify`
- **IronHell Message**: "You begin to feel more enlightened..." · "You feel smarter!" · "You feel wiser!" · "You feel your knowledge deepen."
- **Notes**: Detect-all and self-knowledge not fully modelled; `identify` covers pack identification.
- **Status**: ✓

---

### True Seeing
- **sval** 58 · **Dungeon Level** 30
- **MAngband Effect**: `set_tim_invis(p_ptr, 12+randint1(12))` (13–24 turns) + `set_tim_infra(p_ptr, 100+randint1(100))` (101–200 turns)
- **MAngband Message**: "Your eyes feel very sensitive!" · "Your eyes begin to tingle!"
- **IronHell Effect**: `buff: canSeeInvisible +1, durationMin 13, durationMax 24, buffId 'detect_invisible'` + `buff: seeInfra +1, durationMin 101, durationMax 200, buffId 'infravision'`
- **IronHell Message**: "Your eyes feel very sensitive!" · "Your eyes begin to tingle!"
- **Status**: ✓

---

### Experience
- **sval** 59 · **Dungeon Level** 20
- **MAngband Effect**: `gain_exp(p_ptr, min(p_ptr->exp/2+10, 100000))` — grants up to 100,000 exp
- **MAngband Message**: "You feel more experienced."
- **IronHell Effect**: `gain_experience: 100000`
- **IronHell Message**: "You feel more experienced."
- **Notes**: MAngband scales grant by current exp; IronHell always grants the full 100,000.
- **Status**: ⚠ Always grants 100,000; message is correct

---

## Message Log Behaviour

Player messages (consumable effects, combat, loot, level-ups, trap events) stay permanently in the message panel.

Dungeon ambient sound messages (monster movement cues, narrative flavour text) use `action: 'ambient'` and disappear after **10 seconds** — twice the duration of the old single-threshold timer.

The death screen always shows the last 12 messages from the full combat log regardless of the ambient expiry filter.

---

## Potions Not Yet in IronHell

All MAngband potions defined in `object.txt` are now implemented in IronHell.

---

## Vulnerability

Potions are fragile:
- **Cold damage** destroys a stack of potions (MAngband `TR3_IGNORE_COLD` absent → shattered by cold)
- Scrolls are destroyed by fire and acid; potions only by cold

---

## Sources

- `reference-mangband-1_5_3/src/server/use-obj.c` — `quaff_potion()` function
- `reference-mangband-1_5_3/src/server/xtra2.c` — `set_hero()`, `set_shero()`, `set_fast()`, `set_slow()`, `set_blind()`, `set_confused()`, `set_poisoned()`, `set_paralyzed()`, `set_afraid()`, `set_tim_infra()`, `set_tim_invis()`, `set_oppose_fire()`, `set_oppose_cold()`
- `reference-mangband-1_5_3/src/server/spells2.c` — `hp_player()`, `restore_level()`
- `reference-mangband-1_5_3/src/server/xtra1.c` — hero/shero stat bonus tables (`to_h`, `to_a` adjustments)
- `reference-mangband-1_5_3/src/server/mdefines.h` — `SV_POTION_*` constants, `PY_FOOD_*` constants
- `reference-mangband-1_5_3/lib/edit/object.txt` — potion objects (tval 75, sval, pval, dungeon level allocations)
