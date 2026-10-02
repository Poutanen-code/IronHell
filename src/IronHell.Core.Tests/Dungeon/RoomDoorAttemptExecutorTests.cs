using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class RoomDoorAttemptExecutorTests
{
    [Fact]
    public void Execute_SecretDoor_WritesExactFeatureWithoutRng()
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        var flags = DungeonCellStates.Room | DungeonCellStates.Icky | DungeonCellStates.Glow;
        grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        grid.AddCellFlags(position, flags);
        var random = new ScriptedRandomSource();

        var result = RoomDoorAttemptExecutor.Execute(grid, [new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, position)], random);

        Assert.Equal(DungeonGrid.SecretDoorFeatureId, grid.GetFeatureId(position));
        Assert.Null(grid.GetDoorState(position));
        Assert.Equal(flags, grid.GetCellFlags(position));
        Assert.Empty(result.RemainingAttempts);
        Assert.Single(result.ExecutedDoorAttempts);
        Assert.Empty(random.Requests);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    public void Execute_LockedDoor_UsesExactPowerRoll(int power)
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);
        var random = new ScriptedRandomSource(power);

        var result = RoomDoorAttemptExecutor.Execute(grid, [new RoomContentAttempt(RoomContentAttemptKind.LockedDoor, position)], random);

        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(DoorCondition.Locked, power), grid.GetDoorState(position));
        Assert.Empty(result.RemainingAttempts);
        Assert.Equal((1, 8), Assert.Single(random.Requests));
    }

    [Fact]
    public void Execute_MixedAttempts_ExecutesOnlyDoorsAndPreservesRemainingOrder()
    {
        var grid = RockGrid();
        var secret = new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, new DungeonPosition(10, 10));
        var monster = new RoomContentAttempt(RoomContentAttemptKind.Monster, new DungeonPosition(10, 11), DefinitionId: "orc");
        var locked = new RoomContentAttempt(RoomContentAttemptKind.LockedDoor, new DungeonPosition(10, 12));
        var trap = new RoomContentAttempt(RoomContentAttemptKind.Trap, new DungeonPosition(10, 13));
        var item = new RoomContentAttempt(RoomContentAttemptKind.Object, new DungeonPosition(10, 14));
        var stair = new RoomContentAttempt(RoomContentAttemptKind.RandomStair, new DungeonPosition(10, 15));
        var random = new ScriptedRandomSource(4);

        var result = RoomDoorAttemptExecutor.Execute(grid, [secret, monster, locked, trap, item, stair], random);

        Assert.Equal([secret, locked], result.ExecutedDoorAttempts);
        Assert.Equal([monster, trap, item, stair], result.RemainingAttempts);
        Assert.Equal(DungeonGrid.SecretDoorFeatureId, grid.GetFeatureId(secret.Origin));
        Assert.Equal(new DoorState(DoorCondition.Locked, 4), grid.GetDoorState(locked.Origin));
        Assert.Equal([(1, 8)], random.Requests);
    }

    [Fact]
    public void Execute_ProcessesDoorsInOriginalOrderIncludingSameCell()
    {
        var grid = RockGrid();
        var position = new DungeonPosition(10, 10);
        var attempts = new[]
        {
            new RoomContentAttempt(RoomContentAttemptKind.LockedDoor, position),
            new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, position),
            new RoomContentAttempt(RoomContentAttemptKind.LockedDoor, position),
        };
        var random = new ScriptedRandomSource(2, 6);

        var result = RoomDoorAttemptExecutor.Execute(grid, attempts, random);

        Assert.Equal(attempts, result.ExecutedDoorAttempts);
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(DoorCondition.Locked, 6), grid.GetDoorState(position));
        Assert.Equal([(1, 8), (1, 8)], random.Requests);
    }

    [Fact]
    public void Execute_VaultSecretDoor_PreservesRoomAndIckyFlags()
    {
        var grid = RockGrid();
        var definition = new VaultDefinition("door_vault", 7, 1, [".+."]);
        var vault = VaultRoomBuilder.TryBuild(grid, new RoomBlockPosition(0, 0), 5, definition, new ScriptedRandomSource());
        var request = Assert.Single(vault.Attempts);
        var random = new ScriptedRandomSource();

        _ = RoomDoorAttemptExecutor.Execute(grid, vault.Attempts, random);

        Assert.Equal(RoomContentAttemptKind.SecretDoor, request.Kind);
        Assert.Equal(DungeonGrid.SecretDoorFeatureId, grid.GetFeatureId(request.Origin));
        Assert.Equal(DungeonCellStates.Room | DungeonCellStates.Icky, grid.GetCellFlags(request.Origin));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Execute_LargeRoomVariantTwoLockedDoorStaysDeferredUntilExecution()
    {
        var grid = RockGrid();
        var builderRandom = new ScriptedRandomSource(1, 2, 1, 1, 7, 1, 0, 1);
        var room = RoomGeometryBuilder.TryBuildLarge(grid, new RoomBlockPosition(0, 0), 1, builderRandom);
        var locked = Assert.Single(room.Attempts.Where(attempt => attempt.Kind == RoomContentAttemptKind.LockedDoor));

        Assert.Equal(RoomGeometryBuilder.InnerWallFeatureId, grid.GetFeatureId(locked.Origin));
        Assert.Equal(
            [(1, 26), (1, 6), (1, 5), (1, 5), (1, 8), (1, 4), (0, 100), (1, 4)],
            builderRandom.Requests);

        var executorRandom = new ScriptedRandomSource();
        var result = RoomDoorAttemptExecutor.Execute(grid, room.Attempts, executorRandom);

        Assert.Equal(new DoorState(DoorCondition.Locked, 7), grid.GetDoorState(locked.Origin));
        Assert.Contains(locked, result.ExecutedDoorAttempts);
        Assert.Empty(executorRandom.Requests);
    }

    [Fact]
    public void Execute_NoDoorAttemptsLeavesGridAndRngUnchanged()
    {
        var grid = RockGrid();
        var attempts = new[]
        {
            new RoomContentAttempt(RoomContentAttemptKind.Monster, new DungeonPosition(10, 10)),
            new RoomContentAttempt(RoomContentAttemptKind.Trap, new DungeonPosition(10, 11)),
            new RoomContentAttempt(RoomContentAttemptKind.Object, new DungeonPosition(10, 12)),
        };
        var random = new ScriptedRandomSource();

        var result = RoomDoorAttemptExecutor.Execute(grid, attempts, random);

        Assert.Empty(result.ExecutedDoorAttempts);
        Assert.Equal(attempts, result.RemainingAttempts);
        Assert.Equal(DungeonGrid.GraniteWallBasicFeatureId, grid.GetFeatureId(new DungeonPosition(10, 10)));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void Execute_IdenticalAttemptsAndRng_ReplaysExactly()
    {
        var attempts = new[]
        {
            new RoomContentAttempt(RoomContentAttemptKind.SecretDoor, new DungeonPosition(10, 10)),
            new RoomContentAttempt(RoomContentAttemptKind.LockedDoor, new DungeonPosition(10, 11)),
            new RoomContentAttempt(RoomContentAttemptKind.Trap, new DungeonPosition(10, 12)),
        };
        var firstGrid = RockGrid();
        var secondGrid = RockGrid();
        var first = RoomDoorAttemptExecutor.Execute(firstGrid, attempts, new ScriptedRandomSource(5));
        var second = RoomDoorAttemptExecutor.Execute(secondGrid, attempts, new ScriptedRandomSource(5));

        Assert.Equal(first.ExecutedDoorAttempts, second.ExecutedDoorAttempts);
        Assert.Equal(first.RemainingAttempts, second.RemainingAttempts);
        Assert.Equal(firstGrid.GetFeatureId(new DungeonPosition(10, 10)), secondGrid.GetFeatureId(new DungeonPosition(10, 10)));
        Assert.Equal(firstGrid.GetDoorState(new DungeonPosition(10, 11)), secondGrid.GetDoorState(new DungeonPosition(10, 11)));
        Assert.Equal(firstGrid.GetCellFlags(new DungeonPosition(10, 10)), secondGrid.GetCellFlags(new DungeonPosition(10, 10)));
    }

    private static DungeonGrid RockGrid()
    {
        var grid = new DungeonGrid();
        grid.InitializeRock();
        return grid;
    }

    private sealed class ScriptedRandomSource(params int[] values) : IRandomSource
    {
        private readonly Queue<int> _values = new(values);

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Requests.Add((minInclusive, maxExclusive));
            if (!_values.TryDequeue(out var value))
            {
                throw new InvalidOperationException("The scripted random source ran out of values.");
            }

            if (value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException($"Scripted value {value} is outside [{minInclusive}, {maxExclusive}).");
            }

            return value;
        }

        public int RollDice(int count, int sides) => throw new NotSupportedException();
    }
}
