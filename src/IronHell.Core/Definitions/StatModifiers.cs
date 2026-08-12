namespace IronHell.Core.Definitions;

/// <summary>Six-stat modifier block, keyed to the Angband stat order STR/INT/WIS/DEX/CON/CHR.</summary>
public readonly record struct StatModifiers(
    int Strength,
    int Intelligence,
    int Wisdom,
    int Dexterity,
    int Constitution,
    int Charisma)
{
    public static StatModifiers operator +(StatModifiers a, StatModifiers b) => new(
        a.Strength     + b.Strength,
        a.Intelligence + b.Intelligence,
        a.Wisdom       + b.Wisdom,
        a.Dexterity    + b.Dexterity,
        a.Constitution + b.Constitution,
        a.Charisma     + b.Charisma);
}
