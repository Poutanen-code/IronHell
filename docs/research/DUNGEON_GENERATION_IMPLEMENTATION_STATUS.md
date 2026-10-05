# Dungeon Generation Implementation Status

**Checkpoint date:** 2026-10-05

This document is the repository-tracked implementation handoff for IronHell dungeon-generation work. It records current implementation status, not new parity research.

The repository is authoritative for current structure and behavior. `DUNGEON_GENERATION_PARITY_SPEC.md` is the primary behavioral contract. Frozen research documents remain frozen; they are not rewritten here. The implementation must continue to distinguish verified MAngband behavior from IronHell infrastructure introduced to make progress.

The requested `DUNGEON_GENERATION_COMPENDIUM.md` is not present in the current checkout. The available frozen research sources are the parity specification, gap audit, and final research audit under `docs/research/`.

**Warning:** the existence of infrastructure does not mean MAngband parity is complete.

## 1. Executive Status

### Current milestone

**Room, Connectivity, Door, Stair, and Prepared Nest/Pit Placement Foundations**

IronHell can now execute this bounded path in Core:

```text
MonsterDefinition
    -> base allocation entries
    -> hook-filtered prepared weights
    -> OOD/effective eligibility
    -> weighted selection and candidate comparison
    -> explicit runtime placement
    -> ordinary request accounting
    -> FRIENDS expansion
    -> escort filtering, selection, and placement
```

The current implementation is deterministic, Godot-independent, and tested through Core xUnit tests. It is not a complete dungeon generator. The verified 50-attempt dispatcher orchestrates all eight room families, room centers connect through the bounded source-style tunnel foundation, and tunnel/room/vault doors execute. Global stairs and room `RandomStair` requests execute for the represented scope; prepared nest/pit monsters now place at their recorded coordinates. Ordinary/vault monster content, objects, gold, artifacts, traps, and dungeon lifecycle behavior remain incomplete.

Completed foundation areas are REPO-CONFIRMED:

- deterministic RNG boundary and controlled tests;
- allocation entry construction;
- allocation hook filtering;
- initial weighted selection;
- additional candidate comparison;
- two-roll OOD transformation;
- effective allocation eligibility;
- post-allocation placement eligibility;
- runtime monster identity and occupancy;
- finite placement-space legality;
- bounded ordinary location-search infrastructure;
- ordinary request count and failure-aware request processing;
- FRIENDS group expansion;
- escort filtering, allocation, scatter, and placement.
- nest family selection, family predicates, and 64-candidate preparation.
- pit family selection, dragon mask selection, 16-candidate preparation, sorting, and tier extraction.
- nest and pit candidate preparation remains authoritative and is now consumed by spatial room builders.
- prepared nest/pit monster attempts flow from spatial builders into exact-position runtime placement with occupancy and group expansion suppressed.
- authoritative 198x66 dungeon grid, 11x11 room blocks, atomic reservation, cell flags, and room centers;
- type-3 cross and type-4 large room geometry, all verified internal variants, and ordered deferred room-content attempts.
- source-order 50-attempt room dispatcher for all simple, overlapping, cross, large, nest, pit, and vault builders.
- explicit granite initialization, cyclic room-center connectivity, bounded tunnel carving, outer-wall piercing, and deferred junction candidates.
- distinct source-order tunnel entrance and junction door passes using stable door terrain IDs and the injected RNG stream.
- ordered room/vault door execution, stair execution, and prepared nest/pit monster placement; unsupported content requests remain deferred.
- ordinary global stair allocation with source count, candidate, wall-relaxation, and direction order.
- source-time prepared room `RandomStair` direction plus ordered stair-only request execution; other content remains deferred.
- prepared nest/pit monster requests now place exact runtime definitions at their recorded positions without allocation or group expansion.

Stair legality remains partial: nest/pit monster occupancy is now authoritative but `DungeonStairAllocator` does not consult it; object occupancy and level-entry coordinates are still absent. Town generation and complete quest lifecycle remain conditional.

## 2. Completed Implementation Slices

### Slice 1A — RNG test-controllability foundation

- **Outcome:** preserved the existing Core-safe `IRandomSource` / `SeededRandomSource` boundary and proved scripted integer boundaries and sequential consumption.
- **Requirements:** DG-RNG-001 foundation.
- **Implementation:** `src/IronHell.Core/Randomness/IRandomSource.cs`, `SeededRandomSource.cs`; focused coverage also exists in `src/IronHell.Core.Tests/Randomness/SeededRandomSourceTests.cs` and `Items/FlavorServiceTests.cs`.
- **Decision:** no replacement RNG abstraction was introduced. Core receives randomness explicitly.
- **Status:** SATISFIED for the current implemented behaviors. Complete whole-dungeon replay is not yet demonstrable because the dungeon generator does not exist.

### Slice 1B — Allocation entry construction

- **Outcome:** immutable entries preserve stable definition ID, native level, and integer base weight `100 / rarity`; rarity-zero definitions are excluded and entries are ordered by native level.
- **Requirements:** DG-MON-001.
- **Implementation:** `MonsterAllocationTableBuilder.cs`, `CatalogDefinitions.cs`, `MonsterDefinitionReader.cs`.
- **Tests:** `MonsterAllocationTableTests.cs` and `MonsterDefinitionReaderTests.cs`.
- **Status:** SATISFIED.

### Slice 1C — Allocation hook filtering

- **Outcome:** explicit predicates produce immutable prepared entries; accepted entries retain base weight and rejected entries remain present with zero prepared weight.
- **Requirements:** DG-MON-002 filtering portion.
- **Implementation:** `MonsterAllocationTableBuilder.Prepare`, `MonsterAllocationPreparedEntry`.
- **Tests:** `MonsterAllocationTableTests.cs`.
- **Decision:** no global active hook; definitions are passed explicitly.
- **Status:** SATISFIED for hook preparation.

### Slice 1D — Initial weighted selection

- **Outcome:** positive prepared weights are summed and selected through one cumulative `Next(0, totalWeight)` draw; zero total returns no selection without RNG.
- **Requirements:** DG-MON-002 selection portion; DG-RNG-001.
- **Implementation:** `MonsterAllocationSelector.Select`.
- **Tests:** `MonsterAllocationTableTests.cs`.
- **Status:** SATISFIED for the implemented prepared/effective selection stages.

### Slice 1E — Additional candidate comparison

- **Outcome:** one shared `Next(0,100)` control draw governs one or two additional weighted picks; sampling is with replacement and strict smaller absolute native level wins.
- **Requirements:** DG-MON-002 comparison portion.
- **Implementation:** `MonsterAllocationSelector.SelectWithComparison`.
- **Tests:** `MonsterAllocationTableTests.cs`.
- **Status:** SATISFIED.

### Slice 1F — OOD and effective allocation eligibility

- **Outcome:** positive requested levels consume two independent `Next(0,50)` rolls; each success applies `min(level / 4 + 2, 5)` using the already modified level. Effective entries retain prepared state and derive separate effective weights.
- **Requirements:** DG-MON-003; remaining allocation eligibility in DG-MON-002.
- **Implementation:** `MonsterAllocationLevelResolver.cs`, `MonsterAllocationEligibility.cs`, effective selector overloads.
- **Tests:** `MonsterAllocationEligibilityTests.cs`.
- **Status:** SATISFIED for current represented allocation state.

### Slice 1G — Placement eligibility boundary

- **Outcome:** selected definitions can be rejected after allocation when unique capacity is unavailable; ordinary definitions pass unique-only checks.
- **Requirements:** DG-MON-004 boundary.
- **Implementation:** `MonsterPlacementEligibility.cs`.
- **Decision:** caller-owned `UniqueHasRemainingCapacity` was retained as a lower-level input until runtime ownership existed.
- **Status:** PARTIAL overall because player/level unique eligibility is not represented.

### Slice 2A — Runtime placement and occupancy foundation

- **Outcome:** explicit positions can atomically place one runtime monster; occupancy and one-concurrent-instance unique capacity are authoritative in Core runtime state; IDs are deterministic (`monster-1`, etc.).
- **Requirements:** DG-MON-004 placement boundary.
- **Implementation:** `MonsterPosition.cs`, `MonsterRuntimeState.cs`, `MonsterPlacementService.cs`.
- **Tests:** `MonsterPlacementTests.cs`.
- **Decision:** no runtime fields were added to `MonsterDefinition`; no HP/AI/combat state was added to the minimal runtime instance.
- **Status:** PARTIAL overall because terrain and player/level rules are not modeled.

### Slice 2B — Placement-space and location-search foundation

- **Outcome:** finite width/height bounds, explicit illegal cells, occupancy-aware availability, and bounded deterministic coordinate search were added.
- **Requirements:** infrastructure for DG-MON-004/DG-MON-005.
- **Implementation:** `MonsterPlacementSpace.cs`, `MonsterLocationSearch.cs`; `MonsterPlacementService` accepts optional space validation.
- **Tests:** `MonsterPlacementSpaceTests.cs`.
- **Classification:** IRONHELL-PROPOSED INFRASTRUCTURE. Exact ordinary MAngband `alloc_monster` location-search behavior is not frozen sufficiently.
- **Status:** infrastructure present; not a claim of full terrain parity.

### Slice 2C — Ordinary request count and failure-aware population

