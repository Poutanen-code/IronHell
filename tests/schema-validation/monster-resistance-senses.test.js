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

const commonSchema = loadJson("data/schemas/common.schema.json");
const resistanceSchema = loadJson("data/schemas/resistances.schema.json");
const monsterSchema = loadJson("data/schemas/monsters/monsters.schema.json");
const resistanceCatalog = loadJson("data/definitions/resistances.json");
const monsters = loadJson("data/definitions/monsters/monsters.json");
const ajv = new Ajv2020({ strict: false, allErrors: true });
ajv.addSchema(commonSchema);
const validateResistances = ajv.compile(resistanceSchema);
const validateResistanceList = ajv.compile(monsterSchema.$defs.monster.properties.resistances);
const validateSenses = ajv.compile(monsterSchema.$defs.monster_senses);
const validateFlags = ajv.compile(monsterSchema.$defs.monster_flags);

test("canonical resistance catalog contains parity-confirmed status immunities", () => {
  assert.equal(validateResistances(resistanceCatalog), true, JSON.stringify(validateResistances.errors?.slice(0, 5)));
  const byId = new Map(resistanceCatalog.resistances.map((resistance) => [resistance.id, resistance]));

  for (const [id, channel] of [["imm_sleep", "sleep"], ["imm_fear", "fear"], ["imm_confu", "confu"]]) {
    assert.equal(byId.get(id)?.semantic_kind, "immunity");
    assert.equal(byId.get(id)?.target_scope, "bearer");
    assert.equal(byId.get(id)?.channel, channel);
  }
});

test("all migrated monster resistance references resolve uniquely", () => {
  const knownIds = new Set(resistanceCatalog.resistances.map((resistance) => resistance.id));

  for (const monster of monsters.monsters) {
    assert.equal(validateResistanceList(monster.resistances), true, `${monster.id}: ${JSON.stringify(validateResistanceList.errors)}`);
    assert.equal(new Set(monster.resistances).size, monster.resistances.length, `${monster.id} has duplicate resistances`);
    for (const resistanceId of monster.resistances) {
      assert.ok(knownIds.has(resistanceId), `${monster.id} references unknown resistance ${resistanceId}`);
    }
  }
});

test("MAngband NO_SLEEP NO_FEAR and NO_CONF map to immunities", () => {
  const byId = new Map(monsters.monsters.map((monster) => [monster.id, monster]));
  assert.deepEqual(byId.get("farmer_maggot").resistances, ["imm_sleep", "imm_confu"]);
  assert.deepEqual(byId.get("grey_mold").resistances, ["imm_pois", "imm_sleep", "imm_fear", "imm_confu"]);
});

test("migrated senses preserve alertness and telepathy profiles", () => {
  const byId = new Map(monsters.monsters.map((monster) => [monster.id, monster]));
  for (const monster of monsters.monsters) {
    assert.equal(validateSenses(monster.senses), true, `${monster.id}: ${JSON.stringify(validateSenses.errors)}`);
  }
  assert.deepEqual(byId.get("filthy_street_urchin").senses, { alertness: 40, telepathy_profile: "normal" });
  assert.equal(byId.get("giant_yellow_centipede").senses.telepathy_profile, "weird_mind");
  assert.equal(byId.get("grey_mold").senses.telepathy_profile, "empty_mind");
});

test("monster schema rejects invalid senses and duplicate resistances", () => {
  const negativeAlertness = structuredClone(monsters);
  negativeAlertness.monsters[0].senses.alertness = -1;
  assert.equal(validateSenses(negativeAlertness.monsters[0].senses), false);

  const invalidProfile = structuredClone(monsters);
  invalidProfile.monsters[0].senses.telepathy_profile = "unknown";
  assert.equal(validateSenses(invalidProfile.monsters[0].senses), false);

  const duplicateResistance = structuredClone(monsters);
  duplicateResistance.monsters[0].resistances = ["imm_fire", "imm_fire"];
  assert.equal(validateResistanceList(duplicateResistance.monsters[0].resistances), false);
});

test("monster schema rejects migrated legacy resistance and sensing flags", () => {
  for (const legacyFlag of ["immune_acid", "immune_elec", "immune_fire", "immune_cold", "immune_pois", "no_sleep", "no_fear", "no_conf", "alertness", "weird_mind", "empty_mind"]) {
    const legacyMonster = structuredClone(monsters);
    legacyMonster.monsters[0].flags ??= {};
    legacyMonster.monsters[0].flags[legacyFlag] = legacyFlag === "alertness" ? 20 : true;
    assert.equal(validateFlags(legacyMonster.monsters[0].flags), false, `${legacyFlag} should be rejected`);
  }
});