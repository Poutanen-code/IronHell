import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";
import assert from "node:assert/strict";
import Ajv2020 from "ajv/dist/2020.js";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");

function loadJSON(path) {
  return JSON.parse(readFileSync(resolve(root, path), "utf8").replace(/^\uFEFF/, ""));
}

const contracts = [
  ["actions", "data/definitions/actions.json", "data/schemas/actions.schema.json"],
  ["statuses", "data/definitions/statuses.json", "data/schemas/statuses.schema.json"],
  ["capabilities", "data/definitions/capabilities.json", "data/schemas/capabilities.schema.json"],
  ["resistances", "data/definitions/resistances.json", "data/schemas/resistances.schema.json"],
  ["combat modifiers", "data/definitions/combat_modifiers.json", "data/schemas/combat_modifiers.schema.json"],
  ["activations", "data/definitions/activations.json", "data/schemas/activations.schema.json"],
  ["monster abilities", "data/definitions/monsters/monster_abilities.json", "data/schemas/monsters/monster_abilities.schema.json"],
  ["monster capabilities", "data/definitions/monsters/monster_capabilities.json", "data/schemas/monsters/monster_capabilities.schema.json"],
  ["monster loot", "data/definitions/monsters/monster_loot.json", "data/schemas/monsters/monster_loot.schema.json"],
  ["monsters", "data/definitions/monsters/monsters.json", "data/schemas/monsters/monsters.schema.json"],
  ["terrain", "data/definitions/environment/terrain_definitions.json", "data/schemas/environment/terrain_definitions.schema.json"],
  ["traps", "data/definitions/environment/traps.json", "data/schemas/environment/traps.schema.json"],
  ["weapons", "data/definitions/items/weapons.json", "data/schemas/items/weapons.schema.json"],
  ["armor", "data/definitions/items/armor.json", "data/schemas/items/armor.schema.json"],
  ["lights", "data/definitions/items/lights.json", "data/schemas/items/lights.schema.json"],
  ["consumables", "data/definitions/items/consumables.json", "data/schemas/items/consumables.schema.json"],
  ["potions", "data/definitions/items/potions.json", "data/schemas/items/potions.schema.json"],
  ["scrolls", "data/definitions/items/scrolls.json", "data/schemas/items/scrolls.schema.json"],
  ["accessories", "data/definitions/items/accessories.json", "data/schemas/items/accessories.schema.json"],
  ["staves", "data/definitions/items/staves.json", "data/schemas/items/staves.schema.json"],
  ["wands", "data/definitions/items/wands.json", "data/schemas/items/wands.schema.json"],
  ["rods", "data/definitions/items/rods.json", "data/schemas/items/rods.schema.json"],
  ["ego items", "data/definitions/items/ego_items.json", "data/schemas/items/ego_items.schema.json"],
  ["artifacts", "data/definitions/items/artifacts.json", "data/schemas/items/artifacts.schema.json"],
  ["item affixes", "data/definitions/items/item_affixes.json", "data/schemas/items/item_affixes.schema.json"],
  ["flavors", "data/definitions/items/flavors.json", "data/schemas/flavors.schema.json"],
  ["spell books", "data/definitions/magic/spell_books.json", "data/schemas/magic/spell_books.schema.json"],
  ["mage spells", "data/definitions/magic/mage_spells.json", "data/schemas/magic/spells.schema.json"],
  ["priest prayers", "data/definitions/magic/priest_prayers.json", "data/schemas/magic/spells.schema.json"],
  ["races", "data/definitions/character/races.json", "data/schemas/character/races.schema.json"],
  ["classes", "data/definitions/character/classes.json", "data/schemas/character/classes.schema.json"],
  ["race/class rules", "data/definitions/character/race_class_rules.json", "data/schemas/character/race_class_rules.schema.json"],
];

const ajv = new Ajv2020({ strict: false, allErrors: true });
ajv.addSchema(loadJSON("data/schemas/common.schema.json"));
ajv.addSchema(loadJSON("data/schemas/items/common_item.schema.json"));
for (const schemaPath of new Set(contracts.map(([, , path]) => path))) {
  ajv.addSchema(loadJSON(schemaPath));
}
const validators = contracts.map(([label, dataPath, schemaPath]) => ({
  label,
  data: loadJSON(dataPath),
  validate: ajv.getSchema(loadJSON(schemaPath).$id),
}));

const byLabel = new Map(validators.map(contract => [contract.label, contract.data]));
const capabilities = byLabel.get("capabilities").capabilities;
const resistances = byLabel.get("resistances").resistances;
const actions = byLabel.get("actions").actions;
const statuses = byLabel.get("statuses").statuses;
const activations = byLabel.get("activations").activations;
const spellBooks = byLabel.get("spell books").books;
const artifactData = byLabel.get("artifacts").artifacts;

