using IronHell.Core.Randomness;
using IronHell.Core.Monsters;

namespace IronHell.Core.Dungeon;

public sealed record RoomBuildResult(
    bool Succeeded,
    DungeonPosition? Center,
    RoomFamily? Family = null,
    int? Variant = null,
    IReadOnlyList<RoomContentAttempt>? ContentAttempts = null)
{
    public IReadOnlyList<RoomContentAttempt> Attempts => ContentAttempts ?? [];
}

public static class RoomGeometryBuilder
{
    public const string OpenFloorFeatureId = "open_floor";
    public const string OuterWallFeatureId = "granite_wall_outer";
    public const string InnerWallFeatureId = "granite_wall_inner";

    public static RoomBuildResult TryBuildNest(
        DungeonGrid grid,
        RoomBlockPosition startBlock,
        MonsterNestPreparationResult preparation,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(randomSource);

        var family = RoomFamily.Nest;
        var center = CenterFromBlocks(startBlock, RoomFamilies.Get(family).Footprint);
        if (!grid.TryCommitRoom(startBlock, RoomFamilies.Get(family).Footprint, center))
        {
            return Failed(family);
        }

        WriteDoubleRectangle(grid, center.Row - 4, center.Row + 4, center.Column - 11, center.Column + 11, light: false);
        var attempts = new List<RoomContentAttempt>
        {
            new(RoomContentAttemptKind.SecretDoor, SelectSpecialRoomDoor(center, randomSource)),
        };

        for (var row = center.Row - 2; row <= center.Row + 2; row++)
        {
            for (var column = center.Column - 9; column <= center.Column + 9; column++)
            {
                var definitionId = preparation.CandidateDefinitionIds[randomSource.Next(0, preparation.CandidateDefinitionIds.Count)];
                attempts.Add(new RoomContentAttempt(
                    RoomContentAttemptKind.Monster,
                    new DungeonPosition(row, column),
                    DefinitionId: definitionId,
                    AllowGroupExpansion: false,
                    MonsterSource: RoomMonsterAttemptSource.PreparedNestPit));
            }
        }

        return Succeeded(family, center, 0, attempts);
    }

    public static RoomBuildResult TryBuildPit(
        DungeonGrid grid,
        RoomBlockPosition startBlock,
        MonsterPitPreparationResult preparation,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(randomSource);

        var family = RoomFamily.Pit;
        var center = CenterFromBlocks(startBlock, RoomFamilies.Get(family).Footprint);
        if (!grid.TryCommitRoom(startBlock, RoomFamilies.Get(family).Footprint, center))
        {
            return Failed(family);
        }

        WriteDoubleRectangle(grid, center.Row - 4, center.Row + 4, center.Column - 11, center.Column + 11, light: false);
        var attempts = new List<RoomContentAttempt>
        {
            new(RoomContentAttemptKind.SecretDoor, SelectSpecialRoomDoor(center, randomSource)),
        };

        AddPitTier(attempts, preparation.TierDefinitionIds[0], center, rowOffset: -2, columnStart: -9, columnEnd: 9);
        AddPitTier(attempts, preparation.TierDefinitionIds[0], center, rowOffset: 2, columnStart: -9, columnEnd: 9);
        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            AddPitMonster(attempts, preparation.TierDefinitionIds[0], center, rowOffset, -9);
            AddPitMonster(attempts, preparation.TierDefinitionIds[0], center, rowOffset, 9);
            AddPitMonster(attempts, preparation.TierDefinitionIds[1], center, rowOffset, -8);
            AddPitMonster(attempts, preparation.TierDefinitionIds[1], center, rowOffset, 8);
            AddPitMonster(attempts, preparation.TierDefinitionIds[1], center, rowOffset, -7);
            AddPitMonster(attempts, preparation.TierDefinitionIds[1], center, rowOffset, 7);
            AddPitMonster(attempts, preparation.TierDefinitionIds[2], center, rowOffset, -6);
            AddPitMonster(attempts, preparation.TierDefinitionIds[2], center, rowOffset, 6);
            AddPitMonster(attempts, preparation.TierDefinitionIds[2], center, rowOffset, -5);
            AddPitMonster(attempts, preparation.TierDefinitionIds[2], center, rowOffset, 5);
            AddPitMonster(attempts, preparation.TierDefinitionIds[3], center, rowOffset, -4);
            AddPitMonster(attempts, preparation.TierDefinitionIds[3], center, rowOffset, 4);
            AddPitMonster(attempts, preparation.TierDefinitionIds[3], center, rowOffset, -3);
            AddPitMonster(attempts, preparation.TierDefinitionIds[3], center, rowOffset, 3);
            AddPitMonster(attempts, preparation.TierDefinitionIds[4], center, rowOffset, -2);
            AddPitMonster(attempts, preparation.TierDefinitionIds[4], center, rowOffset, 2);
        }

