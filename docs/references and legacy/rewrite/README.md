# Rewrite Data Architecture Blueprint

This folder contains a new data-driven content architecture for the next-generation rewrite.
It is intentionally isolated from the current runtime so it can be developed in parallel.

## Goals

- Move gameplay balancing data out of code and into validated JSON.
- Preserve MAngband parity where needed while enabling modern live-ops workflows.
- Add schemas and manifest/versioning from day one.

## Folder Layout

- data/rewrite/content_manifest.json
- data/rewrite/terrain_features.json
- data/rewrite/vault_templates.json
- data/rewrite/shop_owners.json
- data/rewrite/shop_rules.json
- data/rewrite/shop_race_price_adjustments.json
- data/rewrite/character_history_charts.json
- data/rewrite/spell_books.json
- data/rewrite/spell_progression_by_class.json
- data/rewrite/spell_metadata.json
- data/rewrite/dungeon_generation_rules.json
- data/rewrite/wilderness_terrain_rules.json
- data/rewrite/wilderness_hotspots.json
- data/rewrite/simulation_limits.json
- data/rewrite/social_actions.json
- data/rewrite/economy_rules.json
- data/rewrite/balance_targets.json
- data/rewrite/monster_ai_profiles.json
- data/rewrite/content_migrations.json
- data/rewrite/savegame_schema.json
- data/rewrite/i18n/en/content_text.json
- data/rewrite/network_messages.json
- data/rewrite/savegame_compatibility_matrix.json
- data/rewrite/schemas/*.schema.json

## Minimal Schema Contracts (Per File)

1) content_manifest.json
- version: string
- dataPackId: string
- generatedAt: string (ISO timestamp)
- files: array of { id, path, schemaPath, version, checksum }

2) terrain_features.json
- version: string
- features: array of { id, index, name, symbol, color, blocksLos, walkable, trapType?, shopType?, appearsAsFeatureId? }

3) vault_templates.json
- version: string
- templates: array of { id, name, roomType, rating, rows, cols, layout, tags }

4) shop_owners.json
- version: string
- ownersByShopType: object keyed by shop type -> owner array
- owner: { id, name, raceId, purse, greed, maxGreed }

5) shop_rules.json
- version: string
- restock: { turns, minKeep, maxKeep, turnover, shuffleChance }
- pricing: { blackMarketMinValue, baseGreed, priceAdjustmentModel }
- stockPools: object keyed by shop type with selection-weighted entries

6) shop_race_price_adjustments.json
- version: string
- raceOrder: array of race ids
- matrix: object keyed by race id -> array of adjustment numbers

7) character_history_charts.json
- version: string
- templates: tokens and gender forms
- charts: array of { id, fromNode, toNode, rollCeiling, socialClass, text }
- raceStartNodes: map of race id -> start node

8) spell_books.json
- version: string
- meta: { contract, idFormat, linkKey, pairedWith, notes }
- realms: array of { id, classes, books }
- book: { id, bookIndex, spells }
- book.spell: { spellId, spellIndex, spellName }

9) spell_progression_by_class.json
- version: string
- progressionByClass: object keyed by class id -> progression list
- progression item: { spellIndex, level, mana, fail, exp }

10) spell_metadata.json
- version: string
- meta: { contract, idFormat, linkKey, pairedWith, sources, notes }
- spells: array of {
	spellId, realm, spellIndex, name, bookIndex,
	targeting { mode, requiresLineOfSight, canTargetSelf, canTargetAlly, canTargetEnemy },
	casting { slevel, smana, sfail },
	effects [{ effectType, summary, damageModel? }]
}

11) dungeon_generation_rules.json
- version: string
- constants: object (rooms, blocks, tunnel params)
- stairs: object
- trapDistributionByDepth: array
- roomTypeWeightsByDepth: array

12) wilderness_terrain_rules.json
- version: string
- world: { width, height, centerX, centerY }
- terrainTypes: array of { id, baseDensities, depthModel, ringRules }

13) wilderness_hotspots.json
- version: string
- hotspotRules: array of { terrainType, shape, chance, minRadius, maxRadius, tileFactory }

14) simulation_limits.json
- version: string
- limits: { maxFeatureTypes, maxObjectsOnLevel, maxMonstersOnLevel, maxVaults, ... }

15) social_actions.json
- version: string
- actions: array of { id, minDistance, noTargetLines, targetLines }

16) economy_rules.json
- version: string
- goldSources, goldSinks, inflation, rarityCurves, sellFloorRules

17) balance_targets.json
- version: string
- combat, progression, economy, and encounter pacing targets

18) monster_ai_profiles.json
- version: string
- profiles: array of { id, tactics, aggroRules, retreatRules, targetingRules, spellPolicy }

19) content_migrations.json
- version: string
- migrations: array of { id, fromVersion, toVersion, kind, steps }

20) savegame_schema.json
- version: string
- entities, inventory, world, progression, metadata contracts

21) i18n/en/content_text.json
- version: string
- strings grouped by domain ids for localization-ready text separation

22) network_messages.json
- version: string
- messages: array of { id, direction, reliable, schemaRef, deprecated }

23) savegame_compatibility_matrix.json
- version: string
- currentSaveVersion: string
- compatibility: array of { fromVersion, toVersion, strategy, migrationScript? }

## Detailed Build Plan

Phase 1 - Foundation
1. Create all new domain JSON files with stable ids and version keys.
2. Create one schema file per domain.
3. Add all domain files to content_manifest.json.

Phase 2 - Loader Integration
1. Implement a new rewrite loader module that validates against schemas.
2. Keep existing loader intact and run both in parallel behind a feature flag.
3. Add deterministic snapshot tests for all parsed domains.

Phase 3 - Gameplay Wiring
1. Move shop owners and stock pools from code to shop_owners and shop_rules.
2. Move spell book/progression tables from code to spell files.
3. Move dungeon and wilderness constants into rule files.

Phase 4 - MAngband Parity Sweep
1. Cross-check terrain, vaults, history charts, and race pricing against lib/edit files.
2. Add parity tests that compare expected sample rows/entries.

Phase 5 - Live Ops Readiness
1. Add checksums and semantic version gates in manifest.
2. Add migration scripts and compatibility matrix updates per content release.
3. Add CI checks for schema validation and referential integrity.

## Current Progress

- terrain_features.json: full parity with terrain.txt (92 of 92 feature entries, max index 135).
- vault_templates.json: partial parity with vault.txt (21 of 149 templates, imported through serial 23).
- shop_owners.json: full classic owner pools imported from shop_own.txt.
- shop_race_price_adjustments.json: full cost_adj matrix imported from cost_adj.txt.
- character_history_charts.json: partial parity with p_hist.txt (40 of 165 chart rows).
- social_actions.json: partial parity with socials.txt (8 of 66 socials).
- shop_rules.json: stock pools expanded from MAngband-derived store_table parity data already captured in the current codebase.
- spell_books.json: full realm book mappings imported with explicit spell names for AI readability.
- spell_progression_by_class.json: fully expanded from canonical in-repo progression tables for mage, priest, rogue, ranger, paladin (5 x 64 entries).
- spell_metadata.json: refactored to a runtime-focused portable model with full realm index coverage (128 entries), canonical `spellId` linkage, and explicit damage models for known damage spells.

Spell metadata enrichment sources:
- compendia/MAGIC_COMPENDIUM.md
- compendia/PRAYERS_COMPENDIUM.md

Runtime-focused spell metadata notes:
- `spellId` (`<realm>-<index>`) is the canonical cross-file key shared by `spell_books.json` and `spell_metadata.json`.
- Removed non-runtime/provenance fields from spell metadata: `sourceRefs`, `aiHints`, `category`, `type`, `targetMode`, `targeting.description`, and `book` allocation/store payloads.
- `targeting` + `casting` now contain only implementation-critical values needed for effect execution and cast validation.
- `effects[].damageModel` is present for known damage spells and mirrors IronHell runtime effect catalog intent (effect key, shape, radius, damage type, scaling).

## MAngband Reference Gap Analysis (2026-07-03)

Mapped to rewrite domains now:
- terrain.txt -> terrain_features.json (complete)
- vault.txt -> vault_templates.json (partial)
- p_hist.txt -> character_history_charts.json (partial)
- socials.txt -> social_actions.json (partial)
- shop_own.txt -> shop_owners.json (complete)
- cost_adj.txt -> shop_race_price_adjustments.json (complete)

Reference files covered by existing legacy data files (not yet promoted into rewrite-pack domains):
- artifact.txt -> data/artifacts.json (125 artifacts)
- ego_item.txt -> data/ego_items.json (116 ego templates)
- flavor.txt -> data/flavors.json (102 flavor rows)
- object.txt -> data/item_templates.json (237 templates across weapon/armor/accessory/consumable/material/chest)
- p_class.txt + p_race.txt -> data/races_and_classes.json (11 races, 6 classes, 44 race/class combos)
- monster.txt -> data/monster_compendium.json (616 monsters with serial parity metadata)

Reference files still missing dedicated data domains:
- randarts.txt (randart generation parameters parity domain still missing)
- limits.txt (full low-level limits parity map still missing; simulation_limits is currently partial/curated)

## Legacy Core Data Audit (2026-07-03)

Analyzed files:
- data/artifacts.json
- data/ego_items.json
- data/flavors.json
- data/item_templates.json
- data/races_and_classes.json
- data/monster_compendium.json

Dataset size snapshot:
- artifacts: 125 entries
- ego items: 116 entries (+ rich _meta block)
- flavors: 102 entries
- item templates: 237 total entries (65 weapons, 63 armor, 49 accessories, 39 consumables, 14 materials, 7 chests)
- races/classes: 11 races, 6 classes, 44 compatibility combos
- monster compendium: 616 entries (+ integrity and portability _meta)

Schema posture snapshot:
- Root-level schemas for all six files are strict (additionalProperties=false at document root).
- Strongly strict item-level schema: flavors and monster_compendium.
- Partially permissive item-level schema: artifacts, ego_items, item_templates, races_and_classes currently allow additionalProperties in nested item objects for forward compatibility.

Documentation impact:
- These six files should be treated as current parity-ready legacy sources and migration feeders.
- Rewrite gap analysis should track two states separately: (1) source data exists in legacy files, (2) source has been promoted into rewrite-pack domain + schema + manifest entry.
- Future parity work for artifact/ego/flavor/object/class/race/monster should prioritize transformation and normalization into data/rewrite/* rather than re-sourcing raw reference text from scratch.

## Immediate Next Tasks

1. Continue vault_templates import from serial 24 onward until full vault.txt parity.
2. Expand character_history_charts to cover all 165 p_hist.txt rows and normalize template token handling.
3. Expand social_actions from 8 to full 66 socials with accurate no-target and target line variants.
4. Promote legacy parity datasets into rewrite domains: artifacts, ego items, flavors, object templates, races/classes, monsters.
5. Create missing parity domains for randarts and full limits.
6. Implement the rewrite loader and schema validation pipeline.
7. Replace hardcoded sources behind a rewrite feature toggle.
