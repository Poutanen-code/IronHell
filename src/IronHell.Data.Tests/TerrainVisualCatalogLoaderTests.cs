using System.Text.Json;
using System.Text.Json.Nodes;
using IronHell.Data.Visuals;
using Xunit;

namespace IronHell.Data.Tests;

public sealed class TerrainVisualCatalogLoaderTests : IDisposable
{
    private readonly string _dataRoot;
    private readonly string _temporaryRoot;

    public TerrainVisualCatalogLoaderTests()
    {
        _temporaryRoot = Path.Combine(Path.GetTempPath(), "IronHell.Data.Tests", Guid.NewGuid().ToString("N"));
        _dataRoot = Path.Combine(_temporaryRoot, "data");
        Directory.CreateDirectory(Path.Combine(_dataRoot, "definitions", "environment"));
        Directory.CreateDirectory(Path.Combine(_dataRoot, "art", "environment"));
        var schemaRoot = Path.Combine(_dataRoot, "schemas", "environment");
        Directory.CreateDirectory(schemaRoot);
        File.Copy(
            Path.Combine(RepositoryRoot, "data", "schemas", "environment", "terrain_visuals.schema.json"),
            Path.Combine(schemaRoot, "terrain_visuals.schema.json"));
    }

    [Fact]
    public async Task LoadAsync_RepositoryCatalog_ResolvesExistingTerrainAndSprites()
    {
        var terrainDocument = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(RepositoryRoot, "data", "definitions", "environment", "terrain_definitions.json")))!;
        var terrainIds = terrainDocument["terrain_definitions"]!.AsArray()
            .Select(definition => definition!["id"]!.GetValue<string>())
            .ToArray();

        var result = await TerrainVisualCatalogLoader.LoadAsync(Path.Combine(RepositoryRoot, "data"), terrainIds);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Report.Errors.Select(error => error.Message)));
        var catalog = Assert.IsType<TerrainVisualCatalog>(result.Catalog);
        Assert.Equal("door_closed.png", catalog.SpritePaths["door_closed_base"]);
        Assert.Equal("door_open.png", catalog.SpritePaths["open_door"]);
        Assert.Equal("door_open.png", catalog.SpritePaths["opened_home_door"]);
        Assert.Equal("door_broken.png", catalog.SpritePaths["broken_door"]);
        Assert.Equal("trap.png", catalog.SpritePaths["trap_door"]);
        Assert.Equal("trap.png", catalog.SpritePaths["dart_strength"]);
        Assert.Equal("wall_ore.png", catalog.SpritePaths["magma_vein"]);
        Assert.Equal("wall_ore.png", catalog.SpritePaths["quartz_vein"]);
        Assert.DoesNotContain("secret_door", catalog.SpritePaths.Keys);
        Assert.DoesNotContain("invisible_trap", catalog.SpritePaths.Keys);
        Assert.DoesNotContain("home_door_strength_1", catalog.SpritePaths.Keys);
    }

    [Fact]
    public async Task LoadAsync_UnknownTerrainId_ReturnsValidationError()
    {
        WriteCatalog(("missing_terrain", "door_closed.png"));
        CreateSprite("door_closed.png");

        var result = await LoadAsync("known_terrain");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Report.Errors, error => error.Code == "unknown_terrain");
    }

    [Fact]
    public async Task LoadAsync_MissingSprite_ReturnsValidationError()
    {
        WriteCatalog(("known_terrain", "missing.png"));

        var result = await LoadAsync("known_terrain");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Report.Errors, error => error.Code == "missing_sprite");
    }

    [Fact]
    public async Task LoadAsync_DuplicateTerrainMapping_ReturnsValidationError()
    {
        WriteCatalog(("known_terrain", "door_closed.png"), ("known_terrain", "door_open.png"));
        CreateSprite("door_closed.png");
        CreateSprite("door_open.png");

        var result = await LoadAsync("known_terrain");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Report.Errors, error => error.Code == "duplicate_terrain_mapping");
    }

    [Fact]
    public async Task LoadAsync_UnmappedTerrainIsAllowedAndSpriteReuseIsAllowed()
    {
        WriteCatalog(("first_terrain", "wall.png"), ("second_terrain", "wall.png"));
        CreateSprite("wall.png");

        var result = await LoadAsync("first_terrain", "second_terrain", "unmapped_terrain");

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Catalog!.SpritePaths.Count);
        Assert.Equal("wall.png", result.Catalog.SpritePaths["first_terrain"]);
        Assert.Equal("wall.png", result.Catalog.SpritePaths["second_terrain"]);
        Assert.DoesNotContain("unmapped_terrain", result.Catalog.SpritePaths.Keys);
    }

    [Fact]
    public async Task LoadAsync_RejectsPathOutsideEnvironmentArtworkRoot()
    {
        WriteCatalog(("known_terrain", "../wall.png"));

        var result = await LoadAsync("known_terrain");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Report.Errors, error => error.Code == "schema_validation");
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryRoot))
        {
            Directory.Delete(_temporaryRoot, recursive: true);
        }
    }

    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private Task<TerrainVisualCatalogLoadResult> LoadAsync(params string[] terrainIds) =>
        TerrainVisualCatalogLoader.LoadAsync(_dataRoot, terrainIds);

    private void WriteCatalog(params (string TerrainId, string SpritePath)[] mappings)
    {
        var document = new
        {
            schema_version = 1,
            mappings = mappings.Select(mapping => new { terrain_id = mapping.TerrainId, sprite_path = mapping.SpritePath }),
        };
        File.WriteAllText(
            Path.Combine(_dataRoot, "definitions", "environment", "terrain_visuals.json"),
            JsonSerializer.Serialize(document));
    }

    private void CreateSprite(string fileName) =>
        File.WriteAllBytes(Path.Combine(_dataRoot, "art", "environment", fileName), []);
}