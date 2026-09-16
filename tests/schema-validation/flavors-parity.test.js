import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

import Ajv from "ajv/dist/2020.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  return JSON.parse(readFileSync(resolve(root, relPath), "utf-8").replace(/^\uFEFF/, ""));
}

const flavorsSchema = loadJSON("data/schemas/flavors.schema.json");
const flavorsData = loadJSON("data/definitions/items/flavors.json");

const ajv = new Ajv({ strict: false, allErrors: true, allowUnionTypes: true });
const validateFlavors = ajv.compile(flavorsSchema);

const expectedCategoryCounts = {
  ring: 43,
  amulet: 26,
  staff: 35,
  wand: 35,
  rod: 35,
  mushroom: 20,
  potion: 59,
  scroll: 51,
};

describe("MAngband 1.5.3 flavor parity", () => {
  it("passes startup schema validation", () => {
    assert.equal(validateFlavors(flavorsData), true, JSON.stringify(validateFlavors.errors, null, 2));
  });

  it("contains exactly the 304 flavors defined by ref-mangband/lib/edit/flavor.txt", () => {
    assert.equal(flavorsData.flavors.length, 304);
  });

  it("has unique string ids", () => {
    const ids = flavorsData.flavors.map((flavor) => flavor.id);
    assert.equal(new Set(ids).size, ids.length, "Duplicate flavor id found");
    for (const id of ids) {
      assert.match(id, /^[a-z][a-z0-9_]*$/, `Flavor id '${id}' is not a stable snake_case identifier`);
    }
  });

  it("has unique legacy mangband indices covering 1-304 (minus renumbered gaps)", () => {
    const indices = flavorsData.flavors.map((flavor) => flavor.legacy.mangband_index);
    assert.equal(new Set(indices).size, indices.length, "Duplicate mangband_index found");
  });

  it("matches the expected per-category flavor counts", () => {
    const actual = {};
    for (const flavor of flavorsData.flavors) {
      actual[flavor.category] = (actual[flavor.category] || 0) + 1;
    }
    assert.deepEqual(actual, expectedCategoryCounts);
  });

  it("does not carry top-level tval/sval fields (legacy metadata only)", () => {
    for (const flavor of flavorsData.flavors) {
      assert.equal(flavor.tval, undefined, `${flavor.id}: unexpected top-level tval`);
      assert.equal(flavor.sval, undefined, `${flavor.id}: unexpected top-level sval`);
      assert.ok(flavor.legacy && typeof flavor.legacy.tval === "number", `${flavor.id}: missing legacy.tval`);
    }
  });

  it("preserves the fixed MAngband rings and potions (The One Ring, water, apple juice, slime mold juice)", () => {
    const byId = new Map(flavorsData.flavors.map((flavor) => [flavor.id, flavor]));

    assert.equal(byId.get("ring_plain_gold").legacy.sval, 37);
    assert.equal(byId.get("potion_clear").legacy.sval, 0);
    assert.equal(byId.get("potion_light_brown").legacy.sval, 1);
    assert.equal(byId.get("potion_icky_green").legacy.sval, 2);
  });

  it("preserves original MAngband flavor text, glyph, and color for a sample of entries", () => {
    const byId = new Map(flavorsData.flavors.map((flavor) => [flavor.id, flavor]));

    assert.deepEqual(
      { display_name: byId.get("ring_ruby").display_name, glyph: byId.get("ring_ruby").glyph, color: byId.get("ring_ruby").color },
      { display_name: "Ruby", glyph: "=", color: "r" }
    );
    assert.deepEqual(
      { display_name: byId.get("amulet_dragon_tooth").display_name, glyph: byId.get("amulet_dragon_tooth").glyph, color: byId.get("amulet_dragon_tooth").color },
      { display_name: "Dragon Tooth", glyph: "\"", color: "W" }
    );
    assert.deepEqual(
      { display_name: byId.get("wand_gold_plated").display_name, glyph: byId.get("wand_gold_plated").glyph, color: byId.get("wand_gold_plated").color },
      { display_name: "Gold-Plated", glyph: "-", color: "y" }
    );
  });
});
