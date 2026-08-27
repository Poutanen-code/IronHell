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
const wandsSchema = loadJSON("data/schemas/items/wands.schema.json");
const actionsData = loadJSON("data/definitions/actions.json");
const wandsData = loadJSON("data/definitions/items/wands.json");

const ajv = new Ajv({ strict: false, allErrors: true, allowUnionTypes: true });
ajv.addSchema(commonSchema, "https://ironhell.local/schemas/items/common_item.schema.json");
ajv.addSchema(commonSchema, "common_item.schema.json");
const validateWands = ajv.compile(wandsSchema);
const actionById = new Map(actionsData.actions.map((action) => [action.action_id, action]));

const expectedWands = [
  [0, "Heal Monster", "HealHP"],
  [1, "Haste Monster", "ModifyMonsterSpeed"],
  [2, "Clone Monster", "CloneMonster"],
  [3, "Teleport Other", "TeleportTarget"],
  [4, "Disarming", "AlterTerrain"],
  [5, "Trap/Door Destruction", "AlterTerrain"],
  [6, "Stone to Mud", "AlterTerrain"],
  [7, "Light", "BeamDamage"],
  [8, "Sleep Monster", "SleepControl"],
  [9, "Slow Monster", "ModifyMonsterSpeed"],
  [10, "Confuse Monster", "ConfuseControl"],
  [11, "Fear Monster", "FearControl"],
  [12, "Drain Life", "DrainLife"],
  [13, "Polymorph", "TransformEntity"],
  [14, "Stinking Cloud", "BallDamage"],
  [15, "Magic Missile", "BoltDamage"],
  [16, "Acid Bolt", "BoltDamage"],
  [17, "Lightning Bolt", "BoltDamage"],
  [18, "Fire Bolt", "BoltDamage"],
  [19, "Cold Bolt", "BoltDamage"],
  [20, "Acid Ball", "BallDamage"],
  [21, "Lightning Ball", "BallDamage"],
  [22, "Fire Ball", "BallDamage"],
  [23, "Cold Ball", "BallDamage"],
  [24, "Wonder", "RandomActionSelection"],
  [25, "Annihilation", "DrainLife"],
  [26, "Dragon Fire", "BallDamage"],
  [27, "Dragon Cold", "BallDamage"],
  [28, "Dragon Breath", "RandomActionSelection"],
];

function assertActionContracts(catalog) {
  for (const wand of catalog.wands) {
    for (const actionRef of wand.actions || []) {
      const action = actionById.get(actionRef.action_id);
      assert.ok(action, `${wand.id} references unknown action '${actionRef.action_id}'`);
      assert.ok(
        action.allowed_source_families.includes("wand"),
        `${actionRef.action_id} does not allow wand sources`
      );

      const parameters = actionRef.parameters || {};
      const declared = new Map(
        action.parameter_contract.parameters.map((parameter) => [parameter.id, parameter])
      );
      for (const parameterId of Object.keys(parameters)) {
        assert.ok(declared.has(parameterId), `${wand.id}: undeclared parameter '${parameterId}'`);
        const parameter = declared.get(parameterId);
        if (parameter.allowed_values) {
          const values = Array.isArray(parameters[parameterId])
            ? parameters[parameterId]
            : [parameters[parameterId]];
          for (const value of values) {
            assert.ok(
              parameter.allowed_values.includes(value),
              `${wand.id}: invalid ${parameterId} value '${value}'`
            );
          }
        }
      }
      for (const parameter of action.parameter_contract.parameters) {
        if (parameter.required) {
          assert.notEqual(
            parameters[parameter.id],
            undefined,
            `${wand.id}: missing required parameter '${parameter.id}'`
          );
        }
      }
    }
  }
}

describe("MAngband 1.5.3 wand parity", () => {
  it("passes startup schema validation", () => {
    assert.equal(validateWands(wandsData), true, JSON.stringify(validateWands.errors, null, 2));
  });

  it("contains exactly the canonical svals and names", () => {
    assert.deepEqual(
      wandsData.wands.map((wand) => [wand.sval, wand.name]),
      expectedWands.map(([sval, name]) => [sval, name])
    );
  });

  it("maps every wand to a valid wand-capable action contract", () => {
    assert.deepEqual(
      wandsData.wands.map((wand) => [wand.sval, wand.actions[0].action_id]),
      expectedWands.map(([sval, , actionId]) => [sval, actionId])
    );
    assertActionContracts(wandsData);
  });

  it("rejects an invalid action reference", () => {
    const invalidCatalog = structuredClone(wandsData);
    invalidCatalog.wands[0].actions[0].action_id = "MissingWandAction";
    assert.throws(
      () => assertActionContracts(invalidCatalog),
      /references unknown action 'MissingWandAction'/
    );
  });
});
