# Phase 1 Definition Gap Analysis

Status: Phase 1 Planning (Documentation only)
Scope: Terrain, traps, stores, and vault definition readiness
Non-goals:
- Runtime implementation
- Schema implementation
- Gameplay behavior changes

Authority:
- Gameplay Architecture V2
- MAngband 1.5.3 parity objective
- Existing repository references and definitions

---

## 1. Purpose

Identify missing gameplay-definition information required to complete Phase 1 definition infrastructure for:
- Terrain
- Traps
- Stores
- Vaults

Each gap is marked as `verified`, `inferred`, or `unresolved`.

---

## 2. Priority Model

- P0: Blocks parity-critical gameplay semantics or causes high risk of incorrect behavior.
- P1: Strongly impacts feature completeness and balancing.
- P2: Important for maintainability and long-term quality, but not immediate parity blockers.

---

## 3. Terrain Data Gaps

### 3.1 Missing data

| Priority | Gap | Status | Why it matters |
|---|---|---|---|
| P0 | Canonical `destroyable` truth per terrain entry | unresolved | Needed for digging, destruction, and mutation parity. |
| P0 | Canonical separation of hidden/disguised terrain (`appears_as`) from base feature identity | verified | Secret doors, hidden traps, and hidden treasure veins require explicit alias semantics. |
| P1 | Stable `feature_tags` vocabulary | inferred | Needed to avoid symbol-only overloading and enable validation. |
| P1 | Category decision for edge features (`glyph_of_warding`, `darkness`, `drawbridge`) | unresolved | Affects taxonomy consistency and downstream schema constraints. |
| P2 | Wilderness-vs-dungeon applicability tags | inferred | Improves generation and validation separation by context. |

### 3.2 Terrain summary

Most base feature identity exists, but parity-relevant behavioral metadata is incomplete.

---

## 4. Trap Data Gaps

### 4.1 Missing data

| Priority | Gap | Status | Why it matters |
|---|---|---|---|
| P0 | Explicit trigger definitions (on_enter, chest-open, disarm-failure, etc.) per trap family | verified | Trigger semantics are policy-critical and cannot be inferred safely at runtime. |
| P0 | Canonical detection and disarm parameter fields (difficulty, failure consequences) | unresolved | Required for trap fairness and parity behavior. |
| P1 | Effect payload linkage to canonical action identifiers | inferred | Needed to preserve Source -> Policy -> Actions consistency. |
| P1 | Visibility model fields supporting hidden, discovered, and disguised states | verified | Trap discovery is a core gameplay behavior. |
| P2 | Trap placement metadata (depth bands, rarity, biome constraints) | unresolved | Needed for generation fidelity but not immediate for model definition. |

### 4.2 Trap summary

Trap identities are present in references, but operational metadata required for parity-safe execution design is still missing.

---

## 5. Store Data Gaps

### 5.1 Missing data

| Priority | Gap | Status | Why it matters |
|---|---|---|---|
| P0 | Inventory profile definitions per store type | unresolved | Stores cannot be parity-accurate without item domain and quality rules. |
| P0 | Pricing profile definitions per store type | unresolved | Economy behavior and buy/sell values depend on explicit profile data. |
| P1 | Turnover profile definitions (interval, replacement count, retention rules) | unresolved | Needed for deterministic and fair store refresh behavior. |
| P1 | Home store special handling as storage, not commerce | inferred | Prevents incorrect commercialization of Home behavior. |
| P2 | Advanced access and social modifier hooks | inferred | Supports maintainable extension without redesign. |

### 5.2 Store summary

Store terrain identities are available, but store business logic metadata is largely absent.

---

## 6. Vault Data Gaps

### 6.1 Missing data

| Priority | Gap | Status | Why it matters |
|---|---|---|---|
| P0 | Canonical glyph legend semantics (`legend`) for non-terrain symbols | unresolved | Vault layouts cannot be interpreted consistently without symbol semantics. |
| P0 | Placement-rule metadata (depth, rarity, constraints) | unresolved | Core to parity and generation correctness. |
| P1 | Normalized tag taxonomy aligned to gameplay use | inferred | Improves consistency and queryability for generation rules. |
| P1 | Explicit provenance tracking for ambiguous glyph classes (`@`, `&`, `8`, `9`) | inferred | Avoids accidental semantic drift during migration. |
| P2 | Optional descriptive metadata policy (`name`, `legacy_index`) | unresolved | Maintains traceability with legacy templates. |

### 6.2 Vault summary

Layout templates exist and are valuable, but metadata required for robust canonical interpretation is incomplete.

---

## 7. Cross-Domain Gaps

| Priority | Gap | Status | Why it matters |
|---|---|---|---|
| P0 | Shared provenance discipline across all new definition domains | verified | Required to keep parity reasoning explicit and auditable. |
| P1 | Shared tag normalization strategy across terrain, traps, stores, and vaults | inferred | Reduces schema drift and authoring ambiguity. |
| P1 | Validation contracts for inter-definition references | inferred | Prevents broken references when new files are introduced. |

---

## 8. Recommended Information Acquisition Order

1. P0 terrain destructibility and alias semantics.
2. P0 trap trigger and disarm/detection parameterization.
3. P0 store inventory and pricing profiles.
4. P0 vault legend semantics and placement metadata.
5. P1/P2 normalization and traceability enhancements.

This order follows gameplay importance and parity risk.

---

## 9. Acceptance Alignment

This document satisfies Phase 1 constraints:
- Documentation only.
- No runtime implementation.
- No gameplay behavior change.
- No targeting or activation redesign.
- No schema implementation yet.
- Verified, inferred, and unresolved distinctions are explicit.
