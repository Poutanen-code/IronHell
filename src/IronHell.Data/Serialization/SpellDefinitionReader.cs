using System.Text.Json.Nodes;
using System.Text.Json;
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

            definitions.Add(new SpellDefinition(id, ReadActionRefs(entry), ReadPolicy(entry)));
        }

        ValidationHelpers.ValidateDuplicates("spells", definitions, report);
        return definitions;
    }

    private static IReadOnlyList<SpellActionRef> ReadActionRefs(JsonObject spell)
    {
        var actionRefs = new List<SpellActionRef>();
        foreach (var actionRef in spell["actions"]?.AsArray().OfType<JsonObject>() ?? [])
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
                ReadStatusIds(parameters?["status_ids"]?.AsArray()),
                ReadParameters(parameters)));
        }

        return Array.AsReadOnly(actionRefs.ToArray());
    }

    private static SpellPolicyDefinition? ReadPolicy(JsonObject spell)
    {
        var policy = spell["policy"] as JsonObject;
        return policy is null
            ? null
            : new SpellPolicyDefinition(
                policy["level"]?.GetValue<int>() ?? 0,
                policy["mana"]?.GetValue<int>() ?? 0,
                policy["fail_rate"]?.GetValue<int>() ?? 0,
                policy["experience_value"]?.GetValue<int>() ?? 0,
                policy["book_id"]?.GetValue<string>() ?? string.Empty,
                policy["realm"]?.GetValue<string>() ?? string.Empty,
                policy["execution_policy"]?.GetValue<string>());
    }

    private static SpellActionAmount? ReadAmount(JsonObject? amount) => amount is null
        ? null
        : new SpellActionAmount(
            amount["kind"]?.GetValue<string>() ?? string.Empty,
            ReadInteger(amount["value"]),
            ReadInteger(amount["count"]),
            ReadInteger(amount["sides"]));

    private static int? ReadInteger(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<int>(out var result) ? result : null;

    private static JsonElement? ReadParameters(JsonObject? parameters)
    {
        if (parameters is null)
        {
            return null;
        }

        using var document = JsonDocument.Parse(parameters.ToJsonString());
        return document.RootElement.Clone();
    }

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
