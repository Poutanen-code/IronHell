import test from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import Ajv2020 from "ajv/dist/2020.js";

const catalogPath = "data/definitions/monsters/monsters.json";
const catalog = JSON.parse(fs.readFileSync(catalogPath, "utf8"));
const schema = JSON.parse(
  fs.readFileSync("data/schemas/monsters/monsters.schema.json", "utf8")
);
const damageTypes = {
  HURT: "physical",
  FIRE: "fire",
  COLD: "cold",
  ACID: "acid",
  ELEC: "lightning",
  POISON: "poison"
};
const statusMap = {
  CONFUSE: "confused",
  BLIND: "blinded",
  PARALYZE: "paralyzed",
  TERRIFY: "afraid",
  HALLU: "hallucinating"
};
const statMap = {
  LOSE_STR: "strength",
  LOSE_INT: "intelligence",
  LOSE_WIS: "wisdom",
  LOSE_DEX: "dexterity",
  LOSE_CON: "constitution",
  LOSE_CHR: "charisma"
};
const experienceEffects = new Set(["EXP_10", "EXP_20", "EXP_40", "EXP_80"]);
const itemActions = {
  EAT_GOLD: ["StealGold"],
  EAT_ITEM: ["StealItem"],
  EAT_FOOD: ["StealItem"],
  EAT_LITE: ["ModifyLightSource"],
  UN_BONUS: ["ModifyItemPower", "remove_bonus"],
  UN_POWER: ["ModifyItemPower", "remove_power"]
};
const finalEffects = new Set(["LOSE_ALL", "SHATTER"]);

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
  assert.equal(attacks.length, 1640);

  let migrated = 0;
  for (const { attack } of attacks) {
    if (attack.actions) {
      migrated++;
      const damage = attack.actions[0];
      assert.equal(damage.action_id, "ApplyDamage");
      assert.equal(damage.parameters.target_mode, "target");
      assert.equal(damage.parameters.amount.kind, "dice");
      assert.ok(Number.isInteger(damage.parameters.amount.count));
      assert.ok(Number.isInteger(damage.parameters.amount.sides));
      if (attack.actions.length === 2 && attack.actions[1].action_id === "ApplyStatus") {
        assert.equal(attack.actions[1].action_id, "ApplyStatus");
        assert.ok(Object.values(statusMap).includes(attack.actions[1].parameters.status_id));
      }
      if (attack.actions.length === 2 && attack.actions[1].action_id === "ModifyAttribute") {
        assert.equal(attack.actions[1].action_id, "ModifyAttribute");
        assert.ok(Object.values(statMap).includes(attack.actions[1].parameters.stat_id));
      }
      if (attack.actions.length === 2 && attack.actions[1].action_id === "ModifyExperience") {
        assert.equal(attack.actions[1].action_id, "ModifyExperience");
        assert.ok(experienceEffects.has(attack.actions[1].parameters.amount.source_effect));
      }
      if (["StealGold", "StealItem", "ModifyLightSource", "ModifyItemPower"].includes(attack.actions[1]?.action_id)) {
        assert.equal(damage.action_id, "ApplyDamage");
        assert.equal(attack.actions[1].parameters.target_mode, "target");
        if (attack.actions[1].action_id === "ModifyItemPower") {
          assert.ok(["remove_bonus", "remove_power"].includes(attack.actions[1].parameters.operation));
        }
      }
    }
    assert.ok(!damageTypes[attack.effect]);
  }

  assert.equal(migrated, 1617);
});

test("out-of-scope legacy blows remain represented as legacy effects", () => {
  const remainingEffects = new Set(
    attacks.map(({ attack }) => attack.effect).filter(Boolean)
  );
  assert.ok(remainingEffects.has("SHATTER"));
  for (const effect of Object.keys(itemActions)) {
    assert.ok(!remainingEffects.has(effect));
  }
  for (const effect of [...Object.keys(statusMap), ...Object.keys(statMap), ...experienceEffects]) {
    assert.ok(!remainingEffects.has(effect));
  }
  for (const effect of Object.keys(itemActions)) {
    assert.ok(!remainingEffects.has(effect));
  }
  for (const effect of finalEffects) {
    assert.ok(!remainingEffects.has(effect));
  }
});

test("final legacy blow effects preserve their verified pipelines", () => {
  const finalAttacks = attacks.filter(({ attack }) => attack.actions?.some((action) => action.action_id === "Earthquake" || action.parameters?.stat_id === "strength"));
  assert.equal(finalAttacks.length, 7);
  const shatterAttacks = attacks.filter(({ attack }) => attack.actions?.some((action) => action.action_id === "Earthquake"));
  assert.equal(shatterAttacks.length, 6);
  for (const { attack } of shatterAttacks) {
    assert.equal(attack.actions[0].parameters.amount.kind, "runtime_formula");
    assert.equal(attack.actions[0].parameters.amount.source_effect, "SHATTER");
    assert.deepEqual(attack.actions[1], { action_id: "Earthquake", parameters: { radius: 8, center_mode: "self" } });
  }
  const loseAll = attacks.find(({ attack }) => attack.actions?.filter((action) => action.action_id === "ModifyAttribute").length === 6);
  assert.ok(loseAll);
  assert.deepEqual(loseAll.attack.actions.slice(1).map((action) => action.parameters.stat_id), ["strength", "intelligence", "wisdom", "dexterity", "constitution", "charisma"]);
});

test("remaining non-action entries are the audited empty placeholders", () => {
  const emptyAttacks = attacks.filter(({ attack }) => Object.keys(attack).length === 0);
  assert.equal(emptyAttacks.length, 16);
  assert.ok(emptyAttacks.every(({ attack }) => !attack.method && !attack.effect));
});
