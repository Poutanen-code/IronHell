using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public sealed record RoomStairExecutionResult(
    IReadOnlyList<RoomContentAttempt> ExecutedStairAttempts,
    IReadOnlyList<RoomContentAttempt> RemainingAttempts);

public static class RoomStairAttemptExecutor
{
    public static RoomStairExecutionResult Execute(
        DungeonGrid grid,
        IReadOnlyList<RoomContentAttempt> attempts,
        int depth,
        bool isQuestLevel,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(randomSource);

        var executed = new List<RoomContentAttempt>();
        var remaining = new List<RoomContentAttempt>();
        foreach (var attempt in attempts)
        {
            if (attempt.Kind != RoomContentAttemptKind.RandomStair)
            {
                remaining.Add(attempt);
                continue;
            }

            if (attempt.PreparedStairFeatureId is { } preparedFeatureId)
            {
                DungeonStairGenerator.TryPlaceStair(grid, attempt.Origin, preparedFeatureId);
            }
            else if (grid.GetFeatureId(attempt.Origin) == RoomGeometryBuilder.OpenFloorFeatureId)
            {
                var featureId = DungeonStairGenerator.PrepareRandomStairFeature(depth, isQuestLevel, randomSource);
                DungeonStairGenerator.TryPlaceStair(grid, attempt.Origin, featureId);
            }

            executed.Add(attempt);
        }

        return new RoomStairExecutionResult(executed.AsReadOnly(), remaining.AsReadOnly());
    }
}