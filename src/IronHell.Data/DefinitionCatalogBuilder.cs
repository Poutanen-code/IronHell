using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;
using IronHell.Data.Serialization;
using IronHell.Data.Validation;

namespace IronHell.Data;

internal static class DefinitionCatalogBuilder
{
    public static IDefinitionCatalog? Build(FrozenDictionary<string, JsonObject> documents, DefinitionValidationReport report)
    {
        var actions = ActionDefinitionReader.Read(documents["actions"], report);
        var statuses = StatusDefinitionReader.Read(documents["statuses"], report);
        var capabilities = SimpleDefinitionReader.Read<CapabilityDefinition>(documents["capabilities"], "capabilities", "id", id => new CapabilityDefinition(id), report);
        var resistances = SimpleDefinitionReader.Read<ResistanceDefinition>(documents["resistances"], "resistances", "id", id => new ResistanceDefinition(id), report);
        var items = ItemDefinitionReader.Read(documents, report);
        var mageSpells = SpellDefinitionReader.Read(documents["mage_spells"], report);
        var priestPrayers = SpellDefinitionReader.Read(documents["priest_prayers"], report);
        var activations = SimpleDefinitionReader.Read<ActivationDefinition>(documents["activations"], "activations", "activation_id", id => new ActivationDefinition(id), report);
        var monsterAbilities = MonsterDefinitionReader.ReadAbilities(documents["monster_abilities"], report);
        var monsters = MonsterDefinitionReader.ReadMonsters(documents["monsters"], report);
        var terrain = TerrainDefinitionReader.Read(documents["terrain"], report);
        var traps = TrapDefinitionReader.Read(documents["traps"], report);
        var races = CharacterDefinitionReader.ReadRaces(documents["races"], report);
        var classes = CharacterDefinitionReader.ReadClasses(documents["classes"], report);
        var rules = CharacterDefinitionReader.ReadRules(documents["race_class_rules"], report);

        if (report.HasErrors)
        {
            return null;
        }

        var actionRegistry = new DefinitionRegistry<ActionDefinition>(actions);
        var statusRegistry = new DefinitionRegistry<StatusDefinition>(statuses);
        var capabilityRegistry = new DefinitionRegistry<CapabilityDefinition>(capabilities);
        var resistanceRegistry = new DefinitionRegistry<ResistanceDefinition>(resistances);
        var raceRegistry = new DefinitionRegistry<RaceDefinition>(races);
        var classRegistry = new DefinitionRegistry<ClassDefinition>(classes);
        var itemRegistry = new DefinitionRegistry<ItemDefinition>(items);
        var mageSpellRegistry = new DefinitionRegistry<SpellDefinition>(mageSpells);
        var priestPrayerRegistry = new DefinitionRegistry<SpellDefinition>(priestPrayers);
        var activationRegistry = new DefinitionRegistry<ActivationDefinition>(activations);
        var monsterAbilityRegistry = new DefinitionRegistry<MonsterAbilityDefinition>(monsterAbilities);
        var monsterRegistry = new DefinitionRegistry<MonsterDefinition>(monsters);
        var terrainRegistry = new DefinitionRegistry<TerrainDefinition>(terrain);
        var trapRegistry = new DefinitionRegistry<TrapDefinition>(traps);

        var characterDefinitions = new CharacterDefinitionSet(rules, races, classes);
        CharacterValidator.Validate(raceRegistry, classRegistry, capabilityRegistry, itemRegistry, characterDefinitions, report);

        var validationRegistries = new ValidationRegistries(actionRegistry, statusRegistry, capabilityRegistry, resistanceRegistry, itemRegistry, mageSpellRegistry, priestPrayerRegistry, activationRegistry, monsterAbilityRegistry, monsterRegistry, terrainRegistry, trapRegistry);
        CoreCatalogValidator.ValidateCapabilities(documents["capabilities"], resistanceRegistry, report);
        CoreCatalogValidator.ValidateResistances(documents["resistances"], statusRegistry, report);
        CoreCatalogValidator.ValidateStatuses(documents["statuses"], statusRegistry, report);
        ItemValidator.Validate(documents, actionRegistry, statusRegistry, capabilityRegistry, resistanceRegistry, report);
        ItemValidator.ValidateEgoCombatModifiers(documents["ego_items"], documents["combat_modifiers"], report);
        ItemValidator.ValidateArtifactCombatModifiers(documents["artifacts"], documents["combat_modifiers"], report);
        ItemValidator.ValidateItemAffixes(documents, report);
        ItemValidator.ValidateArtifactEffects(documents, report);
        SpellValidator.Validate(documents, validationRegistries, report);
        ActivationValidator.Validate(documents["activations"], validationRegistries, report);
        MonsterValidator.ValidateAbilities(documents["monster_abilities"], validationRegistries, report);
        MonsterValidator.ValidateMonsters(documents["monsters"], validationRegistries, report);
        CombatModifierValidator.Validate(documents["combat_modifiers"], documents["monsters"], report);
        TerrainValidator.Validate(documents["terrain"], terrainRegistry, report);
        TrapValidator.Validate(documents["traps"], validationRegistries, report);

        if (report.HasErrors)
        {
            return null;
        }

        return new DefinitionCatalog(
            actionRegistry,
            statusRegistry,
            capabilityRegistry,
            resistanceRegistry,
            raceRegistry,
            classRegistry,
            itemRegistry,
            mageSpellRegistry,
            priestPrayerRegistry,
            activationRegistry,
            monsterAbilityRegistry,
            monsterRegistry,
            terrainRegistry,
            trapRegistry,
            Array.AsReadOnly(rules.OrderBy(rule => rule.RaceId, StringComparer.Ordinal).ThenBy(rule => rule.ClassId, StringComparer.Ordinal).ToArray()));
    }
}