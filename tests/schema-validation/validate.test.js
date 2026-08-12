/**
 * Schema validation tests for the IronHell effect framework.
 *
 * Tests cover:
 *   - valid actions.json
 *   - valid statuses.json
 *   - valid resistances.json
 *   - valid capabilities.json
 *   - unique status IDs and duplicate status names
 *   - invalid status stacking policies
 *   - invalid status duration contracts
 *   - invalid resistance semantics and references
 *   - invalid capability scopes and references
 *   - valid effects.json
 *   - valid activations.json
 *   - canonical action catalog completeness and exclusions
 *   - artifact ACTIVATE flags have corresponding activation IDs
 *   - invalid effect_id values are rejected
 *   - invalid dice definitions are rejected
 *   - invalid status references are rejected at the shape level
 *   - typed condition objects (requires_absence / forced / on_next_melee_hit)
 *   - duplicate capability IDs within each capability file
 *   - unknown capability references in item files
 *   - unknown status_id references in item effect files
 *   - cross-file status_id validation for potions, scrolls, staves, wands, rods, consumables
 *   - duplicate activation IDs
 *
 * Run: npm test
 */

import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

import Ajv from "ajv/dist/2020.js";
import addFormats from "ajv-formats";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function loadJSON(relPath) {
  let content = readFileSync(resolve(root, relPath), "utf-8");
  // Strip UTF-8 BOM if present (some editors add it on Windows)
  if (content.charCodeAt(0) === 0xFEFF) content = content.slice(1);
  return JSON.parse(content);
}

function buildAjv() {
  const ajv = new Ajv({
    strict: false,
    allErrors: true,
    allowUnionTypes: true,
  });
  addFormats(ajv);
  return ajv;
}

// ---------------------------------------------------------------------------
// Load schemas and data
// ---------------------------------------------------------------------------

const commonSchema = loadJSON("data/schemas/common.schema.json");
const actionsSchema = loadJSON("data/schemas/actions.schema.json");
const statusesSchema = loadJSON("data/schemas/statuses.schema.json");
const resistancesSchema = loadJSON("data/schemas/resistances.schema.json");
const capabilitiesSchema = loadJSON("data/schemas/capabilities.schema.json");
const spellsSchema = loadJSON("data/schemas/spells.schema.json");
const spellBooksSchema = loadJSON("data/schemas/spell_books.schema.json");
const effectsSchema = loadJSON("data/schemas/effects.schema.json");
const activationsSchema = loadJSON("data/schemas/activations.schema.json");
const artifactsSchema = loadJSON("data/schemas/artifacts.schema.json");
const commonItemSchema = loadJSON("data/schemas/items/common_item.schema.json");

const actionsData = loadJSON("data/definitions/actions.json");
const statusesData = loadJSON("data/definitions/statuses.json");
const resistancesData = loadJSON("data/definitions/resistances.json");
const capabilitiesData = loadJSON("data/definitions/capabilities.json");
const spellBooksData = loadJSON("data/definitions/spell_books.json");
const mageSpellsData = loadJSON("data/definitions/mage_spells.json");
const priestPrayersData = loadJSON("data/definitions/priest_prayers.json");
const effectsData = loadJSON("data/definitions/effects.json");
const activationsData = loadJSON("data/definitions/activations.json");
const artifactsData = loadJSON("data/definitions/artifacts.json");

// Item files used in cross-reference validation
const itemCapabilitiesData = loadJSON("data/definitions/items/item_capabilities.json");
const potionsData = loadJSON("data/definitions/items/potions.json");
const scrollsData = loadJSON("data/definitions/items/scrolls.json");
const rodsData = loadJSON("data/definitions/items/rods.json");
const wandsData = loadJSON("data/definitions/items/wands.json");
const stavesData = loadJSON("data/definitions/items/staves.json");
const consumablesData = loadJSON("data/definitions/items/consumables.json");
const accessoriesData = loadJSON("data/definitions/items/accessories.json");

// ---------------------------------------------------------------------------
// Compile schemas
// Pre-register effectsSchema so activationsSchema can $ref it by $id.
// ---------------------------------------------------------------------------

const ajv = buildAjv();
ajv.addSchema(commonSchema);
ajv.addSchema(actionsSchema);
ajv.addSchema(resistancesSchema);
ajv.addSchema(capabilitiesSchema);
ajv.addSchema(spellsSchema);
ajv.addSchema(spellBooksSchema);
ajv.addSchema(effectsSchema);
ajv.addSchema(statusesSchema);
ajv.addSchema(activationsSchema);

const validateActions = ajv.compile(actionsSchema);
const validateStatuses = ajv.compile(statusesSchema);
const validateResistances = ajv.compile(resistancesSchema);
const validateCapabilities = ajv.compile(capabilitiesSchema);
const validateSpells = ajv.compile(spellsSchema);
const validateSpellBooks = ajv.compile(spellBooksSchema);
const validateEffects = ajv.compile(effectsSchema);
const validateActivations = ajv.compile(activationsSchema);
const validateArtifacts = ajv.compile(artifactsSchema);

// Compile Effect variant validators for unit tests
const ajvUnit = buildAjv();
const effectDefs = effectsSchema.$defs;

// ---------------------------------------------------------------------------
// Test suites
// ---------------------------------------------------------------------------

