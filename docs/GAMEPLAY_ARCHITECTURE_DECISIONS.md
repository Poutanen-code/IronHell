# Gameplay Architecture Decisions

> Date: 2026-08-03
> Scope: gameplay definition architecture under `data/definitions/` and validation contracts under `data/schemas/`
> Purpose: long-lived architectural decision record for gameplay data modeling
>
> This document is decision guidance only. It does not perform migrations or prescribe immediate file edits.

---

## Decision Framework

IronHell architectural decisions are evaluated in this order:

1. MAngband fidelity
2. Deterministic behaviour
3. Data validation
4. Maintainability
5. Simplicity

If a proposed cleanup improves elegance but risks parity or determinism, it is deferred.

---

## 1) Capability Model

### Finding C-01

Reference:
- Original review finding ID: F-01 (Two Capability Files with Overlapping IDs)

### Current State

- Two files define capability-like concepts:
  - `data/definitions/capabilities.json` (class/shared style capabilities, plus `grant_sources` metadata)
  - `data/definitions/items/item_capabilities.json` (item-bearer/item-self capabilities)
- There are many overlapping IDs across both files (for example `free_act`, `res_fire`, `see_invis`, sustains, resist sets).
- Item-only protections (`ignore_fire`, `ignore_acid`, etc.) are represented separately in item capability data.

### Decision

Deferred

### Recommendation

Keep the two capability systems separate in the short term. Define and enforce a strict conceptual boundary now:

- Character lineage/identity capabilities (race/class/native) remain in `capabilities.json`.
- Equipment transfer capabilities and item-self protections remain in `items/item_capabilities.json`.

Plan an eventual merge only after verified parity mappings and reference-backed scope rules exist.

### Benefits

- Preserves existing semantics during parity-sensitive stage.
- Avoids accidental merge errors where item-self protections get treated as bearer buffs.
- Reduces short-term migration risk while still creating a clear architecture target.

### Risks

- Continued overlap can keep tooling and validation more complex.
- Duplicated IDs can drift if governance is weak.

### Dependencies

- Written capability taxonomy with explicit scope semantics (`bearer`, `item_self`, `native_identity`).
- MAngband verification of item-self protection semantics and when they should not propagate to bearer state.
- Cross-file uniqueness/resolution policy.

### Acceptance Criteria

- Every capability ID has one canonical semantic definition documented in ADR tables.
- Any duplicate ID across files is explicitly marked as a temporary parity duplicate with rationale.
- Validation rules can unambiguously resolve capability references.

---

### Finding C-02

Reference:
- Original review finding ID: F-01 (grant source concerns)

### Current State

- `capabilities.json` contains `grant_sources` metadata indicating potential sources (race/ring/amulet/etc.).
- This metadata is descriptive and may drift from actual definitions unless validated.

### Decision

Accepted

### Recommendation

Retain `grant_sources` as documentation metadata for now, but treat it as non-authoritative until validation support exists. It should not drive runtime behavior.

### Benefits

- Useful architectural documentation for designers.
- Supports future schema-level governance without runtime coupling.

### Risks

- Stale metadata can mislead if interpreted as source-of-truth.

### Dependencies

- Metadata governance rule: runtime must never infer behavior from `grant_sources`.
- Optional future validation consistency checks.

### Acceptance Criteria

- Team guidance explicitly labels `grant_sources` as documentation-only.
- No runtime decision depends on this field.

---

## 2) Affix Model

### Finding A-01

Reference:
- Original review finding IDs: F-04, F-08

### Current State

- `affixes[]` is used for numeric modifiers in several item categories.
- Additional affix types are present in data but not fully catalogued in `item_affixes.json` (documented known limitation).
- Resistances and other passive mechanics are split between `capability_ids[]` and `affixes[]`.

### Decision

Accepted

### Recommendation

Keep affixes distinct from capabilities in the medium term.

