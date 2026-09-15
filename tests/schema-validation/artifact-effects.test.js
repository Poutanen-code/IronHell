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

const validateArtifacts = new Ajv({ strict: false, allErrors: true }).compile(artifactsSchema);

describe("artifact canonical effects", () => {
  it("contains a complete effects object for every artifact", () => {
    assert.equal(artifactsData.artifacts.length, 136);
    for (const artifact of artifactsData.artifacts) {
      assert.deepEqual(Object.keys(artifact.effects).sort(), ["activations", "affixes", "capability_ids", "combat_modifiers", "curses", "display_flags", "resistance_ids"]);
      assert.equal(Object.hasOwn(artifact, "affixes"), false, `${artifact.id} has deprecated top-level affixes`);
      assert.equal(Object.hasOwn(artifact, "flags"), false, `${artifact.id} has deprecated legacy flags`);
    }
  });

  it("resolves canonical capabilities, resistances, and curses", () => {
    for (const artifact of artifactsData.artifacts) {
      for (const id of artifact.effects.capability_ids) assert.ok(capabilities.has(id), `${artifact.id} has unknown capability ${id}`);
      for (const id of artifact.effects.resistance_ids) assert.ok(resistances.has(id), `${artifact.id} has unknown resistance ${id}`);
      for (const id of artifact.effects.curses) assert.ok(["light_curse", "heavy_curse", "perma_curse"].includes(id), `${artifact.id} has unknown curse ${id}`);
    }
  });

  it("extracts activation identifiers and keeps each canonical effect list unique", () => {
    for (const artifact of artifactsData.artifacts) {
      const expected = artifact.activation ? [artifact.activation.id] : [];
      assert.deepEqual(artifact.effects.activations, expected, `${artifact.id} activation mismatch`);
      for (const id of artifact.effects.activations) assert.ok(activations.has(id), `${artifact.id} has unknown activation ${id}`);
      for (const property of ["capability_ids", "resistance_ids", "activations", "curses"]) {
        assert.equal(new Set(artifact.effects[property]).size, artifact.effects[property].length, `${artifact.id} has duplicate ${property}`);
      }
    }
  });

  it("uses only canonical display metadata", () => {
    for (const artifact of artifactsData.artifacts) {
      for (const displayFlag of artifact.effects.display_flags) {
        assert.ok(["show_mods", "hide_type"].includes(displayFlag), `${artifact.id} has unknown display flag ${displayFlag}`);
      }
    }
  });

  it("rejects duplicate canonical effect references through the artifact schema", () => {
    const invalid = structuredClone(artifactsData);
    invalid.artifacts[0].effects.capability_ids = ["see_invis", "see_invis"];
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
