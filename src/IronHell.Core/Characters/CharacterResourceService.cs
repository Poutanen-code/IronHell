namespace IronHell.Core.Characters;

public static class CharacterResourceService
{
    public static ResourceModificationResult Modify(
        Character character,
        CharacterResourceType resourceType,
        int delta)
    {
        ArgumentNullException.ThrowIfNull(character);

        return resourceType switch
        {
            CharacterResourceType.HitPoints => ModifyHitPoints(character, delta),
            CharacterResourceType.Mana => ModifyBounded(resourceType, character.State.CurrentMana, character.State.MaxMana, delta, value => character.State.CurrentMana = value),
            CharacterResourceType.Food => ModifyBounded(resourceType, character.State.CurrentFood, null, delta, value => character.State.CurrentFood = value),
            CharacterResourceType.Gold => ModifyBounded(resourceType, character.State.Gold, null, delta, value => character.State.Gold = value),
            _ => new ResourceModificationResult(resourceType, 0, 0, false, "Unknown resource type."),
        };
    }

    private static ResourceModificationResult ModifyHitPoints(Character character, int delta)
    {
        var state = character.State;
        var before = state.CurrentHp;
        if (state.IsDead && delta > 0)
        {
            return new ResourceModificationResult(CharacterResourceType.HitPoints, before, before, false, "Cannot restore hit points to a dead character.");
        }

        var after = Clamp(before, state.MaxHp, delta);
        state.CurrentHp = after;
        if (after == 0)
        {
            state.IsDead = true;
        }

        return new ResourceModificationResult(CharacterResourceType.HitPoints, before, after, true, null);
    }

    private static ResourceModificationResult ModifyBounded(
        CharacterResourceType resourceType,
        int before,
        int? maximum,
        int delta,
        Action<int> assign)
    {
        var after = Clamp(before, maximum, delta);
        assign(after);
        return new ResourceModificationResult(resourceType, before, after, true, null);
    }

    private static int Clamp(int current, int? maximum, int delta)
    {
        var value = (long)current + delta;
        var lowerBounded = Math.Max(0, value);
        return maximum is null
            ? (int)Math.Min(lowerBounded, int.MaxValue)
            : (int)Math.Min(lowerBounded, maximum.Value);
    }
}