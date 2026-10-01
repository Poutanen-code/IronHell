using System.Text.Json.Nodes;
using IronHell.Core.Dungeon;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class VaultDefinitionReader
{
    public static List<VaultDefinition> Read(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<VaultDefinition>();
        foreach (var node in document["vaults"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = node["id"]?.GetValue<string>() ?? string.Empty;
            var tags = node["tags"]?.AsArray().Select(value => value?.GetValue<string>()).OfType<string>().ToArray() ?? [];
            var sourceType = 0;
            if (tags.Contains("room_type:7", StringComparer.Ordinal))
            {
                sourceType = 7;
            }
            else if (tags.Contains("room_type:8", StringComparer.Ordinal))
            {
                sourceType = 8;
            }
            var layout = node["layout"]?.AsArray().Select(value => value?.GetValue<string>() ?? string.Empty).ToArray() ?? [];
            if (sourceType == 0)
            {
                report.Add("environment/vaults.json", id, "tags", "missing_vault_type", "Vault tags must identify room_type:7 or room_type:8.");
                continue;
            }

            var definition = new VaultDefinition(id, sourceType, node["rating"]?.GetValue<int>() ?? 0, layout);
            try
            {
                definition.Validate();
                definitions.Add(definition);
            }
            catch (InvalidOperationException exception)
            {
                report.Add("environment/vaults.json", id, "layout", "invalid_vault_definition", exception.Message);
            }
        }

        return definitions;
    }
}