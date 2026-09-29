using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class CoreCatalogValidator
{
    private const string ResistanceIdProperty = "resistance_id";
    private const string UnknownResistanceError = "unknown_resistance";
    private const string StatusIdProperty = "status_id";
    private const string UnknownStatusError = "unknown_status";

    public static void ValidateCapabilities(
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        DefinitionValidationReport report)
    {
        foreach (var capability in capabilities.All)
        {
            ValidationHelpers.ValidateReference(capability.ResistanceId, resistances, "capabilities.json", capability.Id, ResistanceIdProperty, UnknownResistanceError, report);
            if (capability.CanonicalOwner == "resistance")
            {
                ValidationHelpers.ValidateReference(capability.MigrationTargetId, resistances, "capabilities.json", capability.Id, "migration_target_id", UnknownResistanceError, report);
            }

            if (capability.ResistanceId is { } resistanceId && resistances.TryGet(resistanceId, out var resistance))
            {
                var capabilityIsItemSelf = capability.Scope == CapabilityScope.ItemSelfPassive;
                var resistanceIsItemSelfIgnore = resistance.SemanticKind == ResistanceSemanticKind.Ignore &&
                    resistance.TargetScope == ResistanceTargetScope.ItemSelf;
                if (capabilityIsItemSelf != resistanceIsItemSelfIgnore || resistance.SemanticKind == ResistanceSemanticKind.Oppose)
                {
                    report.Add("capabilities.json", capability.Id, ResistanceIdProperty, "capability_resistance_scope_mismatch", "Capability and resistance scopes/semantics must agree; timed Oppose belongs to Statuses.");
                }
            }
        }
    }

    public static void ValidateResistances(
        IDefinitionRegistry<ResistanceDefinition> resistances,
        IDefinitionRegistry<StatusDefinition> statuses,
        DefinitionValidationReport report)
    {
        foreach (var resistance in resistances.All)
        {
            ValidationHelpers.ValidateReference(resistance.StatusId, statuses, "resistances.json", resistance.Id, StatusIdProperty, UnknownStatusError, report);
            if (resistance.SemanticKind == ResistanceSemanticKind.Oppose && string.IsNullOrWhiteSpace(resistance.StatusId))
            {
                report.Add("resistances.json", resistance.Id, StatusIdProperty, "missing_oppose_status", "Oppose resistance must reference its timed Status definition.");
            }
        }
    }

    public static void ValidateStatuses(
        JsonObject document,
        IDefinitionRegistry<StatusDefinition> statuses,
        DefinitionValidationReport report)
    {
        foreach (var status in document["statuses"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = status[StatusIdProperty]?.GetValue<string>() ?? string.Empty;
            ValidationHelpers.ValidateReference(status["migration_target_status_id"]?.GetValue<string>(), statuses, "statuses.json", id, "migration_target_status_id", UnknownStatusError, report);
            foreach (var target in status["migration_target_status_ids"]?.AsArray() ?? [])
            {
                ValidationHelpers.ValidateReference(target?.GetValue<string>(), statuses, "statuses.json", id, "migration_target_status_ids", UnknownStatusError, report);
            }
        }
    }
}
