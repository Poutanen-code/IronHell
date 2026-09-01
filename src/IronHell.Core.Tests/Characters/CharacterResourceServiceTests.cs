using IronHell.Core.Characters;
using Xunit;

namespace IronHell.Core.Tests.Characters;

public sealed class CharacterResourceServiceTests
{
    [Fact]
    public void Modify_HitPoints_IncreasesAndClampsToMaximum()
    {
        var character = CreateCharacter(currentHp: 8, maxHp: 10);

        var result = CharacterResourceService.Modify(character, CharacterResourceType.HitPoints, 5);

        Assert.True(result.Success);
        Assert.Equal(8, result.BeforeValue);
        Assert.Equal(10, result.AfterValue);
    }

    [Fact]
    public void Modify_HitPoints_DecreasesToZeroAndMarksDeath()
    {
        var character = CreateCharacter(currentHp: 5, maxHp: 10);

        var result = CharacterResourceService.Modify(character, CharacterResourceType.HitPoints, -8);

        Assert.True(result.Success);
        Assert.Equal(0, result.AfterValue);
        Assert.True(character.State.IsDead);
    }

    [Fact]
    public void Modify_Mana_ClampsBetweenZeroAndMaximum()
    {
        var character = CreateCharacter();
        character.State.CurrentMana = 3;
        character.State.MaxMana = 10;

        var spent = CharacterResourceService.Modify(character, CharacterResourceType.Mana, -5);
        var restored = CharacterResourceService.Modify(character, CharacterResourceType.Mana, 20);

        Assert.Equal(0, spent.AfterValue);
        Assert.Equal(10, restored.AfterValue);
    }

    [Fact]
    public void Modify_FoodAndGold_DoNotDropBelowZero()
    {
        var character = CreateCharacter();
        character.State.CurrentFood = 3;
        character.State.Gold = 4;

        var food = CharacterResourceService.Modify(character, CharacterResourceType.Food, -5);
        var gold = CharacterResourceService.Modify(character, CharacterResourceType.Gold, -8);

        Assert.Equal(0, food.AfterValue);
        Assert.Equal(0, gold.AfterValue);
    }

    [Fact]
    public void Modify_DeadCharacterHitPointIncrease_ReturnsFailureWithoutMutation()
    {
        var character = CreateCharacter(currentHp: 0, maxHp: 10);
        character.State.IsDead = true;

        var result = CharacterResourceService.Modify(character, CharacterResourceType.HitPoints, 3);

        Assert.False(result.Success);
        Assert.Equal(0, character.State.CurrentHp);
    }

    [Fact]
    public void Modify_IdenticalInputs_ProduceIdenticalResults()
    {
        var first = CreateCharacter(currentHp: 7, maxHp: 10);
        var second = CreateCharacter(currentHp: 7, maxHp: 10);

        var firstResult = CharacterResourceService.Modify(first, CharacterResourceType.HitPoints, -3);
        var secondResult = CharacterResourceService.Modify(second, CharacterResourceType.HitPoints, -3);

        Assert.Equal(firstResult, secondResult);
        Assert.Equal(first.State.IsDead, second.State.IsDead);
    }

    private static Character CreateCharacter(int currentHp = 10, int maxHp = 10)
    {
        var character = new Character("character-1", "human", "warrior");
        character.State.CurrentHp = currentHp;
        character.State.MaxHp = maxHp;
        return character;
    }
}