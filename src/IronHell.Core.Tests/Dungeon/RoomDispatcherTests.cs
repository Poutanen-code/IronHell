using IronHell.Core.Dungeon;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class RoomDispatcherTests
{
    [Fact]
    public void RoomDispatcher_ProcessesExactly50Attempts()
    {
        var grid = FullyReservedGrid();
        var random = new ScriptedRandomSource(defaultValue: 199);

        var result = RoomDispatcher.Generate(grid, 1, [], random);

        Assert.Equal(50, result.AttemptCount);
        Assert.Equal(0, result.SuccessfulRoomCount);
        Assert.Equal(150, random.Requests.Count);
    }

    [Fact]
    public void RoomDispatcher_BlockSelectionUsesVerifiedRangesAndOrder()
    {
        var grid = FullyReservedGrid();
        var random = new ScriptedRandomSource(defaultValue: 199);

        _ = RoomDispatcher.Generate(grid, 1, [], random);

        Assert.Equal([(0, 6), (0, 18), (0, 200)], random.Requests.Take(3));
        Assert.Equal([(0, 6), (0, 18), (0, 200)], random.Requests.Skip(147).Take(3));
    }

    [Fact]
    public void RoomDispatcher_UnusualRollIsConditionalAndVeryUnusualIsIndependent()
    {
        var grid = FullyReservedGrid();
        var random = WithDefault(199, 0, 0, 0, 199);

        _ = RoomDispatcher.Generate(grid, 1, [], random);

        Assert.Equal([(0, 6), (0, 18), (0, 200), (0, 100), (0, 200)], random.Requests.Take(5));
    }

    [Fact]
    public void RoomDispatcher_NormalFallbackUsesExistingSimpleBuilder()
    {
        var result = RoomDispatcher.Generate(
            new DungeonGrid(),
            1,
            [],
            WithDefault(0, 0, 0, 199));

        Assert.Equal(50, result.AttemptCount);
        Assert.NotEmpty(result.SuccessfulRooms);
        Assert.Contains(result.SuccessfulRooms, room => room.Family == RoomFamily.Simple);
    }

    [Fact]
    public void RoomDispatcher_NoEligibleVaultRemainsFailedWithoutFallbackForThatBranch()
    {
        var grid = FullyReservedGrid();
        var random = WithDefault(199, 0, 0, 0, 0);

        var result = RoomDispatcher.Generate(grid, 10, [], random);

        Assert.Equal(50, result.AttemptCount);
        Assert.Empty(result.SuccessfulRooms);
    }

    [Fact]
    public void RoomDispatcher_SuccessfulCentersRemainInBuildOrder()
    {
        var grid = new DungeonGrid();
        var result = RoomDispatcher.Generate(grid, 1, [], new ScriptedRandomSource(defaultValue: 199));

        Assert.Equal(result.SuccessfulRooms.Select(room => room.Center), grid.RoomCenters.Select(center => (DungeonPosition?)center));
    }

    private static DungeonGrid FullyReservedGrid()
    {
        var grid = new DungeonGrid();
        for (var row = 0; row < DungeonGrid.RoomBlockRows; row++)
        {
            for (var column = 0; column < DungeonGrid.RoomBlockColumns; column++)
            {
                Assert.True(grid.TryCommitRoom(new RoomBlockPosition(row, column), new RoomBlockFootprint(1, 1), new DungeonPosition(row * 11, column * 11)));
            }
        }

        return grid;
    }

    private static ScriptedRandomSource WithDefault(int defaultValue, params int[] values) =>
        new(values, defaultValue);

    private sealed class ScriptedRandomSource : IRandomSource
    {
        private readonly Queue<int> _values;
        private readonly int _defaultValue;

        public ScriptedRandomSource(params int[] values)
            : this(values, 0)
        {
        }

        public ScriptedRandomSource(int defaultValue, params int[] values)
            : this(values, defaultValue)
        {
        }

        internal ScriptedRandomSource(IEnumerable<int> values, int defaultValue)
        {
            _values = new Queue<int>(values);
            _defaultValue = defaultValue;
        }

        public List<(int MinInclusive, int MaxExclusive)> Requests { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Requests.Add((minInclusive, maxExclusive));
            var value = _values.Count == 0 ? _defaultValue : _values.Dequeue();
            return Math.Clamp(value, minInclusive, maxExclusive - 1);
        }

        public int RollDice(int count, int sides) => Enumerable.Range(0, count).Sum(_ => Next(1, sides + 1));
    }
}