function walk(value, visit) {
  if (Array.isArray(value)) {
    for (const entry of value) walk(entry, visit);
    return;
  }
  if (!value || typeof value !== "object") return;
  visit(value);
  for (const child of Object.values(value)) walk(child, visit);
}

test("every manifest-backed definition validates against its strict schema", () => {
  for (const contract of validators) {
    assert.ok(contract.validate, `${contract.label} schema did not compile`);
    assert.equal(
      contract.validate(contract.data),
      true,
      `${contract.label}: ${JSON.stringify(contract.validate.errors)}`,
    );
  }
});

test("actions have unique IDs and closed, internally consistent parameter contracts", () => {
  assert.equal(new Set(actions.map(action => action.action_id)).size, actions.length);
  for (const action of actions) {
    const parameters = action.parameter_contract.parameters;
    assert.equal(new Set(parameters.map(parameter => parameter.id)).size, parameters.length, action.action_id);
    assert.equal(action.validation.reject_unknown_parameters, true, action.action_id);
    assert.equal(action.validation.require_declared_required_parameters, true, action.action_id);
    assert.equal(action.validation.enforce_declared_value_types, true, action.action_id);
    for (const parameter of parameters) {
      if (parameter.value_type === "enum" || parameter.value_type === "enum_list") {
        assert.ok(Array.isArray(parameter.allowed_values) && parameter.allowed_values.length > 0, `${action.action_id}.${parameter.id} lacks allowed values`);
      }
    }
  }
});

test("action schemas reject unknown IDs and enum parameters without allowed values", () => {
  const contract = validators.find(entry => entry.label === "actions");
  const unknownAction = structuredClone(contract.data);
  unknownAction.actions[0].action_id = "CreateTrap";
  assert.equal(contract.validate(unknownAction), false);

  const missingAllowedValues = structuredClone(contract.data);
  const action = missingAllowedValues.actions.find(entry => entry.parameter_contract.parameters.some(parameter => parameter.value_type === "enum"));
  const parameter = action.parameter_contract.parameters.find(entry => entry.value_type === "enum");
  delete parameter.allowed_values;
  assert.equal(contract.validate(missingAllowedValues), false);
});

test("statuses have unique stable IDs/names and valid stack/duration semantics", () => {
  assert.equal(new Set(statuses.map(status => status.status_id)).size, statuses.length);
  assert.equal(new Set(statuses.map(status => status.name.trim().toLowerCase())).size, statuses.length);
  const ids = new Set(statuses.map(status => status.status_id));
  for (const status of statuses) {
    const duration = status.default_duration;
    assert.ok(status.is_timed && duration, `${status.status_id} must declare timed duration`);
    assert.ok(duration.base >= 0, `${status.status_id} has negative base duration`);
    assert.ok(duration.base > 0 || duration.kind === "dice" || duration.level_multiplier > 0, `${status.status_id} has no duration source`);
    if (duration.kind === "dice") {
      assert.ok(Number.isInteger(duration.count) && duration.count > 0, `${status.status_id} has invalid dice count`);
      assert.ok(Number.isInteger(duration.sides) && duration.sides > 0, `${status.status_id} has invalid dice sides`);
    }
    if (status.is_stackable) assert.ok(["additive", "max", "replace"].includes(status.refresh_policy), status.status_id);
    else assert.equal(status.refresh_policy, "none", status.status_id);
    for (const target of status.migration_target_status_ids ?? []) assert.ok(ids.has(target), `${status.status_id} -> ${target}`);
    if (status.migration_target_status_id) assert.ok(ids.has(status.migration_target_status_id), `${status.status_id} -> ${status.migration_target_status_id}`);
  }

  const invalidStackPolicy = structuredClone(byLabel.get("statuses"));
  const nonStackable = invalidStackPolicy.statuses.find(status => !status.is_stackable);
  nonStackable.refresh_policy = "additive";
  assert.equal(validators.find(contract => contract.label === "statuses").validate(invalidStackPolicy), false);
});

test("capability scopes and resistance semantics remain explicit and distinct", () => {
  const itemSelfCapabilities = new Set(capabilities
    .filter(capability => capability.scope === "item_self_passive")
    .map(capability => capability.id));
  assert.ok(capabilities.every(capability => ["native_identity", "bearer_passive", "item_self_passive"].includes(capability.scope)));
  assert.equal(itemSelfCapabilities.has("ignore_fire"), false);
  assert.equal(resistances.find(resistance => resistance.id === "ignore_fire").semantic_kind, "ignore");

  for (const resistance of resistances) {
    if (resistance.semantic_kind === "ignore") {
      assert.equal(resistance.target_scope, "item_self", resistance.id);
    }
    if (resistance.semantic_kind === "oppose") {
      assert.equal(resistance.target_scope, "bearer", resistance.id);
      assert.equal(typeof resistance.status_id, "string", resistance.id);
    }
  }
  assert.ok(resistances.some(resistance => resistance.semantic_kind === "immunity"));
  assert.ok(resistances.some(resistance => resistance.semantic_kind === "resist"));
});

