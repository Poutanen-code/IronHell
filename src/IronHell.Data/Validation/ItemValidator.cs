using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Serialization;

namespace IronHell.Data.Validation;

internal static class ItemValidator
{
    private const string CapabilityIdsProperty = "capability_ids";
    private const string ResistanceIdsProperty = "resistance_ids";
    private const string EgoItemsDocument = "items/ego_items.json";
    private const string ArtifactsDocument = "items/artifacts.json";
    private const string CombatModifiersDocument = "combat_modifiers.json";
    private const string CombatModifiersProperty = "combat_modifiers";
    private const string ItemAffixesDocument = "items/item_affixes.json";
    private const string AffixesProperty = "affixes";
    private const string EgoItemsProperty = "ego_items";
    private const string ArtifactsProperty = "artifacts";
    private const string CapabilitiesDocument = "capabilities";
    private const string ResistancesDocument = "resistances";
    private const string EffectsProperty = "effects";
    private const string CapabilitiesProperty = "capability_ids";
    private const string ResistancesProperty = "resistance_ids";
    private const string ActivationsProperty = "activations";
    private const string CursesProperty = "curses";
    private const string ValueSourceProperty = "value_source";
    private static readonly IReadOnlySet<string> ArtifactAffixValueSources = new HashSet<string>(StringComparer.Ordinal)
    {
        "pval", "plus_to_hit", "plus_to_dam", "plus_to_ac",
    };
    private static readonly IReadOnlySet<string> CurseIds = new HashSet<string>(StringComparer.Ordinal)
    {
        "light_curse", "heavy_curse", "perma_curse",
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
                    ValidationHelpers.ValidateCapabilityReference(capabilityId?.GetValue<string>(), capabilities, $"items/{catalog.DocumentName}.json", id, report);
                }

