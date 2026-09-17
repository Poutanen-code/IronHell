using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Validation;

namespace IronHell.Data.Serialization;

internal static class MonsterDefinitionReader
{
    public static List<MonsterAbilityDefinition> ReadAbilities(JsonObject document, DefinitionValidationReport report) =>
        SimpleDefinitionReader.Read<MonsterAbilityDefinition>(document, "abilities", "id", id => new MonsterAbilityDefinition(id), report);

    public static List<MonsterCapabilityDefinition> ReadCapabilities(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<MonsterCapabilityDefinition>();
        foreach (var capability in document["capabilities"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = capability["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add("monster_capabilities.json", null, "$.capabilities", "missing_id", "Definition identifier is required.");
                continue;
            }

            definitions.Add(new MonsterCapabilityDefinition(
                id,
                capability["name"]?.GetValue<string>() ?? string.Empty,
                capability["description"]?.GetValue<string>() ?? string.Empty));
        }

        ValidationHelpers.ValidateDuplicates("monster_capabilities", definitions, report);
        return definitions;
    }

    public static List<MonsterDefinition> ReadMonsters(JsonObject document, DefinitionValidationReport report)
    {
        var definitions = new List<MonsterDefinition>();
        foreach (var monster in document["monsters"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = monster["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add("monsters/monsters.json", null, "$.monsters", "missing_id", "Definition identifier is required.");
                continue;
            }

            var hpRoll = monster["hp_roll"]?.AsObject();
            var ai = monster["ai"]?.AsObject();
            var capabilities = monster["capabilities"]?.AsArray()
                .Select(capability => capability?.GetValue<string>() ?? string.Empty)
                .ToArray() ?? [];
            var resistances = monster["resistances"]?.AsArray()
                .Select(resistance => resistance?.GetValue<string>() ?? string.Empty)
                .ToArray() ?? [];
            var senses = monster["senses"]?.AsObject();
            var spawnPolicy = monster["spawn_policy"] as JsonObject;
            definitions.Add(new MonsterDefinition(id, new DiceRollDefinition(
                hpRoll?["kind"]?.GetValue<string>() ?? string.Empty,
                hpRoll?["count"]?.GetValue<int>() ?? 0,
                hpRoll?["sides"]?.GetValue<int>() ?? 0),
                new MonsterAiDefinition(
                    ai?["behavior"]?.GetValue<string>() ?? string.Empty,
                    ai?["random_move_chance"]?.GetValue<int>() ?? 0,
                    ai?["stupid"]?.GetValue<bool>() ?? false,
                    ai?["smart"]?.GetValue<bool>() ?? false),
                Array.AsReadOnly(capabilities),
                Array.AsReadOnly(resistances),
                new MonsterSensesDefinition(
                    senses?["alertness"]?.GetValue<int>() ?? 0,
                    ParseTelepathyProfile(senses?["telepathy_profile"]?.GetValue<string>())),
                new SpawnPolicy(
                    ReadBoolean(spawnPolicy, "unique"),
                    ReadBoolean(spawnPolicy, "questor"),
                    ReadBoolean(spawnPolicy, "force_depth"),
                    ReadBoolean(spawnPolicy, "force_max_hp"),
                    ReadBoolean(spawnPolicy, "force_sleep"),
                    ReadBoolean(spawnPolicy, "escort"),
                    ReadBoolean(spawnPolicy, "escorts"),
                    ReadBoolean(spawnPolicy, "friends"),
                    ReadBoolean(spawnPolicy, "wanderer"))));
        }

        ValidationHelpers.ValidateDuplicates("monsters", definitions, report);
        return definitions;
    }

    private static MonsterTelepathyProfile ParseTelepathyProfile(string? profile) => profile switch
    {
        "weird_mind" => MonsterTelepathyProfile.WeirdMind,
        "empty_mind" => MonsterTelepathyProfile.EmptyMind,
        _ => MonsterTelepathyProfile.Normal,
    };

    private static bool ReadBoolean(JsonObject? policy, string property) =>
        policy?[property] is JsonValue value && value.TryGetValue<bool>(out var result) && result;
}
