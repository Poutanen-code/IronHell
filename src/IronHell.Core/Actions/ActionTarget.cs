using IronHell.Core.Characters;

namespace IronHell.Core.Actions;

public sealed record ActionTarget
{
    private ActionTarget(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);
        Character = character;
    }

    public Character Character { get; }

    public static ActionTarget Self(Character source) => new(source);

    public static ActionTarget CharacterTarget(Character target) => new(target);
}