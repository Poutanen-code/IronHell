using IronHell.Core.Definitions;
using IronHell.Core.Randomness;

namespace IronHell.Core.Monsters;

public sealed record MonsterEscortExpansionOptions(
    IReadOnlyList<MonsterAllocationEntry> AllocationEntries,
    IReadOnlyCollection<MonsterDefinition> Definitions,
    MonsterPlacementSpace Space,
    int Depth,
    bool AllowGroupExpansion,
    IRandomSource RandomSource);
