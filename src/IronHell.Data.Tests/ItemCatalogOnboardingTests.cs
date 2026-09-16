using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;
using IronHell.Data.Serialization;
using IronHell.Data.Validation;
using Xunit;

namespace IronHell.Data.Tests;

/// <summary>
/// Exercises ItemDefinitionReader directly against real repository item catalogs,
/// bypassing DefinitionDocumentLoader's JSON-schema pass (see the pre-existing,
/// unrelated ResistanceReference $ref bug tracked separately for races.schema.json).
/// </summary>
public sealed class ItemCatalogOnboardingTests
{
    private static readonly string DefinitionsRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../data/definitions"));

    [Fact]
    public void Read_RepositoryItemCatalogs_OnboardsAccessoriesStavesWandsAndRods()
    {
        var (items, report) = ReadRepositoryItems();

        Assert.False(report.HasErrors, string.Join(Environment.NewLine, report.ToImmutable().Errors.Select(error => error.Message)));

        Assert.Equal(32, items.Count(item => item.Category == ItemCategory.Ring));
        Assert.Equal(20, items.Count(item => item.Category == ItemCategory.Amulet));
        Assert.Equal(30, items.Count(item => item.Category == ItemCategory.Staff));
        Assert.Equal(29, items.Count(item => item.Category == ItemCategory.Wand));
        Assert.Equal(35, items.Count(item => item.Category == ItemCategory.Rod));
    }

    [Fact]
    public void Read_RepositoryItemCatalogs_RingAndAmuletTypesMatchCategory()
    {
        var (items, _) = ReadRepositoryItems();

        Assert.All(items.Where(item => item.Category == ItemCategory.Ring), item => Assert.Equal("ring", item.Type));
        Assert.All(items.Where(item => item.Category == ItemCategory.Amulet), item => Assert.Equal("amulet", item.Type));
    }

    [Fact]
    public void Read_RepositoryItemCatalogs_HasUniqueIdsAcrossAllCatalogs()
    {
        var (items, _) = ReadRepositoryItems();

        Assert.Equal(items.Count, items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Read_AccessoryWithInvalidType_ReturnsValidationError()
    {
        var documents = LoadRepositoryDocuments();
        documents["accessories"]["accessories"]!.AsArray().Add(new JsonObject
        {
            ["id"] = "ring_of_test_invalid",
            ["type"] = "necklace",
        });

        var report = new DefinitionValidationReport();
        ItemDefinitionReader.Read(documents.ToFrozenDictionary(StringComparer.Ordinal), report);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "invalid_item_category" && error.DefinitionId == "ring_of_test_invalid");
    }

    [Fact]
    public void ValidateFlavorCategoryAlignment_RepositoryData_HasNoMismatches()
    {
        var (items, _) = ReadRepositoryItems();
        var itemRegistry = new DefinitionRegistry<ItemDefinition>(items);

        var flavorsPath = Path.Combine(DefinitionsRoot, "items", "flavors.json");
        var flavorsDocument = (JsonNode.Parse(File.ReadAllText(flavorsPath)) as JsonObject)!;
        var flavorReport = new DefinitionValidationReport();
        var flavors = FlavorDefinitionReader.Read(flavorsDocument, flavorReport);
        var flavorRegistry = new DefinitionRegistry<FlavorDefinition>(flavors);

        var report = new DefinitionValidationReport();
        ItemValidator.ValidateFlavorCategoryAlignment(itemRegistry, flavorRegistry, report);

        Assert.False(report.HasErrors, string.Join(Environment.NewLine, report.ToImmutable().Errors.Select(error => error.Message)));
    }

    [Fact]
    public void ValidateFlavorCategoryAlignment_ItemCategoryWithoutFlavors_ReturnsValidationError()
    {
        var itemRegistry = new DefinitionRegistry<ItemDefinition>([new ItemDefinition("ring_of_test", ItemCategory.Ring, "ring")]);
        var flavorRegistry = new DefinitionRegistry<FlavorDefinition>([]);

        var report = new DefinitionValidationReport();
        ItemValidator.ValidateFlavorCategoryAlignment(itemRegistry, flavorRegistry, report);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "missing_flavor_category");
    }

    private static (List<ItemDefinition> Items, DefinitionValidationReport Report) ReadRepositoryItems()
    {
        var documents = LoadRepositoryDocuments();
        var report = new DefinitionValidationReport();
        var items = ItemDefinitionReader.Read(documents.ToFrozenDictionary(StringComparer.Ordinal), report);
        return (items, report);
    }

    private static Dictionary<string, JsonObject> LoadRepositoryDocuments()
    {
        var documents = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        foreach (var catalog in ItemDefinitionReader.ItemCatalogs)
        {
            if (documents.ContainsKey(catalog.DocumentName))
            {
                continue;
            }

            var relativePath = catalog.DocumentName == "spell_books"
                ? Path.Combine("magic", "spell_books.json")
                : Path.Combine("items", $"{catalog.DocumentName}.json");
            var content = File.ReadAllText(Path.Combine(DefinitionsRoot, relativePath));
            documents.Add(catalog.DocumentName, (JsonNode.Parse(content) as JsonObject)!);
        }

        return documents;
    }
}
