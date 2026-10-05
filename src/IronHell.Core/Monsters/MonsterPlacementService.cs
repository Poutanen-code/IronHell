using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public enum MonsterPlacementFailureReason
{
    None,
    OutOfBounds,
    IllegalCell,
    Occupied,
    UniqueUnavailable,
    ForceDepth,
}

public sealed record MonsterPlacementResult(
    bool Success,
    MonsterRuntimeInstance? Monster,
    MonsterPlacementFailureReason FailureReason);

public static class MonsterPlacementService
{
    public static MonsterPlacementResult Place(
        MonsterRuntimeState state,
        MonsterDefinition definition,
        MonsterPosition position,
        MonsterPlacementSpace? space = null,
        int? depth = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(definition);

        if (space is not null && !space.IsInBounds(position))
        {
            return new MonsterPlacementResult(
                Success: false,
                Monster: null,
                MonsterPlacementFailureReason.OutOfBounds);
        }

        if (space is not null && !space.IsMonsterPlacementLegal(position))
        {
            return new MonsterPlacementResult(
                Success: false,
                Monster: null,
                MonsterPlacementFailureReason.IllegalCell);
        }

        if (state.IsOccupied(position))
        {
            return new MonsterPlacementResult(
                Success: false,
                Monster: null,
                MonsterPlacementFailureReason.Occupied);
        }

        var eligibility = MonsterPlacementEligibility.Evaluate(
            definition,
            new MonsterPlacementEligibilityContext(state.HasUniqueCapacity(definition)));
        if (!eligibility.IsEligible)
        {
            return new MonsterPlacementResult(
                Success: false,
                Monster: null,
                MapFailureReason(eligibility.RejectionReason));
        }

        if (depth is { } placementDepth &&
            definition.SpawnPolicy.ForceDepth &&
            placementDepth < definition.NativeLevel)
        {
            return new MonsterPlacementResult(
                Success: false,
                Monster: null,
                MonsterPlacementFailureReason.ForceDepth);
        }

        var monster = state.Register(definition.Id, position);
        return new MonsterPlacementResult(
            Success: true,
            Monster: monster,
            MonsterPlacementFailureReason.None);
    }

    private static MonsterPlacementFailureReason MapFailureReason(
        MonsterPlacementRejectionReason reason) => reason switch
        {
            MonsterPlacementRejectionReason.UniqueUnavailable => MonsterPlacementFailureReason.UniqueUnavailable,
            _ => throw new InvalidOperationException($"Unsupported placement rejection reason '{reason}'."),
        };
}
