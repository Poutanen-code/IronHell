using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Actions;
using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Actions;

public sealed class ActionExecutorTests
{
    [Fact]
    public void Execute_HealHp_ClampsToMaximum()
    {
        var target = CreateCharacter(currentHp: 8, maxHp: 10);

        var result = Execute(target, [new ActionInvocation("HealHP", Amount: 5)]);

        Assert.True(result.Success);
        Assert.Equal(10, target.State.CurrentHp);
        Assert.Equal(["HealHP:8->10"], result.AppliedChanges);
    }

    [Fact]
    public void Execute_ApplyDamage_ReducesHitPoints()
    {
        var target = CreateCharacter(currentHp: 10, maxHp: 10);

        var result = Execute(target, [new ActionInvocation("ApplyDamage", Amount: 3)]);

        Assert.True(result.Success);
        Assert.Equal(7, target.State.CurrentHp);
        Assert.False(target.State.IsDead);
    }

    [Fact]
    public void Execute_ApplyDamage_ExactLethalDamageMarksCharacterDead()
    {
        var target = CreateCharacter(currentHp: 5, maxHp: 10);

        var result = Execute(target, [new ActionInvocation("ApplyDamage", Amount: 5)]);

        Assert.True(result.Success);
        Assert.Equal(0, target.State.CurrentHp);
        Assert.True(target.State.IsDead);
    }

    [Fact]
    public void Execute_ApplyDamage_OverkillClampsHitPointsToZero()
    {
        var target = CreateCharacter(currentHp: 5, maxHp: 10);

        var result = Execute(target, [new ActionInvocation("ApplyDamage", Amount: 8)]);

        Assert.True(result.Success);
        Assert.Equal(0, target.State.CurrentHp);
        Assert.True(target.State.IsDead);
    }

    [Fact]
    public void Execute_DeadCharacterRemainsDead()
    {
        var target = CreateCharacter(currentHp: 1, maxHp: 10);

        Execute(target, [new ActionInvocation("ApplyDamage", Amount: 1)]);
        var result = Execute(target, [new ActionInvocation("HealHP", Amount: 5)]);

        Assert.False(result.Success);
        Assert.Equal(0, target.State.CurrentHp);
        Assert.True(target.State.IsDead);
    }

    [Fact]
    public void Execute_IdenticalDamageInputs_ProduceIdenticalDeathTransitions()
    {
        var first = CreateCharacter(currentHp: 4, maxHp: 10);
        var second = CreateCharacter(currentHp: 4, maxHp: 10);
        var actions = new[] { new ActionInvocation("ApplyDamage", Amount: 4) };

        var firstResult = Execute(first, actions);
        var secondResult = Execute(second, actions);

        Assert.Equal(firstResult.AppliedChanges, secondResult.AppliedChanges);
        Assert.Equal(first.State.CurrentHp, second.State.CurrentHp);
        Assert.Equal(first.State.IsDead, second.State.IsDead);
    }

    [Fact]
    public void Execute_ApplyStatus_AddsActiveStatus()
    {
        var target = CreateCharacter();

        var result = Execute(target, [new ActionInvocation("ApplyStatus", StatusId: "blessed")]);

        Assert.True(result.Success);
        AssertStatus(target.ActiveStatuses, "blessed", 12);
    }

    [Fact]
    public void Execute_ApplyStatusWithDiceResolver_UsesSeededDefinitionDuration()
    {
        var target = CreateCharacter();
        var definition = new StatusDefinition(
            "blessed",
            StatusApplicationPolicy.ReplaceExisting,
            new StatusDurationDefinition(10, DiceCount: 2, DiceSides: 6, LevelMultiplier: null));
        var catalog = new TestCatalog(
            new TestRegistry<ActionDefinition>([new("ApplyStatus")]),
            new TestRegistry<StatusDefinition>([definition]));
        var expectedRandom = new SeededRandomSource(12345);

        var result = new ActionExecutor(new DiceDurationResolver(new SeededRandomSource(12345)))
            .Execute(new ActionExecutionContext(catalog, CreateCharacter(), target), [new ActionInvocation("ApplyStatus", StatusId: "blessed")]);

        Assert.True(result.Success);
        AssertStatus(target.ActiveStatuses, "blessed", 10 + expectedRandom.RollDice(2, 6));
    }

