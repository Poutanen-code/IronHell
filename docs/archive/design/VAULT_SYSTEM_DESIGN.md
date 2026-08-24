# Vault System Design

Status: Phase 1 Planning (Documentation only)
Scope: Canonical gameplay-definition design for vaults
Non-goals:
- Runtime vault generation implementation
- Godot integration
- Gameplay Architecture V2 redesign
- Schema implementation
- Migration of actual vault data in this phase

Authority:
- Gameplay Architecture V2
- MAngband 1.5.3 vault expectations
- Existing repository definitions
- docs/references and legacy/rewrite/vault_templates.json (reference source)

---

## 1. Purpose

Define a future-proof canonical vault definition model and map legacy vault template concepts into that model.

This document defines structure and metadata requirements only.

---

## 2. Canonical Model: VaultDefinition

Required fields per task scope:
- `id`
- `rating`
- `tags`
- `dimensions`
- `layout`
- `legend`

Proposed canonical object:

```json
{
  "id": "string",
  "rating": 0,
  "tags": ["string"],
  "dimensions": {
    "rows": 0,
    "cols": 0
  },
  "layout": ["string"],
  "legend": {
    "glyph": {
      "kind": "terrain|monster_spawn|item_spawn|trap_spawn|special_marker",
      "ref": "string",
      "provenance_status": "verified|inferred|unresolved"
    }
  },
  "provenance_status": "verified|inferred|unresolved"
}
```

Design notes:
- `dimensions` is explicit to validate layout integrity.
- `legend` externalizes glyph semantics so layout is stable while interpretation evolves.
- `tags` remains open vocabulary but should include normalized categories.

---

## 3. Legacy Concept Mapping (vault_templates.json -> canonical)

Legacy fields observed:
- `id`
- `index`
- `name`
- `roomType`
- `rating`
- `rows`
- `cols`
- `layout`
- `tags`

Canonical mapping proposal:
- `id` -> `id` (verified)
- `rating` -> `rating` (verified)
- `tags` -> `tags` (verified)
- `rows` and `cols` -> `dimensions.rows` and `dimensions.cols` (verified)
- `layout` -> `layout` (verified)
- glyph meaning from `layout` -> `legend` entries (inferred)
- `roomType` -> normalized `tags` (inferred, for example `shape:type7`, `shape:type8`)
- `index` and `name` -> optional future metadata (inferred)

Unresolved:
- Whether `index` should be preserved as provenance-only metadata.
- Whether `name` should be canonical or descriptive-only.

---

## 4. Dimensions and Layout Rules

Verified:
- Every vault template has explicit row and column bounds.

Inferred canonical constraints:
- `layout.length` must equal `dimensions.rows`.
- Every layout string length must equal `dimensions.cols`.
- Whitespace glyphs in layout are valid glyphs and must not be trimmed.

Unresolved:
- Whether to support rotated or mirrored variants in data versus generation policy.

---

## 5. Legend Design

The `legend` map should define each glyph used by a vault layout.

### 5.1 Verified glyph classes

Verified from repository references and terrain conventions:
- `.` floor-like passable cell
- `#` wall-like blocking cell
- `%` permanent wall or vault outer shell marker
- `+` door-like barrier
- `^` trap marker
- `,` interior filler floor variant
- `X` dense wall/blocker marker

### 5.2 Inferred glyph classes

Inferred from vault templates:
- `*` item or treasure marker
- `&` monster or special spawn marker
- `@` elite or unique monster marker
- `9`, `8` high-value marker classes

### 5.3 Unresolved glyph classes

Unresolved and requiring parity research before data lock:
- Exact semantic distinctions between `&`, `@`, `9`, and `8`
- Whether `X` is always indestructible or context-dependent
- Whether `,` is equivalent to floor or a weighted placement token

---

## 6. Tag Taxonomy Proposal

Verified existing tags in source:
- `type7`
- `type8`
- `starter`
- `greater`
- `special`

Inferred normalized tagging layer (additive, not replacing source tags yet):
- `tier:starter`
- `tier:greater`
- `tier:special`
- `shape:type7`
- `shape:type8`
- `rating:low|mid|high`

Unresolved:
- Final threshold boundaries for rating buckets.

---

## 7. Future-Proofing Guidance

To keep schema evolution safe without data churn:
- Keep `layout` immutable text grid.
- Evolve semantics through `legend` and tags.
- Keep provenance markers so unresolved symbols can be tracked per vault.
- Allow optional metadata extensions later (for example placement bands, rarity bands, biome constraints) without changing required core fields.

---

## 8. Deferred to Later Phases

Not included in Phase 1:
- Actual vault content migration
- Placement algorithm implementation
- Spawn resolution implementation
- Loot and monster generation runtime logic
