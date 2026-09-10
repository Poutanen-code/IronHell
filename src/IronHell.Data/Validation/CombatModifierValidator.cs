using System.Text.Json.Nodes;

namespace IronHell.Data.Validation;

internal static class CombatModifierValidator
{
    private const string Document = "combat_modifiers.json";
    private const string EffectsProperty = "effects";
    private const string IdProperty = "id";
    private const string SourceFlagProperty = "source_flag";
    private static readonly IReadOnlySet<string> SpeciesTargets = new HashSet<string>(StringComparer.Ordinal)
    {
        "beast", "humanoid", "undead", "demon", "dragon", "construct", "aberration", "troll", "giant",
    };
    private static readonly IReadOnlySet<string> CategoryTargets = new HashSet<string>(StringComparer.Ordinal)
    {
        "animal", "orc", "undead", "demon", "dragon",
    };
    private static readonly IReadOnlySet<string> AlignmentTargets = new HashSet<string>(StringComparer.Ordinal)
    {
        "good", "neutral", "evil",
    };

    public static void Validate(JsonObject document, JsonObject monstersDocument, DefinitionValidationReport report)
    {
        var monsters = monstersDocument["monsters"]?.AsArray().OfType<JsonObject>().ToArray() ?? [];
        var modifiers = document["combat_modifiers"]?.AsArray().OfType<JsonObject>().ToArray() ?? [];
        ValidateDuplicates(modifiers, report);
        foreach (var modifier in modifiers)
        {
            var modifierId = modifier[IdProperty]?.GetValue<string>() ?? string.Empty;
            var modifierKind = modifier["modifier_kind"]?.GetValue<string>();
            var effects = modifier[EffectsProperty]?.AsArray().OfType<JsonObject>().ToArray() ?? [];
            if (modifierKind is "slay" or "kill")
            {
                foreach (var effect in effects)
                {
                    ValidateTarget(effect, modifierId, monsters, report);
                }
            }
            else if (modifierKind == "brand")
            {
                ValidateBrand(modifier, modifierId, report);
            }
        }

        ValidateSlayKillPairs(modifiers, report);
    }

    private static void ValidateDuplicates(JsonObject[] modifiers, DefinitionValidationReport report)
    {
        foreach (var duplicateId in modifiers.Where(modifier => modifier[IdProperty] is not null)
                     .GroupBy(modifier => modifier[IdProperty]!.GetValue<string>(), StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key))
        {
            report.Add(Document, duplicateId, IdProperty, "duplicate_combat_modifier_id", $"Duplicate combat modifier identifier '{duplicateId}'.");
        }

        foreach (var duplicateFlag in modifiers.Where(modifier => modifier[SourceFlagProperty] is not null)
                     .GroupBy(modifier => modifier[SourceFlagProperty]!.GetValue<string>(), StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key))
        {
            report.Add(Document, duplicateFlag, SourceFlagProperty, "duplicate_combat_modifier_source_flag", $"Duplicate combat modifier source flag '{duplicateFlag}'.");
        }
    }

    private static void ValidateTarget(JsonObject effect, string modifierId, JsonObject[] monsters, DefinitionValidationReport report)
    {
        var selectors = new[] { "monster_species", "monster_category", "monster_alignment" }
            .Where(selector => effect[selector] is not null)
            .ToArray();
        if (selectors.Length != 1)
        {
            report.Add(Document, modifierId, EffectsProperty, "invalid_taxonomy_selector", "Combat modifier effects must declare exactly one taxonomy selector.");
            return;
        }

        var target = ReadTarget(effect)!.Value;
        if (!IsKnownTarget(target.Kind, target.Value))
        {
            report.Add(Document, modifierId, EffectsProperty, "unknown_taxonomy_target", $"Combat modifier target '{target.Kind}={target.Value}' is not a known monster taxonomy value.");
            return;
        }

        if (!monsters.Any(monster => Matches(monster, target.Kind, target.Value)))
        {
            report.AddWarning(Document, modifierId, EffectsProperty, "zero_taxonomy_matches", $"Combat modifier target '{target.Kind}={target.Value}' has no matching monsters.");
        }
    }

    private static void ValidateBrand(JsonObject modifier, string modifierId, DefinitionValidationReport report)
    {
        var element = modifier["element"]?.GetValue<string>();
        if (element is null or "")
        {
            report.Add(Document, modifierId, "element", "missing_brand_element", "Brand modifiers must declare an element.");
        }
        else if (!BrandElements.Contains(element))
        {
            report.Add(Document, modifierId, "element", "unknown_brand_element", $"Brand element '{element}' is not supported.");
        }
    }

    private static void ValidateSlayKillPairs(JsonObject[] modifiers, DefinitionValidationReport report)
    {
        var byId = modifiers.Where(modifier => modifier[IdProperty] is not null)
            .ToDictionary(modifier => modifier[IdProperty]!.GetValue<string>(), StringComparer.Ordinal);
        foreach (var slay in modifiers.Where(modifier => modifier["modifier_kind"]?.GetValue<string>() == "slay"))
        {
            var slayId = slay[IdProperty]!.GetValue<string>();
            var pairId = $"kill_{slayId[5..]}";
            if (!byId.TryGetValue(pairId, out var kill))
            {
                continue;
            }

            var slayTarget = ReadTarget(slay[EffectsProperty]?.AsArray().OfType<JsonObject>().FirstOrDefault() ?? new JsonObject());
            var killTarget = ReadTarget(kill[EffectsProperty]?.AsArray().OfType<JsonObject>().FirstOrDefault() ?? new JsonObject());
            if (slayTarget != killTarget)
            {
                report.Add(Document, slayId, EffectsProperty, "slay_kill_target_mismatch", $"Slay/kill pair '{slayId}' and '{pairId}' must target the same taxonomy concept.");
            }
        }
    }

    private static readonly IReadOnlySet<string> BrandElements = new HashSet<string>(StringComparer.Ordinal)
    {
        "acid", "elec", "fire", "cold", "pois",
    };

    private static (TargetKind Kind, string Value)? ReadTarget(JsonObject effect)
    {
        if (effect["monster_species"]?.GetValue<string>() is { } species)
        {
            return (TargetKind.Species, species);
        }

        if (effect["monster_category"]?.GetValue<string>() is { } category)
        {
            return (TargetKind.Category, category);
        }

        if (effect["monster_alignment"]?.GetValue<string>() is { } alignment)
        {
            return (TargetKind.Alignment, alignment);
        }

        return null;
    }

    private static bool IsKnownTarget(TargetKind kind, string value) => kind switch
    {
        TargetKind.Species => SpeciesTargets.Contains(value),
        TargetKind.Category => CategoryTargets.Contains(value),
        TargetKind.Alignment => AlignmentTargets.Contains(value),
        _ => false,
    };

    private static bool Matches(JsonObject monster, TargetKind kind, string value) => kind switch
    {
        TargetKind.Species => monster["species"]?.GetValue<string>() == value,
        TargetKind.Category => monster["categories"]?.AsArray().Any(category => category?.GetValue<string>() == value) == true,
        TargetKind.Alignment => monster["alignment"]?.GetValue<string>() == value,
        _ => false,
    };

    private enum TargetKind
    {
        Species,
        Category,
        Alignment,
    }
}
