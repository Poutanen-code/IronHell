using IronHell.Core.Actions;
using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using IronHell.Core.Randomness;
using IronHell.Core.Spells;
using Xunit;

namespace IronHell.Data.Tests;

public sealed class SpellExecutionBootstrapTests
{
    private const string CureLightWounds = "magic_cure_light_wounds";
    private const string Heroism = "magic_heroism";

    [Fact]
    public async Task Execute_RepositoryCureLightWounds_HealsCasterAndCuresCut()
    {
        var catalog = await LoadCatalogAsync();
        var caster = CreateWoundedCaster(catalog);
        StatusApplicationService.ApplyStatus(caster, "cut", 5, StatusApplicationPolicy.ReplaceExisting);

        var result = CreateService().Execute(catalog, CureLightWounds, caster);

        Assert.True(result.Success);
        Assert.True(caster.State.CurrentHp > 4);
        Assert.DoesNotContain(caster.ActiveStatuses, status => status.StatusId == "cut");
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent is HpChangedEvent);
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent is StatusRemovedEvent removed && removed.StatusId == "cut");
    }

    [Fact]
    public async Task Execute_RepositoryHeroism_AppliesHeroismStatus()
    {
        var catalog = await LoadCatalogAsync();
        var caster = CreateWoundedCaster(catalog);

        var result = CreateService().Execute(catalog, Heroism, caster);

        Assert.True(result.Success);
        Assert.Equal(14, caster.State.CurrentHp);
        Assert.Contains(caster.ActiveStatuses, status => status.StatusId == "heroism");
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent is StatusAppliedEvent applied && applied.StatusId == "heroism");
    }

    [Fact]
    public async Task Execute_RepositorySpellTwiceWithSameSeed_ProducesIdenticalResults()
    {
        var catalog = await LoadCatalogAsync();

        var firstCaster = CreateWoundedCaster(catalog);
        var firstResult = CreateService().Execute(catalog, CureLightWounds, firstCaster);

        var secondCaster = CreateWoundedCaster(catalog);
        var secondResult = CreateService().Execute(catalog, CureLightWounds, secondCaster);

        Assert.Equal(firstResult.AppliedChanges, secondResult.AppliedChanges);
        Assert.Equal(firstCaster.State.CurrentHp, secondCaster.State.CurrentHp);
        Assert.Equal(
            firstResult.Events.Select(runtimeEvent => runtimeEvent.GetType()),
            secondResult.Events.Select(runtimeEvent => runtimeEvent.GetType()));
    }

    [Fact]
    public async Task Execute_UnknownRepositorySpell_ReturnsValidationFailure()
    {
        var catalog = await LoadCatalogAsync();
        var caster = CreateWoundedCaster(catalog);

        var result = CreateService().Execute(catalog, "magic_not_a_spell", caster);

        Assert.False(result.Success);
        Assert.Equal(4, caster.State.CurrentHp);
        Assert.Empty(result.Events);
    }

    [Fact]
    public async Task LoadAsync_RepositorySpells_PublishActionReferences()
    {
        var catalog = await LoadCatalogAsync();

        var spell = catalog.MageSpells.GetRequired(CureLightWounds);

        Assert.NotNull(spell.ActionRefs);
        Assert.Collection(
            spell.ActionRefs,
            actionRef =>
            {
                Assert.Equal("HealHP", actionRef.ActionId);
                Assert.Equal(ActionTargetMode.Self, actionRef.TargetMode);
                Assert.Equal("dice", actionRef.Amount?.Kind);
                Assert.Equal(2, actionRef.Amount?.DiceCount);
                Assert.Equal(10, actionRef.Amount?.DiceSides);
            },
            actionRef =>
            {
                Assert.Equal("CureStatus", actionRef.ActionId);
                Assert.Equal("cut", Assert.Single(actionRef.StatusIds!));
            });
    }

    private static SpellExecutionService CreateService() => new(new ActionExecutor(), new SeededRandomSource(2024));

    private static Character CreateWoundedCaster(IDefinitionCatalog catalog)
    {
        var caster = CharacterBootstrapService.Create(catalog, "character-1", "human", "warrior");
        caster.State.CurrentHp = 4;
        return caster;
    }

    private static async Task<IDefinitionCatalog> LoadCatalogAsync()
    {
        var result = await DefinitionCatalogLoader.LoadAsync(RepositoryDefinitionsRoot);
        return Assert.IsType<DefinitionLoadSuccess>(result).Catalog;
    }

    private static string RepositoryDefinitionsRoot => Path.Combine(
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")),
        "data",
        "definitions");
}
