# Device/Consumable Action Contract Normalization Report (2026-08-11)

## Modified JSON Files
- data/definitions/items/rods.json
- data/definitions/items/wands.json
- data/definitions/items/staves.json
- data/definitions/items/scrolls.json
- data/definitions/items/potions.json
- data/definitions/items/consumables.json

## Validation
- All action references use action_id + parameters structure.
- No action parameters exist outside parameters block.
- Unsupported in-contract parameters currently present: 0
- Required-parameter violations currently present: 0

## Action Contract Violations Fixed
- Inline action parameters were moved under parameters blocks across rods, wands, staves, scrolls, potions, and consumables.
- Missing HealHP target_mode was fixed for mushroom_cure_serious_wounds and piece_of_elvish_waybread.
- Legacy target alias fields on action entries were normalized to target_mode where applicable.

## Unsupported Parameters Found (moved out of contracts)
- data/definitions/items/rods.json :: rod_of_clone_monster :: TransformEntity.unresolved_mapping="clone_monster_semantics_not_modeled"
- data/definitions/items/scrolls.json :: scroll_of_darkness :: ApplyTimedBuff.condition={"requires_absence":"res_blind"}
- data/definitions/items/scrolls.json :: scroll_of_aggravate_monster :: DetectEntities.unresolved_mapping="aggravate_not_modeled"
- data/definitions/items/scrolls.json :: scroll_curse_armor :: RemoveCurse.unresolved_mapping="inverse_behavior_curse_not_modeled"; RemoveCurse.target="armor"; RemoveCurse.target_mode="armor"
- data/definitions/items/scrolls.json :: scroll_curse_weapon :: RemoveCurse.unresolved_mapping="inverse_behavior_curse_not_modeled"; RemoveCurse.target="weapon"; RemoveCurse.target_mode="weapon"
- data/definitions/items/scrolls.json :: scroll_of_artifact_creation :: AlterTerrain.quality="great"; AlterTerrain.count=1
- data/definitions/items/scrolls.json :: scroll_of_trap_creation :: AlterTerrain.unresolved_mapping="create_traps_not_modeled"
- data/definitions/items/scrolls.json :: scroll_of_enchant_armor :: EnchantEquipment.attempts=1
- data/definitions/items/scrolls.json :: scroll_of_enchant_weapon_to_hit :: EnchantEquipment.attempts=1
- data/definitions/items/scrolls.json :: scroll_of_enchant_weapon_to_dam :: EnchantEquipment.attempts=1
- data/definitions/items/scrolls.json :: scroll_of_enchant_armor_heavy :: EnchantEquipment.attempts_base=2; EnchantEquipment.attempts_dice={"count":1,"sides":3}
- data/definitions/items/scrolls.json :: scroll_of_enchant_weapon_heavy :: EnchantEquipment.attempts_dice={"count":1,"sides":3}
- data/definitions/items/scrolls.json :: scroll_of_monster_confusion :: ConfuseControl.condition={"on_next_melee_hit":true}
- data/definitions/items/scrolls.json :: scroll_of_destruction :: ApplyTimedBuff.condition={"requires_absence":"res_blind"}
- data/definitions/items/scrolls.json :: scroll_of_acquirement :: AlterTerrain.quality="good"; AlterTerrain.count=1
- data/definitions/items/scrolls.json :: scroll_of_great_acquirement :: AlterTerrain.quality="great"; AlterTerrain.count=1; AlterTerrain.count_dice={"count":1,"sides":2}
- data/definitions/items/potions.json :: potion_of_salt_water :: ApplyTimedBuff.condition={"forced":true}
- data/definitions/items/potions.json :: potion_of_poison :: ApplyTimedBuff.condition={"requires_absence":"res_pois"}
- data/definitions/items/potions.json :: potion_of_blindness :: ApplyTimedBuff.condition={"requires_absence":"res_blind"}
- data/definitions/items/potions.json :: potion_of_sleep :: ApplyTimedBuff.condition={"requires_absence":"free_act"}
- data/definitions/items/consumables.json :: mushroom_blindness :: ApplyTimedBuff.target="self"
- data/definitions/items/consumables.json :: mushroom_paranoia :: ApplyTimedBuff.target="self"
- data/definitions/items/consumables.json :: mushroom_confusion :: ApplyTimedBuff.target="self"
- data/definitions/items/consumables.json :: mushroom_hallucination :: ApplyTimedBuff.target="self"
- data/definitions/items/consumables.json :: mushroom_paralysis :: ApplyTimedBuff.target="self"
- data/definitions/items/consumables.json :: mushroom_poison :: ApplyTimedBuff.target="self"

