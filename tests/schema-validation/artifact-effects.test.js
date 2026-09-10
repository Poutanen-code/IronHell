import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";
import Ajv from "ajv/dist/2020.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  let content = readFileSync(resolve(root, relPath), "utf8");
  if (content.charCodeAt(0) === 0xfeff) content = content.slice(1);
  return JSON.parse(content);
}

const artifactsData = loadJSON("data/definitions/items/artifacts.json");
const artifactsSchema = loadJSON("data/schemas/items/artifacts.schema.json");
const capabilities = new Set(loadJSON("data/definitions/capabilities.json").capabilities.map((entry) => entry.id));
const resistances = new Set(loadJSON("data/definitions/resistances.json").resistances.map((entry) => entry.id));
const activations = new Set(loadJSON("data/definitions/activations.json").activations.map((entry) => entry.activation_id));

const capabilityFlags = {
  FREE_ACT: "free_act", SEE_INVIS: "see_invis", HOLD_LIFE: "hold_life", REGEN: "regen",
  SLOW_DIGEST: "slow_digest", TELEPATHY: "telepathy", FEATHER: "feather", SUST_STR: "sust_str",
  SUST_INT: "sust_int", SUST_WIS: "sust_wis", SUST_DEX: "sust_dex", SUST_CON: "sust_con",
  SUST_CHR: "sust_chr", AGGRAVATE: "aggravate_monsters", DRAIN_EXP: "drain_exp",
};
const resistanceFlags = {
  RES_ACID: "res_acid", RES_ELEC: "res_elec", RES_FIRE: "res_fire", RES_COLD: "res_cold",
  RES_POIS: "res_pois", RES_FEAR: "res_fear", RES_CONFU: "res_confu", RES_SOUND: "res_sound",
  RES_SHARD: "res_shard", RES_NEXUS: "res_nexus", RES_NETHR: "res_nethr", RES_CHAOS: "res_chaos",
  RES_DISEN: "res_disen", RES_LITE: "res_lite", RES_DARK: "res_dark", RES_BLIND: "res_blind",
  IM_ACID: "imm_acid", IM_ELEC: "imm_elec", IM_FIRE: "imm_fire", IM_COLD: "imm_cold",
  IGNORE_ACID: "ignore_acid", IGNORE_ELEC: "ignore_elec", IGNORE_FIRE: "ignore_fire", IGNORE_COLD: "ignore_cold",
};
const curseFlags = { LIGHT_CURSE: "light_curse", HEAVY_CURSE: "heavy_curse", PERMA_CURSE: "perma_curse" };
const validateArtifacts = new Ajv({ strict: false, allErrors: true }).compile(artifactsSchema);

function assertCoverage(artifact, mapping, property) {
  for (const flag of artifact.flags) {
    const id = mapping[flag];
    if (id) assert.ok(artifact.effects[property].includes(id), `${artifact.id} is missing ${id} for ${flag}`);
  }
}

describe("artifact canonical effects", () => {
  it("contains a complete effects object for every artifact", () => {
    assert.equal(artifactsData.artifacts.length, 136);
    for (const artifact of artifactsData.artifacts) {
      assert.deepEqual(Object.keys(artifact.effects).sort(), ["activations", "affixes", "capabilities", "combat_modifiers", "curses", "display_flags", "resistances"]);
      assert.equal(Object.hasOwn(artifact, "affixes"), false, `${artifact.id} has deprecated top-level affixes`);
    }
  });

  it("extracts canonical capabilities, resistances, immunities, ignores, and curses", () => {
    for (const artifact of artifactsData.artifacts) {
      assertCoverage(artifact, capabilityFlags, "capabilities");
      assertCoverage(artifact, resistanceFlags, "resistances");
      assertCoverage(artifact, curseFlags, "curses");
      for (const id of artifact.effects.capabilities) assert.ok(capabilities.has(id), `${artifact.id} has unknown capability ${id}`);
      for (const id of artifact.effects.resistances) assert.ok(resistances.has(id), `${artifact.id} has unknown resistance ${id}`);
    }
  });

  it("extracts activation identifiers and keeps each canonical effect list unique", () => {
    for (const artifact of artifactsData.artifacts) {
      const expected = artifact.activation ? [artifact.activation.id] : [];
      assert.deepEqual(artifact.effects.activations, expected, `${artifact.id} activation mismatch`);
      for (const id of artifact.effects.activations) assert.ok(activations.has(id), `${artifact.id} has unknown activation ${id}`);
      for (const property of ["capabilities", "resistances", "activations", "curses"]) {
        assert.equal(new Set(artifact.effects[property]).size, artifact.effects[property].length, `${artifact.id} has duplicate ${property}`);
      }
    }
  });

  it("rejects duplicate canonical effect references through the artifact schema", () => {
    const invalid = structuredClone(artifactsData);
    invalid.artifacts[0].effects.capabilities = ["see_invis", "see_invis"];
    assert.equal(validateArtifacts(invalid), false);
  });

  it("rejects deprecated top-level artifact affixes through the artifact schema", () => {
    const invalid = structuredClone(artifactsData);
    invalid.artifacts[0].affixes = [];
    assert.equal(validateArtifacts(invalid), false);
  });

  it("identifies unknown canonical effect references", () => {
    const invalidId = "missing_capability";
    assert.equal(capabilities.has(invalidId), false);
  });
});
