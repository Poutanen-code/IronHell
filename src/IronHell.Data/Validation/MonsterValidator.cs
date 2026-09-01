using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class MonsterValidator
{
    private const string MonsterAbilitiesDocument = "monsters/monster_abilities.json";
    private const string MonstersDocument = "monsters/monsters.json";
    private const string AbilitiesProperty = "abilities";
    private const string ActionIdProperty = "action_id";
    private const string CapabilityIdProperty = "capability_id";
    private const string ResistanceIdProperty = "resistance_id";
    private const string CapabilityIdsProperty = "capability_ids";
    private const string ResistanceIdsProperty = "resistance_ids";
    private const string UnknownCapabilityError = "unknown_capability";
    private const string UnknownResistanceError = "unknown_resistance";

    public static void ValidateAbilities(
        JsonObject document,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var ability in document[AbilitiesProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var abilityId = ability["id"]?.GetValue<string>() ?? string.Empty;
            foreach (var action in ability["action_refs"]?.AsArray().OfType<JsonObject>() ?? [])
            {
                ValidationHelpers.ValidateActionReference(action, registries.Actions, registries.Statuses, MonsterAbilitiesDocument, abilityId, report);
            }

            ValidationHelpers.ValidateReference(ability[CapabilityIdProperty]?.GetValue<string>(), registries.Capabilities, MonsterAbilitiesDocument, abilityId, CapabilityIdProperty, UnknownCapabilityError, report);
            ValidationHelpers.ValidateReference(ability[ResistanceIdProperty]?.GetValue<string>(), registries.Resistances, MonsterAbilitiesDocument, abilityId, ResistanceIdProperty, UnknownResistanceError, report);
            ValidateReferenceList(ability[CapabilityIdsProperty]?.AsArray(), registries.Capabilities, abilityId, MonsterAbilitiesDocument, CapabilityIdsProperty, UnknownCapabilityError, report);
            ValidateReferenceList(ability[ResistanceIdsProperty]?.AsArray(), registries.Resistances, abilityId, MonsterAbilitiesDocument, ResistanceIdsProperty, UnknownResistanceError, report);
        }
    }

    public static void ValidateMonsters(
        JsonObject document,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var monster in document["monsters"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var monsterId = monster["id"]?.GetValue<string>() ?? string.Empty;
            foreach (var abilityId in monster[AbilitiesProperty]?.AsArray() ?? [])
            {
                ValidationHelpers.ValidateReference(abilityId?.GetValue<string>(), registries.MonsterAbilities, MonstersDocument, monsterId, AbilitiesProperty, "unknown_monster_ability", report);
            }

            ValidateMonsterNode(monster, monsterId, registries, report);
        }
    }

    private static void ValidateMonsterNode(
        JsonNode? node,
        string monsterId,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        if (node is JsonObject value)
        {
            if (value[ActionIdProperty] is not null)
            {
                ValidationHelpers.ValidateActionReference(value, registries.Actions, registries.Statuses, MonstersDocument, monsterId, report);
            }

            ValidationHelpers.ValidateReference(value[CapabilityIdProperty]?.GetValue<string>(), registries.Capabilities, MonstersDocument, monsterId, CapabilityIdProperty, UnknownCapabilityError, report);
            ValidationHelpers.ValidateReference(value[ResistanceIdProperty]?.GetValue<string>(), registries.Resistances, MonstersDocument, monsterId, ResistanceIdProperty, UnknownResistanceError, report);
            ValidateReferenceList(value[CapabilityIdsProperty]?.AsArray(), registries.Capabilities, monsterId, MonstersDocument, CapabilityIdsProperty, UnknownCapabilityError, report);
            ValidateReferenceList(value[ResistanceIdsProperty]?.AsArray(), registries.Resistances, monsterId, MonstersDocument, ResistanceIdsProperty, UnknownResistanceError, report);

            foreach (var property in value)
            {
                ValidateMonsterNode(property.Value, monsterId, registries, report);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                ValidateMonsterNode(item, monsterId, registries, report);
            }
        }
    }

    private static void ValidateReferenceList<T>(
        JsonArray? references,
        IDefinitionRegistry<T> registry,
        string definitionId,
        string documentPath,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        foreach (var reference in references ?? [])
        {
            ValidationHelpers.ValidateReference(reference?.GetValue<string>(), registry, documentPath, definitionId, property, errorCode, report);
        }
    }
}
