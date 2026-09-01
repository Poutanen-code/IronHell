using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Actions;
using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using IronHell.Core.Randomness;
using IronHell.Core.Spells;
using Xunit;

namespace IronHell.Core.Tests.Spells;

public sealed class SpellExecutionServiceTests
{
    private const string HealAction = "HealHP";
    private const string StatusAction = "ApplyStatus";
    private const string CureAction = "CureStatus";
    private const string BlessedStatus = "blessed";

    [Fact]
    public void Execute_UnknownSpell_ReturnsValidationFailure()
    {
        var caster = CreateCharacter(5);

        var result = CreateService().Execute(CreateCatalog(), "missing_spell", caster);

        Assert.False(result.Success);
        Assert.Equal("Unknown spell 'missing_spell'.", Assert.Single(result.ValidationFailures));
        Assert.Empty(result.Events);
        Assert.Equal(5, caster.State.CurrentHp);
    }

    [Fact]
    public void Execute_SpellWithoutActionRefs_ReturnsValidationFailure()
    {
        var catalog = CreateCatalog(new SpellDefinition("empty_spell"));

        var result = CreateService().Execute(catalog, "empty_spell", CreateCharacter(5));

        Assert.False(result.Success);
        Assert.Equal("Spell 'empty_spell' has no executable action references.", Assert.Single(result.ValidationFailures));
    }

    [Fact]
    public void Execute_SelfTargetSpell_HealsCasterAndEmitsEvent()
    {
        var caster = CreateCharacter(4);
        var catalog = CreateCatalog(SelfHealSpell());

        var result = CreateService().Execute(catalog, "self_heal", caster);

        Assert.True(result.Success);
        Assert.Equal(9, caster.State.CurrentHp);
        var hpChanged = Assert.IsType<HpChangedEvent>(Assert.Single(result.Events));
        Assert.Equal(4, hpChanged.BeforeValue);
        Assert.Equal(9, hpChanged.AfterValue);
    }

    [Fact]
    public void Execute_OtherTargetSpell_AppliesToProvidedTarget()
    {
        var caster = CreateCharacter(10);
        var target = CreateCharacter(3);
        var catalog = CreateCatalog(OtherTargetHealSpell());

        var result = CreateService().Execute(catalog, "other_heal", caster, target);

        Assert.True(result.Success);
        Assert.Equal(10, caster.State.CurrentHp);
        Assert.Equal(8, target.State.CurrentHp);
    }

    [Fact]
    public void Execute_OtherTargetSpellWithoutTarget_ReturnsValidationFailure()
    {
        var caster = CreateCharacter(10);
        var catalog = CreateCatalog(OtherTargetHealSpell());

        var result = CreateService().Execute(catalog, "other_heal", caster);

        Assert.False(result.Success);
        Assert.Equal("Action 'HealHP' requires another character target.", Assert.Single(result.ValidationFailures));
        Assert.Empty(result.Events);
    }

    [Fact]
    public void Execute_UnsupportedAmountKind_ReturnsValidationFailureWithoutMutation()
    {
        var caster = CreateCharacter(4);
        var spell = new SpellDefinition("formula_spell", [
            new SpellActionRef(HealAction, ActionTargetMode.Self, new SpellActionAmount("runtime_formula", null, null, null))
        ]);

        var result = CreateService().Execute(CreateCatalog(spell), "formula_spell", caster);

        Assert.False(result.Success);
        Assert.Equal("Action 'HealHP' uses unsupported amount kind 'runtime_formula'.", Assert.Single(result.ValidationFailures));
        Assert.Equal(4, caster.State.CurrentHp);
    }

    [Fact]
    public void Execute_MultiActionSpell_TranslatesActionRefsInDeclaredOrder()
    {
        var caster = CreateCharacter(4);
        StatusApplicationService.ApplyStatus(caster, BlessedStatus, 5, StatusApplicationPolicy.ReplaceExisting);
        var catalog = CreateCatalog(MultiActionSpell());

        var result = CreateService().Execute(catalog, "multi_spell", caster);

        Assert.True(result.Success);
        Assert.Equal(["HealHP:4->9", "ApplyStatus:blessed", "CureStatus:1"], result.AppliedChanges);
        Assert.Collection(
            result.Events,
            runtimeEvent => Assert.IsType<HpChangedEvent>(runtimeEvent),
            runtimeEvent => Assert.IsType<StatusAppliedEvent>(runtimeEvent),
            runtimeEvent => Assert.IsType<StatusRemovedEvent>(runtimeEvent));
    }

    [Fact]
    public void Execute_DiceAmountSpell_IsDeterministicForSameSeed()
    {
        var catalog = CreateCatalog(DiceHealSpell());

        var firstCaster = CreateCharacter(1);
        var firstResult = CreateService(new SeededRandomSource(4242)).Execute(catalog, "dice_heal", firstCaster);

        var secondCaster = CreateCharacter(1);
        var secondResult = CreateService(new SeededRandomSource(4242)).Execute(catalog, "dice_heal", secondCaster);

        Assert.Equal(firstResult.AppliedChanges, secondResult.AppliedChanges);
        Assert.Equal(firstCaster.State.CurrentHp, secondCaster.State.CurrentHp);
        Assert.Equal(
            firstResult.Events.Select(runtimeEvent => runtimeEvent.GetType()),
            secondResult.Events.Select(runtimeEvent => runtimeEvent.GetType()));
        Assert.True(firstCaster.State.CurrentHp > 1);
    }