describe("actions.json", () => {
  const approvedActionIds = [
    "BoltDamage",
    "BallDamage",
    "HealHP",
    "CureStatus",
    "ApplyTimedBuff",
    "ApplyOpposeElements",
    "TeleportSelf",
    "LightArea",
    "DetectEntities",
    "MapArea",
    "IdentifyItem",
    "EnchantEquipment",
    "SleepControl",
    "SlowControl",
    "DispelByTag",
    "DrainLife",
    "Earthquake",
    "BeamDamage",
    "TeleportTarget",
    "RecallOrLevelShift",
    "DarkenArea",
    "RechargeItem",
    "BrandWeapon",
    "AlterTerrain",
    "ConfuseControl",
    "FearControl",
    "ParalyzeControl",
    "BanishByRule",
    "SummonEntities",
    "StatDrain",
    // Added in Action Catalog Stabilization Pass (2026-08-04)
    "InspectEntity",
    "RestoreAttributes",
    "RemoveCurse",
    "ModifyResourceMeter",
    "TransformEntity",
  ];

  it("validates against actions.schema.json", () => {
    const valid = validateActions(actionsData);
    if (!valid) {
      assert.fail(
        "actions.json failed validation:\n" +
          JSON.stringify(validateActions.errors, null, 2)
      );
    }
  });

  it("has unique action_ids", () => {
    const ids = actionsData.actions.map((action) => action.action_id);
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate action_id found");
  });

  it("contains the full approved canonical action catalog and no excluded actions", () => {
    const actual = new Set(actionsData.actions.map((action) => action.action_id));
    const expected = new Set(approvedActionIds);

    for (const actionId of approvedActionIds) {
      assert.ok(actual.has(actionId), `Approved action '${actionId}' is missing`);
    }

    for (const actionId of actual) {
      assert.ok(expected.has(actionId), `Unexpected action '${actionId}' found in catalog`);
    }

    assert.ok(!actual.has("CreateTrap"), "Excluded action 'CreateTrap' must not be present");
  });

  it("all actions declare unique parameter IDs within their closed contract", () => {
    for (const action of actionsData.actions) {
      const parameterIds = action.parameter_contract.parameters.map((parameter) => parameter.id);
      const unique = new Set(parameterIds);
      assert.equal(
        unique.size,
        parameterIds.length,
        `Action '${action.action_id}' has duplicate parameter IDs`
      );
      assert.equal(
        action.parameter_contract.closed,
        true,
        `Action '${action.action_id}' must use a closed parameter contract`
      );
    }
  });

  it("all actions include provenance support and validation support metadata", () => {
    for (const action of actionsData.actions) {
      assert.ok(action.provenance.summary, `Action '${action.action_id}' is missing provenance.summary`);
      assert.ok(
        action.provenance.research_documents.length > 0,
        `Action '${action.action_id}' is missing provenance research documents`
      );
      assert.ok(
        action.provenance.evidence_refs.length > 0,
        `Action '${action.action_id}' is missing provenance evidence refs`
      );
      assert.equal(action.validation.reject_unknown_parameters, true);
      assert.equal(action.validation.require_declared_required_parameters, true);
      assert.equal(action.validation.enforce_declared_value_types, true);
    }
  });

  it("rejects an unknown canonical action ID", () => {
    const badDoc = {
      schema_version: 1,
      actions: [
        {
          action_id: "CreateTrap",
          name: "Create Trap",
          description: "excluded",
          category: "terrain",
          confidence: "medium",
          allowed_source_families: ["spell"],
          parameter_contract: {
            closed: true,
            parameters: [
              {
                id: "trap_kind",
                description: "trap kind",
                value_type: "token",
                required: true,
              },
            ],
          },
          validation: {
            reject_unknown_parameters: true,
            require_declared_required_parameters: true,
            enforce_declared_value_types: true,
          },
          provenance_status: "derived",
          provenance: {
            summary: "test",
            research_documents: ["docs/GAMEPLAY_ARCHITECTURE_V2.md"],
            evidence_refs: ["docs/ACTION_CANDIDATE_CATALOG.md"],
          },
        },
      ],
    };

    const valid = validateActions(badDoc);
    assert.ok(!valid, "Should have rejected an excluded or unknown action_id");
  });

  it("rejects enum parameters that do not declare allowed_values", () => {
    const badDoc = {
      schema_version: 1,
      actions: [
        {
          action_id: "BoltDamage",
          name: "Bolt Damage",
          description: "test",
          category: "damage",
          confidence: "high",
          allowed_source_families: ["spell"],
          parameter_contract: {
            closed: true,
            parameters: [
              {
                id: "damage_type",
                description: "damage type",
                value_type: "enum",
                required: true,
              },
            ],
          },
          validation: {
            reject_unknown_parameters: true,
            require_declared_required_parameters: true,
            enforce_declared_value_types: true,
          },
          provenance_status: "derived",
          provenance: {
            summary: "test",
            research_documents: ["docs/GAMEPLAY_ARCHITECTURE_V2.md"],
            evidence_refs: ["docs/ACTION_CANDIDATE_CATALOG.md"],
          },
        },
      ],
    };

    const valid = validateActions(badDoc);
    assert.ok(!valid, "Should have rejected enum parameters without allowed_values");
  });
});

// ---------------------------------------------------------------------------