Architecture rule:
- Capabilities represent discrete passive properties (boolean-like gameplay toggles and identity traits).
- Affixes represent numeric tuning values (stats, rates, percentages, power modifiers).

Do not replace affixes with scaled capabilities until parity evidence shows no information loss.

### Benefits

- Aligns with MAngband-like conceptual split between flags and magnitudes.
- Easier to reason about balancing data separately from passive trait toggles.
- Lower risk than broad unification during parity phase.

### Risks

- Two systems require stronger authoring conventions.
- Some modifiers can look conceptually close to capabilities, causing ambiguity.

### Dependencies

- Canonical affix catalog completion (including IronHell-specific affix types currently undocumented in catalog file).
- Authoring rules for what must be an affix vs capability.

### Acceptance Criteria

- Every numeric modifier type is catalogued and documented.
- New content can be classified by a deterministic decision table: capability or affix.

---

### Finding A-02

Reference:
- Original review finding IDs: F-04, F-08

### Current State

- Some item definitions currently mix passive resist capability and resistance percent affix patterns.

### Decision

Requires MAngband Verification

### Recommendation

Before any model unification, verify per mechanic whether the data needs both:
- Binary eligibility/behavior toggle
- Numeric magnitude channel

If both are required by parity, keep both channels by design.

### Benefits

- Prevents flattening distinct mechanics into an oversimplified model.

### Risks

- Verification effort is non-trivial and requires reference-by-reference checks.

### Dependencies

- MAngband reference mapping for each resistance/mechanic type.

### Acceptance Criteria

- Each mixed mechanic has documented parity rationale and final representation rule.

---

## 3) Effects and Actions

### Finding E-01

Reference:
- Original review finding ID: F-02 (effects library orphaning)

### Current State

- `effects.json` is a named effect catalog.
- Consumables and devices primarily use inline `effects[]` objects.
- Catalog reuse is currently weak/inconsistent.

### Decision

Deferred

### Recommendation

Keep `effects.json` in place for now, but classify it as a design-time library, not canonical runtime source. Do not enforce immediate removal or immediate centralization.

### Benefits

- Avoids premature deletion of potentially useful design abstractions.
- Preserves optional reuse catalog while parity work proceeds.

### Risks

- Continued duality (named library + inline effects) may confuse authors.

### Dependencies

- Reference scan proving whether named effects can faithfully express all current inline patterns without losing determinism.

### Acceptance Criteria

- ADR addendum clearly defines whether named effects are authoritative, optional aliasing, or deprecated.

---

### Finding E-02

Reference:
- Original review finding ID: F-03 (activations indirection)

### Current State

- `activations.json` defines activation payloads.
- `artifacts.json` references activation IDs.
- Duplicate legacy-like activation entries exist (e.g., equivalent starlight entries).

### Decision

Accepted

### Recommendation

Retain separate `activations.json` as a valid indirection layer during parity-first era.

Rationale: activation IDs preserve recognizable MAngband-like identity and support deterministic cataloging of activation semantics.

### Benefits

- Stable identifier layer for artifact activations.
- Supports controlled deduplication without changing artifact payload fields.
- Useful for parity tracing and testability.

### Risks

- Duplicate definitions can persist if governance is weak.

### Dependencies

- Activation catalog hygiene rules (no semantic duplicates unless explicit compatibility alias).

### Acceptance Criteria

- Every activation ID maps to one documented semantic behavior.
- Compatibility aliases are explicitly annotated and tested.

---

### Finding E-03

Reference:
- Original review finding IDs: F-03, F-05, F-06

### Current State

- Gameplay effects are represented in multiple vocabularies:
  - canonical-like item effect IDs
  - activation-specific shorthand
  - monster-specific MAngband codes

### Decision

Deferred

### Recommendation

Adopt “actions as universal runtime concept” as a long-term target architecture, but do not force vocabulary unification until parity mappings are verified for monsters, activations, and edge cases.

### Benefits

- Provides a coherent future architecture direction.
- Avoids risky premature convergence.

