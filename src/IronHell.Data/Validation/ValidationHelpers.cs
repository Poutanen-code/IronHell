using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class ValidationHelpers
{
    private const string StatusIdProperty = "status_id";
    private const string UnknownStatusError = "unknown_status";
    private const string ActionIdProperty = "action_id";
    private const string CapabilityIdsProperty = "capability_ids";
    private const string UnknownCapabilityError = "unknown_capability";

    public static readonly FrozenSet<string> LegacyCapabilityIds = new[]
    {
        "regeneration", "searching", "slay_undead", "stealth",
    }.ToFrozenSet(StringComparer.Ordinal);

    public static void ValidateDuplicates<T>(string catalog, IEnumerable<T> definitions, DefinitionValidationReport report) where T : IIdentifiedDefinition
    {
        foreach (var duplicateId in definitions.GroupBy(definition => definition.Id, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key))
        {
            report.Add($"{catalog}.json", duplicateId, "id", "duplicate_id", $"Duplicate identifier '{duplicateId}'.");
        }
    }

    public static bool ValidateReference<T>(
        string? id,
        IDefinitionRegistry<T> registry,
        string documentPath,
        string definitionId,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return true;
        }

        if (registry.TryGet(id, out _))
        {
            return true;
        }

        report.Add(documentPath, definitionId, property, errorCode, $"Reference '{id}' does not resolve in {typeof(T).Name}.");
        return false;
    }

    public static void ValidateCapabilityReference(
        string? id,
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        string documentPath,
        string definitionId,
        DefinitionValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(id) || capabilities.TryGet(id, out _) || resistances.TryGet(id, out _) || LegacyCapabilityIds.Contains(id))
        {
            return;
        }

        report.Add(documentPath, definitionId, CapabilityIdsProperty, UnknownCapabilityError, $"Capability '{id}' does not resolve.");
    }

    public static void ValidateActionReference(
        JsonObject action,
        IDefinitionRegistry<ActionDefinition> actions,
        IDefinitionRegistry<StatusDefinition> statuses,
        string documentPath,
        string definitionId,
        DefinitionValidationReport report)
    {
        var actionId = action[ActionIdProperty]?.GetValue<string>();
        if (!ValidateReference(actionId, actions, documentPath, definitionId, "actions.action_id", "unknown_action", report))
        {
            return;
        }

        var parameters = action["parameters"]?.AsObject();
        ValidateReference(parameters?[StatusIdProperty]?.GetValue<string>(), statuses, documentPath, definitionId, "actions.parameters.status_id", UnknownStatusError, report);
        foreach (var statusId in parameters?["status_ids"]?.AsArray() ?? [])
        {
            ValidateReference(statusId?.GetValue<string>(), statuses, documentPath, definitionId, "actions.parameters.status_ids", UnknownStatusError, report);
        }
    }
}
