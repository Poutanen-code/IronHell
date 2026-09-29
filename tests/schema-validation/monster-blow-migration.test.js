import test from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import Ajv2020 from "ajv/dist/2020.js";

const catalogPath = "data/definitions/monsters/monsters.json";
const catalog = JSON.parse(fs.readFileSync(catalogPath, "utf8").replace(/^\uFEFF/, ""));
const schema = JSON.parse(fs.readFileSync("data/schemas/monsters/monsters.schema.json", "utf8").replace(/^\uFEFF/, ""));
const actions = JSON.parse(fs.readFileSync("data/definitions/actions.json", "utf8").replace(/^\uFEFF/, ""));
const statuses = JSON.parse(fs.readFileSync("data/definitions/statuses.json", "utf8").replace(/^\uFEFF/, ""));
const actionIds = new Set(actions.actions.map((action) => action.action_id));
const statusIds = new Set(statuses.statuses.map((status) => status.status_id));
const statIds = new Set(["strength", "intelligence", "wisdom", "dexterity", "constitution", "charisma"]);
const experienceEffects = new Set(["EXP_10", "EXP_20", "EXP_40", "EXP_80"]);

const attacks = catalog.monsters.flatMap((monster) =>
  (monster.attacks ?? []).map((attack) => ({ monster: monster.id, attack }))
);

test("monster blow entries validate against the monster blow schema", () => {
  const ajv = new Ajv2020({ allErrors: true });
  const validate = ajv.compile(schema.$defs.monster_blow);
  const populatedAttacks = attacks.filter(({ attack }) => Object.keys(attack).length > 0);
  const valid = populatedAttacks.every(({ attack }) => validate(attack));
  assert.equal(valid, true, JSON.stringify(validate.errors?.slice(0, 5)));
});

test("monster damage blows preserve hybrid effects in canonical actions", () => {
  assert.equal(catalog.monsters.length, 616);
  assert.equal(attacks.length, 1624);

  for (const { attack } of attacks) {
    assert.ok(Array.isArray(attack.actions) && attack.actions.length > 0);
    assert.equal(Object.hasOwn(attack, "effect"), false);
    for (const action of attack.actions) {
      assert.ok(actionIds.has(action.action_id), `unknown attack action '${action.action_id}'`);
    }
  }
});

test("hybrid damage, status, attribute, experience, and item effects use canonical actions", () => {
  for (const { monster, attack } of attacks) {
    const damage = attack.actions.find((action) => action.action_id === "ApplyDamage");
    if (damage) {
      assert.equal(damage.parameters.target_mode, "target", `${monster} damage target`);
      assert.equal(damage.parameters.amount.kind, "dice", `${monster} damage amount`);
      assert.ok(Number.isInteger(damage.parameters.amount.count), `${monster} damage count`);
      assert.ok(Number.isInteger(damage.parameters.amount.sides), `${monster} damage sides`);
    }

    for (const action of attack.actions) {
      if (action.action_id === "ApplyStatus") assert.ok(statusIds.has(action.parameters.status_id), `${monster} status reference`);
      if (action.action_id === "ModifyAttribute") assert.ok(statIds.has(action.parameters.stat_id), `${monster} stat reference`);
      if (action.action_id === "ModifyExperience") assert.ok(experienceEffects.has(action.parameters.amount.source_effect), `${monster} experience source`);
      if (["StealGold", "StealItem", "ModifyLightSource", "ModifyItemPower"].includes(action.action_id)) {
        assert.equal(action.parameters.target_mode, "target", `${monster} item action target`);
      }
      if (action.action_id === "ModifyItemPower") assert.ok(["remove_bonus", "remove_power"].includes(action.parameters.operation));
    }
  }
});

test("Morgoth's all-stat drain preserves its six explicit attribute actions", () => {
  const loseAll = attacks.find(({ attack }) => attack.actions?.filter((action) => action.action_id === "ModifyAttribute").length === 6);
  assert.ok(loseAll);
  assert.deepEqual(loseAll.attack.actions.slice(1).map((action) => action.parameters.stat_id), ["strength", "intelligence", "wisdom", "dexterity", "constitution", "charisma"]);
});

test("all monster blow entries are populated action definitions", () => {
  const emptyAttacks = attacks.filter(({ attack }) => Object.keys(attack).length === 0);
  assert.deepEqual(emptyAttacks, []);
});
