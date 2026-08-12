# Capability Scope Matrix

Date: 2026-08-03  
Scope: G-01 capability scope verification using MAngband 1.5.3

## Objective

Determine whether capability scopes are distinct gameplay concepts or only storage locations.

## Evidence Basis

- Player native flag synthesis from race and class: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L649)
- Class flags and semantics: [reference-mangband-1_5_3/src/server/mdefines.h](reference-mangband-1_5_3/src/server/mdefines.h#L2194), [reference-mangband-1_5_3/lib/edit/p_class.txt](reference-mangband-1_5_3/lib/edit/p_class.txt#L68)
- Race-level passive flags: [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L72)
- Equipment flag application to bearer runtime state: [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2249)
- Item-self protection flags: [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c#L453), [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L790)
- Oppose timers (temporary status channel): [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L970), [reference-mangband-1_5_3/src/server/dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c#L1417)

Legend:
- Y: yes
- N: no
- P: partially, only for some grant paths

## Matrix

| Capability ID | Description | Native Identity | Bearer Passive | Item Self Passive | Temporary Status Equivalent | MAngband provenance | Notes |
|---|---|---|---|---|---|---|---|
| back_stab | Extra melee benefit on sleeping or fleeing targets | Y | N | N | N | [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L1408) | Class policy capability.
| beam | Better beam chance on bolt-like spell output | Y | N | N | N | [reference-mangband-1_5_3/src/server/x-spell.c](reference-mangband-1_5_3/src/server/x-spell.c#L1080) | Class casting policy.
| bless_weapon | Priest edged-weapon and blessing interaction | Y | P | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2993) | Class rule with equipment dependency.
| bravery_30 | Fear resistance from class progression | Y | N | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L662) | Native class progression policy.
| choose_spells | Spell selection model policy | Y | N | N | N | [reference-mangband-1_5_3/lib/edit/p_class.txt](reference-mangband-1_5_3/lib/edit/p_class.txt#L68) | No item-based equivalent.
| cumber_glove | Glove penalty policy for spellcasting | Y | N | N | N | [reference-mangband-1_5_3/lib/edit/p_class.txt](reference-mangband-1_5_3/lib/edit/p_class.txt#L68) | Class policy channel.
| extra_shot | Additional ranged shots for class and ammo constraints | Y | P | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2913) | Native class capability gated by equipped launcher/ammo.
| hp_bonus | Extra hitpoint progression | Y | N | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2011) | Native class progression only.
| pseudo_id_heavy | Pseudo-ID policy mode | Y | N | N | N | [reference-mangband-1_5_3/src/server/dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c#L154) | Pure policy capability.
| pseudo_id_improv | Pseudo-ID scaling policy | Y | N | N | N | [reference-mangband-1_5_3/src/server/dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c#L169) | Pure policy capability.
| speed_bonus | Class speed progression | Y | N | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2376) | Native class progression only.
| stealing_improv | Stealing success policy modifier | Y | N | N | N | [reference-mangband-1_5_3/src/server/cmd3.c](reference-mangband-1_5_3/src/server/cmd3.c#L1177) | Class policy, not item flag.
| stealth_mode | Search mode replacement policy | Y | N | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L382) | Behavior-mode capability.
| zero_fail | Failure floor policy | Y | N | N | N | [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L77) | Class-only casting policy.
| free_act | Paralysis immunity | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L102), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2285) | Exists in native and equipment channels.
| hold_life | Protection from life drain | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L92), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2286) | Exists in native and equipment channels.
| regen | HP regeneration boost | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L132), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2281) | Native and bearer channels.
| see_invis | Perception of invisible entities | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L152), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2284) | Native and bearer channels.
| feather | Falling protection | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2280), [reference-mangband-1_5_3/src/server/cmd1.c](reference-mangband-1_5_3/src/server/cmd1.c#L926) | Bearer passive effect.
| slow_digest | Food consumption reduction | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2279) | Bearer passive effect.
| telepathy | Mind sensing | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2283) | Bearer passive effect.
| infravision | Heat-based sight extension | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L58), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2267) | Racial base plus equipment modifiers.
| aggravate_monsters | Global monster wake and aggro pressure | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2274) | Bearer passive curse-like flag.
| teleportation | Random forced teleport behavior | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2275) | Bearer passive curse-like flag.
| stealth | Stealth stat modifier | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2315) | Item channel only in IronHell item capability file.
| slay_undead | Offensive slay multiplier | N | Y | N | N | [reference-mangband-1_5_3/src/server/init1.c](reference-mangband-1_5_3/src/server/init1.c#L393) | Offensive property, not a status.
| sust_str | Protects strength from drain | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L132), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2288) | Native and bearer channels.
| sust_int | Protects intelligence from drain | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2289) | Bearer passive.
| sust_wis | Protects wisdom from drain | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2290) | Bearer passive.
| sust_dex | Protects dexterity from drain | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L72), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2291) | Native and bearer channels.
| sust_con | Protects constitution from drain | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L142), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2292) | Native and bearer channels.
| sust_chr | Protects charisma from drain | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2293) | Bearer passive.
| res_acid | Acid resistance | P | Y | N | Y | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L995), [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L970) | Permanent resist plus temporary oppose acid.
| res_elec | Lightning resistance | N | Y | N | Y | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1020), [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L1021) | Permanent resist plus temporary oppose elec.
| res_fire | Fire resistance | N | Y | N | Y | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1043), [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L1072) | Permanent resist plus temporary oppose fire.
| res_cold | Cold resistance | N | Y | N | Y | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L1066), [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L1123) | Permanent resist plus temporary oppose cold.
| res_pois | Poison resistance | P | Y | N | Y | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L162), [reference-mangband-1_5_3/src/server/xtra2.c](reference-mangband-1_5_3/src/server/xtra2.c#L1174) | Permanent resist plus temporary oppose poison.
| res_fear | Fear resistance | P | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L662), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2298) | No oppose timer channel.
| res_lite | Light resistance | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L82), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2299) | Permanent only.
| res_dark | Darkness resistance | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L122), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2300) | Permanent only.
| res_blind | Blindness resistance | P | Y | N | N | [reference-mangband-1_5_3/lib/edit/p_race.txt](reference-mangband-1_5_3/lib/edit/p_race.txt#L112), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2301) | Permanent only.
| res_confu | Confusion resistance | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2302) | Permanent only.
| res_sound | Sound resistance | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2303) | Permanent only.
| res_shard | Shard resistance | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2304) | Permanent only.
| res_nexus | Nexus resistance | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2305) | Permanent only.
| res_nethr | Nether resistance | P | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L676), [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2306) | Ghost native path plus bearer path.
| res_chaos | Chaos resistance | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2307) | Permanent only.
| res_disen | Disenchant resistance | N | Y | N | N | [reference-mangband-1_5_3/src/server/xtra1.c](reference-mangband-1_5_3/src/server/xtra1.c#L2308) | Permanent only.
| ignore_acid | Item protection from acid destruction | N | N | Y | N | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L798) | Item-self only.
| ignore_elec | Item protection from electricity destruction | N | N | Y | N | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L811) | Item-self only.
| ignore_fire | Item protection from fire destruction | N | N | Y | N | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L824) | Item-self only.
| ignore_cold | Item protection from cold destruction | N | N | Y | N | [reference-mangband-1_5_3/src/server/spells1.c](reference-mangband-1_5_3/src/server/spells1.c#L837) | Item-self only.

## Findings

1. Scope is conceptually real, not just storage location.
2. Native identity capabilities are mainly class and race semantics, including policy capabilities not representable as timed statuses.
3. Bearer passive capabilities are runtime flags aggregated from equipment into player state.
4. Item-self passive capabilities are distinct from bearer passives and only protect items from destruction channels.
5. Temporary status equivalents exist mainly for elemental resist channels via oppose timers and should not be conflated with permanent passive resist capabilities.

## Architectural Implication

A single flat capability layer is insufficient for parity. At minimum, parity-safe modeling needs explicit separation between native identity, bearer passive, and item-self passive channels, with temporary status channels modeled separately from permanent capability grants.
