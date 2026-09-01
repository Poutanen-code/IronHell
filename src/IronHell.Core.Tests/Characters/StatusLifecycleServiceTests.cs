using IronHell.Core.Characters;
using Xunit;

namespace IronHell.Core.Tests.Characters;

public sealed class StatusLifecycleServiceTests
{
    [Fact]
    public void AdvanceStatuses_SingleStatus_DecrementsDuration()
    {
        var character = CreateCharacter(new ActiveStatus("blessed", 2));

        var result = StatusLifecycleService.AdvanceStatuses(character);

        Assert.Equal(["blessed"], result.AdvancedStatusIds);
        Assert.Empty(result.ExpiredStatusIds);
        Assert.Equal(1, Assert.Single(character.ActiveStatuses).RemainingDuration);
    }

    [Fact]
    public void AdvanceStatuses_MultipleStatuses_UpdatesIndependentlyInApplicationOrder()
    {
        var character = CreateCharacter(new ActiveStatus("blessed", 2), new ActiveStatus("poisoned", 1));

        var result = StatusLifecycleService.AdvanceStatuses(character);

        Assert.Equal(["blessed", "poisoned"], result.AdvancedStatusIds);
        Assert.Equal(["poisoned"], result.ExpiredStatusIds);
        Assert.Equal([new ActiveStatus("blessed", 1)], character.ActiveStatuses, ActiveStatusComparer.Instance);
    }

    [Fact]
    public void AdvanceStatuses_ExpiredStatus_IsRemoved()
    {
        var character = CreateCharacter(new ActiveStatus("blessed", 1));

        var result = StatusLifecycleService.AdvanceStatuses(character);

        Assert.Equal(["blessed"], result.ExpiredStatusIds);
        Assert.Empty(character.ActiveStatuses);
    }

    [Fact]
    public void AdvanceStatuses_RepeatedCalls_ExpireAtExpectedTick()
    {
        var character = CreateCharacter(new ActiveStatus("blessed", 2));

        StatusLifecycleService.AdvanceStatuses(character);
        var result = StatusLifecycleService.AdvanceStatuses(character);

        Assert.Equal(["blessed"], result.ExpiredStatusIds);
        Assert.Empty(character.ActiveStatuses);
    }

    [Fact]
    public void AdvanceStatuses_EmptyCollection_ReturnsEmptyResult()
    {
        var result = StatusLifecycleService.AdvanceStatuses(CreateCharacter());

        Assert.Empty(result.AdvancedStatusIds);
        Assert.Empty(result.ExpiredStatusIds);
    }

    private static Character CreateCharacter(params ActiveStatus[] statuses)
    {
        var character = new Character("character-1", "human", "warrior");
        foreach (var status in statuses)
        {
            character.ApplyStatus(status);
        }

        return character;
    }

    private sealed class ActiveStatusComparer : IEqualityComparer<ActiveStatus>
    {
        public static ActiveStatusComparer Instance { get; } = new();

        public bool Equals(ActiveStatus? left, ActiveStatus? right) =>
            left?.StatusId == right?.StatusId && left?.RemainingDuration == right?.RemainingDuration;

        public int GetHashCode(ActiveStatus status) => HashCode.Combine(status.StatusId, status.RemainingDuration);
    }
}