### Risks

- Keeping multiple vocabularies longer increases mental overhead.

### Dependencies

- Verified translation tables from MAngband blow/spell/effect vocabulary to canonical action vocabulary.
- Deterministic mapping rules for all edge cases.

### Acceptance Criteria

- A complete, reference-backed mapping exists for each vocabulary domain.
- No unresolved semantics remain before execution-oriented refactor decisions.

---

## 4) Status Model

### Finding S-01

Reference:
- Original review finding IDs: F-05, F-07

### Current State

- `statuses.json` is generally clean and timed-status oriented.
- `apply_status` and `cure_status` are clear action verbs for temporal state transitions.

### Decision

Accepted

### Recommendation

Retain current status model foundations. Continue treating statuses as temporal state and actions as the mechanism to apply/remove them.

### Benefits

- Clear temporal semantics.
- Strong fit for deterministic turn/tick behavior.

### Risks

- Future content may accidentally encode always-on capabilities as statuses.

### Dependencies

- Guardrail rules distinguishing status vs capability.

### Acceptance Criteria

- New statuses must include explicit temporal or conditional lifecycle rationale.
- No permanent passive trait is authored as a status.

---

### Finding S-02

Reference:
- Original review finding IDs: F-05, F-08

### Current State

- Some effects can be represented both as shorthand effect IDs and as `apply_status` to a specific status concept.

### Decision

Deferred

### Recommendation

Do not immediately eliminate shorthand effect IDs. Instead, define canonical equivalence classes and decide per-case whether shorthand is retained for fidelity/readability.

### Benefits

- Maintains compatibility while avoiding sudden mass normalization.

### Risks

- Semantic duplication remains if equivalence governance is weak.

### Dependencies

- Equivalence table and validation strategy.

### Acceptance Criteria

- Each duplicated concept has a chosen canonical form plus compatibility policy.

---

## 5) Conditions

### Finding K-01

Reference:
- Original review finding IDs: F-07, F-08, L-03

### Current State

- Condition object currently supports:
  - `requires_absence`
  - `forced`
  - `on_next_melee_hit`
- These express different semantics: guard, override, deferred trigger.

### Decision

Accepted

### Recommendation

Keep the condition model minimal and explicit with exactly these three core forms until parity proves additional forms are necessary.

Classification rule:
- Guard conditions: `requires_absence`
- Override conditions: `forced`
- Deferred trigger conditions: `on_next_melee_hit`

### Benefits

- High clarity and validation simplicity.
- Prevents growth of ad-hoc mini-language patterns.

### Risks

- Some future mechanics may pressure expansion.

### Dependencies

- Trigger vs guard semantics documentation in architecture notes.
- Validation checks to ensure each condition type is used in appropriate context.

### Acceptance Criteria

- No legacy string conditions appear in data.
- Every condition instance can be classified as guard/override/trigger.

---

### Finding K-02

Reference:
- Original review finding ID: F-07

### Current State

- `blocked_by` legacy semantics were historically used in string format but are effectively represented by absence/guard logic.

### Decision

Rejected

### Recommendation

Reject reintroduction of a separate `blocked_by` condition type in core architecture unless MAngband verification demonstrates behavior not representable by `requires_absence`.

### Benefits

- Avoids duplicate condition semantics.
- Keeps deterministic condition evaluation straightforward.

### Risks

- If a truly distinct mechanic appears, rework may be required.

### Dependencies

- MAngband behavior verification for edge-condition semantics.

### Acceptance Criteria

- No new condition variant duplicates existing semantics.

---

## 6) Monster Model

### Finding M-01

Reference:
- Original review finding ID: F-06

### Current State

- Monster blows/spells use MAngband-aligned effect vocabulary (e.g., blow/spell codes), not item action vocabulary.

### Decision

Accepted

### Recommendation

Retain MAngband-aligned monster vocabulary as authoritative representation during parity phase.

### Benefits

