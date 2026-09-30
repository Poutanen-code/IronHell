using System.Text.Json;

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

public enum CapabilityCategory
{
	Resistance,
	ItemIgnore,
	Sustain,
	Perception,
	Survival,
	Curse,
	Combat,
	ClassCombat,
	ClassMagic,
	ClassMisc,
	OffensiveModifier,
}

public enum CapabilityScope
{
	NativeIdentity,
	BearerPassive,
	ItemSelfPassive,
}

public sealed record CapabilityDefinition(
	string Id,
	string Name,
	string Description,
	CapabilityCategory Category,
	CapabilityScope Scope,
	string ProvenanceStatus,
	IReadOnlyList<string> GrantSources,
	string? ResistanceId,
	IReadOnlyList<string> Aliases,
	bool? Deprecated,
	string? CanonicalOwner,
	string? MigrationTargetId,
	IReadOnlyList<string> PolicyHooks,
	string? Notes) : IIdentifiedDefinition;

public enum ResistanceSemanticKind
{
	Resist,
	Oppose,
	Immunity,
	Ignore,
}

public enum ResistanceTargetScope
{
	Bearer,
	ItemSelf,
}

public enum ResistanceChannel
{
	Acid,
	Elec,
	Fire,
	Cold,
	Pois,
	Lite,
	Dark,
	Blind,
	Confu,
	Sound,
	Shard,
	Nexus,
	Nethr,
	Chaos,
	Disen,
	Fear,
	Sleep,
	Stun,
	ElementalBundle,
}

public sealed record ResistanceDefinition(
	string Id,
	string Name,
	string Description,
	ResistanceSemanticKind SemanticKind,
	ResistanceTargetScope TargetScope,
	ResistanceChannel Channel,
	string ProvenanceStatus,
	string? StatusId,
	IReadOnlyList<string> GrantSources,
	string? Notes) : IIdentifiedDefinition;

public sealed record SpellActionAmount(string Kind, int? Value, int? DiceCount, int? DiceSides);

public sealed record SpellActionRef(
	string ActionId,
	ActionTargetMode TargetMode,
	SpellActionAmount? Amount = null,
	string? StatusId = null,
	IReadOnlyList<string>? StatusIds = null,
	JsonElement? Parameters = null);

public sealed record SpellDefinition(
	string Id,
	IReadOnlyList<SpellActionRef>? ActionRefs = null,
	SpellPolicyDefinition? Policy = null) : IIdentifiedDefinition;

public sealed record SpellPolicyDefinition(
	int Level,
	int Mana,
	int FailRate,
	int ExperienceValue,
	string BookId,
	string Realm,
	string? ExecutionPolicyId = null);

public sealed record ActivationDefinition(string Id) : IIdentifiedDefinition;

public sealed record MonsterAbilityDefinition(string Id) : IIdentifiedDefinition;

public sealed record MonsterCapabilityDefinition(
	string Id,
	string Name,
	string Description) : IIdentifiedDefinition;

public sealed record MonsterLootProfileDefinition(
	string Id,
	string DropKind,
	DiceRollDefinition Quantity,
	IReadOnlyList<BonusDropRuleDefinition> BonusDropRules,
	IReadOnlyList<string> GenerationRules,
	IReadOnlyList<string> SpecialRewards) : IIdentifiedDefinition;

public sealed record BonusDropRuleDefinition(int Chance, int Drops);

public sealed record DiceRollDefinition(string Kind, int Count, int Sides);

public sealed record MonsterAiDefinition(
	string Behavior,
	int RandomMoveChance,
	bool Stupid,
	bool Smart);

public enum MonsterTelepathyProfile
{
	Normal,
	WeirdMind,
	EmptyMind,
}

public sealed record MonsterSensesDefinition(
	int Alertness,
	MonsterTelepathyProfile TelepathyProfile);

public sealed record SpawnPolicy(
	bool Unique,
	bool Questor,
	bool ForceDepth,
	bool ForceMaxHp,
	bool ForceSleep,
	bool Escort,
	bool Escorts,
	bool Friends,
	bool Wanderer);

