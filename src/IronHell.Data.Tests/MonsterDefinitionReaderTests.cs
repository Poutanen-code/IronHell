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
        Assert.Equal(new MonsterAiDefinition("wanderer", 25, false, false), Get(monsters, "filthy_street_urchin").Ai);
        Assert.Equal(new MonsterAiDefinition("wanderer", 50, false, false), Get(monsters, "singing_happy_drunk").Ai);
        Assert.Equal(75, Get(monsters, "white_icky_thing").Ai.RandomMoveChance);
        Assert.True(Get(monsters, "grey_mold").Ai.Stupid);
        Assert.Equal(["open_doors", "take_items"], Get(monsters, "filthy_street_urchin").Capabilities);
        Assert.Contains("cold_blooded", Get(monsters, "poltergeist").Capabilities);
        Assert.Contains("hurt_by_light", Get(monsters, "poltergeist").Capabilities);
        Assert.Contains("hurt_by_rock_removal", Get(monsters, "earth_spirit").Capabilities);
        Assert.Contains("powerful", Get(monsters, "fire_vortex").Capabilities);
        Assert.Contains("never_move", Get(monsters, "grey_mold").Capabilities);
        Assert.Contains("never_blow", Get(monsters, "giant_black_dragon_fly").Capabilities);
        Assert.Contains("destroy_items", Get(monsters, "fire_elemental").Capabilities);
        Assert.Equal(new MonsterSensesDefinition(40, MonsterTelepathyProfile.Normal), Get(monsters, "filthy_street_urchin").Senses);
        Assert.Equal(MonsterTelepathyProfile.WeirdMind, Get(monsters, "giant_yellow_centipede").Senses.TelepathyProfile);
        Assert.Equal(MonsterTelepathyProfile.EmptyMind, Get(monsters, "grey_mold").Senses.TelepathyProfile);
        Assert.Equal(["imm_sleep", "imm_confu"], Get(monsters, "farmer_maggot").Resistances);
        Assert.Equal(["imm_pois", "imm_sleep", "imm_fear", "imm_confu"], Get(monsters, "grey_mold").Resistances);
        Assert.Equal(new SpawnPolicy(false, false, false, false, false, false, false, false, false), Get(monsters, "mean_mercenary").SpawnPolicy);
    }

    [Fact]
    public void ReadCapabilities_RepositoryDefinitions_LoadsCanonicalCatalog()
    {
        var report = new DefinitionValidationReport();

        var capabilities = MonsterDefinitionReader.ReadCapabilities(
            LoadJson("data/definitions/monsters/monster_capabilities.json"), report);

        Assert.False(report.HasErrors);
        Assert.Equal(18, capabilities.Count);
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
    public void ValidateMonsters_UnknownResistance_ReturnsValidationError()
    {
        var monster = CreateMonsterNode([]);
        monster["resistances"] = new JsonArray("missing_resistance");

        var report = ValidateMonster(monster);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "unknown_monster_resistance");
    }

    [Fact]
    public void ValidateMonsters_DuplicateResistance_ReturnsValidationError()
    {
        var monster = CreateMonsterNode([]);
        monster["resistances"] = new JsonArray("imm_fire", "imm_fire");

        var report = ValidateMonster(monster);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "duplicate_monster_resistance");
    }

    [Fact]
    public void ReadMonsters_LoadsSpawnPolicy()
    {
        var report = new DefinitionValidationReport();
        var document = new JsonObject
        {
            ["monsters"] = new JsonArray { CreateMonsterNode([]) },
        };
        document["monsters"]![0]! ["spawn_policy"] = new JsonObject
        {
            ["unique"] = true,
            ["questor"] = true,
            ["force_depth"] = true,
            ["force_max_hp"] = true,
            ["force_sleep"] = true,
            ["escort"] = true,
            ["escorts"] = true,
            ["friends"] = true,
            ["wanderer"] = true,
        };

        var monster = Assert.Single(MonsterDefinitionReader.ReadMonsters(document, report));

        Assert.False(report.HasErrors);
        Assert.Equal(new SpawnPolicy(true, true, true, true, true, true, true, true, true), monster.SpawnPolicy);
    }

    [Fact]
    public void ValidateMonsters_AllowsUniqueAndQuestorTogether()
    {
        var monster = CreateMonsterNode([]);
        monster["spawn_policy"] = new JsonObject { ["unique"] = true, ["questor"] = true };

        var report = ValidateMonster(monster);

        Assert.False(report.HasErrors);
    }

    [Fact]
    public void ValidateMonsters_InvalidSpawnPolicy_ReturnsValidationError()
    {
        var monster = CreateMonsterNode([]);
        monster["spawn_policy"] = new JsonObject { ["friends"] = "yes", ["unknown"] = true };

        var report = ValidateMonster(monster);

        Assert.Equal(2, report.ToImmutable().Errors.Count(error => error.Code == "invalid_spawn_policy"));
    }

    [Fact]
    public void ValidateMonsters_LegacySpawnPolicyFlag_ReturnsValidationError()
    {
        var monster = CreateMonsterNode([]);
        monster["flags"] = new JsonObject { ["friends"] = true };

        var report = ValidateMonster(monster);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == "legacy_spawn_policy_flag");
    }

    [Theory]
    [InlineData(-1, "normal", "invalid_monster_alertness")]
    [InlineData(0, "unknown", "invalid_telepathy_profile")]
    public void ValidateMonsters_InvalidSenses_ReturnsValidationError(int alertness, string profile, string expectedCode)
    {
        var monster = CreateMonsterNode([]);
        monster["senses"] = new JsonObject
        {
            ["alertness"] = alertness,
            ["telepathy_profile"] = profile,
        };

        var report = ValidateMonster(monster);

        Assert.Contains(report.ToImmutable().Errors, error => error.Code == expectedCode);
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
        ["resistances"] = new JsonArray(),
        ["senses"] = new JsonObject { ["alertness"] = 0, ["telepathy_profile"] = "normal" },
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
        new DefinitionRegistry<ResistanceDefinition>([new("imm_fire"), new("imm_sleep"), new("imm_fear"), new("imm_confu")]),
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