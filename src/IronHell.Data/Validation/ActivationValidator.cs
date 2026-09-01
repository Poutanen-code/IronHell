using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class ActivationValidator
{
    private const string ActivationsDocument = "activations.json";
    private const string ActivationIdProperty = "activation_id";
    private const string ActionIdProperty = "action_id";
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
        foreach (var activation in document["activations"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var activationId = activation[ActivationIdProperty]?.GetValue<string>() ?? string.Empty;
            ValidateActivationNode(activation, activationId, registries, report);
        }
    }

    private static void ValidateActivationNode(
        JsonNode? node,
        string activationId,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        if (node is JsonObject value)
        {
            if (value[ActionIdProperty] is not null)
            {
                ValidationHelpers.ValidateActionReference(value, registries.Actions, registries.Statuses, ActivationsDocument, activationId, report);
            }

            ValidationHelpers.ValidateReference(value[CapabilityIdProperty]?.GetValue<string>(), registries.Capabilities, ActivationsDocument, activationId, CapabilityIdProperty, UnknownCapabilityError, report);
            ValidationHelpers.ValidateReference(value[ResistanceIdProperty]?.GetValue<string>(), registries.Resistances, ActivationsDocument, activationId, ResistanceIdProperty, UnknownResistanceError, report);
            ValidateReferenceList(value[CapabilityIdsProperty]?.AsArray(), registries.Capabilities, activationId, CapabilityIdsProperty, UnknownCapabilityError, report);
            ValidateReferenceList(value[ResistanceIdsProperty]?.AsArray(), registries.Resistances, activationId, ResistanceIdsProperty, UnknownResistanceError, report);

            foreach (var property in value)
            {
                ValidateActivationNode(property.Value, activationId, registries, report);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var arrayItem in array)
            {
                ValidateActivationNode(arrayItem, activationId, registries, report);
            }
        }
    }

    private static void ValidateReferenceList<T>(
        JsonArray? references,
        IDefinitionRegistry<T> registry,
        string activationId,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        foreach (var reference in references ?? [])
        {
            ValidationHelpers.ValidateReference(reference?.GetValue<string>(), registry, ActivationsDocument, activationId, property, errorCode, report);
        }
    }
}
