using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class TrapDefinitionReader
{
    public static List<TrapDefinition> Read(JsonObject document, DefinitionValidationReport report) =>
        SimpleDefinitionReader.Read<TrapDefinition>(document, "traps", "id", id => new TrapDefinition(id), report);
}
