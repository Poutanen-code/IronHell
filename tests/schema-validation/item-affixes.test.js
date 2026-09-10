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

const catalog = loadJSON("data/definitions/items/item_affixes.json");
const artifacts = loadJSON("data/definitions/items/artifacts.json");
const egoItems = loadJSON("data/definitions/items/ego_items.json");
const affixIds = new Set(catalog.item_affixes.map((affix) => affix.id));

function assertAffixesResolve(item, affixes, label) {
  assert.ok(Array.isArray(affixes), `${label} is missing affixes`);
  const ids = affixes.map((affix) => typeof affix === "string" ? affix : affix.id);
  assert.equal(new Set(ids).size, ids.length, `${label} has duplicate affix references`);
  for (const id of ids) assert.ok(affixIds.has(id), `${label} references unknown affix ${id}`);
}

describe("item affix catalog", () => {
  it("has unique canonical IDs", () => {
    assert.equal(new Set(catalog.item_affixes.map((affix) => affix.id)).size, catalog.item_affixes.length);
    assert.equal(catalog.item_affixes.length, 18);
  });

  it("validates canonical artifact effects affix references", () => {
    assert.equal(artifacts.artifacts.length, 136);
    for (const artifact of artifacts.artifacts) {
      assert.equal(Object.hasOwn(artifact, "affixes"), false, `${artifact.id} has deprecated top-level affixes`);
      assertAffixesResolve(artifact, artifact.effects.affixes, artifact.id);
      assert.ok(Array.isArray(artifact.flags));
      assert.equal(typeof artifact.pval, "number");
      assert.equal(typeof artifact.plus_to_hit, "number");
      assert.equal(typeof artifact.plus_to_dam, "number");
      assert.equal(typeof artifact.plus_to_ac, "number");
    }
  });

  it("validates ego effects affix references", () => {
    for (const ego of egoItems.ego_items) {
      const affixes = ego.effects?.affixes ?? [];
      assertAffixesResolve(ego, affixes, ego.id);
    }
  });
});
