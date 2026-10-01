using IronHell.Core.Definitions;

namespace IronHell.Core.Monsters;

public static class MonsterEscortEligibility
{
    public static bool IsEligible(
        MonsterAllocationEntry candidate,
        MonsterDefinition leader,
        MonsterDefinition candidateDefinition)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(leader);
        ArgumentNullException.ThrowIfNull(candidateDefinition);

        return candidateDefinition.Symbol == leader.Symbol &&
            candidateDefinition.NativeLevel <= leader.NativeLevel &&
            !candidateDefinition.SpawnPolicy.Unique &&
            candidateDefinition.Id != leader.Id &&
            candidate.MonsterDefinitionId == candidateDefinition.Id;
    }
}