test("capability/resistance cross-links resolve without merging their semantics", () => {
  const resistanceIds = new Set(resistances.map(resistance => resistance.id));
  const statusIds = new Set(statuses.map(status => status.status_id));
  for (const capability of capabilities) {
    if (["resistance", "item_ignore"].includes(capability.category)) {
      assert.ok(resistanceIds.has(capability.resistance_id), `${capability.id} has unknown resistance ${capability.resistance_id}`);
    }
    if (capability.canonical_owner === "resistance") {
      assert.ok(resistanceIds.has(capability.migration_target_id), `${capability.id} has unknown migration target`);
    }
  }
  for (const resistance of resistances) {
    if (resistance.semantic_kind === "oppose") assert.ok(statusIds.has(resistance.status_id), `${resistance.id} has unknown timed status`);
  }

  const invalidCapability = structuredClone(byLabel.get("capabilities"));
  invalidCapability.capabilities[0].scope = "item_self";
  assert.equal(validators.find(contract => contract.label === "capabilities").validate(invalidCapability), false);

  const invalidItemSelf = structuredClone(byLabel.get("capabilities"));
  const bearerCapability = invalidItemSelf.capabilities.find(capability => capability.scope === "bearer_passive");
  bearerCapability.scope = "item_self_passive";
  assert.equal(validators.find(contract => contract.label === "capabilities").validate(invalidItemSelf), false);
});

test("all definition references resolve by stable IDs", () => {
  const capabilityIds = new Set(capabilities.map(capability => capability.id));
  const resistanceIds = new Set(resistances.map(resistance => resistance.id));
  const actionIds = new Set(actions.map(action => action.action_id));
  const statusIds = new Set(statuses.map(status => status.status_id));
  const monsterCapabilityIds = new Set(byLabel.get("monster capabilities").capabilities.map(capability => capability.id));
  const lootProfileIds = new Set(byLabel.get("monster loot").loot_profiles.map(profile => profile.id));
  const modifierIds = new Set(byLabel.get("combat modifiers").combat_modifiers.map(modifier => modifier.id));
  const bookIds = new Set(byLabel.get("spell books").books.map(book => book.id));
  const spellIds = new Set([
    ...byLabel.get("mage spells").spells.map(spell => spell.id),
    ...byLabel.get("priest prayers").spells.map(spell => spell.id),
  ]);
  const missing = [];

  for (const [, dataPath] of contracts) {
    const document = loadJSON(dataPath);
    walk(document, value => {
      if (typeof value.action_id === "string" && !actionIds.has(value.action_id)) missing.push(`${dataPath}: action ${value.action_id}`);
      if (typeof value.status_id === "string" && !statusIds.has(value.status_id)) missing.push(`${dataPath}: status ${value.status_id}`);
      for (const id of Array.isArray(value.status_ids) ? value.status_ids : []) if (!statusIds.has(id)) missing.push(`${dataPath}: status ${id}`);
      for (const id of Array.isArray(value.capability_ids) ? value.capability_ids : []) if (!capabilityIds.has(id)) missing.push(`${dataPath}: capability ${id}`);
      if (typeof value.capability_id === "string" && !capabilityIds.has(value.capability_id)) missing.push(`${dataPath}: capability ${value.capability_id}`);
      for (const id of Array.isArray(value.resistance_ids) ? value.resistance_ids : []) if (!resistanceIds.has(id)) missing.push(`${dataPath}: resistance ${id}`);
      if (typeof value.resistance_id === "string" && !resistanceIds.has(value.resistance_id)) missing.push(`${dataPath}: resistance ${value.resistance_id}`);
      for (const id of Array.isArray(value.combat_modifiers) ? value.combat_modifiers : []) {
        if (typeof id === "string" && !modifierIds.has(id)) missing.push(`${dataPath}: combat modifier ${id}`);
      }
      if (typeof value.loot_profile === "string" && !lootProfileIds.has(value.loot_profile)) missing.push(`${dataPath}: loot profile ${value.loot_profile}`);
      if (typeof value.monster_capability_id === "string" && !monsterCapabilityIds.has(value.monster_capability_id)) missing.push(`${dataPath}: monster capability ${value.monster_capability_id}`);
      for (const id of Array.isArray(value.spell_ids) ? value.spell_ids : []) if (!spellIds.has(id)) missing.push(`${dataPath}: spell ${id}`);
      if (typeof value.book_id === "string" && !bookIds.has(value.book_id)) missing.push(`${dataPath}: spell book ${value.book_id}`);
    });
  }

  assert.deepEqual(missing, []);
});