## Unresolved MAngband Behaviors
- data/definitions/items/rods.json :: rod_of_haste_monster :: MAngband uses speed_monster() (GF_OLD_SPEED with potency based on player level). Current action catalog has no explicit targeted monster haste control action.
- data/definitions/items/rods.json :: rod_of_clone_monster :: Unsupported action parameters moved out of contract: TransformEntity.unresolved_mapping="clone_monster_semantics_not_modeled"
- data/definitions/items/staves.json :: staff_of_haste_monsters :: MAngband uses speed_monsters() (monster-targeted haste sweep), but current action catalog has no explicit monster-haste area control action.
- data/definitions/items/scrolls.json :: scroll_of_aggravate_monster :: Unsupported action parameters moved out of contract: DetectEntities.unresolved_mapping="aggravate_not_modeled"
- data/definitions/items/scrolls.json :: scroll_curse_armor :: Unsupported action parameters moved out of contract: RemoveCurse.unresolved_mapping="inverse_behavior_curse_not_modeled"; RemoveCurse.target="armor"; RemoveCurse.target_mode="armor"
- data/definitions/items/scrolls.json :: scroll_curse_weapon :: Unsupported action parameters moved out of contract: RemoveCurse.unresolved_mapping="inverse_behavior_curse_not_modeled"; RemoveCurse.target="weapon"; RemoveCurse.target_mode="weapon"
- data/definitions/items/scrolls.json :: scroll_of_trap_creation :: Unsupported action parameters moved out of contract: AlterTerrain.unresolved_mapping="create_traps_not_modeled"

## Healing Values Verified/Kept Explicit
- data/definitions/items/rods.json :: rod_of_heal_monster :: {"kind":"dice","count":4,"sides":6}
- data/definitions/items/wands.json :: wand_of_healing :: {"kind":"flat","value":500}
- data/definitions/items/staves.json :: staff_of_cure_light_wounds :: {"kind":"dice","count":1,"sides":8}
- data/definitions/items/staves.json :: staff_of_healing :: {"kind":"flat","value":300}
- data/definitions/items/potions.json :: potion_of_heroism :: {"kind":"flat","value":10}
- data/definitions/items/potions.json :: potion_of_berserk_strength :: {"kind":"flat","value":30}
- data/definitions/items/potions.json :: potion_of_cure_light_wounds :: {"kind":"flat","value":15}
- data/definitions/items/potions.json :: potion_of_cure_serious_wounds :: {"kind":"dice","count":1,"sides":5,"base":20}
- data/definitions/items/potions.json :: potion_of_cure_critical_wounds :: {"kind":"dice","count":1,"sides":5,"base":25}
- data/definitions/items/potions.json :: potion_of_healing :: {"kind":"flat","value":300}
- data/definitions/items/consumables.json :: mushroom_cure_serious_wounds :: {"kind":"dice","count":4,"sides":8}
- data/definitions/items/consumables.json :: piece_of_elvish_waybread :: {"kind":"dice","count":4,"sides":8}

