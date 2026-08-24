# Gameplay Parity Coverage Audit

## 1. Executive Summary

This is a coverage audit, not a validation audit. It compares the MAngband 1.5.3
gameplay systems documented in the repository's compendia and research notes
with the gameplay definition catalogs under `data/definitions`.

No code, gameplay data, schema, architecture, targeting, or activation file was
modified.

### Coverage conclusion

IronHell has broad definition coverage for character identity, core item
families, passive capabilities, resistances, statuses, player spells/prayers,
monster records, artifacts, ego items, and shop metadata. The strongest data
coverage is in the source catalogs themselves:

- 11 races and 6 classes;
- 38 stat-index labels;
- 46 canonical capabilities and 30 resistance definitions;
- 26 statuses;
- 59 armor, 65 weapon, 52 accessory, 116 ego-item, and 125 artifact records;
- 46 potions, 45 scrolls, 27 wands, 35 rods, 18 staves, and 27 consumables;
- 64 mage spells, 58 priest prayers, and 18 spell books;
- 616 monster records;
- 42 actions and 52 named artifact activation definitions.

The coverage is not yet a complete MAngband gameplay-definition set. The main
gaps are not additional runtime systems; they are missing or thin data domains
for world features, traps, and quests. Several other systems are represented
through compatibility structures or prose research rather than dedicated
definition catalogs: monster spell/AI policy, monster drops, breeding, uniques,
recall, death/ghost behavior, parties, trading, and multiplayer rules.

### Classification meaning

- **Fully Defined:** the repository contains a coherent data catalog or
  authoritative definition set covering the named gameplay domain.
- **Partially Defined:** meaningful MAngband data exists, but important domain
  records or variants remain outside a dedicated, complete catalog.
- **Compatibility Layer:** the repository preserves MAngband-shaped flags,
  names, serials, or prose as an interoperability/parity representation.
- **Missing:** a genuinely distinct gameplay-definition domain has no dedicated
  data catalog in the inspected repository.
- **Unresolved:** repository evidence establishes the MAngband system, but the
  appropriate canonical data boundary or parity coverage is not yet settled.

“Runtime dependency risk” identifies where data coverage depends on policy or
execution semantics. It is not a claim that runtime code should be added in
this audit.

## 2. Gameplay Coverage Matrix