    [Fact]
    public void Execute_SourcePolicyViolation_ReturnsValidationFailure()
    {
        var caster = CreateCharacter(4);
        var catalog = CreateCatalog(
            SelfHealSpell(),
            new ActionDefinition(HealAction, new HashSet<ActionSourceType> { ActionSourceType.Trap }, null));

        var result = CreateService().Execute(catalog, "self_heal", caster);

        Assert.False(result.Success);
        Assert.NotEmpty(result.ValidationFailures);
        Assert.Equal(4, caster.State.CurrentHp);
    }

    [Fact]
    public void Execute_PriestPrayer_UsesPrayerSourceType()
    {
        var caster = CreateCharacter(4);
        var prayer = new SpellDefinition("prayer_heal", [
            new SpellActionRef(HealAction, ActionTargetMode.Self, new SpellActionAmount("flat", 5, null, null))
        ]);
        var catalog = new TestCatalog(
            new TestRegistry<ActionDefinition>([
                new ActionDefinition(HealAction, new HashSet<ActionSourceType> { ActionSourceType.CharacterPrayer }, null)
            ]),
            TestRegistry<StatusDefinition>.Empty,
            TestRegistry<SpellDefinition>.Empty,
            new TestRegistry<SpellDefinition>([prayer]));

        var result = CreateService().Execute(catalog, "prayer_heal", caster);

        Assert.True(result.Success);
        Assert.Equal(9, caster.State.CurrentHp);
    }

    private static SpellDefinition SelfHealSpell() => new("self_heal", [
        new SpellActionRef(HealAction, ActionTargetMode.Self, new SpellActionAmount("flat", 5, null, null))
    ]);

    private static SpellDefinition OtherTargetHealSpell() => new("other_heal", [
        new SpellActionRef(HealAction, ActionTargetMode.OtherCharacter, new SpellActionAmount("flat", 5, null, null))
    ]);

    private static SpellDefinition DiceHealSpell() => new("dice_heal", [
        new SpellActionRef(HealAction, ActionTargetMode.Self, new SpellActionAmount("dice", null, 2, 10))
    ]);

    private static SpellDefinition MultiActionSpell() => new("multi_spell", [
        new SpellActionRef(HealAction, ActionTargetMode.Self, new SpellActionAmount("flat", 5, null, null)),
        new SpellActionRef(StatusAction, ActionTargetMode.Self, null, BlessedStatus),
        new SpellActionRef(CureAction, ActionTargetMode.Self, null, null, [BlessedStatus])
    ]);

    private static SpellExecutionService CreateService(IRandomSource? randomSource = null) =>
        new(new ActionExecutor(), randomSource ?? new SeededRandomSource(1));

    private static Character CreateCharacter(int currentHp)
    {
        var character = new Character("character-1", "human", "warrior");
        character.State.CurrentHp = currentHp;
        character.State.MaxHp = 20;
        return character;
    }

    private static IDefinitionCatalog CreateCatalog(SpellDefinition? spell = null, ActionDefinition? action = null)
    {
        var actions = action is null
            ? new TestRegistry<ActionDefinition>([new ActionDefinition(HealAction), new ActionDefinition(StatusAction), new ActionDefinition(CureAction)])
            : new TestRegistry<ActionDefinition>([action]);
        var spells = spell is null
            ? TestRegistry<SpellDefinition>.Empty
            : new TestRegistry<SpellDefinition>([spell]);
        return new TestCatalog(
            actions,
            new TestRegistry<StatusDefinition>([new StatusDefinition(BlessedStatus, StatusApplicationPolicy.ReplaceExisting, new StatusDurationDefinition(12, null, null, null))]),
            spells,
            TestRegistry<SpellDefinition>.Empty);
    }

    private sealed class TestCatalog(
        IDefinitionRegistry<ActionDefinition> actions,
        IDefinitionRegistry<StatusDefinition> statuses,
        IDefinitionRegistry<SpellDefinition> mageSpells,
        IDefinitionRegistry<SpellDefinition> priestPrayers) : IDefinitionCatalog
    {
        public IDefinitionRegistry<ActionDefinition> Actions { get; } = actions;
        public IDefinitionRegistry<StatusDefinition> Statuses { get; } = statuses;
        public IDefinitionRegistry<CapabilityDefinition> Capabilities { get; } = TestRegistry<CapabilityDefinition>.Empty;
        public IDefinitionRegistry<ResistanceDefinition> Resistances { get; } = TestRegistry<ResistanceDefinition>.Empty;
        public IDefinitionRegistry<RaceDefinition> Races { get; } = TestRegistry<RaceDefinition>.Empty;
        public IDefinitionRegistry<ClassDefinition> Classes { get; } = TestRegistry<ClassDefinition>.Empty;
        public IDefinitionRegistry<ItemDefinition> Items { get; } = TestRegistry<ItemDefinition>.Empty;
        public IDefinitionRegistry<SpellDefinition> MageSpells { get; } = mageSpells;
        public IDefinitionRegistry<SpellDefinition> PriestPrayers { get; } = priestPrayers;
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