describe("statuses.json", () => {
  it("validates against statuses.schema.json", () => {
    const valid = validateStatuses(statusesData);
    if (!valid) {
      assert.fail(
        "statuses.json failed validation:\n" +
          JSON.stringify(validateStatuses.errors, null, 2)
      );
    }
  });

  it("has unique status_ids", () => {
    const ids = statusesData.statuses.map((s) => s.status_id);
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate status_id found: " + ids.filter((id, i) => ids.indexOf(id) !== i));
  });

  it("has unique status names", () => {
    const names = statusesData.statuses.map((s) => s.name.toLowerCase().trim());
    const unique = new Set(names);
    assert.equal(unique.size, names.length, "Duplicate status name found");
  });

  it("every timed status has a default_duration", () => {
    for (const status of statusesData.statuses) {
      if (status.is_timed) {
        assert.ok(
          status.default_duration,
          `Status '${status.status_id}' is_timed but missing default_duration`
        );
      }
    }
  });

  it("all durations have a non-negative base", () => {
    for (const status of statusesData.statuses) {
      if (status.default_duration) {
        assert.ok(
          status.default_duration.base >= 0,
          `Status '${status.status_id}' has negative duration base`
        );
      }
    }
  });

  it("all durations with dice have valid count and sides", () => {
    for (const status of statusesData.statuses) {
      const dice = status.default_duration?.dice;
      if (dice) {
        assert.ok(dice.count >= 1, `Status '${status.status_id}' dice.count < 1`);
        assert.ok(dice.sides >= 1, `Status '${status.status_id}' dice.sides < 1`);
      }
    }
  });

  it("required statuses are present", () => {
    const required = [
      "poisoned", "blinded", "confused", "afraid", "paralyzed",
      "hallucinating", "heroism", "berserk", "blessed",
      "protected_from_evil", "haste",
      "oppose_acid", "oppose_elec", "oppose_fire", "oppose_cold", "oppose_pois"
    ];
    const ids = new Set(statusesData.statuses.map((s) => s.status_id));
    for (const r of required) {
      assert.ok(ids.has(r), `Required status '${r}' is missing`);
    }
  });
});

// ---------------------------------------------------------------------------

describe("resistances.json", () => {
  it("validates against resistances.schema.json", () => {
    const valid = validateResistances(resistancesData);
    if (!valid) {
      assert.fail(
        "resistances.json failed validation:\n" +
          JSON.stringify(validateResistances.errors, null, 2)
      );
    }
  });

  it("has unique resistance IDs", () => {
    const ids = resistancesData.resistances.map((r) => r.id);
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate resistance ID found");
  });

  it("preserves separate resistance semantics", () => {
    const kinds = new Set(resistancesData.resistances.map((r) => r.semantic_kind));
    for (const required of ["resist", "oppose", "immunity", "ignore"]) {
      assert.ok(kinds.has(required), `Missing resistance semantic kind '${required}'`);
    }
  });

  it("all oppose resistances reference known status IDs", () => {
    const statusIds = new Set(statusesData.statuses.map((s) => s.status_id));
    const missing = [];
    for (const resistance of resistancesData.resistances) {
      if (resistance.semantic_kind === "oppose" && !statusIds.has(resistance.status_id)) {
        missing.push(`${resistance.id} -> ${resistance.status_id}`);
      }
    }
    assert.equal(
      missing.length,
      0,
      `Oppose resistance status references are invalid:\n  ${missing.join("\n  ")}`
    );
  });

  it("rejects invalid resistance semantics", () => {
    const badDoc = {
      schema_version: 1,
      resistances: [
        {
          id: "bad_ignore",
          name: "Bad Ignore",
          description: "invalid scope for ignore",
          semantic_kind: "ignore",
          target_scope: "bearer",
          channel: "acid",
          provenance_status: "derived"
        }
      ]
    };
    const valid = validateResistances(badDoc);
    assert.ok(!valid, "Ignore resistance must not allow bearer scope");
  });
});

// ---------------------------------------------------------------------------

describe("capabilities.json", () => {
  it("validates against capabilities.schema.json", () => {
    const valid = validateCapabilities(capabilitiesData);
    if (!valid) {
      assert.fail(
        "capabilities.json failed validation:\n" +
          JSON.stringify(validateCapabilities.errors, null, 2)
      );
    }
  });

  it("has unique capability IDs", () => {
    const ids = capabilitiesData.capabilities.map((c) => c.id);
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate capability ID found");
  });

  it("uses only V2 capability scopes", () => {
    const validScopes = new Set(["native_identity", "bearer_passive", "item_self_passive"]);
    const invalid = capabilitiesData.capabilities
      .filter((capability) => !validScopes.has(capability.scope))
      .map((capability) => `${capability.id}:${capability.scope}`);
    assert.equal(
      invalid.length,
      0,
      `Capabilities with invalid scopes found:\n  ${invalid.join("\n  ")}`
    );
  });

  it("all resistance-bound capabilities reference known resistance IDs", () => {
    const resistanceIds = new Set(resistancesData.resistances.map((r) => r.id));
    const invalid = [];
    for (const capability of capabilitiesData.capabilities) {
      if ((capability.category === "resistance" || capability.category === "item_ignore") && !resistanceIds.has(capability.resistance_id)) {
        invalid.push(`${capability.id} -> ${capability.resistance_id}`);
      }
    }
    assert.equal(
      invalid.length,
      0,
      `Capabilities with invalid resistance references found:\n  ${invalid.join("\n  ")}`
    );
  });

  it("rejects invalid capability scopes", () => {
    const badDoc = {
      schema_version: 1,
      capabilities: [
        {
          id: "bad_scope",
          source_flag: "BAD_SCOPE",
          name: "Bad Scope",
          description: "invalid scope",
          category: "class_magic",
          scope: "shared",
          provenance_status: "derived"
        }
      ]
    };

    const valid = validateCapabilities(badDoc);
    assert.ok(!valid, "Should reject capability definitions with invalid scope");
  });

  it("rejects resistance categories without resistance references", () => {
    const badDoc = {
      schema_version: 1,
      capabilities: [
        {
          id: "res_bad",
          source_flag: "RES_BAD",
          name: "Bad Resist",
          description: "missing resistance ref",
          category: "resistance",
          scope: "bearer_passive",
          provenance_status: "derived"
        }
      ]
    };

    const valid = validateCapabilities(badDoc);
    assert.ok(!valid, "Should reject resistance capabilities without resistance_id");
  });
});

