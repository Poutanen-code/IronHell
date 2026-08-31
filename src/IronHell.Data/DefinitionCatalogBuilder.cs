using System.Collections.Frozen;
using System.Text.Json.Nodes;
using IronHell.Core.Definitions;
using IronHell.Data.Registries;
using IronHell.Data.Validation;

namespace IronHell.Data;

internal static class DefinitionCatalogBuilder
{
    private const string StatusIdProperty = "status_id";
    private const string ActionIdProperty = "action_id";
    private const string CapabilityIdsProperty = "capability_ids";
    private const string UnknownStatusError = "unknown_status";
    private const string MagicRealm = "magic";
    private const string CapabilitiesCatalog = "capabilities";
    private const string ResistancesCatalog = "resistances";
    private const string ActivationsCatalog = "activations";
    private const string ActivationsDocument = "activations.json";
    private const string ActivationIdProperty = "activation_id";
    private const string ResistanceIdProperty = "resistance_id";
    private const string UnknownResistanceError = "unknown_resistance";
    private const string UnknownCapabilityError = "unknown_capability";
    private const string CapabilityIdProperty = "capability_id";
    private const string ResistanceIdsProperty = "resistance_ids";
    private const string MonsterAbilitiesDocument = "monsters/monster_abilities.json";
    private const string MonstersCatalog = "monsters";
    private const string MonstersDocument = "monsters/monsters.json";
    private const string MonsterAbilitiesCatalog = "monster_abilities";
    private const string AbilitiesProperty = "abilities";
    private const string TerrainCatalog = "terrain";
    private const string TerrainDefinitionsProperty = "terrain_definitions";
    private const string TerrainDocument = "environment/terrain_definitions.json";

    private sealed record ValidationRegistries(
        IDefinitionRegistry<ActionDefinition> Actions,
        IDefinitionRegistry<StatusDefinition> Statuses,
        IDefinitionRegistry<CapabilityDefinition> Capabilities,
        IDefinitionRegistry<ResistanceDefinition> Resistances,
        IDefinitionRegistry<ItemDefinition> Items,
        IDefinitionRegistry<SpellDefinition> MageSpells,
        IDefinitionRegistry<SpellDefinition> PriestPrayers,
        IDefinitionRegistry<ActivationDefinition> Activations,
        IDefinitionRegistry<MonsterAbilityDefinition> MonsterAbilities,
        IDefinitionRegistry<MonsterDefinition> Monsters,
        IDefinitionRegistry<TerrainDefinition> Terrain);

    private static readonly (string DocumentName, string CollectionName, ItemCategory Category)[] ItemCatalogs =
    [
        ("weapons", "weapons", ItemCategory.Weapon),
        ("armor", "armor", ItemCategory.Armor),
        ("lights", "lights", ItemCategory.Light),
        ("consumables", "consumables", ItemCategory.Consumable),
        ("potions", "potions", ItemCategory.Potion),
        ("scrolls", "scrolls", ItemCategory.Scroll),
        ("spell_books", "books", ItemCategory.SpellBook),
    ];

    private sealed record CharacterDefinitionSet(
        IReadOnlyCollection<RaceClassRule> Rules,
        IReadOnlyCollection<RaceDefinition> Races,
        IReadOnlyCollection<ClassDefinition> Classes);

