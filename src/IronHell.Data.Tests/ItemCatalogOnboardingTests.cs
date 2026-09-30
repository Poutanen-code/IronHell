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
        Assert.Equal(27, items.Count(item => item.Category == ItemCategory.Rod));
    }

    [Fact]
    public void Read_RepositoryItemCatalogs_PreservesClassificationDimensions()
    {
        var (items, _) = ReadRepositoryItems();

        var dagger = items.Single(item => item.Id == "dagger");
        Assert.Equal(WeaponHandling.OneHanded, dagger.WeaponHandling);
        Assert.Equal(WeaponFamily.Dagger, dagger.WeaponFamily);
        Assert.Equal(WeaponClass.Blade, dagger.WeaponClass);

        var crossbow = items.Single(item => item.Id == "light_crossbow");
        Assert.Equal(WeaponHandling.Ranged, crossbow.WeaponHandling);
        Assert.Equal(WeaponFamily.Crossbow, crossbow.WeaponFamily);
        Assert.Equal(WeaponClass.Launcher, crossbow.WeaponClass);

        var armor = items.Single(item => item.Id == "soft_leather_armor");
        Assert.Equal(ArmorForm.Chest, armor.ArmorForm);
        Assert.Equal(BodyArmorFamily.Soft, armor.BodyArmorFamily);
        Assert.Equal(ArmorMaterial.Leather, armor.ArmorMaterial);

        Assert.Equal(BodyArmorFamily.Hard, items.Single(item => item.Id == "chain_mail").BodyArmorFamily);
        Assert.Equal(BodyArmorFamily.DragonScale, items.Single(item => item.Id == "black_dragon_scale_mail").BodyArmorFamily);

        Assert.All(items.Where(item => item.Category == ItemCategory.Ring), item => Assert.Null(item.WeaponFamily));
        Assert.All(items.Where(item => item.Category == ItemCategory.Amulet), item => Assert.Null(item.WeaponFamily));
    }

    [Fact]
    public void Read_RepositoryItemCatalogs_HasUniqueIdsAcrossAllCatalogs()
    {
        var (items, _) = ReadRepositoryItems();

        Assert.Equal(items.Count, items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Read_RepositoryItemCatalogs_PreservesStaticEffectAndLightFields()
    {
        var (items, report) = ReadRepositoryItems();
        Assert.False(report.HasErrors);

        var torch = items.Single(item => item.Id == "wooden_torch");
        Assert.Equal(4000, torch.FuelPval);
        Assert.Equal(1, torch.LightRadius);
        Assert.Equal(7, torch.StackSize);
        Assert.Equal(10000, items.Single(item => item.Id == "dwarven_lantern").FuelPval);
        Assert.Equal(10000, items.Single(item => item.Id == "feanorian_lamp").FuelPval);

        var ring = items.Single(item => item.Id == "ring_of_searching");
        Assert.Equal(new GeneratedItemAffixDefinition("search", 1, 6), Assert.Single(ring.GeneratedAffixes!));
        Assert.Empty(ring.CapabilityIds!);

        Assert.Equal(["slay_undead"], items.Single(item => item.Id == "mace_of_disruption").CombatModifierIds);
        Assert.Contains("ignore_fire", items.Single(item => item.Id == "amulet_of_sustenance").ResistanceIds!);
    }

    [Fact]
    public void Read_RepositoryItemCatalogs_PreservesNumericBehaviorAsSemanticProperties()
    {
        var (items, report) = ReadRepositoryItems();
        Assert.False(report.HasErrors);
        var byId = items.ToDictionary(item => item.Id, StringComparer.Ordinal);

        Assert.Equal(0, byId["dagger"].TreeCuttingEffectiveness);
        Assert.Equal(1, byId["small_sword"].TreeCuttingEffectiveness);
        Assert.Equal(1, byId["broad_sword"].TreeCuttingEffectiveness);
        Assert.Equal(2, byId["long_sword"].TreeCuttingEffectiveness);
        Assert.Equal(2, byId["blade_of_chaos"].TreeCuttingEffectiveness);
        Assert.Equal(2, byId["beaked_axe"].TreeCuttingEffectiveness);
        Assert.Equal(3, byId["scythe"].TreeCuttingEffectiveness);
        Assert.Equal(4, byId["scythe_of_slicing"].TreeCuttingEffectiveness);

        Assert.Equal(2, byId["sling"].LauncherPowerMultiplier);
        Assert.Equal(AmmunitionFamily.Shot, byId["sling"].CompatibleAmmunitionFamily);
        Assert.Equal(2, byId["short_bow"].LauncherPowerMultiplier);
        Assert.Equal(AmmunitionFamily.Arrow, byId["short_bow"].CompatibleAmmunitionFamily);
        Assert.Equal(3, byId["long_bow"].LauncherPowerMultiplier);
        Assert.Equal(AmmunitionFamily.Arrow, byId["long_bow"].CompatibleAmmunitionFamily);
        Assert.Equal(3, byId["light_crossbow"].LauncherPowerMultiplier);
        Assert.Equal(AmmunitionFamily.Bolt, byId["light_crossbow"].CompatibleAmmunitionFamily);
        Assert.Equal(4, byId["heavy_crossbow"].LauncherPowerMultiplier);
        Assert.Equal(AmmunitionFamily.Bolt, byId["heavy_crossbow"].CompatibleAmmunitionFamily);
        Assert.Null(byId["dagger"].LauncherPowerMultiplier);
        Assert.Null(byId["dagger"].CompatibleAmmunitionFamily);

        var expectedCutting = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["small_sword"] = 1, ["short_sword"] = 1, ["sabre"] = 1, ["cutlass"] = 1, ["tulwar"] = 1, ["broad_sword"] = 1,
            ["long_sword"] = 2, ["scimitar"] = 2, ["katana"] = 2, ["bastard_sword"] = 2, ["two_handed_sword"] = 2, ["executioners_sword"] = 2, ["blade_of_chaos"] = 2,
            ["beaked_axe"] = 2, ["broad_axe"] = 2, ["glaive"] = 2, ["halberd"] = 2,
            ["scythe"] = 3, ["lance"] = 3, ["battle_axe"] = 3, ["great_axe"] = 3, ["lochaber_axe"] = 3,
            ["scythe_of_slicing"] = 4,
        };
        var actualCutting = byId.Values
            .Where(item => item.TreeCuttingEffectiveness > 0)
            .ToDictionary(item => item.Id, item => item.TreeCuttingEffectiveness, StringComparer.Ordinal);
        Assert.Equal(expectedCutting.OrderBy(pair => pair.Key), actualCutting.OrderBy(pair => pair.Key));

        var expectedLauncherMultipliers = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["sling"] = 2,
            ["short_bow"] = 2,
            ["long_bow"] = 3,
            ["light_crossbow"] = 3,
            ["heavy_crossbow"] = 4,
        };
        var actualLauncherMultipliers = byId.Values
            .Where(item => item.LauncherPowerMultiplier.HasValue)
            .ToDictionary(item => item.Id, item => item.LauncherPowerMultiplier!.Value, StringComparer.Ordinal);
        Assert.Equal(expectedLauncherMultipliers.OrderBy(pair => pair.Key), actualLauncherMultipliers.OrderBy(pair => pair.Key));

        Assert.Equal(LightFuelPolicy.Finite, byId["brass_lantern"].LightFuelPolicy);
        Assert.Equal(LightFuelPolicy.Inexhaustible, byId["dwarven_lantern"].LightFuelPolicy);

        var bookDocument = LoadRepositoryDocuments()["spell_books"];
        foreach (var bookEntry in bookDocument["books"]!.AsArray().OfType<JsonObject>())
        {
            var book = byId[bookEntry["id"]!.GetValue<string>()];
            var expectedQuality = bookEntry["generation_quality"]!.GetValue<string>() == "good"
                ? GoodGenerationQuality.Good
                : GoodGenerationQuality.Normal;
            Assert.Equal(expectedQuality, book.GenerationQuality);
            Assert.NotNull(book.SpellBookRealm);
            Assert.NotEmpty(book.SpellIds!);
            Assert.Null(book.Actions);
        }

        var lightRodAction = Assert.Single(byId["rod_of_light"].Actions!);
        Assert.Equal("BeamDamage", lightRodAction.ActionId);
        Assert.Equal("aimed", lightRodAction.Parameters!.Value.GetProperty("direction").GetString());
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
    public void Read_LauncherWithIncompatibleAmmunitionFamily_ReturnsValidationError()
    {
        var documents = LoadRepositoryDocuments();
        var sling = documents["weapons"]["weapons"]!.AsArray().OfType<JsonObject>().Single(item => item["id"]?.GetValue<string>() == "sling");
        sling["stats"]!["compatible_ammo_family"] = "arrow";

        var report = new DefinitionValidationReport();
        ItemDefinitionReader.Read(documents.ToFrozenDictionary(StringComparer.Ordinal), report);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "invalid_item_category" && error.DefinitionId == "sling");
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
        var itemRegistry = new DefinitionRegistry<ItemDefinition>([new ItemDefinition("ring_of_test", ItemCategory.Ring)]);
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