| System | Exists in repository | Coverage | Definition evidence | Missing definitions | MAngband research needed | Runtime dependency risk |
|---|---|---|---|---|---|---|
| Character creation | Yes | Fully Defined | `races.json`, `classes.json`, `race_class_rules.json`, `stat_tables.json`, starting equipment; [Character Creation Specification](../implementation/CHARACTER_CREATION_SPECIFICATION.md) | None identified | Confirm any remaining birth-history edge cases | High: stat rolling, history, gold, and equipment sequencing are policy-sensitive |
| Races | Yes | Fully Defined | 11 records in `races.json`, with stat/skill modifiers and capabilities | None identified | Low; source parity is documented in character compendia | Medium: derived racial effects and capability application |
| Classes | Yes | Fully Defined | 6 records in `classes.json`, class stats, skills, spells, equipment, capabilities | None identified | Low; class source values are documented | Medium: class-native traits and spell policy |
| Stats | Yes | Fully Defined | Six-stat modifiers plus 38-entry `stat_tables.json`; character compendia document Angband scale | None identified | Low for definitions; verify source tables before changes | High: non-linear stat indices drive many derived outcomes |
| Skills | Yes | Partially Defined | Race/class `base_skills`, `skill_growth`, and skill references in compendia | No standalone skill-definition catalog | Confirm complete skill formulas and caps | High: skill values feed searching, devices, combat, saving throws, and stealth |
| Experience | Yes | Partially Defined | `exp_factor` in races/classes and monster experience fields | No dedicated experience curve/catalog | Confirm kill XP, level XP thresholds, drain, and multiplayer sharing | High: XP is affected by monster, class, depth, and drain policy |
| Level progression | Yes | Partially Defined | Class titles, `first_spell_level`, spell policy levels, and character specification | No complete player level progression table | Confirm all level thresholds, HP/mana rolls, spell slots, and title boundaries | High: progression combines stats, class, spells, and XP |
| Capabilities | Yes | Fully Defined | 46 records in `capabilities.json`; prior capability audit maps scopes and grants | Legacy aliases remain outside canonical catalog | Confirm remaining source-flag equivalences | High: passive behavior is policy/runtime dependent |
| Resistances | Yes | Fully Defined | 30 typed records in `resistances.json`, including resist/oppose/immunity/ignore | No missing core resistance family identified | Confirm monster/player interaction nuances | High: mitigation, timed oppose, immunity, and item-self semantics differ |
| Statuses | Yes | Fully Defined | 26 records in `statuses.json`, durations and status categories | No missing core status record identified | Confirm legacy aliases and source-specific clearing bundles | High: status stacking, timers, and secondary effects are policy-sensitive |
| Equipment | Yes | Fully Defined | Armor, weapons, accessories, lights, and item affixes catalogs | No core base-equipment family missing | Confirm all MAngband allocation and slot details | High: slot legality, weight, enchantment, and passive grants interact |
| Inventory | Yes | Partially Defined | Item catalogs and inventory layout in [Character Creation Specification](../implementation/CHARACTER_CREATION_SPECIFICATION.md) | No standalone inventory-layout/stack-rule definition catalog | Confirm pack/equipment limits, stacking, carrying, and transfer rules | High: pickup, stacking, destruction, and equipment mutation are policy-heavy |
| Consumables | Yes | Fully Defined | 27 records in `items/consumables.json` plus food/light-related definitions | None identified at family level | Confirm food, hunger, and side-effect variants | Medium: effects and nutrition depend on status and time policy |
| Potions | Yes | Fully Defined | 46 records in `items/potions.json`; potion compendium maps MAngband effects | None identified at family level | Review documented deviations such as salt-water paralysis bypass | Medium: quaff targeting and bundled effects |
| Scrolls | Yes | Fully Defined | 45 records in `items/scrolls.json` | None identified at family level | Confirm reading legality, blindness, and cursed-scroll policy | Medium: identification, targeting, and item consumption |
| Wands | Yes | Fully Defined | 27 records in `items/wands.json` | None identified at family level | Confirm charge use, stack splitting, and recharge semantics | High: charge economy is source-specific policy |
| Rods | Yes | Fully Defined | 35 records in `items/rods.json` | None identified at family level | Confirm timeout and stack availability rules | High: timeout economy is not equivalent to charges |
| Staves | Yes | Fully Defined | 18 records in `items/staves.json` | None identified at family level | Confirm shared charges and recharge behavior | High: charge pool policy differs from wands and rods |
| Ego items | Yes | Compatibility Layer | 116 records in `ego_items.json`; MAngband flags, xtra pools, and serial provenance | No separate canonical affix-generation policy catalog | Confirm random bonus pools and flag interactions | High: generated properties depend on item generation policy |
| Artifacts | Yes | Fully Defined | 125 named records in `artifacts.json`, including flags, depth, rarity, and activations | No core artifact record family missing | Confirm exact serial allocation and activation edge cases | High: unique generation, flags, and activations are coupled |
| Activations | Yes | Compatibility Layer | 52 named records in `activations.json`, referenced by artifact legacy IDs | No core activation record family missing | Confirm effect equivalence and legacy serial behavior | High: activation policy and source-specific payloads remain distinct |
| Mage spells | Yes | Fully Defined | 64 records in `mage_spells.json`, 18 books, action refs and policy metadata | None identified at catalog level | Confirm complete spell-book membership and lifecycle values | High: learning, mana, failure, targeting, and level gates |
| Priest prayers | Yes | Fully Defined | 58 records in `priest_prayers.json`, books, action refs and policy metadata | None identified at catalog level | Confirm realm-specific learning and prayer failure rules | High: prayer legality and divine spell policy differ from mage policy |
| Monster races | Yes | Fully Defined | 616 records in `monster_compendium.json` with identity, depth, HP, blows, flags, and spells | No base monster record family missing | Confirm all legacy flags and edge species semantics | High: monster flags drive movement, combat, AI, and drops |
| Monster spells | Yes | Partially Defined | Monster spell flags and spell projection research in `monster_compendium.json` and [Monster Action Projection](../archive/research/MONSTER_ACTION_PROJECTION.md) | No standalone monster-spell definition catalog | High: spell flag grammar, breaths, smart masks, and source-specific effects | High: selection and legality are AI policy, not payload data |
| Monster AI | Yes | Compatibility Layer | Monster intelligence/brain/smart flags and [Monsters Compendium](../../compendia/MONSTERS_COMPENDIUM.md) research | No dedicated AI policy definition catalog | High: smart casting, learning, desperation, LOS, and movement | Very high: AI behavior is explicitly policy-controlled |
| Monster drops | Yes | Partially Defined | Monster drop flags and depth/rarity data in `monster_compendium.json`; compendium drop rules | No dedicated drop-table or quality-policy catalog | Confirm stackable drop flags, chosen tables, and object-depth scaling | High: generation, quality, gold/item choice, and unique rules interact |
| Summoning | Yes | Partially Defined | `SummonEntities` action, monster spell flags, activation/effect records | No summon-family/count/depth policy catalog | Confirm summon groups, caps, exclusions, and source variants | High: summon selection is source and world-state policy |
| Breeding | Yes | Compatibility Layer | Monster `MULTIPLY` flag is documented in the monster compendium | No dedicated breeding/reproduction definition catalog | Confirm reproduction timing, limits, and species behavior | High: world processing and monster lifecycle semantics |
| Uniques | Yes | Partially Defined | Unique/boss flags and named monsters are present in `monster_compendium.json` | No dedicated unique progression/respawn policy catalog | Confirm unique allocation, death persistence, respawn, and multiplayer rules | High: unique lifecycle is world-state policy |
| Terrain | Research only | Missing | World Compendium documents MAngband wilderness biome and terrain generation | No terrain/biome definition catalog identified | High: seeded biome composition, wilderness coordinates, and terrain bleeding | High: terrain data feeds generation, movement, LOS, and effects |
| Dungeon features | Research only | Missing | World Compendium documents rooms, tunnels, vaults, streamers, rubble, and level ratings | No dungeon-feature/template definition catalog identified | High: room templates, vault sources, feature probabilities, and depth gates | High: generated world state and object/trap placement |
| Traps | Research only | Missing | [Trap Architecture Research](../archive/research/TRAP_ARCHITECTURE_RESEARCH.md) and effect inventory document floor/chest trap semantics | No floor-trap or chest-trap definition catalog identified | High: trap types, discovery, trigger, disarm, and payload mapping | Very high: trigger and chest lifecycle are policy concerns |
| Doors | Partial prose | Unresolved | World Compendium and trap research mention doors, secret doors, locked doors, and bashing | No dedicated door-state/door-generation definition catalog identified | Confirm locked/secret/closed/open/bashed state semantics | High: movement, monsters, traps, and terrain mutation interact |
| Stairs | Partial prose | Unresolved | World Compendium documents up/down stair placement and depth transitions; `RecallOrLevelShift` action exists | No stair/transition definition catalog identified | Confirm stair placement, quest stairs, and recall interaction | High: level transitions and player placement are world policy |
| Stores | Yes | Partially Defined | `shops/shop_owners.json`, `shop_rules.json`, and `shop_race_price_adjustments.json`; World Compendium store sections | No complete store inventory-choice catalog identified | Confirm stock tables, restock cadence, pricing, homes, and black market | High: economy, charisma, restocking, and multiplayer ownership |
| Quests | Research only | Missing | Monster and world compendia mention questors/quest behavior as MAngband systems | No quest definition catalog identified | High: quest objectives, rewards, questors, depth gates, and completion state | High: quest state is persistent world/player policy |
| Recall | Partial data | Partially Defined | `RecallOrLevelShift` action, scroll/rod sources, character spec `word_recall` field, effect inventory | No dedicated recall policy/transition catalog | Confirm timer, town/depth direction, interruption, and multiplayer rules | High: delayed transition and world persistence |
| Death | Partial prose | Partially Defined | Character compendium and character specification document death/revival behavior | No death/revival rule catalog identified | Confirm death causes, inventory loss, restart, and level persistence | High: player lifecycle and persistence |
| Ghosts | Yes | Partially Defined | Ghost monster records and MAngband ghost/death research exist | No player-ghost/revival definition domain identified | Confirm player ghost lifecycle versus ghost monster species | High: death, world state, and multiplayer visibility |
| Parties | Research only | Compatibility Layer | Multiplayer Compendium documents party, hostility, and PvP systems | No party rule or membership definition catalog identified | High: sharing, hostility, party persistence, and level visibility | Very high: shared authoritative world policy |
| Trading | Partial data | Partially Defined | Store catalogs and item definitions provide economy objects; multiplayer compendium documents player trading | No player-trade transaction/rule definition catalog identified | Confirm give/drop/trade legality, ownership, and pricing | High: inventory mutation and multiplayer authority |
| Multiplayer support | Research only | Compatibility Layer | Multiplayer Compendium documents server authority, parties, PvP, shared levels, and packet model | No multiplayer gameplay-definition catalog identified | High: MAngband server behavior and fairness rules | Very high: networking and shared-world policy |

