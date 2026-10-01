using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public static class VaultRoomBuilder
{
    public const string PermanentInnerWallFeatureId = "permanent_wall_inner";

    public static VaultDefinition? Select(
        IReadOnlyList<VaultDefinition> definitions,
        RoomFamily family,
        int depth,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(randomSource);

        var metadata = RoomFamilies.Get(family);
        var eligible = definitions.Where(definition =>
            definition.EffectiveType == TypeFor(family) &&
            depth >= metadata.MinimumDepth).ToArray();
        if (eligible.Length == 0)
        {
            return null;
        }

        while (true)
        {
            var candidate = definitions[randomSource.Next(0, definitions.Count)];
            if (candidate.EffectiveType == TypeFor(family) && depth >= metadata.MinimumDepth)
            {
                return candidate;
            }
        }
    }

    public static RoomBuildResult TryBuild(
        DungeonGrid grid,
        RoomBlockPosition startBlock,
        int depth,
        VaultDefinition definition,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(randomSource);
        definition.Validate();

        var family = definition.Family;
        var metadata = RoomFamilies.Get(family);
        var center = CenterFromBlocks(startBlock, metadata.Footprint);
        var top = center.Row - definition.Rows / 2;
        var left = center.Column - definition.Columns / 2;

        if (!grid.TryCommitRoom(startBlock, metadata.Footprint, center))
        {
            return new RoomBuildResult(false, null, family, ContentAttempts: []);
        }

        var attempts = new List<RoomContentAttempt>();
        WriteTerrain(grid, definition, top, left, randomSource, attempts);
        WriteContent(attempts, definition, top, left, randomSource);
        return new RoomBuildResult(true, center, family, ContentAttempts: attempts);
    }

    private static void WriteTerrain(
        DungeonGrid grid,
        VaultDefinition definition,
        int top,
        int left,
        IRandomSource randomSource,
        List<RoomContentAttempt> attempts)
    {
        for (var row = 0; row < definition.Rows; row++)
        {
            for (var column = 0; column < definition.Columns; column++)
            {
                var glyph = definition.Layout[row][column];
                if (glyph == ' ')
                {
                    continue;
                }

                var position = new DungeonPosition(top + row, left + column);
                grid.AddCellFlags(position, DungeonCellStates.Room | DungeonCellStates.Icky);
                SetFeature(grid, position, GetTerrainFeatureId(glyph));
                AddFirstPassAttempt(attempts, glyph, position, randomSource);
            }
        }
    }

    private static void WriteContent(
        List<RoomContentAttempt> attempts,
        VaultDefinition definition,
        int top,
        int left,
        IRandomSource randomSource)
    {
        for (var row = 0; row < definition.Rows; row++)
        {
            for (var column = 0; column < definition.Columns; column++)
            {
                var position = new DungeonPosition(top + row, left + column);
                switch (definition.Layout[row][column])
                {
                    case '&':
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.Monster, position, GenerationDepthOffset: 5, Special: true));
                        break;
                    case '@':
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.Monster, position, GenerationDepthOffset: 11, Special: true));
                        break;
                    case '9':
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.Monster, position, GenerationDepthOffset: 9, Special: true));
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SpecialObject, position, GenerationDepthOffset: 7, Special: true));
                        break;
                    case '8':
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.Monster, position, GenerationDepthOffset: 40, Special: true));
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SpecialObject, position, GenerationDepthOffset: 20, Special: true));
                        break;
                    case ',':
                        if (randomSource.Next(0, 100) < 50)
                        {
                            attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.Monster, position, GenerationDepthOffset: 3, Special: true));
                        }

                        if (randomSource.Next(0, 100) < 50)
                        {
                            attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.Object, position, GenerationDepthOffset: 7));
                        }
                        break;
                }
            }
        }
    }

    private static int TypeFor(RoomFamily family) => family switch
    {
        RoomFamily.LesserVault => 7,
        RoomFamily.GreaterVault => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(family)),
    };

    private static DungeonPosition CenterFromBlocks(RoomBlockPosition start, RoomBlockFootprint footprint) =>
        new(
            ((start.Row + footprint.HeightBlocks - 1 + 1) * DungeonGrid.BlockHeight) / 2,
            ((start.Column + footprint.WidthBlocks - 1 + 1) * DungeonGrid.BlockWidth) / 2);

    private static void SetFeature(DungeonGrid grid, DungeonPosition position, string featureId) =>
        grid.SetFeatureId(position, featureId);

    private static string GetTerrainFeatureId(char glyph) => glyph switch
    {
        '%' => RoomGeometryBuilder.OuterWallFeatureId,
        '#' => RoomGeometryBuilder.InnerWallFeatureId,
        'X' => PermanentInnerWallFeatureId,
        _ => RoomGeometryBuilder.OpenFloorFeatureId,
    };

    private static void AddFirstPassAttempt(
        List<RoomContentAttempt> attempts,
        char glyph,
        DungeonPosition position,
        IRandomSource randomSource)
    {
        switch (glyph)
        {
            case '*':
                attempts.Add(new RoomContentAttempt(
                    randomSource.Next(0, 100) < 75 ? RoomContentAttemptKind.Object : RoomContentAttemptKind.Trap,
                    position));
                break;
            case '+':
                attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, position));
                break;
            case '^':
                attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.Trap, position));
                break;
        }
    }
}