- Preserves highest-fidelity source mapping.
- Easier diffing against reference data and behavior.

### Risks

- Cross-domain tooling remains fragmented.

### Dependencies

- Comprehensive monster effect code glossary and behavior notes.

### Acceptance Criteria

- Every monster effect code used in data has documented MAngband meaning and deterministic resolution notes.

---

### Finding M-02

Reference:
- Original review finding IDs: F-06, F-10

### Current State

- Canonical action unification for monsters is architecturally attractive but currently unverifiable end-to-end without full mapping proof.

### Decision

Requires MAngband Verification

### Recommendation

Treat monster action unification as a future candidate only after full verified mapping tables exist for blow and spell vocabularies, including edge cases (steals, disenchant, XP drain tiers, summon variants).

### Benefits

- Prevents semantic drift in one of the most parity-sensitive systems.

### Risks

- Delayed architecture convergence.

### Dependencies

- Verified mapping inventory from MAngband references and parity test scenarios.

### Acceptance Criteria

- 100% of monster effect codes have reviewed mapping proposals and parity rationale.

---

### Finding M-03

Reference:
- Original review finding IDs: F-06, V-13

### Current State

- Monster definitions can be improved by stronger validation even without changing representation.

### Decision

Accepted

### Recommendation

Add/maintain validation improvements without redesign:
- enforce known blow method enum
- enforce known monster effect code enums
- enforce dice integrity constraints
- enforce spell cooldown/range consistency rules

### Benefits

- Immediate quality gains with low parity risk.

### Risks

- Slight schema complexity increase.

### Dependencies

- Consolidated allowed-code lists from current data + MAngband references.

### Acceptance Criteria

- Invalid blow/spell codes are rejected at validation time.

---

## 7) Item Architecture

### Finding I-01 (Armor and Weapons)

Reference:
- Original review finding IDs: F-04

### Current State

- Armor and weapon base structures are relatively stable and understandable.
- Core physical fields are deterministic and MAngband-aligned.

### Decision

Accepted

### Recommendation

Keep current base structure for armor/weapons. Avoid structural redesign before parity completion.

### Benefits

- Stable backbone for combat/item generation.

### Risks

- Incremental drift if exceptions accumulate.

### Dependencies

- Ongoing validation for field ranges and tval/sval uniqueness.

### Acceptance Criteria

- No structural rewrite needed to express parity behavior for core base items.

---

### Finding I-02 (Accessories)

Reference:
- Original review finding ID: F-08

### Current State

- Accessories are currently the most inconsistent item category (mixed capability, affix, and effect patterns).

### Decision

Accepted

### Recommendation

Do not migrate structure immediately, but lock policy now:
- equipped passive behavior should prefer passive channels (capabilities/affixes)
- triggered one-shot behavior should require explicit architectural exception rationale

### Benefits

- Stops inconsistency growth immediately.

### Risks

- Legacy inconsistencies remain until future cleanup.

### Dependencies

- Category-specific authoring rules and review checklist.

### Acceptance Criteria

- New accessories follow one coherent policy; no new phantom effect IDs.

---

### Finding I-03 (Artifacts and Ego Items)

Reference:
- Original review finding IDs: F-03, F-04

### Current State

- Artifacts/ego items retain strong MAngband heritage via flag-based representation and activation references.

### Decision

Accepted

### Recommendation

Keep flag-oriented representation as parity-authoritative for now. Any future normalized representation must be derivative and provably equivalent.

### Benefits

- Direct traceability to MAngband sources.
- Lower risk of semantic reinterpretation.

### Risks

- Parsing/authoring remains less uniform than other item categories.

### Dependencies

- Reference-verified flag semantic mapping table.

### Acceptance Criteria

- Every gameplay-relevant flag has deterministic documented effect mapping.

---

### Finding I-04 (Consumables, Devices, Chests)

Reference:
- Original review finding IDs: F-09, F-10

### Current State