## 3. Fully Defined Systems

The following domains have coherent gameplay definition coverage, not merely
names in prose:

- Races and classes, including primary/secondary stats, modifiers, skills,
  capabilities, titles, proficiencies, and starting equipment.
- Core stats and stat-index labels.
- Capabilities, typed resistances, and statuses.
- Base equipment families: armor, weapons, accessories, lights, and affixes.
- Consumables, potions, scrolls, wands, rods, and staves.
- Named artifacts and the artifact activation catalog.
- Mage spells, priest prayers, and spell books.
- Base monster records, including depth, rarity, attacks, flags, and spell
  references.

“Fully Defined” means the source-domain records exist and are sufficiently
structured for parity work. It does not claim that every MAngband policy or
runtime consumer is already represented.

## 4. Partially Defined Systems

These domains have substantial data but do not yet contain a complete,
source-shaped definition boundary:

- Skills, experience, and level progression: values are embedded in race/class
  records and specifications, but there is no complete standalone progression
  catalog.
- Inventory: item data and slot constants exist, but inventory state, stacking,
  carrying, and transfer rules are not a definition catalog.
- Monster spells, drops, summoning, and uniques: monster flags and actions carry
  much of the source identity, while source-specific policy remains in research
  prose.
- Stores: owner/rule/race-price catalogs exist, but the full stock and restock
  table domain is not represented as a complete repository catalog.
