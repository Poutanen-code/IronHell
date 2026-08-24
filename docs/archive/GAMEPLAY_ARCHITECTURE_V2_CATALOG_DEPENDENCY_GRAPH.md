# Gameplay Architecture V2 Catalog Dependency Graph

Date: 2026-08-22

## Canonical Dependency Graph

- `resistances.json` -> `statuses.json`
  - via `status_id` for oppose semantics.
- `capabilities.json` -> `resistances.json`
  - via compatibility-only `resistance_id` references on deprecated overlap entries.
- `ego_items.json` -> `item_affixes.json`
  - via `_meta.flag_effects.affix_id` and `_meta.normalized_flag_catalog.affix_id`.
- `ego_items.json` -> `capabilities.json`
  - via `_meta.flag_effects.capability_id` and `_meta.normalized_flag_catalog.capability_id`.
- `ego_items.json` -> `resistances.json`
  - via `_meta.flag_effects.resistance_id` and `_meta.normalized_flag_catalog.resistance_id`.
- `activations.json` -> `actions.json`
  - via `effects[].action_id`.
- `actions.json` -> `statuses.json`
  - where action contracts reference status IDs (for timed/status actions).

## Ownership Map

- Affix owner: `item_affixes.json`
- Capability owner: `capabilities.json`
- Resistance owner: `resistances.json`
- Status owner: `statuses.json`
- Action owner: `actions.json` and activation compositions in `activations.json`
- Policy owner: runtime policy + explicit catalog policy metadata (`ego_items.json` combat modifier policy, capability `policy_hooks`)

## Normalization Notes

- Compatibility edges are intentional and migration-safe.
- No new top-level catalogs were introduced.
- Existing catalogs were extended with metadata only.
