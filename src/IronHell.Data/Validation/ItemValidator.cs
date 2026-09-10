using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;
using IronHell.Data.Serialization;

namespace IronHell.Data.Validation;

internal static class ItemValidator
{
    private const string CapabilityIdsProperty = "capability_ids";
    private const string EgoItemsDocument = "items/ego_items.json";
    private const string ArtifactsDocument = "items/artifacts.json";
    private const string CombatModifiersDocument = "combat_modifiers.json";
    private const string CombatModifiersProperty = "combat_modifiers";
    private const string SourceFlagProperty = "source_flag";
    private static readonly IReadOnlySet<string> CombatModifierFlags = new HashSet<string>(StringComparer.Ordinal)
    {
        "SLAY_ANIMAL", "SLAY_EVIL", "SLAY_UNDEAD", "SLAY_DEMON", "SLAY_ORC", "SLAY_TROLL", "SLAY_GIANT", "SLAY_DRAGON",
        "KILL_DRAGON", "KILL_DEMON", "KILL_UNDEAD", "BRAND_ACID", "BRAND_ELEC", "BRAND_FIRE", "BRAND_COLD", "BRAND_POIS", "IMPACT",
    };

    public static void Validate(
        FrozenDictionary<string, JsonObject> documents,
        IDefinitionRegistry<ActionDefinition> actions,
        IDefinitionRegistry<StatusDefinition> statuses,
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        DefinitionValidationReport report)
    {
        foreach (var catalog in ItemDefinitionReader.ItemCatalogs.Where(catalog => catalog.Category != ItemCategory.SpellBook))
        {
            foreach (var item in documents[catalog.DocumentName][catalog.CollectionName]?.AsArray().OfType<JsonObject>() ?? [])
            {
                var id = item["id"]?.GetValue<string>() ?? string.Empty;
                foreach (var capabilityId in item[CapabilityIdsProperty]?.AsArray() ?? [])
                {
                    ValidationHelpers.ValidateCapabilityReference(capabilityId?.GetValue<string>(), capabilities, resistances, $"items/{catalog.DocumentName}.json", id, report);
                }

                foreach (var action in item["actions"]?.AsArray().OfType<JsonObject>() ?? [])
                {
                    ValidationHelpers.ValidateActionReference(action, actions, statuses, $"items/{catalog.DocumentName}.json", id, report);
                }
            }
        }
    }

    public static void ValidateEgoCombatModifiers(
        JsonObject egoDocument,
        JsonObject combatModifierDocument,
        DefinitionValidationReport report)
    {
        var modifiers = combatModifierDocument["combat_modifiers"]?.AsArray().OfType<JsonObject>() ?? [];
        var modifierIds = modifiers
            .Where(modifier => modifier["id"] is not null)
            .Select(modifier => modifier["id"]!.GetValue<string>())
            .ToHashSet(StringComparer.Ordinal);
        var flagToModifier = modifiers
            .Where(modifier => modifier[SourceFlagProperty] is not null && modifier["id"] is not null)
            .ToDictionary(
                modifier => modifier[SourceFlagProperty]!.GetValue<string>(),
                modifier => modifier["id"]!.GetValue<string>(),
                StringComparer.Ordinal);

        foreach (var egoItem in egoDocument["ego_items"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var egoId = egoItem["id"]?.GetValue<string>() ?? string.Empty;
            var references = egoItem[CombatModifiersProperty]?.AsArray().Select(reference => reference?.GetValue<string>()).ToArray() ?? [];
            ValidateReferences(EgoItemsDocument, egoId, references, modifierIds, report);
            ValidateCombatFlagCoverage(EgoItemsDocument, egoItem, egoId, references, flagToModifier, report);
        }
    }

    public static void ValidateArtifactCombatModifiers(
        JsonObject artifactDocument,
        JsonObject combatModifierDocument,
        DefinitionValidationReport report)
    {
        var modifiers = combatModifierDocument["combat_modifiers"]?.AsArray().OfType<JsonObject>() ?? [];
        var modifierIds = modifiers
            .Where(modifier => modifier["id"] is not null)
            .Select(modifier => modifier["id"]!.GetValue<string>())
            .ToHashSet(StringComparer.Ordinal);
        var flagToModifier = modifiers
            .Where(modifier => modifier[SourceFlagProperty] is not null && modifier["id"] is not null)
            .ToDictionary(
                modifier => modifier[SourceFlagProperty]!.GetValue<string>(),
                modifier => modifier["id"]!.GetValue<string>(),
                StringComparer.Ordinal);

        foreach (var artifact in artifactDocument["artifacts"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var artifactId = artifact["id"]?.GetValue<string>() ?? artifact["name"]?.GetValue<string>() ?? string.Empty;
            var references = artifact[CombatModifiersProperty]?.AsArray().Select(reference => reference?.GetValue<string>()).ToArray() ?? [];
            ValidateReferences(ArtifactsDocument, artifactId, references, modifierIds, report);
            ValidateCombatFlagCoverage(ArtifactsDocument, artifact, artifactId, references, flagToModifier, report);
        }
    }

    private static void ValidateReferences(
        string document,
        string egoId,
        string?[] references,
        IReadOnlySet<string> modifierIds,
        DefinitionValidationReport report)
    {
        foreach (var duplicate in references.Where(reference => reference is not null)
                     .GroupBy(reference => reference!, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key))
        {
            report.Add(document, egoId, CombatModifiersProperty, "duplicate_combat_modifier_reference", $"Combat modifier '{duplicate}' is referenced more than once.");
        }

        foreach (var reference in references
                     .Where(reference => !string.IsNullOrWhiteSpace(reference))
                     .Where(reference => !modifierIds.Contains(reference!)))
        {
            report.Add(document, egoId, CombatModifiersProperty, "unknown_combat_modifier", $"Combat modifier '{reference}' does not resolve in {CombatModifiersDocument}.");
        }
    }

    private static void ValidateCombatFlagCoverage(
        string document,
        JsonObject egoItem,
        string egoId,
        string?[] references,
        IReadOnlyDictionary<string, string> flagToModifier,
        DefinitionValidationReport report)
    {
        var flags = egoItem["flags"]?.AsArray().Select(flag => flag?.GetValue<string>()).Where(flag => flag is not null) ?? [];
        foreach (var flag in flags.Where(flag => CombatModifierFlags.Contains(flag!)))
        {
            if (!flagToModifier.TryGetValue(flag!, out var modifierId) || !references.Contains(modifierId, StringComparer.Ordinal))
            {
                report.Add(document, egoId, "flags", "missing_combat_modifier_reference", $"Combat flag '{flag}' does not have its canonical combat modifier reference.");
            }
        }
    }
}
