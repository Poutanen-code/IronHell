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
            new FlavorDefinition("ring_ruby", FlavorCategory.Ring, "Ruby", "=", "r", new FlavorLegacyMetadata(28, 45, null), "verified"),
            new FlavorDefinition("amulet_amber", FlavorCategory.Amulet, "Amber", "\"", "y", new FlavorLegacyMetadata(44, 40, null), "verified"),
        ]);
        var service = new FlavorService(registry);

        var flavor = service.GetRandomFlavorForCategory(FlavorCategory.Ring, new SeededRandomSource(1));

        Assert.Equal("ring_ruby", flavor.Id);
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
}
