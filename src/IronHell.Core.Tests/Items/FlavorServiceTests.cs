using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Definitions;
using IronHell.Core.Items;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Items;

public sealed class FlavorServiceTests
{
    [Fact]
    public void GetRandomFlavorForCategory_ReturnsFlavorMatchingRequestedCategory()
    {
        var registry = new TestFlavorRegistry(
        [
            new FlavorDefinition("ring_ruby", FlavorCategory.Ring, "Ruby", "=", "r", new FlavorLegacyMetadata(28), "verified"),
            new FlavorDefinition("amulet_amber", FlavorCategory.Amulet, "Amber", "\"", "y", new FlavorLegacyMetadata(44), "verified"),
        ]);
        var service = new FlavorService(registry);

        var flavor = service.GetRandomFlavorForCategory(FlavorCategory.Ring, new SeededRandomSource(1));

        Assert.Equal("ring_ruby", flavor.Id);
    }

    [Fact]
    public void GetRandomFlavorForCategory_UsesScriptedBoundaryValuesInOrder()
    {
        var registry = new TestFlavorRegistry(
            Enumerable.Range(0, 50)
                .Select(index => new FlavorDefinition(
                    $"ring_{index:D2}",
                    FlavorCategory.Ring,
                    $"Flavor {index}",
                    "=",
                    "r",
                    new FlavorLegacyMetadata(index),
                    "test"))
                .ToArray());
        var random = new ScriptedRandomSource(0, 49);
        var service = new FlavorService(registry);

        var first = service.GetRandomFlavorForCategory(FlavorCategory.Ring, random);
        var second = service.GetRandomFlavorForCategory(FlavorCategory.Ring, random);

        Assert.Equal("ring_00", first.Id);
        Assert.Equal("ring_49", second.Id);
        Assert.Equal([(0, 50), (0, 50)], random.Requests);
    }

    [Fact]
    public void GetRandomFlavorForCategory_NoFlavorsForCategory_Throws()
    {
        var service = new FlavorService(new TestFlavorRegistry([]));

        Assert.Throws<KeyNotFoundException>(() => service.GetRandomFlavorForCategory(FlavorCategory.Wand, new SeededRandomSource(1)));
    }

    private sealed class TestFlavorRegistry(IReadOnlyCollection<FlavorDefinition> all) : IDefinitionRegistry<FlavorDefinition>
    {
        public IReadOnlyCollection<FlavorDefinition> All { get; } = all;

        public bool TryGet(string id, [NotNullWhen(true)] out FlavorDefinition? definition)
        {
            definition = All.FirstOrDefault(flavor => flavor.Id == id);
            return definition is not null;
        }

        public FlavorDefinition GetRequired(string id) => All.First(flavor => flavor.Id == id);
    }

    private sealed class ScriptedRandomSource(params int[] values) : IRandomSource
    {
        private readonly Queue<int> _values = new(values);

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Requests.Add((minInclusive, maxExclusive));
            if (!_values.TryDequeue(out var value))
            {
                throw new InvalidOperationException("The scripted random source ran out of values.");
            }

            if (value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException($"Scripted value {value} is outside [{minInclusive}, {maxExclusive}).");
            }

            return value;
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }
}
