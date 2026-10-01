using System.Text.Json;
using System.Text.Json.Nodes;
using IronHell.Data.Validation;
using Json.Schema;

namespace IronHell.Data.Visuals;

public static class TerrainVisualCatalogLoader
{
    private const string CatalogRelativePath = "definitions/environment/terrain_visuals.json";
    private const string SchemaRelativePath = "schemas/environment/terrain_visuals.schema.json";
    private const string ArtRelativePath = "art/environment";

    public static async Task<TerrainVisualCatalogLoadResult> LoadAsync(
        string dataRoot,
        IEnumerable<string> terrainDefinitionIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataRoot);
        ArgumentNullException.ThrowIfNull(terrainDefinitionIds);

        var report = new DefinitionValidationReport();
        var catalogPath = Path.Combine(dataRoot, CatalogRelativePath);
        var schemaPath = Path.Combine(dataRoot, SchemaRelativePath);
        var artRoot = Path.GetFullPath(Path.Combine(dataRoot, ArtRelativePath));
        if (!ValidateRequiredFiles(catalogPath, schemaPath, report))
        {
            return Failure(report);
        }

        var loaded = await TryLoadCatalogAsync(catalogPath, schemaPath, report, cancellationToken);
        if (loaded is null)
        {
            return Failure(report);
        }

        return BuildCatalog(loaded.Value.Document, loaded.Value.Schema, terrainDefinitionIds, artRoot, report);
    }

    private static bool ValidateRequiredFiles(
        string catalogPath,
        string schemaPath,
        DefinitionValidationReport report)
    {
        var valid = true;
        if (!File.Exists(catalogPath))
        {
            report.Add(CatalogRelativePath, null, "$", "missing_file", "Terrain visual catalog does not exist.");
            valid = false;
        }

        if (!File.Exists(schemaPath))
        {
            report.Add(CatalogRelativePath, null, "$", "missing_schema", "Terrain visual schema does not exist.");
            valid = false;
        }

        return valid;
    }

    private static async Task<(JsonObject Document, JsonSchema Schema)?> TryLoadCatalogAsync(
        string catalogPath,
        string schemaPath,
        DefinitionValidationReport report,
        CancellationToken cancellationToken)
    {
        try
        {
            var catalogJson = await File.ReadAllTextAsync(catalogPath, cancellationToken);
            if (catalogJson.Length > 0 && catalogJson[0] == '\uFEFF')
            {
                catalogJson = catalogJson[1..];
            }

            var document = JsonNode.Parse(catalogJson) as JsonObject
                ?? throw new JsonException("Root JSON value must be an object.");
            return (document, JsonSchema.FromFile(schemaPath));
        }
        catch (JsonException exception)
        {
            report.Add(CatalogRelativePath, null, "$", "malformed_json", exception.Message);
        }
        catch (IOException exception)
        {
            report.Add(CatalogRelativePath, null, "$", "unreadable_file", exception.Message);
        }

        return null;
    }

    private static TerrainVisualCatalogLoadResult BuildCatalog(
        JsonObject document,
        JsonSchema schema,
        IEnumerable<string> terrainDefinitionIds,
        string artRoot,
        DefinitionValidationReport report)
    {
        if (!schema.Evaluate(document).IsValid)
        {
            report.Add(CatalogRelativePath, null, "$", "schema_validation", "Terrain visual catalog does not match its schema.");
            return Failure(report);
        }

        var terrainIds = terrainDefinitionIds.ToHashSet(StringComparer.Ordinal);
        var seenTerrainIds = new HashSet<string>(StringComparer.Ordinal);
        var spritePaths = new Dictionary<string, string>(StringComparer.Ordinal);
        var mappings = document["mappings"]!.AsArray();
        for (var index = 0; index < mappings.Count; index++)
        {
            var mapping = mappings[index]!.AsObject();
            AddMapping(mapping, index, terrainIds, seenTerrainIds, artRoot, spritePaths, report);
        }

        return report.HasErrors
            ? Failure(report)
            : new TerrainVisualCatalogLoadResult(new TerrainVisualCatalog(spritePaths), report.ToImmutable());
    }

    private static void AddMapping(
        JsonObject mapping,
        int index,
        HashSet<string> terrainIds,
        HashSet<string> seenTerrainIds,
        string artRoot,
        Dictionary<string, string> spritePaths,
        DefinitionValidationReport report)
    {
        var terrainId = mapping["terrain_id"]!.GetValue<string>();
        var spritePath = mapping["sprite_path"]!.GetValue<string>();
        var fieldPath = $"mappings[{index}]";

        if (!seenTerrainIds.Add(terrainId))
        {
            report.Add(CatalogRelativePath, terrainId, $"{fieldPath}.terrain_id", "duplicate_terrain_mapping", "Terrain definition is mapped more than once.");
            return;
        }

        if (!terrainIds.Contains(terrainId))
        {
            report.Add(CatalogRelativePath, terrainId, $"{fieldPath}.terrain_id", "unknown_terrain", "Terrain ID does not exist in the canonical terrain definitions.");
            return;
        }

        if (!TryResolveSprite(artRoot, spritePath, out var fullSpritePath))
        {
            report.Add(CatalogRelativePath, terrainId, $"{fieldPath}.sprite_path", "unsafe_sprite_path", "Sprite path must be a filename relative to data/art/environment.");
            return;
        }

        if (!File.Exists(fullSpritePath))
        {
            report.Add(CatalogRelativePath, terrainId, $"{fieldPath}.sprite_path", "missing_sprite", $"Sprite asset '{spritePath}' does not exist.");
            return;
        }

        spritePaths.Add(terrainId, spritePath);
    }

    private static bool TryResolveSprite(string artRoot, string spritePath, out string fullPath)
    {
        fullPath = string.Empty;
        if (Path.IsPathRooted(spritePath) ||
            spritePath.Contains('/') ||
            spritePath.Contains('\\') ||
            !string.Equals(Path.GetFileName(spritePath), spritePath, StringComparison.Ordinal))
        {
            return false;
        }

        var candidate = Path.GetFullPath(Path.Combine(artRoot, spritePath));
        var artRootPrefix = Path.EndsInDirectorySeparator(artRoot)
            ? artRoot
            : artRoot + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!candidate.StartsWith(artRootPrefix, comparison))
        {
            return false;
        }

        fullPath = candidate;
        return true;
    }

    private static TerrainVisualCatalogLoadResult Failure(DefinitionValidationReport report) =>
        new(null, report.ToImmutable());
}