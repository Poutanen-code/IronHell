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
        var evaluationOptions = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
        };
        var schemas = LoadSchemas(definitionsRoot, manifest, evaluationOptions.SchemaRegistry, report);
        foreach (var entry in manifest.OrderBy(entry => entry.JsonPath, StringComparer.Ordinal))
        {
            var document = await TryLoadDocumentAsync(definitionsRoot, entry, report, cancellationToken);
            if (document is null)
            {
                continue;
            }

            if (schemas.TryGetValue(entry.Name, out var schema))
            {
                var evaluation = schema.Evaluate(document, evaluationOptions);
                foreach (var error in GetSchemaErrors(evaluation).Where(error => !IsSchemaBranchNoise(entry, error)))
                {
                    report.Add(entry.JsonPath, null, error.InstancePath, "schema_validation", error.Message);
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
            var content = await File.ReadAllTextAsync(jsonPath, cancellationToken);
            if (content.Length > 0 && content[0] == '\uFEFF')
            {
                content = content[1..];
            }

            return JsonNode.Parse(content) as JsonObject
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
        SchemaRegistry schemaRegistry,
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
                schemaRegistry.Register(schema);
                if (IsItemSchema(schemaPath))
                {
                    schemaRegistry.Register(new Uri($"https://ironhell.local/schemas/items/{Path.GetFileName(schemaPath)}"), schema);
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
        var schema = JsonNode.Parse(File.ReadAllText(schemaPath))?.AsObject()
            ?? throw new JsonException("Item schema root must be an object.");

        var definitions = schema["$defs"]?.AsObject();
        if (definitions is null)
        {
            return schema.ToJsonString();
        }

        if (commonItemDefinitions is null)
        {
            throw new JsonException("The common item schema does not define '$defs'.");
        }

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

    private sealed record SchemaValidationMessage(string EvaluationPath, string InstancePath, string Message);

    private static IEnumerable<SchemaValidationMessage> GetSchemaErrors(EvaluationResults evaluation)
    {
        if (evaluation.IsValid)
        {
            return [];
        }

        var errors = evaluation.Errors?.Values
            .Select(message => new SchemaValidationMessage(
                evaluation.EvaluationPath.ToString(),
                evaluation.InstanceLocation.ToString(),
                message)) ?? [];
        var nestedErrors = evaluation.Details?
            .Where(detail => !detail.IsValid)
            .SelectMany(GetSchemaErrors) ?? [];
        return errors.Concat(nestedErrors);
    }

    private static bool IsSchemaBranchNoise(DefinitionManifestEntry entry, SchemaValidationMessage error)
    {
        var isAlternativeBranch = error.EvaluationPath.Contains("/oneOf", StringComparison.Ordinal) ||
            error.EvaluationPath.Contains("/anyOf", StringComparison.Ordinal);
        return isAlternativeBranch &&
            (IsItemSchemaBranchNoise(entry, error.Message) ||
             IsMonsterSchemaBranchNoise(entry, error.Message) ||
             IsTrapSchemaBranchNoise(entry, error.Message));
    }

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

    private static bool IsTrapSchemaBranchNoise(DefinitionManifestEntry entry, string error) =>
        entry.JsonPath == "environment/traps.json" && error == "All values fail against the false schema";
}