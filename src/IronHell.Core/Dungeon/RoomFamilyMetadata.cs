namespace IronHell.Core.Dungeon;

public enum RoomFamily
{
    Simple,
    Overlapping,
    Cross,
    Large,
    Nest,
    Pit,
    LesserVault,
    GreaterVault,
}

public sealed record RoomFamilyMetadata(
    RoomFamily Family,
    int MinimumDepth,
    RoomBlockFootprint Footprint);

public static class RoomFamilies
{
    private static readonly IReadOnlyList<RoomFamilyMetadata> AllMetadata =
    [
        new(RoomFamily.Simple, 1, new RoomBlockFootprint(1, 3)),
        new(RoomFamily.Overlapping, 1, new RoomBlockFootprint(1, 3)),
        new(RoomFamily.Cross, 3, new RoomBlockFootprint(1, 3)),
        new(RoomFamily.Large, 3, new RoomBlockFootprint(1, 3)),
        new(RoomFamily.Nest, 5, new RoomBlockFootprint(1, 3)),
        new(RoomFamily.Pit, 5, new RoomBlockFootprint(1, 3)),
        new(RoomFamily.LesserVault, 5, new RoomBlockFootprint(2, 3)),
        new(RoomFamily.GreaterVault, 10, new RoomBlockFootprint(4, 6)),
    ];

    public static IReadOnlyList<RoomFamilyMetadata> All => AllMetadata;

    public static RoomFamilyMetadata Get(RoomFamily family) =>
        AllMetadata.Single(metadata => metadata.Family == family);
}
