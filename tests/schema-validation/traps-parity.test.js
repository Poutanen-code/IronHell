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

const commonSchema = loadJSON("data/schemas/common.schema.json");
const trapsSchema = loadJSON("data/schemas/environment/traps.schema.json");
const actionsData = loadJSON("data/definitions/actions.json");
const trapsData = loadJSON("data/definitions/environment/traps.json");

const ajv = new Ajv({ strict: false, allErrors: true, allowUnionTypes: true });
ajv.addSchema(commonSchema, "https://ironhell.local/schemas/common.schema.json");
ajv.addSchema(commonSchema, "common.schema.json");
const validateTraps = ajv.compile(trapsSchema);
const actionById = new Map(actionsData.actions.map((action) => [action.action_id, action]));

const expectedMangbandTrapIds = [
  "trap_door", "pit", "spiked_pit", "poison_pit",
  "summon_rune", "teleport_rune", "fire_spot", "acid_spot", "dart_slow",
  "dart_strength", "dart_dexterity", "dart_constitution", "gas_blind",
  "gas_confuse", "gas_poison", "gas_sleep",
];

const expectedCustomTrapIds = ["invisible_trap"];

const canonicalTrapActions = {
  trap_door: ["ApplyDamage", "RecallOrLevelShift"],
  pit: ["ApplyDamage"],
  spiked_pit: ["ApplyDamage", "ApplyStatus"],
  poison_pit: ["ApplyDamage", "ApplyStatus"],
  summon_rune: ["SummonEntities"],
  teleport_rune: ["TeleportSelf"],
  fire_spot: ["ApplyDamage"],
  acid_spot: ["ApplyDamage"],
  dart_slow: ["ApplyDamage", "ModifyPlayerSpeed"],
  dart_strength: ["ApplyDamage", "ModifyAttribute"],
  dart_dexterity: ["ApplyDamage", "ModifyAttribute"],
  dart_constitution: ["ApplyDamage", "ModifyAttribute"],
  gas_blind: ["ApplyStatus"],
  gas_confuse: ["ApplyStatus"],
  gas_poison: ["ApplyStatus"],
  gas_sleep: ["ParalyzeControl"],
};

function assertActionContract(actionId) {
  const action = actionById.get(actionId);
  assert.ok(action, `Unknown canonical trap action '${actionId}'`);
  assert.ok(action.allowed_source_families.includes("trap"), `${actionId} does not allow trap sources`);
  assert.equal(action.parameter_contract.closed, true, `${actionId} must have a closed contract`);
  const parameterIds = action.parameter_contract.parameters.map((parameter) => parameter.id);
  assert.equal(new Set(parameterIds).size, parameterIds.length, `${actionId} has duplicate parameter IDs`);
}

function assertTrapActions(trap) {
  if (trap.provenance_status === "ironhell_specific") {
    assert.deepEqual(trap.actions || [], [], `${trap.id} must remain a generic non-executable trap`);
    return;
  }

  assert.ok(Array.isArray(trap.actions) && trap.actions.length > 0, `${trap.id} must define executable actions`);
  for (const actionRef of trap.actions) {
    const action = actionById.get(actionRef.action_id);
    assert.ok(action, `${trap.id} references unknown action '${actionRef.action_id}'`);
    assert.ok(action.allowed_source_families.includes("trap"), `${actionRef.action_id} does not allow trap sources`);

    const parameters = actionRef.parameters || {};
    const declared = new Map(action.parameter_contract.parameters.map((parameter) => [parameter.id, parameter]));
    for (const [parameterId, value] of Object.entries(parameters)) {
      const parameter = declared.get(parameterId);
      assert.ok(parameter, `${trap.id}: undeclared parameter '${parameterId}'`);
      if (parameter.allowed_values) {
        const values = Array.isArray(value) ? value : [value];
        for (const item of values) assert.ok(parameter.allowed_values.includes(item), `${trap.id}: invalid ${parameterId} value '${item}'`);
      }
    }
    for (const parameter of action.parameter_contract.parameters) {
      if (parameter.required) assert.notEqual(parameters[parameter.id], undefined, `${trap.id}: missing required parameter '${parameter.id}'`);
    }
  }
}

describe("MAngband 1.5.3 trap parity", () => {
  it("passes startup schema validation", () => {
    assert.equal(validateTraps(trapsData), true, JSON.stringify(validateTraps.errors, null, 2));
  });

  it("contains every dungeon trap exactly once", () => {
    const ids = trapsData.traps.map((trap) => trap.id);
    assert.equal(new Set(ids).size, ids.length, "Duplicate trap id found");
    assert.deepEqual(ids.filter((id) => expectedMangbandTrapIds.includes(id)), expectedMangbandTrapIds);
    assert.deepEqual(ids.filter((id) => expectedCustomTrapIds.includes(id)), expectedCustomTrapIds);
    assert.deepEqual(
      trapsData.traps.filter((trap) => expectedMangbandTrapIds.includes(trap.id)).map((trap) => trap.source_feature_id),
      expectedMangbandTrapIds
    );
  });

  it("maps every trap to existing canonical trap-capable actions", () => {
    for (const actionId of new Set(Object.values(canonicalTrapActions).flat())) assertActionContract(actionId);
    for (const trap of trapsData.traps) assertTrapActions(trap);
  });

  it("keeps custom and chest integration scope explicit", () => {
    assert.deepEqual(
      trapsData.traps.filter((trap) => trap.provenance_status === "ironhell_specific").map((trap) => trap.id),
      expectedCustomTrapIds
    );
    for (const trap of trapsData.traps) assert.deepEqual(trap.allowed_sources, ["floor", "chest"]);
    assert.ok(trapsData.traps.some((trap) => trap.effect_tags.includes("fall_damage_2d6")));
    assert.ok(trapsData.traps.some((trap) => trap.effect_tags.includes("paralysis_status_5_plus_1d10")));
  });
});