- **Outcome:** exact request-count formula and a fixed number of independent requests were implemented. Failed location search, allocation, or placement does not extend the request budget.
- **Requirements:** DG-MON-005.
- **Implementation:** `OrdinaryMonsterRequestCount.cs`, `OrdinaryMonsterPopulation.cs`, `OrdinaryMonsterPopulationOptions.cs`.
- **Tests:** `OrdinaryMonsterPopulationTests.cs`.
- **Decision:** `RequestedCount`, leader `SuccessfulPlacements`, and `TotalRuntimeMonstersAdded` remain distinct.
- **Status:** PARTIAL because ordinary location-search/call-order parity remains proposed infrastructure.

### Slice 2D — FRIENDS expansion

- **Outcome:** verified group-size adjustment, GROUP_MAX=32, explicit suppression, eight-way adjacency, and breadth-first same-race expansion were implemented.
- **Requirements:** DG-GROUP-001.
- **Implementation:** `MonsterGroupSizeCalculator.cs`, `MonsterGroupExpander.cs`; population options expose explicit group expansion.
- **Tests:** `MonsterGroupExpansionTests.cs` and population integration coverage.
- **Status:** SATISFIED for the verified represented behavior.

### Slice 2E — Escort expansion

- **Outcome:** existing monster symbols are mapped into Core; escort candidates are filtered by symbol, leader level, uniqueness, and leader identity; selection uses normal allocation weights; 50 scatter attempts and escort FRIENDS/ESCORTS behavior are implemented.
- **Requirements:** DG-GROUP-002.
- **Implementation:** `MonsterEscortEligibility.cs`, `MonsterEscortExpansionOptions.cs`, `MonsterEscortExpander.cs`; `MonsterGroupExpander.cs` supports forced escort groups; population integrates escorts after FRIENDS.
- **Tests:** `MonsterEscortTests.cs`; symbol mapping coverage is in `MonsterDefinitionReaderTests.cs`.
- **Status:** PARTIAL overall because full LOS/terrain escort legality is not represented by the minimal placement space.

### Slice 3A — Nest family filtering and candidate sampling

- **Outcome:** selected the verified jelly/animal/undead nest family, applied an isolated family predicate through the existing allocation pipeline, sampled exactly 64 candidates with replacement at `Depth + 10`, and aborted explicitly when a required sample had no candidate.
- **Requirements:** DG-NEST-001, DG-NEST-002.
- **Implementation:** `MonsterNestPreparer.cs`; existing `MonsterAllocationTableBuilder`, `MonsterAllocationEligibility`, and `MonsterAllocationSelector` are reused.
- **Data mapping:** existing monster `categories` were mapped into `MonsterDefinition.Categories`; existing `symbol` mapping remains the source for jelly predicates and escort behavior. JSON and schemas were unchanged.
- **Tests:** `MonsterNestPreparationTests.cs`; reader coverage remains in `MonsterDefinitionReaderTests.cs`.
- **Decision:** the result contains only family and ordered candidate definition IDs. No room, terrain, runtime state, placement, FRIENDS, escort, or pit behavior is included.
- **Status:** SATISFIED for the frozen non-spatial candidate-preparation behavior.

### Slice 3B — Pit family filtering and candidate tiers

- **Outcome:** selected the verified orc/troll/giant/dragon/demon family, selected dragon masks, applied isolated pit predicates, sampled exactly 16 candidates with replacement at `Depth + 10`, sorted by native level, and retained sorted indexes `0,2,4,6,8,10,12,14` as eight tiers.
- **Requirements:** DG-PIT-001, DG-PIT-002.
- **Implementation:** `MonsterPitPreparer.cs`; existing allocation preparation, effective eligibility, and weighted comparison selection are reused.
- **Data mapping:** existing monster `abilities` were mapped into `MonsterDefinition.Abilities` for the current breath-mask representation. JSON and schemas were unchanged.
- **Tests:** `MonsterPitPreparationTests.cs`; affected reader coverage remains in `MonsterDefinitionReaderTests.cs`.
- **Decision:** result contains family, dragon mask where applicable, sampled IDs, sorted IDs, and tier IDs only. No room, terrain, runtime state, placement, FRIENDS, or escort behavior is included.
- **Status:** PARTIAL: non-spatial family/sampling/tier behavior is implemented, but exact source `flags4` equivalence is represented through existing `breath_*` ability IDs rather than a dedicated source-flags model.

### Slice 4A — Minimal dungeon grid and room reservation foundation

- **Outcome:** added the authoritative 198x66 Core dungeon grid, 11x11 room-block partition, atomic rectangular reservation, composable room/icky cell state, and deterministic room-center storage.
- **Requirements:** DG-ROOM-001; representation/reservation foundation for DG-ROOM-006.
- **Implementation:** `src/IronHell.Core/Dungeon/DungeonGrid.cs`.
- **Tests:** `src/IronHell.Core.Tests/Dungeon/DungeonGridTests.cs`.
- **Decision:** dungeon coordinates use explicit row/column types separate from monster positions. Room reservation commits the full footprint and center atomically; no room geometry or terrain features are implemented.
- **Status:** DG-ROOM-001 SATISFIED for reservation-grid behavior; DG-ROOM-006 PARTIAL because room builders/floor-cell creation do not yet exist.

### Slice 4B — Room selection foundation and simple/overlapping geometry

- **Outcome:** added verified room-family metadata, source-order type-1 simple-room geometry, source-order type-2 overlapping-room geometry, stable floor/outer-wall/inner-wall feature writes, Glow state, and room-state/center integration.
- **Requirements:** DG-ROOM-002 foundation; DG-ROOM-003; DG-ROOM-004; DG-ROOM-006 for implemented builders; DG-ROOM-007; DG-ROOM-008.
- **Implementation:** `RoomFamilyMetadata.cs`, `RoomGeometryBuilder.cs`, and `DungeonGrid.cs` feature/state support.
- **Tests:** `RoomGeometryBuilderTests.cs`; Slice 4A `DungeonGridTests.cs` remains green.
- **Decision:** stable existing terrain IDs `open_floor`, `granite_wall_outer`, and `granite_wall_inner` are referenced by Core; no full feature catalog or room-attempt loop was introduced.
- **Status at 4B:** DG-ROOM-003/004/006/007/008 SATISFIED for the then-implemented metadata/builders; the later 4F/4G slices close the dispatcher boundary.

### Slice 4C — Cross and large rooms

- **Outcome:** implemented verified type-3 cross and type-4 large-room base geometry, all four type-3 variants, all five type-4 variants, source-order nested RNG, and one-center/one-reservation integration.
- **Requirements:** DG-ROOM-005; DG-ROOM-006 for type-3/type-4 behavior; DG-ROOM-009; DG-ROOM-010.
- **Implementation:** `RoomGeometryBuilder.cs`, `RoomContentAttempt.cs`, and the existing `DungeonGrid.cs` feature/state foundation.
- **Content boundary:** room-local secret-door, locked-door, monster, trap, object, object/gold, special-object, and random-stair effects are recorded as ordered immutable attempts. They are requests, not successful placements; no runtime monster/object/trap/stair state is created.
- **Tests:** `RoomGeometryBuilderTests.cs` covers base shapes, variant selection, nested branches, checkerboard/four-room layouts, attempt ordering/counts, 80/20 object/stair branching, atomic failure, and deterministic replay.
- **Narrow source check:** `generate.c :: build_type3()`, `build_type4()`, `place_cross_room()`, `place_double_room()`, and directly required helper call shapes only.
- **Status at 4C:** DG-ROOM-005/009/010 SATISFIED; DG-ROOM-006 SATISFIED for then-current ordinary room geometry and state behavior; the later 4F/4G slices close room dispatch.

### Slice 4E — Vault interpretation and spatial vault builders

- **Outcome:** added immutable vault definitions, lesser/greater eligibility and source-range retry selection, one-time type-9 normalization, source-compatible two-pass glyph interpretation, local row/column mapping, exact lesser/greater reservation footprints, Room/Icky state, stable terrain writes, and ordered deferred content attempts.
- **Requirements:** DG-VAULT-001; DG-VAULT-002; DG-ROOM-006 vault state; DG-RNG-001 deterministic stream behavior. DG-ROOM-002 was deferred at the 4E checkpoint and is addressed by the subsequent 4F/4G dispatcher slices.
- **Implementation:** `VaultDefinition.cs`, `VaultRoomBuilder.cs`, `VaultDefinitionReader.cs`, and the `Vaults` definition registry.
- **Catalog:** current `data/definitions/environment/vaults.json` contains 10 migrated records: 7 lesser and 3 greater. No type-9 record is present in the current catalog. The source inventory remains 67 type-7, 75 type-8, and 7 type-9 records; missing source content is parity debt, not invented here.
- **Content boundary:** monsters, objects, traps, and secret doors remain ordered deferred `RoomContentAttempt` requests. No runtime entity or rating/feeling state is created.
- **Status:** DG-VAULT-001/002 SATISFIED for the represented definition shape and glyph contract; full source-content parity remains partial because the repository catalog is incomplete.

### Slice 4F — Complete room-attempt selection and dispatcher

- **Outcome:** added the verified 50-attempt room loop, row/column block draws, conditional unusual and very-unusual rolls, source-order family branches, minimum-depth/footprint preflight, existing ordinary/vault builder dispatch, failure accounting, and ordered successful-room results.
- **Requirements:** DG-ROOM-002; DG-RNG-001; DG-ROOM-001/003-010 traceability.
- **Implementation:** `RoomDispatcher.cs` and `RoomDispatcherTests.cs`.
- **Status:** SATISFIED for the verified dispatcher behavior. Simple, overlapping, cross, large, nest, pit, lesser-vault, and greater-vault branches dispatch through bounded builders. Content execution and complete source-content parity remain separate concerns.