- Recall, death, ghosts, and trading: source effects, statuses, items, and
  prose exist, but the lifecycle rules are not captured in dedicated gameplay
  definitions.

## 5. Missing Systems

Only genuinely missing gameplay-definition domains are listed here. Runtime
systems and validation gaps are intentionally excluded.

### Terrain and biome definitions

The World Compendium documents wilderness types, terrain composition, seeded
generation, hotspots, dwellings, and day/night behavior, but no corresponding
terrain/biome definition catalog was found under `data/definitions`.

### Dungeon feature definitions

Rooms, vaults, tunnels, streamers, destroyed levels, rubble, and feature
placement are documented as MAngband generation behavior, but no dedicated
room/template/feature catalog was found.

### Trap definitions

Trap Architecture Research documents floor traps, chest traps, discovery,
trigger, disarm, and payload semantics. The repository has reusable actions and
effects references, but no trap-type or chest-trap definition catalog.

### Quest definitions

Questors and quest behavior are documented in the MAngband research material,
but no quest objective, reward, state, or questor definition catalog was found.

Doors and stairs are listed as **Unresolved** rather than Missing because the
world research establishes their generation and transition semantics and they
may be representable as world-feature policy records. This audit does not force
a new data boundary for them.

## 6. Unresolved MAngband Areas

These areas have authoritative MAngband evidence but require a parity decision
before definition work can be considered complete:

1. **Monster AI and spell selection:** MAngband smart casting, resistance
   learning, desperation masks, frequency checks, and line-of-sight constraints
   are policy semantics. Monster flags alone are not a complete AI definition.
2. **Device economies:** rods use timeout availability; wands and staves use
   charge pools with different stack behavior. A shared item-effect record does
   not resolve those policy differences.
3. **World generation:** seeded wilderness, room/vault templates, doors, stairs,
   traps, and level persistence are documented but not expressed as a single
   canonical world-definition vocabulary.
4. **Monster lifecycle:** breeding, unique persistence/respawn, monster drops,
   and summon selection depend on world state and depth policy.
5. **Death and ghosts:** the distinction between a player ghost/revival state
   and ghost monster records remains a domain-boundary question.
6. **Stores and trading:** shop stock, restocking, player-owned shops, pricing,
   and player-to-player transfer are described across separate research sources
   without one economy-definition model.
7. **Parties and multiplayer:** the Multiplayer Compendium documents the
   authoritative server model, shared levels, parties, PvP, and unique respawn,
   but there is no gameplay-definition representation for those policies.
8. **Legacy compatibility vocabularies:** ego-item flags, monster flags, named
   activations, and legacy status/capability aliases are intentionally preserved
   but have not all been assigned a single canonical data boundary.

## 7. Highest Priority Next Work

Priorities below are definition-research priorities only; they do not authorize
implementation or migration.

### 1. Establish the world-feature definition boundary

Highest parity impact and relatively low semantic risk if it begins as a
read-only catalog of MAngband feature identities and source references. Start
with terrain, doors, stairs, dungeon features, and traps because they control
large portions of exploration and are currently prose-only.

### 2. Capture monster policy inputs without flattening AI

