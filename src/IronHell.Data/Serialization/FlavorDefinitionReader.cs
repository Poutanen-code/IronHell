using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class FlavorDefinitionReader
{
    private const string DocumentPath = "items/flavors.json";

    public static List<FlavorDefinition> Read(JsonObject document, DefinitionValidationReport report)
    {
        var flavors = new List<FlavorDefinition>();
        var mangbandIndices = new Dictionary<int, string>();

        foreach (var entry in document["flavors"]?.AsArray() ?? [])
        {
            var id = entry?["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add(DocumentPath, null, "$.id", "missing_id", "Flavor identifier is required.");
                continue;
            }

            var categoryText = entry?["category"]?.GetValue<string>();
            if (!TryParseCategory(categoryText, out var category))
            {
                report.Add(DocumentPath, id, "category", "invalid_category", $"Flavor category '{categoryText}' is not a recognized category.");
                continue;
            }

            var legacyNode = entry?["legacy"];
            var mangbandIndex = legacyNode?["mangband_index"]?.GetValue<int>();
            var tval = legacyNode?["tval"]?.GetValue<int>();
            var sval = legacyNode?["sval"]?.GetValue<int?>();
            if (mangbandIndex is null || tval is null)
            {
                report.Add(DocumentPath, id, "legacy", "missing_legacy_metadata", "Flavor legacy metadata (mangband_index, tval) is required.");
                continue;
            }

            if (mangbandIndices.TryGetValue(mangbandIndex.Value, out var existingId))
            {
                report.Add(DocumentPath, id, "legacy.mangband_index", "duplicate_mangband_index", $"Mangband index {mangbandIndex} is already used by '{existingId}'.");
            }
            else
            {
                mangbandIndices.Add(mangbandIndex.Value, id);
            }

            var displayName = entry?["display_name"]?.GetValue<string>();
            var glyph = entry?["glyph"]?.GetValue<string>();
            var color = entry?["color"]?.GetValue<string>();
            var provenanceStatus = entry?["provenance_status"]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                report.Add(DocumentPath, id, "display_name", "missing_display_name", "Flavor display name is required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(glyph))
            {
                report.Add(DocumentPath, id, "glyph", "missing_glyph", "Flavor glyph is required.");
                continue;
            }

            flavors.Add(new FlavorDefinition(
                id,
                category,
                displayName,
                glyph,
                color ?? string.Empty,
                new FlavorLegacyMetadata(mangbandIndex.Value, tval.Value, sval),
                provenanceStatus ?? "unverified"));
        }

        ValidationHelpers.ValidateDuplicates("items/flavors", flavors, report);
        return flavors;
    }

    private static bool TryParseCategory(string? categoryText, out FlavorCategory category)
    {
        category = default;
        if (string.IsNullOrWhiteSpace(categoryText))
        {
            return false;
        }

        switch (categoryText)
        {
            case "ring": category = FlavorCategory.Ring; return true;
            case "amulet": category = FlavorCategory.Amulet; return true;
            case "staff": category = FlavorCategory.Staff; return true;
            case "wand": category = FlavorCategory.Wand; return true;
            case "rod": category = FlavorCategory.Rod; return true;
            case "potion": category = FlavorCategory.Potion; return true;
            case "mushroom": category = FlavorCategory.Mushroom; return true;
            case "scroll": category = FlavorCategory.Scroll; return true;
            default: return false;
        }
    }
}
