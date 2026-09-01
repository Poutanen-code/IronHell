using IronHell.Core.Definitions;
using IronHell.Data.Registries;

namespace IronHell.Data.Validation;

internal sealed record CharacterDefinitionSet(
    IReadOnlyCollection<RaceClassRule> Rules,
    IReadOnlyCollection<RaceDefinition> Races,
    IReadOnlyCollection<ClassDefinition> Classes);

internal static class CharacterValidator
{
    private const string CapabilityIdsProperty = "capability_ids";
    private const string UnknownCapabilityError = "unknown_capability";

    public static void Validate(
        IDefinitionRegistry<RaceDefinition> races,
        IDefinitionRegistry<ClassDefinition> classes,
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        IDefinitionRegistry<ItemDefinition> items,
        CharacterDefinitionSet characterDefinitions,
        DefinitionValidationReport report)
    {
        foreach (var rule in characterDefinitions.Rules)
        {
            if (!races.TryGet(rule.RaceId, out _)) report.Add("character/race_class_rules.json", rule.RaceId, "race_id", "unknown_race", "Race reference does not resolve.");
            if (!classes.TryGet(rule.ClassId, out _)) report.Add("character/race_class_rules.json", rule.ClassId, "class_id", "unknown_class", "Class reference does not resolve.");
        }

        ValidateCapabilityReferences(characterDefinitions.Races, capabilities, report);
        ValidateCapabilityReferences(characterDefinitions.Classes, capabilities, report);
        ValidateStartingEquipment(characterDefinitions.Classes, items, report);
    }

    private static void ValidateStartingEquipment(
        IEnumerable<ClassDefinition> classDefinitions,
        IDefinitionRegistry<ItemDefinition> items,
        DefinitionValidationReport report)
    {
        foreach (var @class in classDefinitions)
        {
            foreach (var equipment in @class.StartingEquipment)
            {
                if (equipment.Min > equipment.Max)
                {
                    report.Add("character/classes.json", @class.Id, "starting_equipment", "invalid_quantity_range", $"Starting equipment '{equipment.Id}' has minimum greater than maximum.");
                }

                if (!items.TryGet(equipment.Id, out _))
                {
                    report.Add("character/classes.json", @class.Id, "starting_equipment", "unknown_item", $"Starting equipment '{equipment.Id}' does not resolve.");
                }
            }
        }
    }

    private static void ValidateCapabilityReferences<T>(
        IEnumerable<T> definitions,
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        foreach (var definition in definitions)
        {
            var capabilityIds = definition switch
            {
                RaceDefinition race => race.CapabilityIds,
                ClassDefinition @class => @class.CapabilityIds,
                _ => throw new InvalidOperationException($"Unsupported capability definition type '{typeof(T).Name}'."),
            };
            foreach (var capabilityId in capabilityIds)
            {
                if (!capabilities.TryGet(capabilityId, out _)) report.Add("character", definition.Id, CapabilityIdsProperty, UnknownCapabilityError, $"Capability '{capabilityId}' does not resolve.");
            }
        }
    }
}
