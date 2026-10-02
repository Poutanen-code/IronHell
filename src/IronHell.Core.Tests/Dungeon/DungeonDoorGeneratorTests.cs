using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class DungeonDoorGeneratorTests
{
    [Theory]
    [InlineData(DoorCondition.Closed, 0)]
    [InlineData(DoorCondition.Locked, 1)]
    [InlineData(DoorCondition.Locked, 7)]
    [InlineData(DoorCondition.Stuck, 0)]
    [InlineData(DoorCondition.Stuck, 7)]
    public void DoorState_AcceptsLegalConditionAndPower(DoorCondition condition, int power)
    {
        var state = new DoorState(condition, power);

        Assert.Equal(condition, state.Condition);
        Assert.Equal(power, state.Power);
    }

    [Theory]
    [InlineData(DoorCondition.Closed, 1)]
    [InlineData(DoorCondition.Locked, 0)]
    [InlineData(DoorCondition.Locked, 8)]
    [InlineData(DoorCondition.Stuck, 8)]
    public void DoorState_RejectsIllegalConditionAndPower(DoorCondition condition, int power)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DoorState(condition, power));
    }

    [Fact]
    public void DoorState_RejectsUnknownCondition()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DoorState((DoorCondition)99, 0));
    }

    [Fact]
    public void DungeonGrid_AssociatesDoorStateAndClearsItWhenFeatureChanges()
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);
        var locked = new DoorState(DoorCondition.Locked, 4);

        grid.SetClosedDoor(position, locked);

        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(locked, grid.GetDoorState(position));

        grid.SetFeatureId(position, RoomGeometryBuilder.OpenFloorFeatureId);

        Assert.Null(grid.GetDoorState(position));
    }

    [Fact]
    public void SettingClosedDoorFeatureThroughGrid_DefaultsToOrdinaryClosedState()
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);

        grid.SetFeatureId(position, DungeonGrid.ClosedDoorFeatureId);

        Assert.Equal(new DoorState(DoorCondition.Closed, 0), grid.GetDoorState(position));
    }

    [Fact]
    public void PlaceRandomClosedDoor_OrdinaryBoundaryUsesOnlyConditionRoll()
    {
        var grid = new DungeonGrid();
        var random = new ScriptedRandomSource(299);
        var position = new DungeonPosition(10, 20);

        DungeonDoorGenerator.PlaceRandomClosedDoor(grid, position, random);

        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(DoorCondition.Closed, 0), grid.GetDoorState(position));
        Assert.Equal([(0, 400)], random.Requests);
    }

    [Theory]
    [InlineData(300, 1, DoorCondition.Locked, 1)]
    [InlineData(398, 7, DoorCondition.Locked, 7)]
    [InlineData(399, 0, DoorCondition.Stuck, 0)]
    [InlineData(399, 7, DoorCondition.Stuck, 7)]
    public void PlaceRandomClosedDoor_MapsConditionAndPower(
        int conditionRoll,
        int powerRoll,
        DoorCondition expectedCondition,
        int expectedPower)
    {
        var grid = new DungeonGrid();
        var random = new ScriptedRandomSource(conditionRoll, powerRoll);
        var position = new DungeonPosition(10, 20);

        DungeonDoorGenerator.PlaceRandomClosedDoor(grid, position, random);

        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(expectedCondition, expectedPower), grid.GetDoorState(position));
        Assert.Equal([(0, 400), expectedCondition == DoorCondition.Locked ? (1, 8) : (0, 8)], random.Requests);
    }

    [Fact]
    public void PlaceRandomClosedDoor_CanGenerateEveryLockedAndStuckPower()
    {
        for (var power = 1; power <= 7; power++)
        {
            var lockedGrid = new DungeonGrid();
            DungeonDoorGenerator.PlaceRandomClosedDoor(
                lockedGrid,
                new DungeonPosition(1, power),
                new ScriptedRandomSource(300, power));

            Assert.Equal(new DoorState(DoorCondition.Locked, power), lockedGrid.GetDoorState(new DungeonPosition(1, power)));
        }

        for (var power = 0; power <= 7; power++)
        {
            var stuckGrid = new DungeonGrid();
            DungeonDoorGenerator.PlaceRandomClosedDoor(
                stuckGrid,
                new DungeonPosition(1, power),
                new ScriptedRandomSource(399, power));

            Assert.Equal(new DoorState(DoorCondition.Stuck, power), stuckGrid.GetDoorState(new DungeonPosition(1, power)));
        }
    }

    [Fact]
    public void TryDiscoverSecretDoor_ConvertsSecretDoorToRandomClosedDoorState()
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);
        var random = new ScriptedRandomSource(399, 7);

        grid.SetFeatureId(position, DungeonGrid.SecretDoorFeatureId);

        var discovered = DungeonDoorGenerator.TryDiscoverSecretDoor(grid, position, random);

        Assert.True(discovered);
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(DoorCondition.Stuck, 7), grid.GetDoorState(position));
        Assert.Equal([(0, 400), (0, 8)], random.Requests);
    }

    [Theory]
    [InlineData(0, DoorCondition.Closed, 0)]
    [InlineData(299, DoorCondition.Closed, 0)]
    [InlineData(300, DoorCondition.Locked, 1)]
    [InlineData(398, DoorCondition.Locked, 7)]
    [InlineData(399, DoorCondition.Stuck, 0)]
    [InlineData(399, DoorCondition.Stuck, 7)]
    public void TryDiscoverSecretDoor_UsesOrdinaryClosedDoorDistributionBoundaries(int conditionRoll, DoorCondition expectedCondition, int expectedPower)
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);
        var random = expectedCondition == DoorCondition.Closed
            ? new ScriptedRandomSource(conditionRoll)
            : new ScriptedRandomSource(conditionRoll, expectedPower);

        grid.SetFeatureId(position, DungeonGrid.SecretDoorFeatureId);

        var discovered = DungeonDoorGenerator.TryDiscoverSecretDoor(grid, position, random);

        Assert.True(discovered);
        Assert.Equal(DungeonGrid.ClosedDoorFeatureId, grid.GetFeatureId(position));
        Assert.Equal(new DoorState(expectedCondition, expectedPower), grid.GetDoorState(position));

        if (expectedCondition == DoorCondition.Closed)
        {
            Assert.Equal([(0, 400)], random.Requests);
        }
        else
        {
            Assert.Equal([(0, 400), expectedCondition == DoorCondition.Locked ? (1, 8) : (0, 8)], random.Requests);
        }
    }

    [Theory]
    [InlineData(RoomGeometryBuilder.OpenFloorFeatureId)]
    [InlineData(DungeonGrid.GraniteWallBasicFeatureId)]
    [InlineData(DungeonGrid.OpenDoorFeatureId)]
    [InlineData(DungeonGrid.ClosedDoorFeatureId)]
    public void TryDiscoverSecretDoor_IgnoresNonSecretFeaturesWithoutMutatingState(string featureId)
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);
        var random = new ScriptedRandomSource();

        grid.SetFeatureId(position, featureId);

        var discovered = DungeonDoorGenerator.TryDiscoverSecretDoor(grid, position, random);

        Assert.False(discovered);
        Assert.Equal(featureId, grid.GetFeatureId(position));
        Assert.Empty(random.Requests);
    }

    [Fact]
    public void PlaceRandomClosedDoor_ReplaysIdenticallyForSeed()
    {
        const int seed = 731942;
        var firstGrid = GenerateGrid(seed);
        var secondGrid = GenerateGrid(seed);
        var firstStates = ReadDoorStates(firstGrid);
        var secondStates = ReadDoorStates(secondGrid);

        Assert.True(firstStates.SequenceEqual(secondStates), $"Seed: {seed}");
        Assert.All(firstStates, cell => Assert.Equal(DungeonGrid.ClosedDoorFeatureId, cell.FeatureId));
    }

    private static DungeonGrid GenerateGrid(int seed)
    {
        var grid = new DungeonGrid();
        var random = new SeededRandomSource(seed);

        for (var row = 0; row < 10; row++)
        {
            for (var column = 0; column < 10; column++)
            {
                DungeonDoorGenerator.PlaceRandomClosedDoor(grid, new DungeonPosition(row, column), random);
            }
        }

        return grid;
    }

    private static (string? FeatureId, DoorState? State)[] ReadDoorStates(DungeonGrid grid) =>
        Enumerable.Range(0, 100)
            .Select(index => new DungeonPosition(index / 10, index % 10))
            .Select(position => (grid.GetFeatureId(position), grid.GetDoorState(position)))
            .ToArray();

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