using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class ActionDefinitionReader
{
    private const string ActionIdProperty = "action_id";
    private const string MissingIdError = "missing_id";

    public static List<ActionDefinition> Read(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<ActionDefinition>();
        foreach (var entry in document["actions"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = entry[ActionIdProperty]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add("actions.json", null, ActionIdProperty, MissingIdError, "Definition identifier is required.");
                continue;
            }

            var sourceTypes = entry["allowed_source_families"]?.AsArray()
                .Select(value => value?.GetValue<string>())
                .Where(value => value is not null)
                .Select(MapSourceType)
                .Where(value => value is not null)
                .Select(value => value!.Value)
                .ToHashSet();
            var targetModes = entry["parameter_contract"]?["parameters"]?.AsArray()
                .OfType<JsonObject>()
                .FirstOrDefault(parameter => parameter["id"]?.GetValue<string>() == "target_mode")?["allowed_values"]?.AsArray()
                .Select(value => value?.GetValue<string>())
                .Where(value => value is not null)
                .Select(MapTargetMode)
                .Where(value => value is not null)
                .Select(value => value!.Value)
                .ToHashSet();
            definitions.Add(new ActionDefinition(
                id,
                sourceTypes,
                targetModes,
                entry["name"]?.GetValue<string>() ?? string.Empty,
                entry["description"]?.GetValue<string>() ?? string.Empty,
                MapCategory(entry["category"]?.GetValue<string>()),
                MapConfidence(entry["confidence"]?.GetValue<string>()),
                entry["allowed_source_families"]?.AsArray()
                    .Select(value => MapSourceFamily(value?.GetValue<string>()))
                    .Where(value => value is not null)
                    .Select(value => value!.Value)
                    .ToHashSet(),
                ReadParameterContract(entry["parameter_contract"]?.AsObject()),
                ReadValidationRules(entry["validation"]?.AsObject()),
                entry["provenance_status"]?.GetValue<string>(),
                entry["provenance"]?["summary"]?.GetValue<string>() is { } summary
                    ? new ActionProvenance(summary)
                    : null));
        }

        ValidationHelpers.ValidateDuplicates("actions", definitions, report);
        return definitions;
    }

    private static ActionSourceType? MapSourceType(string? sourceFamily) => sourceFamily switch
    {
        "spell" => ActionSourceType.CharacterSpell,
        "prayer" => ActionSourceType.CharacterPrayer,
        "activation" => ActionSourceType.ItemActivation,
        "monster_ability" => ActionSourceType.MonsterAbility,
        "trap" => ActionSourceType.Trap,
        _ => null,
    };

    private static ActionSourceFamily? MapSourceFamily(string? sourceFamily) => sourceFamily switch
    {
        "spell" => ActionSourceFamily.Spell,
        "prayer" => ActionSourceFamily.Prayer,
        "monster_attack" => ActionSourceFamily.MonsterAttack,
        "monster_ability" => ActionSourceFamily.MonsterAbility,
        "rod" => ActionSourceFamily.Rod,
        "wand" => ActionSourceFamily.Wand,
        "staff" => ActionSourceFamily.Staff,
        "potion" => ActionSourceFamily.Potion,
        "scroll" => ActionSourceFamily.Scroll,
        "consumable" => ActionSourceFamily.Consumable,
        "activation" => ActionSourceFamily.Activation,
        "trap" => ActionSourceFamily.Trap,
        "chest" => ActionSourceFamily.Chest,
        "treasure" => ActionSourceFamily.Treasure,
        "death_drop" => ActionSourceFamily.DeathDrop,
        "environmental" => ActionSourceFamily.Environmental,
        "item_property" => ActionSourceFamily.ItemProperty,
        "monster_blow" => ActionSourceFamily.MonsterBlow,
        "monster_spell" => ActionSourceFamily.MonsterSpell,
        _ => null,
    };

    private static ActionCategory MapCategory(string? category) => category switch
    {
        "damage" => ActionCategory.Damage,
        "healing" => ActionCategory.Healing,
        "status" => ActionCategory.Status,
        "movement" => ActionCategory.Movement,
        "information" => ActionCategory.Information,
        "item" => ActionCategory.Item,
        "terrain" => ActionCategory.Terrain,
        "control" => ActionCategory.Control,
        "summoning" => ActionCategory.Summoning,
        "attribute" => ActionCategory.Attribute,
        "progression" => ActionCategory.Progression,
        _ => ActionCategory.Utility,
    };

    private static ActionConfidence MapConfidence(string? confidence) => confidence == "high"
        ? ActionConfidence.High
        : ActionConfidence.Medium;

    private static ActionParameterContract? ReadParameterContract(JsonObject? value)
    {
        if (value is null) return null;
        var parameters = value["parameters"]?.AsArray().OfType<JsonObject>().Select(parameter => new ActionParameterDefinition(
            parameter["id"]?.GetValue<string>() ?? string.Empty,
            parameter["description"]?.GetValue<string>() ?? string.Empty,
            MapValueType(parameter["value_type"]?.GetValue<string>()),
            parameter["required"]?.GetValue<bool>() ?? false,
            parameter["reference_domain"]?.GetValue<string>(),
            parameter["allowed_values"]?.AsArray().Select(item => item?.GetValue<string>()).Where(item => item is not null).Select(item => item!).ToHashSet(StringComparer.Ordinal),
            parameter["minimum"]?.GetValue<int>(),
            parameter["maximum"]?.GetValue<int>(),
            parameter["min_items"]?.GetValue<int>(),
            parameter["max_items"]?.GetValue<int>(),
            parameter["notes"]?.GetValue<string>())).ToArray() ?? [];
        return new ActionParameterContract(value["closed"]?.GetValue<bool>() ?? false, Array.AsReadOnly(parameters));
    }

    private static ActionValidationRules? ReadValidationRules(JsonObject? value) => value is null
        ? null
        : new ActionValidationRules(
            value["reject_unknown_parameters"]?.GetValue<bool>() ?? false,
            value["require_declared_required_parameters"]?.GetValue<bool>() ?? false,
            value["enforce_declared_value_types"]?.GetValue<bool>() ?? false);

    private static ActionParameterValueType MapValueType(string? value) => value switch
    {
        "integer" => ActionParameterValueType.Integer,
        "boolean" => ActionParameterValueType.Boolean,
        "id" => ActionParameterValueType.Id,
        "id_list" => ActionParameterValueType.IdList,
        "enum" => ActionParameterValueType.Enum,
        "enum_list" => ActionParameterValueType.EnumList,
        "token" => ActionParameterValueType.Token,
        "structured_amount" => ActionParameterValueType.StructuredAmount,
        "duration" => ActionParameterValueType.Duration,
        _ => throw new InvalidOperationException($"Unsupported action parameter value type '{value}'."),
    };

    private static ActionTargetMode? MapTargetMode(string? targetMode) => targetMode switch
    {
        "self" => ActionTargetMode.Self,
        "target" or "ally" or "triggering_actor" => ActionTargetMode.OtherCharacter,
        _ => null,
    };
}
