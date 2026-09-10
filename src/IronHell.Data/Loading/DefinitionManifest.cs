namespace IronHell.Data.Loading;

public sealed record DefinitionManifestEntry(string Name, string JsonPath, string SchemaPath);

public static class DefinitionManifest
{
    public static IReadOnlyList<DefinitionManifestEntry> CharacterCreation { get; } =
    [
        new("actions", "actions.json", "../schemas/actions.schema.json"),
        new("statuses", "statuses.json", "../schemas/statuses.schema.json"),
        new("capabilities", "capabilities.json", "../schemas/capabilities.schema.json"),
        new("resistances", "resistances.json", "../schemas/resistances.schema.json"),
        new("combat_modifiers", "combat_modifiers.json", "../schemas/combat_modifiers.schema.json"),
        new("activations", "activations.json", "../schemas/activations.schema.json"),
        new("monster_abilities", "monsters/monster_abilities.json", "../schemas/monsters/monster_abilities.schema.json"),
        new("monsters", "monsters/monsters.json", "../schemas/monsters/monsters.schema.json"),
        new("terrain", "environment/terrain_definitions.json", "../schemas/environment/terrain_definitions.schema.json"),
        new("traps", "environment/traps.json", "../schemas/environment/traps.schema.json"),
        new("weapons", "items/weapons.json", "../schemas/items/weapons.schema.json"),
        new("armor", "items/armor.json", "../schemas/items/armor.schema.json"),
        new("lights", "items/lights.json", "../schemas/items/lights.schema.json"),
        new("consumables", "items/consumables.json", "../schemas/items/consumables.schema.json"),
        new("potions", "items/potions.json", "../schemas/items/potions.schema.json"),
        new("scrolls", "items/scrolls.json", "../schemas/items/scrolls.schema.json"),
        new("ego_items", "items/ego_items.json", "../schemas/items/ego_items.schema.json"),
        new("artifacts", "items/artifacts.json", "../schemas/items/artifacts.schema.json"),
        new("item_affixes", "items/item_affixes.json", "../schemas/items/item_affixes.schema.json"),
        new("spell_books", "magic/spell_books.json", "../schemas/magic/spell_books.schema.json"),
        new("mage_spells", "magic/mage_spells.json", "../schemas/magic/spells.schema.json"),
        new("priest_prayers", "magic/priest_prayers.json", "../schemas/magic/spells.schema.json"),
        new("races", "character/races.json", "../schemas/character/races.schema.json"),
        new("classes", "character/classes.json", "../schemas/character/classes.schema.json"),
        new("race_class_rules", "character/race_class_rules.json", "../schemas/character/race_class_rules.schema.json"),
    ];
}