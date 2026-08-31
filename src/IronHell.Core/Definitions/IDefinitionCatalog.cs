namespace IronHell.Core.Definitions;

public interface IDefinitionCatalog
{
    IDefinitionRegistry<ActionDefinition> Actions { get; }

    IDefinitionRegistry<StatusDefinition> Statuses { get; }

    IDefinitionRegistry<CapabilityDefinition> Capabilities { get; }

    IDefinitionRegistry<ResistanceDefinition> Resistances { get; }

    IDefinitionRegistry<RaceDefinition> Races { get; }

    IDefinitionRegistry<ClassDefinition> Classes { get; }

    IDefinitionRegistry<ItemDefinition> Items { get; }

    IDefinitionRegistry<SpellDefinition> MageSpells { get; }

    IDefinitionRegistry<SpellDefinition> PriestPrayers { get; }

    IDefinitionRegistry<ActivationDefinition> Activations { get; }

    IDefinitionRegistry<MonsterAbilityDefinition> MonsterAbilities { get; }

    IReadOnlyCollection<RaceClassRule> RaceClassRules { get; }
}