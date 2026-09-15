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

const egoData = loadJSON("data/definitions/items/ego_items.json");
const combatData = loadJSON("data/definitions/combat_modifiers.json");
const combatIds = new Set(combatData.combat_modifiers.map((modifier) => modifier.id));

describe("ego item combat modifier references", () => {
  it("references only known combat modifier IDs without duplicates", () => {
    for (const ego of egoData.ego_items) {
      assert.ok(Array.isArray(ego.combat_modifiers), `${ego.id} is missing combat_modifiers`);
      assert.equal(new Set(ego.combat_modifiers).size, ego.combat_modifiers.length, `${ego.id} has duplicate references`);
      for (const id of ego.combat_modifiers) {
        assert.ok(combatIds.has(id), `${ego.id} references unknown combat modifier ${id}`);
      }
    }
  });

  it("does not retain legacy flags", () => {
    for (const ego of egoData.ego_items) {
      assert.equal(Object.hasOwn(ego, "flags"), false, `${ego.id} retains legacy flags`);
    }
  });

  it("preserves ego item count", () => {
    assert.equal(egoData.ego_items.length, 116);
  });
});