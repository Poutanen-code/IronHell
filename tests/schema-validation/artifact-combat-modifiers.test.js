import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  let content = readFileSync(resolve(root, relPath), "utf8");
  if (content.charCodeAt(0) === 0xfeff) content = content.slice(1);
  return JSON.parse(content);
}

const artifactsData = loadJSON("data/definitions/items/artifacts.json");
const combatData = loadJSON("data/definitions/combat_modifiers.json");
const combatByFlag = new Map(combatData.combat_modifiers.map((modifier) => [modifier.source_flag, modifier.id]));
const combatIds = new Set(combatData.combat_modifiers.map((modifier) => modifier.id));

function artifactName(artifact) {
  return artifact.id ?? artifact.name;
}

describe("artifact combat modifier references", () => {
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

  it("maps every combat legacy flag to its canonical reference", () => {
    for (const artifact of artifactsData.artifacts) {
      for (const flag of artifact.flags) {
        const modifierId = combatByFlag.get(flag);
        if (modifierId) {
          assert.ok(artifact.effects.combat_modifiers.includes(modifierId), `${artifactName(artifact)} is missing ${modifierId} for ${flag}`);
        }
      }
    }
  });

  it("preserves legacy flags and artifact count", () => {
    assert.ok(artifactsData.artifacts.every((artifact) => Array.isArray(artifact.flags)));
    assert.equal(artifactsData.artifacts.length, 136);
  });
});
