using IronHell.Core.Definitions;
using IronHell.Core.Items;
using Xunit;

namespace IronHell.Core.Tests.Items;

public sealed class InventoryEquipmentTests
{
    [Fact]
    public void Inventory_AddRemoveAndEnumerate_TracksItemInstances()
    {
        var inventory = new Inventory();
        var item = new ItemInstance("character-1:item:1", "dagger");

        inventory.Add(item);

        Assert.Equal([item], inventory.Items);
        Assert.True(inventory.Remove(item.InstanceId));
        Assert.Empty(inventory.Items);
    }

    [Fact]
    public void Equipment_EquipUnequipAndEnumerate_TracksItemInstances()
    {
        var equipment = new Equipment();
        var item = new ItemInstance("character-1:item:1", "dagger");
        var definition = new ItemDefinition("dagger", ItemCategory.Weapon, WeaponHandling: WeaponHandling.OneHanded, WeaponFamily: WeaponFamily.Dagger, WeaponClass: WeaponClass.Blade);

        equipment.Equip(EquipmentSlot.Weapon, item, definition);

        Assert.Equal([item], equipment.Items);
        Assert.Equal(item, equipment.GetEquipped(EquipmentSlot.Weapon));
        Assert.True(equipment.IsEquipped(EquipmentSlot.Weapon));
        Assert.Equal(item, equipment.Unequip(EquipmentSlot.Weapon));
        Assert.Empty(equipment.Items);
    }

    [Fact]
    public void Equipment_Equip_RejectsInvalidSlot()
    {
        var equipment = new Equipment();
        var item = new ItemInstance("character-1:item:1", "dagger");
        var definition = new ItemDefinition("dagger", ItemCategory.Weapon, WeaponHandling: WeaponHandling.OneHanded, WeaponFamily: WeaponFamily.Dagger, WeaponClass: WeaponClass.Blade);

        Assert.Throws<InvalidOperationException>(() => { equipment.Equip(EquipmentSlot.Body, item, definition); });
    }

    [Fact]
    public void Equipment_Equip_ReplacesExistingItemInSameSlot()
    {
        var equipment = new Equipment();
        var first = new ItemInstance("character-1:item:1", "dagger");
        var second = new ItemInstance("character-1:item:2", "short_sword");
        var firstDefinition = new ItemDefinition("dagger", ItemCategory.Weapon, WeaponHandling: WeaponHandling.OneHanded, WeaponFamily: WeaponFamily.Dagger, WeaponClass: WeaponClass.Blade);
        var secondDefinition = new ItemDefinition("short_sword", ItemCategory.Weapon, WeaponHandling: WeaponHandling.OneHanded, WeaponFamily: WeaponFamily.Sword, WeaponClass: WeaponClass.Blade);

        equipment.Equip(EquipmentSlot.Weapon, first, firstDefinition);
        var replaced = equipment.Equip(EquipmentSlot.Weapon, second, secondDefinition);

        Assert.Equal(first, replaced);
        Assert.Equal(second, equipment.GetEquipped(EquipmentSlot.Weapon));
    }

    [Theory]
    [InlineData("sling", WeaponFamily.Sling)]
    [InlineData("short_bow", WeaponFamily.Bow)]
    [InlineData("light_crossbow", WeaponFamily.Crossbow)]
    public void Equipment_EquipsLaunchersInBowSlot(string id, WeaponFamily family)
    {
        var equipment = new Equipment();
        var item = new ItemInstance($"character-1:item:{id}", id);
        var definition = new ItemDefinition(id, ItemCategory.Weapon, WeaponHandling.Ranged, family, WeaponClass.Launcher);

        equipment.Equip(EquipmentSlot.Bow, item, definition);

        Assert.Equal(item, equipment.GetEquipped(EquipmentSlot.Bow));
    }

    [Fact]
    public void Equipment_RejectsAmmunitionAsEquippableWeapon()
    {
        var equipment = new Equipment();
        var item = new ItemInstance("character-1:item:arrow", "arrow");
        var definition = new ItemDefinition("arrow", ItemCategory.Weapon, WeaponHandling.Ammunition, WeaponFamily.Arrow, WeaponClass.Ammunition);

        Assert.Throws<InvalidOperationException>(() => equipment.Equip(EquipmentSlot.Weapon, item, definition));
    }
}