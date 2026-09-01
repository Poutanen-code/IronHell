using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class ItemDefinitionReader
{
    private const string MissingIdError = "missing_id";

    public static readonly (string DocumentName, string CollectionName, ItemCategory Category)[] ItemCatalogs =
    [
        ("weapons", "weapons", ItemCategory.Weapon),
        ("armor", "armor", ItemCategory.Armor),
        ("lights", "lights", ItemCategory.Light),
        ("consumables", "consumables", ItemCategory.Consumable),
        ("potions", "potions", ItemCategory.Potion),
        ("scrolls", "scrolls", ItemCategory.Scroll),
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

                ValidateItemCategory(entry, catalog, id, report);
                items.Add(new ItemDefinition(id, catalog.Category, entry?["type"]?.GetValue<string>()));
            }
        }

        ValidationHelpers.ValidateDuplicates("items", items, report);
        return items;
    }

    private static void ValidateItemCategory(JsonNode? entry, (string DocumentName, string CollectionName, ItemCategory Category) catalog, string id, DefinitionValidationReport report)
    {
        var allowedTypes = catalog.Category switch
        {
            ItemCategory.Weapon => new[] { "sword", "dagger", "axe", "mace", "polearm", "staff", "bow", "crossbow", "shot", "arrow", "bolt", "digger" },
            ItemCategory.Armor => new[] { "chest", "helmet", "crown", "gloves", "boots", "shield", "cloak" },
            ItemCategory.Light => new[] { "torch", "lantern" },
            ItemCategory.Consumable => new[] { "mushroom", "food" },
            _ => [],
        };
        var type = entry?["type"]?.GetValue<string>();
        if (allowedTypes.Length > 0 && (type is null || !allowedTypes.Contains(type, StringComparer.Ordinal)))
        {
            report.Add($"items/{catalog.DocumentName}.json", id, "type", "invalid_item_category", "Item type is not valid for its catalog.");
        }
    }
}
