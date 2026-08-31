using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using Xunit;

namespace IronHell.Data.Tests;

public sealed class DefinitionCatalogLoaderTests : IDisposable
{
    private readonly List<string> _temporaryRoots = [];

    [Fact]
    public async Task LoadAsync_RepositoryDefinitions_ReturnsCatalogThatCreatesHumanWarrior()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var human = success.Catalog.Races.GetRequired("human");
        var warrior = success.Catalog.Classes.GetRequired("warrior");

        Assert.All(warrior.StartingEquipment, equipment => Assert.True(success.Catalog.Items.TryGet(equipment.Id, out _)));

        var character = CharacterFactory.Create(human, warrior, success.Catalog.RaceClassRules);

        Assert.Equal("human", character.RaceId);
        Assert.Equal("warrior", character.ClassId);
        Assert.Equal(19, character.HitDie);
    }

    [Fact]
    public async Task LoadAsync_MissingFile_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        File.Delete(Path.Combine(root, "actions.json"));

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "missing_file");
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        await File.WriteAllTextAsync(Path.Combine(root, "actions.json"), "{");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "malformed_json");
    }

    [Fact]
    public async Task LoadAsync_DuplicateRaceId_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "character", "races.json"), "\"id\": \"human\"", "\"id\": \"half_elf\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "duplicate_id");
    }

    [Fact]
    public async Task LoadAsync_UnknownRace_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "character", "race_class_rules.json"), "\"race_id\": \"human\"", "\"race_id\": \"missing_race\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_race");
    }

    [Fact]
    public async Task LoadAsync_UnknownClass_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "character", "race_class_rules.json"), "\"class_id\": \"warrior\"", "\"class_id\": \"missing_class\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_class");
    }

    [Fact]
    public async Task LoadAsync_UnknownCapability_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "character", "races.json"), "\"sust_dex\"", "\"missing_capability\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_capability");
    }

    [Fact]
    public async Task LoadAsync_UnknownStartingEquipmentItem_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "character", "classes.json"), "\"id\": \"broad_sword\"", "\"id\": \"missing_item\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_item");
    }

    [Fact]
    public async Task LoadAsync_DuplicateItemIdAcrossCatalogs_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "items", "lights.json"), "\"id\": \"wooden_torch\"", "\"id\": \"dagger\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "duplicate_id");
    }

    [Fact]
    public async Task LoadAsync_InvalidStartingEquipmentQuantityRange_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "character", "classes.json"), "\"id\": \"broad_sword\", \"min\": 1, \"max\": 1", "\"id\": \"broad_sword\", \"min\": 2, \"max\": 1");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "invalid_quantity_range");
    }

    [Fact]
    public async Task LoadAsync_InvalidItemCategory_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "items", "weapons.json"), "\"type\": \"dagger\"", "\"type\": \"invalid_weapon\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "schema_validation");
    }

    [Fact]
    public async Task LoadAsync_UnknownCapabilityResistance_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "capabilities.json"), "\"resistance_id\": \"res_blind\"", "\"resistance_id\": \"missing_resistance\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_resistance");
    }

    [Fact]
    public async Task LoadAsync_UnknownResistanceStatus_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "resistances.json"), "\"status_id\": \"oppose_acid\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_UnknownItemAction_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "items", "potions.json"), "\"action_id\": \"ModifyResourceMeter\"", "\"action_id\": \"MissingAction\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_UnknownItemActionStatus_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "items", "consumables.json"), "\"status_id\": \"blinded\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_MultipleUnknownReferences_ReturnsAllErrorsAndNoCatalog()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "resistances.json"), "\"status_id\": \"oppose_acid\"", "\"status_id\": \"missing_status\"");
        ReplaceFirst(Path.Combine(root, "items", "potions.json"), "\"action_id\": \"ModifyResourceMeter\"", "\"action_id\": \"MissingAction\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_status");
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_RepositoryDefinitions_LoadsSpellRegistries()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        Assert.True(success.Catalog.MageSpells.TryGet("magic_magic_missile", out _));
        Assert.True(success.Catalog.PriestPrayers.TryGet("prayer_detect_evil", out _));
    }

    [Fact]
    public async Task LoadAsync_UnknownSpellBookReference_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "magic", "spell_books.json"), "\"magic_magic_missile\"", "\"missing_spell\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_spell");
    }

    [Fact]
    public async Task LoadAsync_UnknownSpellAction_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "magic", "mage_spells.json"), "\"action_id\": \"BoltDamage\"", "\"action_id\": \"StatDrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_UnknownSpellStatus_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "magic", "priest_prayers.json"), "\"status_id\": \"blessed\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_MultipleUnknownSpellReferences_ReturnsAllErrorsAndNoCatalog()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "magic", "spell_books.json"), "\"magic_magic_missile\"", "\"missing_spell\"");
        ReplaceFirst(Path.Combine(root, "magic", "mage_spells.json"), "\"action_id\": \"BoltDamage\"", "\"action_id\": \"StatDrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_spell");
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_RepositoryDefinitions_LoadsOrderedActivationRegistry()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var ids = success.Catalog.Activations.All.Select(activation => activation.Id).ToArray();
        Assert.True(success.Catalog.Activations.TryGet("ILLUMINATION", out _));
        Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), ids);
    }

    [Fact]
    public async Task LoadAsync_DuplicateActivationId_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "activations.json"), "\"activation_id\": \"MAGIC_MAP\"", "\"activation_id\": \"ILLUMINATION\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "duplicate_id");
    }

    [Fact]
    public async Task LoadAsync_UnknownActivationAction_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "activations.json"), "\"action_id\": \"LightArea\"", "\"action_id\": \"StatDrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_UnknownActivationStatus_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "activations.json"), "\"status_id\": \"protected_from_evil\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_MultipleUnknownActivationReferences_ReturnsAllErrorsAndNoCatalog()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "activations.json"), "\"action_id\": \"LightArea\"", "\"action_id\": \"StatDrain\"");
        ReplaceFirst(Path.Combine(root, "activations.json"), "\"status_id\": \"protected_from_evil\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_action");
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_RepositoryDefinitions_LoadsOrderedMonsterAbilityRegistry()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var ids = success.Catalog.MonsterAbilities.All.Select(ability => ability.Id).ToArray();
        Assert.True(success.Catalog.MonsterAbilities.TryGet("arrow_1", out _));
        Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), ids);
    }

    [Fact]
    public async Task LoadAsync_DuplicateMonsterAbilityId_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "monsters", "monster_abilities.json"), "\"id\": \"arrow_2\"", "\"id\": \"arrow_1\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "duplicate_id");
    }

    [Fact]
    public async Task LoadAsync_UnknownMonsterAbilityAction_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "monsters", "monster_abilities.json"), "\"action_id\": \"BoltDamage\"", "\"action_id\": \"StatDrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_UnknownMonsterAbilityStatus_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "monsters", "monster_abilities.json"), "\"status_id\": \"confused\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_MultipleUnknownMonsterAbilityReferences_ReturnsAllErrorsAndNoCatalog()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "monsters", "monster_abilities.json"), "\"action_id\": \"BoltDamage\"", "\"action_id\": \"StatDrain\"");
        ReplaceFirst(Path.Combine(root, "monsters", "monster_abilities.json"), "\"status_id\": \"confused\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_action");
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_RepositoryDefinitions_LoadsOrderedMonsterRegistry()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var ids = success.Catalog.Monsters.All.Select(monster => monster.Id).ToArray();
        Assert.True(success.Catalog.Monsters.TryGet("filthy_street_urchin", out _));
        Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), ids);
    }

    [Fact]
    public async Task LoadAsync_DuplicateMonsterId_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "monsters", "monsters.json"), "\"id\": \"scrawny_cat\"", "\"id\": \"filthy_street_urchin\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "duplicate_id");
    }

    [Fact]
    public async Task LoadAsync_UnknownMonsterAbility_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "monsters", "monster_abilities.json"), "\"id\": \"arrow_1\"", "\"id\": \"missing_ability\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_monster_ability");
    }

    [Fact]
    public async Task LoadAsync_UnknownMonsterAction_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "actions.json"), "\"action_id\": \"ApplyDamage\"", "\"action_id\": \"StatDrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_MultipleUnknownMonsterReferences_ReturnsAllErrorsAndNoCatalog()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "monsters", "monster_abilities.json"), "\"id\": \"arrow_1\"", "\"id\": \"missing_ability\"");
        ReplaceFirst(Path.Combine(root, "actions.json"), "\"action_id\": \"ApplyDamage\"", "\"action_id\": \"StatDrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_monster_ability");
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_RepositoryDefinitions_LoadsOrderedTerrainRegistry()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var terrain = success.Catalog.Terrain.All;
        var ids = terrain.Select(definition => definition.Id).ToArray();
        Assert.True(success.Catalog.Terrain.TryGet("open_floor", out _));
        Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), ids);
        Assert.True(Assert.IsAssignableFrom<ICollection<TerrainDefinition>>(terrain).IsReadOnly);
    }

    [Fact]
    public async Task LoadAsync_DuplicateTerrainId_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "environment", "terrain_definitions.json"), "\"id\": \"open_floor\"", "\"id\": \"darkness\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "duplicate_id");
    }

    [Fact]
    public async Task LoadAsync_UnknownTerrainReference_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "environment", "terrain_definitions.json"), "\"appears_as\": \"open_floor\"", "\"appears_as\": \"missing_terrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_terrain");
    }

    [Fact]
    public async Task LoadAsync_MultipleUnknownTerrainReferences_ReturnsAllErrorsAndNoCatalog()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "environment", "terrain_definitions.json"), "\"appears_as\": \"open_floor\"", "\"appears_as\": \"missing_terrain\"");
        ReplaceFirst(Path.Combine(root, "environment", "terrain_definitions.json"), "\"appears_as\": \"crops\"", "\"appears_as\": \"another_missing_terrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.True(failure.Report.Errors.Count(error => error.Code == "unknown_terrain") >= 2);
    }

    [Fact]
    public async Task LoadAsync_RepositoryDefinitions_LoadsOrderedTrapRegistry()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var traps = success.Catalog.Traps.All;
        var ids = traps.Select(trap => trap.Id).ToArray();
        Assert.True(success.Catalog.Traps.TryGet("trap_door", out _));
        Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), ids);
        Assert.True(Assert.IsAssignableFrom<ICollection<TrapDefinition>>(traps).IsReadOnly);
    }

    [Fact]
    public async Task LoadAsync_DuplicateTrapId_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "environment", "traps.json"), "\"id\": \"explosive_trap\"", "\"id\": \"gas_paralyze\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "duplicate_id");
    }

    [Fact]
    public async Task LoadAsync_UnknownTrapAction_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "environment", "traps.json"), "\"action_id\": \"ParalyzeControl\"", "\"action_id\": \"StatDrain\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_action");
    }

    [Fact]
    public async Task LoadAsync_UnknownTrapStatus_ReturnsValidationReport()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "environment", "traps.json"), "\"status_id\": \"cut\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        AssertError(result, "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_MultipleUnknownTrapReferences_ReturnsAllErrorsAndNoCatalog()
    {
        var root = CreateDefinitionsCopy();
        ReplaceFirst(Path.Combine(root, "environment", "traps.json"), "\"action_id\": \"ParalyzeControl\"", "\"action_id\": \"StatDrain\"");
        ReplaceFirst(Path.Combine(root, "environment", "traps.json"), "\"status_id\": \"cut\"", "\"status_id\": \"missing_status\"");

        var result = await DefinitionCatalogLoader.LoadAsync(root);

        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_action");
        Assert.Contains(failure.Report.Errors, error => error.Code == "unknown_status");
    }

    [Fact]
    public async Task LoadAsync_OrdersRegistryEntriesByOrdinalId()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var ids = success.Catalog.Races.All.Select(race => race.Id).ToArray();
        Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), ids);
    }

    [Fact]
    public async Task LoadAsync_ExposesReadOnlyRegistryEntries()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);

        var success = Assert.IsType<DefinitionLoadSuccess>(result);
        var races = Assert.IsAssignableFrom<ICollection<RaceDefinition>>(success.Catalog.Races.All);

        Assert.True(races.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => races.Add(success.Catalog.Races.GetRequired("human")));
    }

    public void Dispose()
    {
        foreach (var temporaryRoot in _temporaryRoots)
        {
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    private static string RepositoryDefinitionsRoot => Path.Combine(RepositoryRoot, "data", "definitions");

    private static string RepositoryRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private string CreateDefinitionsCopy()
    {
        var root = Path.Combine(Path.GetTempPath(), "IronHell.Data.Tests", Guid.NewGuid().ToString("N"));
        CopyDirectory(Path.Combine(RepositoryRoot, "data"), Path.Combine(root, "data"));
        var definitionsRoot = Path.Combine(root, "data", "definitions");
        _temporaryRoots.Add(root);
        return definitionsRoot;
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        }

        foreach (var directory in Directory.GetDirectories(source))
        {
            CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
        }
    }

    private static void ReplaceFirst(string path, string oldValue, string newValue)
    {
        var content = File.ReadAllText(path);
        Assert.Contains(oldValue, content, StringComparison.Ordinal);
        File.WriteAllText(path, content.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    private static void AssertError(IDefinitionLoadResult result, string code)
    {
        var failure = Assert.IsType<DefinitionLoadFailure>(result);
        Assert.Contains(failure.Report.Errors, error => error.Code == code);
    }
}