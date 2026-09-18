using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using IronHell.Core.Randomness;
using IronHell.Core.World;
using Xunit;

namespace IronHell.Core.Tests.World;

public sealed class WorldRuntimeStateTests
{
    [Fact]
    public void Constructor_SameCatalogAndSeed_ReconstructsIdenticalAssignments()
    {
        var catalog = CreateCatalog();

        var first = new WorldRuntimeState(catalog, new FlavorSeed(42));
        var second = new WorldRuntimeState(catalog, new FlavorSeed(42));

        Assert.Equal(first.FlavorAssignments.Assignments, second.FlavorAssignments.Assignments);
    }

    [Fact]
    public void CharacterBootstrapService_Create_TwoCharacters_ShareTheSameWorldFlavorAssignments()
    {
        var catalog = CreateCatalog();
        var world = new WorldRuntimeState(catalog, new FlavorSeed(7));

        var first = CharacterBootstrapService.Create(catalog, "character-1", "human", "warrior", world);
        var second = CharacterBootstrapService.Create(catalog, "character-2", "human", "warrior", world);

        var firstRing = Assert.Single(first.Inventory.Items);
        var secondRing = Assert.Single(second.Inventory.Items);

        Assert.NotNull(firstRing.FlavorId);
        Assert.Equal(firstRing.FlavorId, secondRing.FlavorId);
    }

    [Fact]
    public void CharacterBootstrapService_Create_DoesNotRegenerateWorldFlavorAssignments()
    {
        var catalog = CreateCatalog();
        var world = new WorldRuntimeState(catalog, new FlavorSeed(7));
        var assignmentsBefore = world.FlavorAssignments.Assignments;

        CharacterBootstrapService.Create(catalog, "character-1", "human", "warrior", world);
        CharacterBootstrapService.Create(catalog, "character-2", "human", "warrior", world);

        Assert.Same(world.FlavorAssignments, world.FlavorAssignments);
        Assert.Equal(assignmentsBefore, world.FlavorAssignments.Assignments);
    }

    [Fact]
    public void CharacterBootstrapService_Create_WithoutWorld_LeavesItemsUnflavored()
    {
        var catalog = CreateCatalog();

        var character = CharacterBootstrapService.Create(catalog, "character-1", "human", "warrior");

        Assert.Null(Assert.Single(character.Inventory.Items).FlavorId);
    }

    private static TestCatalog CreateCatalog()
    {
        var races = new TestRegistry<RaceDefinition>([
            new RaceDefinition("human", "Human", default, default, 10, 100, 0, 1, [], []),
        ]);
        var classes = new TestRegistry<ClassDefinition>([
            new ClassDefinition("warrior", "Warrior", default, default, default, 9, 100, null, 0, 0, 1, 0, 1, 0, 0, [], [new StartingEquipmentEntry("ring_of_feather_falling", 1, 1)]),
        ]);
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("ring_of_feather_falling", ItemCategory.Ring, "ring"),
        ]);
        var flavors = new TestRegistry<FlavorDefinition>([
            new FlavorDefinition("ring_ruby", FlavorCategory.Ring, "Ruby", "=", "r", new FlavorLegacyMetadata(28, 45, null), "verified"),
            new FlavorDefinition("ring_jade", FlavorCategory.Ring, "Jade", "=", "G", new FlavorLegacyMetadata(16, 45, null), "verified"),
        ]);
        var rules = new[] { new RaceClassRule("human", "warrior") };

        return new TestCatalog(races, classes, items, flavors, rules);
    }

    private sealed class TestCatalog(
        IDefinitionRegistry<RaceDefinition> races,
        IDefinitionRegistry<ClassDefinition> classes,
        IDefinitionRegistry<ItemDefinition> items,
        IDefinitionRegistry<FlavorDefinition> flavors,
        IReadOnlyCollection<RaceClassRule> raceClassRules) : IDefinitionCatalog
    {
        public IDefinitionRegistry<ActionDefinition> Actions { get; } = TestRegistry<ActionDefinition>.Empty;
        public IDefinitionRegistry<StatusDefinition> Statuses { get; } = TestRegistry<StatusDefinition>.Empty;
        public IDefinitionRegistry<CapabilityDefinition> Capabilities { get; } = TestRegistry<CapabilityDefinition>.Empty;
        public IDefinitionRegistry<ResistanceDefinition> Resistances { get; } = TestRegistry<ResistanceDefinition>.Empty;
        public IDefinitionRegistry<RaceDefinition> Races { get; } = races;
        public IDefinitionRegistry<ClassDefinition> Classes { get; } = classes;
        public IDefinitionRegistry<ItemDefinition> Items { get; } = items;
        public IDefinitionRegistry<FlavorDefinition> Flavors { get; } = flavors;
        public IDefinitionRegistry<SpellDefinition> MageSpells { get; } = TestRegistry<SpellDefinition>.Empty;
        public IDefinitionRegistry<SpellDefinition> PriestPrayers { get; } = TestRegistry<SpellDefinition>.Empty;
        public IDefinitionRegistry<ActivationDefinition> Activations { get; } = TestRegistry<ActivationDefinition>.Empty;
        public IDefinitionRegistry<MonsterAbilityDefinition> MonsterAbilities { get; } = TestRegistry<MonsterAbilityDefinition>.Empty;
        public IDefinitionRegistry<MonsterCapabilityDefinition> MonsterCapabilities { get; } = TestRegistry<MonsterCapabilityDefinition>.Empty;
        public IDefinitionRegistry<MonsterLootProfileDefinition> MonsterLootProfiles { get; } = TestRegistry<MonsterLootProfileDefinition>.Empty;
        public IDefinitionRegistry<MonsterDefinition> Monsters { get; } = TestRegistry<MonsterDefinition>.Empty;
        public IDefinitionRegistry<TerrainDefinition> Terrain { get; } = TestRegistry<TerrainDefinition>.Empty;
        public IDefinitionRegistry<TrapDefinition> Traps { get; } = TestRegistry<TrapDefinition>.Empty;
        public IReadOnlyCollection<RaceClassRule> RaceClassRules { get; } = raceClassRules;
    }

    private sealed class TestRegistry<T>(IEnumerable<T> definitions) : IDefinitionRegistry<T>
        where T : IIdentifiedDefinition
    {
        private readonly FrozenDictionary<string, T> _definitions = definitions.ToFrozenDictionary(definition => definition.Id, StringComparer.Ordinal);

        public static TestRegistry<T> Empty { get; } = new([]);

        public IReadOnlyCollection<T> All { get; } = Array.AsReadOnly(definitions.OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray());

        public bool TryGet(string id, [NotNullWhen(true)] out T? definition) => _definitions.TryGetValue(id, out definition);

        public T GetRequired(string id) => TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Definition '{id}' was not found in {typeof(T).Name} registry.");
    }
}
