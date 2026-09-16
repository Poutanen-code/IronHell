using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class ItemDefinitionReader
{
    private const string MissingIdError = "missing_id";
    private const string InvalidCategoryError = "invalid_item_category";

    public static readonly (string DocumentName, string CollectionName, ItemCategory? Category)[] ItemCatalogs =
    [
        ("weapons", "weapons", ItemCategory.Weapon),
        ("armor", "armor", ItemCategory.Armor),
        ("lights", "lights", ItemCategory.Light),
        ("consumables", "consumables", ItemCategory.Consumable),
        ("potions", "potions", ItemCategory.Potion),
        ("scrolls", "scrolls", ItemCategory.Scroll),
        ("accessories", "accessories", null),
        ("staves", "staves", ItemCategory.Staff),
        ("wands", "wands", ItemCategory.Wand),
        ("rods", "rods", ItemCategory.Rod),
        ("spell_books", "books", ItemCategory.SpellBook),
    ];

    public static List<ItemDefinition> Read(FrozenDictionary<string, JsonObject> documents, DefinitionValidationReport report)
    {
        var items = new List<ItemDefinition>();
        foreach (var catalog in ItemCatalogs)
        {
            foreach (var entry in documents[catalog.DocumentName][catalog.CollectionName]?.AsArray() ?? [])
            {
                var id = entry?["id"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(id))
                {
                    report.Add($"items/{catalog.DocumentName}.json", null, "$.id", MissingIdError, "Item identifier is required.");
                    continue;
                }

                var type = entry?["type"]?.GetValue<string>();
                var category = catalog.Category ?? ResolveAccessoryCategory(type, catalog.DocumentName, id, report);
                if (category is null)
                {
                    continue;
                }

                ValidateItemCategory(type, catalog.DocumentName, category.Value, id, report);
                items.Add(new ItemDefinition(id, category.Value, type));
            }
        }

        ValidationHelpers.ValidateDuplicates("items", items, report);
        return items;
    }

    private static ItemCategory? ResolveAccessoryCategory(string? type, string documentName, string id, DefinitionValidationReport report)
    {
        switch (type)
        {
            case "ring": return ItemCategory.Ring;
            case "amulet": return ItemCategory.Amulet;
            default:
                report.Add($"items/{documentName}.json", id, "type", InvalidCategoryError, "Accessory type must be 'ring' or 'amulet'.");
                return null;
        }
    }

    private static void ValidateItemCategory(string? type, string documentName, ItemCategory category, string id, DefinitionValidationReport report)
    {
        var allowedTypes = category switch
        {
            ItemCategory.Weapon => new[] { "sword", "dagger", "axe", "mace", "polearm", "staff", "bow", "crossbow", "shot", "arrow", "bolt", "digger" },
            ItemCategory.Armor => new[] { "chest", "helmet", "crown", "gloves", "boots", "shield", "cloak" },
            ItemCategory.Light => new[] { "torch", "lantern" },
            ItemCategory.Consumable => new[] { "mushroom", "food" },
            ItemCategory.Ring => new[] { "ring" },
            ItemCategory.Amulet => new[] { "amulet" },
            _ => [],
        };
        if (allowedTypes.Length > 0 && (type is null || !allowedTypes.Contains(type, StringComparer.Ordinal)))
        {
            report.Add($"items/{documentName}.json", id, "type", InvalidCategoryError, "Item type is not valid for its catalog.");
        }
    }
}

