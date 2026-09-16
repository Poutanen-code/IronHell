using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Items;

public interface IFlavorService
{
    FlavorDefinition GetRandomFlavorForCategory(FlavorCategory category, IRandomSource random);
}

public sealed class FlavorService(IDefinitionRegistry<FlavorDefinition> flavors) : IFlavorService
{
    public FlavorDefinition GetRandomFlavorForCategory(FlavorCategory category, IRandomSource random)
    {
        var candidates = flavors.All.Where(flavor => flavor.Category == category).ToArray();
        if (candidates.Length == 0)
        {
            throw new KeyNotFoundException($"No flavors are defined for category '{category}'.");
        }

        return candidates[random.Next(0, candidates.Length)];
    }
}