test("spell actions and special execution policy are explicit", () => {
  for (const label of ["mage spells", "priest prayers"]) {
    for (const spell of byLabel.get(label).spells) {
      assert.ok(spell.policy, `${spell.id} lacks policy`);
      assert.ok(Array.isArray(spell.actions), `${spell.id} lacks actions`);
      assert.equal(spell.policy.book_id.length > 0, true, `${spell.id} lacks a book ID`);
      assert.equal(spell.policy.realm, byLabel.get(label).realm, `${spell.id} realm mismatch`);
    }
  }
  const wonder = byLabel.get("mage spells").spells.find(spell => spell.id === "magic_wonder");
  assert.equal(wonder.policy.execution_policy, "spell_wonder");
  assert.deepEqual(wonder.actions, []);
  assert.equal(wonder.provenance_status, "unresolved");
});

test("spell book IDs, realms, and canonical action references resolve", () => {
  assert.equal(new Set(spellBooks.map(book => book.id)).size, spellBooks.length);
  const booksById = new Map(spellBooks.map(book => [book.id, book]));
  for (const label of ["mage spells", "priest prayers"]) {
    const catalog = byLabel.get(label);
    assert.equal(catalog.realm, label === "mage spells" ? "magic" : "prayer");
    for (const spell of catalog.spells) {
      const book = booksById.get(spell.policy.book_id);
      assert.ok(book, `${spell.id} references unknown book ${spell.policy.book_id}`);
      assert.equal(book.realm, spell.policy.realm, `${spell.id} book realm mismatch`);
      for (const action of spell.actions) assert.ok(actions.some(known => known.action_id === action.action_id), `${spell.id} references unknown action ${action.action_id}`);
    }
  }
  for (const book of spellBooks) {
    const expectedRealm = book.realm === "magic" ? "mage spells" : "priest prayers";
    const knownSpellIds = new Set(byLabel.get(expectedRealm).spells.map(spell => spell.id));
    for (const spellId of book.spell_ids) assert.ok(knownSpellIds.has(spellId), `${book.id} references unknown spell ${spellId}`);
  }
});

test("activation IDs and artifact activation references resolve", () => {
  assert.equal(new Set(activations.map(activation => activation.id)).size, activations.length);
  const activationIds = new Set(activations.map(activation => activation.id));
  const actionIds = new Set(actions.map(action => action.action_id));
  for (const activation of activations) {
    assert.ok(activation.actions.length > 0, `${activation.id} has no actions`);
    walk(activation.actions, value => {
      if (typeof value.action_id === "string") assert.ok(actionIds.has(value.action_id), `${activation.id} references unknown action ${value.action_id}`);
    });
  }
  for (const artifact of artifactData) {
    assert.deepEqual(artifact.effects.activations, artifact.activation ? [artifact.activation.id] : [], artifact.id);
    for (const id of artifact.effects.activations) assert.ok(activationIds.has(id), `${artifact.id} references unknown activation ${id}`);
  }
});

test("monster loot and changed light definitions retain their typed data", () => {
  const monsters = byLabel.get("monsters").monsters;
  const loot = byLabel.get("monster loot").loot_profiles;
  const profileIds = new Set(loot.map(profile => profile.id));
  assert.equal(monsters.length, 616);
  assert.ok(monsters.every(monster => profileIds.has(monster.loot_profile)));

  const lights = byLabel.get("lights").lights;
  assert.equal(lights.length, 4);
  assert.equal(lights.find(light => light.id === "wooden_torch").fuel_pval, 4000);
  assert.equal(lights.find(light => light.id === "wooden_torch").light_radius, 1);
  assert.equal(lights.find(light => light.id === "dwarven_lantern").fuel_pval, 10000);
  assert.equal(lights.find(light => light.id === "feanorian_lamp").fuel_pval, 10000);
  assert.equal(lights.find(light => light.id === "feanorian_lamp").light_radius, 3);
});

test("schemas reject unknown fields and invalid flat duration dice", () => {
  const statusContract = validators.find(contract => contract.label === "statuses");
  const malformed = structuredClone(statusContract.data);
  malformed.statuses[0].default_duration.count = 0;
  assert.equal(statusContract.validate(malformed), false);

  const artifactContract = validators.find(contract => contract.label === "artifacts");
  const legacy = structuredClone(artifactContract.data);
  legacy.artifacts[0].flags = {};
  assert.equal(artifactContract.validate(legacy), false);
});