Document monster spell frequency, smart flags, breath variants, drop flags,
breeding, unique markers, and summon families as explicit policy inputs. Keep
monster AI decisions separate from reusable action payloads, as required by
[Gameplay Architecture V2](../architecture/GAMEPLAY_ARCHITECTURE_V2.md).

### 3. Complete progression data as a source-shaped catalog

Separate embedded skill growth, experience thresholds, level milestones, spell
slots, HP/mana progression, and titles into a documented progression model only
after confirming the MAngband source tables. This has high gameplay impact but
should follow world and monster evidence so cross-system dependencies are clear.

### 4. Resolve economy definition coverage

Consolidate store stock choices, restocking, pricing modifiers, player-owned
shops, trading constraints, and device charge/timeout metadata as definition
research. Preserve rod, wand, and staff economy distinctions.

### 5. Define lifecycle records for quests, death, ghosts, and recall

These systems affect persistence and multiplayer parity. They should be
researched together because recall, death/revival, unique persistence, quest
state, and shared-level behavior overlap in world lifecycle semantics.

## 8. Recommended Definition Milestones

### Milestone A — World vocabulary inventory

Produce a read-only inventory of MAngband terrain features, doors, stairs,
rooms, vaults, streamers, traps, and level-generation markers. Record source
identities, depth constraints, flags, and relationships without choosing a new
runtime representation.

### Milestone B — Monster policy input inventory

Catalog monster spell frequencies, spell groups, breath families, drop flags,
smart-casting flags, movement flags, reproduction flags, unique markers, and
summon relationships. Preserve legacy MAngband terminology until each mapping
has evidence.

### Milestone C — Progression and skill inventory

Trace all class/race skill formulas, experience tables, level thresholds,
titles, spell slots, HP/mana rolls, and stat-index lookups to authoritative
MAngband sources. Identify embedded values that are definition data versus
policy formulas.

### Milestone D — Economy and device inventory

Document store tables, owner/race pricing, restock rules, player-owned shop
metadata, trading constraints, rod timeout, wand charges, staff charges, and
recharge outcomes as separate source policies.

### Milestone E — Lifecycle and multiplayer inventory

Document quest state, recall timers, death/revival, ghosts, unique persistence,
parties, shared levels, PvP/hostility, and player trading. Mark which records
are player-local, level-local, or world-global.

### Milestone F — Coverage sign-off

Re-run this matrix against the completed definition inventories. A domain should
move to Fully Defined only when its MAngband source identity, variants,
relationships, and parity-sensitive parameters are documented. No migration or
runtime change is implied by that sign-off.

## Evidence Base

Primary repository evidence used for this audit:

- [Gameplay Architecture V2](../architecture/GAMEPLAY_ARCHITECTURE_V2.md)
- [Character Creation Specification](../implementation/CHARACTER_CREATION_SPECIFICATION.md)
- [Repository Integrity Audit](../archive/audits/REPOSITORY_INTEGRITY_AUDIT.md)
- [Capability and Progression Audit](../archive/audits/CAPABILITY_AND_PROGRESSION_AUDIT.md)
- [Character Compendium](../../compendia/CHARACTER_COMPENDIUM.md)
- [Magic Compendium](../../compendia/MAGIC_COMPENDIUM.md)
- [Monsters Compendium](../../compendia/MONSTERS_COMPENDIUM.md)
- [Systems Compendium](../../compendia/SYSTEMS_COMPENDIUM.md)
- [World Compendium](../../compendia/WORLD_COMPENDIUM.md)
- [Multiplayer Compendium](../../compendia/MULTIPLAYER_COMPENDIUM.md)
- [MAngband Effect Inventory](../archive/research/MANGBAND_EFFECT_INVENTORY.md)
- [Monster Action Projection](../archive/research/MONSTER_ACTION_PROJECTION.md)
- [Trap Architecture Research](../archive/research/TRAP_ARCHITECTURE_RESEARCH.md)
- [Device Semantics Analysis](../archive/research/DEVICE_SEMANTICS_ANALYSIS.md)
- [Shared Effect Matrix](../archive/research/SHARED_EFFECT_MATRIX.md)

The MAngband source authority is the 1.5.3 reference tree cited by those
documents, especially `birth.c`, `xtra1.c`, `xtra2.c`, `tables.c`,
`generate.c`, `melee1.c`, `melee2.c`, `use-obj.c`, `cmd2.c`, `cmd6.c`,
`dungeon.c`, `store.c`, `party.c`, and the corresponding `lib/edit` data
files.