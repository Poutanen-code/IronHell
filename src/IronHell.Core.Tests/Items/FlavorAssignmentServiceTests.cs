using System.Diagnostics.CodeAnalysis;
using IronHell.Core.Definitions;
using IronHell.Core.Items;
using IronHell.Core.Randomness;
using Xunit;

namespace IronHell.Core.Tests.Items;

public sealed class FlavorAssignmentServiceTests
{
    [Fact]
    public void Constructor_SameSeed_ProducesIdenticalAssignments()
    {
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("ring_a", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_b", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_c", ItemCategory.Ring, "ring"),
        ]);
        var flavors = ThreeRingFlavors();

        var first = new FlavorAssignmentService(items, flavors, new FlavorSeed(42));
        var second = new FlavorAssignmentService(items, flavors, new FlavorSeed(42));

        Assert.Equal(first.Assignments, second.Assignments);
    }

    [Fact]
    public void Constructor_DifferentSeed_ProducesDifferentAssignments()
    {
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("ring_a", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_b", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_c", ItemCategory.Ring, "ring"),
        ]);
        var flavors = ThreeRingFlavors();

        var first = new FlavorAssignmentService(items, flavors, new FlavorSeed(1));
        var second = new FlavorAssignmentService(items, flavors, new FlavorSeed(987654));

        Assert.NotEqual(first.Assignments, second.Assignments);
    }

    [Fact]
    public void Constructor_AssignsOnlyFlavorsFromTheMatchingCategory()
    {
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("ring_a", ItemCategory.Ring, "ring"),
            new ItemDefinition("amulet_a", ItemCategory.Amulet, "amulet"),
        ]);
        var flavors = new TestRegistry<FlavorDefinition>([
            new FlavorDefinition("ring_ruby", FlavorCategory.Ring, "Ruby", "=", "r", new FlavorLegacyMetadata(28, 45, null), "verified"),
            new FlavorDefinition("amulet_amber", FlavorCategory.Amulet, "Amber", "\"", "y", new FlavorLegacyMetadata(44, 40, null), "verified"),
        ]);

        var service = new FlavorAssignmentService(items, flavors, new FlavorSeed(7));

        Assert.Equal("ring_ruby", service.GetFlavorForItemKind("ring_a").Id);
        Assert.Equal("amulet_amber", service.GetFlavorForItemKind("amulet_a").Id);
    }

    [Fact]
    public void Constructor_ItemsNotEligibleForFlavors_AreIgnored()
    {
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("sword_a", ItemCategory.Weapon, "sword"),
        ]);
        var flavors = new TestRegistry<FlavorDefinition>([]);

        var service = new FlavorAssignmentService(items, flavors, new FlavorSeed(1));

        Assert.Empty(service.Assignments);
        Assert.False(service.TryGetFlavorForItemKind("sword_a", out _));
    }

    [Fact]
    public void Constructor_NoFlavorsForRequiredCategory_ThrowsAggregatedError()
    {
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("ring_a", ItemCategory.Ring, "ring"),
        ]);
        var flavors = new TestRegistry<FlavorDefinition>([]);

        var exception = Assert.Throws<InvalidOperationException>(() => new FlavorAssignmentService(items, flavors, new FlavorSeed(1)));
        Assert.Contains("Ring", exception.Message);
    }

    [Fact]
    public void Constructor_MorePoolFlavorsThanItems_NoDuplicateAssignments()
    {
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("ring_a", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_b", ItemCategory.Ring, "ring"),
        ]);
        var flavors = ThreeRingFlavors();

        var service = new FlavorAssignmentService(items, flavors, new FlavorSeed(11));

        var assignedFlavorIds = service.Assignments.Select(assignment => assignment.FlavorDefinitionId).ToArray();
        Assert.Equal(assignedFlavorIds.Length, assignedFlavorIds.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Constructor_MoreItemsThanPoolFlavors_RepeatsOnlyAfterFullPass()
    {
        var items = new TestRegistry<ItemDefinition>([
            new ItemDefinition("ring_a", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_b", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_c", ItemCategory.Ring, "ring"),
            new ItemDefinition("ring_d", ItemCategory.Ring, "ring"),
        ]);
        var flavors = new TestRegistry<FlavorDefinition>([
            new FlavorDefinition("ring_ruby", FlavorCategory.Ring, "Ruby", "=", "r", new FlavorLegacyMetadata(28, 45, null), "verified"),
            new FlavorDefinition("ring_jade", FlavorCategory.Ring, "Jade", "=", "G", new FlavorLegacyMetadata(16, 45, null), "verified"),
        ]);

        var service = new FlavorAssignmentService(items, flavors, new FlavorSeed(5));

        var assignedFlavorIds = service.Assignments.Select(assignment => assignment.FlavorDefinitionId).ToArray();
        Assert.Equal(assignedFlavorIds[0], assignedFlavorIds[2]);
        Assert.Equal(assignedFlavorIds[1], assignedFlavorIds[3]);
        Assert.NotEqual(assignedFlavorIds[0], assignedFlavorIds[1]);
    }

    [Fact]
    public void GetFlavorForItemKind_UnknownItem_Throws()
    {
        var service = new FlavorAssignmentService(new TestRegistry<ItemDefinition>([]), new TestRegistry<FlavorDefinition>([]), new FlavorSeed(1));

        Assert.Throws<KeyNotFoundException>(() => service.GetFlavorForItemKind("missing"));
    }

    [Fact]
    public void ItemInstanceFactory_Create_ResolvesFlavorIdFromAssignment()
    {
        var items = new TestRegistry<ItemDefinition>([new ItemDefinition("ring_a", ItemCategory.Ring, "ring")]);
        var flavors = ThreeRingFlavors();
        var service = new FlavorAssignmentService(items, flavors, new FlavorSeed(3));
        var definition = items.GetRequired("ring_a");

        var instance = ItemInstanceFactory.Create("char-1:item:1", definition, service);

        Assert.Equal(service.GetFlavorForItemKind("ring_a").Id, instance.FlavorId);
    }

    [Fact]
    public void ItemInstanceFactory_Create_WithoutAssignmentService_LeavesFlavorIdNull()
    {
        var definition = new ItemDefinition("ring_a", ItemCategory.Ring, "ring");

        var instance = ItemInstanceFactory.Create("char-1:item:1", definition);

        Assert.Null(instance.FlavorId);
    }

    private static TestRegistry<FlavorDefinition> ThreeRingFlavors() => new(
    [
        new FlavorDefinition("ring_ruby", FlavorCategory.Ring, "Ruby", "=", "r", new FlavorLegacyMetadata(28, 45, null), "verified"),
        new FlavorDefinition("ring_jade", FlavorCategory.Ring, "Jade", "=", "G", new FlavorLegacyMetadata(16, 45, null), "verified"),
        new FlavorDefinition("ring_opal", FlavorCategory.Ring, "Opal", "=", "W", new FlavorLegacyMetadata(23, 45, null), "verified"),
    ]);

    private sealed class TestRegistry<T>(IReadOnlyCollection<T> all) : IDefinitionRegistry<T>
        where T : IIdentifiedDefinition
    {
        public IReadOnlyCollection<T> All { get; } = all;

        public bool TryGet(string id, [NotNullWhen(true)] out T? definition)
        {
            definition = All.FirstOrDefault(item => item.Id == id);
            return definition is not null;
        }

        public T GetRequired(string id) => All.First(item => item.Id == id);
    }
}