    [Theory]
    [InlineData(StatusApplicationPolicy.IgnoreIfPresent, 5, false)]
    [InlineData(StatusApplicationPolicy.RefreshDuration, 12, false)]
    [InlineData(StatusApplicationPolicy.ReplaceExisting, 12, true)]
    public void Execute_ApplyStatus_UsesDefinitionApplicationPolicy(
        StatusApplicationPolicy policy,
        int expectedDuration,
        bool expectsReplacement)
    {
        var target = CreateCharacter();
        var original = new ActiveStatus("blessed", 5);
        target.ApplyStatus(original);

        var result = Execute(target, [new ActionInvocation("ApplyStatus", StatusId: "blessed")], CreateCatalog(policy));

        Assert.True(result.Success);
        Assert.Equal(expectsReplacement, !ReferenceEquals(original, Assert.Single(target.ActiveStatuses)));
        AssertStatus(target.ActiveStatuses, "blessed", expectedDuration);
    }

    [Fact]
    public void Execute_CureStatus_RemovesMatchingActiveStatuses()
    {
        var target = CreateCharacter();
        target.ApplyStatus(new ActiveStatus("blessed", 12));
        target.ApplyStatus(new ActiveStatus("poisoned", 8));

        var result = Execute(target, [new ActionInvocation("CureStatus", StatusIds: ["blessed"])]);

        Assert.True(result.Success);
        AssertStatus(target.ActiveStatuses, "poisoned", 8);
        Assert.Equal(["CureStatus:1"], result.AppliedChanges);
    }

    [Fact]
    public void Execute_ApplyStatusWithUnknownStatus_ReturnsFailureWithoutMutation()
    {
        var target = CreateCharacter();

        var result = Execute(target, [new ActionInvocation("ApplyStatus", StatusId: "missing_status")]);

        Assert.False(result.Success);
        Assert.Equal(["Unknown status 'missing_status'."], result.ValidationFailures);
        Assert.Empty(target.ActiveStatuses);
    }

    [Fact]
    public void Execute_CureStatusWithUnknownStatus_ReturnsFailureWithoutMutation()
    {
        var target = CreateCharacter();
        target.ApplyStatus(new ActiveStatus("blessed", 12));

        var result = Execute(target, [new ActionInvocation("CureStatus", StatusIds: ["missing_status"])]);

        Assert.False(result.Success);
        AssertStatus(target.ActiveStatuses, "blessed", 12);
    }

    [Fact]
    public void Execute_MultipleActions_AppliesInInputOrder()
    {
        var target = CreateCharacter(currentHp: 5, maxHp: 10);

        var result = Execute(target,
        [
            new ActionInvocation("HealHP", Amount: 3),
            new ActionInvocation("ApplyStatus", StatusId: "blessed"),
            new ActionInvocation("CureStatus", StatusIds: ["blessed"]),
        ]);

        Assert.True(result.Success);
        Assert.Equal(8, target.State.CurrentHp);
        Assert.Empty(target.ActiveStatuses);
        Assert.Equal(["HealHP:5->8", "ApplyStatus:blessed", "CureStatus:1"], result.AppliedChanges);
    }

    [Fact]
    public void Execute_IdenticalInputs_ProduceIdenticalResults()
    {
        var first = CreateCharacter(currentHp: 4, maxHp: 10);
        var second = CreateCharacter(currentHp: 4, maxHp: 10);
        var actions = new[]
        {
            new ActionInvocation("HealHP", Amount: 3),
            new ActionInvocation("ApplyStatus", StatusId: "blessed"),
        };

        var firstResult = Execute(first, actions);
        var secondResult = Execute(second, actions);

        Assert.Equal(firstResult.Success, secondResult.Success);
        Assert.Equal(firstResult.AppliedChanges, secondResult.AppliedChanges);
        Assert.Equal(firstResult.ValidationFailures, secondResult.ValidationFailures);
        Assert.Equal(first.State.CurrentHp, second.State.CurrentHp);
        Assert.Equal(first.ActiveStatuses.Select(status => (status.StatusId, status.RemainingDuration)), second.ActiveStatuses.Select(status => (status.StatusId, status.RemainingDuration)));
    }

