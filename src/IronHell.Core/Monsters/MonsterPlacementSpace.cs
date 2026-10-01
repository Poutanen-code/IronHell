namespace IronHell.Core.Monsters;

public sealed class MonsterPlacementSpace
{
    private readonly HashSet<MonsterPosition> _illegalPositions;

    public MonsterPlacementSpace(
        int width,
        int height,
        IEnumerable<MonsterPosition>? illegalPositions = null)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        Width = width;
        Height = height;
        _illegalPositions = illegalPositions?.ToHashSet() ?? [];
    }

    public int Width { get; }

    public int Height { get; }

    public bool IsInBounds(MonsterPosition position) =>
        position.X >= 0 && position.X < Width &&
        position.Y >= 0 && position.Y < Height;

    public bool IsMonsterPlacementLegal(MonsterPosition position) =>
        IsInBounds(position) && !_illegalPositions.Contains(position);

    public bool IsAvailable(MonsterPosition position, MonsterRuntimeState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return IsMonsterPlacementLegal(position) && !state.IsOccupied(position);
    }
}
