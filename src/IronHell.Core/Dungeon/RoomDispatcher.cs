using IronHell.Core.Definitions;
using IronHell.Core.Monsters;
using IronHell.Core.Randomness;

namespace IronHell.Core.Dungeon;

public sealed record RoomDispatchResult(
    int AttemptCount,
    IReadOnlyList<RoomBuildResult> SuccessfulRooms)
{
    public int SuccessfulRoomCount => SuccessfulRooms.Count;

    public int FailedRoomCount => AttemptCount - SuccessfulRoomCount;

}

public sealed record RoomDispatchInputs(
    IReadOnlyList<VaultDefinition> VaultDefinitions,
    IReadOnlyList<MonsterDefinition> MonsterDefinitions,
    IReadOnlyList<MonsterAllocationEntry> MonsterAllocationEntries,
    bool IsQuestLevel = false);

public static class RoomDispatcher
{
    public const int AttemptCount = 50;
    public const int UnusualThreshold = 200;

    public static RoomDispatchResult Generate(
        DungeonGrid grid,
        int depth,
        IReadOnlyList<VaultDefinition> vaultDefinitions,
        IRandomSource randomSource)
        => Generate(grid, depth, new RoomDispatchInputs(vaultDefinitions, [], []), randomSource);

    public static RoomDispatchResult Generate(
        DungeonGrid grid,
        int depth,
        RoomDispatchInputs inputs,
        IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(randomSource);

        var successfulRooms = new List<RoomBuildResult>();
        for (var attempt = 0; attempt < AttemptCount; attempt++)
        {
            var start = new RoomBlockPosition(
                randomSource.Next(0, DungeonGrid.RoomBlockRows),
                randomSource.Next(0, DungeonGrid.RoomBlockColumns));
            var room = TryBuildAttempt(grid, start, depth, inputs, randomSource);
            if (room.Succeeded)
            {
                successfulRooms.Add(room);
            }
        }

        return new RoomDispatchResult(AttemptCount, successfulRooms.AsReadOnly());
    }

    private static RoomBuildResult TryBuildAttempt(
        DungeonGrid grid,
        RoomBlockPosition start,
        int depth,
        RoomDispatchInputs inputs,
        IRandomSource randomSource)
    {
        if (randomSource.Next(0, UnusualThreshold) >= depth)
        {
            return TryBuildFamily(grid, start, depth, RoomFamily.Simple, null, randomSource, inputs.IsQuestLevel);
        }

        var familyRoll = randomSource.Next(0, 100);
        if (randomSource.Next(0, UnusualThreshold) < depth)
        {
            var veryUnusual = TrySpecialFamily(grid, start, depth, familyRoll, inputs, randomSource);
            if (veryUnusual?.Succeeded == true)
            {
                return veryUnusual;
            }
        }

        if (familyRoll < 25)
        {
            var large = TryBuildFamily(grid, start, depth, RoomFamily.Large, null, randomSource, inputs.IsQuestLevel);
            if (large.Succeeded)
            {
                return large;
            }
        }

        if (familyRoll < 50)
        {
            var cross = TryBuildFamily(grid, start, depth, RoomFamily.Cross, null, randomSource, inputs.IsQuestLevel);
            if (cross.Succeeded)
            {
                return cross;
            }
        }

        if (familyRoll < 100)
        {
            var overlapping = TryBuildFamily(grid, start, depth, RoomFamily.Overlapping, null, randomSource, inputs.IsQuestLevel);
            if (overlapping.Succeeded)
            {
                return overlapping;
            }
        }

        return TryBuildFamily(grid, start, depth, RoomFamily.Simple, null, randomSource, inputs.IsQuestLevel);
    }

    private static RoomBuildResult? TrySpecialFamily(
        DungeonGrid grid,
        RoomBlockPosition start,
        int depth,
        int familyRoll,
        RoomDispatchInputs inputs,
        IRandomSource randomSource)
    {
        if (familyRoll < 10)
        {
            return TryBuildVault(grid, start, depth, RoomFamily.GreaterVault, inputs.VaultDefinitions, randomSource);
        }

        if (familyRoll < 25)
        {
            return TryBuildVault(grid, start, depth, RoomFamily.LesserVault, inputs.VaultDefinitions, randomSource);
        }

        if (familyRoll < 40)
        {
            return TryBuildPit(grid, start, depth, inputs, randomSource);
        }

        if (familyRoll < 50)
        {
            return TryBuildNest(grid, start, depth, inputs, randomSource);
        }

        return null;
    }

