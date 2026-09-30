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

const egoData = loadJSON("data/definitions/items/ego_items.json");
const egoSchema = loadJSON("data/schemas/items/ego_items.schema.json");
const commonItemSchema = loadJSON("data/schemas/items/common_item.schema.json");
const combatData = loadJSON("data/definitions/combat_modifiers.json");
const combatSchema = loadJSON("data/schemas/combat_modifiers.schema.json");
const combatIds = new Set(combatData.combat_modifiers.map((modifier) => modifier.id));
const ajv = new Ajv({ strict: false, allErrors: true });
ajv.addSchema(commonItemSchema);
const validateEgoItems = ajv.compile(egoSchema);
const validateCombat = ajv.compile(combatSchema);

describe("ego item combat modifier references", () => {
  it("validates the closed canonical ego item structure", () => {
    assert.equal(validateEgoItems(egoData), true, JSON.stringify(validateEgoItems.errors));

    const invalid = structuredClone(egoData);
    invalid.ego_items[0].legacy_flags = {};
    assert.equal(validateEgoItems(invalid), false);
  });

  it("validates canonical combat modifiers including elemental brand effects", () => {
    assert.equal(validateCombat(combatData), true, JSON.stringify(validateCombat.errors));
    for (const modifier of combatData.combat_modifiers.filter((entry) => entry.modifier_kind === "brand")) {
      assert.equal(modifier.effects.length, 1, `${modifier.id} must define its damage effect`);
      assert.equal(modifier.effects[0].monster_species, "all", `${modifier.id} must apply to all species`);
    }
  });

  it("references only known combat modifier IDs without duplicates", () => {
    for (const ego of egoData.ego_items) {
      const combatModifiers = ego.combat_modifiers ?? [];
      assert.equal(new Set(combatModifiers).size, combatModifiers.length, `${ego.id} has duplicate references`);
      for (const id of combatModifiers) {
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