### Slice 4G — Nest/Pit spatial-dispatch closure

- **Outcome:** restored the bounded type-5/type-6 spatial builders, reused `MonsterNestPreparer` and `MonsterPitPreparer`, emitted exact prepared `DefinitionId` requests, preserved the fixed nest/pit spatial patterns, and wired both branches into the 50-attempt dispatcher.
- **Requirements:** DG-ROOM-002; DG-NEST-001/002; DG-PIT-001/002; DG-ROOM-006; DG-RNG-001.
- **Implementation:** `RoomGeometryBuilder.TryBuildNest`, `RoomGeometryBuilder.TryBuildPit`, `RoomDispatchInputs`, and `RoomDispatcher` special-family integration.
- **Content boundary:** all nest/pit monsters remain deferred `RoomContentAttempt` requests with `AllowGroupExpansion = false`; no runtime placement occurs.
- **Status:** SATISFIED for current verified family-dispatch behavior.

### Slice 5A — Room-center connectivity and tunnel-carving foundation

- **Outcome:** added explicit granite initialization, source-compatible center shuffling and cyclic connection ordering, bounded tunnel direction/correction/random-turn behavior, ordinary rock carving, existing-floor junction candidates, outer-wall piercing with adjacent protection, and permanent-wall rejection.
- **Requirements:** DG-CONN-001; DG-TUN-001; DG-TUN-002; DG-RNG-001; DG-ROOM-001/002/006.
- **Implementation:** `DungeonGrid.InitializeRock`, `RoomConnectivityBuilder`, `DungeonTunnelBuilder`, `TunnelBuildResult`, and `DungeonCellStates.TunnelSolid`. Source queue capacities are 1,800 tunnel cells, 1,000 wall piercings, and 400 shared junction candidates.
- **Content boundary:** tunnel piercing and junction positions are returned as ordered deferred data. Door rolls and door execution remain deferred; room-local `RoomContentAttempt` values are untouched.
- **Status:** SATISFIED for the bounded connectivity/tunnel foundation. Whole-generation replay and later door execution remain deferred.

### Slice 5B — Tunnel entrance/junction door placement

- **Outcome:** added separate entrance and junction door passes in source order, verified 25%/90% rolls, source neighbor eligibility/order, the 1,000-point random-door distribution, stable door feature writes, and exact source queue capacities. Corrected 5A’s tunnel bounds to honor source `in_bounds()` strict-interior semantics.
- **Requirements:** DG-DOOR-001; tunnel-door portion of DG-DOOR-002; DG-TUN-002; DG-RNG-001 traceability.
- **Implementation:** `DungeonTunnelDoorBuilder`, `DungeonDoorGenerator.PlaceRandomDoor`, and the `RoomConnectivityBuilder` tunnel composition boundary.
- **Upstream correction:** narrow source inspection found `TUNN_MAX=1800`, `WALL_MAX=1000`, and `DOOR_MAX=400` caps were not enforced by the 5A lists, and 5A used full array bounds where source tunnel bounds exclude the outermost row/column. The bounded tunnel builder/connectivity aggregation now preserve those limits and strict-interior behavior; guard/boundary regressions pass.
- **Content boundary:** room-local deferred door requests remain untouched. Tunnel candidates are not merged with `RoomContentAttempt`.
- **Status:** DG-DOOR-001 SATISFIED for random tunnel door selection; DG-TUN-002 SATISFIED including tunnel door execution; the remaining room/vault-local portion of DG-DOOR-002 was closed by Slice 5C.

### Slice 5C — Room/vault-local deferred door execution

- **Outcome:** added an ordered executor for `SecretDoor` and `LockedDoor`; all other room-content attempt kinds remain in their original relative order as deferred requests.
- **Requirements:** DG-DOOR-002; DG-RNG-001; DG-ROOM-005/006/009/010; DG-VAULT-001/002.
- **Implementation:** `RoomDoorAttemptExecutor` and `RoomDoorExecutionResult`. Secret doors write the stable secret feature without RNG. Locked requests use `DungeonDoorGenerator`; the type-4 variant-2 producer carries its source-position `DoorState` so power RNG is consumed in the original order and is not rerolled during execution.
- **Producer correction:** removed the immediate locked-door grid write from type-4 variant 2. The builder records the prepared locked state; the executor performs the deferred feature/state write.
- **Content boundary:** only door attempts execute. Monster, trap, object, object/gold, special-object, and random-stair attempts remain deferred.
- **Status:** DG-DOOR-002 SATISFIED for all current verified room/vault door request producers. Tunnel door processing remains distinct.

### Slice 5D — Stair placement foundation and deferred RandomStair execution

- **Outcome:** added ordinary global stair count/allocation behavior and an ordered room stair executor. The type-4 variant-2 producer prepares the source direction roll when `RandomStair` is created, before the subsequent trap-count roll.
- **Requirements:** DG-STAIR-001/002 represented scope; DG-RNG-001; DG-ROOM-005/006/009/010; DG-DOOR-001/002; DG-CONN-001; DG-TUN-001/002.
- **Implementation:** `DungeonStairAllocator`, `DungeonStairGenerator`, and `RoomStairAttemptExecutor`; the dispatcher carries an optional quest-level flag. Existing terrain IDs `up_staircase` and `down_staircase` are reused.
- **Global RNG:** `Next(3,5)` for down count, then each candidate uses `Next(0,66)` row followed by `Next(0,198)` column. After all down requests, `Next(1,3)` selects the up count. Each request tries 3,001 candidates at the current cardinal-wall requirement, then decrements the requirement and continues without a source failure limit.
- **Room RNG:** after the producer's `Next(0,100)` 80/20 object/stair choice, ordinary positive non-quest depths consume `Next(0,100)` in `place_random_stairs` order; `< 50` prepares down, otherwise up. Town prepares down, quest/bottom prepares up, without a direction roll. Prepared direction is not rerolled by the executor.
- **Cell behavior:** only represented `open_floor` cells are eligible. Stair placement changes the feature ID and preserves `Room`, `Icky`, `Glow`, and `TunnelSolid`; permanent/non-floor terrain is not overwritten.
- **Content boundary:** doors execute in their existing executor; stair attempts execute in original order; monster, trap, object, object/gold, and special-object attempts remain deferred and ordered.
- **Status:** DG-STAIR-001 PARTIAL because object/monster occupancy and source level-entry coordinates are not represented. DG-STAIR-002 PARTIAL/CONDITIONAL because town generation and lifecycle callers are absent; quest/bottom direction overrides are available when the caller supplies the level flag.
- **Narrow research:** `generate.c::cave_gen`, `alloc_stairs`, `place_random_stairs`, `next_to_walls`; `mdefines.h::cave_naked_bold` and `cave_clean_bold`; `common/defines.h::MAX_DEPTH`.

### Slice 6A — Prepared nest/pit monster execution

- **Outcome:** added exact-position execution for prepared nest/pit `Monster` requests. Requests with a prepared `DefinitionId` and `AllowGroupExpansion = false` resolve through the stable definition map and pass through `MonsterPlacementService` into `MonsterRuntimeState`; failures are recorded once and are not relocated or retried.
- **Producer classification:** nest (`TryBuildNest`) and pit (`TryBuildPit`) are the only current producers with both prepared definition IDs and explicit group suppression. Ordinary type-1 through type-4 room monster requests (`RoomGeometryBuilder.AddAttempts`) and vault glyph monster requests (`VaultRoomBuilder`) have no prepared `DefinitionId`, so they remain deferred. No other Monster producer is in scope.
- **Coordinate bridge:** `DungeonPosition.Row -> MonsterPosition.Y`; `DungeonPosition.Column -> MonsterPosition.X`, through `RoomMonsterAttemptExecutor.ToMonsterPosition`.
- **Placement legality:** the executor requires represented `open_floor`; `MonsterPlacementService` retains placement-space bounds/static illegality, occupied-position, unique-capacity, and actual-depth `ForceDepth` rejection. Source player/level unique checks and object occupancy are unavailable. Room flags are preserved, not used as a source placement predicate; positive-depth nest/pit placement does not reject `Icky`.
- **Group behavior:** the supported request contract requires `AllowGroupExpansion = false`. The executor calls neither FRIENDS expansion nor escort placement, regardless of definition flags.
- **RNG:** prepared identity and position consume zero allocation/OOD/selection RNG; the executor has no RNG parameter. Narrow source inspection found `place_monster_one` does consume runtime-state RNG for HP, speed variance, and energy (plus conditional force-sleep/mimic behavior). Those values are not represented by current `MonsterDefinition`/`MonsterRuntimeInstance` fields, so IronHell does not consume those source spawn-state draws yet; complete stream parity remains partial.
- **Upstream geometry correction:** source `place_double_wall` places inner walls one cell outside the requested nest/pit region. `WriteDoubleRectangle` had placed them on the requested perimeter, making 44 nest and pit targets wall terrain. The shared border offset now matches source and a regression verifies all prepared targets are room floor.
- **Stair follow-up:** `MonsterRuntimeState` now contains authoritative occupancy for successfully placed nest/pit monsters, providing a concrete future input for stair legality. `DungeonStairAllocator` was not changed and does not yet consult this occupancy; object occupancy and level-entry coordinates remain gaps.
- **Narrow research:** `generate.c::build_type5`, `build_type6`, `place_double_room`, `place_double_wall`, and `place_wall`; `monster2.c::place_monster_aux` and `place_monster_one`; `mdefines.h::cave_empty_bold`.
- **Status:** bounded exact prepared placement is implemented. DG-MON-004/005 remain partial; DG-RNG-001 remains partial.