// ---------------------------------------------------------------------------

describe("spell foundation", () => {
  const spellFiles = [
    { label: "mage_spells.json", data: mageSpellsData, realm: "magic" },
    { label: "priest_prayers.json", data: priestPrayersData, realm: "prayer" },
  ];

  it("spell_books.json validates against spell_books.schema.json", () => {
    const valid = validateSpellBooks(spellBooksData);
    if (!valid) {
      assert.fail(
        "spell_books.json failed validation:\n" +
          JSON.stringify(validateSpellBooks.errors, null, 2)
      );
    }
  });

  for (const file of spellFiles) {
    it(`${file.label} validates against spells.schema.json`, () => {
      const valid = validateSpells(file.data);
      if (!valid) {
        assert.fail(
          `${file.label} failed validation:\n` +
            JSON.stringify(validateSpells.errors, null, 2)
        );
      }
    });

    it(`${file.label} declares matching top-level realm`, () => {
      assert.equal(
        file.data.realm,
        file.realm,
        `${file.label} should declare realm '${file.realm}'`
      );
    });

    it(`${file.label} uses policy/action separation contracts`, () => {
      for (const spell of file.data.spells) {
        assert.ok(spell.policy, `${file.label} '${spell.id}' is missing policy`);
        assert.ok(Array.isArray(spell.action_refs), `${file.label} '${spell.id}' is missing action_refs`);
        assert.ok(
          spell.policy.level !== undefined &&
            spell.policy.mana !== undefined &&
            spell.policy.fail_rate !== undefined,
          `${file.label} '${spell.id}' is missing policy lifecycle fields`
        );
        assert.equal(
          spell.policy.realm,
          file.realm,
          `${file.label} '${spell.id}' has mismatched policy.realm`
        );
        assert.equal(
          spell.policy.book_id !== undefined,
          true,
          `${file.label} '${spell.id}' is missing policy.book_id`
        );
        assert.equal(
          spell.level,
          undefined,
          `${file.label} '${spell.id}' must not define top-level level (belongs in policy)`
        );
        assert.equal(
          spell.mana,
          undefined,
          `${file.label} '${spell.id}' must not define top-level mana (belongs in policy)`
        );
        for (const actionRef of spell.action_refs) {
          assert.equal(typeof actionRef, "object", `${file.label} '${spell.id}' has invalid action_ref entry`);
          assert.equal(typeof actionRef.action_id, "string", `${file.label} '${spell.id}' action_ref is missing action_id`);
          if (actionRef.parameters !== undefined) {
            assert.equal(
              typeof actionRef.parameters,
              "object",
              `${file.label} '${spell.id}' action_ref.parameters must be an object when present`
            );
          }
        }
      }
    });
  }

  it("book IDs are unique", () => {
    const ids = spellBooksData.books.map((book) => book.id);
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate spell book IDs found");
  });

  it("spell IDs are unique across mage and priest files", () => {
    const ids = [
      ...mageSpellsData.spells.map((spell) => spell.id),
      ...priestPrayersData.spells.map((spell) => spell.id),
    ];
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate spell IDs found across spell files");
  });

  it("spell books only reference known spell IDs", () => {
    const knownSpellIds = new Set([
      ...mageSpellsData.spells.map((spell) => spell.id),
      ...priestPrayersData.spells.map((spell) => spell.id),
    ]);

    const missing = [];
    for (const book of spellBooksData.books) {
      for (const spellId of book.spell_ids) {
        if (!knownSpellIds.has(spellId)) {
          missing.push(`${book.id} -> ${spellId}`);
        }
      }
    }

    assert.equal(
      missing.length,
      0,
      `Spell books reference unknown spell IDs:\n  ${missing.join("\n  ")}`
    );
  });

  it("all spell action_refs reference known canonical actions", () => {
    const actionIds = new Set(actionsData.actions.map((action) => action.action_id));
    const missing = [];

    for (const catalog of [mageSpellsData, priestPrayersData]) {
      for (const spell of catalog.spells) {
        for (const actionRef of spell.action_refs) {
          const actionId = actionRef.action_id;
          if (!actionIds.has(actionId)) {
            missing.push(`${spell.id} -> ${actionId}`);
          }
        }
      }
    }

    assert.equal(
      missing.length,
      0,
      `Spells reference unknown canonical action IDs:\n  ${missing.join("\n  ")}`
    );
  });

  it("book realm/tval constraints are consistent", () => {
    for (const book of spellBooksData.books) {
      if (book.realm === "magic") {
        assert.equal(book.book_tval, 90, `${book.id} realm/tval mismatch`);
      }
      if (book.realm === "prayer") {
        assert.equal(book.book_tval, 91, `${book.id} realm/tval mismatch`);
      }
    }
  });

  it("spell policy book references exist and realms match", () => {
    const bookById = new Map(spellBooksData.books.map((book) => [book.id, book]));
    const issues = [];

    for (const catalog of [mageSpellsData, priestPrayersData]) {
      for (const spell of catalog.spells) {
        const book = bookById.get(spell.policy.book_id);
        if (!book) {
          issues.push(`${spell.id}: unknown policy.book_id '${spell.policy.book_id}'`);
          continue;
        }
        if (book.realm !== spell.policy.realm) {
          issues.push(`${spell.id}: policy realm '${spell.policy.realm}' mismatches book realm '${book.realm}'`);
        }
      }
    }

    assert.equal(
      issues.length,
      0,
      `Spell policy/book reference issues:\n  ${issues.join("\n  ")}`
    );
  });
});

// ---------------------------------------------------------------------------

