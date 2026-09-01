namespace IronHell.Core.Definitions;

public sealed record ActionDefinition(
	string Id,
	IReadOnlySet<ActionSourceType>? AllowedSourceTypes = null,
	IReadOnlySet<ActionTargetMode>? AllowedTargetModes = null) : IIdentifiedDefinition;

public enum ActionSourceType
{
	CharacterSpell,
	CharacterPrayer,
	ItemActivation,
	MonsterAbility,
	Trap,
	DirectRuntime,
}

public enum ActionTargetMode
{
	Self,
	OtherCharacter,
}

public enum StatusApplicationPolicy
{
	IgnoreIfPresent,
	RefreshDuration,
	ReplaceExisting,
}

public sealed record StatusDurationDefinition(
	int FixedDuration,
	int? DiceCount,
	int? DiceSides,
	int? LevelMultiplier);

public sealed record StatusDefinition(
	string Id,
	StatusApplicationPolicy ApplicationPolicy,
	StatusDurationDefinition Duration) : IIdentifiedDefinition;

public sealed record CapabilityDefinition(string Id) : IIdentifiedDefinition;

public sealed record ResistanceDefinition(string Id) : IIdentifiedDefinition;

public sealed record SpellActionAmount(string Kind, int? Value, int? DiceCount, int? DiceSides);

public sealed record SpellActionRef(
	string ActionId,
	ActionTargetMode TargetMode,
	SpellActionAmount? Amount = null,
	string? StatusId = null,
	IReadOnlyList<string>? StatusIds = null);

public sealed record SpellDefinition(
	string Id,
	IReadOnlyList<SpellActionRef>? ActionRefs = null) : IIdentifiedDefinition;

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

public sealed record ItemDefinition(string Id, ItemCategory Category, string? Type) : IIdentifiedDefinition;