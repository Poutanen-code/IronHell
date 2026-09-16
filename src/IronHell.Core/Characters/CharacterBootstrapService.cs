using IronHell.Core.Definitions;
using IronHell.Core.Items;
using IronHell.Core.World;

namespace IronHell.Core.Characters;

public static class CharacterBootstrapService
{
    public static Character Create(
        IDefinitionCatalog catalog,
        string characterId,
        string raceId,
        string classId,
        WorldRuntimeState? world = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);

        var race = catalog.Races.GetRequired(raceId);
        var @class = catalog.Classes.GetRequired(classId);
        var state = CharacterFactory.Create(race, @class, catalog.RaceClassRules);
        var character = new Character(characterId, state.RaceId, state.ClassId);
        character.State.MaxHp = state.HitDie;
        character.State.CurrentHp = character.State.MaxHp;

        var instanceNumber = 0;
        foreach (var equipment in state.StartingEquipment)
        {
            var definition = catalog.Items.GetRequired(equipment.Id);
            for (var count = 0; count < equipment.Min; count++)
            {
                instanceNumber++;
                character.Inventory.Add(ItemInstanceFactory.Create($"{characterId}:item:{instanceNumber}", definition, world?.FlavorAssignments));
            }
        }

        return character;
    }
}