public sealed record MonsterDefinition(
	string Id,
	DiceRollDefinition HpRoll,
	MonsterAiDefinition Ai,
	IReadOnlyList<string> Capabilities,
	IReadOnlyList<string> Resistances,
	MonsterSensesDefinition Senses,
	SpawnPolicy SpawnPolicy,
	string? LootProfileId = null) : IIdentifiedDefinition;

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

public enum WeaponHandling
{
	OneHanded,
	TwoHanded,
	Ranged,
	Ammunition,
	Digging,
}

public enum WeaponFamily
{
	Sword,
	Dagger,
	Axe,
	Mace,
	Polearm,
	Staff,
	Sling,
	Bow,
	Crossbow,
	Shot,
	Arrow,
	Bolt,
	Shovel,
	Pick,
	Mattock,
}

public enum WeaponClass
{
	Blade,
	Hafted,
	PolearmAndAxe,
	Launcher,
	Ammunition,
	DiggingTool,
}

public enum AmmunitionFamily
{
	Shot,
	Arrow,
	Bolt,
}

public enum ArmorForm
{
	Chest,
	Helmet,
	Crown,
	Gloves,
	Boots,
	Shield,
	Cloak,
}

public enum BodyArmorFamily
{
	Soft,
	Hard,
	DragonScale,
}

public enum ArmorMaterial
{
	Cloth,
	Leather,
	Mail,
	Plate,
}

public enum ConsumableKind
{
	Mushroom,
	Food,
}

public enum LightSourceKind
{
	Torch,
	Lantern,
}

public enum LightFuelPolicy
{
	Finite,
	Inexhaustible,
}

public enum SpellBookRealm
{
	Magic,
	Prayer,
}

public enum GoodGenerationQuality
{
	Normal,
	Good,
}

public sealed record ItemDefinition(
	string Id,
	ItemCategory Category,
	WeaponHandling? WeaponHandling = null,
	WeaponFamily? WeaponFamily = null,
	WeaponClass? WeaponClass = null,
	AmmunitionFamily? CompatibleAmmunitionFamily = null,
	ArmorForm? ArmorForm = null,
	BodyArmorFamily? BodyArmorFamily = null,
	ArmorMaterial? ArmorMaterial = null,
	ConsumableKind? ConsumableKind = null,
	LightSourceKind? LightSourceKind = null,
	LightFuelPolicy? LightFuelPolicy = null,
	SpellBookRealm? SpellBookRealm = null,
	GoodGenerationQuality? GenerationQuality = null,
	IReadOnlyList<string>? SpellIds = null,
	IReadOnlyList<ItemActionReference>? Actions = null,
	int TreeCuttingEffectiveness = 0,
	int? LauncherPowerMultiplier = null,
	IReadOnlyList<string>? CombatModifierIds = null,
	IReadOnlyList<string>? CapabilityIds = null,
	IReadOnlyList<string>? ResistanceIds = null,
	IReadOnlyList<ItemAffixDefinition>? Affixes = null,
	IReadOnlyList<GeneratedItemAffixDefinition>? GeneratedAffixes = null,
	string? Name = null,
	int? FuelPval = null,
	int? LightRadius = null,
	int? StackSize = null,
	double? Weight = null,
	int? SellValue = null) : IIdentifiedDefinition;

public sealed record ItemActionReference(string ActionId, JsonElement? Parameters = null);

public sealed record ItemAffixDefinition(string Type, double Value);

public sealed record GeneratedItemAffixDefinition(string Type, int MinValue, int MaxValue);

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

public sealed record FlavorLegacyMetadata(int MangbandIndex);

public sealed record FlavorDefinition(
	string Id,
	FlavorCategory Category,
	string DisplayName,
	string Glyph,
	string Color,
	FlavorLegacyMetadata Legacy,
	string ProvenanceStatus) : IIdentifiedDefinition;