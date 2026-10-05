using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public sealed record DungeonStairAllocationResult(
    IReadOnlyList<DungeonPosition> DownStairPositions,
    IReadOnlyList<DungeonPosition> UpStairPositions);

public static class DungeonStairAllocator
{
    public const int RequestedDownStairMinimum = 3;
    public const int RequestedDownStairMaximum = 4;
    public const int RequestedUpStairMinimum = 1;
    public const int RequestedUpStairMaximum = 2;
    public const int InitialAdjacentWallRequirement = 3;
    public const int CandidateAttemptsBeforeRelaxation = 3001;

    private static readonly HashSet<string> WallFeatureIds =
    [
        "granite_wall_basic",
        "granite_wall_inner",
        "granite_wall_outer",
        "granite_wall_solid",
        "magma_vein",
        "quartz_vein",
        "permanent_wall_basic",
        "permanent_wall_inner",
        "permanent_wall_outer",
        "permanent_wall_solid",
    ];

    public static DungeonStairAllocationResult Allocate(
        DungeonGrid grid,
        int depth,
        bool isQuestLevel,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);
        if (depth <= 0 || depth >= DungeonStairGenerator.MaximumDepth)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "Global dungeon stairs require a positive dungeon depth.");
        }

        var downPositions = new List<DungeonPosition>();
        var upPositions = new List<DungeonPosition>();
        var forceUp = isQuestLevel || depth >= DungeonStairGenerator.MaximumDepth - 1;

        var downCount = randomSource.Next(RequestedDownStairMinimum, RequestedDownStairMaximum + 1);
        for (var index = 0; index < downCount; index++)
        {
            PlaceOne(grid, forceUp ? DungeonGrid.UpStairFeatureId : DungeonGrid.DownStairFeatureId,
                forceUp ? upPositions : downPositions, randomSource);
        }

        var upCount = randomSource.Next(RequestedUpStairMinimum, RequestedUpStairMaximum + 1);
        for (var index = 0; index < upCount; index++)
        {
            PlaceOne(grid, DungeonGrid.UpStairFeatureId, upPositions, randomSource);
        }

        return new DungeonStairAllocationResult(downPositions.AsReadOnly(), upPositions.AsReadOnly());
    }

    private static void PlaceOne(
        DungeonGrid grid,
        string requestedFeatureId,
        List<DungeonPosition> placedPositions,
        IRandomSource randomSource)
    {
        var wallRequirement = InitialAdjacentWallRequirement;
        while (true)
        {
            for (var attempt = 0; attempt < CandidateAttemptsBeforeRelaxation; attempt++)
            {
                var position = new DungeonPosition(
                    randomSource.Next(0, DungeonGrid.Height),
                    randomSource.Next(0, DungeonGrid.Width));
                if (!DungeonGrid.IsInInterior(position) ||
                    grid.GetFeatureId(position) != RoomGeometryBuilder.OpenFloorFeatureId ||
                    CountAdjacentWalls(grid, position) < wallRequirement)
                {
                    continue;
                }

                grid.SetFeatureId(position, requestedFeatureId);
                placedPositions.Add(position);
                return;
            }

            wallRequirement--;
        }
    }

    private static int CountAdjacentWalls(DungeonGrid grid, DungeonPosition position)
    {
        var walls = 0;
        foreach (var offset in CardinalOffsets)
        {
            var adjacent = new DungeonPosition(position.Row + offset.Row, position.Column + offset.Column);
            if (WallFeatureIds.Contains(grid.GetFeatureId(adjacent) ?? string.Empty))
            {
                walls++;
            }
        }

        return walls;
    }

    private static readonly DungeonPosition[] CardinalOffsets =
    [
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1),
    ];
}