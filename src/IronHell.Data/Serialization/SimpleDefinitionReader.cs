using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class SimpleDefinitionReader
{
    public static List<T> Read<T>(JsonObject document, string collectionName, string idProperty, Func<string, T> factory, DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        var definitions = new List<T>();
        foreach (var entry in document[collectionName]?.AsArray() ?? [])
        {
            var id = entry?[idProperty]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add($"{collectionName}.json", null, $"$.{collectionName}", "missing_id", "Definition identifier is required.");
                continue;
            }

            definitions.Add(factory(id));
        }

        ValidationHelpers.ValidateDuplicates(collectionName, definitions, report);
        return definitions;
    }
}
