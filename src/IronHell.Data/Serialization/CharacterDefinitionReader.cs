using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class CharacterDefinitionReader
{
    private const string CapabilityIdsProperty = "capability_ids";

    public static List<RaceDefinition> ReadRaces(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<RaceDefinition>();
        foreach (var entry in document["races"]?.AsArray() ?? [])
        {
            if (entry is not JsonObject race)
            {
                continue;
            }

            var id = RequiredString(race, "id", "races.json", report);
            if (id is null)
            {
                continue;
            }

            definitions.Add(new RaceDefinition(
                id, RequiredString(race, "name", "races.json", report) ?? string.Empty,
                ReadStats(race["stat_modifiers"]), ReadSkills(race["skill_modifiers"]),
                ReadInt(race, "hit_die"), ReadInt(race, "exp_factor"), ReadInt(race, "infravision"), ReadInt(race, "history_chart"), ReadIds(race, CapabilityIdsProperty)));
        }

        ValidationHelpers.ValidateDuplicates("races", definitions, report);
        return definitions;
    }

    public static List<ClassDefinition> ReadClasses(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<ClassDefinition>();
        foreach (var entry in document["classes"]?.AsArray() ?? [])
        {
            if (entry is not JsonObject @class)
            {
                continue;
            }

            var id = RequiredString(@class, "id", "classes.json", report);
            if (id is null)
            {
                continue;
            }

            definitions.Add(new ClassDefinition(
                id, RequiredString(@class, "name", "classes.json", report) ?? string.Empty,
                ReadStats(@class["stat_modifiers"]), ReadSkills(@class["base_skills"]), ReadSkills(@class["skill_growth"]),
                ReadInt(@class, "hit_die"), ReadInt(@class, "exp_factor"), @class["spell_stat"]?.GetValue<string>(),
                ReadInt(@class, "first_spell_level"), ReadInt(@class, "spell_weight"), ReadInt(@class, "max_attacks"), ReadInt(@class, "min_weight"),
                ReadInt(@class, "attack_multiplier"), ReadInt(@class, "sense_base"), ReadInt(@class, "sense_div"),
                ReadIds(@class, CapabilityIdsProperty), ReadEquipment(@class)));
        }

        ValidationHelpers.ValidateDuplicates("classes", definitions, report);
        return definitions;
    }

    public static List<RaceClassRule> ReadRules(JsonObject document, DefinitionValidationReport report)
    {
        var rules = new List<RaceClassRule>();
        foreach (var entry in document["race_class_rules"]?.AsArray() ?? [])
        {
            if (entry is not JsonObject rule)
            {
                continue;
            }

            var raceId = RequiredString(rule, "race_id", "race_class_rules.json", report);
            var classId = RequiredString(rule, "class_id", "race_class_rules.json", report);
            if (raceId is not null && classId is not null)
            {
                rules.Add(new RaceClassRule(raceId, classId));
            }
        }

        var duplicate = rules.GroupBy(rule => (rule.RaceId, rule.ClassId)).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            report.Add("character/race_class_rules.json", duplicate.Key.RaceId, "$.race_class_rules", "duplicate_id", $"Duplicate class rule for '{duplicate.Key.RaceId}' and '{duplicate.Key.ClassId}'.");
        }

        return rules;
    }

    private static string? RequiredString(JsonObject value, string property, string document, DefinitionValidationReport report)
    {
        var result = value[property]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(result)) return result;
        report.Add(document, null, property, "missing_property", $"'{property}' is required.");
        return null;
    }

    private static int ReadInt(JsonObject value, string property) => value[property]?.GetValue<int>() ?? 0;

    private static IReadOnlyList<string> ReadIds(JsonObject value, string property) => Array.AsReadOnly((value[property]?.AsArray() ?? []).Select(entry => entry?.GetValue<string>() ?? string.Empty).ToArray());

    private static IReadOnlyList<StartingEquipmentEntry> ReadEquipment(JsonObject value) => Array.AsReadOnly((value["starting_equipment"]?.AsArray() ?? []).Select(entry => new StartingEquipmentEntry(entry?["id"]?.GetValue<string>() ?? string.Empty, entry?["min"]?.GetValue<int>() ?? 0, entry?["max"]?.GetValue<int>() ?? 0)).ToArray());

    private static StatModifiers ReadStats(JsonNode? value) => new(value?["strength"]?.GetValue<int>() ?? 0, value?["intelligence"]?.GetValue<int>() ?? 0, value?["wisdom"]?.GetValue<int>() ?? 0, value?["dexterity"]?.GetValue<int>() ?? 0, value?["constitution"]?.GetValue<int>() ?? 0, value?["charisma"]?.GetValue<int>() ?? 0);

    private static SkillSet ReadSkills(JsonNode? value) => new(value?["disarming"]?.GetValue<int>() ?? 0, value?["magic_device"]?.GetValue<int>() ?? 0, value?["saving_throw"]?.GetValue<int>() ?? 0, value?["stealth"]?.GetValue<int>() ?? 0, value?["searching"]?.GetValue<int>() ?? 0, value?["search_frequency"]?.GetValue<int>() ?? 0, value?["melee_to_hit"]?.GetValue<int>() ?? 0, value?["ranged_to_hit"]?.GetValue<int>() ?? 0);
}
