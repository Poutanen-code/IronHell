using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Items;

/// <summary>
/// Assigns exactly one flavor per item kind for the lifetime of a world, mirroring MAngband's
/// per-save flavor_init() behavior. Assignments are runtime-only: they never mutate definitions
/// and are fully reproducible from the same (items, flavors, seed) inputs.
/// </summary>
public interface IFlavorAssignmentService
{
    IReadOnlyCollection<FlavorAssignment> Assignments { get; }

    FlavorDefinition GetFlavorForItemKind(string itemDefinitionId);

    bool TryGetFlavorForItemKind(string itemDefinitionId, [NotNullWhen(true)] out FlavorDefinition? flavor);
}

public sealed class FlavorAssignmentService : IFlavorAssignmentService
{
    private readonly Dictionary<string, FlavorDefinition> _flavorByItemId = new(StringComparer.Ordinal);
    private readonly List<FlavorAssignment> _assignments = [];

    public FlavorAssignmentService(
        IDefinitionRegistry<ItemDefinition> items,
        IDefinitionRegistry<FlavorDefinition> flavors,
        FlavorSeed seed)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(flavors);

        var random = new SeededRandomSource(seed.Value);
        var missingCategories = new List<FlavorCategory>();

        foreach (var group in GroupItemsByFlavorCategory(items))
        {
            var pool = flavors.All
                .Where(flavor => flavor.Category == group.Category)
                .OrderBy(flavor => flavor.Id, StringComparer.Ordinal)
                .ToArray();

            if (pool.Length == 0)
            {
                missingCategories.Add(group.Category);
                continue;
            }

            var shuffledPool = Shuffle(pool, random);
            for (var index = 0; index < group.Items.Length; index++)
            {
                var flavor = shuffledPool[index % shuffledPool.Length];
                _assignments.Add(new FlavorAssignment(group.Items[index].Id, flavor.Id));
                _flavorByItemId[group.Items[index].Id] = flavor;
            }
        }

        if (missingCategories.Count > 0)
        {
            throw new InvalidOperationException(
                $"No flavors are defined for category(ies) with item definitions requiring them: {string.Join(", ", missingCategories)}.");
        }
    }

    public IReadOnlyCollection<FlavorAssignment> Assignments => _assignments.AsReadOnly();

    public FlavorDefinition GetFlavorForItemKind(string itemDefinitionId) =>
        TryGetFlavorForItemKind(itemDefinitionId, out var flavor)
            ? flavor
            : throw new KeyNotFoundException($"No flavor assignment exists for item definition '{itemDefinitionId}'.");

    public bool TryGetFlavorForItemKind(string itemDefinitionId, [NotNullWhen(true)] out FlavorDefinition? flavor) =>
        _flavorByItemId.TryGetValue(itemDefinitionId, out flavor);

    private static IEnumerable<(FlavorCategory Category, ItemDefinition[] Items)> GroupItemsByFlavorCategory(
        IDefinitionRegistry<ItemDefinition> items) =>
        items.All
            .Select(item => (Item: item, Matched: FlavorCategoryMapping.TryGetFlavorCategory(item, out var category), Category: category))
            .Where(entry => entry.Matched)
            .GroupBy(entry => entry.Category)
            .OrderBy(group => group.Key)
            .Select(group => (
                Category: group.Key,
                Items: group.Select(entry => entry.Item).OrderBy(item => item.Id, StringComparer.Ordinal).ToArray()));

    private static FlavorDefinition[] Shuffle(FlavorDefinition[] source, IRandomSource random)
    {
        var shuffled = (FlavorDefinition[])source.Clone();
        for (var index = shuffled.Length - 1; index > 0; index--)
        {
            var swapIndex = random.Next(0, index + 1);
            (shuffled[index], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[index]);
        }

        return shuffled;
    }
}
