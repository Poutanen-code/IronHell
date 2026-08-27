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
const stavesSchema = loadJSON("data/schemas/items/staves.schema.json");
const actionsData = loadJSON("data/definitions/actions.json");
const stavesData = loadJSON("data/definitions/items/staves.json");

const ajv = new Ajv({ strict: false, allErrors: true, allowUnionTypes: true });
ajv.addSchema(commonSchema, "https://ironhell.local/schemas/items/common_item.schema.json");
ajv.addSchema(commonSchema, "common_item.schema.json");
const validateStaves = ajv.compile(stavesSchema);
const actionById = new Map(actionsData.actions.map((action) => [action.action_id, action]));

const expectedStaffNames = [
  "Darkness", "Slowness", "Haste Monsters", "Summoning", "Teleportation",
  "Perception", "Remove Curse", "Starlight", "Light", "Enlightenment",
  "Treasure Location", "Object Location", "Trap Location", "Door/Stair Location",
  "Detect Invisible", "Detect Evil", "Cure Light Wounds", "Curing", "Healing",
  "The Magi", "Sleep Monsters", "Slow Monsters", "Speed", "Probing",
  "Dispel Evil", "Power", "Holiness", "Banishment", "Earthquakes", "Destruction",
];

function assertActionContracts(catalog) {
  for (const staff of catalog.staves) {
    for (const actionRef of staff.actions || []) {
      const action = actionById.get(actionRef.action_id);
      assert.ok(action, `${staff.id} references unknown action '${actionRef.action_id}'`);
      assert.ok(action.allowed_source_families.includes("staff"), `${actionRef.action_id} does not allow staff sources`);

      const parameters = actionRef.parameters || {};
      const declared = new Map(action.parameter_contract.parameters.map((parameter) => [parameter.id, parameter]));
      for (const parameterId of Object.keys(parameters)) {
        assert.ok(declared.has(parameterId), `${staff.id}: undeclared parameter '${parameterId}'`);
        const parameter = declared.get(parameterId);
        if (parameter.allowed_values) {
          const values = Array.isArray(parameters[parameterId]) ? parameters[parameterId] : [parameters[parameterId]];
          for (const value of values) {
            assert.ok(parameter.allowed_values.includes(value), `${staff.id}: invalid ${parameterId} value '${value}'`);
          }
        }
      }
      for (const parameter of action.parameter_contract.parameters) {
        if (parameter.required) {
          assert.notEqual(parameters[parameter.id], undefined, `${staff.id}: missing required parameter '${parameter.id}'`);
        }
      }
    }
  }
}

describe("MAngband 1.5.3 staff parity", () => {
  it("passes startup schema validation", () => {
    assert.equal(validateStaves(stavesData), true, JSON.stringify(validateStaves.errors, null, 2));
  });

  it("contains every canonical staff sval exactly once", () => {
    const svals = stavesData.staves.map((staff) => staff.sval);
    assert.equal(stavesData.staves.length, 30);
    assert.deepEqual(svals, Array.from({ length: 30 }, (_, sval) => sval));
    assert.deepEqual(stavesData.staves.map((staff) => staff.name), expectedStaffNames);
  });

  it("maps every staff to a valid staff-capable action contract", () => {
    assertActionContracts(stavesData);
  });

  it("preserves the corrected Slowness and Healing semantics", () => {
    const slowness = stavesData.staves.find((staff) => staff.sval === 1);
    assert.equal(slowness.actions[0].action_id, "ModifyPlayerSpeed");
    assert.equal(slowness.actions[0].parameters.target_mode, "self");

    const healing = stavesData.staves.find((staff) => staff.sval === 18);
    assert.equal(healing.actions[0].action_id, "HealHP");
    assert.deepEqual(healing.actions[0].parameters.amount, { kind: "flat", value: 300 });
    assert.equal(healing.actions[0].parameters.cut_effect, "clear");
    assert.equal(healing.actions[1].action_id, "CureStatus");
    assert.deepEqual(healing.actions[1].parameters.status_ids, ["stunned"]);
  });

  it("rejects an invalid action reference", () => {
    const invalidCatalog = structuredClone(stavesData);
    invalidCatalog.staves[0].actions[0].action_id = "MissingStaffAction";
    assert.throws(() => assertActionContracts(invalidCatalog), /references unknown action 'MissingStaffAction'/);
  });
});
