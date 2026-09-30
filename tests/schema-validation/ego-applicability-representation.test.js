import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";
import Ajv from "ajv/dist/2020.js";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const loadJSON = (path) => JSON.parse(readFileSync(resolve(root, path), "utf8"));
const egoSchema = loadJSON("data/schemas/items/ego_items.schema.json");
const commonSchema = loadJSON("data/schemas/items/common_item.schema.json");
const egoData = loadJSON("data/definitions/items/ego_items.json");
const armor = loadJSON("data/definitions/items/armor.json").armor.map((item) => ({ ...item, catalog: "armor" }));
const weapons = loadJSON("data/definitions/items/weapons.json").weapons.map((item) => ({ ...item, catalog: "weapons" }));
const items = [...armor, ...weapons];

const ajv = new Ajv({ strict: false, allErrors: true });
ajv.addSchema(commonSchema, "common_item.schema.json");
const validate = ajv.compile(egoSchema);

function matches(item, selector) {
  const classes = { sling: "launcher", bow: "launcher", crossbow: "launcher", shot: "ammunition", arrow: "ammunition", bolt: "ammunition", sword: "blade", dagger: "blade", mace: "hafted", staff: "hafted", axe: "polearm_and_axe", polearm: "polearm_and_axe", shovel: "digging_tool", pick: "digging_tool", mattock: "digging_tool" };
  return (!selector.weapon_families || (item.catalog === "weapons" && selector.weapon_families.includes(item.subtype)))
    && (!selector.weapon_classes || (item.catalog === "weapons" && selector.weapon_classes.includes(classes[item.subtype])))
    && (!selector.weapon_handling || (item.catalog === "weapons" && selector.weapon_handling.includes(item.type)))
    && (!selector.armor_forms || (item.catalog === "armor" && selector.armor_forms.includes(item.type)))
    && (!selector.armor_materials || (item.catalog === "armor" && selector.armor_materials.includes(item.armor_weight)))
    && (!selector.armor_body_families || (item.catalog === "armor" && selector.armor_body_families.includes(item.armor_family)));
}

function eligible(selector) {
  return items.filter((item) => matches(item, selector)).map((item) => item.id).sort();
}

describe("named ego applicability representation", () => {
  it("ORs values within one semantic property", () => {
    const ids = eligible({ weapon_families: ["sword", "dagger"] });
    assert.ok(ids.includes("dagger"));
    assert.ok(ids.includes("long_sword"));
    assert.ok(!ids.includes("mace"));
  });

  it("ANDs different semantic properties", () => {
    const ids = eligible({ armor_forms: ["boots"], armor_materials: ["leather"] });
    assert.deepEqual(ids, ["pair_of_hard_leather_boots", "pair_of_soft_leather_boots"]);
  });

  it("keeps explicit item IDs as a union branch", () => {
    const ego = egoData.ego_items.find((entry) => entry.id === "of_slay_undead");
    const semantic = Object.fromEntries(Object.entries(ego.applicability).filter(([key]) => key !== "item_ids"));
    const ids = new Set([...eligible(semantic), ...ego.applicability.item_ids]);
    assert.ok(ids.has("flail"));
    assert.ok(ids.has("bastard_sword"));
  });

  it("rejects unknown selectors and empty applicability", () => {
    const unknown = structuredClone(egoData);
    unknown.ego_items[0].applicability.unknown_selector = ["value"];
    assert.equal(validate(unknown), false);

    const empty = structuredClone(egoData);
    empty.ego_items[0].applicability = {};
    assert.equal(validate(empty), false);
  });

  it("keeps dragon-scale armor distinct from soft and hard families", () => {
    const ids = eligible({ armor_body_families: ["soft", "hard"] });
    assert.ok(!ids.some((id) => id.includes("dragon_scale")));
    assert.ok(ids.includes("robe"));
    assert.ok(ids.includes("rusty_chain_mail"));
  });
});
