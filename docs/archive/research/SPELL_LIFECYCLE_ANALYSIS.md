# Spell Lifecycle Analysis

Date: 2026-08-03  
Scope: MAngband 1.5.3 player spell and prayer lifecycle behavior

## Evidence Basis

- Class spell metadata format: [reference-mangband-1_5_3/lib/edit/p_class.txt](reference-mangband-1_5_3/lib/edit/p_class.txt#L20)
- Parser storing M and B entries into runtime class structures: [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c#L1245)
- Study flow: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L306)
- Spell legality and failure calculation: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L43)
- Cast and pray execution: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L500), [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L760)
- Post-cast finalization, mana spending, and overexertion path: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L639)
- Spell count, forgetting, and remembering logic: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1607)
- Mana regeneration loop: [reference-mangband-1_5_3/src/server/dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c#L327)

## 1. Learning and Studying

### Preconditions

- Class must be literate (spell_book is not zero).
- Player must have at least one new spell slot available.
- Player must have light and not be confused.
- Selected book must match class spell_book tval.

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L323)

### Mage and Magic-Realm Study Behavior

- Player selects a specific spell from the book slot list.
- spell_okay controls whether the selected spell can be learned.

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L377)

### Priest and Prayer-Realm Study Behavior

- Study chooses randomly among legal prayers in the selected book.
- Selection uses reservoir-style randomization over valid options.

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L404)

## 2. Learned, Forgotten, and Remembered States

State flags used in lifecycle:
- PY_SPELL_LEARNED
- PY_SPELL_FORGOTTEN
- PY_SPELL_WORKED

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L116), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1655)

### Automatic Forgetting

Spells are forgotten when:
- spell level exceeds current player level, or
- known spell count exceeds allowed spell count

Evidence: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1650), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1687)

### Automatic Remembering

Forgotten spells are remembered when:
- player has available learn slots, and
- spell level is now legal for current level

Evidence: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1725)

## 3. Level Requirements and Class Dependencies

### Spell Metadata

Each class record includes:
- spell_book tval
- spell_stat
- spell_first
- spell_weight

Each spell slot includes:
- slevel
- smana
- sfail
- sexp

Evidence: [reference-mangband-1_5_3/lib/edit/p_class.txt](reference-mangband-1_5_3/lib/edit/p_class.txt#L51), [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c#L1257)

### Legality Gate

A spell is illegal if slevel is above player level.

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L114)

### New Spell Capacity

Allowed known spells is derived from:
- player level relative to spell_first
- class spell_stat via adj_mag_study

Evidence: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L1628)

## 4. Failure Calculation

Failure chance baseline:
- chance starts at sfail
- reduced by 3 times level delta above slevel
- reduced by spell stat adjustment

Minimum and caps:
- minimum fail derived from adj_mag_fail
- non-zero-fail classes enforce minimum 5 percent
- capped at 95 percent fail

Modifiers:
- edged-weapon penalty for priests via icky_wield
- stun adds fail chance

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L57)

## 5. Casting and Mana Consumption

### Normal Cast Flow

- validate command conditions
- validate selected book tval and spell legality
- validate mana availability
- roll fail chance
- on success, dispatch cast_spell
- on failure, still finalize turn and mana flow via do_cmd_cast_fin

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L606), [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L630)

### Mana Cost Behavior

- successful or failed attempts consume turn and mana through do_cmd_cast_fin
- if enough mana, smana is deducted
- overexert branch exists (fainting, paralysis, possible constitution damage)

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L667)

Note: regular cast path checks mana before casting, so overexert path is mostly relevant to edge conditions or alternate call paths.

## 6. Mana Recovery

Mana regeneration uses:
- msp scaled by regen percent
- fractional accumulator
- cap at msp

Evidence: [reference-mangband-1_5_3/src/server/dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c#L327)

## 7. Worked State and Experience

First successful cast marks spell as worked and awards experience based on sexp times slevel.

Evidence: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L651)

## 8. Lifecycle Summary

1. Class metadata defines spell realm and progression envelope.  
2. Book contents define candidate spells.  
3. Study operation moves unknown legal spell to learned state.  
4. Level changes can force forget or allow remember transitions.  
5. Cast and pray operations evaluate policy gates then dispatch payload.  
6. Mana and experience are updated in finalization step.  

## 9. Architectural Research Implications

- Spell lifecycle includes policy-heavy behavior not present in normal consumable use.
- Payload overlap with items and monster spells is high, but policy layer is distinct.
- Any canonical model must preserve a separate policy channel for learning, legality, and progression.