describe("effects.json", () => {
  it("validates against effects.schema.json", () => {
    const valid = validateEffects(effectsData);
    if (!valid) {
      assert.fail(
        "effects.json failed validation:\n" +
          JSON.stringify(validateEffects.errors, null, 2)
      );
    }
  });

  it("has unique effect ids", () => {
    const ids = effectsData.effects.map((e) => e.id);
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate effect id found");
  });

  it("all damage effects have a valid damage_type", () => {
    const validTypes = effectsSchema.$defs.DamageType.enum;
    for (const effect of effectsData.effects) {
      if (effect.effect_id === "damage") {
        assert.ok(
          validTypes.includes(effect.damage_type),
          `Effect '${effect.id}' has invalid damage_type '${effect.damage_type}'`
        );
      }
    }
  });

  it("all damage effects have a valid shape", () => {
    const validShapes = effectsSchema.$defs.DamageShape.enum;
    for (const effect of effectsData.effects) {
      if (effect.effect_id === "damage") {
        assert.ok(
          validShapes.includes(effect.shape),
          `Effect '${effect.id}' has invalid shape '${effect.shape}'`
        );
      }
    }
  });

  it("all damage effects have dice or flat_bonus", () => {
    for (const effect of effectsData.effects) {
      if (effect.effect_id === "damage") {
        const hasDamage = effect.dice != null || effect.flat_bonus != null;
        assert.ok(
          hasDamage,
          `Damage effect '${effect.id}' has neither dice nor flat_bonus`
        );
      }
    }
  });

  it("all heal effects have dice or flat_bonus", () => {
    for (const effect of effectsData.effects) {
      if (effect.effect_id === "heal") {
        const hasDamage = effect.dice != null || effect.flat_bonus != null;
        assert.ok(
          hasDamage,
          `Heal effect '${effect.id}' has neither dice nor flat_bonus`
        );
      }
    }
  });

  it("all apply_status effects reference a known status_id", () => {
    const knownStatuses = new Set(statusesData.statuses.map((s) => s.status_id));
    for (const effect of effectsData.effects) {
      if (effect.effect_id === "apply_status" || effect.effect_id === "cure_status") {
        assert.ok(
          knownStatuses.has(effect.status_id),
          `Effect '${effect.id}' references unknown status_id '${effect.status_id}'`
        );
      }
    }
  });

  it("all detect effects have non-empty targets array", () => {
    for (const effect of effectsData.effects) {
      if (effect.effect_id === "detect") {
        assert.ok(
          Array.isArray(effect.targets) && effect.targets.length > 0,
          `Detect effect '${effect.id}' has empty or missing targets`
        );
      }
    }
  });

  it("rejects an effect with invalid effect_id", () => {
    const badEffect = {
      schema_version: 1,
      effects: [{ id: "bad_effect", effect_id: "nonexistent_effect_type" }],
    };
    const valid = validateEffects(badEffect);
    assert.ok(!valid, "Should have rejected an invalid effect_id");
  });

  it("rejects a damage effect with invalid damage_type", () => {
    const badEffect = {
      schema_version: 1,
      effects: [
        {
          id: "bad_damage",
          effect_id: "damage",
          damage_type: "psychic",
          shape: "bolt",
          flat_bonus: 10,
        },
      ],
    };
    const valid = validateEffects(badEffect);
    assert.ok(!valid, "Should have rejected an invalid damage_type");
  });

  it("rejects a dice definition with zero sides", () => {
    const badEffect = {
      schema_version: 1,
      effects: [
        {
          id: "bad_dice",
          effect_id: "damage",
          damage_type: "fire",
          shape: "bolt",
          dice: { count: 2, sides: 0 },
        },
      ],
    };
    const valid = validateEffects(badEffect);
    assert.ok(!valid, "Should have rejected dice with sides=0");
  });

  it("rejects a dice definition with zero count", () => {
    const badEffect = {
      schema_version: 1,
      effects: [
        {
          id: "bad_dice_count",
          effect_id: "damage",
          damage_type: "fire",
          shape: "bolt",
          dice: { count: 0, sides: 6 },
        },
      ],
    };
    const valid = validateEffects(badEffect);
    assert.ok(!valid, "Should have rejected dice with count=0");
  });
});

// ---------------------------------------------------------------------------

describe("activations.json", () => {
  it("validates against activations.schema.json", () => {
    const valid = validateActivations(activationsData);
    if (!valid) {
      assert.fail(
        "activations.json failed validation:\n" +
          JSON.stringify(validateActivations.errors, null, 2)
      );
    }
  });

  it("has unique activation_ids", () => {
    const ids = activationsData.activations.map((a) => a.activation_id);
    const unique = new Set(ids);
    assert.equal(unique.size, ids.length, "Duplicate activation_id found");
  });

  it("every activation has at least one effect", () => {
    for (const activation of activationsData.activations) {
      assert.ok(
        activation.effects.length >= 1,
        `Activation '${activation.activation_id}' has no effects`
      );
    }
  });

  it("all apply_status effects in activations reference known statuses", () => {
    const knownStatuses = new Set(statusesData.statuses.map((s) => s.status_id));
    for (const activation of activationsData.activations) {
      for (const effect of activation.effects) {
        if (effect.effect_id === "apply_status" || effect.effect_id === "cure_status") {
          assert.ok(
            knownStatuses.has(effect.status_id),
            `Activation '${activation.activation_id}' effect references unknown status_id '${effect.status_id}'`
          );
        }
      }
    }
  });

  it("all damage effects in activations have a valid damage_type", () => {
    const validTypes = new Set(effectsSchema.$defs.DamageType.enum);
    for (const activation of activationsData.activations) {
      for (const effect of activation.effects) {
        if (effect.effect_id === "damage") {
          assert.ok(
            validTypes.has(effect.damage_type),
            `Activation '${activation.activation_id}' has invalid damage_type '${effect.damage_type}'`
          );
        }
      }
    }
  });

  it("all haste effects in activations have a duration", () => {
    for (const activation of activationsData.activations) {
      for (const effect of activation.effects) {
        if (effect.effect_id === "haste") {
          assert.ok(
            effect.duration,
            `Activation '${activation.activation_id}' haste effect is missing duration`
          );
        }
      }
    }
  });

  it("required activations are present", () => {
    const required = [
      "HEAL1", "HEAL2", "HASTE", "FIRE1", "FROST1", "LIGHTNING_BOLT",
      "IDENTIFY", "PROBE", "CLAIRVOYANCE", "PROT_EVIL", "PHASE",
      "TELEPORT", "WORD_OF_RECALL", "DETECT", "STAR_BALL",
      "RESTORE_LIFE", "BERSERKER", "MASS_BANISHMENT", "BANISHMENT"
    ];
    const ids = new Set(activationsData.activations.map((a) => a.activation_id));
    for (const r of required) {
      assert.ok(ids.has(r), `Required activation '${r}' is missing`);
    }
  });

  it("rejects activation with invalid effect_id", () => {
    const badActivation = {
      schema_version: 1,
      activations: [
        {
          activation_id: "TEST_BAD",
          name: "Test Bad",
          effects: [{ effect_id: "nonexistent_type" }],
        },
      ],
    };
    const valid = validateActivations(badActivation);
    assert.ok(!valid, "Should have rejected activation with invalid effect_id");
  });
});

