using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public sealed record MonsterGroupExpansionResult(
    int DesiredGroupSize,
    int SuccessfulGroupSize);

public static class MonsterGroupExpander
{
    private static readonly (int X, int Y)[] AdjacentOffsets =
    [
        (0, 1),
        (0, -1),
        (1, 0),
        (-1, 0),
        (1, 1),
        (-1, 1),
        (1, -1),
        (-1, -1),
    ];

    public static MonsterGroupExpansionResult Expand(
        MonsterRuntimeState state,
        MonsterRuntimeInstance leader,
        MonsterDefinition definition,
        MonsterPlacementSpace space,
        int depth,
        bool allowGroupExpansion,
        IronHell.Core.Randomness.IRandomSource randomSource,
        bool forceGroupExpansion = false)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(leader);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(space);
        ArgumentNullException.ThrowIfNull(randomSource);

        if (!state.Contains(leader))
        {
            throw new InvalidOperationException($"Leader '{leader.InstanceId}' is not registered in runtime state.");
        }

        if (!allowGroupExpansion || (!definition.SpawnPolicy.Friends && !forceGroupExpansion))
        {
            return new MonsterGroupExpansionResult(1, 1);
        }

        var desiredGroupSize = MonsterGroupSizeCalculator.Calculate(definition.NativeLevel, depth, randomSource);
        if (desiredGroupSize == 1)
        {
            return new MonsterGroupExpansionResult(1, 1);
        }

        var origins = new Queue<MonsterPosition>();
        origins.Enqueue(leader.Position);
        var successfulGroupSize = 1;

        while (origins.Count > 0 && successfulGroupSize < desiredGroupSize)
        {
            var origin = origins.Dequeue();
            foreach (var offset in AdjacentOffsets)
            {
                if (successfulGroupSize >= desiredGroupSize)
                {
                    break;
                }

                var target = new MonsterPosition(origin.X + offset.X, origin.Y + offset.Y);
                var placement = MonsterPlacementService.Place(state, definition, target, space);
                if (placement.Success)
                {
                    successfulGroupSize++;
                    origins.Enqueue(placement.Monster!.Position);
                }
            }
        }

        return new MonsterGroupExpansionResult(desiredGroupSize, successfulGroupSize);
    }
}
