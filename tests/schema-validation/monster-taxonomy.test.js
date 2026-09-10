import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

import Ajv from "ajv/dist/2020.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  return JSON.parse(readFileSync(resolve(root, relPath), "utf-8"));
}

const monstersData = loadJSON("data/definitions/monsters/monsters.json");
const monstersSchema = loadJSON("data/schemas/monsters/monsters.schema.json");
const modifiersData = loadJSON("data/definitions/combat_modifiers.json");
const modifiersSchema = loadJSON("data/schemas/combat_modifiers.schema.json");
const ajv = new Ajv({ strict: false, allErrors: true });
const validateMonsters = ajv.compile(monstersSchema);
const validateModifiers = ajv.compile(modifiersSchema);

const speciesValues = new Set(["beast", "humanoid", "undead", "demon", "dragon", "construct", "aberration"]);
const categoryValues = new Set(["animal", "orc", "undead", "demon", "dragon"]);
const alignmentValues = new Set(["good", "neutral", "evil"]);

function assertValid(validate, value, label) {
  assert.equal(validate(value), true, `${label} failed validation: ${JSON.stringify(validate.errors)}`);
}

function matches(monster, effect) {
  if (effect.monster_species) return monster.species === effect.monster_species;
  if (effect.monster_category) return monster.categories.includes(effect.monster_category);
  if (effect.monster_alignment) return monster.alignment === effect.monster_alignment;
  return false;
}

describe("monster taxonomy schema", () => {
  it("accepts the repository monster catalog", () => {
    assertValid(validateMonsters, monstersData, "monsters.json");
  });

  it("uses species as the required primary taxonomy field", () => {
    for (const monster of monstersData.monsters) {
      assert.ok(speciesValues.has(monster.species), `${monster.id} has unknown species ${monster.species}`);
      assert.equal(Object.hasOwn(monster, "type"), false, `${monster.id} still uses deprecated type`);
    }
  });

  it("requires valid alignment values", () => {
    for (const monster of monstersData.monsters) {
      assert.ok(alignmentValues.has(monster.alignment), `${monster.id} has unknown alignment ${monster.alignment}`);
    }
  });

  it("uses unique, non-empty, controlled category values", () => {
    for (const monster of monstersData.monsters) {
      assert.equal(new Set(monster.categories).size, monster.categories.length, `${monster.id} has duplicate categories`);
      for (const category of monster.categories) {
        assert.ok(categoryValues.has(category), `${monster.id} has unknown category ${category}`);
        assert.notEqual(category, "", `${monster.id} has an empty category`);
      }
    }
  });
});

describe("combat modifier taxonomy metadata", () => {
  it("accepts the combat modifier catalog", () => {
    assertValid(validateModifiers, modifiersData, "combat_modifiers.json");
  });

  it("uses canonical fields for slay targets", () => {
    const effects = Object.fromEntries(
      modifiersData.combat_modifiers
        .filter((modifier) => modifier.id.startsWith("slay_"))
        .map((modifier) => [modifier.id, modifier.effects[0]])
    );

    assert.equal(effects.slay_animal.monster_category, "animal");
    assert.equal(effects.slay_orc.monster_category, "orc");
    assert.equal(effects.slay_evil.monster_alignment, "evil");
    assert.equal(effects.slay_dragon.monster_species, "dragon");
    assert.equal(effects.slay_undead.monster_species, "undead");
    assert.equal(effects.slay_demon.monster_species, "demon");
    assert.equal(effects.slay_troll.monster_species, "troll");
    assert.equal(effects.slay_giant.monster_species, "giant");
  });

  it("declares one explicit modifier kind for every entry", () => {
    const kinds = new Set(["impact", "slay", "kill", "brand"]);
    for (const modifier of modifiersData.combat_modifiers) {
      assert.ok(kinds.has(modifier.modifier_kind), `${modifier.id} has an invalid modifier kind`);
    }
  });

  it("declares supported elements for every brand", () => {
    const elements = new Set(["acid", "elec", "fire", "cold", "pois"]);
    for (const modifier of modifiersData.combat_modifiers.filter((entry) => entry.modifier_kind === "brand")) {
      assert.ok(elements.has(modifier.element), `${modifier.id} has an invalid brand element`);
      assert.equal(Object.hasOwn(modifier, "effects"), false, `${modifier.id} unexpectedly defines an effect payload`);
    }
  });

  it("rejects mixed taxonomy selectors", () => {
    const invalid = structuredClone(modifiersData);
    invalid.combat_modifiers.find((modifier) => modifier.id === "slay_dragon").effects[0].monster_category = "animal";
    assert.equal(validateModifiers(invalid), false);
  });

  it("keeps slay and kill pairs on the same target", () => {
    const modifiers = Object.fromEntries(modifiersData.combat_modifiers.map((modifier) => [modifier.id, modifier]));
    for (const slay of modifiersData.combat_modifiers.filter((modifier) => modifier.modifier_kind === "slay")) {
      const kill = modifiers[`kill_${slay.id.slice("slay_".length)}`];
      if (kill) {
        assert.deepEqual(slay.effects[0], {
          ...kill.effects[0],
          multiplier: slay.effects[0].multiplier,
        });
      }
    }
  });

  it("has unique modifier IDs and source flags", () => {
    const ids = modifiersData.combat_modifiers.map((modifier) => modifier.id);
    const sourceFlags = modifiersData.combat_modifiers.map((modifier) => modifier.source_flag);
    assert.equal(new Set(ids).size, ids.length);
    assert.equal(new Set(sourceFlags).size, sourceFlags.length);
  });

  it("reports zero-coverage target families without changing monster data", () => {
    const targets = modifiersData.combat_modifiers
      .filter((modifier) => modifier.id.startsWith("slay_"))
      .map((modifier) => ({ id: modifier.id, effect: modifier.effects[0] }));
    const zeroCoverage = targets
      .filter(({ effect }) => !monstersData.monsters.some((monster) => matches(monster, effect)))
      .map(({ id }) => id);

    assert.deepEqual(zeroCoverage, ["slay_troll", "slay_giant"]);
  });

  it("does not change the canonical monster target rules", () => {
    const targets = modifiersData.combat_modifiers
      .filter((modifier) => modifier.id.startsWith("slay_"))
      .map((modifier) => ({ id: modifier.id, effect: modifier.effects[0] }));
    const expected = {
      slay_animal: "category",
      slay_evil: "alignment",
      slay_undead: "species",
      slay_demon: "species",
      slay_orc: "category",
      slay_troll: "species",
      slay_giant: "species",
      slay_dragon: "species",
    };

    for (const { id, effect } of targets) {
      const field = Object.keys(effect).find((key) => key.startsWith("monster_"));
      assert.equal(field.replace("monster_", ""), expected[id]);
    }
  });
});