        AddPitTier(attempts, preparation.TierDefinitionIds[5], center, -1, -1, 1);
        AddPitTier(attempts, preparation.TierDefinitionIds[5], center, 1, -1, 1);
        AddPitMonster(attempts, preparation.TierDefinitionIds[6], center, 0, -1);
        AddPitMonster(attempts, preparation.TierDefinitionIds[6], center, 0, 1);
        AddPitMonster(attempts, preparation.TierDefinitionIds[7], center, 0, 0);

        return Succeeded(family, center, 0, attempts);
    }

    public static RoomBuildResult TryBuildSimple(
        DungeonGrid grid,
        RoomBlockPosition startBlock,
        int depth,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        var center = CenterFromBlocks(startBlock, RoomFamilies.Get(RoomFamily.Simple).Footprint);
        var light = depth <= randomSource.Next(1, 26);
        var north = randomSource.Next(1, 5);
        var south = randomSource.Next(1, 4);
        var west = randomSource.Next(1, 12);
        var east = randomSource.Next(1, 12);
        var y1 = center.Row - north;
        var y2 = center.Row + south;
        var x1 = center.Column - west;
        var x2 = center.Column + east;

        if (!grid.TryCommitRoom(startBlock, RoomFamilies.Get(RoomFamily.Simple).Footprint, center))
        {
            return new RoomBuildResult(false, null);
        }

        WriteRectangle(grid, y1, y2, x1, x2, light);
        if (randomSource.Next(0, 20) == 0)
        {
            WritePillars(grid, y1, y2, x1, x2);
        }
        else if (randomSource.Next(0, 50) == 0)
        {
            WriteRaggedEdges(grid, y1, y2, x1, x2);
        }

        return new RoomBuildResult(true, center);
    }

    public static RoomBuildResult TryBuildOverlapping(
        DungeonGrid grid,
        RoomBlockPosition startBlock,
        int depth,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        var center = CenterFromBlocks(startBlock, RoomFamilies.Get(RoomFamily.Overlapping).Footprint);
        var light = depth <= randomSource.Next(1, 26);
        var northA = randomSource.Next(1, 5);
        var southA = randomSource.Next(1, 4);
        var westA = randomSource.Next(1, 12);
        var eastA = randomSource.Next(1, 11);
        var northB = randomSource.Next(1, 4);
        var southB = randomSource.Next(1, 5);
        var westB = randomSource.Next(1, 11);
        var eastB = randomSource.Next(1, 12);

        if (!grid.TryCommitRoom(startBlock, RoomFamilies.Get(RoomFamily.Overlapping).Footprint, center))
        {
            return new RoomBuildResult(false, null);
        }

        WriteRectangle(grid, center.Row - northA, center.Row + southA, center.Column - westA, center.Column + eastA, light);
        WriteRectangle(grid, center.Row - northB, center.Row + southB, center.Column - westB, center.Column + eastB, light);
        return new RoomBuildResult(true, center);
    }

    public static RoomBuildResult TryBuildCross(
        DungeonGrid grid,
        RoomBlockPosition startBlock,
        int depth,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        var center = CenterFromBlocks(startBlock, RoomFamilies.Get(RoomFamily.Cross).Footprint);
        var light = depth <= randomSource.Next(1, 26);
        var verticalHalfHeight = randomSource.Next(3, 5);
        var horizontalHalfWidth = randomSource.Next(3, 12);
        var verticalHalfWidth = 1;
        var horizontalHalfHeight = 1;

        if (!grid.TryCommitRoom(startBlock, RoomFamilies.Get(RoomFamily.Cross).Footprint, center))
        {
            return Failed(RoomFamily.Cross);
        }

        WriteCross(grid, center, verticalHalfHeight, horizontalHalfWidth, light);

        var variant = randomSource.Next(0, 4);
        var attempts = new List<RoomContentAttempt>();
        var verticalLeft = center.Column - verticalHalfWidth;
        var verticalRight = center.Column + verticalHalfWidth;
        var horizontalTop = center.Row - horizontalHalfHeight;
        var horizontalBottom = center.Row + horizontalHalfHeight;

        ApplyCrossVariant(grid, center, variant, horizontalTop, horizontalBottom, verticalLeft, verticalRight, randomSource, attempts);
        return Succeeded(RoomFamily.Cross, center, variant, attempts);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107")]
    private static void ApplyCrossVariant(
        DungeonGrid grid,
        DungeonPosition center,
        int variant,
        int horizontalTop,
        int horizontalBottom,
        int verticalLeft,
        int verticalRight,
        IRandomSource randomSource,
        List<RoomContentAttempt> attempts)
    {
        switch (variant)
        {
            case 1:
                FillRectangle(grid, horizontalTop, horizontalBottom, verticalLeft, verticalRight, InnerWallFeatureId);
                break;
            case 2:
                WriteInnerBorder(grid, horizontalTop, horizontalBottom, verticalLeft, verticalRight);
                attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, SelectCrossVaultDoor(center, horizontalTop, horizontalBottom, verticalLeft, verticalRight, randomSource)));
                attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SpecialObject, center, Special: true));
                AddAttempts(attempts, RoomContentAttemptKind.Monster, center, randomSource.Next(0, 3) + 3, 1, 1, 2);
                AddAttempts(attempts, RoomContentAttemptKind.Trap, center, randomSource.Next(0, 4) + 2, 4, 4);
                break;
            case 3:
                ApplyCrossVariantThree(grid, center, horizontalTop, horizontalBottom, verticalLeft, verticalRight, randomSource, attempts);
                break;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107")]
    private static void ApplyCrossVariantThree(
        DungeonGrid grid,
        DungeonPosition center,
        int horizontalTop,
        int horizontalBottom,
        int verticalLeft,
        int verticalRight,
        IRandomSource randomSource,
        List<RoomContentAttempt> attempts)
    {
        var branch = randomSource.Next(0, 3);
        if (branch == 0)
        {
            for (var row = horizontalTop; row <= horizontalBottom; row++)
            {
                if (row != center.Row)
                {
                    SetFeature(grid, row, verticalLeft - 1, InnerWallFeatureId);
                    SetFeature(grid, row, verticalRight + 1, InnerWallFeatureId);
                }
            }

            for (var column = verticalLeft; column <= verticalRight; column++)
            {
                if (column != center.Column)
                {
                    SetFeature(grid, horizontalTop - 1, column, InnerWallFeatureId);
                    SetFeature(grid, horizontalBottom + 1, column, InnerWallFeatureId);
                }
            }

            if (randomSource.Next(0, 3) == 0)
            {
                AddSecretDoor(attempts, center.Row, verticalLeft - 1);
                AddSecretDoor(attempts, center.Row, verticalRight + 1);
                AddSecretDoor(attempts, horizontalTop - 1, center.Column);
                AddSecretDoor(attempts, horizontalBottom + 1, center.Column);
            }
            return;
        }

        if (randomSource.Next(0, 3) == 0)
        {
            SetFeature(grid, center.Row, center.Column, InnerWallFeatureId);
            SetFeature(grid, horizontalTop, center.Column, InnerWallFeatureId);
            SetFeature(grid, horizontalBottom, center.Column, InnerWallFeatureId);
            SetFeature(grid, center.Row, verticalLeft, InnerWallFeatureId);
            SetFeature(grid, center.Row, verticalRight, InnerWallFeatureId);
            return;
        }

        if (randomSource.Next(0, 3) == 0)
        {
            SetFeature(grid, center.Row, center.Column, InnerWallFeatureId);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776")]
    public static RoomBuildResult TryBuildLarge(
        DungeonGrid grid,
        RoomBlockPosition startBlock,
        int depth,
        IRandomSource randomSource,
        bool isQuestLevel = false)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(randomSource);

        var center = CenterFromBlocks(startBlock, RoomFamilies.Get(RoomFamily.Large).Footprint);
        var outerTop = center.Row - 4;
        var outerBottom = center.Row + 4;
        var outerLeft = center.Column - 11;
        var outerRight = center.Column + 11;
        var light = depth <= randomSource.Next(1, 26);

        if (!grid.TryCommitRoom(startBlock, RoomFamilies.Get(RoomFamily.Large).Footprint, center))
        {
            return Failed(RoomFamily.Large);
        }

        WriteDoubleRectangle(grid, outerTop, outerBottom, outerLeft, outerRight, light);
        var innerTop = outerTop + 2;
        var innerBottom = outerBottom - 2;
        var innerLeft = outerLeft + 2;
        var innerRight = outerRight - 2;
        var variant = randomSource.Next(1, 6);
        var attempts = new List<RoomContentAttempt>();

        switch (variant)
        {
            case 1:
                AddSecretDoorAttempt(attempts, center, innerTop, innerBottom, innerLeft, innerRight, randomSource);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, center, 1, 1, 1, 2);
                break;
            case 2:
                AddSecretDoorAttempt(attempts, center, innerTop, innerBottom, innerLeft, innerRight, randomSource);
                WriteInnerRoom(grid, center);
                AddLockedDoorAttempt(attempts, center, randomSource);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, center, randomSource.Next(1, 4) + 2, 1, 1, 2);
                if (randomSource.Next(0, 100) < 80)
                {
                    attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SpecialObject, center, Special: true));
                }
                else
                {
                    var preparedStairFeatureId = grid.GetFeatureId(center) == OpenFloorFeatureId
                        ? DungeonStairGenerator.PrepareRandomStairFeature(depth, isQuestLevel, randomSource)
                        : null;
                    attempts.Add(new RoomContentAttempt(
                        RoomContentAttemptKind.RandomStair,
                        center,
                        PreparedStairFeatureId: preparedStairFeatureId));
                }
                AddAttempts(attempts, RoomContentAttemptKind.Trap, center, randomSource.Next(1, 4) + 2, 4, 10);
                break;
            case 3:
                AddSecretDoorAttempt(attempts, center, innerTop, innerBottom, innerLeft, innerRight, randomSource);
                WriteInnerRoom(grid, center);
                if (randomSource.Next(0, 2) == 0)
                {
                    var offset = randomSource.Next(1, 3);
                    WriteLargeSidePillars(grid, center, offset);
                }
                if (randomSource.Next(0, 3) == 0)
                {
                    WriteInnerSideRooms(grid, center);
                    attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, new DungeonPosition(center.Row - 3 + randomSource.Next(1, 3) * 2, center.Column - 3)));
                    attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, new DungeonPosition(center.Row - 3 + randomSource.Next(1, 3) * 2, center.Column + 3)));
                    AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row, center.Column - 2), randomSource.Next(1, 3), 1, 1, 2);
                    AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row, center.Column + 2), randomSource.Next(1, 3), 1, 1, 2);
                    if (randomSource.Next(0, 3) == 0)
                    {
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SpecialObject, new DungeonPosition(center.Row, center.Column - 2), Special: true));
                    }
                    if (randomSource.Next(0, 3) == 0)
                    {
                        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SpecialObject, new DungeonPosition(center.Row, center.Column + 2), Special: true));
                    }
                }
                break;
            case 4:
                AddSecretDoorAttempt(attempts, center, innerTop, innerBottom, innerLeft, innerRight, randomSource);
                WriteCheckerboard(grid, innerTop, innerBottom, innerLeft, innerRight);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row, center.Column - 5), randomSource.Next(1, 4), 1, 1, 2);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row, center.Column + 5), randomSource.Next(1, 4), 1, 1, 2);
                AddAttempts(attempts, RoomContentAttemptKind.Trap, new DungeonPosition(center.Row, center.Column - 3), randomSource.Next(1, 4), 2, 8);
                AddAttempts(attempts, RoomContentAttemptKind.Trap, new DungeonPosition(center.Row, center.Column + 3), randomSource.Next(1, 4), 2, 8);
                AddAttempts(attempts, RoomContentAttemptKind.ObjectOrGold, center, 3, 2, 3);
                break;
            case 5:
                WriteFourRoomCross(grid, center, innerTop, innerBottom, innerLeft, innerRight);
                if (randomSource.Next(0, 100) < 50)
                {
                    var offset = randomSource.Next(1, 11);
                    AddSecretDoor(attempts, center.Row - 4, center.Column - offset);
                    AddSecretDoor(attempts, center.Row - 4, center.Column + offset);
                    AddSecretDoor(attempts, center.Row + 4, center.Column - offset);
                    AddSecretDoor(attempts, center.Row + 4, center.Column + offset);
                }
                else
                {
                    var offset = randomSource.Next(1, 4);
                    AddSecretDoor(attempts, center.Row + offset, innerLeft - 1);
                    AddSecretDoor(attempts, center.Row - offset, innerLeft - 1);
                    AddSecretDoor(attempts, center.Row + offset, innerRight + 1);
                    AddSecretDoor(attempts, center.Row - offset, innerRight + 1);
                }
                AddAttempts(attempts, RoomContentAttemptKind.ObjectOrGold, center, randomSource.Next(1, 3) + 2, 2, 3);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row + 1, center.Column - 4), randomSource.Next(1, 5), 1, 1, 2);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row + 1, center.Column + 4), randomSource.Next(1, 5), 1, 1, 2);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row - 1, center.Column - 4), randomSource.Next(1, 5), 1, 1, 2);
                AddAttempts(attempts, RoomContentAttemptKind.Monster, new DungeonPosition(center.Row - 1, center.Column + 4), randomSource.Next(1, 5), 1, 1, 2);
                break;
        }

        return Succeeded(RoomFamily.Large, center, variant, attempts);
    }

    private static void AddSecretDoorAttempt(
        List<RoomContentAttempt> attempts,
        DungeonPosition center,
        int top,
        int bottom,
        int left,
        int right,
        IRandomSource randomSource)
    {
        var door = randomSource.Next(1, 5) switch
        {
            1 => new DungeonPosition(top - 1, center.Column),
            2 => new DungeonPosition(bottom + 1, center.Column),
            3 => new DungeonPosition(center.Row, left - 1),
            _ => new DungeonPosition(center.Row, right + 1),
        };
        AddSecretDoor(attempts, door.Row, door.Column);
    }

    private static DungeonPosition SelectCrossVaultDoor(
        DungeonPosition center,
        int top,
        int bottom,
        int left,
        int right,
        IRandomSource randomSource) => randomSource.Next(0, 4) switch
        {
            0 => new DungeonPosition(top, center.Column),
            1 => new DungeonPosition(bottom, center.Column),
            2 => new DungeonPosition(center.Row, left),
            _ => new DungeonPosition(center.Row, right),
        };

    private static void AddLockedDoorAttempt(
        List<RoomContentAttempt> attempts,
        DungeonPosition center,
        IRandomSource randomSource)
    {
        var door = randomSource.Next(1, 5) switch
        {
            1 => new DungeonPosition(center.Row - 1, center.Column),
            2 => new DungeonPosition(center.Row + 1, center.Column),
            3 => new DungeonPosition(center.Row, center.Column - 1),
            _ => new DungeonPosition(center.Row, center.Column + 1),
        };
        var state = DungeonDoorGenerator.CreateLockedDoorState(randomSource);
        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.LockedDoor, door, PreparedDoorState: state));
    }

    private static void AddSecretDoor(List<RoomContentAttempt> attempts, int row, int column) =>
        attempts.Add(new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, new DungeonPosition(row, column)));

    private static DungeonPosition SelectSpecialRoomDoor(DungeonPosition center, IRandomSource randomSource) =>
        randomSource.Next(1, 5) switch
        {
            1 => new DungeonPosition(center.Row - 3, center.Column),
            2 => new DungeonPosition(center.Row + 3, center.Column),
            3 => new DungeonPosition(center.Row, center.Column - 10),
            _ => new DungeonPosition(center.Row, center.Column + 10),
        };

    private static void AddPitTier(
        List<RoomContentAttempt> attempts,
        string definitionId,
        DungeonPosition center,
        int rowOffset,
        int columnStart,
        int columnEnd)
    {
        for (var columnOffset = columnStart; columnOffset <= columnEnd; columnOffset++)
        {
            AddPitMonster(attempts, definitionId, center, rowOffset, columnOffset);
        }
    }

    private static void AddPitMonster(
        List<RoomContentAttempt> attempts,
        string definitionId,
        DungeonPosition center,
        int rowOffset,
        int columnOffset) =>
        attempts.Add(new RoomContentAttempt(
            RoomContentAttemptKind.Monster,
            new DungeonPosition(center.Row + rowOffset, center.Column + columnOffset),
            DefinitionId: definitionId,
            AllowGroupExpansion: false,
            MonsterSource: RoomMonsterAttemptSource.PreparedNestPit));

    private static void AddAttempts(
        List<RoomContentAttempt> attempts,
        RoomContentAttemptKind kind,
        DungeonPosition origin,
        int count,
        int radiusRows = 0,
        int radiusColumns = 0,
        int generationDepthOffset = 0)
    {
        for (var index = 0; index < count; index++)
        {
            attempts.Add(new RoomContentAttempt(
                kind,
                origin,
                radiusRows,
                radiusColumns,
                generationDepthOffset,
                kind == RoomContentAttemptKind.SpecialObject,
                MonsterSource: kind == RoomContentAttemptKind.Monster
                    ? RoomMonsterAttemptSource.OrdinaryRoomVaultMonsters
                    : RoomMonsterAttemptSource.Unspecified,
                SleepOnSpawn: kind == RoomContentAttemptKind.Monster));
        }
    }

    private static RoomBuildResult Failed(RoomFamily family) => new(false, null, family, null, []);

    private static RoomBuildResult Succeeded(RoomFamily family, DungeonPosition center, int variant, IReadOnlyList<RoomContentAttempt> attempts) =>
        new(true, center, family, variant, attempts);

    private static DungeonPosition CenterFromBlocks(RoomBlockPosition start, RoomBlockFootprint footprint) =>
        new(
            ((start.Row + footprint.HeightBlocks - 1 + 1) * DungeonGrid.BlockHeight) / 2,
            ((start.Column + footprint.WidthBlocks - 1 + 1) * DungeonGrid.BlockWidth) / 2);

    private static void WriteCross(
        DungeonGrid grid,
        DungeonPosition center,
        int verticalHalfHeight,
        int horizontalHalfWidth,
        bool light)
    {
        WriteRectangle(grid, center.Row - verticalHalfHeight, center.Row + verticalHalfHeight, center.Column - 1, center.Column + 1, light);
        WriteRectangle(grid, center.Row - 1, center.Row + 1, center.Column - horizontalHalfWidth, center.Column + horizontalHalfWidth, light);
        FillRectangle(grid, center.Row - verticalHalfHeight, center.Row + verticalHalfHeight, center.Column - 1, center.Column + 1, OpenFloorFeatureId);
        FillRectangle(grid, center.Row - 1, center.Row + 1, center.Column - horizontalHalfWidth, center.Column + horizontalHalfWidth, OpenFloorFeatureId);
    }

    private static void WriteDoubleRectangle(DungeonGrid grid, int y1, int y2, int x1, int x2, bool light)
    {
        WriteRectangle(grid, y1, y2, x1, x2, light);
        WriteInnerBorder(grid, y1 + 1, y2 - 1, x1 + 1, x2 - 1);
    }

    private static void WriteInnerBorder(DungeonGrid grid, int y1, int y2, int x1, int x2)
    {
        for (var row = y1; row <= y2; row++)
        {
            SetFeature(grid, row, x1, InnerWallFeatureId);
            SetFeature(grid, row, x2, InnerWallFeatureId);
        }

        for (var column = x1; column <= x2; column++)
        {
            SetFeature(grid, y1, column, InnerWallFeatureId);
            SetFeature(grid, y2, column, InnerWallFeatureId);
        }
    }

    private static void FillRectangle(DungeonGrid grid, int y1, int y2, int x1, int x2, string featureId)
    {
        for (var row = y1; row <= y2; row++)
        {
            for (var column = x1; column <= x2; column++)
            {
                SetFeature(grid, row, column, featureId);
            }
        }
    }

    private static void WriteInnerRoom(DungeonGrid grid, DungeonPosition center)
    {
        for (var row = center.Row - 1; row <= center.Row + 1; row++)
        {
            for (var column = center.Column - 1; column <= center.Column + 1; column++)
            {
                if (row != center.Row || column != center.Column)
                {
                    SetFeature(grid, row, column, InnerWallFeatureId);
                }
            }
        }
    }

    private static void WriteLargeSidePillars(DungeonGrid grid, DungeonPosition center, int offset)
    {
        for (var row = center.Row - 1; row <= center.Row + 1; row++)
        {
            for (var column = center.Column - 5 - offset; column <= center.Column - 3 - offset; column++)
            {
                SetFeature(grid, row, column, InnerWallFeatureId);
            }

            for (var column = center.Column + 3 + offset; column <= center.Column + 5 + offset; column++)
            {
                SetFeature(grid, row, column, InnerWallFeatureId);
            }
        }
    }

    private static void WriteInnerSideRooms(DungeonGrid grid, DungeonPosition center)
    {
        for (var column = center.Column - 5; column <= center.Column + 5; column++)
        {
            SetFeature(grid, center.Row - 1, column, InnerWallFeatureId);
            SetFeature(grid, center.Row + 1, column, InnerWallFeatureId);
        }

        SetFeature(grid, center.Row, center.Column - 5, InnerWallFeatureId);
        SetFeature(grid, center.Row, center.Column + 5, InnerWallFeatureId);
    }

    private static void WriteCheckerboard(DungeonGrid grid, int y1, int y2, int x1, int x2)
    {
        for (var row = y1; row <= y2; row++)
        {
            for (var column = x1; column <= x2; column++)
            {
                if (((row + column) & 1) == 1)
                {
                    SetFeature(grid, row, column, InnerWallFeatureId);
                }
            }
        }
    }

    private static void WriteFourRoomCross(DungeonGrid grid, DungeonPosition center, int y1, int y2, int x1, int x2)
    {
        for (var row = y1; row <= y2; row++)
        {
            SetFeature(grid, row, center.Column, InnerWallFeatureId);
        }

        for (var column = x1; column <= x2; column++)
        {
            SetFeature(grid, center.Row, column, InnerWallFeatureId);
        }
    }

    private static void WriteRectangle(DungeonGrid grid, int y1, int y2, int x1, int x2, bool light)
    {
        for (var row = y1 - 1; row <= y2 + 1; row++)
        {
            for (var column = x1 - 1; column <= x2 + 1; column++)
            {
                var position = new DungeonPosition(row, column);
                grid.SetFeatureId(position, OpenFloorFeatureId);
                grid.AddCellFlags(position, DungeonCellStates.Room | (light ? DungeonCellStates.Glow : DungeonCellStates.None));
            }
        }

        for (var row = y1 - 1; row <= y2 + 1; row++)
        {
            SetFeature(grid, row, x1 - 1, OuterWallFeatureId);
            SetFeature(grid, row, x2 + 1, OuterWallFeatureId);
        }

        for (var column = x1 - 1; column <= x2 + 1; column++)
        {
            SetFeature(grid, y1 - 1, column, OuterWallFeatureId);
            SetFeature(grid, y2 + 1, column, OuterWallFeatureId);
        }
    }

    private static void WritePillars(DungeonGrid grid, int y1, int y2, int x1, int x2)
    {
        for (var row = y1; row <= y2; row += 2)
        {
            for (var column = x1; column <= x2; column += 2)
            {
                SetFeature(grid, row, column, InnerWallFeatureId);
            }
        }
    }

    private static void WriteRaggedEdges(DungeonGrid grid, int y1, int y2, int x1, int x2)
    {
        for (var row = y1 + 2; row <= y2 - 2; row += 2)
        {
            SetFeature(grid, row, x1, InnerWallFeatureId);
            SetFeature(grid, row, x2, InnerWallFeatureId);
        }

        for (var column = x1 + 2; column <= x2 - 2; column += 2)
        {
            SetFeature(grid, y1, column, InnerWallFeatureId);
            SetFeature(grid, y2, column, InnerWallFeatureId);
        }
    }

    private static void SetFeature(DungeonGrid grid, int row, int column, string featureId) =>
        grid.SetFeatureId(new DungeonPosition(row, column), featureId);
}