    private static RoomBuildResult TryBuildFamily(
        DungeonGrid grid,
        RoomBlockPosition start,
        int depth,
        RoomFamily family,
        VaultDefinition? definition,
        IRandomSource randomSource,
        bool isQuestLevel = false)
    {
        var metadata = RoomFamilies.Get(family);
        if (depth < metadata.MinimumDepth || !grid.IsRoomFootprintAvailable(start, metadata.Footprint))
        {
            return Failed(family);
        }

        var result = family switch
        {
            RoomFamily.Simple => RoomGeometryBuilder.TryBuildSimple(grid, start, depth, randomSource),
            RoomFamily.Overlapping => RoomGeometryBuilder.TryBuildOverlapping(grid, start, depth, randomSource),
            RoomFamily.Cross => RoomGeometryBuilder.TryBuildCross(grid, start, depth, randomSource),
            RoomFamily.Large => RoomGeometryBuilder.TryBuildLarge(grid, start, depth, randomSource, isQuestLevel),
            RoomFamily.LesserVault or RoomFamily.GreaterVault => VaultRoomBuilder.TryBuild(grid, start, depth, definition!, randomSource),
            _ => Failed(family),
        };

        return result.Succeeded && result.Family is null
            ? result with { Family = family }
            : result;
    }

    private static RoomBuildResult TryBuildVault(
        DungeonGrid grid,
        RoomBlockPosition start,
        int depth,
        RoomFamily family,
        IReadOnlyList<VaultDefinition> definitions,
        IRandomSource randomSource)
    {
        var metadata = RoomFamilies.Get(family);
        if (depth < metadata.MinimumDepth || !grid.IsRoomFootprintAvailable(start, metadata.Footprint))
        {
            return Failed(family);
        }

        var definition = VaultRoomBuilder.Select(definitions, family, depth, randomSource);
        return definition is null
            ? Failed(family)
            : TryBuildFamily(grid, start, depth, family, definition, randomSource);
    }

    private static RoomBuildResult TryBuildNest(
        DungeonGrid grid,
        RoomBlockPosition start,
        int depth,
        RoomDispatchInputs inputs,
        IRandomSource randomSource)
    {
        if (!CanBuild(grid, start, depth, RoomFamily.Nest))
        {
            return Failed(RoomFamily.Nest);
        }

        var preparation = MonsterNestPreparer.Prepare(
            inputs.MonsterDefinitions,
            inputs.MonsterAllocationEntries,
            depth,
            randomSource);
        return preparation.IsSuccess
            ? RoomGeometryBuilder.TryBuildNest(grid, start, preparation, randomSource)
            : Failed(RoomFamily.Nest);
    }

    private static RoomBuildResult TryBuildPit(
        DungeonGrid grid,
        RoomBlockPosition start,
        int depth,
        RoomDispatchInputs inputs,
        IRandomSource randomSource)
    {
        if (!CanBuild(grid, start, depth, RoomFamily.Pit))
        {
            return Failed(RoomFamily.Pit);
        }

        var preparation = MonsterPitPreparer.Prepare(
            inputs.MonsterDefinitions,
            inputs.MonsterAllocationEntries,
            depth,
            randomSource);
        return preparation.IsSuccess
            ? RoomGeometryBuilder.TryBuildPit(grid, start, preparation, randomSource)
            : Failed(RoomFamily.Pit);
    }

    private static bool CanBuild(DungeonGrid grid, RoomBlockPosition start, int depth, RoomFamily family)
    {
        var metadata = RoomFamilies.Get(family);
        return depth >= metadata.MinimumDepth && grid.IsRoomFootprintAvailable(start, metadata.Footprint);
    }

    private static RoomBuildResult Failed(RoomFamily family) => new(false, null, family, ContentAttempts: []);
}