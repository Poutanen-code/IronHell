import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";
import assert from "node:assert/strict";

import Ajv2020 from "ajv/dist/2020.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJson(relativePath) {
  return JSON.parse(readFileSync(resolve(root, relativePath), "utf8").replace(/^\uFEFF/, ""));
}

const monsterSchema = loadJson("data/schemas/monsters/monsters.schema.json");
const capabilitySchema = loadJson("data/schemas/monster_capabilities.schema.json");
const monsters = loadJson("data/definitions/monsters/monsters.json");
const capabilityCatalog = loadJson("data/definitions/monster_capabilities.json");
const ajv = new Ajv2020({ strict: false, allErrors: true });
const validateMonsters = ajv.compile(monsterSchema);
const validateCapabilities = ajv.compile(capabilitySchema);

test("migrated monster AI preserves random movement and stupid flags", () => {
  assert.equal(validateMonsters(monsters), true, JSON.stringify(validateMonsters.errors?.slice(0, 5)));
  const byId = new Map(monsters.monsters.map((monster) => [monster.id, monster]));

  assert.deepEqual(byId.get("filthy_street_urchin").ai, { behavior: "wanderer", random_move_chance: 25 });
  assert.equal(byId.get("singing_happy_drunk").ai.random_move_chance, 50);
  assert.equal(byId.get("white_icky_thing").ai.random_move_chance, 75);
  assert.equal(byId.get("grey_mold").ai.stupid, true);
});

test("all monster capability references resolve uniquely", () => {
  assert.equal(validateCapabilities(capabilityCatalog), true, JSON.stringify(validateCapabilities.errors));
  const capabilityIds = capabilityCatalog.capabilities.map((capability) => capability.id);
  assert.equal(new Set(capabilityIds).size, capabilityIds.length);

  const knownCapabilities = new Set(capabilityIds);
  for (const monster of monsters.monsters) {
    assert.equal(new Set(monster.capabilities).size, monster.capabilities.length, `${monster.id} has duplicate capabilities`);
    for (const capabilityId of monster.capabilities) {
      assert.ok(knownCapabilities.has(capabilityId), `${monster.id} references unknown capability ${capabilityId}`);
    }
  }
});

test("monster schema rejects invalid AI and duplicate capabilities", () => {
  const missingBehavior = structuredClone(monsters);
  delete missingBehavior.monsters[0].ai.behavior;
  assert.equal(validateMonsters(missingBehavior), false);

  for (const chance of [-1, 101]) {
    const invalidChance = structuredClone(monsters);
    invalidChance.monsters[0].ai.random_move_chance = chance;
    assert.equal(validateMonsters(invalidChance), false);
  }

  const duplicateCapability = structuredClone(monsters);
  duplicateCapability.monsters[0].capabilities.push(duplicateCapability.monsters[0].capabilities[0]);
  assert.equal(validateMonsters(duplicateCapability), false);
});

test("monster schema rejects migrated legacy fields", () => {
  const legacyBehavior = structuredClone(monsters);
  legacyBehavior.monsters[0].ai_behavior = "wanderer";
  assert.equal(validateMonsters(legacyBehavior), false);

  for (const legacyFlag of ["rand25", "rand50", "stupid", "open_door", "bash_door", "take_item", "take_gold", "pass_wall", "kill_wall", "move_body", "kill_body", "multiply", "regenerate", "invisible"]) {
    const legacyMonster = structuredClone(monsters);
    legacyMonster.monsters[0].flags[legacyFlag] = true;
    assert.equal(validateMonsters(legacyMonster), false, `${legacyFlag} should be rejected`);
  }
});