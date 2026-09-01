namespace IronHell.Core.Characters;

public sealed class CharacterState
{
    public CharacterAttributes Attributes { get; } = new();

    public int Level { get; set; } = 1;

    public long Experience { get; set; }

    public int CurrentHp { get; set; }

    public int MaxHp { get; set; }

    public int CurrentMana { get; set; }

    public int MaxMana { get; set; }

    public int CurrentFood { get; set; } = 9999;

    public int Gold { get; set; }

    public bool IsDead { get; set; }
}