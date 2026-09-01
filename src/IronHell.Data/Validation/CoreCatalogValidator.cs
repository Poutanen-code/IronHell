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
        JsonObject document,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        DefinitionValidationReport report)
    {
        foreach (var capability in document["capabilities"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = capability["id"]?.GetValue<string>() ?? string.Empty;
            ValidationHelpers.ValidateReference(capability[ResistanceIdProperty]?.GetValue<string>(), resistances, "capabilities.json", id, ResistanceIdProperty, UnknownResistanceError, report);
            if (capability["canonical_owner"]?.GetValue<string>() == "resistance")
            {
                ValidationHelpers.ValidateReference(capability["migration_target_id"]?.GetValue<string>(), resistances, "capabilities.json", id, "migration_target_id", UnknownResistanceError, report);
            }
        }
    }

    public static void ValidateResistances(
        JsonObject document,
        IDefinitionRegistry<StatusDefinition> statuses,
        DefinitionValidationReport report)
    {
        foreach (var resistance in document["resistances"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = resistance["id"]?.GetValue<string>() ?? string.Empty;
            ValidationHelpers.ValidateReference(resistance[StatusIdProperty]?.GetValue<string>(), statuses, "resistances.json", id, StatusIdProperty, UnknownStatusError, report);
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
