import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const loadJSON = (path) => JSON.parse(readFileSync(resolve(root, path), "utf8"));
const egoData = loadJSON("data/definitions/items/ego_items.json").ego_items;
const golden = loadJSON("tests/schema-validation/ego-applicability-golden.json");
const baseCatalogs = ["weapons", "armor", "potions", "rods", "scrolls", "staves", "wands"];
const baseItems = baseCatalogs.flatMap((catalog) => {
  const document = loadJSON(`data/definitions/items/${catalog}.json`);
  return document[catalog].map((item) => ({ ...item, catalog }));
});
const itemIds = new Set(baseItems.map((item) => item.id));
const sorted = (values) => [...new Set(values)].sort();

const semanticSelectors = {
  "16:0:2": { weapon_families: ["shot"] },
  "16:0:99": { weapon_families: ["shot"] },
  "17:0:99": { weapon_families: ["arrow"] },
  "18:0:99": { weapon_families: ["bolt"] },
  "19:0:99": { weapon_classes: ["launcher"] },
  "19:2:2": { weapon_families: ["sling"] },
  "19:12:13": { weapon_families: ["bow"] },
  "20:0:99": { weapon_handling: ["digging"] },
  "21:0:99": { weapon_families: ["mace", "staff"] },
  "22:0:99": { weapon_families: ["axe", "polearm"] },
  "23:0:99": { weapon_families: ["sword", "dagger"] },
  "30:0:99": { armor_forms: ["boots"] },
  "30:2:3": { armor_forms: ["boots"], armor_materials: ["leather"] },
  "31:0:99": { armor_forms: ["gloves"] },
  "32:0:99": { armor_forms: ["helmet"] },
  "33:0:99": { armor_forms: ["crown"] },
  "34:0:99": { armor_forms: ["shield"] },
  "35:0:99": { armor_forms: ["cloak"] },
  "36:0:99": { armor_body_families: ["soft"] },
  "37:0:99": { armor_body_families: ["hard"] },
};
const explicitIdRules = [
  "16:2:99", "17:0:2", "17:2:99", "18:0:2", "18:2:99", "19:23:23",
  "21:0:18", "21:12:99", "22:10:99", "23:16:99", "31:1:1", "31:2:99",
  "36:2:2", "37:3:99",
];

function matchesSelector(item, selector) {
  if (selector.weapon_families && (item.catalog !== "weapons" || !selector.weapon_families.includes(item.subtype))) return false;
  if (selector.weapon_handling && (item.catalog !== "weapons" || !selector.weapon_handling.includes(item.type))) return false;
  if (selector.weapon_classes) {
    const classes = { sling: "launcher", bow: "launcher", crossbow: "launcher", shot: "ammunition", arrow: "ammunition", bolt: "ammunition", sword: "blade", dagger: "blade", mace: "hafted", staff: "hafted", axe: "polearm_and_axe", polearm: "polearm_and_axe", shovel: "digging_tool", pick: "digging_tool", mattock: "digging_tool" };
    if (item.catalog !== "weapons" || !selector.weapon_classes.includes(classes[item.subtype])) return false;
  }
  if (selector.armor_forms && (item.catalog !== "armor" || !selector.armor_forms.includes(item.type))) return false;
  if (selector.armor_materials && (item.catalog !== "armor" || !selector.armor_materials.includes(item.armor_weight))) return false;
  if (selector.armor_body_families && (item.catalog !== "armor" || !selector.armor_body_families.includes(item.armor_family))) return false;
  return true;
}

function eligibleIds(ego) {
  const semantic = Object.fromEntries(Object.entries(ego.applicability).filter(([key]) => key !== "item_ids"));
  return sorted(baseItems.filter((item) =>
    (Object.keys(semantic).length > 0 && matchesSelector(item, semantic)) ||
    (ego.applicability.item_ids ?? []).includes(item.id),
  ).map((item) => item.id));
}

describe("legacy ego applicability golden matrix", () => {
  it("preserves each ego's exact source rule and eligible base item IDs", () => {
    assert.equal(new Set(egoData.map((ego) => ego.id)).size, egoData.length);
    assert.deepEqual(sorted(egoData.map((ego) => ego.id)), sorted(Object.keys(golden.egos)));
    assert.equal(new Set(egoData.map((ego) => ego.legacy_source_serial)).size, egoData.length);

    for (const ego of egoData) {
      const expected = golden.egos[ego.id];
      assert.equal(ego.legacy_source_serial, expected.legacy_source_serial, ego.id);
      assert.ok(Object.keys(ego.applicability).length > 0, `${ego.id}: applicability`);
      assert.deepEqual(eligibleIds(ego), expected.included_item_ids, `${ego.id}: eligible base item IDs`);
    }
  });

  it("covers all 34 unique rules and proves each semantic predicate equals its legacy set", () => {
    const goldenRuleKeys = sorted(Object.keys(golden.patterns));
    assert.equal(goldenRuleKeys.length, 34);
    assert.equal(Object.keys(semanticSelectors).length, 20);
    assert.deepEqual(sorted(goldenRuleKeys.filter((key) => !semanticSelectors[key])), sorted(explicitIdRules));
    assert.equal(explicitIdRules.length, 14);

    for (const ruleKey of goldenRuleKeys) {
      if (semanticSelectors[ruleKey]) {
        const semanticSet = sorted(baseItems.filter((item) => matchesSelector(item, semanticSelectors[ruleKey])).map((item) => item.id));
        assert.deepEqual(semanticSet, golden.patterns[ruleKey], `${ruleKey}: semantic set must equal legacy set`);
      } else {
        assert.ok(golden.patterns[ruleKey].every((id) => itemIds.has(id)), `${ruleKey}: explicit IDs must resolve`);
      }
    }

    assert.deepEqual(sorted(Object.keys(semanticSelectors).filter((key) => golden.patterns[key])), sorted(Object.keys(semanticSelectors)));
  });
});
