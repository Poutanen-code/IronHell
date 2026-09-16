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

const schema = loadJson("data/schemas/monsters/monsters.schema.json");
const catalog = loadJson("data/definitions/monsters/monsters.json");
const validate = new Ajv2020({ strict: false, allErrors: true }).compile(schema);

test("all monsters use valid canonical hp_roll definitions", () => {
  assert.equal(catalog.monsters.length, 616);
  assert.equal(validate(catalog), true, JSON.stringify(validate.errors?.slice(0, 5)));

  for (const monster of catalog.monsters) {
    assert.deepEqual(Object.keys(monster.hp_roll), ["kind", "count", "sides"]);
    assert.equal(monster.hp_roll.kind, "dice");
    assert.ok(monster.hp_roll.count > 0);
    assert.ok(monster.hp_roll.sides > 0);
    assert.equal(monster.stats.hp, undefined);
    assert.equal(monster.hp_dice, undefined);
    assert.equal(monster.stats.hp_dice, undefined);
  }
});

const invalidCases = [
  ["missing hp_roll", (monster) => delete monster.hp_roll],
  ["non-positive count", (monster) => { monster.hp_roll.count = 0; }],
  ["non-positive sides", (monster) => { monster.hp_roll.sides = 0; }],
  ["legacy stats.hp", (monster) => { monster.stats.hp = 2; }],
  ["legacy hp_dice", (monster) => { monster.stats.hp_dice = "1d4"; }],
];

for (const [name, mutate] of invalidCases) {
  test(`monster schema rejects ${name}`, () => {
    const invalidCatalog = structuredClone(catalog);
    mutate(invalidCatalog.monsters[0]);
    assert.equal(validate(invalidCatalog), false);
  });
}