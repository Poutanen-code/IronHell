using IronHell.Core.Dungeon;
using Xunit;

namespace IronHell.Core.Tests.Dungeon;

public sealed class DungeonGridTests
{
    [Fact]
    public void DungeonGrid_UsesVerifiedDimensions()
    {
        Assert.Equal(198, DungeonGrid.Width);
        Assert.Equal(66, DungeonGrid.Height);
        Assert.Equal(18, DungeonGrid.RoomBlockColumns);
        Assert.Equal(6, DungeonGrid.RoomBlockRows);
    }

    [Fact]
    public void RoomReservation_UsesElevenByElevenBlocks()
    {
        Assert.True(DungeonGrid.IsInBounds(new DungeonPosition(0, 0)));
        Assert.True(DungeonGrid.IsInBounds(new DungeonPosition(10, 10)));
        Assert.True(DungeonGrid.IsInBounds(new DungeonPosition(55, 197)));
        Assert.False(DungeonGrid.IsInBounds(new DungeonPosition(66, 0)));
        Assert.False(DungeonGrid.IsInBounds(new DungeonPosition(0, 198)));
    }

    [Fact]
    public void TryReserve_InBoundsFreeFootprint_ReservesEveryBlock()
    {
        var grid = new DungeonGrid();

        var reserved = grid.TryCommitRoom(
            new RoomBlockPosition(1, 2),
            new RoomBlockFootprint(2, 3),
            new DungeonPosition(16, 27));

        Assert.True(reserved);
        for (var row = 1; row < 3; row++)
        {
            for (var column = 2; column < 5; column++)
            {
                Assert.True(grid.IsBlockReserved(new RoomBlockPosition(row, column)));
            }
        }
    }

    [Fact]
    public void TryReserve_OutOfBoundsFootprint_FailsWithoutMutation()
    {
        var grid = new DungeonGrid();
        var footprint = new RoomBlockFootprint(1, 3);

        Assert.False(grid.IsRoomFootprintAvailable(new RoomBlockPosition(0, 16), footprint));
        Assert.False(grid.TryCommitRoom(new RoomBlockPosition(0, 16), footprint, new DungeonPosition(5, 170)));
        Assert.False(grid.IsBlockReserved(new RoomBlockPosition(0, 16)));
        Assert.Empty(grid.RoomCenters);
    }

    [Fact]
    public void TryReserve_ExactOverlap_FailsWithoutMutation()
    {
        var grid = new DungeonGrid();
        var footprint = new RoomBlockFootprint(2, 3);
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(1, 1), footprint, new DungeonPosition(16, 16)));

        Assert.False(grid.TryCommitRoom(new RoomBlockPosition(1, 1), footprint, new DungeonPosition(27, 27)));
        Assert.Single(grid.RoomCenters);
    }

    [Fact]
    public void TryReserve_PartialOverlap_FailsWithoutMutation()
    {
        var grid = new DungeonGrid();
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(1, 1), new RoomBlockFootprint(1, 3), new DungeonPosition(5, 16)));

        Assert.False(grid.TryCommitRoom(new RoomBlockPosition(1, 3), new RoomBlockFootprint(1, 3), new DungeonPosition(5, 38)));
        Assert.False(grid.IsBlockReserved(new RoomBlockPosition(1, 4)));
        Assert.Single(grid.RoomCenters);
    }

    [Fact]
    public void TryReserve_NonOverlappingFootprints_Succeed()
    {
        var grid = new DungeonGrid();

        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(0, 0), new RoomBlockFootprint(1, 3), new DungeonPosition(5, 16)));
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(2, 4), new RoomBlockFootprint(2, 3), new DungeonPosition(27, 49)));
        Assert.Equal(2, grid.RoomCenters.Count);
    }

    [Fact]
    public void DungeonCell_RoomFlagCanBeSetAndRead()
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);

        grid.AddCellFlags(position, DungeonCellStates.Room);

        Assert.Equal(DungeonCellStates.Room, grid.GetCellFlags(position));
    }

    [Fact]
    public void DungeonCell_IckyFlagComposesWithRoomFlag()
    {
        var grid = new DungeonGrid();
        var position = new DungeonPosition(10, 20);

        grid.AddCellFlags(position, DungeonCellStates.Room);
        grid.AddCellFlags(position, DungeonCellStates.Icky);

        Assert.Equal(DungeonCellStates.Room | DungeonCellStates.Icky, grid.GetCellFlags(position));
    }

    [Fact]
    public void DungeonCells_HaveNoRoomFlagsByDefault()
    {
        var grid = new DungeonGrid();

        Assert.Equal(DungeonCellStates.None, grid.GetCellFlags(new DungeonPosition(0, 0)));
    }

    [Fact]
    public void SuccessfulRoomCommit_RecordsCenter()
    {
        var grid = new DungeonGrid();
        var center = new DungeonPosition(16, 27);

        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(1, 2), new RoomBlockFootprint(2, 3), center));
        Assert.Equal([center], grid.RoomCenters);
    }

    [Fact]
    public void FailedRoomCommit_DoesNotRecordCenter()
    {
        var grid = new DungeonGrid();
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(1, 1), new RoomBlockFootprint(1, 1), new DungeonPosition(5, 5)));

        Assert.False(grid.TryCommitRoom(new RoomBlockPosition(1, 1), new RoomBlockFootprint(1, 1), new DungeonPosition(16, 16)));
        Assert.Single(grid.RoomCenters);
    }

    [Fact]
    public void RoomCenters_PreserveInsertionOrder()
    {
        var grid = new DungeonGrid();
        var first = new DungeonPosition(5, 5);
        var second = new DungeonPosition(27, 49);

        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(0, 0), new RoomBlockFootprint(1, 1), first));
        Assert.True(grid.TryCommitRoom(new RoomBlockPosition(2, 4), new RoomBlockFootprint(1, 1), second));

        Assert.Equal([first, second], grid.RoomCenters);
    }
}