## 3. Current Monster Generation Architecture

The current repository-confirmed pipeline is:

```text
MonsterDefinition
    -> MonsterAllocationEntry
    -> MonsterAllocationPreparedEntry
    -> MonsterAllocationEffectiveEntry
    -> OOD/effective-level processing
    -> MonsterAllocationSelector.SelectWithComparison
    -> selected MonsterDefinition
    -> MonsterLocationSearch / explicit position
    -> MonsterPlacementService
    -> MonsterRuntimeState
    -> optional MonsterGroupExpander
    -> optional MonsterEscortExpander
```

Nest preparation is a separate pre-spatial branch:

```text
MonsterDefinition catalog
    -> MonsterNestPreparer.SelectFamily
    -> isolated family predicate through MonsterAllocationTableBuilder.Prepare
    -> MonsterAllocationEligibility at Depth + 10
    -> MonsterAllocationSelector.SelectWithComparison, 64 times
    -> MonsterNestPreparationResult
```

The nest branch produces candidate definition IDs for the spatial builder. Those prepared IDs and exact positions now flow through `RoomMonsterAttemptExecutor` into runtime placement; placement failures do not trigger selection or relocation.

Pit preparation is a parallel pre-spatial branch:

```text
MonsterDefinition catalog
    -> MonsterPitPreparer.SelectFamily
    -> optional dragon mask selection
    -> isolated pit predicate through MonsterAllocationTableBuilder.Prepare
    -> MonsterAllocationEligibility at Depth + 10
    -> MonsterAllocationSelector.SelectWithComparison, 16 times
    -> native-level sort
    -> even-index tier extraction
    -> MonsterPitPreparationResult
```

The pit preparation branch produces candidate identities and tiers; the spatial builder's prepared identities and exact positions now flow through `RoomMonsterAttemptExecutor` into runtime placement.

Dungeon spatial foundation is now a separate authoritative Core branch:

```text
DungeonGrid
    -> fixed 198x66 cell addressing
    -> composable Room / Icky cell state
    -> atomic RoomBlockFootprint reservation on a 6x18 block grid
    -> ordered RoomCenters
    -> RoomDispatcher -> 50 source-order attempts
    -> type-1/type-2/type-3/type-4 builders
    -> Nest -> MonsterNestPreparer -> spatial nest builder
    -> Pit -> MonsterPitPreparer -> spatial pit builder
    -> VaultDefinition -> vault eligibility/selection -> VaultRoomBuilder
    -> vault terrain/Room/Icky state
    -> ordered RoomContentAttempt requests
    -> RoomConnectivityBuilder -> DungeonTunnelBuilder -> DungeonGrid tunnel terrain
    -> per-tunnel pierced-wall entrance door pass
    -> after all tunnels, ordered junction candidate neighbor pass
    -> RoomDoorAttemptExecutor executes room/vault SecretDoor and LockedDoor requests
    -> DungeonStairAllocator places ordinary global stairs
    -> RoomStairAttemptExecutor executes ordered RandomStair requests
    -> RoomMonsterAttemptExecutor executes prepared nest/pit Monster requests
    -> ordinary/vault Monster and all object/gold/artifact/trap attempts stay deferred
```

Room builders now expose a bounded deferred-content boundary:

```text
RoomGeometryBuilder
    -> DungeonGrid geometry/state
    + ordered RoomContentAttempt requests
    -> RoomDoorAttemptExecutor executes SecretDoor/LockedDoor
    -> RoomStairAttemptExecutor executes RandomStair
    -> RoomMonsterAttemptExecutor executes only prepared nest/pit Monster requests
    -> ordinary/vault Monster and trap/object/object-gold/special-object requests remain deferred
```

`RoomContentAttempt` preserves verified request kind, source position/context, order, stable `DefinitionId`, explicit group-expansion intent, and prepared door/stair decisions where source RNG occurs at request creation. Door, stair, and prepared-monster executors return handled attempts separately from remaining content. `MonsterRuntimeState` owns occupancy for successfully placed monsters; object occupancy and full source terrain semantics are not modeled.

Definitions and runtime instances remain separate:

- `MonsterDefinition` is static content, including `Id`, `Symbol`, `NativeLevel`, `Rarity`, and `SpawnPolicy`.
- `MonsterRuntimeInstance` is minimal runtime identity: instance ID, definition ID, and position.
- `MonsterRuntimeState` owns runtime instances, occupancy, deterministic IDs, and represented unique presence.
- `MonsterPlacementSpace` owns current finite bounds and explicit static illegal cells.
- `MonsterPlacementService` validates and atomically commits one explicit placement.
- `MonsterGroupExpander` and `MonsterEscortExpander` add effects after a successful leader/escort placement; they do not replace the single-monster placement primitive.

No complete terrain feature system, player state, quest state, persistence, networking, or Godot dependency is present in this generation path.

## 4. Deterministic RNG Contract and Current Consumption

REPO-CONFIRMED:

- `IRandomSource.Next(minInclusive, maxExclusive)` uses an exclusive upper bound.
- `SeededRandomSource` is a Core-owned deterministic source.
- scripted test sources establish exact values and requested ranges.
- one RNG stream is passed through composed operations; new seeded sources are not created per request.

Important current consumers:

- OOD: two `Next(0,50)` calls for positive allocation levels;
- weighted allocation: cumulative `Next(0,totalWeight)`;
- candidate comparison: one shared `Next(0,100)`;
- ordinary request count: one `Next(1,9)`;
- proposed ordinary coordinate search: `Next(0,Width)` and `Next(0,Height)` per attempt;
- FRIENDS size and depth adjustment: `Next(1,14)` plus verified signed adjustment roll;
- escort scatter and escort allocation/effective selection.
- nest family selection: `Next(1, depth + 1)`; each of 64 samples then uses the existing `Depth + 10` OOD/effective pipeline and weighted comparison selection.
- pit family selection: `Next(1, depth + 1)`; dragon family adds `Next(0,6)` for the mask; each of 16 samples uses the existing `Depth + 10` OOD/effective pipeline and weighted comparison selection.
- type-1 rooms: lighting `Next(1,26)`, extents `Next(1,5)`, `Next(1,4)`, `Next(1,12)`, `Next(1,12)`, then mutually exclusive `Next(0,20)` pillar or `Next(0,50)` ragged branch.
- type-2 rooms: one shared lighting `Next(1,26)`, followed by the verified eight asymmetric rectangle-extent rolls.
- type-3 rooms: lighting `Next(1,26)`, vertical half-height `Next(3,5)`, horizontal half-width `Next(3,12)`, variant `Next(0,4)`, then variant-specific door/count/nested rolls in source order.
- type-4 rooms: fixed outer offsets, lighting `Next(1,26)`, variant `Next(1,6)`, then variant-specific door/count/nested rolls in source order. The object/stair branch uses `Next(0,100)` with `< 80` selecting special object and the remainder selecting random stair.
- room `RandomStair`: the selected branch prepares depth/quest direction immediately; ordinary positive non-quest depths consume `Next(0,100)` (`< 50` down, otherwise up) before the subsequent trap-count roll. The attempt stores the prepared feature and execution does not reroll.
- global stairs: `Next(3,5)` down count, then per candidate `Next(0,66)` row and `Next(0,198)` column, then `Next(1,3)` up count and its candidates. Each stair relaxes the cardinal-wall requirement after 3,001 rejected candidates; the source loop has no terminal retry limit.
- prepared nest/pit monster execution: zero allocation/OOD/selection RNG; `RoomMonsterAttemptExecutor` accepts no random source. Source `place_monster_one` separately rolls runtime HP (`damroll` unless force-max-HP), possible speed variance, and starting energy, plus conditional force-sleep/mimic state. These runtime fields/rolls are not represented and are an explicit RNG parity gap.
- deferred content attempts retain source helper origins/radii and counts without executing downstream helper RNG or placement.
- room dispatch: each attempt consumes `Next(0,6)` for row, `Next(0,18)` for column, then `Next(0,200)` for unusual; unusual attempts consume `Next(0,100)` for the family roll and a conditional second `Next(0,200)` for very-unusual selection. Builder RNG remains delegated after footprint preflight.
- connectivity/tunnels: each center-shuffle iteration consumes two `Next(0, centerCount)` calls; each tunnel consumes `Next(0,100)` for direction change, conditional `Next(0,100)` for diagonal correction and `Next(0,100)`/`Next(0,4)` for random direction, plus `Next(0,100)` for source continuation at existing-floor intersections.
- tunnel doors: each recorded piercing consumes `Next(0,100)`; a success (`< 25`) then consumes `Next(0,1000)` for door type and a conditional power roll (`Next(1,8)` locked, `Next(0,8)` stuck). After all tunnels, eligible junction-neighbor cells consume `Next(0,100)` before doorway-shape checking; a success (`< 90`) then uses the same random-door ranges.
- room/vault doors: `SecretDoor` writes `secret_door` with no RNG. Type-4 variant-2 `LockedDoor` consumes `Next(1,8)` when the source helper occurs, stores that immutable `DoorState` on the deferred attempt, and is later written without reroll. A manually constructed locked attempt without prepared state consumes `Next(1,8)` when executed.

Failed randomized attempts remain consumed. Zero-attempt and zero-total paths consume no unnecessary RNG. Call order remains parity-significant.

## 5. DG Requirement Implementation Matrix

