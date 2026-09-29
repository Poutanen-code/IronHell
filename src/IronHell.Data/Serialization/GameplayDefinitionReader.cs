using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class GameplayDefinitionReader
{
    private const string CapabilitiesDocument = "capabilities.json";
    private const string ResistancesDocument = "resistances.json";

    public static List<CapabilityDefinition> ReadCapabilities(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<CapabilityDefinition>();
        foreach (var capability in document["capabilities"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = capability["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add(CapabilitiesDocument, null, "capabilities", "missing_id", "Definition identifier is required.");
                continue;
            }

            definitions.Add(new CapabilityDefinition(
                id,
                capability["name"]?.GetValue<string>() ?? string.Empty,
                capability["description"]?.GetValue<string>() ?? string.Empty,
                ReadEnum<CapabilityCategory>(capability["category"]?.GetValue<string>(), CapabilitiesDocument, id, "category", report),
                ReadEnum<CapabilityScope>(capability["scope"]?.GetValue<string>(), CapabilitiesDocument, id, "scope", report),
                capability["provenance_status"]?.GetValue<string>() ?? string.Empty,
                ReadStringArray(capability["grant_sources"]?.AsArray()),
                capability["resistance_id"]?.GetValue<string>(),
                ReadStringArray(capability["aliases"]?.AsArray()),
                capability["deprecated"]?.GetValue<bool>(),
                capability["canonical_owner"]?.GetValue<string>(),
                capability["migration_target_id"]?.GetValue<string>(),
                ReadStringArray(capability["policy_hooks"]?.AsArray()),
                capability["notes"]?.GetValue<string>()));
        }

        ValidationHelpers.ValidateDuplicates("capabilities", definitions, report);
        return definitions;
    }

    public static List<ResistanceDefinition> ReadResistances(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<ResistanceDefinition>();
        foreach (var resistance in document["resistances"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = resistance["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add(ResistancesDocument, null, "resistances", "missing_id", "Definition identifier is required.");
                continue;
            }

            definitions.Add(new ResistanceDefinition(
                id,
                resistance["name"]?.GetValue<string>() ?? string.Empty,
                resistance["description"]?.GetValue<string>() ?? string.Empty,
                ReadEnum<ResistanceSemanticKind>(resistance["semantic_kind"]?.GetValue<string>(), ResistancesDocument, id, "semantic_kind", report),
                ReadEnum<ResistanceTargetScope>(resistance["target_scope"]?.GetValue<string>(), ResistancesDocument, id, "target_scope", report),
                ReadEnum<ResistanceChannel>(resistance["channel"]?.GetValue<string>(), ResistancesDocument, id, "channel", report),
                resistance["provenance_status"]?.GetValue<string>() ?? string.Empty,
                resistance["status_id"]?.GetValue<string>(),
                ReadStringArray(resistance["grant_sources"]?.AsArray()),
                resistance["notes"]?.GetValue<string>()));
        }

        ValidationHelpers.ValidateDuplicates("resistances", definitions, report);
        return definitions;
    }

    private static TEnum ReadEnum<TEnum>(
        string? value,
        string document,
        string definitionId,
        string property,
        DefinitionValidationReport report)
        where TEnum : struct, Enum
    {
        var enumValue = value?.Replace("_", string.Empty, StringComparison.Ordinal);
        if (enumValue is not null && Enum.TryParse<TEnum>(enumValue, ignoreCase: true, out var result))
        {
            return result;
        }

        report.Add(document, definitionId, property, "invalid_definition_enum", $"Value '{value}' is not a known {property} value.");
        return default;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonArray? values) =>
        Array.AsReadOnly((values ?? [])
            .Select(value => value?.GetValue<string>() ?? string.Empty)
            .ToArray());
}