namespace IronHell.Core.Definitions;

public sealed record ActionDefinition(
	string ActionId,
	IReadOnlySet<ActionSourceType>? AllowedSourceTypes = null,
	IReadOnlySet<ActionTargetMode>? AllowedTargetModes = null,
	string Name = "",
	string Description = "",
	ActionCategory Category = ActionCategory.Utility,
	ActionConfidence Confidence = ActionConfidence.Medium,
	IReadOnlySet<ActionSourceFamily>? AllowedSourceFamilies = null,
	ActionParameterContract? ParameterContract = null,
	ActionValidationRules? ValidationRules = null,
	string? ProvenanceStatus = null,
	ActionProvenance? Provenance = null) : IIdentifiedDefinition
{
	public string Id => ActionId;
}

public enum ActionCategory
{
	Damage,
	Healing,
	Status,
	Movement,
	Information,
	Item,
	Terrain,
	Control,
	Summoning,
	Utility,
	Attribute,
	Progression,
}

public enum ActionConfidence
{
	High,
	Medium,
}

public enum ActionSourceFamily
{
	Spell,
	Prayer,
	MonsterAttack,
	MonsterAbility,
	Rod,
	Wand,
	Staff,
	Potion,
	Scroll,
	Consumable,
	Activation,
	Trap,
	Chest,
	Treasure,
	DeathDrop,
	Environmental,
	ItemProperty,
	MonsterBlow,
	MonsterSpell,
}

public enum ActionParameterValueType
{
	Integer,
	Boolean,
	Id,
	IdList,
	Enum,
	EnumList,
	Token,
	StructuredAmount,
	Duration,
}

public sealed record ActionParameterDefinition(
	string Id,
	string Description,
	ActionParameterValueType ValueType,
	bool Required,
	string? ReferenceDomain = null,
	IReadOnlySet<string>? AllowedValues = null,
	int? Minimum = null,
	int? Maximum = null,
	int? MinItems = null,
	int? MaxItems = null,
	string? Notes = null);

public sealed record ActionParameterContract(
	bool Closed,
	IReadOnlyList<ActionParameterDefinition> Parameters);

public sealed record ActionValidationRules(
	bool RejectUnknownParameters,
	bool RequireDeclaredRequiredParameters,
	bool EnforceDeclaredValueTypes);

public sealed record ActionProvenance(string Summary);

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

public sealed record DiceRollDefinition(string Kind, int Count, int Sides);

public sealed record MonsterDefinition(string Id, DiceRollDefinition HpRoll) : IIdentifiedDefinition;

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
	Ring,
	Amulet,
	Staff,
	Wand,
	Rod,
}

public sealed record ItemDefinition(string Id, ItemCategory Category, string? Type) : IIdentifiedDefinition;

public enum FlavorCategory
{
	Ring,
	Amulet,
	Staff,
	Wand,
	Rod,
	Potion,
	Mushroom,
	Scroll,
}

public sealed record FlavorLegacyMetadata(int MangbandIndex, int Tval, int? Sval);

public sealed record FlavorDefinition(
	string Id,
	FlavorCategory Category,
	string DisplayName,
	string Glyph,
	string Color,
	FlavorLegacyMetadata Legacy,
	string ProvenanceStatus) : IIdentifiedDefinition;