| Requirement | Status | Current Implementation | Remaining Work / Caveat |
|---|---|---|---|
| DG-RNG-001 | PARTIAL | Deterministic injected RNG; prepared nest/pit execution uses no allocation/OOD/selection draws | Source `place_monster_one` runtime-stat draws are not represented; full replay awaits a complete generator and golden fixtures |
| DG-MON-001 | SATISFIED | Immutable allocation entries, integer rarity weights, level ordering | No known remaining allocation-table gap |
| DG-MON-002 | SATISFIED | Hook filtering, effective weights, weighted selection, comparison | Placement remains a separate boundary |
| DG-MON-003 | SATISFIED | Two independent OOD transformations | No whole-dungeon consumer yet |
| DG-MON-004 | PARTIAL | Runtime occupancy, bounds/static legality, grid open-floor gate for prepared nest/pit positions, unique capacity, actual-depth `ForceDepth` rejection | Player/level unique eligibility, object occupancy, and full source terrain/player legality are absent |
| DG-MON-005 | PARTIAL | Formula, bounded requests, failure-aware counts, current location infrastructure | Exact ordinary `alloc_monster` spatial search/call order remains proposed |
| DG-GROUP-001 | SATISFIED | Exact size adjustment, 32 cap, BFS, adjacency order, suppression | Group expansion remains separate from dungeon room/tunnel placement |
| DG-GROUP-002 | PARTIAL | Symbol predicate, weighted escort selection, 50 attempts, scatter, Escort/Escorts, escort FRIENDS | Full LOS/terrain legality unavailable |
| DG-NEST-001/002 | SATISFIED | `MonsterNestPreparer` plus spatial builder select the verified family, prepare 64 candidates with replacement, and execute the fixed-position requests without group expansion | Source runtime-stat RNG and player/object placement state are not represented |
| DG-PIT-001/002 | PARTIAL | `MonsterPitPreparer` plus spatial builder select family/mask, prepare 16 candidates, sort/extract eight tiers, and execute fixed-position requests without group expansion | Exact source `flags4` equivalence remains represented through `breath_*`; source runtime-stat RNG and player/object placement state are not represented |
| DG-ROOM-001 | SATISFIED | `DungeonGrid` exposes verified 198x66 dimensions, explicit `InitializeRock`, 11x11 blocks, 6x18 reservation, bounds, overlap checks, and atomic footprints | Full cave initialization remains out of scope |
| DG-ROOM-006 | SATISFIED | `DungeonCellStates.Room/Icky/Glow`, stable floor/wall feature IDs, ordered centers, ordinary/nest/pit/vault commits, and one-center semantics exist | Full lifecycle remains |
| DG-ROOM-002 | SATISFIED | `RoomDispatcher` processes exactly 50 attempts with verified coordinate/unusual branch order and dispatches all eight room families | Non-door content execution and whole-dungeon lifecycle remain separate |
| DG-ROOM-003 | SATISFIED | `RoomFamilies` exposes verified minimum depths and rectangular footprints for all listed families and dispatcher preflights them | Metadata does not imply every builder exists |
| DG-ROOM-004 | SATISFIED | Type-1/simple and type-2/overlapping base room geometry are executable | No remaining base-geometry gap for families 1-4 |
| DG-ROOM-005 | SATISFIED | Type-3 cross base geometry uses verified extents and crossing-arm construction | Deferred attempts do not execute downstream placement |
| DG-ROOM-009 | SATISFIED | All four type-3 variants, nested branches, geometry, and ordered requests are implemented | Non-door content execution remains deferred |
| DG-ROOM-010 | SATISFIED | All five type-4 variants, inner geometry, nested branches, and ordered requests are implemented | Non-door content execution remains deferred |
| DG-ROOM-007 | SATISFIED | Type-1 asymmetric extents, lighting, pillar/ragged ordering, and feature writes are implemented | Non-door content execution remains deferred |
| DG-ROOM-008 | SATISFIED | Type-2 asymmetric rectangles, one reservation/center, overlap, and shared lighting are implemented | Non-door content execution remains deferred |
| DG-STAIR-001 | PARTIAL | Ordinary global counts, candidate order, wall relaxation, stair terrain, and room-stair execution; nest/pit monsters now create authoritative runtime occupancy | `DungeonStairAllocator` does not yet consult monster occupancy; object occupancy and source level-entry coordinates remain unrepresented |
| DG-STAIR-002 | PARTIAL / CONDITIONAL | Quest and bottom direction overrides are implemented when level context is supplied; town RandomStair direction is down | Town generation, town terrain, and quest/static-level lifecycle are absent |
| DG-VAULT-001/002 | SATISFIED / CONTENT-PARTIAL | `VaultRoomBuilder` selects eligible definitions, normalizes type 9, maps layouts, writes stable terrain/state, and records ordered deferred attempts | Current catalog contains 10 migrated records versus 149 source records; missing source content and downstream execution remain |
| DG-CONN-001 | SATISFIED | `RoomConnectivityBuilder` copies, shuffles, and cyclically connects authoritative room centers without mutating `DungeonGrid.RoomCenters` | Whole-dungeon golden replay remains deferred |
| DG-TUN-001 | SATISFIED | `DungeonTunnelBuilder` implements verified direction correction, random turns, rock carving, existing-floor handling, strict-interior bounds, source buffer capacities, and the 2,000-step guard | Whole-generation golden replay remains deferred |
| DG-TUN-002 | SATISFIED | Ordered wall piercings, adjacent outer-wall protection, permanent-wall rejection, 25% entrance doors, and 90% eligible junction-door processing are implemented | No additional tunnel-door gap is known |
| DG-DOOR-001 | SATISFIED | `DungeonDoorGenerator.PlaceRandomDoor` maps the verified 0..999 source distribution to stable open/broken/secret/closed/locked/stuck door state | No additional tunnel-door gap is known |
| DG-DOOR-002 | SATISFIED | Tunnel doors plus ordered room/vault `SecretDoor` and `LockedDoor` request execution are implemented | Other room-content kinds remain deferred under their own requirements |
| DG-DROP-* | NOT IMPLEMENTED | Loot definitions exist, no death-drop runtime | Requires monster lifecycle |
| DG-OBJ-* | NOT IMPLEMENTED | Item definitions exist, no dungeon object generation | Requires object allocation/runtime |
| DG-TRAP-* | NOT IMPLEMENTED | Trap definitions exist, no dungeon trap allocation | Requires grid/features |
| DG-RATING-* | NOT IMPLEMENTED | No dungeon rating/feeling runtime | Requires generation lifecycle |
| DG-FEEL-* | NOT IMPLEMENTED | No feeling calculation runtime | Requires rating/lifecycle |
| DG-DEST-* | NOT IMPLEMENTED | No destroyed-level rewriting | Requires grid/generation |
| DG-TOWN-* | CONDITIONAL | No town generator/runtime | Configuration-dependent behavior remains deferred |
| DG-QUEST-* | CONDITIONAL | No quest/static-level runtime | Player/configuration/save-state prerequisites absent |
| DG-WILD-* | NOT IMPLEMENTED | No wilderness generator/population runtime | Requires wilderness state/terrain |
| DG-LIFE-* | PARTIAL | Monster request/placement fragment exists | Complete lifecycle, retries, rooms, objects, traps, and rating are absent |

## 6. Current Dungeon/Monster Generation Implementation Inventory

### Allocation

- `src/IronHell.Core/Definitions/CatalogDefinitions.cs` — `MonsterDefinition`, `SpawnPolicy`, and static monster fields.
- `src/IronHell.Core/Monsters/MonsterAllocationTableBuilder.cs` — base and prepared allocation entries.
- `src/IronHell.Core/Monsters/MonsterAllocationLevelResolver.cs` — two-roll OOD transformation.
- `src/IronHell.Core/Monsters/MonsterAllocationEligibility.cs` — effective-level/weight preparation.
- `src/IronHell.Core/Monsters/MonsterAllocationSelector.cs` — cumulative weighted selection and candidate comparison.
- `src/IronHell.Core/Monsters/MonsterEscortEligibility.cs` — isolated escort candidate predicate.
- `src/IronHell.Core/Monsters/MonsterNestPreparer.cs` — verified nest family selection, isolated predicates, and 64-candidate preparation.
- `src/IronHell.Core/Monsters/MonsterPitPreparer.cs` — verified pit family/mask selection, isolated predicates, 16-candidate sampling, sorting, and tier extraction.

### Placement/runtime

- `src/IronHell.Core/Monsters/MonsterPosition.cs` — minimal coordinate value.
- `src/IronHell.Core/Monsters/MonsterRuntimeState.cs` — runtime instances, occupancy, unique presence, deterministic IDs.
- `src/IronHell.Core/Monsters/MonsterPlacementEligibility.cs` — lower-level unique eligibility result.
- `src/IronHell.Core/Monsters/MonsterPlacementSpace.cs` — finite bounds/static illegal cells.
- `src/IronHell.Core/Monsters/MonsterLocationSearch.cs` — bounded proposed coordinate search.
- `src/IronHell.Core/Monsters/MonsterPlacementService.cs` — atomic one-monster placement.

### Dungeon grid/reservation

