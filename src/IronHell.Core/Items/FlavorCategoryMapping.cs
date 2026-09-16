using IronHell.Core.Definitions;

namespace IronHell.Core.Items;

/// <summary>Maps an ItemDefinition to the FlavorCategory pool it should draw from, if any.</summary>
public static class FlavorCategoryMapping
{
    public static bool TryGetFlavorCategory(ItemDefinition item, out FlavorCategory category)
    {
        switch (item.Category)
        {
            case ItemCategory.Ring:
                category = FlavorCategory.Ring;
                return true;
            case ItemCategory.Amulet:
                category = FlavorCategory.Amulet;
                return true;
            case ItemCategory.Staff:
                category = FlavorCategory.Staff;
                return true;
            case ItemCategory.Wand:
                category = FlavorCategory.Wand;
                return true;
            case ItemCategory.Rod:
                category = FlavorCategory.Rod;
                return true;
            case ItemCategory.Potion:
                category = FlavorCategory.Potion;
                return true;
            case ItemCategory.Scroll:
                category = FlavorCategory.Scroll;
                return true;
            case ItemCategory.Consumable when item.Type == "mushroom":
                category = FlavorCategory.Mushroom;
                return true;
            default:
                category = default;
                return false;
        }
    }
}
