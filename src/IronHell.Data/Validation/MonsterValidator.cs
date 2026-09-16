using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class MonsterValidator
{
    private const string MonsterAbilitiesDocument = "monsters/monster_abilities.json";
    private const string MonstersDocument = "monsters/monsters.json";
    private const string AbilitiesProperty = "abilities";
    private const string CapabilitiesProperty = "capabilities";
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
            ValidateHpRoll(monster, monsterId, report);
            ValidateAi(monster, monsterId, report);
            foreach (var abilityId in monster[AbilitiesProperty]?.AsArray() ?? [])
            {
                ValidationHelpers.ValidateReference(abilityId?.GetValue<string>(), registries.MonsterAbilities, MonstersDocument, monsterId, AbilitiesProperty, "unknown_monster_ability", report);
            }

            var seenCapabilities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var capabilityIdNode in monster[CapabilitiesProperty]?.AsArray() ?? [])
            {
                var capabilityId = capabilityIdNode?.GetValue<string>();
                ValidationHelpers.ValidateReference(capabilityId, registries.MonsterCapabilities, MonstersDocument, monsterId, CapabilitiesProperty, "unknown_monster_capability", report);
                if (capabilityId is not null && !seenCapabilities.Add(capabilityId))
                {
                    report.Add(MonstersDocument, monsterId, CapabilitiesProperty, "duplicate_monster_capability", $"Monster capability '{capabilityId}' is duplicated.");
                }
            }

            ValidateMonsterNode(monster, monsterId, registries, report);
        }
    }

    private static void ValidateAi(JsonObject monster, string monsterId, DefinitionValidationReport report)
    {
        if (monster["ai"] is not JsonObject ai || string.IsNullOrWhiteSpace(ai["behavior"]?.GetValue<string>()))
        {
            report.Add(MonstersDocument, monsterId, "ai.behavior", "missing_monster_ai", "Monster AI behavior is required.");
            return;
        }

        if (ai["random_move_chance"]?.GetValue<int>() is not >= 0 or > 100)
        {
            report.Add(MonstersDocument, monsterId, "ai.random_move_chance", "invalid_random_move_chance", "Monster random movement chance must be between 0 and 100.");
        }
    }

    private static void ValidateHpRoll(JsonObject monster, string monsterId, DefinitionValidationReport report)
    {
        if (monster["hp_roll"] is not JsonObject hpRoll)
        {
            report.Add(MonstersDocument, monsterId, "hp_roll", "missing_hp_roll", "Monster HP roll is required.");
            return;
        }

        if (hpRoll["kind"]?.GetValue<string>() != "dice" ||
            hpRoll["count"]?.GetValue<int>() is not > 0 ||
            hpRoll["sides"]?.GetValue<int>() is not > 0)
        {
            report.Add(MonstersDocument, monsterId, "hp_roll", "invalid_hp_roll", "Monster HP roll must use positive dice count and sides.");
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