    private static ActionExecutionResult Execute(
        Character target,
        IReadOnlyCollection<ActionInvocation> actions,
        IDefinitionCatalog? catalog = null) =>
        new ActionExecutor().Execute(new ActionExecutionContext(catalog ?? CreateCatalog(), CreateCharacter(), target), actions);

    private static Character CreateCharacter(int currentHp = 10, int maxHp = 10)
    {
        var character = new Character("character-1", "human", "warrior");
        character.State.CurrentHp = currentHp;
        character.State.MaxHp = maxHp;
        return character;
    }

    private static void AssertStatus(IReadOnlyCollection<ActiveStatus> statuses, string statusId, int remainingDuration)
    {
        var status = Assert.Single(statuses);
        Assert.Equal(statusId, status.StatusId);
        Assert.Equal(remainingDuration, status.RemainingDuration);
    }

    private static IDefinitionCatalog CreateCatalog(StatusApplicationPolicy policy = StatusApplicationPolicy.ReplaceExisting) => new TestCatalog(
        new TestRegistry<ActionDefinition>([new("ApplyDamage"), new("HealHP"), new("ApplyStatus"), new("CureStatus")]),
        new TestRegistry<StatusDefinition>([new("blessed", policy, new StatusDurationDefinition(12, null, null, null)), new("poisoned", StatusApplicationPolicy.ReplaceExisting, new StatusDurationDefinition(8, null, null, null))]));

    private sealed class TestCatalog(
        IDefinitionRegistry<ActionDefinition> actions,
        IDefinitionRegistry<StatusDefinition> statuses) : IDefinitionCatalog
    {
        public IDefinitionRegistry<ActionDefinition> Actions { get; } = actions;
        public IDefinitionRegistry<StatusDefinition> Statuses { get; } = statuses;
        public IDefinitionRegistry<CapabilityDefinition> Capabilities { get; } = TestRegistry<CapabilityDefinition>.Empty;
        public IDefinitionRegistry<ResistanceDefinition> Resistances { get; } = TestRegistry<ResistanceDefinition>.Empty;
        public IDefinitionRegistry<RaceDefinition> Races { get; } = TestRegistry<RaceDefinition>.Empty;
        public IDefinitionRegistry<ClassDefinition> Classes { get; } = TestRegistry<ClassDefinition>.Empty;
        public IDefinitionRegistry<ItemDefinition> Items { get; } = TestRegistry<ItemDefinition>.Empty;
        public IDefinitionRegistry<SpellDefinition> MageSpells { get; } = TestRegistry<SpellDefinition>.Empty;
        public IDefinitionRegistry<SpellDefinition> PriestPrayers { get; } = TestRegistry<SpellDefinition>.Empty;
        public IDefinitionRegistry<ActivationDefinition> Activations { get; } = TestRegistry<ActivationDefinition>.Empty;
        public IDefinitionRegistry<MonsterAbilityDefinition> MonsterAbilities { get; } = TestRegistry<MonsterAbilityDefinition>.Empty;
        public IDefinitionRegistry<MonsterDefinition> Monsters { get; } = TestRegistry<MonsterDefinition>.Empty;
        public IDefinitionRegistry<TerrainDefinition> Terrain { get; } = TestRegistry<TerrainDefinition>.Empty;
        public IDefinitionRegistry<TrapDefinition> Traps { get; } = TestRegistry<TrapDefinition>.Empty;
        public IReadOnlyCollection<RaceClassRule> RaceClassRules { get; } = Array.Empty<RaceClassRule>();
    }

    private sealed class TestRegistry<T> : IDefinitionRegistry<T>
        where T : IIdentifiedDefinition
    {
        private readonly FrozenDictionary<string, T> _definitions;

        public TestRegistry(IEnumerable<T> definitions)
        {
            _definitions = definitions.ToFrozenDictionary(definition => definition.Id, StringComparer.Ordinal);
            All = Array.AsReadOnly(_definitions.Values.OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray());
        }

        public static TestRegistry<T> Empty { get; } = new(Array.Empty<T>());

        public IReadOnlyCollection<T> All { get; }

        public bool TryGet(string id, [NotNullWhen(true)] out T? definition) => _definitions.TryGetValue(id, out definition);

        public T GetRequired(string id) => TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException();
    }
}