- `src/IronHell.Core/Dungeon/DungeonGrid.cs` — authoritative 198x66 cell grid, composable room/icky/glow cell state, 6x18 room-block reservation, feature IDs, and ordered room centers.
- `src/IronHell.Core/Dungeon/RoomFamilyMetadata.cs` — verified family minimum-depth and rectangular block-footprint metadata.
- `src/IronHell.Core/Dungeon/RoomContentAttempt.cs` — immutable ordered content request representation with prepared door/stair decisions.
- `src/IronHell.Core/Dungeon/RoomGeometryBuilder.cs` — ordinary geometry plus verified nest/pit shells and deferred spatial monster requests.
- `src/IronHell.Core/Dungeon/VaultDefinition.cs` — immutable stable-ID vault definition and type normalization.
- `src/IronHell.Core/Dungeon/VaultRoomBuilder.cs` — vault eligibility, selection, coordinate mapping, terrain/state writes, and deferred glyph attempts.
- `src/IronHell.Core/Dungeon/RoomDispatcher.cs` — 50-attempt source-order room selection and all-eight-family orchestration.
- `src/IronHell.Core/Dungeon/RoomConnectivityBuilder.cs` — working-center shuffle, cyclic pair ordering, and tunnel aggregation.
- `src/IronHell.Core/Dungeon/DungeonTunnelBuilder.cs` — bounded source-style tunnel progression, carving, wall piercing, and deferred candidates.
- `src/IronHell.Core/Dungeon/DungeonTunnelDoorBuilder.cs` — ordered 25% entrance-door and 90% eligible junction-door passes.
- `src/IronHell.Core/Dungeon/DungeonDoorGenerator.cs` — verified random door feature/state distribution plus existing room-specific door helpers.
- `src/IronHell.Core/Dungeon/RoomDoorAttemptExecutor.cs` — ordered room/vault secret/locked request execution with non-door attempts retained.
- `src/IronHell.Core/Dungeon/RoomMonsterAttemptExecutor.cs` — exact-position execution of prepared, group-suppressed nest/pit requests; all unsupported attempts remain ordered.
- `src/IronHell.Core/Dungeon/DungeonStairAllocator.cs` — ordinary global stair counts, candidate order, and wall-requirement relaxation.
- `src/IronHell.Core/Dungeon/DungeonStairGenerator.cs` — verified depth/quest direction selection and floor-only stair feature writes.
- `src/IronHell.Core/Dungeon/RoomStairAttemptExecutor.cs` — ordered RandomStair execution with remaining room content retained.
- `src/IronHell.Data/Loading/DefinitionDocumentLoader.cs` — loads and evaluates definition schemas with a per-load registry.
- `src/IronHell.Data/Visuals/TerrainVisualCatalogLoader.cs` — evaluates terrain visuals with a per-load registry.
- `src/IronHell.Data/Serialization/VaultDefinitionReader.cs` — maps `environment/vaults.json` into validated Core definitions.

### Population and formations

- `src/IronHell.Core/Monsters/OrdinaryMonsterRequestCount.cs` — DG-MON-005 count formula.
- `src/IronHell.Core/Monsters/OrdinaryMonsterPopulation.cs` — fixed request loop and accounting.
- `src/IronHell.Core/Monsters/OrdinaryMonsterPopulationOptions.cs` — location budget/group toggle.
- `src/IronHell.Core/Monsters/MonsterGroupSizeCalculator.cs` — verified FRIENDS size rules.
- `src/IronHell.Core/Monsters/MonsterGroupExpander.cs` — BFS FRIENDS expansion.
- `src/IronHell.Core/Monsters/MonsterEscortExpansionOptions.cs` — escort operation inputs.
- `src/IronHell.Core/Monsters/MonsterEscortExpander.cs` — escort filtering/scatter/placement.

### Definition/Data mapping

- `src/IronHell.Data/Serialization/MonsterDefinitionReader.cs` — maps existing monster JSON fields including `stats.level`, `stats.rarity`, `symbol`, `categories`, and `abilities`.
- `src/IronHell.Data/Validation/MonsterValidator.cs` — validates current monster definition structure and references.

### Tests

- `src/IronHell.Core.Tests/Monsters/MonsterAllocationTableTests.cs` — allocation construction, preparation, selection, and comparison.
- `src/IronHell.Core.Tests/Monsters/MonsterAllocationEligibilityTests.cs` — OOD/effective eligibility.
- `src/IronHell.Core.Tests/Monsters/MonsterPlacementEligibilityTests.cs` — post-allocation unique boundary.
- `src/IronHell.Core.Tests/Monsters/MonsterPlacementTests.cs` — runtime placement/occupancy/unique state.
- `src/IronHell.Core.Tests/Monsters/MonsterPlacementSpaceTests.cs` — bounds/static legality/location search.
- `src/IronHell.Core.Tests/Monsters/OrdinaryMonsterPopulationTests.cs` — DG-MON-005 accounting and replay.
- `src/IronHell.Core.Tests/Monsters/MonsterGroupExpansionTests.cs` — FRIENDS rules/BFS/cap/suppression.
- `src/IronHell.Core.Tests/Monsters/MonsterEscortTests.cs` — escort predicate/allocation/placement.
- `src/IronHell.Core.Tests/Monsters/MonsterNestPreparationTests.cs` — nest family thresholds, predicates, 64-sample replacement, abort, isolation, and replay.
- `src/IronHell.Core.Tests/Monsters/MonsterPitPreparationTests.cs` — pit family/mask thresholds, predicates, 16-sample replacement, sorting, tiers, abort, and isolation.
- `src/IronHell.Data.Tests/MonsterDefinitionReaderTests.cs` — monster field mapping, including symbol.
- `src/IronHell.Data.Tests/DefinitionCatalogLoaderTests.cs` — concurrent definition/terrain schema loading regression.
- `src/IronHell.Core.Tests/Dungeon/DungeonGridTests.cs` — dimensions, 11x11 block mapping, atomic reservations, flags, and room-center order.
- `src/IronHell.Core.Tests/Dungeon/RoomGeometryBuilderTests.cs` — family metadata, ordinary geometry, RNG ordering, variants, deferred attempts, replay, lighting, overlap, and failure behavior.
- `src/IronHell.Core.Tests/Dungeon/VaultRoomBuilderTests.cs` — vault eligibility, type-9 normalization, coordinates, footprints, atomic failure, flags, glyph ordering, depth offsets, and replay.
- `src/IronHell.Core.Tests/Dungeon/RoomDispatcherTests.cs` — 50-attempt count, coordinate RNG, conditional unusual rolls, fallback, failure accounting, and center ordering.
- `src/IronHell.Core.Tests/Dungeon/RoomSpecialBuilderTests.cs` — nest/pit preparation dispatch, fixed spatial request patterns, IDs, group suppression, vault reachability, and failure fallback.
- `src/IronHell.Core.Tests/Dungeon/ConnectivityAndTunnelTests.cs` — rock prerequisite, center ordering, cyclic connections, carving, piercing, junction candidates, permanent terrain, and replay.
- `src/IronHell.Core.Tests/Dungeon/DungeonTunnelDoorBuilderTests.cs` — entrance/junction ordering and thresholds, doorway eligibility, duplicate semantics, feature distribution, flags, replay, and composed connectivity integration.
- `src/IronHell.Core.Tests/Dungeon/RoomDoorAttemptExecutorTests.cs` — secret/locked behavior, mixed ordering/filtering, same-cell attempts, vault flags, large-room deferred integration, no-door behavior, and replay.
- `src/IronHell.Core.Tests/Dungeon/RoomMonsterAttemptExecutorTests.cs` — prepared nest/pit execution, coordinates, order, failures, terrain, ForceDepth, suppression, deferred producers, and replay.
- `src/IronHell.Core.Tests/Dungeon/DungeonStairTests.cs` — stair direction thresholds, source-time preparation, global counts/candidates/retries, legality, flags, mixed attempts, and replay.

## 7. Verified Parity vs IronHell Infrastructure

### `MonsterPlacementSpace`

IRONHELL-PROPOSED INFRASTRUCTURE: finite bounds plus explicit illegal cells. It exists because the full MAngband feature grid is not implemented. `RoomMonsterAttemptExecutor` composes it with the current dungeon-grid `open_floor` check for prepared nest/pit positions; it is not the final terrain or occupancy representation for all dungeon content.

### `DungeonGrid`

REPO-CONFIRMED AUTHORITATIVE FOUNDATION: fixed 198x66 row/column cell addressing, composable `Room`/`Icky` cell state, a 6x18 room-block reservation grid using 11x11 blocks, atomic rectangular reservation, and ordered room centers. This is the dungeon-generation grid foundation, not a complete terrain feature map.

### Room feature writes

REPO-CONFIRMED LIMITED TERRAIN FOUNDATION: type-1/type-2 builders write the existing stable terrain IDs `open_floor`, `granite_wall_outer`, and `granite_wall_inner` into `DungeonGrid`. `DungeonCellStates.Room`, `Icky`, and `Glow` are structural generation state. This is not a complete feature catalog or dungeon terrain system.

### `RoomContentAttempt`

IRONHELL-PROPOSED INFRASTRUCTURE: this immutable ordered request record preserves MAngband-verified room-local effects. Door and RandomStair execution consume their request kinds; prepared nest/pit Monster requests also execute. Ordinary/vault monsters, objects, gold, artifacts, and traps remain deferred. It records requests, origins, radii, depth context, special intent, and prepared source-time decisions where verified; unsupported requests do not create runtime entities or claim placement success.

### `MonsterLocationSearch`

IRONHELL-PROPOSED INFRASTRUCTURE: current ordinary search uses one X and one Y draw per attempt:

```text
Next(0, Width)
Next(0, Height)
```

Failed attempts consume both draws. Exact ordinary MAngband `alloc_monster` search and call order remain unresolved in implementation.

### Ordinary population order

IRONHELL-PROPOSED INFRASTRUCTURE: current composition is:

```text
find location
    -> allocate/select race
    -> place leader
    -> FRIENDS
    -> escorts
```

