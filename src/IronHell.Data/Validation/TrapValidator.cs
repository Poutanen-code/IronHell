using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class TrapValidator
{
    private const string TrapsDocument = "environment/traps.json";
    private const string ActionRefsProperty = "action_refs";
    private const string CapabilityIdProperty = "capability_id";
    private const string ResistanceIdProperty = "resistance_id";
    private const string CapabilityIdsProperty = "capability_ids";
    private const string ResistanceIdsProperty = "resistance_ids";
    private const string UnknownCapabilityError = "unknown_capability";
    private const string UnknownResistanceError = "unknown_resistance";

    public static void Validate(
        JsonObject document,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var trap in document["traps"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var trapId = trap["id"]?.GetValue<string>() ?? string.Empty;
            foreach (var action in trap[ActionRefsProperty]?.AsArray().OfType<JsonObject>() ?? [])
            {
                ValidationHelpers.ValidateActionReference(action, registries.Actions, registries.Statuses, TrapsDocument, trapId, report);
            }

            ValidationHelpers.ValidateReference(trap[CapabilityIdProperty]?.GetValue<string>(), registries.Capabilities, TrapsDocument, trapId, CapabilityIdProperty, UnknownCapabilityError, report);
            ValidationHelpers.ValidateReference(trap[ResistanceIdProperty]?.GetValue<string>(), registries.Resistances, TrapsDocument, trapId, ResistanceIdProperty, UnknownResistanceError, report);
            ValidateReferenceList(trap[CapabilityIdsProperty]?.AsArray(), registries.Capabilities, trapId, CapabilityIdsProperty, UnknownCapabilityError, report);
            ValidateReferenceList(trap[ResistanceIdsProperty]?.AsArray(), registries.Resistances, trapId, ResistanceIdsProperty, UnknownResistanceError, report);
        }
    }

    private static void ValidateReferenceList<T>(
        JsonArray? references,
        IDefinitionRegistry<T> registry,
        string trapId,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        foreach (var reference in references ?? [])
        {
            ValidationHelpers.ValidateReference(reference?.GetValue<string>(), registry, TrapsDocument, trapId, property, errorCode, report);
        }
    }
}