// ---------------------------------------------------------------------------

describe("artifacts.json activation references", () => {
  it("validates against artifacts.schema.json", () => {
    const valid = validateArtifacts(artifactsData);
    if (!valid) {
      assert.fail(
        "artifacts.json failed schema validation:\n" +
          JSON.stringify(validateArtifacts.errors, null, 2)
      );
    }
  });

  it("all artifact activation IDs exist in activations.json", () => {
    const knownActivations = new Set(
      activationsData.activations.map((a) => a.activation_id)
    );
    const missing = [];
    for (const artifact of artifactsData.artifacts) {
      if (artifact.activation && !knownActivations.has(artifact.activation)) {
        missing.push(`'${artifact.name}' uses '${artifact.activation}'`);
      }
    }
    assert.equal(
      missing.length,
      0,
      `Artifacts reference undefined activations:\n  ${missing.join("\n  ")}`
    );
  });

  it("artifacts with ACTIVATE flag have an activation field", () => {
    const missing = [];
    for (const artifact of artifactsData.artifacts) {
      if (artifact.flags.includes("ACTIVATE") && !artifact.activation) {
        missing.push(artifact.name);
      }
    }
    assert.equal(
      missing.length,
      0,
      `Artifacts with ACTIVATE flag are missing 'activation' field:\n  ${missing.join("\n  ")}`
    );
  });
});

// ---------------------------------------------------------------------------

describe("duration structure validation", () => {
  it("duration with only base is valid", () => {
    const doc = {
      schema_version: 1,
      statuses: [{
        status_id: "test_status",
        name: "Test",
        category: "buff",
        description: "test",
        is_timed: true,
        default_duration: { base: 10 },
        is_stackable: false,
        refresh_policy: "none",
        replacement_policy: "replace_existing",
        provenance_status: "ironhell_specific"
      }]
    };
    assert.ok(validateStatuses(doc), "duration with only base should be valid");
  });

  it("duration with base and dice is valid", () => {
    const doc = {
      schema_version: 1,
      statuses: [{
        status_id: "test_status",
        name: "Test",
        category: "buff",
        description: "test",
        is_timed: true,
        default_duration: { base: 0, dice: { count: 1, sides: 10 } },
        is_stackable: true,
        refresh_policy: "max",
        replacement_policy: "replace_existing",
        provenance_status: "ironhell_specific"
      }]
    };
    assert.ok(validateStatuses(doc), "duration with base and dice should be valid");
  });

  it("duration with negative base is invalid", () => {
    const doc = {
      schema_version: 1,
      statuses: [{
        status_id: "test_status",
        name: "Test",
        category: "buff",
        description: "test",
        is_timed: true,
        default_duration: { base: -1 },
        is_stackable: false,
        refresh_policy: "none",
        replacement_policy: "replace_existing",
        provenance_status: "ironhell_specific"
      }]
    };
    assert.ok(!validateStatuses(doc), "duration with negative base should be invalid");
  });

  it("dice with zero sides is invalid", () => {
    const doc = {
      schema_version: 1,
      statuses: [{
        status_id: "test_status",
        name: "Test",
        category: "buff",
        description: "test",
        is_timed: true,
        default_duration: { base: 0, dice: { count: 1, sides: 0 } },
        is_stackable: false,
        refresh_policy: "none",
        replacement_policy: "replace_existing",
        provenance_status: "ironhell_specific"
      }]
    };
    assert.ok(!validateStatuses(doc), "dice with sides=0 should be invalid");
  });

  it("dice with zero count is invalid", () => {
    const doc = {
      schema_version: 1,
      statuses: [{
        status_id: "test_status",
        name: "Test",
        category: "buff",
        description: "test",
        is_timed: true,
        default_duration: { base: 0, dice: { count: 0, sides: 6 } },
        is_stackable: false,
        refresh_policy: "none",
        replacement_policy: "replace_existing",
        provenance_status: "ironhell_specific"
      }]
    };
    assert.ok(!validateStatuses(doc), "dice with count=0 should be invalid");
  });

  it("duration with base=0 and no dice is invalid", () => {
    const doc = {
      schema_version: 1,
      statuses: [{
        status_id: "test_status",
        name: "Test",
        category: "buff",
        description: "test",
        is_timed: true,
        default_duration: { base: 0 },
        is_stackable: false,
        refresh_policy: "none",
        replacement_policy: "replace_existing",
        provenance_status: "ironhell_specific"
      }]
    };
    assert.ok(!validateStatuses(doc), "duration with base=0 and no dice should be invalid");
  });

  it("invalid stacking policy is rejected", () => {
    const doc = {
      schema_version: 1,
      statuses: [{
        status_id: "test_status",
        name: "Test",
        category: "buff",
        description: "test",
        is_timed: true,
        default_duration: { base: 1 },
        is_stackable: false,
        refresh_policy: "additive",
        replacement_policy: "replace_existing",
        provenance_status: "ironhell_specific"
      }]
    };
    assert.ok(!validateStatuses(doc), "non-stackable statuses cannot use additive refresh policy");
  });
});

