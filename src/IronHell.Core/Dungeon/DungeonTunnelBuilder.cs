using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public sealed record TunnelBuildResult(
    bool Completed,
    IReadOnlyList<DungeonPosition> TunnelPositions,
    IReadOnlyList<DungeonPosition> PiercedWallPositions,
    IReadOnlyList<DungeonPosition> DoorCandidatePositions);

public static class DungeonTunnelBuilder
{
    public const int DirectionChangePercent = 30;
    public const int RandomDirectionPercent = 10;
    public const int ExtraTunnelingPercent = 15;
    public const int TunnelIterationLimit = 2000;

    public static TunnelBuildResult Build(
        DungeonGrid grid,
        DungeonPosition start,
        DungeonPosition target,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);
        if (!DungeonGrid.IsInBounds(start) || !DungeonGrid.IsInBounds(target))
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (!grid.IsRockInitialized)
        {
            return Empty(false);
        }

        var tunnelPositions = new List<DungeonPosition>();
        var piercedWalls = new List<DungeonPosition>();
        var doorCandidates = new List<DungeonPosition>();
        var row = start.Row;
        var column = start.Column;
        var rowDirection = 0;
        var columnDirection = 0;
        CorrectDirection(ref rowDirection, ref columnDirection, row, column, target.Row, target.Column, randomSource);
        var startRow = row;
        var startColumn = column;
        var doorFlag = false;
        var loopCount = 0;

        while (row != target.Row || column != target.Column)
        {
            if (loopCount++ > TunnelIterationLimit)
            {
                break;
            }

            if (randomSource.Next(0, 100) < DirectionChangePercent)
            {
                CorrectDirection(ref rowDirection, ref columnDirection, row, column, target.Row, target.Column, randomSource);
                TryRandomDirection(ref rowDirection, ref columnDirection, randomSource);
            }

            var next = new DungeonPosition(row + rowDirection, column + columnDirection);
            while (!DungeonGrid.IsInBounds(next))
            {
                CorrectDirection(ref rowDirection, ref columnDirection, row, column, target.Row, target.Column, randomSource);
                TryRandomDirection(ref rowDirection, ref columnDirection, randomSource);
                next = new DungeonPosition(row + rowDirection, column + columnDirection);
            }

            var featureId = grid.GetFeatureId(next);
            if (grid.GetCellFlags(next).HasFlag(DungeonCellStates.TunnelSolid) || IsPermanent(featureId))
            {
                continue;
            }

            if (featureId == RoomGeometryBuilder.OuterWallFeatureId)
            {
                var beyond = new DungeonPosition(next.Row + rowDirection, next.Column + columnDirection);
                if (!DungeonGrid.IsInBounds(beyond) ||
                    grid.GetCellFlags(beyond).HasFlag(DungeonCellStates.TunnelSolid) ||
                    IsPermanent(grid.GetFeatureId(beyond)) ||
                    grid.GetFeatureId(beyond) == RoomGeometryBuilder.OuterWallFeatureId)
                {
                    continue;
                }

                row = next.Row;
                column = next.Column;
                piercedWalls.Add(next);
                MarkAdjacentOuterWallsSolid(grid, next);
                continue;
            }

            if (grid.GetCellFlags(next).HasFlag(DungeonCellStates.Room))
            {
                row = next.Row;
                column = next.Column;
                continue;
            }

            if (featureId == DungeonGrid.GraniteWallBasicFeatureId)
            {
                row = next.Row;
                column = next.Column;
                tunnelPositions.Add(next);
                doorFlag = false;
                continue;
            }

            row = next.Row;
            column = next.Column;
            if (!doorFlag)
            {
                doorCandidates.Add(next);
                doorFlag = true;
            }

            if (randomSource.Next(0, 100) >= ExtraTunnelingPercent &&
                (Math.Abs(row - startRow) > 10 || Math.Abs(column - startColumn) > 10))
            {
                break;
            }
        }

        foreach (var position in tunnelPositions)
        {
            grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        }

        foreach (var position in piercedWalls)
        {
            grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        }

        return new TunnelBuildResult(
            row == target.Row && column == target.Column,
            tunnelPositions.AsReadOnly(),
            piercedWalls.AsReadOnly(),
            doorCandidates.AsReadOnly());
    }

    private static void CorrectDirection(
        ref int rowDirection,
        ref int columnDirection,
        int row,
        int column,
        int targetRow,
        int targetColumn,
        IRandomSource randomSource)
    {
        rowDirection = Math.Sign(targetRow - row);
        columnDirection = Math.Sign(targetColumn - column);
        if (rowDirection != 0 && columnDirection != 0)
        {
            if (randomSource.Next(0, 100) < 50)
            {
                rowDirection = 0;
            }
            else
            {
                columnDirection = 0;
            }
        }
    }

    private static void TryRandomDirection(ref int rowDirection, ref int columnDirection, IRandomSource randomSource)
    {
        if (randomSource.Next(0, 100) < RandomDirectionPercent)
        {
            switch (randomSource.Next(0, 4))
            {
                case 0:
                    rowDirection = 1;
                    columnDirection = 0;
                    break;
                case 1:
                    rowDirection = -1;
                    columnDirection = 0;
                    break;
                case 2:
                    rowDirection = 0;
                    columnDirection = 1;
                    break;
                default:
                    rowDirection = 0;
                    columnDirection = -1;
                    break;
            }
        }
    }

    private static void MarkAdjacentOuterWallsSolid(DungeonGrid grid, DungeonPosition position)
    {
        for (var row = position.Row - 1; row <= position.Row + 1; row++)
        {
            for (var column = position.Column - 1; column <= position.Column + 1; column++)
            {
                var adjacent = new DungeonPosition(row, column);
                if (DungeonGrid.IsInBounds(adjacent) && grid.GetFeatureId(adjacent) == RoomGeometryBuilder.OuterWallFeatureId)
                {
                    grid.AddCellFlags(adjacent, DungeonCellStates.TunnelSolid);
                }
            }
        }
    }

    private static bool IsPermanent(string? featureId) => featureId is
        "permanent_wall_basic" or
        "permanent_wall_inner" or
        "permanent_wall_outer" or
        "permanent_wall_solid";

    private static TunnelBuildResult Empty(bool completed) => new(completed, [], [], []);
}