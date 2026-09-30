using System.Collections.Frozen;
using System.Text.Json;
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

                var classification = ReadClassification(entry?.AsObject(), category.Value, type, catalog.DocumentName, id, report);
                items.Add(new ItemDefinition(
                    id,
                    category.Value,
                    classification.WeaponHandling,
                    classification.WeaponFamily,
                    classification.WeaponClass,
                        ReadCompatibleAmmunitionFamily(entry?["stats"]?["compatible_ammo_family"]?.GetValue<string>(), category.Value, classification.WeaponFamily, classification.WeaponClass, id, report),
                    classification.ArmorForm,
                    classification.BodyArmorFamily,
                    classification.ArmorMaterial,
                    classification.ConsumableKind,
                    classification.LightSourceKind,
                    ReadLightFuelPolicy(entry?["fuel_policy"]?.GetValue<string>(), catalog.DocumentName, id, report),
                    ReadSpellBookRealm(entry?["realm"]?.GetValue<string>(), catalog.DocumentName, id, report),
                    ReadGenerationQuality(entry?["generation_quality"]?.GetValue<string>(), catalog.DocumentName, id, report),
                    ReadStringIds(entry?["spell_ids"]),
                    ReadActionReferences(entry?["actions"]),
                    entry?["stats"]?["tree_cutting_effectiveness"]?.GetValue<int>() ?? 0,
                    entry?["stats"]?["launcher_power_multiplier"]?.GetValue<int>(),
                    ReadStringIds(entry?["combat_modifiers"]),
                    ReadStringIds(entry?["capability_ids"]),
                    ReadStringIds(entry?["resistance_ids"]),
                    ReadAffixes(entry?["affixes"]),
                    ReadGeneratedAffixes(entry?["generated_affixes"]),
                    entry?["name"]?.GetValue<string>(),
                    entry?["fuel_pval"]?.GetValue<int>(),
                    entry?["light_radius"]?.GetValue<int>(),
                    entry?["stack_size"]?.GetValue<int>(),
                    ReadNumber(entry?["weight"]),
                    entry?["sell_value"]?.GetValue<int>()));
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

    private readonly record struct ItemClassification(
        WeaponHandling? WeaponHandling = null,
        WeaponFamily? WeaponFamily = null,
        WeaponClass? WeaponClass = null,
        ArmorForm? ArmorForm = null,
        BodyArmorFamily? BodyArmorFamily = null,
        ArmorMaterial? ArmorMaterial = null,
        ConsumableKind? ConsumableKind = null,
        LightSourceKind? LightSourceKind = null);

    private static ItemClassification ReadClassification(
        JsonObject? entry,
        ItemCategory category,
        string? type,
        string documentName,
        string id,
        DefinitionValidationReport report)
    {
        switch (category)
        {
            case ItemCategory.Weapon:
            {
                var handling = type switch
                {
                    "one_handed" => WeaponHandling.OneHanded,
                    "two_handed" => WeaponHandling.TwoHanded,
                    "ranged" => WeaponHandling.Ranged,
                    "ammo" => WeaponHandling.Ammunition,
                    "digging" => WeaponHandling.Digging,
                    _ => (WeaponHandling?)null,
                };
                var family = ParseWeaponFamily(entry?["subtype"]?.GetValue<string>());
                var weaponClass = ResolveWeaponClass(family);
                var compatible = handling switch
                {
                    WeaponHandling.OneHanded or WeaponHandling.TwoHanded => family is WeaponFamily.Sword or WeaponFamily.Dagger or WeaponFamily.Axe or WeaponFamily.Mace or WeaponFamily.Polearm or WeaponFamily.Staff,
                    WeaponHandling.Ranged => weaponClass == WeaponClass.Launcher,
                    WeaponHandling.Ammunition => weaponClass == WeaponClass.Ammunition,
                    WeaponHandling.Digging => weaponClass == WeaponClass.DiggingTool,
                    _ => false,
                };
                if (handling is null || family is null || weaponClass is null || !compatible)
                {
                    report.Add($"items/{documentName}.json", id, "type/subtype", InvalidCategoryError, "Weapon handling and family must be recognized and compatible values.");
                }

                return new ItemClassification(WeaponHandling: handling, WeaponFamily: family, WeaponClass: weaponClass);
            }
            case ItemCategory.Armor:
            {
                var form = type switch
                {
                    "chest" => ArmorForm.Chest,
                    "helmet" => ArmorForm.Helmet,
                    "crown" => ArmorForm.Crown,
                    "gloves" => ArmorForm.Gloves,
                    "boots" => ArmorForm.Boots,
                    "shield" => ArmorForm.Shield,
                    "cloak" => ArmorForm.Cloak,
                    _ => (ArmorForm?)null,
                };
                var material = entry?["armor_weight"]?.GetValue<string>() switch
                {
                    "cloth" => ArmorMaterial.Cloth,
                    "leather" => ArmorMaterial.Leather,
                    "mail" => ArmorMaterial.Mail,
                    "plate" => ArmorMaterial.Plate,
                    _ => (ArmorMaterial?)null,
                };
                var bodyFamily = entry?["armor_family"]?.GetValue<string>() switch
                {
                    "soft" => BodyArmorFamily.Soft,
                    "hard" => BodyArmorFamily.Hard,
                    "dragon_scale" => BodyArmorFamily.DragonScale,
                    null => null,
                    _ => (BodyArmorFamily?)null,
                };
                var compatibleMaterial = bodyFamily switch
                {
                    BodyArmorFamily.Soft => material is ArmorMaterial.Cloth or ArmorMaterial.Leather,
                    BodyArmorFamily.Hard => material is ArmorMaterial.Mail or ArmorMaterial.Plate,
                    BodyArmorFamily.DragonScale => material == ArmorMaterial.Plate,
                    null => true,
                    _ => false,
                };
                if (form is null || material is null || (form == ArmorForm.Chest) != (bodyFamily is not null) || !compatibleMaterial)
                {
                    report.Add($"items/{documentName}.json", id, "type/armor_weight/armor_family", InvalidCategoryError, "Armor form and material must be recognized, and body armor must declare exactly one armor family.");
                }

                return new ItemClassification(ArmorForm: form, BodyArmorFamily: bodyFamily, ArmorMaterial: material);
            }
            case ItemCategory.Light:
            {
                var kind = type switch
                {
                    "torch" => LightSourceKind.Torch,
                    "lantern" => LightSourceKind.Lantern,
                    _ => (LightSourceKind?)null,
                };
                if (kind is null)
                {
                    report.Add($"items/{documentName}.json", id, "type", InvalidCategoryError, "Light source kind must be 'torch' or 'lantern'.");
                }

                return new ItemClassification(LightSourceKind: kind);
            }
            case ItemCategory.Consumable:
            {
                var kind = type switch
                {
                    "mushroom" => ConsumableKind.Mushroom,
                    "food" => ConsumableKind.Food,
                    _ => (ConsumableKind?)null,
                };
                if (kind is null)
                {
                    report.Add($"items/{documentName}.json", id, "type", InvalidCategoryError, "Consumable kind must be recognized.");
                }

                return new ItemClassification(ConsumableKind: kind);
            }
            default:
                return new ItemClassification();
        }
    }

    private static WeaponFamily? ParseWeaponFamily(string? family) => family switch
    {
        "sword" => WeaponFamily.Sword,
        "dagger" => WeaponFamily.Dagger,
        "axe" => WeaponFamily.Axe,
        "mace" => WeaponFamily.Mace,
        "polearm" => WeaponFamily.Polearm,
        "staff" => WeaponFamily.Staff,
        "sling" => WeaponFamily.Sling,
        "bow" => WeaponFamily.Bow,
        "crossbow" => WeaponFamily.Crossbow,
        "shot" => WeaponFamily.Shot,
        "arrow" => WeaponFamily.Arrow,
        "bolt" => WeaponFamily.Bolt,
        "shovel" => WeaponFamily.Shovel,
        "pick" => WeaponFamily.Pick,
        "mattock" => WeaponFamily.Mattock,
        _ => null,
    };

    private static WeaponClass? ResolveWeaponClass(WeaponFamily? family) => family switch
    {
        WeaponFamily.Sword or WeaponFamily.Dagger => WeaponClass.Blade,
        WeaponFamily.Mace or WeaponFamily.Staff => WeaponClass.Hafted,
        WeaponFamily.Axe or WeaponFamily.Polearm => WeaponClass.PolearmAndAxe,
        WeaponFamily.Sling or WeaponFamily.Bow or WeaponFamily.Crossbow => WeaponClass.Launcher,
        WeaponFamily.Shot or WeaponFamily.Arrow or WeaponFamily.Bolt => WeaponClass.Ammunition,
        WeaponFamily.Shovel or WeaponFamily.Pick or WeaponFamily.Mattock => WeaponClass.DiggingTool,
        _ => null,
    };

    private static AmmunitionFamily? ReadCompatibleAmmunitionFamily(
        string? value,
        ItemCategory category,
        WeaponFamily? weaponFamily,
        WeaponClass? weaponClass,
        string id,
        DefinitionValidationReport report)
    {
        var expected = weaponFamily switch
        {
            WeaponFamily.Sling => AmmunitionFamily.Shot,
            WeaponFamily.Bow => AmmunitionFamily.Arrow,
            WeaponFamily.Crossbow => AmmunitionFamily.Bolt,
            _ => (AmmunitionFamily?)null,
        };

        if (value is null)
        {
            if (weaponClass == WeaponClass.Launcher)
            {
                report.Add("items/weapons.json", id, "stats.compatible_ammo_family", InvalidCategoryError, "Launcher must declare its compatible ammunition family.");
            }

            return null;
        }

        if (category != ItemCategory.Weapon || weaponClass != WeaponClass.Launcher)
        {
            report.Add("items/weapons.json", id, "stats.compatible_ammo_family", InvalidCategoryError, "Only launcher weapons may declare compatible ammunition.");
            return null;
        }

        var actual = value switch
        {
            "shot" => AmmunitionFamily.Shot,
            "arrow" => AmmunitionFamily.Arrow,
            "bolt" => AmmunitionFamily.Bolt,
            _ => InvalidCompatibleAmmunitionFamily(id, report),
        };
        if (actual is not null && actual != expected)
        {
            report.Add("items/weapons.json", id, "stats.compatible_ammo_family", InvalidCategoryError, $"Launcher family '{weaponFamily}' requires ammunition family '{expected}', not '{actual}'.");
        }

        return actual;
    }

    private static AmmunitionFamily? InvalidCompatibleAmmunitionFamily(string id, DefinitionValidationReport report)
    {
        report.Add("items/weapons.json", id, "stats.compatible_ammo_family", InvalidCategoryError, "Compatible ammunition family must be shot, arrow, or bolt.");
        return null;
    }

    private static LightFuelPolicy? ReadLightFuelPolicy(string? value, string documentName, string id, DefinitionValidationReport report)
    {
        if (documentName != "lights")
        {
            return null;
        }

        return value switch
        {
            "finite" => LightFuelPolicy.Finite,
            "inexhaustible" => LightFuelPolicy.Inexhaustible,
            _ => InvalidLightFuelPolicy(documentName, id, report),
        };
    }

    private static LightFuelPolicy? InvalidLightFuelPolicy(string documentName, string id, DefinitionValidationReport report)
    {
        report.Add($"items/{documentName}.json", id, "fuel_policy", InvalidCategoryError, "Light fuel policy must be 'finite' or 'inexhaustible'.");
        return null;
    }

    private static SpellBookRealm? ReadSpellBookRealm(string? value, string documentName, string id, DefinitionValidationReport report)
    {
        if (documentName != "spell_books")
        {
            return null;
        }

        return value switch
        {
            "magic" => SpellBookRealm.Magic,
            "prayer" => SpellBookRealm.Prayer,
            _ => InvalidSpellBookRealm(documentName, id, report),
        };
    }

    private static SpellBookRealm? InvalidSpellBookRealm(string documentName, string id, DefinitionValidationReport report)
    {
        report.Add($"magic/{documentName}.json", id, "realm", InvalidCategoryError, "Spell book realm must be 'magic' or 'prayer'.");
        return null;
    }

    private static GoodGenerationQuality? ReadGenerationQuality(string? value, string documentName, string id, DefinitionValidationReport report)
    {
        if (documentName != "spell_books")
        {
            return null;
        }

        return value switch
        {
            "normal" => GoodGenerationQuality.Normal,
            "good" => GoodGenerationQuality.Good,
            _ => InvalidGenerationQuality(documentName, id, report),
        };
    }

    private static GoodGenerationQuality? InvalidGenerationQuality(string documentName, string id, DefinitionValidationReport report)
    {
        report.Add($"magic/{documentName}.json", id, "generation_quality", InvalidCategoryError, "Generation quality must be 'normal' or 'good'.");
        return null;
    }

    private static IReadOnlyList<ItemActionReference>? ReadActionReferences(JsonNode? node) => node is JsonArray array
        ? Array.AsReadOnly(array.OfType<JsonObject>()
            .Select(action => new ItemActionReference(
                action["action_id"]?.GetValue<string>() ?? string.Empty,
                ReadParameters(action["parameters"]?.AsObject())))
            .ToArray())
        : null;

    private static JsonElement? ReadParameters(JsonObject? parameters)
    {
        if (parameters is null)
        {
            return null;
        }

        using var document = JsonDocument.Parse(parameters.ToJsonString());
        return document.RootElement.Clone();
    }

    private static IReadOnlyList<string>? ReadStringIds(JsonNode? node) => node is JsonArray array
        ? Array.AsReadOnly(array.Select(value => value?.GetValue<string>() ?? string.Empty).ToArray())
        : null;

    private static IReadOnlyList<ItemAffixDefinition>? ReadAffixes(JsonNode? node) => node is JsonArray array
        ? Array.AsReadOnly(array.OfType<JsonObject>()
            .Select(affix => new ItemAffixDefinition(
                affix["type"]?.GetValue<string>() ?? string.Empty,
                ReadNumber(affix["value"]) ?? throw new InvalidOperationException("Static item affix value must be numeric.")))
            .ToArray())
        : null;

    private static IReadOnlyList<GeneratedItemAffixDefinition>? ReadGeneratedAffixes(JsonNode? node) => node is JsonArray array
        ? Array.AsReadOnly(array.OfType<JsonObject>()
            .Select(affix => new GeneratedItemAffixDefinition(
                affix["type"]?.GetValue<string>() ?? string.Empty,
                affix["min_value"]?.GetValue<int>() ?? 0,
                affix["max_value"]?.GetValue<int>() ?? 0))
            .ToArray())
        : null;

    private static double? ReadNumber(JsonNode? node)
    {
        if (node is not JsonValue value)
        {
            return null;
        }

        if (value.TryGetValue<double>(out var number))
        {
            return number;
        }

        if (value.TryGetValue<int>(out var integer))
        {
            return integer;
        }

        return null;
    }
}

