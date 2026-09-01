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
            definitions.Add(new ActionDefinition(id, sourceTypes, targetModes));
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

    private static ActionTargetMode? MapTargetMode(string? targetMode) => targetMode switch
    {
        "self" => ActionTargetMode.Self,
        "target" or "ally" or "triggering_actor" => ActionTargetMode.OtherCharacter,
        _ => null,
    };
}