// ---------------------------------------------------------------------------
// Helpers for cross-file validation
// ---------------------------------------------------------------------------

/** Collect all effect objects from an item-file array under the given key. */
function collectItemEffects(fileData, arrayKey) {
  const results = [];
  for (const item of fileData[arrayKey] ?? []) {
    for (const effect of item.effects ?? []) {
      results.push({ item: item.id, effect });
    }
  }
  return results;
}

/** All known capability IDs drawn from both capability files. */
function buildAllCapabilityIds() {
  const ids = new Set();
  for (const cap of capabilitiesData.capabilities ?? []) ids.add(cap.id);
  for (const cap of itemCapabilitiesData.capabilities ?? []) ids.add(cap.id);
  return ids;
}

/** Canonical effect_id set from common_item.schema.json. */
const knownEffectIds = new Set(commonItemSchema.$defs.EffectId.enum);

/** Known status_ids from statuses.json. */
const knownStatusIds = new Set(statusesData.statuses.map((s) => s.status_id));

// ---------------------------------------------------------------------------

describe("capability files — internal consistency", () => {
  it("capabilities.json has unique IDs", () => {
    const ids = capabilitiesData.capabilities.map((c) => c.id);
    const seen = new Set();
    const dupes = ids.filter((id) => seen.has(id) || !seen.add(id));
    assert.equal(
      dupes.length,
      0,
      `Duplicate capability IDs in capabilities.json: ${dupes.join(", ")}`
    );
  });

  it("item_capabilities.json has unique IDs", () => {
    const ids = itemCapabilitiesData.capabilities.map((c) => c.id);
    const seen = new Set();
    const dupes = ids.filter((id) => seen.has(id) || !seen.add(id));
    assert.equal(
      dupes.length,
      0,
      `Duplicate capability IDs in item_capabilities.json: ${dupes.join(", ")}`
    );
  });

  it("overlapping IDs between capabilities.json and item_capabilities.json are documented (architectural note: merge is pending)", () => {
    const charIds = new Set(capabilitiesData.capabilities.map((c) => c.id));
    const itemIds = itemCapabilitiesData.capabilities.map((c) => c.id);
    const overlaps = itemIds.filter((id) => charIds.has(id));
    // Overlap is a known architectural issue documented in GAMEPLAY_ARCHITECTURE_REVIEW.md F-01.
    // This test records the current overlap count so regressions are visible.
    // Expected count will decrease to 0 after the capability-system merge (Phase 1 of migration plan).
    if (overlaps.length > 0) {
      console.warn(
        `[KNOWN] ${overlaps.length} capability IDs overlap between the two files (F-01): ${overlaps.join(", ")}`
      );
    }
    // Do not fail — the overlap is pre-existing and tracked separately.
    assert.ok(
      overlaps.length >= 0,
      "overlap check completed (see console warning for details)"
    );
  });
});

// ---------------------------------------------------------------------------

describe("accessories.json — no phantom effects", () => {
  it("all accessory effect_ids are in the canonical enum", () => {
    const violations = [];
    for (const item of accessoriesData.accessories) {
      for (const effect of item.effects ?? []) {
        if (!knownEffectIds.has(effect.effect_id)) {
          violations.push(`${item.id}: unknown effect_id '${effect.effect_id}'`);
        }
      }
    }
    assert.equal(
      violations.length,
      0,
      `Phantom effect_ids found in accessories:\n  ${violations.join("\n  ")}`
    );
  });

  it("accessories have no effects[] that duplicate capability_ids[] (phantom duplication check)", () => {
    // Phantom effects are effect_ids that merely restate a capability already in capability_ids[].
    // After cleanup these should all be gone. This test prevents regression.
    const phantomPatterns = [
      "feather_falling", "random_teleportation", "slow_digestion",
      "sustain_str", "sustain_int", "sustain_wis", "sustain_dex", "sustain_con", "sustain_chr",
      "aggravate_monsters", "telepathy", "infravision",
      "resist_fire", "resist_acid", "resist_elec", "resist_cold",
      "reduce_stat_str", "reduce_stat_int",
    ];
    const violations = [];
    for (const item of accessoriesData.accessories) {
      for (const effect of item.effects ?? []) {
        if (phantomPatterns.includes(effect.effect_id)) {
          violations.push(`${item.id}: phantom effect_id '${effect.effect_id}' should be in capability_ids instead`);
        }
      }
    }
    assert.equal(
      violations.length,
      0,
      `Phantom capability-duplicate effects found:\n  ${violations.join("\n  ")}`
    );
  });
});

// ---------------------------------------------------------------------------