The parity specification does not establish this exact ordinary call order sufficiently; it must not be described as MAngband-verified.

### Escort spatial legality

IRONHELL-PROPOSED INFRASTRUCTURE: escort scatter operates against finite bounds, static illegal cells, and occupancy. Full source LOS/terrain behavior cannot yet be represented.

### Nest preparation result

IRONHELL-PROPOSED API shape: `MonsterNestPreparationResult` carries the selected family and ordered candidate definition IDs. The family thresholds, predicates, sample count, `Depth + 10` allocation level, replacement semantics, and abort behavior are MANGBAND-VERIFIED; the result record and spatial request boundary are IronHell infrastructure. The dispatcher consumes the result without reimplementing preparation.

### Pit preparation result and dragon mask

IRONHELL-PROPOSED API shape: `MonsterPitPreparationResult` carries family selection, optional dragon mask, sampled IDs, sorted IDs, and eight retained tier IDs. The family thresholds, 16 samples, native-level sorting, and even-index extraction are MANGBAND-VERIFIED. Current Core derives exact breath masks from existing `breath_*` ability IDs; this is the available data representation, not a direct source `flags4` model.

### Vault definitions and interpretation

REPO-CONFIRMED DATA MAPPING: `environment/vaults.json` is loaded by the C# definition manifest and reader into immutable `VaultDefinition` records. The current records are classified from stable `room_type:7` and `room_type:8` tags; no type-9 record is present to normalize at load time.

MANGBAND-VERIFIED BEHAVIOR: source selection retries random indexes until the requested runtime type matches; type 9 is normalized to runtime type 8 by the source parser. `build_vault()` uses a row-major direct-write pass followed by a row-major monster/object pass. Non-padding cells receive floor plus `Room|Icky`; vault lighting is not randomly rolled. `%`, `#`, and `X` write outer, inner, and permanent-inner wall features. `*`, `+`, and `^` request object/trap or secret-door effects during the first pass; `&`, `@`, `9`, `8`, and `,` emit deferred effects during the second pass, preserving sequential same-cell attempts.

IRONHELL-PROPOSED BOUNDARY: `VaultRoomBuilder` and the `RoomContentAttempt` representation are Core infrastructure for those verified effects. They do not execute monsters, objects, traps, doors, rating, or feelings.

## 8. Narrow Research Exceptions Since Freeze

### FRIENDS

- `ref-mangband/src/server/monster2.c :: place_monster_group()`
- `ref-mangband/src/server/tables.c :: ddx_ddd[] / ddy_ddd[]`

Verified exact group-size adjustment, positive-extra cap, GROUP_MAX, adjacency order, and BFS behavior. This was a narrow implementation-blocking check, not broad archaeology.

### Escorts

- `ref-mangband/src/server/monster2.c :: place_monster_aux()`
- `ref-mangband/src/server/monster2.c :: place_monster_okay()`
- `ref-mangband/src/server/monster2.c :: place_monster_group()`
- `ref-mangband/src/server/cave.c :: scatter()`
- `ref-mangband/src/common/z-rand.h :: rand_spread`

Verified 50 attempts, candidate predicate/order, Escort/Escorts behavior, radius-3 scatter, and escort FRIENDS interaction. This was a narrow implementation-blocking check, not broad archaeology.

### Nests

- `ref-mangband/src/server/generate.c :: build_type5()`
- `ref-mangband/src/server/generate.c :: vault_aux_jelly()`
- `ref-mangband/src/server/generate.c :: vault_aux_animal()`
- `ref-mangband/src/server/generate.c :: vault_aux_undead()`

Verified family thresholds, jelly symbols, animal/undead predicates, `Depth + 10` candidate allocation level, 64 samples with replacement, and abort-on-zero behavior. This was a narrow implementation-blocking check, not broad archaeology.

The same narrow check verified the type-5 double-room shell, one of four secret-door positions, the fixed `5x19` interior traversal, 95 candidate picks with replacement, and `grp = FALSE` placement semantics. IronHell records those picks as deferred requests rather than runtime placements.

### Cross and large rooms

- `ref-mangband/src/server/generate.c :: build_type3()`
- `ref-mangband/src/server/generate.c :: build_type4()`
- `ref-mangband/src/server/generate.c :: place_cross_room()`
- `ref-mangband/src/server/generate.c :: place_double_room()`
- `ref-mangband/src/server/generate.c :: vault_monsters()`
- `ref-mangband/src/server/generate.c :: vault_traps()`
- `ref-mangband/src/server/generate.c :: vault_objects()`

Verified type-3/type-4 geometry, nested branch order, door positions, content-request counts, and the fixed source helper origins/radii needed for deferred attempts. Contradiction with the frozen specification: NO. Broader research: NO.

### Vaults

- `ref-mangband/src/server/generate.c :: build_type7()`
- `ref-mangband/src/server/generate.c :: build_type8()`
- `ref-mangband/src/server/generate.c :: build_vault()`
- `ref-mangband/src/server/init1.c :: parse_v_info()`

Verified lesser/greater selection, type-9 normalization, center-relative row/column mapping, two-pass traversal, glyph behavior, Room/Icky state, and same-cell ordering. Contradiction with the frozen specification: NO. Broader research: NO.

### Pits

- `ref-mangband/src/server/generate.c :: build_type6()`
- `ref-mangband/src/server/generate.c :: vault_aux_orc()`
- `ref-mangband/src/server/generate.c :: vault_aux_troll()`
- `ref-mangband/src/server/generate.c :: vault_aux_giant()`
- `ref-mangband/src/server/generate.c :: vault_aux_dragon()`
- `ref-mangband/src/server/generate.c :: vault_aux_demon()`

Verified pit thresholds, six-way dragon mask selection, exact predicate inputs, `Depth + 10` allocation, 16 samples, abort-on-failure, bubble sorting by native level, and even-index tier extraction. This was a narrow implementation-blocking check, not broad archaeology.

The same narrow check verified the type-6 double-room shell, one of four secret-door positions, and the fixed 95-position tier pattern: outer rows use tier 0, the three middle rows use tiers 0 through 4 symmetrically, then tiers 5, 6, and 7 occupy the center approaches. IronHell records every request with `AllowGroupExpansion = false`.

### Connectivity and tunnels

- `ref-mangband/src/server/generate.c :: cave_gen()`
- `ref-mangband/src/server/generate.c :: build_tunnel()`
- `ref-mangband/src/server/generate.c :: correct_dir()`
- `ref-mangband/src/server/generate.c :: rand_dir()`
- `ref-mangband/src/server/generate.c :: place_random_door()`
- `ref-mangband/src/server/generate.c :: next_to_corr()`
- `ref-mangband/src/server/generate.c :: possible_doorway()`
- `ref-mangband/src/server/generate.c :: try_door()`
- `ref-mangband/src/server/generate.c :: cave_gen()` tunnel/door consumer loop
- `ref-mangband/src/server/mdefines.h :: in_bounds()`

Verified center shuffling with two random indexes per center, last-center cyclic connection setup, direction correction, 30% direction changes, 10% random cardinal turns, 15% extra-tunneling termination, the 2,000 iteration guard, granite carving, room-floor traversal, outer-wall piercing, adjacent outer-wall solidification, and ordered tunnel/wall/junction collection. Verified `in_bounds()` excludes the outermost row and column, unlike IronHell’s array bounds. Also verified 1,800/1,000/400 tunnel/wall/door array bounds, 25% per-piercing random-door pass, post-tunnel junction processing order and four-neighbor order, eligibility-before-roll behavior, 90% roll-before-shape-check behavior, and `place_random_door()`'s 1,000-point distribution. The 5A implementation was corrected narrowly for these source constraints. Contradiction with frozen requirements: NO. Broader research: NO.

### Room/vault door requests

- `ref-mangband/src/server/generate.c :: place_secret_door()`
- `ref-mangband/src/server/generate.c :: place_locked_door()`
- `ref-mangband/src/server/generate.c :: build_type4()` variant 2

Verified secret-door placement is a direct `FEAT_SECRET` write with no RNG; locked-door placement writes a locked door with `randint1(7)` power. In type-4 variant 2, the locked helper is called immediately after its position roll, before monster/object/trap generation. A current-repository discrepancy was found: the builder both recorded a deferred request and directly wrote the door, with the power roll too late. The duplicate write was removed; the source-position power roll is retained as immutable `PreparedDoorState` and applied later by the narrow executor. Frozen-contract contradiction: NO. Broader research: NO.

## 9. Known Specification Contradictions and Errata

The frozen parity specification contains a DG-MON-005 arithmetic contradiction.

Explicit formula:

```text
14 + randint1(8) + clamp(depth / 3, 2, 10)
```

At DL1 this yields `17..24`, but existing derived examples state `16..23`.

Current implementation follows the explicit verified formula using `Next(1,9)`. The frozen parity specification was not edited. This requires future specification-maintenance review.

No additional contradiction was introduced by this checkpoint.

## 10. Deferred Verified Behavior

### Player/level unique eligibility

DG-MON-004 remains partial because Core has no authoritative player membership, quest state, level ownership, or player-specific eligibility.

### Full monster terrain legality

`MonsterPlacementSpace` is intentionally minimal. It does not represent walls, floors, doors, room flags, vault flags, LOS, or terrain transitions.

### Ordinary `alloc_monster` location parity

The current coordinate search is proposed infrastructure. Exact source search/call-order parity remains unresolved.

### Escort LOS/terrain semantics

Current escort legality cannot reproduce source LOS and full terrain checks.

### Monster removal/death lifecycle

Runtime state is append-only for this foundation. No general death/removal operation releases occupancy or unique capacity.

