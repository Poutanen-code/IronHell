namespace IronHell.Core.Definitions;

/// <summary>Eight-skill block matching the JSON skill field names.</summary>
public readonly record struct SkillSet(
    int Disarming,
    int MagicDevice,
    int SavingThrow,
    int Stealth,
    int Searching,
    int SearchFrequency,
    int MeleeToHit,
    int RangedToHit)
{
    public static SkillSet operator +(SkillSet a, SkillSet b) => new(
        a.Disarming       + b.Disarming,
        a.MagicDevice     + b.MagicDevice,
        a.SavingThrow     + b.SavingThrow,
        a.Stealth         + b.Stealth,
        a.Searching       + b.Searching,
        a.SearchFrequency + b.SearchFrequency,
        a.MeleeToHit      + b.MeleeToHit,
        a.RangedToHit     + b.RangedToHit);
}
