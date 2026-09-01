using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal static class TerrainValidator
{
    private const string TerrainDefinitionsProperty = "terrain_definitions";
    private const string TerrainDocument = "environment/terrain_definitions.json";

    public static void Validate(
        JsonObject document,
        IDefinitionRegistry<TerrainDefinition> terrain,
        DefinitionValidationReport report)
    {
        foreach (var definition in document[TerrainDefinitionsProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = definition["id"]?.GetValue<string>() ?? string.Empty;
            ValidationHelpers.ValidateReference(definition["appears_as"]?.GetValue<string>(), terrain, TerrainDocument, id, "appears_as", "unknown_terrain", report);
        }
    }
}
