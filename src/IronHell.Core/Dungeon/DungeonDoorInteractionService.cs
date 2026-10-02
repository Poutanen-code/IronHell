using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public enum DoorInteractionResult
{
    Opened,
    LockpickFailed,
    Stuck,
    Spiked,
    BashFailed,
    BashedOpen,
    BashedBroken,
    Closed,
    BrokenCannotClose,
    NotClosedDoor,
    NotOpenDoor,
}

public static class DungeonDoorInteractionService
{
    public const string OpenDoorFeatureId = DungeonGrid.OpenDoorFeatureId;
    public const string BrokenDoorFeatureId = DungeonGrid.BrokenDoorFeatureId;

    public static DoorInteractionResult Open(
        DungeonGrid grid,
        DungeonPosition position,
        int skillDis,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        if (!TryGetDoorState(grid, position, out var state))
        {
            return DoorInteractionResult.NotClosedDoor;
        }

        switch (state.Condition)
        {
            case DoorCondition.Closed:
                grid.SetFeatureId(position, OpenDoorFeatureId);
                return DoorInteractionResult.Opened;
            case DoorCondition.Locked:
                var lockpickChance = Math.Max(2, skillDis - (4 * state.Power));
                if (randomSource.Next(0, 100) >= lockpickChance)
                {
                    return DoorInteractionResult.LockpickFailed;
                }

                grid.SetFeatureId(position, OpenDoorFeatureId);
                return DoorInteractionResult.Opened;
            case DoorCondition.Stuck:
                return DoorInteractionResult.Stuck;
            default:
                throw new InvalidOperationException("Door state has an unsupported condition.");
        }
    }

    public static DoorInteractionResult Spike(DungeonGrid grid, DungeonPosition position)
    {
        ArgumentNullException.ThrowIfNull(grid);

        if (!TryGetDoorState(grid, position, out var state))
        {
            return DoorInteractionResult.NotClosedDoor;
        }

        var power = Math.Min(state.Power + 1, 7);
        grid.SetClosedDoor(position, new DoorState(DoorCondition.Stuck, power));
        return DoorInteractionResult.Spiked;
    }

    public static DoorInteractionResult Bash(
        DungeonGrid grid,
        DungeonPosition position,
        int bashPower,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        if (!TryGetDoorState(grid, position, out var state))
        {
            return DoorInteractionResult.NotClosedDoor;
        }

        var bashChance = Math.Max(1, bashPower - (10 * state.Power));
        if (randomSource.Next(0, 100) >= bashChance)
        {
            return DoorInteractionResult.BashFailed;
        }

        if (randomSource.Next(0, 100) < 50)
        {
            grid.SetFeatureId(position, BrokenDoorFeatureId);
            return DoorInteractionResult.BashedBroken;
        }

        grid.SetFeatureId(position, OpenDoorFeatureId);
        return DoorInteractionResult.BashedOpen;
    }

    public static DoorInteractionResult Close(DungeonGrid grid, DungeonPosition position)
    {
        ArgumentNullException.ThrowIfNull(grid);

        var featureId = grid.GetFeatureId(position);
        if (featureId == BrokenDoorFeatureId)
        {
            return DoorInteractionResult.BrokenCannotClose;
        }

        if (featureId != OpenDoorFeatureId)
        {
            return DoorInteractionResult.NotOpenDoor;
        }

        grid.SetFeatureId(position, DungeonGrid.ClosedDoorFeatureId);
        return DoorInteractionResult.Closed;
    }

    private static bool TryGetDoorState(DungeonGrid grid, DungeonPosition position, out DoorState state)
    {
        if (grid.GetFeatureId(position) == DungeonGrid.ClosedDoorFeatureId &&
            grid.GetDoorState(position) is { } doorState)
        {
            state = doorState;
            return true;
        }

        state = default;
        return false;
    }
}