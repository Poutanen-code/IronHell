using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class DungeonDoorInteractionTests
{
    [Fact]
    public void Open_ClosedDoor_OpensWithoutConsumingRandomness()
    {
        var (grid, position) = CreateDoor(DoorCondition.Closed, 0);
        var random = new ScriptedRandomSource();

        var result = DungeonDoorInteractionService.Open(grid, position, skillDis: 20, random);

        Assert.Equal(DoorInteractionResult.Opened, result);
        Assert.Equal(DungeonDoorInteractionService.OpenDoorFeatureId, grid.GetFeatureId(position));
        Assert.Null(grid.GetDoorState(position));
        Assert.Empty(random.Requests);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Open_StuckDoor_DoesNotOpenOrConsumeRandomness(int power)
    {
        var (grid, position) = CreateDoor(DoorCondition.Stuck, power);
        var random = new ScriptedRandomSource();

        var result = DungeonDoorInteractionService.Open(grid, position, skillDis: 20, random);

        Assert.Equal(DoorInteractionResult.Stuck, result);
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(DoorCondition.Stuck, power), grid.GetDoorState(position));
        Assert.Empty(random.Requests);
    }

    [Theory]
    [InlineData(1, 20, 15, DoorInteractionResult.Opened)]
    [InlineData(1, 20, 16, DoorInteractionResult.LockpickFailed)]
    [InlineData(7, 29, 1, DoorInteractionResult.Opened)]
    [InlineData(7, 29, 2, DoorInteractionResult.LockpickFailed)]
    public void Open_LockedDoor_UsesPowerAndTwoPercentFloor(
        int power,
        int skillDis,
        int roll,
        DoorInteractionResult expectedResult)
    {
        var (grid, position) = CreateDoor(DoorCondition.Locked, power);
        var random = new ScriptedRandomSource(roll);

        var result = DungeonDoorInteractionService.Open(grid, position, skillDis, random);

        Assert.Equal(expectedResult, result);
        Assert.Equal((0, 100), Assert.Single(random.Requests));
        if (expectedResult == DoorInteractionResult.Opened)
        {
            Assert.Equal(DungeonDoorInteractionService.OpenDoorFeatureId, grid.GetFeatureId(position));
            Assert.Null(grid.GetDoorState(position));
        }
        else
        {
            Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
            Assert.Equal(new DoorState(DoorCondition.Locked, power), grid.GetDoorState(position));
        }
    }

    [Theory]
    [InlineData(DoorCondition.Closed, 0, DoorCondition.Stuck, 1)]
    [InlineData(DoorCondition.Locked, 1, DoorCondition.Stuck, 2)]
    [InlineData(DoorCondition.Locked, 7, DoorCondition.Stuck, 7)]
    [InlineData(DoorCondition.Stuck, 0, DoorCondition.Stuck, 1)]
    [InlineData(DoorCondition.Stuck, 7, DoorCondition.Stuck, 7)]
    public void Spike_TransitionsDoorToStuckWithPowerCappedAtSeven(
        DoorCondition initialCondition,
        int initialPower,
        DoorCondition expectedCondition,
        int expectedPower)
    {
        var (grid, position) = CreateDoor(initialCondition, initialPower);

        var result = DungeonDoorInteractionService.Spike(grid, position);

        Assert.Equal(DoorInteractionResult.Spiked, result);
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(expectedCondition, expectedPower), grid.GetDoorState(position));
    }

    [Fact]
    public void Bash_UsesDoorPowerAndCanBreakDoorDeterministically()
    {
        var (grid, position) = CreateDoor(DoorCondition.Locked, 3);
        var random = new ScriptedRandomSource(19, 49);

        var result = DungeonDoorInteractionService.Bash(grid, position, bashPower: 50, random);

        Assert.Equal(DoorInteractionResult.BashedBroken, result);
        Assert.Equal([(0, 100), (0, 100)], random.Requests);
        Assert.Equal(DungeonDoorInteractionService.BrokenDoorFeatureId, grid.GetFeatureId(position));
        Assert.Null(grid.GetDoorState(position));
    }

    [Fact]
    public void Bash_StuckDoorUsesPowerAndCanOpenDeterministically()
    {
        var (grid, position) = CreateDoor(DoorCondition.Stuck, 7);
        var random = new ScriptedRandomSource(9, 50);

        var result = DungeonDoorInteractionService.Bash(grid, position, bashPower: 80, random);

        Assert.Equal(DoorInteractionResult.BashedOpen, result);
        Assert.Equal([(0, 100), (0, 100)], random.Requests);
        Assert.Equal(DungeonDoorInteractionService.OpenDoorFeatureId, grid.GetFeatureId(position));
        Assert.Null(grid.GetDoorState(position));
    }

    [Fact]
    public void Bash_FailsAtDoorPowerThresholdAndPreservesState()
    {
        var (grid, position) = CreateDoor(DoorCondition.Locked, 3);
        var initialState = grid.GetDoorState(position);
        var random = new ScriptedRandomSource(20);

        var result = DungeonDoorInteractionService.Bash(grid, position, bashPower: 50, random);

        Assert.Equal(DoorInteractionResult.BashFailed, result);
        Assert.Equal((0, 100), Assert.Single(random.Requests));
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(initialState, grid.GetDoorState(position));
    }

    [Fact]
    public void Close_OpenDoor_RestoresOrdinaryClosedDoorState()
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);
        grid.SetFeatureId(position, DungeonDoorInteractionService.OpenDoorFeatureId);

        var result = DungeonDoorInteractionService.Close(grid, position);

        Assert.Equal(DoorInteractionResult.Closed, result);
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(DoorCondition.Closed, 0), grid.GetDoorState(position));
    }

    [Fact]
    public void Close_BrokenDoor_LeavesItBroken()
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);
        grid.SetFeatureId(position, DungeonDoorInteractionService.BrokenDoorFeatureId);

        var result = DungeonDoorInteractionService.Close(grid, position);

        Assert.Equal(DoorInteractionResult.BrokenCannotClose, result);
        Assert.Equal(DungeonDoorInteractionService.BrokenDoorFeatureId, grid.GetFeatureId(position));
        Assert.Null(grid.GetDoorState(position));
    }

    private static (DungeonGrid Grid, DungeonPosition Position) CreateDoor(DoorCondition condition, int power)
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);
        grid.SetClosedDoor(position, new DoorState(condition, power));
        return (grid, position);
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
