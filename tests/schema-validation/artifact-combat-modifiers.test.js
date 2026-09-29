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
const weaponsData = loadJSON("data/definitions/items/weapons.json");
const combatData = loadJSON("data/definitions/combat_modifiers.json");
const weaponsSchema = loadJSON("data/schemas/items/weapons.schema.json");
const commonItemSchema = loadJSON("data/schemas/items/common_item.schema.json");
const combatIds = new Set(combatData.combat_modifiers.map((modifier) => modifier.id));
const ajv = new Ajv({ strict: false, allErrors: true });
ajv.addSchema(commonItemSchema);
const validateWeapons = ajv.compile(weaponsSchema);

function artifactName(artifact) {
  return artifact.id ?? artifact.name;
}

describe("artifact combat modifier references", () => {
  it("validates base weapon combat modifier references", () => {
    assert.equal(validateWeapons(weaponsData), true, JSON.stringify(validateWeapons.errors));
    for (const weapon of weaponsData.weapons) {
      for (const id of weapon.combat_modifiers ?? []) {
        assert.ok(combatIds.has(id), `${weapon.id} references unknown combat modifier ${id}`);
      }
    }
    assert.deepEqual(weaponsData.weapons.find((weapon) => weapon.id === "mace_of_disruption").combat_modifiers, ["slay_undead"]);
  });

  it("references only known combat modifier IDs without duplicates", () => {
    for (const artifact of artifactsData.artifacts) {
      assert.equal(Object.hasOwn(artifact, "combat_modifiers"), false, `${artifactName(artifact)} has a deprecated top-level combat_modifiers property`);
      assert.ok(Array.isArray(artifact.effects.combat_modifiers), `${artifactName(artifact)} is missing effects.combat_modifiers`);
      assert.equal(new Set(artifact.effects.combat_modifiers).size, artifact.effects.combat_modifiers.length, `${artifactName(artifact)} has duplicate references`);
      for (const id of artifact.effects.combat_modifiers) {
        assert.ok(combatIds.has(id), `${artifactName(artifact)} references unknown combat modifier ${id}`);
      }
    }
  });

  it("does not retain legacy flags", () => {
    for (const artifact of artifactsData.artifacts) {
      assert.equal(Object.hasOwn(artifact, "flags"), false, `${artifactName(artifact)} retains legacy flags`);
    }
  });

  it("preserves artifact count", () => {
    assert.equal(artifactsData.artifacts.length, 136);
  });
});