    public static IDefinitionCatalog? Build(FrozenDictionary<string, JsonObject> documents, DefinitionValidationReport report)
    {
        var actions = ReadSimpleDefinitions<ActionDefinition>(documents["actions"], "actions", ActionIdProperty, id => new ActionDefinition(id), report);
        var statuses = ReadSimpleDefinitions<StatusDefinition>(documents["statuses"], "statuses", StatusIdProperty, id => new StatusDefinition(id), report);
        var capabilities = ReadSimpleDefinitions<CapabilityDefinition>(documents[CapabilitiesCatalog], CapabilitiesCatalog, "id", id => new CapabilityDefinition(id), report);
        var resistances = ReadSimpleDefinitions<ResistanceDefinition>(documents[ResistancesCatalog], ResistancesCatalog, "id", id => new ResistanceDefinition(id), report);
        var items = ReadItems(documents, report);
        var mageSpells = ReadSimpleDefinitions<SpellDefinition>(documents["mage_spells"], "spells", "id", id => new SpellDefinition(id), report);
        var priestPrayers = ReadSimpleDefinitions<SpellDefinition>(documents["priest_prayers"], "spells", "id", id => new SpellDefinition(id), report);
        var activations = ReadSimpleDefinitions<ActivationDefinition>(documents[ActivationsCatalog], ActivationsCatalog, ActivationIdProperty, id => new ActivationDefinition(id), report);
        var monsterAbilities = ReadSimpleDefinitions<MonsterAbilityDefinition>(documents[MonsterAbilitiesCatalog], AbilitiesProperty, "id", id => new MonsterAbilityDefinition(id), report);
        var monsters = ReadSimpleDefinitions<MonsterDefinition>(documents[MonstersCatalog], MonstersCatalog, "id", id => new MonsterDefinition(id), report);
        var terrain = ReadSimpleDefinitions<TerrainDefinition>(documents[TerrainCatalog], TerrainDefinitionsProperty, "id", id => new TerrainDefinition(id), report);
        var races = ReadRaces(documents["races"], report);
        var classes = ReadClasses(documents["classes"], report);
        var rules = ReadRules(documents["race_class_rules"], report);

        if (report.HasErrors)
        {
            return null;
        }

        var actionRegistry = new DefinitionRegistry<ActionDefinition>(actions);
        var statusRegistry = new DefinitionRegistry<StatusDefinition>(statuses);
        var capabilityRegistry = new DefinitionRegistry<CapabilityDefinition>(capabilities);
        var resistanceRegistry = new DefinitionRegistry<ResistanceDefinition>(resistances);
        var raceRegistry = new DefinitionRegistry<RaceDefinition>(races);
        var classRegistry = new DefinitionRegistry<ClassDefinition>(classes);
        var itemRegistry = new DefinitionRegistry<ItemDefinition>(items);
        var mageSpellRegistry = new DefinitionRegistry<SpellDefinition>(mageSpells);
        var priestPrayerRegistry = new DefinitionRegistry<SpellDefinition>(priestPrayers);
        var activationRegistry = new DefinitionRegistry<ActivationDefinition>(activations);
        var monsterAbilityRegistry = new DefinitionRegistry<MonsterAbilityDefinition>(monsterAbilities);
        var monsterRegistry = new DefinitionRegistry<MonsterDefinition>(monsters);
        var terrainRegistry = new DefinitionRegistry<TerrainDefinition>(terrain);

        var characterDefinitions = new CharacterDefinitionSet(rules, races, classes);
        ValidateReferences(raceRegistry, classRegistry, capabilityRegistry, itemRegistry, characterDefinitions, report);
        var validationRegistries = new ValidationRegistries(actionRegistry, statusRegistry, capabilityRegistry, resistanceRegistry, itemRegistry, mageSpellRegistry, priestPrayerRegistry, activationRegistry, monsterAbilityRegistry, monsterRegistry, terrainRegistry);
        ValidateCatalogReferences(documents, validationRegistries, report);
        if (report.HasErrors)
        {
            return null;
        }

        return new DefinitionCatalog(
            actionRegistry,
            statusRegistry,
            capabilityRegistry,
            resistanceRegistry,
            raceRegistry,
            classRegistry,
            itemRegistry,
            mageSpellRegistry,
            priestPrayerRegistry,
            activationRegistry,
            monsterAbilityRegistry,
            monsterRegistry,
            terrainRegistry,
            Array.AsReadOnly(rules.OrderBy(rule => rule.RaceId, StringComparer.Ordinal).ThenBy(rule => rule.ClassId, StringComparer.Ordinal).ToArray()));
    }

