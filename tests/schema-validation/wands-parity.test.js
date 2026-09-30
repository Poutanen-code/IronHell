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
  ["Heal Monster", "HealHP"], ["Haste Monster", "ModifyMonsterSpeed"], ["Clone Monster", "CloneMonster"], ["Teleport Other", "TeleportTarget"], ["Disarming", "AlterTerrain"], ["Trap/Door Destruction", "AlterTerrain"], ["Stone to Mud", "AlterTerrain"], ["Light", "BeamDamage"], ["Sleep Monster", "SleepControl"], ["Slow Monster", "ModifyMonsterSpeed"], ["Confuse Monster", "ConfuseControl"], ["Fear Monster", "FearControl"], ["Drain Life", "DrainLife"], ["Polymorph", "TransformEntity"], ["Stinking Cloud", "BallDamage"], ["Magic Missile", "BoltDamage"], ["Acid Bolt", "BoltDamage"], ["Lightning Bolt", "BoltDamage"], ["Fire Bolt", "BoltDamage"], ["Cold Bolt", "BoltDamage"], ["Acid Ball", "BallDamage"], ["Lightning Ball", "BallDamage"], ["Fire Ball", "BallDamage"], ["Cold Ball", "BallDamage"], ["Wonder", "RandomActionSelection"], ["Annihilation", "DrainLife"], ["Dragon Fire", "BallDamage"], ["Dragon Cold", "BallDamage"], ["Dragon Breath", "RandomActionSelection"],
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

  it("contains exactly the canonical stable names", () => {
    assert.deepEqual(
      wandsData.wands.map((wand) => wand.name),
      expectedWands.map(([name]) => name)
    );
  });

  it("maps every wand to a valid wand-capable action contract", () => {
    assert.deepEqual(
      wandsData.wands.map((wand) => [wand.name, wand.actions[0].action_id]),
      expectedWands
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