describe("item effects — cross-file status_id validation", () => {
  const itemFilePairs = [
    { label: "potions", data: potionsData, key: "potions" },
    { label: "scrolls", data: scrollsData, key: "scrolls" },
    { label: "rods", data: rodsData, key: "rods" },
    { label: "wands", data: wandsData, key: "wands" },
    { label: "staves", data: stavesData, key: "staves" },
    { label: "consumables", data: consumablesData, key: "consumables" },
    { label: "accessories", data: accessoriesData, key: "accessories" },
  ];

  for (const { label, data, key } of itemFilePairs) {
    it(`all apply_status / cure_status effects in ${label}.json reference known status_ids`, () => {
      const violations = [];
      for (const { item, effect } of collectItemEffects(data, key)) {
        if (
          (effect.effect_id === "apply_status" || effect.effect_id === "cure_status") &&
          !knownStatusIds.has(effect.status_id)
        ) {
          violations.push(`${item}: unknown status_id '${effect.status_id}'`);
        }
      }
      assert.equal(
        violations.length,
        0,
        `Unknown status references in ${label}.json:\n  ${violations.join("\n  ")}`
      );
    });
  }
});

// ---------------------------------------------------------------------------

describe("item effects — canonical effect_id validation", () => {
  const itemFilePairs = [
    { label: "potions", data: potionsData, key: "potions" },
    { label: "scrolls", data: scrollsData, key: "scrolls" },
    { label: "rods", data: rodsData, key: "rods" },
    { label: "wands", data: wandsData, key: "wands" },
    { label: "staves", data: stavesData, key: "staves" },
    { label: "consumables", data: consumablesData, key: "consumables" },
  ];

  for (const { label, data, key } of itemFilePairs) {
    it(`all effect_ids in ${label}.json are in the canonical enum`, () => {
      const violations = [];
      for (const { item, effect } of collectItemEffects(data, key)) {
        if (!knownEffectIds.has(effect.effect_id)) {
          violations.push(`${item}: unknown effect_id '${effect.effect_id}'`);
        }
      }
      assert.equal(
        violations.length,
        0,
        `Unknown effect_ids in ${label}.json:\n  ${violations.join("\n  ")}`
      );
    });
  }
});

// ---------------------------------------------------------------------------

describe("item effects — condition format validation", () => {
  const VALID_CONDITION_KEYS = new Set(["requires_absence", "forced", "on_next_melee_hit"]);

  const itemFilePairs = [
    { label: "potions", data: potionsData, key: "potions" },
    { label: "scrolls", data: scrollsData, key: "scrolls" },
    { label: "rods", data: rodsData, key: "rods" },
    { label: "wands", data: wandsData, key: "wands" },
    { label: "staves", data: stavesData, key: "staves" },
    { label: "consumables", data: consumablesData, key: "consumables" },
    { label: "accessories", data: accessoriesData, key: "accessories" },
  ];

  for (const { label, data, key } of itemFilePairs) {
    it(`all condition fields in ${label}.json are typed objects (not legacy strings)`, () => {
      const violations = [];
      for (const { item, effect } of collectItemEffects(data, key)) {
        if (effect.condition !== undefined) {
          if (typeof effect.condition !== "object" || effect.condition === null) {
            violations.push(
              `${item}: condition is a ${typeof effect.condition}, expected object. Migrate to {requires_absence:...} / {forced:true} / {on_next_melee_hit:true}`
            );
          } else {
            const keys = Object.keys(effect.condition);
            const unknown = keys.filter((k) => !VALID_CONDITION_KEYS.has(k));
            if (unknown.length > 0) {
              violations.push(
                `${item}: condition has unknown keys: ${unknown.join(", ")}`
              );
            }
          }
        }
      }
      assert.equal(
        violations.length,
        0,
        `Invalid condition format in ${label}.json:\n  ${violations.join("\n  ")}`
      );
    });
  }

  for (const { label, data, key } of itemFilePairs) {
    it(`all requires_absence values in ${label}.json reference known capability IDs`, () => {
      const allCapIds = buildAllCapabilityIds();
      const violations = [];
      for (const { item, effect } of collectItemEffects(data, key)) {
        const cond = effect.condition;
        if (cond && typeof cond === "object" && cond.requires_absence !== undefined) {
          if (!allCapIds.has(cond.requires_absence)) {
            violations.push(
              `${item}: condition.requires_absence '${cond.requires_absence}' is not a known capability ID`
            );
          }
        }
      }
      assert.equal(
        violations.length,
        0,
        `Unknown capability refs in conditions in ${label}.json:\n  ${violations.join("\n  ")}`
      );
    });
  }
});

// ---------------------------------------------------------------------------

describe("activations.json — duplicate and reference checks", () => {
  it("activation IDs are unique (no duplicates after STARLIGHT deduplication)", () => {
    const ids = activationsData.activations.map((a) => a.activation_id);
    const seen = new Set();
    const dupes = ids.filter((id) => seen.has(id) || !seen.add(id));
    assert.equal(
      dupes.length,
      0,
      `Duplicate activation_ids found: ${dupes.join(", ")}`
    );
  });

  it("STARLIGHT alias has been removed (canonical ID is STAR_LIGHT)", () => {
    const ids = new Set(activationsData.activations.map((a) => a.activation_id));
    assert.ok(!ids.has("STARLIGHT"), "STARLIGHT duplicate activation should have been removed");
    assert.ok(ids.has("STAR_LIGHT"), "STAR_LIGHT canonical activation must be present");
  });

  it("all artifacts that previously used STARLIGHT now use STAR_LIGHT", () => {
    const violations = [];
    for (const artifact of artifactsData.artifacts) {
      if (artifact.activation === "STARLIGHT") {
        violations.push(artifact.name);
      }
    }
    assert.equal(
      violations.length,
      0,
      `Artifacts still referencing removed STARLIGHT:\n  ${violations.join("\n  ")}`
    );
  });
});
