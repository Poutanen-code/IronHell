using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Data.Validation;
using Xunit;

namespace IronHell.Data.Tests;

public sealed class ArtifactAffixValidationTests
{
    [Fact]
    public void ValidateItemAffixes_ArtifactEffectsAffixesWithKnownIdsAndValueSources_Succeeds()
    {
        var report = ValidateArtifactAffixes("""
            {
              "id": "artifact",
              "effects": {
                "affixes": [
                  { "id": "strength", "value_source": "pval" },
                  { "id": "to_hit", "value_source": "plus_to_hit" },
                  { "id": "to_damage", "value_source": "plus_to_dam" },
                  { "id": "to_ac", "value_source": "plus_to_ac" }
                ]
              }
            }
            """);

        Assert.Empty(report.Errors);
    }

    [Fact]
    public void ValidateItemAffixes_UnknownArtifactEffectsAffixId_ReturnsValidationError()
    {
        var report = ValidateArtifactAffixes("""
            {
              "id": "artifact",
              "effects": {
                "affixes": [
                  { "id": "missing_affix", "value_source": "pval" }
                ]
              }
            }
            """);

        Assert.Contains(report.Errors, error => error.Code == "unknown_affix" && error.FieldPath == "artifacts.effects.affixes");
    }

    [Fact]
    public void ValidateItemAffixes_UnknownArtifactAffixValueSource_ReturnsValidationError()
    {
        var report = ValidateArtifactAffixes("""
            {
              "id": "artifact",
              "effects": {
                "affixes": [
                  { "id": "strength", "value_source": "missing_value_source" }
                ]
              }
            }
            """);

        Assert.Contains(report.Errors, error => error.Code == "unknown_affix_value_source" && error.FieldPath == "artifacts.effects.affixes.value_source");
    }

    [Fact]
    public void ValidateItemAffixes_DuplicateArtifactEffectsAffixId_ReturnsValidationError()
    {
        var report = ValidateArtifactAffixes("""
            {
              "id": "artifact",
              "effects": {
                "affixes": [
                  { "id": "strength", "value_source": "pval" },
                  { "id": "strength", "value_source": "plus_to_hit" }
                ]
              }
            }
            """);

        Assert.Contains(report.Errors, error => error.Code == "duplicate_affix_reference" && error.FieldPath == "artifacts.effects.affixes");
    }

    [Fact]
    public void ValidateItemAffixes_TopLevelArtifactAffixes_AggregatesWithEffectsAffixErrors()
    {
        var report = ValidateArtifactAffixes("""
            {
              "id": "artifact",
              "affixes": [],
              "effects": {
                "affixes": [
                  { "id": "missing_affix", "value_source": "missing_value_source" },
                  { "id": "missing_affix", "value_source": "pval" }
                ]
              }
            }
            """);

        Assert.Contains(report.Errors, error => error.Code == "top_level_artifact_affixes");
        Assert.Contains(report.Errors, error => error.Code == "unknown_affix");
        Assert.Contains(report.Errors, error => error.Code == "unknown_affix_value_source");
        Assert.Contains(report.Errors, error => error.Code == "duplicate_affix_reference");
    }

    private static DefinitionValidationReportSnapshot ValidateArtifactAffixes(string artifactJson)
    {
        var report = new DefinitionValidationReport();
        var documents = new Dictionary<string, JsonObject>(StringComparer.Ordinal)
        {
            ["item_affixes"] = JsonNode.Parse("""
                {
                  "item_affixes": [
                    { "id": "strength" },
                    { "id": "to_hit" },
                    { "id": "to_damage" },
                    { "id": "to_ac" }
                  ]
                }
                """)!.AsObject(),
            ["artifacts"] = JsonNode.Parse($$"""
                {
                  "artifacts": [
                    {{artifactJson}}
                  ]
                }
                """)!.AsObject(),
            ["ego_items"] = JsonNode.Parse("{ \"ego_items\": [] }")!.AsObject(),
        }.ToFrozenDictionary(StringComparer.Ordinal);

        ItemValidator.ValidateItemAffixes(documents, report);
        return report.ToImmutable();
    }
}