using IronHell.Core.Definitions;

namespace IronHell.Data;

internal sealed record DefinitionCatalog(
    IDefinitionRegistry<ActionDefinition> Actions,
    IDefinitionRegistry<StatusDefinition> Statuses,
    IDefinitionRegistry<CapabilityDefinition> Capabilities,
    IDefinitionRegistry<ResistanceDefinition> Resistances,
    IDefinitionRegistry<RaceDefinition> Races,
    IDefinitionRegistry<ClassDefinition> Classes,
    IDefinitionRegistry<ItemDefinition> Items,
    IDefinitionRegistry<SpellDefinition> MageSpells,
    IDefinitionRegistry<SpellDefinition> PriestPrayers,
    IDefinitionRegistry<ActivationDefinition> Activations,
    IDefinitionRegistry<MonsterAbilityDefinition> MonsterAbilities,
    IDefinitionRegistry<MonsterDefinition> Monsters,
    IDefinitionRegistry<TerrainDefinition> Terrain,
    IReadOnlyCollection<RaceClassRule> RaceClassRules) : IDefinitionCatalog;