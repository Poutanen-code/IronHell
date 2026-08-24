/**
 * Schema validation tests for the Phase 1 canonical gameplay-definition catalogs:
 *   - terrain_definitions.json
 *   - traps.json
 *   - stores.json
 *   - vaults.json
 *
 * Run: node --test tests/schema-validation/phase1-catalogs.test.js
 */

import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

import Ajv from "ajv/dist/2020.js";
import addFormats from "ajv-formats";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  let content = readFileSync(resolve(root, relPath), "utf-8");
  if (content.charCodeAt(0) === 0xfeff) content = content.slice(1);
  return JSON.parse(content);
}

function buildAjv() {
  const ajv = new Ajv({ strict: false, allErrors: true, allowUnionTypes: true });
  addFormats(ajv);
  return ajv;
}

const commonSchema = loadJSON("data/schemas/common.schema.json");
const terrainSchema = loadJSON("data/schemas/terrain_definitions.schema.json");
const trapsSchema = loadJSON("data/schemas/traps.schema.json");
const storesSchema = loadJSON("data/schemas/stores.schema.json");
const vaultsSchema = loadJSON("data/schemas/vaults.schema.json");

const terrainData = loadJSON("data/definitions/terrain_definitions.json");
const trapsData = loadJSON("data/definitions/traps.json");
const storesData = loadJSON("data/definitions/stores.json");
const vaultsData = loadJSON("data/definitions/vaults.json");

const ajv = buildAjv();
ajv.addSchema(commonSchema);

const validateTerrain = ajv.compile(terrainSchema);
const validateTraps = ajv.compile(trapsSchema);
const validateStores = ajv.compile(storesSchema);
const validateVaults = ajv.compile(vaultsSchema);

function assertValid(validator, data, label) {
  const valid = validator(data);
  if (!valid) {
    assert.fail(`${label} failed schema validation:\n` + JSON.stringify(validator.errors, null, 2));
  }
}

describe("terrain_definitions.json", () => {
  it("validates against terrain_definitions.schema.json", () => {
    assertValid(validateTerrain, terrainData, "terrain_definitions.json");
  });

  it("has unique ids", () => {
    const ids = terrainData.terrain_definitions.map((t) => t.id);
    assert.equal(new Set(ids).size, ids.length, "Duplicate terrain id found");
  });

  it("appears_as references resolve to known terrain ids", () => {
    const ids = new Set(terrainData.terrain_definitions.map((t) => t.id));
    for (const t of terrainData.terrain_definitions) {
      if (t.appears_as !== null) {
        assert.ok(ids.has(t.appears_as), `Terrain '${t.id}' appears_as references unknown id '${t.appears_as}'`);
      }
    }
  });

  it("represents all required terrain categories", () => {
    const required = [
      "floor", "wall", "permanent_wall", "door", "secret_door", "stair",
      "trap", "vein", "shop", "water", "vegetation", "wilderness", "special",
    ];
    const actual = new Set(terrainData.terrain_definitions.map((t) => t.terrain_category));
    for (const category of required) {
      assert.ok(actual.has(category), `Missing terrain_category '${category}'`);
    }
  });
});

describe("traps.json", () => {
  it("validates against traps.schema.json", () => {
    assertValid(validateTraps, trapsData, "traps.json");
  });

  it("has unique ids", () => {
    const ids = trapsData.traps.map((t) => t.id);
    assert.equal(new Set(ids).size, ids.length, "Duplicate trap id found");
  });

  it("source_feature_id references resolve to known terrain ids", () => {
    const terrainIds = new Set(terrainData.terrain_definitions.map((t) => t.id));
    for (const trap of trapsData.traps) {
      assert.ok(
        terrainIds.has(trap.source_feature_id),
        `Trap '${trap.id}' source_feature_id references unknown terrain id '${trap.source_feature_id}'`
      );
    }
  });
});

describe("stores.json", () => {
  it("validates against stores.schema.json", () => {
    assertValid(validateStores, storesData, "stores.json");
  });

  it("has unique ids and store_types", () => {
    const ids = storesData.stores.map((s) => s.id);
    assert.equal(new Set(ids).size, ids.length, "Duplicate store id found");
    const types = storesData.stores.map((s) => s.store_type);
    assert.equal(new Set(types).size, types.length, "Duplicate store_type found");
  });

  it("terrain_feature_id references resolve to known terrain ids", () => {
    const terrainIds = new Set(terrainData.terrain_definitions.map((t) => t.id));
    for (const store of storesData.stores) {
      assert.ok(
        terrainIds.has(store.terrain_feature_id),
        `Store '${store.id}' terrain_feature_id references unknown terrain id '${store.terrain_feature_id}'`
      );
    }
  });

  it("represents all 8 canonical store types", () => {
    const required = [
      "general_store", "armory", "weapon_smith", "temple",
      "alchemy_shop", "magic_shop", "black_market", "home",
    ];
    const actual = new Set(storesData.stores.map((s) => s.store_type));
    for (const type of required) {
      assert.ok(actual.has(type), `Missing store_type '${type}'`);
    }
  });
});

describe("vaults.json", () => {
  it("validates against vaults.schema.json", () => {
    assertValid(validateVaults, vaultsData, "vaults.json");
  });

  it("has unique ids", () => {
    const ids = vaultsData.vaults.map((v) => v.id);
    assert.equal(new Set(ids).size, ids.length, "Duplicate vault id found");
  });

  it("layout row count matches dimensions.rows", () => {
    for (const vault of vaultsData.vaults) {
      assert.equal(
        vault.layout.length,
        vault.dimensions.rows,
        `Vault '${vault.id}' layout has ${vault.layout.length} rows, expected ${vault.dimensions.rows}`
      );
    }
  });

  it("every layout row width matches dimensions.cols", () => {
    for (const vault of vaultsData.vaults) {
      for (const [index, row] of vault.layout.entries()) {
        assert.equal(
          row.length,
          vault.dimensions.cols,
          `Vault '${vault.id}' layout row ${index} has width ${row.length}, expected ${vault.dimensions.cols}`
        );
      }
    }
  });

  it("every glyph used in layout is present in legend", () => {
    for (const vault of vaultsData.vaults) {
      const glyphs = new Set(vault.layout.join("").split(""));
      for (const glyph of glyphs) {
        assert.ok(
          Object.prototype.hasOwnProperty.call(vault.legend, glyph),
          `Vault '${vault.id}' uses glyph '${glyph}' with no legend entry`
        );
      }
    }
  });

  it("includes 5 starter, 3 greater, and 2 special vaults", () => {
    const starter = vaultsData.vaults.filter((v) => v.tags.includes("starter"));
    const greater = vaultsData.vaults.filter((v) => v.tags.includes("greater"));
    const special = vaultsData.vaults.filter((v) => v.tags.includes("special"));
    assert.equal(starter.length, 5, "Expected 5 starter vaults");
    assert.equal(greater.length, 3, "Expected 3 greater vaults");
    assert.equal(special.length, 2, "Expected 2 special vaults");
  });
});