    private static List<ItemDefinition> ReadItems(FrozenDictionary<string, JsonObject> documents, DefinitionValidationReport report)
    {
        var items = new List<ItemDefinition>();
        foreach (var catalog in ItemCatalogs)
        {
            foreach (var entry in documents[catalog.DocumentName][catalog.CollectionName]?.AsArray() ?? [])
            {
                var id = entry?["id"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(id))
                {
                    report.Add($"items/{catalog.DocumentName}.json", null, "$.id", "missing_id", "Item identifier is required.");
                    continue;
                }

                ValidateItemCategory(entry, catalog, id, report);
                items.Add(new ItemDefinition(id, catalog.Category));
            }
        }

        ValidateDuplicates("items", items, report);
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

    private static List<T> ReadSimpleDefinitions<T>(JsonObject document, string collectionName, string idProperty, Func<string, T> factory, DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        var definitions = new List<T>();
        foreach (var entry in document[collectionName]?.AsArray() ?? [])
        {
            var id = entry?[idProperty]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id))
            {
                report.Add($"{collectionName}.json", null, $"$.{collectionName}", "missing_id", "Definition identifier is required.");
                continue;
            }

            definitions.Add(factory(id));
        }

        ValidateDuplicates(collectionName, definitions, report);
        return definitions;
    }

    private static List<RaceDefinition> ReadRaces(JsonObject document, DefinitionValidationReport report)
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

        ValidateDuplicates("races", definitions, report);
        return definitions;
    }

    private static List<ClassDefinition> ReadClasses(JsonObject document, DefinitionValidationReport report)
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

        ValidateDuplicates("classes", definitions, report);
        return definitions;
    }

    private static List<RaceClassRule> ReadRules(JsonObject document, DefinitionValidationReport report)
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

    private static void ValidateReferences(
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

    private static void ValidateCatalogReferences(
        FrozenDictionary<string, JsonObject> documents,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        ValidateCapabilityResistanceReferences(documents[CapabilitiesCatalog], registries.Resistances, report);
        ValidateResistanceStatusReferences(documents[ResistancesCatalog], registries.Statuses, report);
        ValidateStatusMigrationReferences(documents["statuses"], registries.Statuses, report);
        ValidateItemReferences(documents, registries.Actions, registries.Statuses, registries.Capabilities, registries.Resistances, report);
        ValidateSpellReferences(documents, registries, report);
        ValidateActivationReferences(documents[ActivationsCatalog], registries, report);
        ValidateMonsterAbilityReferences(documents[MonsterAbilitiesCatalog], registries, report);
        ValidateMonsterReferences(documents[MonstersCatalog], registries, report);
        ValidateTerrainReferences(documents[TerrainCatalog], registries.Terrain, report);
    }

    private static void ValidateCapabilityResistanceReferences(
        JsonObject document,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        DefinitionValidationReport report)
    {
        foreach (var capability in document["capabilities"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = capability["id"]?.GetValue<string>() ?? string.Empty;
            ValidateReference(capability[ResistanceIdProperty]?.GetValue<string>(), resistances, "capabilities.json", id, ResistanceIdProperty, UnknownResistanceError, report);
            if (capability["canonical_owner"]?.GetValue<string>() == "resistance")
            {
                ValidateReference(capability["migration_target_id"]?.GetValue<string>(), resistances, "capabilities.json", id, "migration_target_id", UnknownResistanceError, report);
            }
        }
    }

    private static void ValidateResistanceStatusReferences(
        JsonObject document,
        IDefinitionRegistry<StatusDefinition> statuses,
        DefinitionValidationReport report)
    {
        foreach (var resistance in document["resistances"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = resistance["id"]?.GetValue<string>() ?? string.Empty;
            ValidateReference(resistance[StatusIdProperty]?.GetValue<string>(), statuses, "resistances.json", id, StatusIdProperty, UnknownStatusError, report);
        }
    }

    private static void ValidateStatusMigrationReferences(
        JsonObject document,
        IDefinitionRegistry<StatusDefinition> statuses,
        DefinitionValidationReport report)
    {
        foreach (var status in document["statuses"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = status[StatusIdProperty]?.GetValue<string>() ?? string.Empty;
            ValidateReference(status["migration_target_status_id"]?.GetValue<string>(), statuses, "statuses.json", id, "migration_target_status_id", UnknownStatusError, report);
            foreach (var target in status["migration_target_status_ids"]?.AsArray() ?? [])
            {
                ValidateReference(target?.GetValue<string>(), statuses, "statuses.json", id, "migration_target_status_ids", UnknownStatusError, report);
            }
        }
    }

    private static void ValidateItemReferences(
        FrozenDictionary<string, JsonObject> documents,
        IDefinitionRegistry<ActionDefinition> actions,
        IDefinitionRegistry<StatusDefinition> statuses,
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        DefinitionValidationReport report)
    {
        foreach (var catalog in ItemCatalogs.Where(catalog => catalog.Category != ItemCategory.SpellBook))
        {
            foreach (var item in documents[catalog.DocumentName][catalog.CollectionName]?.AsArray().OfType<JsonObject>() ?? [])
            {
                var id = item["id"]?.GetValue<string>() ?? string.Empty;
                foreach (var capabilityId in item[CapabilityIdsProperty]?.AsArray() ?? [])
                {
                    ValidateCapabilityReference(capabilityId?.GetValue<string>(), capabilities, resistances, $"items/{catalog.DocumentName}.json", id, report);
                }

                foreach (var action in item["actions"]?.AsArray().OfType<JsonObject>() ?? [])
                {
                    ValidateActionReference(action, actions, statuses, $"items/{catalog.DocumentName}.json", id, report);
                }
            }
        }
    }

    private static void ValidateSpellReferences(
        FrozenDictionary<string, JsonObject> documents,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        ValidateSpellBookReferences(documents["spell_books"], registries.MageSpells, registries.PriestPrayers, report);
        ValidateSpellDefinitions(documents["mage_spells"], MagicRealm, "magic/mage_spells.json", registries, report);
        ValidateSpellDefinitions(documents["priest_prayers"], "prayer", "magic/priest_prayers.json", registries, report);
    }

    private static void ValidateSpellBookReferences(
        JsonObject document,
        IDefinitionRegistry<SpellDefinition> mageSpells,
        IDefinitionRegistry<SpellDefinition> priestPrayers,
        DefinitionValidationReport report)
    {
        foreach (var book in document["books"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var bookId = book["id"]?.GetValue<string>() ?? string.Empty;
            var spells = book["realm"]?.GetValue<string>() == "magic" ? mageSpells : priestPrayers;
            foreach (var spellId in book["spell_ids"]?.AsArray() ?? [])
            {
                ValidateReference(spellId?.GetValue<string>(), spells, "magic/spell_books.json", bookId, "spell_ids", "unknown_spell", report);
            }
        }
    }

    private static void ValidateSpellDefinitions(
        JsonObject document,
        string expectedRealm,
        string documentPath,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var spell in document["spells"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var spellId = spell["id"]?.GetValue<string>() ?? string.Empty;
            var policy = spell["policy"]?.AsObject();
            ValidateSpellPolicy(policy, expectedRealm, documentPath, spellId, registries.Items, report);

            foreach (var action in spell["action_refs"]?.AsArray().OfType<JsonObject>() ?? [])
            {
                ValidateActionReference(action, registries.Actions, registries.Statuses, documentPath, spellId, report);
            }
        }
    }

    private static void ValidateSpellPolicy(
        JsonObject? policy,
        string expectedRealm,
        string documentPath,
        string spellId,
        IDefinitionRegistry<ItemDefinition> items,
        DefinitionValidationReport report)
    {
        if (policy?["realm"]?.GetValue<string>() != expectedRealm)
        {
            report.Add(documentPath, spellId, "policy.realm", "invalid_spell_realm", "Spell policy realm does not match its catalog.");
        }

        var bookId = policy?["book_id"]?.GetValue<string>();
        if (!items.TryGet(bookId ?? string.Empty, out var book) || book.Category != ItemCategory.SpellBook)
        {
            report.Add(documentPath, spellId, "policy.book_id", "unknown_spell_book", $"Spell book '{bookId}' does not resolve to a spell book item.");
        }
    }

    private static void ValidateActivationReferences(
        JsonObject document,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var activation in document["activations"]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var activationId = activation[ActivationIdProperty]?.GetValue<string>() ?? string.Empty;
            ValidateActivationNode(activation, activationId, registries, report);
        }
    }

    private static void ValidateActivationNode(
        JsonNode? node,
        string activationId,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        if (node is JsonObject value)
        {
            if (value[ActionIdProperty] is not null)
            {
                ValidateActionReference(value, registries.Actions, registries.Statuses, ActivationsDocument, activationId, report);
            }

            ValidateReference(value[CapabilityIdProperty]?.GetValue<string>(), registries.Capabilities, ActivationsDocument, activationId, CapabilityIdProperty, UnknownCapabilityError, report);
            ValidateReference(value[ResistanceIdProperty]?.GetValue<string>(), registries.Resistances, ActivationsDocument, activationId, ResistanceIdProperty, UnknownResistanceError, report);
            ValidateActivationReferenceList(value[CapabilityIdsProperty]?.AsArray(), registries.Capabilities, activationId, CapabilityIdsProperty, UnknownCapabilityError, report);
            ValidateActivationReferenceList(value[ResistanceIdsProperty]?.AsArray(), registries.Resistances, activationId, ResistanceIdsProperty, UnknownResistanceError, report);

            foreach (var property in value)
            {
                ValidateActivationNode(property.Value, activationId, registries, report);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var arrayItem in array)
            {
                ValidateActivationNode(arrayItem, activationId, registries, report);
            }
        }
    }

    private static void ValidateActivationReferenceList<T>(
        JsonArray? references,
        IDefinitionRegistry<T> registry,
        string activationId,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        foreach (var reference in references ?? [])
        {
            ValidateReference(reference?.GetValue<string>(), registry, ActivationsDocument, activationId, property, errorCode, report);
        }
    }

    private static void ValidateMonsterAbilityReferences(
        JsonObject document,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var ability in document[AbilitiesProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var abilityId = ability["id"]?.GetValue<string>() ?? string.Empty;
            foreach (var action in ability["action_refs"]?.AsArray().OfType<JsonObject>() ?? [])
            {
                ValidateActionReference(action, registries.Actions, registries.Statuses, MonsterAbilitiesDocument, abilityId, report);
            }

            ValidateReference(ability[CapabilityIdProperty]?.GetValue<string>(), registries.Capabilities, MonsterAbilitiesDocument, abilityId, CapabilityIdProperty, UnknownCapabilityError, report);
            ValidateReference(ability[ResistanceIdProperty]?.GetValue<string>(), registries.Resistances, MonsterAbilitiesDocument, abilityId, ResistanceIdProperty, UnknownResistanceError, report);
            ValidateMonsterAbilityReferenceList(ability[CapabilityIdsProperty]?.AsArray(), registries.Capabilities, abilityId, CapabilityIdsProperty, UnknownCapabilityError, report);
            ValidateMonsterAbilityReferenceList(ability[ResistanceIdsProperty]?.AsArray(), registries.Resistances, abilityId, ResistanceIdsProperty, UnknownResistanceError, report);
        }
    }

    private static void ValidateMonsterAbilityReferenceList<T>(
        JsonArray? references,
        IDefinitionRegistry<T> registry,
        string abilityId,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        foreach (var reference in references ?? [])
        {
            ValidateReference(reference?.GetValue<string>(), registry, MonsterAbilitiesDocument, abilityId, property, errorCode, report);
        }
    }

    private static void ValidateMonsterReferences(
        JsonObject document,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        foreach (var monster in document[MonstersCatalog]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var monsterId = monster["id"]?.GetValue<string>() ?? string.Empty;
            foreach (var abilityId in monster[AbilitiesProperty]?.AsArray() ?? [])
            {
                ValidateReference(abilityId?.GetValue<string>(), registries.MonsterAbilities, MonstersDocument, monsterId, AbilitiesProperty, "unknown_monster_ability", report);
            }

            ValidateMonsterNode(monster, monsterId, registries, report);
        }
    }

    private static void ValidateMonsterNode(
        JsonNode? node,
        string monsterId,
        ValidationRegistries registries,
        DefinitionValidationReport report)
    {
        if (node is JsonObject value)
        {
            if (value[ActionIdProperty] is not null)
            {
                ValidateActionReference(value, registries.Actions, registries.Statuses, MonstersDocument, monsterId, report);
            }

            ValidateReference(value[CapabilityIdProperty]?.GetValue<string>(), registries.Capabilities, MonstersDocument, monsterId, CapabilityIdProperty, UnknownCapabilityError, report);
            ValidateReference(value[ResistanceIdProperty]?.GetValue<string>(), registries.Resistances, MonstersDocument, monsterId, ResistanceIdProperty, UnknownResistanceError, report);
            ValidateMonsterReferenceList(value[CapabilityIdsProperty]?.AsArray(), registries.Capabilities, monsterId, CapabilityIdsProperty, UnknownCapabilityError, report);
            ValidateMonsterReferenceList(value[ResistanceIdsProperty]?.AsArray(), registries.Resistances, monsterId, ResistanceIdsProperty, UnknownResistanceError, report);

            foreach (var property in value)
            {
                ValidateMonsterNode(property.Value, monsterId, registries, report);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                ValidateMonsterNode(item, monsterId, registries, report);
            }
        }
    }

    private static void ValidateMonsterReferenceList<T>(
        JsonArray? references,
        IDefinitionRegistry<T> registry,
        string monsterId,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        foreach (var reference in references ?? [])
        {
            ValidateReference(reference?.GetValue<string>(), registry, MonstersDocument, monsterId, property, errorCode, report);
        }
    }

    private static void ValidateTerrainReferences(
        JsonObject document,
        IDefinitionRegistry<TerrainDefinition> terrain,
        DefinitionValidationReport report)
    {
        foreach (var definition in document[TerrainDefinitionsProperty]?.AsArray().OfType<JsonObject>() ?? [])
        {
            var id = definition["id"]?.GetValue<string>() ?? string.Empty;
            ValidateReference(definition["appears_as"]?.GetValue<string>(), terrain, TerrainDocument, id, "appears_as", "unknown_terrain", report);
        }
    }

    private static void ValidateActionReference(
        JsonObject action,
        IDefinitionRegistry<ActionDefinition> actions,
        IDefinitionRegistry<StatusDefinition> statuses,
        string documentPath,
        string definitionId,
        DefinitionValidationReport report)
    {
        var actionId = action[ActionIdProperty]?.GetValue<string>();
        if (!ValidateReference(actionId, actions, documentPath, definitionId, "actions.action_id", "unknown_action", report))
        {
            return;
        }

        var parameters = action["parameters"]?.AsObject();
        ValidateReference(parameters?[StatusIdProperty]?.GetValue<string>(), statuses, documentPath, definitionId, "actions.parameters.status_id", UnknownStatusError, report);
        foreach (var statusId in parameters?["status_ids"]?.AsArray() ?? [])
        {
            ValidateReference(statusId?.GetValue<string>(), statuses, documentPath, definitionId, "actions.parameters.status_ids", UnknownStatusError, report);
        }
    }

    private static void ValidateCapabilityReference(
        string? id,
        IDefinitionRegistry<CapabilityDefinition> capabilities,
        IDefinitionRegistry<ResistanceDefinition> resistances,
        string documentPath,
        string definitionId,
        DefinitionValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(id) || capabilities.TryGet(id, out _) || resistances.TryGet(id, out _) || LegacyCapabilityIds.Contains(id))
        {
            return;
        }

        report.Add(documentPath, definitionId, CapabilityIdsProperty, UnknownCapabilityError, $"Capability '{id}' does not resolve.");
    }

    private static bool ValidateReference<T>(
        string? id,
        IDefinitionRegistry<T> registry,
        string documentPath,
        string definitionId,
        string property,
        string errorCode,
        DefinitionValidationReport report)
        where T : IIdentifiedDefinition
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return true;
        }

        if (registry.TryGet(id, out _))
        {
            return true;
        }

        report.Add(documentPath, definitionId, property, errorCode, $"Reference '{id}' does not resolve in {typeof(T).Name}.");
        return false;
    }

    private static readonly FrozenSet<string> LegacyCapabilityIds = new[]
    {
        "regeneration", "searching", "slay_undead", "stealth",
    }.ToFrozenSet(StringComparer.Ordinal);

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

    private static void ValidateDuplicates<T>(string catalog, IEnumerable<T> definitions, DefinitionValidationReport report) where T : IIdentifiedDefinition
    {
        foreach (var duplicateId in definitions.GroupBy(definition => definition.Id, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key))
        {
            report.Add($"{catalog}.json", duplicateId, "id", "duplicate_id", $"Duplicate identifier '{duplicateId}'.");
        }
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