- Potions/scrolls/consumables are largely coherent with inline effect payloads.
- Rod/wand/staff resource semantics need clearer data contracts.
- Chest trap representation is under-specified (`trap_chance` scalar-only style).

### Decision

Requires MAngband Verification

### Recommendation

Prioritize verification of rod recharge semantics and chest trap behavior definitions before any structural redesign.

### Benefits

- Targets highest-value ambiguity areas with parity-first discipline.

### Risks

- Delay in achieving unified device model.

### Dependencies

- MAngband reference extraction for recharge and trap tables.

### Acceptance Criteria

- Rod/wand/staff semantics and chest trap semantics are fully documented and testable.

---

### Finding I-05 (Never-Migrate Candidate)

Reference:
- Original review guidance: monster single-file and category file rules

### Current State

- `monster_compendium.json` intentionally remains a single large file to preserve ordering/reference behavior.

### Decision

Accepted

### Recommendation

Do not split monster compendium by subtype/depth/folder unless parity requirements or tooling constraints prove unavoidable.

### Benefits

- Preserves deterministic serial-order assumptions.

### Risks

- Large file ergonomics.

### Dependencies

- None in short term.

### Acceptance Criteria

- Serial-order-dependent behavior remains reproducible and diffable.

---

## 8) Folder and File Organisation

### Finding O-01

Reference:
- Original guidelines sections 2, 3, 4, 5

### Current State

- Current layout is mostly correct: flat top-level definition files with `items/` category folder and mirrored schema structure.

### Decision

Accepted

### Recommendation

Keep current organization rules as baseline governance:
- keep files flat by domain
- split only when both scale and subtype-boundary criteria are met
- never create single-file folders
- preserve schema mirror structure

### Benefits

- Stable long-term navigability.
- Consistent validation pipeline behavior.

### Risks

- Overly conservative splitting can reduce authoring ergonomics for very large files.

### Dependencies

- Periodic file-size/entry-count review checkpoints.

### Acceptance Criteria

- New content follows naming, placement, and schema co-location rules consistently.

---

### Finding O-02

Reference:
- Original review finding ID: F-09 and organization guidelines

### Current State

- Future folderization pressure may appear for genuinely new content systems.

### Decision

Deferred

### Recommendation

Allow future folder reorganization only when all are true:
- new schema family is required
- at least two files are needed
- content cannot be represented as entries in existing files

### Benefits

- Prevents speculative hierarchy growth.

### Risks

- Requires discipline as project grows.

### Dependencies

- Formal architecture review before each structural reorganization.

### Acceptance Criteria

- Any new folder proposal includes schema rationale and multi-file justification.

---

## Canonical Design Principles

1. Parity-first representation
   - Prefer structures that map directly to MAngband semantics, even if less elegant.

2. Deterministic data contracts
   - Every gameplay-relevant field must have deterministic interpretation.

3. Validation before normalization
   - Strengthen schema and cross-reference guarantees before broad structural refactors.

4. Single semantic owner per concept
   - Any concept (status, capability, activation meaning) must have one canonical meaning.

5. Compatibility through explicit policy
   - If duplicate vocabulary is temporarily allowed, document equivalence and limits.

6. Category-specific clarity
   - Item categories can differ structurally when that preserves fidelity and readability.

7. Minimal condition language
   - Keep guard/override/trigger conditions explicit and small.

8. No speculative restructuring
   - Folder/file reorganization happens only on demonstrated need.

---

## Proposed Canonical Model (Target Architecture)

This section describes the preferred future shape only; it does not prescribe immediate migration.

1. Capability domain
   - A unified capability taxonomy with explicit scopes (`native_identity`, `bearer_passive`, `item_self_passive`) and deterministic reference rules.
   - Whether physically stored in one file or more than one file is implementation detail; semantic unification is the architecture requirement.

2. Affix domain
   - Affixes remain the numeric modifier channel.
   - Capabilities remain the discrete passive trait channel.
   - Mixed mechanics explicitly document when both channels are required.

