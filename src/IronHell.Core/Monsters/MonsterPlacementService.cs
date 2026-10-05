using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

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
    private const int LevelSpeedAtTown = 37_500;
    private const int ExtractEnergyTableLength = 200;

    public static MonsterPlacementResult Place(
        MonsterRuntimeState state,
        MonsterDefinition definition,
        MonsterPosition position,
        IRandomSource randomSource,
        MonsterPlacementSpace? space = null,
        int? depth = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(randomSource);

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

        var spawnState = InitializeSpawnState(definition, randomSource);
        var monster = state.Register(definition.Id, position, spawnState);
        return new MonsterPlacementResult(
            Success: true,
            Monster: monster,
            MonsterPlacementFailureReason.None);
    }

    private static MonsterSpawnState InitializeSpawnState(
        MonsterDefinition definition,
        IRandomSource randomSource)
    {
        var hpRoll = definition.HpRoll;
        if (hpRoll.Kind != "dice" || hpRoll.Count <= 0 || hpRoll.Sides <= 0)
        {
            throw new InvalidOperationException($"Monster '{definition.Id}' has an invalid HP roll.");
        }

        if (definition.MovementSpeed is not { } baseSpeed ||
            baseSpeed < 0 || baseSpeed >= ExtractEnergyTableLength)
        {
            throw new InvalidOperationException($"Monster '{definition.Id}' has no valid source movement speed.");
        }

        var maxHp = definition.SpawnPolicy.ForceMaxHp
            ? hpRoll.Count * hpRoll.Sides
            : RollDamageDice(hpRoll, randomSource);
        var movementSpeed = baseSpeed;
        if (!definition.SpawnPolicy.Unique)
        {
            var variance = GetSpeedVariance(baseSpeed);
            if (variance > 0)
            {
                movementSpeed += randomSource.Next(-variance, variance + 1);
            }
        }

        var energy = randomSource.Next(0, LevelSpeedAtTown);
        if (definition.SpawnPolicy.ForceSleep)
        {
            energy = randomSource.Next(0, LevelSpeedAtTown >> 4);
        }

        return new MonsterSpawnState(maxHp, maxHp, movementSpeed, energy);
    }

    // Source Rand_div returns zero without advancing RNG when the divisor is one.
    private static int RollDamageDice(DiceRollDefinition hpRoll, IRandomSource randomSource) =>
        hpRoll.Sides == 1
            ? hpRoll.Count
            : randomSource.RollDice(hpRoll.Count, hpRoll.Sides);

    // These bands are the exact integer reduction of extract_energy[speed] / 100 / 10.
    private static int GetSpeedVariance(int movementSpeed) => movementSpeed switch
    {
        < 110 => 0,
        < 120 => 1,
        < 130 => 2,
        < 144 => 3,
        _ => 4,
    };

    private static MonsterPlacementFailureReason MapFailureReason(
        MonsterPlacementRejectionReason reason) => reason switch
        {
            MonsterPlacementRejectionReason.UniqueUnavailable => MonsterPlacementFailureReason.UniqueUnavailable,
            _ => throw new InvalidOperationException($"Unsupported placement rejection reason '{reason}'."),
        };
}
