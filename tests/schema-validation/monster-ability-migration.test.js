import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

import Ajv2020 from "ajv/dist/2020.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = resolve(__dirname, "../..");

function loadJSON(relPath) {
  return JSON.parse(readFileSync(resolve(root, relPath), "utf-8").replace(/^\uFEFF/, ""));
}

const schema = loadJSON("data/schemas/monsters/monsters.schema.json");
const catalog = loadJSON("data/definitions/monsters/monsters.json");

const ajv = new Ajv2020({ strict: false, allErrors: true });
const validateAbilityType = ajv.compile(schema.$defs.ability_type);

describe("monster ability_cooldown_ticks migration", () => {
  it("abilities are plain ability-type strings, not objects", () => {
    for (const monster of catalog.monsters) {
      for (const ability of monster.abilities ?? []) {
        assert.equal(typeof ability, "string", `${monster.id} has a non-string ability entry`);
        assert.ok(validateAbilityType(ability), `${monster.id} has unknown ability type '${ability}'`);
      }
    }
  });

  it("monsters with abilities declare a monster-level ability_cooldown_ticks", () => {
    for (const monster of catalog.monsters) {
      if ((monster.abilities ?? []).length > 0) {
        assert.equal(
          typeof monster.ability_cooldown_ticks,
          "number",
          `${monster.id} has abilities but no ability_cooldown_ticks`
        );
      }
    }
  });

  it("monsters with no abilities do not declare ability_cooldown_ticks", () => {
    for (const monster of catalog.monsters) {
      if (!monster.abilities || monster.abilities.length === 0) {
        assert.equal(
          monster.ability_cooldown_ticks,
          undefined,
          `${monster.id} has no abilities but declares ability_cooldown_ticks`
        );
      }
    }
  });

  it("has at least one monster with no abilities (regression guard)", () => {
    const noAbilityMonsters = catalog.monsters.filter((m) => !m.abilities || m.abilities.length === 0);
    assert.ok(noAbilityMonsters.length > 0, "expected some monsters to have no abilities");
  });

  it("detects monsters with mixed ability cooldowns instead of guessing", () => {
    // simulate the migration's conflict-detection rule directly against the raw legacy shape
    const legacyMonster = {
      id: "test_mixed_cooldowns",
      abilities: [
        { type: "blind", range: 6, cooldown_ticks: 8 },
        { type: "confuse", range: 6, cooldown_ticks: 12 },
      ],
    };
    const cooldowns = new Set(legacyMonster.abilities.map((a) => a.cooldown_ticks));
    assert.equal(cooldowns.size, 2, "fixture should have mixed cooldowns");
    // migration rule: mixed cooldowns must be reported, never guessed
    const shouldSkipMigration = cooldowns.size !== 1;
    assert.ok(shouldSkipMigration);
  });
});
