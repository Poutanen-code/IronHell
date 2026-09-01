using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class TerrainDefinitionReader
{
    public static List<TerrainDefinition> Read(JsonObject document, DefinitionValidationReport report) =>
        SimpleDefinitionReader.Read<TerrainDefinition>(document, "terrain_definitions", "id", id => new TerrainDefinition(id), report);
}
