using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public sealed record RoomDoorExecutionResult(
    IReadOnlyList<RoomContentAttempt> ExecutedDoorAttempts,
    IReadOnlyList<RoomContentAttempt> RemainingAttempts);

public static class RoomDoorAttemptExecutor
{
    public static RoomDoorExecutionResult Execute(
        DungeonGrid grid,
        IReadOnlyList<RoomContentAttempt> attempts,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(randomSource);

        var executed = new List<RoomContentAttempt>();
        var remaining = new List<RoomContentAttempt>();
        foreach (var attempt in attempts)
        {
            switch (attempt.Kind)
            {
                case RoomContentAttemptKind.SecretDoor:
                    grid.SetFeatureId(attempt.Origin, DungeonGrid.SecretDoorFeatureId);
                    executed.Add(attempt);
                    break;
                case RoomContentAttemptKind.LockedDoor:
                    if (attempt.PreparedDoorState is { } preparedState)
                    {
                        grid.SetClosedDoor(attempt.Origin, preparedState);
                    }
                    else
                    {
                        DungeonDoorGenerator.PlaceLockedDoor(grid, attempt.Origin, randomSource);
                    }

                    executed.Add(attempt);
                    break;
                default:
                    remaining.Add(attempt);
                    break;
            }
        }

        return new RoomDoorExecutionResult(executed.AsReadOnly(), remaining.AsReadOnly());
    }
}