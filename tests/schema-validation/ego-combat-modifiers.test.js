import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  return JSON.parse(readFileSync(resolve(root, relPath), "utf8"));
}

const egoData = loadJSON("data/definitions/items/ego_items.json");
const combatData = loadJSON("data/definitions/combat_modifiers.json");
const combatByFlag = new Map(combatData.combat_modifiers.map((modifier) => [modifier.source_flag, modifier.id]));
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

  it("maps every combat legacy flag to its canonical reference", () => {
    for (const ego of egoData.ego_items) {
      for (const flag of ego.flags) {
        const modifierId = combatByFlag.get(flag);
        if (modifierId) {
          assert.ok(ego.combat_modifiers.includes(modifierId), `${ego.id} is missing ${modifierId} for ${flag}`);
        }
      }
    }
  });

  it("preserves legacy flags as the source compatibility representation", () => {
    assert.ok(egoData.ego_items.every((ego) => Array.isArray(ego.flags)));
    assert.equal(egoData.ego_items.length, 116);
  });
});