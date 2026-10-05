using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public sealed record OrdinaryMonsterPopulationResult(
    int RequestedCount,
    int SuccessfulPlacements,
    int TotalRuntimeMonstersAdded)
{
    public int FailedPlacements => RequestedCount - SuccessfulPlacements;
}

public static class OrdinaryMonsterPopulation
{
    public static OrdinaryMonsterPopulationResult Populate(
        int depth,
        IReadOnlyList<MonsterAllocationPreparedEntry> preparedEntries,
        IEnumerable<MonsterDefinition> definitions,
        MonsterPlacementSpace space,
        MonsterRuntimeState state,
        IRandomSource randomSource,
        OrdinaryMonsterPopulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(preparedEntries);
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(space);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(randomSource);

        ArgumentNullException.ThrowIfNull(options);

        if (options.LocationAttemptLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Location attempt limit must be non-negative.");
        }

        var definitionsById = definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
        var requestedCount = OrdinaryMonsterRequestCount.Calculate(depth, randomSource);
        var successfulPlacements = 0;
        var totalRuntimeMonstersAdded = 0;

        for (var request = 0; request < requestedCount; request++)
        {
            var position = MonsterLocationSearch.FindPosition(
                space,
                state,
                randomSource,
                options.LocationAttemptLimit);
            if (position is null)
            {
                continue;
            }

            var effectiveEntries = MonsterAllocationEligibility.PrepareForSelection(
                preparedEntries,
                depth,
                randomSource);
            var selected = MonsterAllocationSelector.SelectWithComparison(effectiveEntries, randomSource);
            if (selected is null)
            {
                continue;
            }

            var definitionId = selected.PreparedEntry.BaseEntry.MonsterDefinitionId;
            if (!definitionsById.TryGetValue(definitionId, out var definition))
            {
                throw new KeyNotFoundException($"Monster definition '{definitionId}' was not provided.");
            }

            var placement = MonsterPlacementService.Place(state, definition, position.Value, randomSource, space, depth);
            if (placement.Success)
            {
                successfulPlacements++;
                var group = MonsterGroupExpander.Expand(
                    state,
                    placement.Monster!,
                    definition,
                    space,
                    depth,
                    options.AllowGroupExpansion,
                    randomSource);
                totalRuntimeMonstersAdded += group.SuccessfulGroupSize;

                var escorts = MonsterEscortExpander.Expand(
                    state,
                    placement.Monster!,
                    definition,
                    new MonsterEscortExpansionOptions(
                        preparedEntries.Select(entry => entry.BaseEntry).ToArray(),
                        definitionsById.Values.ToArray(),
                        space,
                        depth,
                        options.AllowGroupExpansion,
                        randomSource));
                totalRuntimeMonstersAdded += escorts.TotalRuntimeMonstersAdded;
            }
        }

        return new OrdinaryMonsterPopulationResult(
            requestedCount,
            successfulPlacements,
            totalRuntimeMonstersAdded);
    }
}
