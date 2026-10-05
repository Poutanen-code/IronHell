using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public sealed record RoomMonsterAttemptPlacement(
    RoomContentAttempt Attempt,
    MonsterPlacementResult Placement);

public sealed record RoomMonsterAttemptExecutionResult(
    IReadOnlyList<RoomMonsterAttemptPlacement> ProcessedAttempts,
    IReadOnlyList<RoomContentAttempt> RemainingAttempts)
{
    public int RequestedCount => ProcessedAttempts.Count;

    public int SuccessfulPlacementCount => ProcessedAttempts.Count(result => result.Placement.Success);
}

public static class RoomMonsterAttemptExecutor
{
    public static RoomMonsterAttemptExecutionResult Execute(
        DungeonGrid grid,
        IReadOnlyList<RoomContentAttempt> attempts,
        int depth,
        IReadOnlyDictionary<string, MonsterDefinition> definitions,
        MonsterRuntimeState runtimeState,
        IRandomSource randomSource,
        MonsterPlacementSpace placementSpace)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(runtimeState);
        ArgumentNullException.ThrowIfNull(randomSource);
        ArgumentNullException.ThrowIfNull(placementSpace);
        ArgumentOutOfRangeException.ThrowIfNegative(depth);

        var processed = new List<RoomMonsterAttemptPlacement>();
        var remaining = new List<RoomContentAttempt>();
        foreach (var attempt in attempts)
        {
            if (!IsPreparedPositionMonster(attempt))
            {
                remaining.Add(attempt);
                continue;
            }

            if (!definitions.TryGetValue(attempt.DefinitionId!, out var definition))
            {
                throw new InvalidOperationException(
                    $"Prepared monster definition '{attempt.DefinitionId}' was not found.");
            }

            var position = ToMonsterPosition(attempt.Origin);
            var placement = PlaceAtRequestedPosition(
                grid,
                runtimeState,
                placementSpace,
                definition,
                position,
                randomSource,
                depth);
            processed.Add(new RoomMonsterAttemptPlacement(attempt, placement));
        }

        return new RoomMonsterAttemptExecutionResult(processed.AsReadOnly(), remaining.AsReadOnly());
    }

    public static MonsterPosition ToMonsterPosition(DungeonPosition position) =>
        new(position.Column, position.Row);

    private static bool IsPreparedPositionMonster(RoomContentAttempt attempt) =>
        attempt.Kind == RoomContentAttemptKind.Monster &&
        attempt.DefinitionId is not null &&
        !attempt.AllowGroupExpansion;

    private static MonsterPlacementResult PlaceAtRequestedPosition(
        DungeonGrid grid,
        MonsterRuntimeState runtimeState,
        MonsterPlacementSpace placementSpace,
        MonsterDefinition definition,
        MonsterPosition position,
        IRandomSource randomSource,
        int depth)
    {
        var dungeonPosition = new DungeonPosition(position.Y, position.X);
        if (!DungeonGrid.IsInBounds(dungeonPosition))
        {
            return new MonsterPlacementResult(false, null, MonsterPlacementFailureReason.OutOfBounds);
        }

        if (grid.GetFeatureId(dungeonPosition) != RoomGeometryBuilder.OpenFloorFeatureId)
        {
            return new MonsterPlacementResult(false, null, MonsterPlacementFailureReason.IllegalCell);
        }

        return MonsterPlacementService.Place(runtimeState, definition, position, randomSource, placementSpace, depth);
    }
}