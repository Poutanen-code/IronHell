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

const monsters = loadJson("data/definitions/monsters/monsters.json");
const lootCatalog = loadJson("data/definitions/monsters/monster_loot.json");
const lootSchema = loadJson("data/schemas/monsters/monster_loot.schema.json");
const ajv = new Ajv2020({ strict: false, allErrors: true });
const validateLoot = ajv.compile(lootSchema);

function profile(id) {
  const found = lootCatalog.loot_profiles.find((entry) => entry.id === id);
  assert.ok(found, `missing loot profile ${id}`);
  return found;
}

function monster(id) {
  const found = monsters.monsters.find((entry) => entry.id === id);
  assert.ok(found, `missing monster ${id}`);
  return found;
}

test("monster loot catalog validates", () => {
  assert.equal(validateLoot(lootCatalog), true, JSON.stringify(validateLoot.errors));
});

test("all monster loot profile references resolve", () => {
  const profileIds = new Set(lootCatalog.loot_profiles.map((entry) => entry.id));
  assert.equal(profileIds.size, lootCatalog.loot_profiles.length, "loot profile ids must be unique");

  for (const entry of monsters.monsters) {
    assert.equal(typeof entry.loot_profile, "string", `${entry.id} is missing loot_profile`);
    assert.ok(profileIds.has(entry.loot_profile), `${entry.id} references unknown loot profile ${entry.loot_profile}`);
  }
});

test("equivalent Xd2 legacy rules are combined in reusable profiles", () => {
  assert.deepEqual(profile("items_9d2").quantity, { kind: "dice", count: 9, sides: 2 });
  assert.equal(monster("ancient_multi_hued_dragon").loot_profile, "items_9d2");

  assert.deepEqual(profile("sauron").quantity, { kind: "dice", count: 9, sides: 2 });
  assert.deepEqual(profile("sauron").generation_rules, ["good", "great"]);
  assert.equal(monster("sauron_the_sorcerer").loot_profile, "sauron");
});

test("bonus drops remain independent and Morgoth uses a special reward", () => {
  assert.deepEqual(profile("smeagol").bonus_drop_rules, [{ chance: 90, drops: 1 }]);
  assert.deepEqual(profile("smeagol").generation_rules, ["good", "great"]);
  assert.equal(monster("smeagol").loot_profile, "smeagol");

  assert.deepEqual(profile("morgoth").quantity, { kind: "dice", count: 10, sides: 2 });
  assert.deepEqual(profile("morgoth").generation_rules, ["good", "great"]);
  assert.deepEqual(profile("morgoth").special_rewards, ["morgoth_victory"]);
  assert.equal(monster("morgoth_lord_of_darkness").loot_profile, "morgoth");
});
