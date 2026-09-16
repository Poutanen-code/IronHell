using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Serialization;
using IronHell.Data.Validation;
using Xunit;

namespace IronHell.Data.Tests;

public sealed class FlavorDefinitionReaderTests
{
    private static readonly string RepositoryFlavorsPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../data/definitions/items/flavors.json"));

    [Fact]
    public void Read_RepositoryFlavors_MatchesMangbandFlavorTxtCoverage()
    {
        var (flavors, report) = ReadRepositoryFlavors();

        Assert.False(report.HasErrors, string.Join(Environment.NewLine, report.ToImmutable().Errors.Select(error => error.Message)));
        Assert.Equal(304, flavors.Count);
    }

    [Fact]
    public void Read_RepositoryFlavors_HasUniqueIdsAndMangbandIndices()
    {
        var (flavors, _) = ReadRepositoryFlavors();

        Assert.Equal(flavors.Count, flavors.Select(flavor => flavor.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(flavors.Count, flavors.Select(flavor => flavor.Legacy.MangbandIndex).Distinct().Count());
    }

    [Theory]
    [InlineData(FlavorCategory.Ring, 43)]
    [InlineData(FlavorCategory.Amulet, 26)]
    [InlineData(FlavorCategory.Staff, 35)]
    [InlineData(FlavorCategory.Wand, 35)]
    [InlineData(FlavorCategory.Rod, 35)]
    [InlineData(FlavorCategory.Mushroom, 20)]
    [InlineData(FlavorCategory.Potion, 59)]
    [InlineData(FlavorCategory.Scroll, 51)]
    public void Read_RepositoryFlavors_HasExpectedCountPerCategory(FlavorCategory category, int expectedCount)
    {
        var (flavors, _) = ReadRepositoryFlavors();

        Assert.Equal(expectedCount, flavors.Count(flavor => flavor.Category == category));
    }

    [Fact]
    public void Read_RepositoryFlavors_PreservesFixedRingAndPotionSvals()
    {
        var (flavors, _) = ReadRepositoryFlavors();

        var theOneRing = flavors.Single(flavor => flavor.Id == "ring_plain_gold");
        Assert.Equal(37, theOneRing.Legacy.Sval);

        var water = flavors.Single(flavor => flavor.Id == "potion_clear");
        Assert.Equal(0, water.Legacy.Sval);
    }

    [Fact]
    public void Read_DuplicateMangbandIndex_ReturnsValidationError()
    {
        var document = (JsonNode.Parse("""
            {
              "flavors": [
                { "id": "ring_a", "category": "ring", "display_name": "A", "glyph": "=", "color": "w", "legacy": { "mangband_index": 1, "tval": 45, "sval": null }, "provenance_status": "verified" },
                { "id": "ring_b", "category": "ring", "display_name": "B", "glyph": "=", "color": "w", "legacy": { "mangband_index": 1, "tval": 45, "sval": null }, "provenance_status": "verified" }
              ]
            }
            """) as JsonObject)!;

        var report = new DefinitionValidationReport();
        FlavorDefinitionReader.Read(document, report);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "duplicate_mangband_index");
    }

    [Fact]
    public void Read_InvalidCategory_ReturnsValidationError()
    {
        var document = (JsonNode.Parse("""
            {
              "flavors": [
                { "id": "ring_a", "category": "chest", "display_name": "A", "glyph": "=", "color": "w", "legacy": { "mangband_index": 1, "tval": 45, "sval": null }, "provenance_status": "verified" }
              ]
            }
            """) as JsonObject)!;

        var report = new DefinitionValidationReport();
        FlavorDefinitionReader.Read(document, report);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "invalid_category");
    }

    private static (List<FlavorDefinition> Flavors, DefinitionValidationReport Report) ReadRepositoryFlavors()
    {
        var content = File.ReadAllText(RepositoryFlavorsPath);
        var document = (JsonNode.Parse(content) as JsonObject)!;
        var report = new DefinitionValidationReport();
        var flavors = FlavorDefinitionReader.Read(document, report);
        return (flavors, report);
    }
}
