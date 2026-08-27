import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

import Ajv from "ajv/dist/2020.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  return JSON.parse(readFileSync(resolve(root, relPath), "utf-8").replace(/^\uFEFF/, ""));
}

const commonSchema = loadJSON("data/schemas/items/common_item.schema.json");
const scrollsSchema = loadJSON("data/schemas/items/scrolls.schema.json");
const actionsData = loadJSON("data/definitions/actions.json");
const scrollsData = loadJSON("data/definitions/items/scrolls.json");

const ajv = new Ajv({ strict: false, allErrors: true, allowUnionTypes: true });
ajv.addSchema(commonSchema, "https://ironhell.local/schemas/items/common_item.schema.json");
ajv.addSchema(commonSchema, "common_item.schema.json");
const validateScrolls = ajv.compile(scrollsSchema);
const actionById = new Map(actionsData.actions.map((action) => [action.action_id, action]));

const expectedSvals = [
  ...Array.from({ length: 19 }, (_, sval) => sval),
  20, 21, 22,
  ...Array.from({ length: 7 }, (_, index) => index + 24),
  ...Array.from({ length: 8 }, (_, index) => index + 32),
  41, 42, 44, 45, 46, 47, 48, 49,
];

function assertStructuredAmount(value, label) {
  assert.ok(value && typeof value === "object", `${label} must be a structured amount`);
  assert.ok(value.kind || value.base !== undefined || value.dice, `${label} has no amount expression`);
}

function assertActionContracts(catalog) {
  for (const scroll of catalog.scrolls) {
    for (const actionRef of scroll.actions || []) {
      const action = actionById.get(actionRef.action_id);
      assert.ok(action, `${scroll.id} references unknown action '${actionRef.action_id}'`);
      assert.ok(action.allowed_source_families.includes("scroll"), `${actionRef.action_id} does not allow scroll sources`);

      const parameters = actionRef.parameters || {};
      const declared = new Map(action.parameter_contract.parameters.map((parameter) => [parameter.id, parameter]));
      for (const [parameterId, value] of Object.entries(parameters)) {
        const parameter = declared.get(parameterId);
        assert.ok(parameter, `${scroll.id}: undeclared parameter '${parameterId}'`);
        if (parameter.value_type === "structured_amount") assertStructuredAmount(value, `${scroll.id}.${parameterId}`);
        if (parameter.allowed_values) {
          const values = Array.isArray(value) ? value : [value];
          for (const item of values) assert.ok(parameter.allowed_values.includes(item), `${scroll.id}: invalid ${parameterId} value '${item}'`);
        }
      }
      for (const parameter of action.parameter_contract.parameters) {
        if (parameter.required) assert.notEqual(parameters[parameter.id], undefined, `${scroll.id}: missing required parameter '${parameter.id}'`);
      }
    }
  }
}

describe("MAngband 1.5.3 scroll parity", () => {
  it("passes startup schema validation", () => {
    assert.equal(validateScrolls(scrollsData), true, JSON.stringify(validateScrolls.errors, null, 2));
  });

  it("contains every canonical scroll sval exactly once", () => {
    const svals = scrollsData.scrolls.map((scroll) => scroll.sval);
    assert.equal(new Set(svals).size, svals.length, "Duplicate scroll sval found");
    assert.deepEqual(svals, expectedSvals);
  });

  it("maps every scroll to an existing scroll-capable action contract", () => {
    assertActionContracts(scrollsData);
  });

  it("defers Monster Confusion and preserves touch destruction semantics", () => {
    const confusion = scrollsData.scrolls.find((scroll) => scroll.sval === 36);
    assert.equal(confusion.actions[0].action_id, "ConfuseControl");
    assert.equal(confusion.actions[0].parameters.trigger_mode, "next_melee_hit");

    const destruction = scrollsData.scrolls.find((scroll) => scroll.sval === 39);
    assert.equal(destruction.actions[0].action_id, "AlterTerrain");
    assert.equal(destruction.actions[0].parameters.target_mode, "touch");
  });

  it("preserves dice, enchantment-attempt, and object-creation parity payloads", () => {
    for (const sval of [4, 5]) {
      const summon = scrollsData.scrolls.find((scroll) => scroll.sval === sval);
      assert.equal(summon.actions[0].parameters.amount.kind, "dice");
    }
    assert.deepEqual(
      scrollsData.scrolls.find((scroll) => scroll.sval === 20).actions[0].parameters.attempts,
      { base: 2, dice: { count: 1, sides: 3 } }
    );
    assert.deepEqual(
      scrollsData.scrolls.find((scroll) => scroll.sval === 47).actions[0].parameters.count,
      { base: 1, dice: { count: 1, sides: 2 } }
    );
  });
});
