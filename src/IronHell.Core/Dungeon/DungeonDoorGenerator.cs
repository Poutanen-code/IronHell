using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public static class DungeonDoorGenerator
{
    public static void PlaceRandomDoor(DungeonGrid grid, DungeonPosition position, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        var roll = randomSource.Next(0, 1000);
        switch (roll)
        {
            case < 300:
                grid.SetFeatureId(position, DungeonGrid.OpenDoorFeatureId);
                break;
            case < 400:
                grid.SetFeatureId(position, DungeonGrid.BrokenDoorFeatureId);
                break;
            case < 600:
                grid.SetFeatureId(position, DungeonGrid.SecretDoorFeatureId);
                break;
            case < 900:
                grid.SetClosedDoor(position, new DoorState(DoorCondition.Closed, 0));
                break;
            case < 999:
                grid.SetClosedDoor(position, new DoorState(DoorCondition.Locked, randomSource.Next(1, 8)));
                break;
            default:
                grid.SetClosedDoor(position, new DoorState(DoorCondition.Stuck, randomSource.Next(0, 8)));
                break;
        }
    }

    public static void PlaceRandomClosedDoor(DungeonGrid grid, DungeonPosition position, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        var roll = randomSource.Next(0, 400);
        var state = roll switch
        {
            < 300 => new DoorState(DoorCondition.Closed, 0),
            < 399 => new DoorState(DoorCondition.Locked, randomSource.Next(1, 8)),
            _ => new DoorState(DoorCondition.Stuck, randomSource.Next(0, 8)),
        };

        grid.SetClosedDoor(position, state);
    }

    public static void PlaceLockedDoor(DungeonGrid grid, DungeonPosition position, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        grid.SetClosedDoor(position, CreateLockedDoorState(randomSource));
    }

    public static DoorState CreateLockedDoorState(IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        return new DoorState(DoorCondition.Locked, randomSource.Next(1, 8));
    }
}