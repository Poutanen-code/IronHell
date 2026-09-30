using System.Text.Json.Nodes;

namespace IronHell.Data.Serialization;

/// <summary>An ego item affix reference normalized from JSON.</summary>
internal readonly record struct EgoAffixReference(string Id, string ValueSource);

/// <summary>Normalized ego item effects; missing JSON collections become empty, never null.</summary>
internal sealed record EgoItemEffects(
    IReadOnlyList<EgoAffixReference> Affixes,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> ResistanceIds,
    IReadOnlyList<string> Activations,
    IReadOnlyList<string> Curses,
    IReadOnlyList<string> DisplayFlags)
{
    public static readonly EgoItemEffects Empty = new([], [], [], [], [], []);
}

/// <summary>Reads ego item collection properties, treating an absent property as equivalent to an empty one.</summary>
internal static class EgoItemEffectsReader
{
    public static IReadOnlyList<string> ReadCombatModifiers(JsonObject egoItem) =>
        ReadStrings(egoItem["combat_modifiers"] as JsonArray);

    public static EgoItemEffects ReadEffects(JsonObject egoItem)
    {
        if (egoItem["effects"] is not JsonObject effects)
        {
            return EgoItemEffects.Empty;
        }

        return new EgoItemEffects(
            ReadAffixes(effects["affixes"] as JsonArray),
            ReadStrings(effects["capability_ids"] as JsonArray),
            ReadStrings(effects["resistance_ids"] as JsonArray),
            ReadStrings(effects["activations"] as JsonArray),
            ReadStrings(effects["curses"] as JsonArray),
            ReadStrings(effects["display_flags"] as JsonArray));
    }

    private static IReadOnlyList<string> ReadStrings(JsonArray? array) =>
        array is null ? [] : array.Select(node => node?.GetValue<string>() ?? string.Empty).ToArray();

    private static IReadOnlyList<EgoAffixReference> ReadAffixes(JsonArray? array) =>
        array is null
            ? []
            : array.OfType<JsonObject>()
                .Select(affix => new EgoAffixReference(
                    affix["id"]?.GetValue<string>() ?? string.Empty,
                    affix["value_source"]?.GetValue<string>() ?? string.Empty))
                .ToArray();
}
