using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using Xunit;

namespace IronHell.Core.Tests.Characters;

public sealed class StatusApplicationServiceTests
{
    [Fact]
    public void ApplyStatus_FirstApplication_AddsStatus()
    {
        var character = CreateCharacter();

        var result = StatusApplicationService.ApplyStatus(character, "blessed", 10, StatusApplicationPolicy.ReplaceExisting);

        Assert.True(result.Applied);
        Assert.False(result.Replaced);
        AssertStatus(character, "blessed", 10);
    }

    [Fact]
    public void ApplyStatus_IgnoreIfPresent_PreservesExistingDuration()
    {
        var character = CreateCharacter(new ActiveStatus("blessed", 5));

        var result = StatusApplicationService.ApplyStatus(character, "blessed", 10, StatusApplicationPolicy.IgnoreIfPresent);

        Assert.False(result.Applied);
        Assert.False(result.Replaced);
        AssertStatus(character, "blessed", 5);
    }

    [Fact]
    public void ApplyStatus_RefreshDuration_UpdatesExistingDuration()
    {
        var character = CreateCharacter(new ActiveStatus("blessed", 5));

        var result = StatusApplicationService.ApplyStatus(character, "blessed", 10, StatusApplicationPolicy.RefreshDuration);

        Assert.True(result.Applied);
        Assert.False(result.Replaced);
        AssertStatus(character, "blessed", 10);
    }

    [Fact]
    public void ApplyStatus_ReplaceExisting_ReplacesRuntimeInstance()
    {
        var original = new ActiveStatus("blessed", 5);
        var character = CreateCharacter(original);

        var result = StatusApplicationService.ApplyStatus(character, "blessed", 10, StatusApplicationPolicy.ReplaceExisting);

        Assert.True(result.Applied);
        Assert.True(result.Replaced);
        Assert.NotSame(original, result.ActiveStatus);
        AssertStatus(character, "blessed", 10);
    }

    [Fact]
    public void ApplyStatus_IndependentStatuses_AreRetained()
    {
        var character = CreateCharacter(new ActiveStatus("blessed", 5));

        StatusApplicationService.ApplyStatus(character, "poisoned", 10, StatusApplicationPolicy.ReplaceExisting);

        Assert.Equal(["blessed", "poisoned"], character.ActiveStatuses.Select(status => status.StatusId));
    }

    [Fact]
    public void ApplyStatus_IdenticalInputs_ProduceIdenticalResults()
    {
        var first = CreateCharacter(new ActiveStatus("blessed", 5));
        var second = CreateCharacter(new ActiveStatus("blessed", 5));

        var firstResult = StatusApplicationService.ApplyStatus(first, "blessed", 10, StatusApplicationPolicy.RefreshDuration);
        var secondResult = StatusApplicationService.ApplyStatus(second, "blessed", 10, StatusApplicationPolicy.RefreshDuration);

        Assert.Equal(firstResult.Applied, secondResult.Applied);
        Assert.Equal(firstResult.Replaced, secondResult.Replaced);
        AssertStatus(first, "blessed", 10);
        AssertStatus(second, "blessed", 10);
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

    private static void AssertStatus(Character character, string statusId, int duration)
    {
        var status = Assert.Single(character.ActiveStatuses);
        Assert.Equal(statusId, status.StatusId);
        Assert.Equal(duration, status.RemainingDuration);
    }
}