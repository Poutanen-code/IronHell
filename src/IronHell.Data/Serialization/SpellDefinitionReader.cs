using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class SpellDefinitionReader
{
    public static List<SpellDefinition> Read(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<SpellDefinition>();
        foreach (var entry in document["spells"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = entry["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add("spells.json", null, "id", "missing_id", "Definition identifier is required.");
                continue;
            }

            definitions.Add(new SpellDefinition(id, ReadActionRefs(entry)));
        }

        ValidationHelpers.ValidateDuplicates("spells", definitions, report);
        return definitions;
    }

    private static IReadOnlyList<SpellActionRef> ReadActionRefs(JsonObject spell)
    {
        var actionRefs = new List<SpellActionRef>();
        foreach (var actionRef in spell["action_refs"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var actionId = actionRef["action_id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(actionId))
            {
                continue;
            }

            var parameters = actionRef["parameters"]?.AsObject();
            actionRefs.Add(new SpellActionRef(
                actionId,
                MapTargetMode(parameters?["target_mode"]?.GetValue<string>()),
                ReadAmount(parameters?["amount"]?.AsObject()),
                parameters?["status_id"]?.GetValue<string>(),
                ReadStatusIds(parameters?["status_ids"]?.AsArray())));
        }

        return Array.AsReadOnly(actionRefs.ToArray());
    }

    private static SpellActionAmount? ReadAmount(JsonObject? amount) => amount is null
        ? null
        : new SpellActionAmount(
            amount["kind"]?.GetValue<string>() ?? string.Empty,
            amount["value"]?.GetValue<int>(),
            amount["count"]?.GetValue<int>(),
            amount["sides"]?.GetValue<int>());

    private static IReadOnlyList<string>? ReadStatusIds(JsonArray? statusIds) => statusIds is null
        ? null
        : Array.AsReadOnly(statusIds.Select(statusId => statusId?.GetValue<string>() ?? string.Empty).ToArray());

    // Spell payloads without an explicit target mode resolve against the caster.
    private static ActionTargetMode MapTargetMode(string? targetMode) => targetMode switch
    {
        "target" or "ally" or "triggering_actor" => ActionTargetMode.OtherCharacter,
        _ => ActionTargetMode.Self,
    };
}