3. Action/effect domain
   - Actions are the conceptual runtime event abstraction across items, activations, monsters, and traps.
   - Domain-specific vocabularies may remain as authoring-facing surfaces if they map deterministically to canonical action semantics.

4. Status domain
   - Statuses remain temporal creature state with explicit lifecycle.
   - Apply/remove behavior remains action-driven.

5. Condition domain
   - Core condition forms stay minimal and typed:
     - guard (`requires_absence`)
     - override (`forced`)
     - deferred trigger (`on_next_melee_hit`)

6. Monster domain
   - MAngband vocabulary remains first-class for parity.
   - Canonical action projection is optional and derivative until full verification.

7. Item domain
   - Base equipment data remains stable and deterministic.
   - Accessories and device semantics adopt stricter category policies to avoid mixed paradigms.

8. Organization domain
   - Maintain current domain-flat structure with restrained, criteria-based splits.

---

## Migration Candidates

Potential future migrations only. Not execution directives.

### Candidate MC-01: Capability Taxonomy Consolidation

- Risk: High (cross-cutting references and overlap)
- Complexity: High
- Expected value: High (clarity + validation simplification)
- Verification requirements:
  - complete scope taxonomy
  - parity-proof mapping of item-self vs bearer capabilities

### Candidate MC-02: Accessory Representation Normalization

- Risk: Medium
- Complexity: Medium
- Expected value: High (removes most inconsistent item category behavior)
- Verification requirements:
  - sustain and cursed-stat interactions verified against MAngband semantics

### Candidate MC-03: Activation Catalog Hygiene and Alias Policy

- Risk: Low
- Complexity: Low
- Expected value: Medium
- Verification requirements:
  - duplicate/alias behaviors validated against source references

### Candidate MC-04: Monster Vocabulary Canonical Projection

- Risk: Very High
- Complexity: Very High
- Expected value: High (cross-domain uniformity)
- Verification requirements:
  - complete blow and spell mapping table
  - parity test cases for steals, summons, disenchant, XP drain tiers

### Candidate MC-05: Device Resource Semantics Clarification (Rods/Wands/Staves)

- Risk: Medium
- Complexity: Medium
- Expected value: Medium
- Verification requirements:
  - recharge/charge behavior table from MAngband references

### Candidate MC-06: Chest Trap Structured Model

- Risk: Medium
- Complexity: Medium
- Expected value: High (major gameplay clarity gap)
- Verification requirements:
  - trap type, probability, and effect behavior extraction from MAngband

### Candidate MC-07: Named Effect Library Role Decision

- Risk: Low
- Complexity: Low to Medium
- Expected value: Medium
- Verification requirements:
  - usage and parity viability audit for named-effect indirection

---

## Recommended Execution Order

### Phase A — Safe Improvements

- Enforce architecture governance rules (no new legacy condition strings, no new phantom effect IDs).
- Tighten authoring conventions per category, especially accessories.
- Document capability/affix/status classification decision tables.

### Phase B — Validation Improvements

- Expand schema/test constraints without changing structural model:
  - stronger cross-reference checks
  - monster code enum checks
  - item identity uniqueness checks
- Make metadata fields explicitly non-runtime where relevant.

### Phase C — Verified Refactors

- Execute only refactors that have completed MAngband verification packages:
  - accessory consistency refactor
  - device resource semantic refactor
  - chest trap model enhancement
  - capability scope cleanup

### Phase D — Post-Parity Refactors

- Consider deeper unifications after parity confidence is established:
  - canonical action projection across domains
  - capability storage consolidation strategy
  - optional simplification of duplicate vocabulary layers

---

## Final Decision Summary

- Keep parity-critical legacy-aligned structures where they provide traceability.
- Prefer governance and validation improvements before structural consolidation.
- Treat broad unification (capabilities/actions/monster vocabulary) as a verified future state, not immediate cleanup.
- Reject elegance-only refactors that are not backed by parity and deterministic behavior evidence.
