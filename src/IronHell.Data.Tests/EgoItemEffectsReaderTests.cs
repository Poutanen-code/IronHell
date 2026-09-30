using System.Text.Json.Nodes;
using IronHell.Data.Serialization;
using Xunit;

namespace IronHell.Data.Tests;

/// <summary>Verifies missing ego JSON collections normalize to empty, never null, C# collections.</summary>
public sealed class EgoItemEffectsReaderTests
{
    private static readonly JsonObject EgoItems = LoadEgoItems();

    private static JsonObject LoadEgoItems()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var path = Path.Combine(repositoryRoot, "data", "definitions", "items", "ego_items.json");
        return JsonNode.Parse(File.ReadAllText(path))!.AsObject();
    }

    private static JsonObject GetEgo(string id) =>
        EgoItems["ego_items"]!.AsArray().OfType<JsonObject>().Single(ego => ego["id"]!.GetValue<string>() == id);

    [Fact]
    public void ReadEffects_ResistanceIdsOnly_NormalizesOtherCollectionsToEmpty()
    {
        var effects = EgoItemEffectsReader.ReadEffects(GetEgo("of_resist_acid"));

        Assert.Equal(["ignore_acid", "res_acid"], effects.ResistanceIds);
        Assert.Empty(effects.Affixes);
        Assert.Empty(effects.CapabilityIds);
        Assert.Empty(effects.Activations);
        Assert.Empty(effects.Curses);
        Assert.Empty(effects.DisplayFlags);
    }

    [Fact]
    public void ReadEffects_CapabilityIdsOnly_NormalizesOtherCollectionsToEmpty()
    {
        var effects = EgoItemEffectsReader.ReadEffects(GetEgo("of_the_teleri"));

        Assert.Equal(["hold_life", "free_act"], effects.CapabilityIds);
        Assert.Empty(effects.Affixes);
        Assert.Empty(effects.ResistanceIds);
    }

    [Fact]
    public void ReadEffects_Affixes_NormalizeToIdAndValueSource()
    {
        var effects = EgoItemEffectsReader.ReadEffects(GetEgo("of_intelligence"));

        Assert.Equal([new EgoAffixReference("intelligence", "pval")], effects.Affixes);
        Assert.Equal(["sust_int"], effects.CapabilityIds);
    }

    [Fact]
    public void ReadEffects_CursesAndDisplayFlags_NormalizeNonEmpty()
    {
        var effects = EgoItemEffectsReader.ReadEffects(GetEgo("of_vulnerability"));

        Assert.Equal(["light_curse"], effects.Curses);
        Assert.Equal(["aggravate_monsters"], effects.CapabilityIds);
        Assert.Empty(effects.DisplayFlags);
    }

    [Fact]
    public void ReadCombatModifiers_Omitted_NormalizesToEmpty()
    {
        var combatModifiers = EgoItemEffectsReader.ReadCombatModifiers(GetEgo("of_resist_acid"));

        Assert.Empty(combatModifiers);
    }

    [Fact]
    public void ReadEffects_Omitted_NormalizesToEmptyDefinition()
    {
        var effects = EgoItemEffectsReader.ReadEffects(GetEgo("of_accuracy"));

        Assert.Same(EgoItemEffects.Empty, effects);
    }

    [Fact]
    public void ReadEffects_NonEmptyActivations_ArePreserved()
    {
        var effects = EgoItemEffectsReader.ReadEffects(GetEgo("of_lordly_resistance"));

        Assert.Equal(["RESIST"], effects.Activations);
    }
}
