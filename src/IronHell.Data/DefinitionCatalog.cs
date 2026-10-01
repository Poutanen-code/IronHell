using IronHell.Core.Definitions;
using IronHell.Core.Dungeon;

namespace IronHell.Data;

internal sealed record DefinitionCatalog(
    IDefinitionRegistry<ActionDefinition> Actions,
    IDefinitionRegistry<StatusDefinition> Statuses,
    IDefinitionRegistry<CapabilityDefinition> Capabilities,
    IDefinitionRegistry<ResistanceDefinition> Resistances,
    IDefinitionRegistry<RaceDefinition> Races,
    IDefinitionRegistry<ClassDefinition> Classes,
    IDefinitionRegistry<ItemDefinition> Items,
    IDefinitionRegistry<FlavorDefinition> Flavors,
    IDefinitionRegistry<SpellDefinition> MageSpells,
    IDefinitionRegistry<SpellDefinition> PriestPrayers,
    IDefinitionRegistry<ActivationDefinition> Activations,
    IDefinitionRegistry<MonsterAbilityDefinition> MonsterAbilities,
    IDefinitionRegistry<MonsterCapabilityDefinition> MonsterCapabilities,
    IDefinitionRegistry<MonsterLootProfileDefinition> MonsterLootProfiles,
    IDefinitionRegistry<MonsterDefinition> Monsters,
    IDefinitionRegistry<TerrainDefinition> Terrain,
    IDefinitionRegistry<TrapDefinition> Traps,
    IDefinitionRegistry<VaultDefinition> Vaults,
    IReadOnlyCollection<RaceClassRule> RaceClassRules) : IDefinitionCatalog;