### Pit dragon source-mask equivalence

Current `MonsterDefinition.Abilities` maps existing `breath_*` IDs, which is sufficient for the current exact-set tests and preparation path. A dedicated source `flags4` representation is absent, so byte-for-byte source flag equivalence remains a representation risk until the source flags are modeled directly.

### Room selection and remaining room families

DG-ROOM-002 is satisfied for the current verified family-dispatch boundary: `RoomDispatcher` processes all 50 attempts and dispatches simple, overlapping, cross, large, nest, pit, lesser-vault, and greater-vault builders. Non-door content execution remains deferred.

### Room-content execution

Room/vault `SecretDoor`, `LockedDoor`, and `RandomStair` requests execute through focused executors. Prepared nest/pit `Monster` requests execute at their exact positions in source order; failed positions do not relocate or retry. Ordinary room and vault `Monster` requests remain deferred, as do trap, object, object/gold, and special-object requests. Requested attempts remain distinct from successful placements.

### Remaining spatial and lifecycle work

Stair placement is implemented for the represented ordinary/global and room-request boundaries. Nest/pit placement now updates authoritative monster occupancy, but `DungeonStairAllocator` does not consume it; object occupancy and level-entry side effects remain unrepresented. Ordinary/vault monster allocation, object generation, traps, rating, and the complete dungeon lifecycle remain deferred. Tunnel entrance/junction and room/vault-local doors execute. Missing source vault content and downstream non-door vault-content execution are explicit parity gaps.

## 11. Test and Build Baseline

Fresh validation on 2026-10-05:

- Focused Slice 6A executor tests: **14 passed, 0 failed, 0 skipped**.
- Focused room geometry, nest/pit, prepared executor, placement, eligibility, group, and escort regressions: **71 passed, 0 failed, 0 skipped**.
- Focused Slice 5D stair tests: **15 passed, 0 failed, 0 skipped**.
- Focused Slice 6A plus nest/pit preparation, room/door/stair/tunnel/grid, monster placement/group/escort regressions: **185 passed, 0 failed, 0 skipped**.
- Full Core tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore` — **420 passed, 0 failed, 0 skipped**.
- Full Data tests: `dotnet test src/IronHell.Data.Tests/IronHell.Data.Tests.csproj --no-restore` — **155 passed, 0 failed, 0 skipped** in 159.5 seconds after schema registry isolation. Earlier attempts were stopped at 120 seconds without a final result; a prior Slice 5D run surfaced the `SchemaRegistry.CopyFrom` collection-modified race.
- Data concurrency fix: definition and terrain loaders now register schemas in their per-call `EvaluationOptions.SchemaRegistry`, never `SchemaRegistry.Global`; a concurrent cross-loader regression passes.
- Solution build: `dotnet build IronHell.sln --no-restore` — **PASS**.
- Full solution/workspace test suite: not run; no workspace-wide test result is claimed.

Historical Slice 5C validation on 2026-10-02:

- Focused vault tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~VaultRoomBuilderTests` — **9 passed, 0 failed, 0 skipped**.
- Focused dispatcher tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~RoomDispatcherTests` — **6 passed, 0 failed, 0 skipped**.
- Focused nest/pit spatial-dispatch tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~RoomSpecialBuilderTests` — **6 passed, 0 failed, 0 skipped**.
- Focused connectivity/tunnel tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~ConnectivityAndTunnelTests` — **12 passed, 0 failed, 0 skipped**.
- Focused tunnel-door tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~DungeonTunnelDoorBuilderTests` — **25 passed, 0 failed, 0 skipped**.
- Focused room-door executor tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~RoomDoorAttemptExecutorTests` — **9 passed, 0 failed, 0 skipped**.

- Focused monster-generation tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~Monsters` — **135 passed, 0 failed, 0 skipped**.
- Focused nest tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~MonsterNestPreparationTests` — **11 passed, 0 failed, 0 skipped**.
- Focused pit tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~MonsterPitPreparationTests` — **23 passed, 0 failed, 0 skipped**.
- Focused dungeon-grid tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~DungeonGridTests` — **13 passed, 0 failed, 0 skipped**.
- Focused room-geometry tests: `dotnet test src/IronHell.Core.Tests/IronHell.Core.Tests.csproj --no-restore --filter FullyQualifiedName~RoomGeometryBuilderTests` — **20 passed, 0 failed, 0 skipped**.
- Data project compilation: `dotnet build IronHell.sln --no-restore` compiled `IronHell.Data` and `IronHell.Data.Tests` successfully; no isolated vault-reader test was added.
The Data test issue is outside the 5D changes and matches the previously observed schema-registry concurrency failure:

- `IronHell.Data.Tests.SpellExecutionBootstrapTests.LoadAsync_RepositorySpells_PublishActionReferences`
- `Json.Schema.SchemaRegistry.RegisterSchema` / `DefinitionDocumentLoader.LoadSchemas`
- failure pattern: concurrent mutation of the schema registry dictionary and related schema-registration errors

No production or test changes were made to address the Data test failure/stall.

## 12. Major Dungeon Generation Areas Not Yet Implemented

The repository contains some static catalogs and validators, but executable runtime behavior remains absent for:

- remaining dungeon-generation subsystems, content execution, and lifecycle behavior;
- complete vault content migration and deferred-content execution;
- object allocation, quality, artifacts, and gold;
- monster drops;
- traps and trap activation;
- rating and feelings;
- destruction;
- complete dungeon lifecycle and retries;
- town generation;
- quest/static-level behavior;
- wilderness generation and population.

Static definitions for some of these domains exist, but definitions/catalogs are not equivalent to executable parity consumers.

## 13. Recommended Next Implementation Sequence

### Next: Slice 6B — Monster spawn-state RNG foundation

**Requirements:** DG-RNG-001; DG-MON-004.

**Bounded outcome:** model and consume the verified `place_monster_one` runtime initialization decisions required by `MonsterRuntimeInstance` before any further monster-placement execution expands. Resolve represented HP/speed/energy fields and keep ordinary allocation, room/vault content selection, groups, objects, and traps out of scope.

## 14. Golden-Seed Status

GS-001 through GS-011 remain planned. No arbitrary seeds, map snapshots, or expected hashes have been invented.

Current deterministic unit tests are not substitutes for verified whole-generation golden seeds. Golden scenarios require trustworthy reference outputs or a verified reference/implementation process. No complete golden-seed infrastructure exists yet.

## 15. Guardrails for Future Dungeon Work

- Keep `IronHell.Core` Godot-independent.
- Keep persistence, networking, and rendering outside generation logic.
- Keep authoritative runtime state in Core/server ownership.
- Keep definitions and runtime instances separate.
- Use stable IDs, never display names, as identity.
- Pass one injected RNG stream through composed behavior.
- Preserve failed-attempt RNG consumption.
- Keep requested count distinct from successful count.
- Keep allocation success distinct from placement success.
- Do not mutate base allocation weights.
- Preserve base/prepared/effective weight lifecycle.
- Keep group suppression explicit for nests/pits.
- Do not flatten FRIENDS, ESCORT, and ESCORTS.
- Preserve verified parity quirks instead of replacing them with cleaner behavior.
- Do not reopen broad MAngband research without a concrete implementation contradiction.
- Do not modify JSON/schema merely for convenience.
- Write deterministic tests before behavior changes.
- Avoid unrelated refactoring.

## 16. Technical Debt vs Parity Debt

### Parity debt

- exact ordinary location search and call order;
- player/level unique eligibility;
- full terrain/LOS legality;
- monster removal/death lifecycle;
- remaining dungeon-generation subsystems, content execution, and lifecycle behavior;
- golden-seed whole-generation evidence.

### Technical debt

No new technical debt was introduced by this checkpoint. The minimal placement space is intentional staged parity infrastructure, not a defect requiring a generic map framework.

## 17. Current Milestone

**Room, Connectivity, Door, Stair, and Prepared Nest/Pit Placement Foundations**

The repository can execute and test:

```text
definitions
    -> allocation table
    -> hook filtering
    -> OOD/effective eligibility
    -> weighted selection/comparison
    -> runtime placement
    -> ordinary request accounting
    -> FRIENDS
    -> escorts
    -> nest preparation and spatial nest builder
    -> pit preparation and spatial pit builder
    -> authoritative dungeon grid and room-block reservation
    -> 50-attempt source-order dispatcher
    -> type-1 simple, type-2 overlapping, type-3 cross, type-4 large
    -> lesser/greater vault selection and spatial interpretation
    -> ordered deferred room-content attempts
    -> cyclic room-center connectivity
    -> bounded tunnel carving, wall piercing, and deferred junction candidates
    -> generated tunnel entrance and junction doors
    -> RoomDoorAttemptExecutor executes room/vault secret and locked doors
    -> DungeonStairAllocator places ordinary global stairs
    -> RoomStairAttemptExecutor executes room RandomStair requests
    -> RoomMonsterAttemptExecutor places prepared nest/pit monsters
    -> deferred ordinary/vault monsters and object/gold/artifact/trap content
```

It is not yet correct to call this complete dungeon-generation parity. All eight current room families, the 50-attempt dispatcher, connectivity/tunnels, doors, represented stair paths, and prepared nest/pit monster placement now execute deterministically. Ordinary/vault monster generation, monster spawn-stat RNG, object/gold/artifact/trap content, drops, rating/feelings, lifecycle retries, town, quest/static levels, wilderness, missing source vault content, complete stair occupancy/entry state, and whole-generation golden evidence remain incomplete or conditional.
