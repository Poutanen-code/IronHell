using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal sealed record ValidationRegistries(
    IDefinitionRegistry<ActionDefinition> Actions,
    IDefinitionRegistry<StatusDefinition> Statuses,
    IDefinitionRegistry<CapabilityDefinition> Capabilities,
    IDefinitionRegistry<ResistanceDefinition> Resistances,
    IDefinitionRegistry<ItemDefinition> Items,
    IDefinitionRegistry<SpellDefinition> MageSpells,
    IDefinitionRegistry<SpellDefinition> PriestPrayers,
    IDefinitionRegistry<ActivationDefinition> Activations,
    IDefinitionRegistry<MonsterAbilityDefinition> MonsterAbilities,
    IDefinitionRegistry<MonsterDefinition> Monsters,
    IDefinitionRegistry<TerrainDefinition> Terrain,
    IDefinitionRegistry<TrapDefinition> Traps);
