import { readFileSync, readdirSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  let content = readFileSync(resolve(root, relPath), "utf-8");
  if (content.charCodeAt(0) === 0xfeff) content = content.slice(1);
  return JSON.parse(content);
}

function loadJSONOrDefault(relPath, fallbackValue) {
  try {
    return loadJSON(relPath);
  } catch {
    return fallbackValue;
  }
}

function listJsonFilesRecursive(absDir, relPrefix = "") {
  const entries = readdirSync(absDir, { withFileTypes: true });
  const files = [];
  for (const entry of entries) {
    const absPath = resolve(absDir, entry.name);
    const relPath = relPrefix ? `${relPrefix}/${entry.name}` : entry.name;
    if (entry.isDirectory()) {
      files.push(...listJsonFilesRecursive(absPath, relPath));
      continue;
    }
    if (entry.isFile() && relPath.endsWith(".json")) {
      files.push(relPath);
    }
  }
  return files;
}

function visitNodes(node, visitor) {
  if (Array.isArray(node)) {
    for (const item of node) visitNodes(item, visitor);
    return;
  }
  if (!node || typeof node !== "object") return;
  visitor(node);
  for (const value of Object.values(node)) {
    visitNodes(value, visitor);
  }
}

const actionsData = loadJSON("data/definitions/actions.json");
const capabilitiesData = loadJSON("data/definitions/capabilities.json");
const itemCapabilitiesData = loadJSONOrDefault("data/definitions/items/item_capabilities.json", {
  schema_version: 1,
  capabilities: [],
});
const priestPrayersData = loadJSON("data/definitions/priest_prayers.json");
const mageSpellsData = loadJSON("data/definitions/mage_spells.json");

const gameplayRootRel = "data/definitions";
const gameplayRootAbs = resolve(root, gameplayRootRel);
const allDefinitionFiles = listJsonFilesRecursive(gameplayRootAbs, gameplayRootRel);

describe("repository-wide gameplay contracts", () => {
  it("all action_refs conform to canonical action contracts", () => {
    const actionById = new Map(actionsData.actions.map((a) => [a.action_id, a]));
    const knownParameterBaseline = new Map([
      ["ConfuseControl", new Set(["radius"])],
    ]);
    const filesToScan = allDefinitionFiles.filter((p) => !p.endsWith("/actions.json"));
    const violations = [];

    for (const relPath of filesToScan) {
      const json = loadJSON(relPath);
      visitNodes(json, (node) => {
        if (typeof node.action_id !== "string") return;

        const action = actionById.get(node.action_id);
        if (!action) {
          violations.push(`${relPath}: unknown action_id '${node.action_id}'`);
          return;
        }

        const parameters = node.parameters && typeof node.parameters === "object" ? node.parameters : {};
        const contractParams = action.parameter_contract.parameters;
        const declaredIds = new Set(contractParams.map((p) => p.id));

        for (const key of Object.keys(parameters)) {
          if (!declaredIds.has(key)) {
            const allowed = knownParameterBaseline.get(node.action_id);
            if (allowed && allowed.has(key)) continue;
            violations.push(`${relPath}: action '${node.action_id}' has undeclared parameter '${key}'`);
          }
        }

        for (const param of contractParams) {
          if (param.required === true && parameters[param.id] === undefined) {
            violations.push(`${relPath}: action '${node.action_id}' missing required parameter '${param.id}'`);
          }
        }
      });
    }

    assert.equal(
      violations.length,
      0,
      `Action contract violations:\n  ${violations.join("\n  ")}`
    );
  });

  it("capability_ids resolve to known catalogs (with explicit legacy baseline)", () => {
    const knownCapabilityIds = new Set([
      ...capabilitiesData.capabilities.map((c) => c.id),
      ...itemCapabilitiesData.capabilities.map((c) => c.id),
    ]);

    const knownLegacyBaseline = new Set([
      "regeneration",
      "res_chaos",
      "res_nethr",
      "res_shard",
      "res_sound",
      "searching",
      "slay_undead",
      "stealth",
    ]);

    const filesToScan = allDefinitionFiles.filter(
      (p) => !p.endsWith("/capabilities.json") && !p.endsWith("/items/item_capabilities.json")
    );

    const violations = [];
    for (const relPath of filesToScan) {
      const json = loadJSON(relPath);
      visitNodes(json, (node) => {
        if (!Array.isArray(node.capability_ids)) return;
        for (const capabilityId of node.capability_ids) {
          if (knownCapabilityIds.has(capabilityId)) continue;
          if (knownLegacyBaseline.has(capabilityId)) continue;
          violations.push(`${relPath}: unknown capability_id '${capabilityId}'`);
        }
      });
    }

    assert.equal(
      violations.length,
      0,
      `Unknown capability_id references outside legacy baseline:\n  ${violations.join("\n  ")}`
    );
  });
});

describe("duration parity formulas", () => {
  function findSpell(catalog, id) {
    const spell = catalog.spells.find((s) => s.id === id);
    assert.ok(spell, `Missing spell/prayer '${id}'`);
    return spell;
  }

  function firstDuration(spell) {
    const withDuration = (spell.action_refs || []).find(
      (a) => a.parameters && a.parameters.duration
    );
    assert.ok(withDuration, `Missing duration payload for '${spell.id}'`);
    return withDuration.parameters.duration;
  }

  it("prayer duration formulas are explicit and parity-correct", () => {
    const expected = new Map([
      ["prayer_bless", { base: 12, dice: { count: 1, sides: 12 } }],
      ["prayer_chant", { base: 24, dice: { count: 1, sides: 24 } }],
      ["prayer_prayer", { base: 48, dice: { count: 1, sides: 48 } }],
      ["prayer_sense_invisible", { base: 24, dice: { count: 1, sides: 24 } }],
      ["prayer_resist_heat_cold", { base: 10, dice: { count: 1, sides: 10 } }],
      ["prayer_protection_from_evil", { base: 0, dice: { count: 1, sides: 25 }, level_multiplier: 3 }],
    ]);

    for (const [id, formula] of expected.entries()) {
      const spell = findSpell(priestPrayersData, id);
      const duration = firstDuration(spell);
      assert.deepEqual(duration, formula, `Formula mismatch for '${id}'`);
    }
  });

  it("mage duration formulas are explicit and parity-correct", () => {
    const expected = new Map([
      ["magic_heroism", { base: 25, random: 25 }],
      ["magic_berserker", { base: 25, dice: { count: 1, sides: 25 } }],
      ["magic_resist_fire", { base: 20, dice: { count: 1, sides: 20 } }],
      ["magic_resist_cold", { base: 20, dice: { count: 1, sides: 20 } }],
      ["magic_resist_poison", { base: 20, dice: { count: 1, sides: 20 } }],
      ["magic_resistance", { base: 20, dice: { count: 1, sides: 20 } }],
    ]);

    for (const [id, formula] of expected.entries()) {
      const spell = findSpell(mageSpellsData, id);
      const duration = firstDuration(spell);
      assert.deepEqual(duration, formula, `Formula mismatch for '${id}'`);
    }
  });
});
