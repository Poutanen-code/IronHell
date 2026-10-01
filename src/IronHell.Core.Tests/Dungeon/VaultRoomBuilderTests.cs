using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class VaultRoomBuilderTests
{
    [Fact]
    public void VaultEligibility_DistinguishesLesserAndGreaterDefinitions()
    {
        var definitions = Definitions(7, 8);

        Assert.Equal("lesser", VaultRoomBuilder.Select(definitions, RoomFamily.LesserVault, 5, new ScriptedRandomSource(0))!.Id);
        Assert.Equal("greater", VaultRoomBuilder.Select(definitions, RoomFamily.GreaterVault, 10, new ScriptedRandomSource(1))!.Id);
        Assert.Null(VaultRoomBuilder.Select(definitions, RoomFamily.GreaterVault, 9, new ScriptedRandomSource(0)));
    }

    [Fact]
    public void VaultSelection_UsesDefinitionRangeAndNormalizesType9()
    {
        var definitions = Definitions(7, 9);
        var random = new ScriptedRandomSource(1);

        var selected = VaultRoomBuilder.Select(definitions, RoomFamily.GreaterVault, 10, random);

        Assert.Equal("type9", selected!.Id);
        Assert.Equal(8, selected.EffectiveType);
        Assert.Equal((0, 2), random.Requests.Single());
    }

    [Fact]
    public void VaultSelection_NoEligibleDefinition_FailsWithoutMutation()
    {
        var selected = VaultRoomBuilder.Select(Definitions(7), RoomFamily.GreaterVault, 10, new ScriptedRandomSource(0));

        Assert.Null(selected);
    }

    [Fact]
    public void VaultCoordinates_AsymmetricTemplate_IsNotTransposedOrMirrored()
    {
        var grid = new DungeonGrid();
        var definition = new VaultDefinition("asymmetric", 7, 1, [
            "%.#",
            "X,+"]);

        var result = VaultRoomBuilder.TryBuild(grid, new RoomBlockPosition(0, 0), 5, definition, new ScriptedRandomSource());

        Assert.True(result.Succeeded);
        Assert.Equal(RoomGeometryBuilder.OuterWallFeatureId, grid.GetFeatureId(new DungeonPosition(10, 15)));
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(10, 16)));
        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(10, 17)));
        Assert.Equal(VaultRoomBuilder.PermanentInnerWallFeatureId, grid.GetFeatureId(new DungeonPosition(11, 15)));
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(11, 16)));
        Assert.Equal(RoomGeometryBuilder.OpenFloorFeatureId, grid.GetFeatureId(new DungeonPosition(11, 17)));
    }

    [Fact]
    public void LesserAndGreaterVaults_UseVerifiedReservationFootprints()
    {
        var lesserGrid = new DungeonGrid();
        var greaterGrid = new DungeonGrid();

        Assert.True(VaultRoomBuilder.TryBuild(lesserGrid, new RoomBlockPosition(0, 0), 5, Definitions(7)[0], new ScriptedRandomSource()).Succeeded);
        Assert.True(VaultRoomBuilder.TryBuild(greaterGrid, new RoomBlockPosition(0, 0), 10, Definitions(8)[0], new ScriptedRandomSource()).Succeeded);
        Assert.True(lesserGrid.IsBlockReserved(new RoomBlockPosition(1, 2)));
        Assert.False(lesserGrid.IsBlockReserved(new RoomBlockPosition(2, 0)));
        Assert.True(greaterGrid.IsBlockReserved(new RoomBlockPosition(3, 5)));
        Assert.Single(lesserGrid.RoomCenters);
        Assert.Single(greaterGrid.RoomCenters);
    }

    [Fact]
    public void VaultReservationFailure_LeavesGridCenterAndAttemptsUnchanged()
    {
        var grid = new DungeonGrid();
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(0, 0), new RoomBlockFootprint(2, 3), new DungeonPosition(16, 16)));

        var result = VaultRoomBuilder.TryBuild(grid, new RoomBlockPosition(0, 0), 5, Definitions(7)[0], new ScriptedRandomSource());

        Assert.False(result.Succeeded);
        Assert.Empty(result.Attempts);
        Assert.Single(grid.RoomCenters);
        Assert.Null(grid.GetFeatureId(new DungeonPosition(10, 16)));
    }

    [Fact]
    public void VaultCells_ReceiveRoomAndIckyWithoutGlow()
    {
        var grid = new DungeonGrid();
        var definition = new VaultDefinition("flags", 7, 1, ["..."]);

        _ = VaultRoomBuilder.TryBuild(grid, new RoomBlockPosition(0, 0), 5, definition, new ScriptedRandomSource());

        Assert.Equal(DungeonCellStates.Room | DungeonCellStates.Icky, grid.GetCellFlags(new DungeonPosition(11, 15)));
    }

    [Fact]
    public void VaultGlyphs_EmitSequentialAttemptsAndExactDepthOffsets()
    {
        var definition = new VaultDefinition("glyphs", 7, 1, ["9", "8", "&", "@", "^"]);
        var result = VaultRoomBuilder.TryBuild(new DungeonGrid(), new RoomBlockPosition(0, 0), 5, definition, new ScriptedRandomSource());

        Assert.Equal(
            [
                (RoomContentAttemptKind.Trap, 0, 0),
                (RoomContentAttemptKind.Monster, 9, 0),
                (RoomContentAttemptKind.SpecialObject, 7, 0),
                (RoomContentAttemptKind.Monster, 40, 0),
                (RoomContentAttemptKind.SpecialObject, 20, 0),
                (RoomContentAttemptKind.Monster, 5, 0),
                (RoomContentAttemptKind.Monster, 11, 0),
            ],
            result.Attempts.Select(attempt => (attempt.Kind, attempt.GenerationDepthOffset, attempt.RadiusRows)));
        Assert.Equal(result.Attempts[1].Origin, result.Attempts[2].Origin);
    }

    [Fact]
    public void IdenticalVaultInputAndRng_ReproducesGridAndAttempts()
    {
        var definition = new VaultDefinition("replay", 7, 1, ["*,,", ",9,"]);
        var firstGrid = new DungeonGrid();
        var secondGrid = new DungeonGrid();
        var first = VaultRoomBuilder.TryBuild(firstGrid, new RoomBlockPosition(0, 0), 5, definition, new ScriptedRandomSource(1, 0, 1, 0, 1, 0, 1, 0));
        var second = VaultRoomBuilder.TryBuild(secondGrid, new RoomBlockPosition(0, 0), 5, definition, new ScriptedRandomSource(1, 0, 1, 0, 1, 0, 1, 0));

        Assert.Equal(first.Attempts, second.Attempts);
        Assert.Equal(firstGrid.GetFeatureId(new DungeonPosition(10, 15)), secondGrid.GetFeatureId(new DungeonPosition(10, 15)));
        Assert.Equal(firstGrid.GetCellFlags(new DungeonPosition(11, 16)), secondGrid.GetCellFlags(new DungeonPosition(11, 16)));
    }

    private static IReadOnlyList<VaultDefinition> Definitions(params int[] types) =>
        types.Select(type => new VaultDefinition(type switch
        {
            7 => "lesser",
            8 => "greater",
            _ => "type9",
        }, type, 1, ["."])).ToArray();

    private sealed class ScriptedRandomSource(params int[] values) : IRandomSource
    {
        private int _index;

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Requests.Add((minInclusive, maxExclusive));
            var value = _index < values.Length ? values[_index++] : minInclusive;
            return Math.Clamp(value, minInclusive, maxExclusive - 1);
        }

        public int RollDice(int count, int sides)
        {
            var total = 0;
            for (var index = 0; index < count; index++)
            {
                total += Next(1, sides + 1);
            }

            return total;
        }
    }
}