namespace IronHell.Core.Definitions;

public sealed record ActionDefinition(string Id) : IIdentifiedDefinition;

public sealed record StatusDefinition(string Id) : IIdentifiedDefinition;

public sealed record CapabilityDefinition(string Id) : IIdentifiedDefinition;

public sealed record ResistanceDefinition(string Id) : IIdentifiedDefinition;

public sealed record SpellDefinition(string Id) : IIdentifiedDefinition;

public sealed record ActivationDefinition(string Id) : IIdentifiedDefinition;

public sealed record MonsterAbilityDefinition(string Id) : IIdentifiedDefinition;

public sealed record MonsterDefinition(string Id) : IIdentifiedDefinition;

public sealed record TerrainDefinition(string Id) : IIdentifiedDefinition;

public sealed record TrapDefinition(string Id) : IIdentifiedDefinition;

public enum ItemCategory
{
	Weapon,
	Armor,
	Light,
	Consumable,
	Potion,
	Scroll,
	SpellBook,
}

public sealed record ItemDefinition(string Id, ItemCategory Category) : IIdentifiedDefinition;