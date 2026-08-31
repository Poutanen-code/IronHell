using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Nodes;
using IronHell.Data.Validation;
using Json.Schema;

namespace IronHell.Data.Loading;

internal static class DefinitionDocumentLoader
{
    public static async Task<FrozenDictionary<string, JsonObject>> LoadAsync(
        string definitionsRoot,
        IReadOnlyList<DefinitionManifestEntry> manifest,
        DefinitionValidationReport report,
        CancellationToken cancellationToken)
    {
        var documents = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        var schemas = LoadSchemas(definitionsRoot, manifest, report);
        foreach (var entry in manifest.OrderBy(entry => entry.JsonPath, StringComparer.Ordinal))
        {
            var document = await TryLoadDocumentAsync(definitionsRoot, entry, report, cancellationToken);
            if (document is null)
            {
                continue;
            }

            if (schemas.TryGetValue(entry.Name, out var schema))
            {
                var evaluation = schema.Evaluate(document, new EvaluationOptions
                {
                    OutputFormat = OutputFormat.List,
                });
                foreach (var error in GetSchemaErrors(evaluation).Where(error => !IsSchemaBranchNoise(entry, error)))
                {
                    report.Add(entry.JsonPath, null, "$", "schema_validation", error);
                }
            }

            documents.Add(entry.Name, document);
        }

        return documents.ToFrozenDictionary(StringComparer.Ordinal);
    }

    private static async Task<JsonObject?> TryLoadDocumentAsync(
        string definitionsRoot,
        DefinitionManifestEntry entry,
        DefinitionValidationReport report,
        CancellationToken cancellationToken)
    {
        var jsonPath = Path.GetFullPath(Path.Combine(definitionsRoot, entry.JsonPath));
        var schemaPath = Path.GetFullPath(Path.Combine(definitionsRoot, entry.SchemaPath));
        if (!File.Exists(jsonPath))
        {
            report.Add(entry.JsonPath, null, "$", "missing_file", "Definition file does not exist.");
            return null;
        }

        if (!File.Exists(schemaPath))
        {
            report.Add(entry.JsonPath, null, "$", "missing_schema", "Schema file does not exist.");
            return null;
        }

        try
        {
            return JsonNode.Parse(await File.ReadAllTextAsync(jsonPath, cancellationToken)) as JsonObject
                ?? throw new JsonException("Root JSON value must be an object.");
        }
        catch (JsonException exception)
        {
            report.Add(entry.JsonPath, null, "$", "malformed_json", exception.Message);
            return null;
        }
        catch (IOException exception)
        {
            report.Add(entry.JsonPath, null, "$", "unreadable_file", exception.Message);
            return null;
        }
    }

    private static FrozenDictionary<string, JsonSchema> LoadSchemas(
        string definitionsRoot,
        IReadOnlyList<DefinitionManifestEntry> manifest,
        DefinitionValidationReport report)
    {
        var schemas = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);
        var commonItemSchemaPath = Path.GetFullPath(Path.Combine(definitionsRoot, "../schemas/items/common_item.schema.json"));
        var commonItemDefinitions = JsonNode.Parse(File.ReadAllText(commonItemSchemaPath))?["$defs"]?.AsObject();
        var schemaPaths = new[]
        {
            "../schemas/common.schema.json",
            "../schemas/items/common_item.schema.json",
        }
        .Concat(manifest.Select(entry => entry.SchemaPath).OrderBy(path => path, StringComparer.Ordinal))
        .Distinct(StringComparer.Ordinal);
        foreach (var schemaPath in schemaPaths)
        {
            var absolutePath = Path.GetFullPath(Path.Combine(definitionsRoot, schemaPath));
            if (!File.Exists(absolutePath))
            {
                continue;
            }

            try
            {
                var schema = IsItemSchema(schemaPath)
                    ? JsonSchema.FromText(BundleCommonItemDefinitions(absolutePath, commonItemDefinitions))
                    : JsonSchema.FromFile(absolutePath);
                SchemaRegistry.Global.Register(schema);
                if (IsItemSchema(schemaPath))
                {
                    SchemaRegistry.Global.Register(new Uri($"https://ironhell.local/schemas/items/{Path.GetFileName(schemaPath)}"), schema);
                }
                foreach (var entry in manifest.Where(candidate => candidate.SchemaPath == schemaPath))
                {
                    schemas.Add(entry.Name, schema);
                }
            }
            catch (JsonException exception)
            {
                report.Add(schemaPath, null, "$", "schema_load_failure", exception.Message);
            }
        }

        return schemas.ToFrozenDictionary(StringComparer.Ordinal);
    }

    private static bool IsItemSchema(string schemaPath) =>
        schemaPath.StartsWith("../schemas/items/", StringComparison.Ordinal) &&
        !schemaPath.EndsWith("common_item.schema.json", StringComparison.Ordinal);

    private static string BundleCommonItemDefinitions(string schemaPath, JsonObject? commonItemDefinitions)
    {
        if (commonItemDefinitions is null)
        {
            throw new JsonException("The common item schema does not define '$defs'.");
        }

        var schema = JsonNode.Parse(File.ReadAllText(schemaPath))?.AsObject()
            ?? throw new JsonException("Item schema root must be an object.");
        var definitions = schema["$defs"]?.AsObject()
            ?? throw new JsonException("Item schema does not define '$defs'.");

        foreach (var definition in commonItemDefinitions)
        {
            definitions.TryAdd(definition.Key, definition.Value?.DeepClone());
        }

        ReplaceCommonItemReferences(schema);
        return schema.ToJsonString();
    }

    private static void ReplaceCommonItemReferences(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"]?.GetValue<string>() is { } reference && reference.StartsWith("common_item.schema.json#", StringComparison.Ordinal))
            {
                obj["$ref"] = reference["common_item.schema.json".Length..];
            }

            foreach (var property in obj.ToArray())
            {
                ReplaceCommonItemReferences(property.Value);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var value in array)
            {
                ReplaceCommonItemReferences(value);
            }
        }
    }

    private static IEnumerable<string> GetSchemaErrors(EvaluationResults evaluation)
    {
        if (evaluation.IsValid)
        {
            return [];
        }

        if (evaluation.Errors is not null)
        {
            return evaluation.Errors.Values;
        }

        return evaluation.Details?.SelectMany(GetSchemaErrors) ?? [];
    }

    private static bool IsSchemaBranchNoise(DefinitionManifestEntry entry, string error) =>
        IsItemSchemaBranchNoise(entry, error) || IsMonsterSchemaBranchNoise(entry, error);

    private static bool IsItemSchemaBranchNoise(DefinitionManifestEntry entry, string error) =>
        entry.JsonPath.StartsWith("items/", StringComparison.Ordinal) &&
        (error == "All values fail against the false schema" || error.EndsWith("but should be \"null\"", StringComparison.Ordinal));

    private static bool IsMonsterSchemaBranchNoise(DefinitionManifestEntry entry, string error) =>
        entry.JsonPath == "monsters/monsters.json" &&
        (error == "All values fail against the false schema" ||
         error == "Required properties [\"spells\"] are not present" ||
         error == "Required properties [\"abilities\"] are not present" ||
         error == "Required properties [\"spell_frequency\"] are not present" ||
         error == "Required properties [\"effect\",\"dice_count\",\"dice_sides\"] are not present");
}