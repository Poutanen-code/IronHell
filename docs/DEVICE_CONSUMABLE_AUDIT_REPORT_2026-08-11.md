# Device and Consumable Audit Report (2026-08-11)

## Scope
- data/definitions/items/rods.json
- data/definitions/items/wands.json
- data/definitions/items/staves.json
- data/definitions/items/scrolls.json
- data/definitions/items/potions.json

## Authoritative Inputs Used
- docs/GAMEPLAY_ARCHITECTURE_V2.md
- docs/GAMEPLAY_ARCHITECTURE_DECISIONS.md
- data/definitions/actions.json
- data/definitions/statuses.json
- data/definitions/resistances.json
- data/definitions/mage_spells.json
- data/definitions/priest_prayers.json
- reference-mangband-1_5_3/src/server/use-obj.c
- reference-mangband-1_5_3/src/server/spells2.c
- reference-mangband-1_5_3/src/server/mdefines.h

## Validation Results
- All five target files were converted from effect_id payloads to action_id payloads.
- No effect_id remains in the five target files.
- All action_id values in the five target files exist in data/definitions/actions.json.
- JSON parsing of all five edited files succeeded.

## Source-Verified Healing Corrections
- rod_of_heal_monster -> 4d6 (from heal_monster in spells2.c)
- wand_of_healing -> 500 HP (from SV_ROD_HEALING in use-obj.c)
- staff_of_cure_light_wounds -> 1d8 (from SV_STAFF_CURE_LIGHT in use-obj.c)
- staff_of_healing -> 300 HP (from SV_STAFF_HEALING in use-obj.c)
- potion_of_heroism -> 10 HP (use-obj.c)
- potion_of_berserk_strength -> 30 HP (use-obj.c)
- potion_of_cure_light_wounds -> 15 HP (use-obj.c)
- potion_of_cure_serious_wounds -> 20 + 1d5 (use-obj.c)
- potion_of_cure_critical_wounds -> 25 + 1d5 (use-obj.c)
- potion_of_healing -> 300 HP (use-obj.c)

## Source-Verified Duration Corrections
- potion_of_speed -> 15 + 1d25
- potion_of_resist_heat -> 10 + 1d10
- potion_of_resist_cold -> 10 + 1d10
- potion_of_heroism -> 25 + 1d25
- potion_of_berserk_strength -> 25 + 1d25
- scroll_of_blessing -> 6 + 1d12
- scroll_of_holy_chant -> 12 + 1d24
- scroll_of_holy_prayer -> 24 + 1d48
- scroll_of_darkness blindness -> 3 + 1d5
- scroll_of_destruction blindness -> 10 + 1d10
- wand_of_speed -> 15 + 1d30 (+5 extension when already hasted)
- staff_of_slowness -> 15 + 1d30

## Changed Item IDs
### rods.json
rod_of_light, rod_of_lightning_bolts, rod_of_frost_bolts, rod_of_fire_bolts, rod_of_stone_to_mud, rod_of_polymorph, rod_of_heal_monster, rod_of_haste_monster, rod_of_slow_monster, rod_of_confuse_monster, rod_of_sleep_monster, rod_of_drain_life, rod_of_trap_door_destruction, rod_of_magic_missile, rod_of_clone_monster, rod_of_scare_monster, rod_of_teleport_other, rod_of_disarming

### wands.json
wand_of_trap_location, wand_of_door_stair_location, wand_of_perception, wand_of_recall, wand_of_illumination, wand_of_enlightenment, wand_of_detection, wand_of_probing, wand_of_curing, wand_of_healing, wand_of_restoration, wand_of_speed

### staves.json
staff_of_darkness, staff_of_slowness, staff_of_haste_monsters, staff_of_summoning, staff_of_teleportation, staff_of_perception, staff_of_remove_curse, staff_of_light, staff_of_enlightenment, staff_of_treasure_location, staff_of_object_location, staff_of_trap_location, staff_of_door_stair_location, staff_of_detect_invisible, staff_of_detect_evil, staff_of_cure_light_wounds, staff_of_curing, staff_of_healing

### scrolls.json
scroll_of_darkness, scroll_of_aggravate_monster, scroll_curse_armor, scroll_curse_weapon, scroll_of_summon_monster, scroll_of_summon_undead, scroll_of_artifact_creation, scroll_of_trap_creation, scroll_of_phase_door, scroll_teleport, scroll_of_teleport_level, scroll_of_word_of_recall, scroll_identify_single, scroll_identify, scroll_of_remove_curse, scroll_of_remove_curse_heavy, scroll_of_enchant_armor, scroll_of_enchant_weapon_to_hit, scroll_of_enchant_weapon_to_dam, scroll_of_enchant_armor_heavy, scroll_of_enchant_weapon_heavy, scroll_of_recharging, scroll_of_light, scroll_of_magic_mapping, scroll_of_treasure_detection, scroll_of_object_detection, scroll_of_trap_detection, scroll_of_door_stair_location, scroll_of_detect_invisible, scroll_of_satisfy_hunger, scroll_of_blessing, scroll_of_holy_chant, scroll_of_holy_prayer, scroll_of_monster_confusion, scroll_of_protection_from_evil, scroll_of_rune_of_protection, scroll_of_trap_door_destruction, scroll_of_destruction, scroll_of_dispel_undead, scroll_of_banishment, scroll_of_mass_banishment, scroll_of_acquirement, scroll_of_great_acquirement, scroll_of_life

### potions.json
potion_of_water, potion_of_apple_juice, potion_of_slime_mold_juice, potion_of_slowness, potion_of_salt_water, potion_of_poison, potion_of_blindness, potion_of_confusion, potion_of_sleep, potion_of_lose_memories, potion_of_infravision, potion_of_detect_invisible, potion_of_slow_poison, potion_of_neutralize_poison, potion_of_boldness, potion_of_speed, potion_of_resist_heat, potion_of_resist_cold, potion_of_heroism, potion_of_berserk_strength, potion_of_cure_light_wounds, potion_of_cure_serious_wounds, potion_of_cure_critical_wounds, potion_of_healing, potion_of_restore_mana, potion_of_enlightenment, potion_of_experience

## Unresolved Behaviors
- rod_of_haste_monster: uses speed_monster semantics (monster-targeted haste with level-based potency) not represented directly by current action catalog.
- rod_of_clone_monster: clone_monster semantics are not equivalent to summon/polymorph and currently lack canonical action support.
- staff_of_haste_monsters: speed_monsters area effect has no direct canonical action.
- scroll_of_aggravate_monster: wake+haste monster-side behavior has no direct canonical action.
- scroll_curse_armor: curse application semantics are opposite of RemoveCurse and lack canonical action.
- scroll_curse_weapon: curse application semantics are opposite of RemoveCurse and lack canonical action.
- scroll_of_trap_creation: trap placement operation has no direct canonical terrain_operation.

## Proposed New Actions
- AggravateMonsters
- ApplyItemCurse
- CreateTraps
- HasteMonsterControl
- CloneMonster

## Notes
- No actions were added in this pass; unresolved mappings were marked explicitly in item definitions.
- The action catalog remains unchanged to avoid unrelated refactoring.