                foreach (var resistanceId in item[ResistanceIdsProperty]?.AsArray() ?? [])
                {
                    ValidationHelpers.ValidateReference(resistanceId?.GetValue<string>(), resistances, $"items/{catalog.DocumentName}.json", id, ResistanceIdsProperty, "unknown_resistance", report);
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
        foreach (var egoItem in egoDocument[EgoItemsProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var egoId = egoItem["id"]?.GetValue<string>() ?? string.Empty;
            var references = egoItem[CombatModifiersProperty]?.AsArray().Select(reference => reference?.GetValue<string>()).ToArray() ?? [];
            ValidateReferences(EgoItemsDocument, egoId, references, modifierIds, report);
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
        foreach (var artifact in artifactDocument[ArtifactsProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var artifactId = artifact["id"]?.GetValue<string>() ?? artifact["name"]?.GetValue<string>() ?? string.Empty;
            var references = artifact[EffectsProperty]?[CombatModifiersProperty]?.AsArray().Select(reference => reference?.GetValue<string>()).ToArray() ?? [];
            ValidateReferences(ArtifactsDocument, artifactId, references, modifierIds, report);
        }
    }

    public static void ValidateItemAffixes(
        FrozenDictionary<string, JsonObject> documents,
        DefinitionValidationReport report)
    {
        var affixes = documents["item_affixes"]["item_affixes"]?.AsArray().OfType<JsonObject>() ?? [];
        var affixIds = affixes
            .Where(affix => affix["id"] is not null)
            .Select(affix => affix["id"]!.GetValue<string>())
            .ToHashSet(StringComparer.Ordinal);

        ValidateAffixReferences(
            documents[ArtifactsProperty][ArtifactsProperty]?.AsArray().OfType<JsonObject>() ?? [],
            $"{ArtifactsProperty}.{EffectsProperty}.{AffixesProperty}",
            artifact => artifact["id"]?.GetValue<string>() ?? string.Empty,
            report,
            affixIds,
            item => item[EffectsProperty]?[AffixesProperty]?.AsArray().OfType<JsonObject>() ?? [],
            validateValueSources: true);

        ValidateAffixReferences(
            documents[EgoItemsProperty][EgoItemsProperty]?.AsArray().OfType<JsonObject>() ?? [],
            $"{EgoItemsProperty}.{EffectsProperty}.{AffixesProperty}",
            egoItem => egoItem["id"]?.GetValue<string>() ?? string.Empty,
            report,
            affixIds,
            item => item[EffectsProperty]?[AffixesProperty]?.AsArray().OfType<JsonObject>() ?? []);
    }

    public static void ValidateArtifactEffects(
        FrozenDictionary<string, JsonObject> documents,
        DefinitionValidationReport report)
    {
        var capabilityIds = GetDefinitionIds(documents[CapabilitiesDocument], CapabilitiesDocument, "id");
        var resistanceIds = GetDefinitionIds(documents[ResistancesDocument], ResistancesDocument, "id");
        var activationIds = GetDefinitionIds(documents[ActivationsProperty], ActivationsProperty, "activation_id");
        foreach (var artifact in documents[ArtifactsProperty][ArtifactsProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var artifactId = artifact["id"]?.GetValue<string>() ?? string.Empty;
            var effects = artifact[EffectsProperty]?.AsObject();
            ValidateEffectReferences(effects?[CapabilitiesProperty]?.AsArray(), capabilityIds, artifactId, CapabilitiesProperty, "unknown_capability", report);
            ValidateEffectReferences(effects?[ResistancesProperty]?.AsArray(), resistanceIds, artifactId, ResistancesProperty, "unknown_resistance", report);
            ValidateEffectReferences(effects?[ActivationsProperty]?.AsArray(), activationIds, artifactId, ActivationsProperty, "unknown_activation", report);
            ValidateEffectReferences(effects?[CursesProperty]?.AsArray(), CurseIds, artifactId, CursesProperty, "unknown_curse", report);
            ValidateArtifactActivationReference(artifact, artifactId, effects, report);
        }
    }

    public static void ValidateEgoEffects(
        FrozenDictionary<string, JsonObject> documents,
        DefinitionValidationReport report)
    {
        var capabilityIds = GetDefinitionIds(documents[CapabilitiesDocument], CapabilitiesDocument, "id");
        var resistanceIds = GetDefinitionIds(documents[ResistancesDocument], ResistancesDocument, "id");
        foreach (var egoItem in documents[EgoItemsProperty][EgoItemsProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var egoId = egoItem["id"]?.GetValue<string>() ?? string.Empty;
            var effects = egoItem[EffectsProperty]?.AsObject();
            ValidateEffectReferences(effects?[CapabilitiesProperty]?.AsArray(), capabilityIds, egoId, CapabilitiesProperty, "unknown_capability", report, EgoItemsDocument);
            ValidateEffectReferences(effects?[ResistancesProperty]?.AsArray(), resistanceIds, egoId, ResistancesProperty, "unknown_resistance", report, EgoItemsDocument);
        }
    }

    private static IReadOnlySet<string> GetDefinitionIds(JsonObject document, string collection, string idProperty) =>
        (document[collection]?.AsArray().OfType<JsonObject>() ?? [])
        .Where(definition => definition[idProperty] is not null)
        .Select(definition => definition[idProperty]!.GetValue<string>())
        .ToHashSet(StringComparer.Ordinal);

    private static void ValidateEffectReferences(
        JsonArray? references,
        IReadOnlySet<string> knownIds,
        string artifactId,
        string effectProperty,
        string errorCode,
        DefinitionValidationReport report,
        string documentPath = ArtifactsDocument)
    {
        var unknownReferences = references?.Select(value => value?.GetValue<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Where(value => !knownIds.Contains(value!)) ?? [];
        foreach (var reference in unknownReferences)
        {
            report.Add(documentPath, artifactId, $"{EffectsProperty}.{effectProperty}", errorCode, $"{effectProperty} reference '{reference}' does not resolve.");
        }
    }

    private static void ValidateArtifactActivationReference(JsonObject artifact, string artifactId, JsonObject? effects, DefinitionValidationReport report)
    {
        if (artifact["activation"]?["id"]?.GetValue<string>() is { } activationId && effects?[ActivationsProperty]?.AsArray().Any(value => value?.GetValue<string>() == activationId) != true)
        {
            report.Add(ArtifactsDocument, artifactId, $"{EffectsProperty}.activations", "missing_activation_reference", $"Activation '{activationId}' must be referenced by canonical effects.");
        }
    }

    private static void ValidateAffixReferences(
        IEnumerable<JsonObject> items,
        string fieldPath,
        Func<JsonObject, string> getId,
        DefinitionValidationReport report,
        IReadOnlySet<string> affixIds,
        Func<JsonObject, IEnumerable<JsonObject>>? getAffixes = null,
        bool validateValueSources = false)
    {
        foreach (var item in items)
        {
            var itemId = getId(item);
            if (validateValueSources && item.ContainsKey(AffixesProperty))
            {
                report.Add(ArtifactsDocument, itemId, AffixesProperty, "top_level_artifact_affixes", "Artifact affixes must be defined only in effects.affixes.");
            }

            var affixObjects = (getAffixes?.Invoke(item) ?? item[AffixesProperty]?.AsArray().OfType<JsonObject>() ?? []).ToArray();
            var references = affixObjects
                .Select(affix => affix["id"]?.GetValue<string>())
                .ToArray();
            foreach (var duplicate in references.Where(id => id is not null)
                         .GroupBy(id => id!, StringComparer.Ordinal)
                         .Where(group => group.Count() > 1)
                         .Select(group => group.Key))
            {
                report.Add(ItemAffixesDocument, itemId, fieldPath, "duplicate_affix_reference", $"Affix '{duplicate}' is referenced more than once.");
            }

            foreach (var reference in references.Where(id => !string.IsNullOrWhiteSpace(id)).Where(id => !affixIds.Contains(id!)))
            {
                report.Add(ItemAffixesDocument, itemId, fieldPath, "unknown_affix", $"Affix '{reference}' does not resolve in item_affixes.json.");
            }

            if (!validateValueSources)
            {
                continue;
            }

            foreach (var valueSource in affixObjects
                         .Select(affix => affix[ValueSourceProperty]?.GetValue<string>())
                         .Where(value => !string.IsNullOrWhiteSpace(value))
                         .Where(value => !ArtifactAffixValueSources.Contains(value!)))
            {
                report.Add(ItemAffixesDocument, itemId, $"{fieldPath}.{ValueSourceProperty}", "unknown_affix_value_source", $"Affix value source '{valueSource}' is not a supported artifact value source.");
            }
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

}
