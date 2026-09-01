using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;
using IronHell.Data.Serialization;

namespace IronHell.Data.Validation;

internal static class ItemValidator
{
    private const string CapabilityIdsProperty = "capability_ids";

    public static void Validate(
        FrozenDictionary<string, JsonObject> documents,
        IDefinitionRegistry<ActionDefinition> actions,
        IDefinitionRegistry<StatusDefinition> statuses,
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        DefinitionValidationReport report)
    {
        foreach (var catalog in ItemDefinitionReader.ItemCatalogs.Where(catalog => catalog.Category != ItemCategory.SpellBook))
        {
            foreach (var item in documents[catalog.DocumentName][catalog.CollectionName]?.AsArray().OfType<JsonObject>() ?? [])
            {
                var id = item["id"]?.GetValue<string>() ?? string.Empty;
                foreach (var capabilityId in item[CapabilityIdsProperty]?.AsArray() ?? [])
                {
                    ValidationHelpers.ValidateCapabilityReference(capabilityId?.GetValue<string>(), capabilities, resistances, $"items/{catalog.DocumentName}.json", id, report);
                }

                foreach (var action in item["actions"]?.AsArray().OfType<JsonObject>() ?? [])
                {
                    ValidationHelpers.ValidateActionReference(action, actions, statuses, $"items/{catalog.DocumentName}.json", id, report);
                }
            }
        }
    }
}
