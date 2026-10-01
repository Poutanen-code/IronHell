namespace IronHell.Core.Dungeon;

public readonly record struct DungeonPosition(int Row, int Column);

public readonly record struct RoomBlockPosition(int Row, int Column);

public readonly record struct RoomBlockFootprint
{
    public RoomBlockFootprint(int heightBlocks, int widthBlocks)
    {
        if (heightBlocks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heightBlocks));
        }

        if (widthBlocks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthBlocks));
        }

        HeightBlocks = heightBlocks;
        WidthBlocks = widthBlocks;
    }

    public int HeightBlocks { get; }

    public int WidthBlocks { get; }
}

[Flags]
public enum DungeonCellStates
{
    None = 0,
    Room = 1,
    Icky = 2,
    Glow = 4,
    TunnelSolid = 8,
}

public sealed class DungeonGrid
{
    public const string GraniteWallBasicFeatureId = "granite_wall_basic";
    public const int DungeonWidth = 198;
    public const int DungeonHeight = 66;
    public const int BlockWidth = 11;
    public const int BlockHeight = 11;
    public const int BlockColumns = 18;
    public const int BlockRows = 6;

    private readonly DungeonCellStates[,] _cellFlags = new DungeonCellStates[DungeonHeight, DungeonWidth];
    private readonly string?[,] _featureIds = new string?[DungeonHeight, DungeonWidth];
    private readonly bool[,] _reservedBlocks = new bool[BlockRows, BlockColumns];
    private readonly List<DungeonPosition> _roomCenters = [];
    private bool _rockInitialized;

    public static int Width => DungeonWidth;

    public static int Height => DungeonHeight;

    public static int RoomBlockRows => BlockRows;

    public static int RoomBlockColumns => BlockColumns;

    public IReadOnlyList<DungeonPosition> RoomCenters => _roomCenters.AsReadOnly();

    public bool IsRockInitialized => _rockInitialized;

    public void InitializeRock()
    {
        for (var row = 0; row < Height; row++)
        {
            for (var column = 0; column < Width; column++)
            {
                if (_featureIds[row, column] is null)
                {
                    _featureIds[row, column] = GraniteWallBasicFeatureId;
                }
            }
        }

        _rockInitialized = true;
    }

    public static bool IsInBounds(DungeonPosition position) =>
        position.Row >= 0 && position.Row < Height &&
        position.Column >= 0 && position.Column < Width;

    public DungeonCellStates GetCellFlags(DungeonPosition position)
    {
        EnsureInBounds(position);
        return _cellFlags[position.Row, position.Column];
    }

    public void AddCellFlags(DungeonPosition position, DungeonCellStates flags)
    {
        EnsureInBounds(position);
        _cellFlags[position.Row, position.Column] |= flags;
    }

    public string? GetFeatureId(DungeonPosition position)
    {
        EnsureInBounds(position);
        return _featureIds[position.Row, position.Column];
    }

    public void SetFeatureId(DungeonPosition position, string featureId)
    {
        EnsureInBounds(position);
        ArgumentException.ThrowIfNullOrWhiteSpace(featureId);
        _featureIds[position.Row, position.Column] = featureId;
    }

    public bool IsBlockReserved(RoomBlockPosition position)
    {
        EnsureBlockInBounds(position);
        return _reservedBlocks[position.Row, position.Column];
    }

    public bool IsRoomFootprintAvailable(RoomBlockPosition start, RoomBlockFootprint footprint)
    {
        if (!IsFootprintInBounds(start, footprint))
        {
            return false;
        }

        for (var row = start.Row; row < start.Row + footprint.HeightBlocks; row++)
        {
            for (var column = start.Column; column < start.Column + footprint.WidthBlocks; column++)
            {
                if (_reservedBlocks[row, column])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool TryCommitRoom(
        RoomBlockPosition start,
        RoomBlockFootprint footprint,
        DungeonPosition center)
    {
        if (!IsInBounds(center) || !IsRoomFootprintAvailable(start, footprint))
        {
            return false;
        }

        for (var row = start.Row; row < start.Row + footprint.HeightBlocks; row++)
        {
            for (var column = start.Column; column < start.Column + footprint.WidthBlocks; column++)
            {
                _reservedBlocks[row, column] = true;
            }
        }

        _roomCenters.Add(center);
        return true;
    }

    private static bool IsFootprintInBounds(RoomBlockPosition start, RoomBlockFootprint footprint) =>
        start.Row >= 0 && start.Column >= 0 &&
        start.Row + footprint.HeightBlocks <= BlockRows &&
        start.Column + footprint.WidthBlocks <= BlockColumns;

    private static void EnsureInBounds(DungeonPosition position)
    {
        if (!IsInBounds(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }
    }

    private static void EnsureBlockInBounds(RoomBlockPosition position)
    {
        if (position.Row < 0 || position.Row >= BlockRows ||
            position.Column < 0 || position.Column >= BlockColumns)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }
    }
}
