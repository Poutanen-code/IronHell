# Item Parity Audit

Status: Historical validation consolidated for runtime reference
Sources: MAngband 1.5.3 item/artifact definitions and IronHell item catalogs

## 1. Final Validation Metrics

- Source artifact count: 136.
- IronHell artifact count after correction: 136.
- Missing artifact serials after correction: none.
- Extra artifacts: none.
- Artifact descriptions: source-complete after continuation-line normalization.
- Cross-catalog broken references: 0 in the audited item catalogs.
- Flavor IDs: unique, with pools covered by the audited item categories.
- Activation rename decisions: `STARLIGHT` -> `STAR_LIGHT` and `WOR` -> `WORD_OF_RECALL` are canonical renames.

## 2. Artifact Field Parity

The corrected artifact catalog matches source serials, numeric fields, flags, descriptions, recharge data, and activation references. Schema-required defaults for source records without `P:` fields are documented as defaults rather than gameplay differences. The corrected entries include the previously missing serials 126-136.

Preserve the source-derived corrections for base AC, damage, hit/damage/AC bonuses, tval/sval/pval, depth, rarity, weight, cost, recharge values, flags, and activation identifiers.

## 3. Activation Parity

Artifact activation references now use the canonical catalog identifiers. Source artifact serial 126 uses `FIREBRAND`, which is now defined. Compatibility aliases are explicitly documented where retained. Exact activation behavior remains governed by `docs/implementation/ACTIVATION_IMPLEMENTATION.md`.

## 4. Item Semantic Ownership

Item normalization removed duplicated semantic descriptions from ego flag effects. Canonical ownership is:

- numeric modifiers: `item_affixes.json`;
- capabilities: `capabilities.json`;
- resistances and ignores: `resistances.json`;
- timed effects: `statuses.json`;
- slay, kill, brand, and curse flags: item metadata resolved by runtime policy;
- activation behavior: `activations.json` and `actions.json`.

Compatibility aliases may remain for source traceability, but they must not become competing authoring definitions.

## 5. Flavor and Cross-Reference Validation

Flavor pools were checked for duplicate IDs and category coverage. Item references to actions, statuses, capabilities, resistances, and activations were checked for broken IDs. Preserve these outcomes as regression expectations for automated validation.

## 6. Remaining Parity Risks

- Runtime aggregation must distinguish bearer capabilities from item-self protections.
- Slay/kill/brand multipliers and immunity checks remain combat-policy behavior.
- `MIGHT`, `SHOTS`, and `BLOWS` must feed runtime stat aggregation.
- Future item changes must preserve source serial identity and canonical reference ownership.
