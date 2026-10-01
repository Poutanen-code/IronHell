using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public sealed record MonsterPlacementEligibilityContext(
    bool UniqueHasRemainingCapacity = true);

public enum MonsterPlacementRejectionReason
{
    None,
    UniqueUnavailable,
}

public sealed record MonsterPlacementEligibilityResult(
    bool IsEligible,
    MonsterPlacementRejectionReason RejectionReason);

public static class MonsterPlacementEligibility
{
    public static MonsterPlacementEligibilityResult Evaluate(
        MonsterDefinition selectedDefinition,
        MonsterPlacementEligibilityContext context)
    {
        ArgumentNullException.ThrowIfNull(selectedDefinition);
        ArgumentNullException.ThrowIfNull(context);

        if (selectedDefinition.SpawnPolicy.Unique && !context.UniqueHasRemainingCapacity)
        {
            return new MonsterPlacementEligibilityResult(
                IsEligible: false,
                MonsterPlacementRejectionReason.UniqueUnavailable);
        }

        return new MonsterPlacementEligibilityResult(
            IsEligible: true,
            MonsterPlacementRejectionReason.None);
    }
}
