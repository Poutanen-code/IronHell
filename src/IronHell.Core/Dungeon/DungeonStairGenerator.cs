using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public static class DungeonStairGenerator
{
    public const int MaximumDepth = 128;

    public static string PrepareRandomStairFeature(int depth, bool isQuestLevel, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(randomSource);
        if (depth < 0 || depth >= MaximumDepth)
        {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }

        if (depth == 0)
        {
            return DungeonGrid.DownStairFeatureId;
        }

        if (isQuestLevel || depth >= MaximumDepth - 1)
        {
            return DungeonGrid.UpStairFeatureId;
        }

        return randomSource.Next(0, 100) < 50
            ? DungeonGrid.DownStairFeatureId
            : DungeonGrid.UpStairFeatureId;
    }

    public static bool TryPlaceStair(DungeonGrid grid, DungeonPosition position, string featureId)
    {
        ArgumentNullException.ThrowIfNull(grid);
        if (featureId is not DungeonGrid.UpStairFeatureId and not DungeonGrid.DownStairFeatureId)
        {
            throw new ArgumentException("The feature must be an up or down stair.", nameof(featureId));
        }

        if (!DungeonGrid.IsInBounds(position) ||
            grid.GetFeatureId(position) != RoomGeometryBuilder.OpenFloorFeatureId)
        {
            return false;
        }

        grid.SetFeatureId(position, featureId);
        return true;
    }
}