## Timed Durations Kept Explicit
- data/definitions/items/rods.json :: rod_of_haste_monster :: haste :: {"base":20,"dice":{"count":1,"sides":20}}
- data/definitions/items/wands.json :: wand_of_speed :: haste :: {"base":15,"dice":{"count":1,"sides":30}}
- data/definitions/items/staves.json :: staff_of_slowness :: slowed :: {"base":15,"dice":{"count":1,"sides":30}}
- data/definitions/items/staves.json :: staff_of_haste_monsters :: haste :: {"base":20,"dice":{"count":1,"sides":20}}
- data/definitions/items/scrolls.json :: scroll_of_darkness :: blinded :: {"base":3,"dice":{"count":1,"sides":5}}
- data/definitions/items/scrolls.json :: scroll_of_blessing :: blessed :: {"base":6,"dice":{"count":1,"sides":12}}
- data/definitions/items/scrolls.json :: scroll_of_holy_chant :: blessed :: {"base":12,"dice":{"count":1,"sides":24}}
- data/definitions/items/scrolls.json :: scroll_of_holy_prayer :: blessed :: {"base":24,"dice":{"count":1,"sides":48}}
- data/definitions/items/scrolls.json :: scroll_of_protection_from_evil :: protected_from_evil :: {"base":0,"dice":{"count":3,"sides":1}}
- data/definitions/items/scrolls.json :: scroll_of_destruction :: blinded :: {"base":10,"dice":{"count":1,"sides":10}}
- data/definitions/items/potions.json :: potion_of_salt_water :: paralyzed :: {"base":4}
- data/definitions/items/potions.json :: potion_of_poison :: poisoned :: {"base":9,"dice":{"count":1,"sides":15}}
- data/definitions/items/potions.json :: potion_of_blindness :: blinded :: {"base":99,"dice":{"count":1,"sides":100}}
- data/definitions/items/potions.json :: potion_of_confusion :: confused :: {"base":14,"dice":{"count":1,"sides":20}}
- data/definitions/items/potions.json :: potion_of_sleep :: paralyzed :: {"base":3,"dice":{"count":1,"sides":4}}
- data/definitions/items/potions.json :: potion_of_infravision :: infravision_boost :: {"base":100,"dice":{"count":1,"sides":100}}
- data/definitions/items/potions.json :: potion_of_detect_invisible :: see_invisible :: {"base":12,"dice":{"count":1,"sides":12}}
- data/definitions/items/potions.json :: potion_of_speed :: haste :: {"base":15,"dice":{"count":1,"sides":25}}
- data/definitions/items/potions.json :: potion_of_resist_heat :: resist_fire :: {"base":10,"dice":{"count":1,"sides":10}}
- data/definitions/items/potions.json :: potion_of_resist_cold :: resist_cold :: {"base":10,"dice":{"count":1,"sides":10}}
- data/definitions/items/potions.json :: potion_of_heroism :: heroism :: {"base":25,"dice":{"count":1,"sides":25}}
- data/definitions/items/potions.json :: potion_of_berserk_strength :: berserk :: {"base":25,"dice":{"count":1,"sides":25}}
- data/definitions/items/consumables.json :: mushroom_blindness :: blinded :: {"base":199,"dice":{"count":1,"sides":200}}
- data/definitions/items/consumables.json :: mushroom_paranoia :: afraid :: {"base":9,"dice":{"count":1,"sides":10}}
- data/definitions/items/consumables.json :: mushroom_confusion :: confused :: {"base":9,"dice":{"count":1,"sides":10}}
- data/definitions/items/consumables.json :: mushroom_hallucination :: hallucinating :: {"base":249,"dice":{"count":1,"sides":250}}
- data/definitions/items/consumables.json :: mushroom_paralysis :: paralyzed :: {"base":9,"dice":{"count":1,"sides":10}}
- data/definitions/items/consumables.json :: mushroom_poison :: poisoned :: {"base":9,"dice":{"count":1,"sides":10}}

## Proposed Action Catalog Changes (minimized)
- Add AggravateMonsters action (monster wake + haste semantics).
- Add ApplyItemCurse action (curse weapon/armor semantics).
- Add CreateTraps action (trap creation around actor).
- Add HasteMonsterControl action (single/area monster haste).
- Add CloneMonster action (GF_OLD_CLONE semantics).
