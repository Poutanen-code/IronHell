using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;
using IronHell.Data.Serialization;
using IronHell.Data.Validation;
using Xunit;

namespace IronHell.Data.Tests;

public sealed class MonsterDefinitionReaderTests
{
    [Fact]
    public void ReadMonsters_RepositoryDefinitions_LoadMigratedAiAndCapabilities()
    {
        var report = new DefinitionValidationReport();
        var document = LoadJson("data/definitions/monsters/monsters.json");

        var monsters = MonsterDefinitionReader.ReadMonsters(document, report);

        Assert.False(report.HasErrors);
        Assert.Equal(616, monsters.Count);
        Assert.Equal(new MonsterAiDefinition("wanderer", 25, false), Get(monsters, "filthy_street_urchin").Ai);
        Assert.Equal(50, Get(monsters, "singing_happy_drunk").Ai.RandomMoveChance);
        Assert.Equal(75, Get(monsters, "white_icky_thing").Ai.RandomMoveChance);
        Assert.True(Get(monsters, "grey_mold").Ai.Stupid);
        Assert.Equal(["open_doors", "take_items"], Get(monsters, "filthy_street_urchin").Capabilities);
    }

    [Fact]
    public void ReadCapabilities_RepositoryDefinitions_LoadsCanonicalCatalog()
    {
        var report = new DefinitionValidationReport();

        var capabilities = MonsterDefinitionReader.ReadCapabilities(
            LoadJson("data/definitions/monster_capabilities.json"), report);

        Assert.False(report.HasErrors);
        Assert.Equal(11, capabilities.Count);
        Assert.Equal("Open Doors", capabilities.Single(capability => capability.Id == "open_doors").Name);
    }

    [Fact]
    public void ValidateMonsters_UnknownCapability_ReturnsValidationError()
    {
        var report = ValidateMonsterCapabilities(["missing_capability"]);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "unknown_monster_capability");
    }

    [Fact]
    public void ValidateMonsters_DuplicateCapability_ReturnsValidationError()
    {
        var report = ValidateMonsterCapabilities(["open_doors", "open_doors"]);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "duplicate_monster_capability");
    }

    [Fact]
    public void ValidateMonsters_MissingAiBehavior_ReturnsValidationError()
    {
        var monster = CreateMonsterNode([]);
        monster["ai"]!.AsObject().Remove("behavior");

        var report = ValidateMonster(monster);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "missing_monster_ai");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void ValidateMonsters_OutOfRangeRandomMoveChance_ReturnsValidationError(int chance)
    {
        var monster = CreateMonsterNode([]);
        monster["ai"]!["random_move_chance"] = chance;

        var report = ValidateMonster(monster);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "invalid_random_move_chance");
    }

    [Fact]
    public void ReadCapabilities_DuplicateDefinitionId_ReturnsValidationError()
    {
        var report = new DefinitionValidationReport();
        var document = JsonNode.Parse("""
            { "capabilities": [
              { "id": "open_doors", "name": "Open Doors", "description": "One" },
              { "id": "open_doors", "name": "Open Doors Again", "description": "Two" }
            ] }
            """)!.AsObject();

        MonsterDefinitionReader.ReadCapabilities(document, report);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "duplicate_id");
    }

    private static DefinitionValidationReport ValidateMonsterCapabilities(string[] capabilityIds)
    {
        return ValidateMonster(CreateMonsterNode(capabilityIds));
    }

    private static JsonObject CreateMonsterNode(string[] capabilityIds) => new()
    {
        ["id"] = "test_monster",
        ["hp_roll"] = new JsonObject { ["kind"] = "dice", ["count"] = 1, ["sides"] = 1 },
        ["ai"] = new JsonObject { ["behavior"] = "wanderer", ["random_move_chance"] = 0 },
        ["capabilities"] = new JsonArray(capabilityIds.Select(id => (JsonNode?)JsonValue.Create(id)).ToArray()),
    };

    private static DefinitionValidationReport ValidateMonster(JsonObject monster)
    {
        var document = new JsonObject
        {
            ["monsters"] = new JsonArray { monster },
        };
        var report = new DefinitionValidationReport();
        MonsterValidator.ValidateMonsters(document, CreateRegistries(), report);
        return report;
    }

    private static ValidationRegistries CreateRegistries() => new(
        new DefinitionRegistry<ActionDefinition>([]),
        new DefinitionRegistry<StatusDefinition>([]),
        new DefinitionRegistry<CapabilityDefinition>([]),
        new DefinitionRegistry<ResistanceDefinition>([]),
        new DefinitionRegistry<ItemDefinition>([]),
        new DefinitionRegistry<SpellDefinition>([]),
        new DefinitionRegistry<SpellDefinition>([]),
        new DefinitionRegistry<ActivationDefinition>([]),
        new DefinitionRegistry<MonsterAbilityDefinition>([]),
        new DefinitionRegistry<MonsterCapabilityDefinition>([new("open_doors", "Open Doors", "Monster can open doors.")]),
        new DefinitionRegistry<MonsterDefinition>([]),
        new DefinitionRegistry<TerrainDefinition>([]),
        new DefinitionRegistry<TrapDefinition>([]));

    private static MonsterDefinition Get(IEnumerable<MonsterDefinition> monsters, string id) =>
        monsters.Single(monster => monster.Id == id);

    private static JsonObject LoadJson(string relativePath) =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(RepositoryRoot, relativePath)).TrimStart('\uFEFF'))!.AsObject();

    private static string RepositoryRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}