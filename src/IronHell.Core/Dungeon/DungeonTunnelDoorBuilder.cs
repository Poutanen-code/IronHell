using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public sealed record TunnelDoorPlacementResult(
    IReadOnlyList<DungeonPosition> EntranceDoorPositions,
    IReadOnlyList<DungeonPosition> JunctionDoorPositions);

public static class DungeonTunnelDoorBuilder
{
    public const int EntranceDoorChance = 25;
    public const int JunctionDoorChance = 90;

    private static readonly string[] WallFeatureIds =
    [
        "magma_vein",
        "quartz_vein",
        "granite_wall_basic",
        "granite_wall_inner",
        "granite_wall_outer",
        "granite_wall_solid",
        "permanent_wall_basic",
        "permanent_wall_inner",
        "permanent_wall_outer",
        "permanent_wall_solid",
    ];

    public static TunnelDoorPlacementResult PlaceTunnelDoors(
        DungeonGrid grid,
        IReadOnlyList<DungeonPosition> piercedWallPositions,
        IReadOnlyList<DungeonPosition> doorCandidatePositions,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(piercedWallPositions);
        ArgumentNullException.ThrowIfNull(doorCandidatePositions);
        ArgumentNullException.ThrowIfNull(randomSource);

        var entrances = PlaceEntranceDoors(grid, piercedWallPositions, randomSource);
        var junctions = PlaceJunctionDoors(grid, doorCandidatePositions, randomSource);
        return new TunnelDoorPlacementResult(entrances, junctions);
    }

    public static IReadOnlyList<DungeonPosition> PlaceEntranceDoors(
        DungeonGrid grid,
        IReadOnlyList<DungeonPosition> piercedWallPositions,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(piercedWallPositions);
        ArgumentNullException.ThrowIfNull(randomSource);

        var placed = new List<DungeonPosition>();
        foreach (var position in piercedWallPositions)
        {
            grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
            if (randomSource.Next(0, 100) < EntranceDoorChance)
            {
                DungeonDoorGenerator.PlaceRandomDoor(grid, position, randomSource);
                placed.Add(position);
            }
        }

        return placed.AsReadOnly();
    }

    public static IReadOnlyList<DungeonPosition> PlaceJunctionDoors(
        DungeonGrid grid,
        IReadOnlyList<DungeonPosition> doorCandidatePositions,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(doorCandidatePositions);
        ArgumentNullException.ThrowIfNull(randomSource);

        var placed = new List<DungeonPosition>();
        foreach (var candidate in doorCandidatePositions)
        {
            TryPlaceJunctionDoor(grid, new DungeonPosition(candidate.Row, candidate.Column - 1), randomSource, placed);
            TryPlaceJunctionDoor(grid, new DungeonPosition(candidate.Row, candidate.Column + 1), randomSource, placed);
            TryPlaceJunctionDoor(grid, new DungeonPosition(candidate.Row - 1, candidate.Column), randomSource, placed);
            TryPlaceJunctionDoor(grid, new DungeonPosition(candidate.Row + 1, candidate.Column), randomSource, placed);
        }

        return placed.AsReadOnly();
    }

    private static void TryPlaceJunctionDoor(
        DungeonGrid grid,
        DungeonPosition position,
        IRandomSource randomSource,
        List<DungeonPosition> placed)
    {
        if (!DungeonGrid.IsInInterior(position) || IsWall(grid.GetFeatureId(position)) ||
            grid.GetCellFlags(position).HasFlag(DungeonCellStates.Room))
        {
            return;
        }

        if (randomSource.Next(0, 100) >= JunctionDoorChance || !IsPossibleDoorway(grid, position))
        {
            return;
        }

        DungeonDoorGenerator.PlaceRandomDoor(grid, position, randomSource);
        placed.Add(position);
    }

    private static bool IsPossibleDoorway(DungeonGrid grid, DungeonPosition position)
    {
        var adjacentCorridors = 0;
        foreach (var offset in CardinalOffsets)
        {
            var adjacent = new DungeonPosition(position.Row + offset.Row, position.Column + offset.Column);
            if (DungeonGrid.IsInInterior(adjacent) &&
                grid.GetFeatureId(adjacent) == RoomGeometryBuilder.OpenFloorFeatureId &&
                !grid.GetCellFlags(adjacent).HasFlag(DungeonCellStates.Room))
            {
                adjacentCorridors++;
            }
        }

        if (adjacentCorridors < 2)
        {
            return false;
        }

        var north = new DungeonPosition(position.Row - 1, position.Column);
        var south = new DungeonPosition(position.Row + 1, position.Column);
        if (DungeonGrid.IsInInterior(north) && DungeonGrid.IsInInterior(south) &&
            IsWall(grid.GetFeatureId(north)) && IsWall(grid.GetFeatureId(south)))
        {
            return true;
        }

        var west = new DungeonPosition(position.Row, position.Column - 1);
        var east = new DungeonPosition(position.Row, position.Column + 1);
        return DungeonGrid.IsInInterior(west) && DungeonGrid.IsInInterior(east) &&
            IsWall(grid.GetFeatureId(west)) && IsWall(grid.GetFeatureId(east));
    }

    private static bool IsWall(string? featureId) =>
        featureId is not null && Array.IndexOf(WallFeatureIds, featureId) >= 0;

    private static readonly DungeonPosition[] CardinalOffsets =
    [
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1),
    ];
}