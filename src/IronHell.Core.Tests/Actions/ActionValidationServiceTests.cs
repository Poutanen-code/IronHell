using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Actions;
using IronHell.Core.Definitions;
using Xunit;

namespace IronHell.Core.Tests.Actions;

public sealed class ActionValidationServiceTests
{
    [Fact]
    public void Validate_ValidAction_Succeeds()
    {
        var result = Validate(
            new Dictionary<string, ActionParameterValue>
            {
                ["damage_type"] = new("fire"),
                ["radius"] = new(2),
            },
            ActionSourceFamily.Spell);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_AggregatesMissingUnknownEnumSourceAndRangeErrors()
    {
        var result = Validate(
            new Dictionary<string, ActionParameterValue>
            {
                ["damage_type"] = new("void"),
                ["radius"] = new(-1),
                ["unknown"] = new(true),
            },
            ActionSourceFamily.Wand);

        Assert.False(result.IsValid);
        Assert.Equal(
        [
            "Action 'BallDamage' does not allow source family 'Wand'.",
            "Action 'BallDamage' does not declare parameter 'unknown'.",
            "Action 'BallDamage' parameter 'damage_type' has invalid value 'void'.",
            "Action 'BallDamage' parameter 'radius' must be at least 0.",
        ], result.Errors);
    }

    [Fact]
    public void Validate_MissingRequiredParameter_Fails()
    {
        var result = Validate(new Dictionary<string, ActionParameterValue>(), ActionSourceFamily.Spell);

        Assert.False(result.IsValid);
        Assert.Equal(
        [
            "Action 'BallDamage' requires parameter 'damage_type'.",
            "Action 'BallDamage' requires parameter 'radius'.",
        ], result.Errors);
    }

    [Fact]
    public void Validate_InvalidValueType_Fails()
    {
        var result = Validate(
            new Dictionary<string, ActionParameterValue>
            {
                ["damage_type"] = new(42),
                ["radius"] = new(1),
            },
            ActionSourceFamily.Spell);

        Assert.False(result.IsValid);
        Assert.Contains("parameter 'damage_type' has invalid value type", Assert.Single(result.Errors));
    }

    [Fact]
    public void TargetContracts_PreserveTargetContextAndCommandInputs()
    {
        var context = new TargetContext(TargetMode.Area, ["tile:4:5"]);
        var command = new ActionCommand(
            "BallDamage",
            ActionSourceFamily.Spell,
            new Dictionary<string, ActionParameterValue> { ["radius"] = new(2) },
            new ActionSourceActor("caster-1"),
            context);

        Assert.Equal("area", command.TargetContext.Mode.Value);
        Assert.Equal(["tile:4:5"], command.TargetContext.TargetIds);
        Assert.Equal("caster-1", command.SourceActor.ActorId);
    }

    private static ActionValidationResult Validate(
        IReadOnlyDictionary<string, ActionParameterValue> parameters,
        ActionSourceFamily sourceFamily)
    {
        var action = new ActionDefinition(
            "BallDamage",
            AllowedSourceFamilies: new HashSet<ActionSourceFamily> { ActionSourceFamily.Spell },
            ParameterContract: new ActionParameterContract(
                true,
                [
                    new("damage_type", "Damage channel", ActionParameterValueType.Enum, true, AllowedValues: new HashSet<string> { "fire", "cold" }),
                    new("radius", "Radius", ActionParameterValueType.Integer, true, Minimum: 0, Maximum: 8),
                ]),
            ValidationRules: new ActionValidationRules(true, true, true));

        return new ActionValidationService().Validate(
            new ActionCommand("BallDamage", sourceFamily, parameters, new ActionSourceActor("caster-1"), new TargetContext(TargetMode.Aimed, [])),
            new TestRegistry<ActionDefinition>([action]));
    }

    private sealed class TestRegistry<T> : IDefinitionRegistry<T>
        where T : IIdentifiedDefinition
    {
        private readonly FrozenDictionary<string, T> _definitions;

        public TestRegistry(IEnumerable<T> definitions)
        {
            _definitions = definitions.ToFrozenDictionary(definition => definition.Id, StringComparer.Ordinal);
        }

        public IReadOnlyCollection<T> All => _definitions.Values;

        public bool TryGet(string id, [NotNullWhen(true)] out T? definition) => _definitions.TryGetValue(id, out definition);

        public T GetRequired(string id) => TryGet(id, out var definition) ? definition : throw new KeyNotFoundException();
    }
}