using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class StatusDefinitionReader
{
    private const string StatusIdProperty = "status_id";
    private const string StatusesDocument = "statuses.json";
    private const string MissingIdError = "missing_id";

    public static List<StatusDefinition> Read(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<StatusDefinition>();
        foreach (var entry in document["statuses"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = entry[StatusIdProperty]?.GetValue<string>();
            var duration = ReadStatusDuration(entry["default_duration"]?.AsObject(), id, report);
            var policy = entry["replacement_policy"]?.GetValue<string>() switch
            {
                "keep_existing" => StatusApplicationPolicy.IgnoreIfPresent,
                "replace_existing" => StatusApplicationPolicy.ReplaceExisting,
                _ => throw new InvalidOperationException($"Status '{id}' has an unsupported replacement policy."),
            };
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add(StatusesDocument, null, StatusIdProperty, MissingIdError, "Definition identifier is required.");
                continue;
            }

            if (duration is not null)
            {
                definitions.Add(new StatusDefinition(id, policy, duration));
            }
        }

        ValidationHelpers.ValidateDuplicates("statuses", definitions, report);
        return definitions;
    }

    private static StatusDurationDefinition? ReadStatusDuration(
        JsonObject? duration,
        string? statusId,
        DefinitionValidationReport report)
    {
        if (duration is null || duration["base"]?.GetValue<int>() is not { } fixedDuration || fixedDuration < 0)
        {
            report.Add(StatusesDocument, statusId, "default_duration", "invalid_duration", "Status default duration must declare a non-negative base duration.");
            return null;
        }

        var kind = duration["kind"]?.GetValue<string>();
        var diceCount = duration["count"]?.GetValue<int>();
        var diceSides = duration["sides"]?.GetValue<int>();
        var levelMultiplier = duration["level_multiplier"]?.GetValue<int>();
        var hasDice = kind is not null || diceCount is not null || diceSides is not null;
        if ((hasDice && (kind != "dice" || diceCount is not > 0 || diceSides is not > 0)) ||
            (levelMultiplier is <= 0) ||
            (fixedDuration == 0 && !hasDice && levelMultiplier is null))
        {
            report.Add(StatusesDocument, statusId, "default_duration", "invalid_duration", "Status duration dice and level scaling must be complete and positive when present.");
            return null;
        }

        return new StatusDurationDefinition(
            fixedDuration,
            diceCount,
            diceSides,
            levelMultiplier);
    }
}
