# Activation Implementation

Status: Current runtime reference
Authority: MAngband 1.5.3 activation dispatch and the canonical action catalog
Sources: `ref-mangband/src/server/use-obj.c`, `spells1.c`, and `spells2.c`

## 1. Scope and Runtime Contract

An activation is an executable action composition with source-specific targeting, policy, and cooldown behavior. The runtime must preserve the distinction between canonical action payloads and activation policy. Activation cooldowns use the source timeout model (`base + random`), and target input must follow the source activation rather than the name of the action.

## 2. Migration Outcome

- 52 activation records audited.
- Canonical action catalog migration completed for the converted records.
- Contract audit: 0 required-parameter errors, 0 unknown-parameter errors, 0 enum/value errors, 0 structured-amount errors, and 0 duration-expression errors.
- Legacy payloads were removed after parity migration; compatibility aliases `HASTE` and `FIRE_BALL_2` remain explicitly marked as non-MAngband aliases.
- `FIREBRAND` was added for source artifact coverage.
- `BIZZARE` uses `RandomActionSelection` with weighted outcome groups matching the source ring-of-power branches.

## 3. Canonical Activation Requirements

The runtime activation bridge must support:

- source-family eligibility and action dispatch;
- aimed, point, touch, area, item, and self target inputs;
- source formulas for damage, duration, distance, and charge gain;
- secondary effects such as healing plus cut/fear removal;
- conditional refresh or stacking behavior;
- item selection and recharge failure/backfire policy;
- banishment selection, radius, LOS, special-level, and self-damage policy;
- delayed recall toggle semantics;
- action timeout and recharge state.

## 4. Corrected Parity Values

The catalog corrections include exact source values and geometry for `ILLUMINATION`, `MAGIC_MAP`, `PROT_EVIL`, `STAR_LIGHT`, `FIRE2`, `FIRE3`, `FROST4`, `FROST5`, `ELEC2`, `STAR_BALL`, `RAGE_BLESS_RESIST`, `HEAL1`, `HEAL2`, `CURE_WOUNDS`, `BERSERKER`, `RESIST`, `CLAIRVOYANCE`, `SLEEP`, `REM_FEAR_POIS`, `TELE_AWAY`, `WORD_OF_RECALL`, `STONE_TO_MUD`, `TRAP_DOOR_DEST`, `RECHARGE1`, `BANISHMENT`, `MASS_BANISHMENT`, `DRAIN_LIFE1`, `DRAIN_LIFE2`, `PROBE`, `CONFUSE`, `HASTE1`, and `HASTE2`.

Important source behaviors to preserve:

- `ILLUMINATION`: radius 3 and `2d15` light-sensitive damage.
- `STAR_LIGHT`: eight strong-light beams, not a single area-light action.
- `BIZZARE`: weighted random composite behavior.
- `RAGE_BLESS_RESIST`: heal 30, clear fear, and oppose five elements for `50 + 1d50`.
- `HEAL1` and `HEAL2`: clear cut; `CURE_WOUNDS` partially reduces cut.
- `HASTE1` and `HASTE2`: add 5 when haste is already active.
- `BANISHMENT`: nearest non-unique display type, with per-deletion self-damage.
- `MASS_BANISHMENT`: non-unique monsters within `MAX_SIGHT`, without LOS requirement, with per-deletion self-damage.
- `RECHARGE1`: wand/staff selection and source failure, charge-loss, destruction, and safe-recharge rules.

## 5. Remaining Runtime Adapters

Catalog representation is complete, but policy tokens still require executable adapters for exact C behavior:

- amount scaling and player-level formulas;
- distance and radius rules;
- conditional status refresh;
- activation source eligibility;
- recharge failure and destruction;
- banishment selection and strain damage;
- directed beams and multi-ray composition;
- delayed recall;
- random action selection.

These are runtime implementation tasks, not catalog redesign tasks.

## 6. Parity Evidence

The authoritative source map includes activation dispatch in `use-obj.c`, direction requirements in `object2.c`, effect handlers in `spells1.c` and `spells2.c`, and recharge behavior in `spells2.c`. The final catalog status is structurally migrated but runtime parity depends on the adapters above.
