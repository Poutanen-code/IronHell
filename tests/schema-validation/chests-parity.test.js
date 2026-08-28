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
const chestsSchema = loadJSON("data/schemas/items/chests.schema.json");
const actionsData = loadJSON("data/definitions/actions.json");
const chestsData = loadJSON("data/definitions/items/chests.json");
const trapsData = loadJSON("data/definitions/environment/traps.json");

const ajv = new Ajv({ strict: false, allErrors: true, allowUnionTypes: true });
ajv.addSchema(commonSchema, "https://ironhell.local/schemas/items/common_item.schema.json");
ajv.addSchema(commonSchema, "common_item.schema.json");
const validateChests = ajv.compile(chestsSchema);
const actionById = new Map(actionsData.actions.map((action) => [action.action_id, action]));
const trapById = new Map(trapsData.traps.map((trap) => [trap.id, trap]));

const expectedRewardCounts = new Map([
  ["ruined_chest", 0],
  ["small_wooden_chest", 2],
  ["small_iron_chest", 4],
  ["small_steel_chest", 6],
  ["large_wooden_chest", 2],
  ["large_iron_chest", 4],
  ["large_steel_chest", 6],
]);

function assertActionRef(actionRef) {
  const action = actionById.get(actionRef.action_id);
  assert.ok(action, `Unknown chest action '${actionRef.action_id}'`);
  assert.ok(action.allowed_source_families.includes("chest"), `${actionRef.action_id} does not allow chest sources`);
  const parameters = actionRef.parameters || {};
  const declared = new Set(action.parameter_contract.parameters.map((parameter) => parameter.id));
  for (const parameterId of Object.keys(parameters)) assert.ok(declared.has(parameterId), `${actionRef.action_id}: undeclared parameter '${parameterId}'`);
  for (const parameter of action.parameter_contract.parameters) {
    if (parameter.required) assert.notEqual(parameters[parameter.id], undefined, `${actionRef.action_id}: missing required parameter '${parameter.id}'`);
  }
}

describe("MAngband 1.5.3 chest parity", () => {
  it("passes chest startup schema validation", () => {
    assert.equal(validateChests(chestsData), true, JSON.stringify(validateChests.errors, null, 2));
  });

  it("preserves the seven source chest definitions and progression", () => {
    assert.equal(new Set(chestsData.chests.map((chest) => chest.id)).size, chestsData.chests.length);
    assert.deepEqual(chestsData.chests.map((chest) => chest.sval), [0, 1, 2, 3, 5, 6, 7]);
    assert.deepEqual(chestsData.chests.map((chest) => chest.allocation_table[0].depth), [75, 5, 25, 45, 15, 35, 55]);
    assert.deepEqual(chestsData.chests.map((chest) => chest.state_policy.opens_once), [true, true, true, true, true, true, true]);
  });

  it("uses exact MAngband reward counts and executable loot actions", () => {
    for (const chest of chestsData.chests) {
      const itemAction = chest.loot_actions.find((action) => action.action_id === "CreateItems");
      const goldAction = chest.loot_actions.find((action) => action.action_id === "CreateGold");
      assert.ok(itemAction || expectedRewardCounts.get(chest.id) === 0, `${chest.id} is missing CreateItems`);
      if (itemAction) assert.equal(itemAction.parameters.amount.value, expectedRewardCounts.get(chest.id));
      if (goldAction) assert.equal(goldAction.parameters.amount.kind, "chest_gold");
      for (const actionRef of chest.loot_actions) assertActionRef(actionRef);
    }
  });

  it("resolves every chest trap reference to a chest-compatible trap", () => {
    for (const chest of chestsData.chests) {
      for (const trapId of chest.trap_ids) {
        const trap = trapById.get(trapId);
        assert.ok(trap, `${chest.id} references unknown trap '${trapId}'`);
        assert.ok(trap.allowed_sources.includes("chest"), `${trapId} is not chest-compatible`);
      }
    }
  });

  it("defines the minimal one-time chest state policy", () => {
    for (const chest of chestsData.chests) {
      assert.equal(chest.state_policy.opens_once, true);
      assert.equal(chest.state_policy.empties_on_open, true);
      assert.equal(chest.state_policy.initial_trapped, chest.trap_chance > 0);
      assert.equal(chest.state_policy.initial_locked, chest.lock_difficulty > 0);
    }
  });
});
