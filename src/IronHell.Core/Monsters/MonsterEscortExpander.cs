using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public sealed record MonsterEscortExpansionResult(
    int Attempts,
    int SuccessfulEscorts,
    int TotalRuntimeMonstersAdded);

public static class MonsterEscortExpander
{
    private const int EscortAttemptLimit = 50;
    private const int ScatterDistance = 3;

    public static MonsterEscortExpansionResult Expand(
        MonsterRuntimeState state,
        MonsterRuntimeInstance leader,
        MonsterDefinition leaderDefinition,
        MonsterEscortExpansionOptions options)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(leader);
        ArgumentNullException.ThrowIfNull(leaderDefinition);
        ArgumentNullException.ThrowIfNull(options);

        if (!state.Contains(leader))
        {
            throw new InvalidOperationException($"Leader '{leader.InstanceId}' is not registered in runtime state.");
        }

        if (!options.AllowGroupExpansion || !leaderDefinition.SpawnPolicy.Escort)
        {
            return new MonsterEscortExpansionResult(0, 0, 0);
        }

        var escortPrepared = PrepareEscortEntries(options, leaderDefinition);
        var definitionsById = options.Definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
        var successfulEscorts = 0;
        var totalRuntimeMonstersAdded = 0;
        var attempts = 0;

        for (; attempts < EscortAttemptLimit; attempts++)
        {
            var target = Scatter(options.Space, leader.Position, options.RandomSource);
            if (!options.Space.IsAvailable(target, state))
            {
                continue;
            }

            var effectiveEntries = MonsterAllocationEligibility.PrepareForSelection(
                escortPrepared,
                leaderDefinition.NativeLevel,
                options.RandomSource);
            var selected = MonsterAllocationSelector.SelectWithComparison(effectiveEntries, options.RandomSource);
            if (selected is null)
            {
                break;
            }

            var definitionId = selected.PreparedEntry.BaseEntry.MonsterDefinitionId;
            if (!definitionsById.TryGetValue(definitionId, out var escortDefinition))
            {
                throw new KeyNotFoundException($"Monster definition '{definitionId}' was not provided.");
            }

            var placement = MonsterPlacementService.Place(
                state,
                escortDefinition,
                target,
                options.RandomSource,
                options.Space,
                options.Depth);
            if (!placement.Success)
            {
                continue;
            }

            successfulEscorts++;
            totalRuntimeMonstersAdded++;
            totalRuntimeMonstersAdded += ExpandEscortGroup(
                state,
                placement.Monster!,
                escortDefinition,
                leaderDefinition,
                options);
        }

        return new MonsterEscortExpansionResult(
            attempts,
            successfulEscorts,
            totalRuntimeMonstersAdded);
    }

    private static IReadOnlyList<MonsterAllocationPreparedEntry> PrepareEscortEntries(
        MonsterEscortExpansionOptions options,
        MonsterDefinition leaderDefinition) =>
        MonsterAllocationTableBuilder.Prepare(
            options.AllocationEntries,
            options.Definitions,
            candidate => candidate.Symbol == leaderDefinition.Symbol &&
                candidate.NativeLevel <= leaderDefinition.NativeLevel &&
                !candidate.SpawnPolicy.Unique &&
                candidate.Id != leaderDefinition.Id);

    private static int ExpandEscortGroup(
        MonsterRuntimeState state,
        MonsterRuntimeInstance escort,
        MonsterDefinition escortDefinition,
        MonsterDefinition leaderDefinition,
        MonsterEscortExpansionOptions options)
    {
        if (!escortDefinition.SpawnPolicy.Friends && !leaderDefinition.SpawnPolicy.Escorts)
        {
            return 0;
        }

        var group = MonsterGroupExpander.Expand(
            state,
            escort,
            escortDefinition,
            options.Space,
            options.Depth,
            options.AllowGroupExpansion,
            options.RandomSource,
            forceGroupExpansion: leaderDefinition.SpawnPolicy.Escorts);
        return group.SuccessfulGroupSize - 1;
    }

    private static MonsterPosition Scatter(
        MonsterPlacementSpace space,
        MonsterPosition origin,
        IRandomSource randomSource)
    {
        while (true)
        {
            var position = new MonsterPosition(
                origin.X + randomSource.Next(-ScatterDistance, ScatterDistance + 1),
                origin.Y + randomSource.Next(-ScatterDistance, ScatterDistance + 1));
            if (space.IsInBounds(position))
            {
                return position;
            }
        }
    }
}
