import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import { describe, it } from "node:test";
import assert from "node:assert/strict";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const rods = JSON.parse(readFileSync(resolve(root, "data/definitions/items/rods.json"), "utf8")).rods;
const expected = [
  ["rod_of_trap_location", ["DetectEntities"]], ["rod_of_door_stair_location", ["DetectEntities"]], ["rod_of_perception", ["IdentifyItem"]], ["rod_of_recall", ["RecallOrLevelShift"]], ["rod_of_illumination", ["LightArea"]], ["rod_of_enlightenment", ["MapArea"]], ["rod_of_detection", ["DetectEntities"]], ["rod_of_probing", ["InspectEntity"]], ["rod_of_curing", ["CureStatus"]], ["rod_of_healing", ["HealHP", "CureStatus"]], ["rod_of_restoration", ["RestoreAttributes", "RestoreAttributes"]], ["rod_of_speed", ["ModifyPlayerSpeed"]], ["rod_of_teleport_other", ["TeleportTarget"]], ["rod_of_disarming", ["AlterTerrain"]], ["rod_of_light", ["BeamDamage"]], ["rod_of_sleep_monster", ["SleepControl"]], ["rod_of_slow_monster", ["ModifyMonsterSpeed"]], ["rod_of_drain_life", ["DrainLife"]], ["rod_of_polymorph", ["TransformEntity"]], ["rod_of_acid_bolt", ["BoltDamage"]], ["rod_of_lightning_bolts", ["BoltDamage"]], ["rod_of_fire_bolts", ["BoltDamage"]], ["rod_of_frost_bolts", ["BoltDamage"]], ["rod_of_acid_ball", ["BallDamage"]], ["rod_of_lightning_ball", ["BallDamage"]], ["rod_of_fire_ball", ["BallDamage"]], ["rod_of_frost_ball", ["BallDamage"]],
];

describe("MAngband 1.5.3 rod identity parity", () => {
  it("maps each supported rod to its stable ID and semantic actions", () => {
    assert.equal(rods.length, expected.length);
    const actual = rods.map((rod) => [rod.id, (rod.actions ?? []).map((action) => action.action_id)]);
    const expectedById = new Map(expected.map(([id, actions]) => [id, [id, actions]]));
    assert.deepEqual(actual.sort(), [...expectedById.values()].sort());
    assert.equal(new Set(rods.map((rod) => rod.id)).size, rods.length);
  });

  it("does not classify wand-only object kinds as rods", () => {
    const forbidden = new Set([
      "rod_of_heal_monster",
      "rod_of_haste_monster",
      "rod_of_clone_monster",
      "rod_of_trap_door_destruction",
      "rod_of_stone_to_mud",
      "rod_of_confuse_monster",
      "rod_of_scare_monster",
      "rod_of_magic_missile",
    ]);
    assert.ok(rods.every((rod) => !forbidden.has(rod.id)));
  });

  it("keeps every current source base-kind tuple globally unique", () => {
    const keys = rods.map((item) => item.id);
    assert.equal(new Set(keys).size, keys.length);
  });
});
