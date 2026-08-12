using IronHell.Core.Characters;
using IronHell.Core.Definitions;
using Xunit;

namespace IronHell.Core.Tests.Characters;

/// <summary>
/// Acceptance tests for CharacterFactory.
/// All expected values are derived from races.json and classes.json.
/// No RNG. No stat rolling. No HP/mana generation.
/// </summary>
public sealed class CharacterFactoryTests
{
    // -------------------------------------------------------------------------
    // Shared test definitions (values sourced verbatim from JSON definitions)
    // -------------------------------------------------------------------------

    private static readonly IReadOnlyList<RaceClassRule> AllowedCombinations =
    [
        new("human",    "warrior"), new("human",    "mage"),   new("human",    "priest"),
        new("human",    "rogue"),   new("human",    "ranger"), new("human",    "paladin"),
        new("half_elf", "warrior"), new("half_elf", "mage"),   new("half_elf", "priest"),
        new("half_elf", "rogue"),   new("half_elf", "ranger"), new("half_elf", "paladin"),
        new("elf",      "warrior"), new("elf",      "mage"),   new("elf",      "rogue"),
        new("elf",      "ranger"),
        new("hobbit",   "warrior"), new("hobbit",   "rogue"),  new("hobbit",   "ranger"),
        new("gnome",    "warrior"), new("gnome",    "mage"),   new("gnome",    "priest"),
        new("gnome",    "rogue"),
        new("dwarf",    "warrior"), new("dwarf",    "priest"), new("dwarf",    "paladin"),
        new("half_orc", "warrior"), new("half_orc", "priest"), new("half_orc", "rogue"),
        new("half_troll","warrior"),new("half_troll","priest"),
        new("dunadan",  "warrior"), new("dunadan",  "mage"),   new("dunadan",  "priest"),
        new("dunadan",  "ranger"),  new("dunadan",  "paladin"),
        new("high_elf", "warrior"), new("high_elf", "mage"),   new("high_elf", "priest"),
        new("high_elf", "ranger"),
        new("kobold",   "warrior"), new("kobold",   "priest"), new("kobold",   "rogue"),
        new("kobold",   "ranger"),
    ];

    // -- Races --

    private static RaceDefinition Human() => new(
        Id: "human", Name: "Human",
        StatModifiers:  new(0,  0,  0,  0,  0,  0),
        SkillModifiers: new(0,  0,  0,  0,  0, 10,  0,  0),
        HitDie: 10, ExpFactor: 100,
        Infravision: 0, HistoryChart: 1,
        CapabilityIds: []);

    private static RaceDefinition HighElf() => new(
        Id: "high_elf", Name: "High-Elf",
        StatModifiers:  new( 1,  3, -1,  3,  1,  5),
        SkillModifiers: new( 4, 20, 20,  3,  3, 14, 10, 25),
        HitDie: 10, ExpFactor: 200,
        Infravision: 4, HistoryChart: 7,
        CapabilityIds: ["res_lite", "see_invis"]);

    private static RaceDefinition Dwarf() => new(
        Id: "dwarf", Name: "Dwarf",
        StatModifiers:  new( 2, -3,  2, -2,  2, -3),
        SkillModifiers: new( 2,  9,  9, -1,  7, 10, 15,  0),
        HitDie: 11, ExpFactor: 120,
        Infravision: 5, HistoryChart: 16,
        CapabilityIds: ["res_blind"]);

    // -- Classes --

    private static ClassDefinition Warrior() => new(
        Id: "warrior", Name: "Warrior",
        StatModifiers:  new( 5, -2, -2,  2,  2, -1),
        BaseSkills:     new(25, 18, 18,  1, 14,  2, 70, 55),
        SkillGrowth:    new(10,  7, 10,  0,  0,  0, 45, 45),
        HitDie: 9, ExpFactor: 0,
        SpellStat: null, FirstSpellLevel: 0, SpellWeight: 0,
        MaxAttacks: 6, MinWeight: 30, AttackMultiplier: 5,
        SenseBase: 9000, SenseDiv: 40,
        CapabilityIds: ["bravery_30", "pseudo_id_heavy", "pseudo_id_improv"],
        StartingEquipment:
        [
            new("broad_sword",              1, 1),
            new("chain_mail",               1, 1),
            new("potion_of_berserk_strength", 1, 1),
            new("ration_of_food",           3, 7),
            new("wooden_torch",             3, 7),
            new("scroll_of_word_of_recall", 1, 1),
        ]);

    private static ClassDefinition Mage() => new(
        Id: "mage", Name: "Mage",
        StatModifiers:  new(-5,  3,  0,  1, -2,  1),
        BaseSkills:     new(30, 36, 30,  2, 16, 20, 34, 20),
        SkillGrowth:    new( 7, 13,  9,  0,  0,  0, 15, 15),
        HitDie: 0, ExpFactor: 30,
        SpellStat: "intelligence", FirstSpellLevel: 1, SpellWeight: 300,
        MaxAttacks: 4, MinWeight: 40, AttackMultiplier: 2,
        SenseBase: 240000, SenseDiv: 5,
        CapabilityIds: ["cumber_glove", "zero_fail", "beam", "choose_spells", "hp_bonus"],
        StartingEquipment:
        [
            new("magic_for_beginners",           1, 1),
            new("dagger",                        1, 1),
            new("potion_of_cure_critical_wounds", 1, 1),
            new("ration_of_food",                3, 7),
            new("wooden_torch",                  3, 7),
            new("scroll_of_word_of_recall",      1, 1),
        ]);

    private static ClassDefinition Paladin() => new(
        Id: "paladin", Name: "Paladin",
        StatModifiers:  new( 3, -3,  1,  0,  2,  2),
        BaseSkills:     new(20, 24, 25,  1, 12,  2, 68, 40),
        SkillGrowth:    new( 7, 10, 11,  0,  0,  0, 35, 30),
        HitDie: 6, ExpFactor: 35,
        SpellStat: "wisdom", FirstSpellLevel: 1, SpellWeight: 400,
        MaxAttacks: 5, MinWeight: 30, AttackMultiplier: 5,
        SenseBase: 80000, SenseDiv: 40,
        CapabilityIds: ["pseudo_id_heavy", "pseudo_id_improv"],
        StartingEquipment:
        [
            new("beginners_handbook",            1, 1),
            new("broad_sword",                   1, 1),
            new("scroll_of_protection_from_evil", 1, 1),
            new("ration_of_food",                3, 7),
            new("wooden_torch",                  3, 7),
            new("scroll_of_word_of_recall",      1, 1),
        ]);

    // -------------------------------------------------------------------------
    // Acceptance test 1 — Human Warrior
    // Combined stats:   STR=5  INT=-2  WIS=-2  DEX=2  CON=2  CHR=-1
    // Combined skills:  DIS=25 DEV=18  SAV=18  STL=1  SRH=14 FOS=12 THN=70 THB=55
    // HitDie=19  ExpFact=100
    // -------------------------------------------------------------------------

    [Fact]
    public void HumanWarrior_StatModifiers_AreCorrect()
    {
        var state = CharacterFactory.Create(Human(), Warrior(), AllowedCombinations);

        Assert.Equal( 5, state.CombinedStatModifiers.Strength);
        Assert.Equal(-2, state.CombinedStatModifiers.Intelligence);
        Assert.Equal(-2, state.CombinedStatModifiers.Wisdom);
        Assert.Equal( 2, state.CombinedStatModifiers.Dexterity);
        Assert.Equal( 2, state.CombinedStatModifiers.Constitution);
        Assert.Equal(-1, state.CombinedStatModifiers.Charisma);
    }

    [Fact]
    public void HumanWarrior_SkillValues_AreCorrect()
    {
        var state = CharacterFactory.Create(Human(), Warrior(), AllowedCombinations);

        Assert.Equal(25, state.BaseSkills.Disarming);
        Assert.Equal(18, state.BaseSkills.MagicDevice);
        Assert.Equal(18, state.BaseSkills.SavingThrow);
        Assert.Equal( 1, state.BaseSkills.Stealth);
        Assert.Equal(14, state.BaseSkills.Searching);
        Assert.Equal(12, state.BaseSkills.SearchFrequency);  // race=10 + class=2
        Assert.Equal(70, state.BaseSkills.MeleeToHit);
        Assert.Equal(55, state.BaseSkills.RangedToHit);
    }

    [Fact]
    public void HumanWarrior_CapabilityIds_AreCorrect()
    {
        var state = CharacterFactory.Create(Human(), Warrior(), AllowedCombinations);

        Assert.Equal(["bravery_30", "pseudo_id_heavy", "pseudo_id_improv"], state.CapabilityIds);
    }

    [Fact]
    public void HumanWarrior_StartingEquipment_IsCorrect()
    {
        var state = CharacterFactory.Create(Human(), Warrior(), AllowedCombinations);

        Assert.Equal(6, state.StartingEquipment.Count);
        Assert.Equal(new StartingEquipmentEntry("broad_sword",               1, 1), state.StartingEquipment[0]);
        Assert.Equal(new StartingEquipmentEntry("chain_mail",                1, 1), state.StartingEquipment[1]);
        Assert.Equal(new StartingEquipmentEntry("potion_of_berserk_strength", 1, 1), state.StartingEquipment[2]);
        Assert.Equal(new StartingEquipmentEntry("ration_of_food",            3, 7), state.StartingEquipment[3]);
        Assert.Equal(new StartingEquipmentEntry("wooden_torch",              3, 7), state.StartingEquipment[4]);
        Assert.Equal(new StartingEquipmentEntry("scroll_of_word_of_recall",  1, 1), state.StartingEquipment[5]);
    }

    [Fact]
    public void HumanWarrior_HitDieAndExpFact_AreCorrect()
    {
        var state = CharacterFactory.Create(Human(), Warrior(), AllowedCombinations);

        Assert.Equal(19,  state.HitDie);   // race=10 + class=9
        Assert.Equal(100, state.ExpFact);  // race=100 + class=0
    }

    // -------------------------------------------------------------------------
    // Acceptance test 2 — High-Elf Mage
    // Combined stats:   STR=-4  INT=6  WIS=-1  DEX=4  CON=-1  CHR=6
    // Combined skills:  DIS=34 DEV=56  SAV=50  STL=5  SRH=19 FOS=34 THN=44 THB=45
    // HitDie=10  ExpFact=230
    // -------------------------------------------------------------------------

    [Fact]
    public void HighElfMage_StatModifiers_AreCorrect()
    {
        var state = CharacterFactory.Create(HighElf(), Mage(), AllowedCombinations);

        Assert.Equal(-4, state.CombinedStatModifiers.Strength);
        Assert.Equal( 6, state.CombinedStatModifiers.Intelligence);
        Assert.Equal(-1, state.CombinedStatModifiers.Wisdom);
        Assert.Equal( 4, state.CombinedStatModifiers.Dexterity);
        Assert.Equal(-1, state.CombinedStatModifiers.Constitution);
        Assert.Equal( 6, state.CombinedStatModifiers.Charisma);
    }

    [Fact]
    public void HighElfMage_SkillValues_AreCorrect()
    {
        var state = CharacterFactory.Create(HighElf(), Mage(), AllowedCombinations);

        Assert.Equal(34, state.BaseSkills.Disarming);       // 4 + 30
        Assert.Equal(56, state.BaseSkills.MagicDevice);     // 20 + 36
        Assert.Equal(50, state.BaseSkills.SavingThrow);     // 20 + 30
        Assert.Equal( 5, state.BaseSkills.Stealth);         // 3 + 2
        Assert.Equal(19, state.BaseSkills.Searching);       // 3 + 16
        Assert.Equal(34, state.BaseSkills.SearchFrequency); // 14 + 20
        Assert.Equal(44, state.BaseSkills.MeleeToHit);      // 10 + 34
        Assert.Equal(45, state.BaseSkills.RangedToHit);     // 25 + 20
    }

    [Fact]
    public void HighElfMage_CapabilityIds_AreCorrect()
    {
        var state = CharacterFactory.Create(HighElf(), Mage(), AllowedCombinations);

        // Race capabilities first, then class; no duplicates
        Assert.Equal(
            ["res_lite", "see_invis", "cumber_glove", "zero_fail", "beam", "choose_spells", "hp_bonus"],
            state.CapabilityIds);
    }

    [Fact]
    public void HighElfMage_StartingEquipment_IsCorrect()
    {
        var state = CharacterFactory.Create(HighElf(), Mage(), AllowedCombinations);

        Assert.Equal(6, state.StartingEquipment.Count);
        Assert.Equal(new StartingEquipmentEntry("magic_for_beginners",            1, 1), state.StartingEquipment[0]);
        Assert.Equal(new StartingEquipmentEntry("dagger",                         1, 1), state.StartingEquipment[1]);
        Assert.Equal(new StartingEquipmentEntry("potion_of_cure_critical_wounds",  1, 1), state.StartingEquipment[2]);
        Assert.Equal(new StartingEquipmentEntry("ration_of_food",                 3, 7), state.StartingEquipment[3]);
        Assert.Equal(new StartingEquipmentEntry("wooden_torch",                   3, 7), state.StartingEquipment[4]);
        Assert.Equal(new StartingEquipmentEntry("scroll_of_word_of_recall",       1, 1), state.StartingEquipment[5]);
    }

    [Fact]
    public void HighElfMage_HitDieAndExpFact_AreCorrect()
    {
        var state = CharacterFactory.Create(HighElf(), Mage(), AllowedCombinations);

        Assert.Equal(10,  state.HitDie);   // race=10 + class=0
        Assert.Equal(230, state.ExpFact);  // race=200 + class=30
    }

    // -------------------------------------------------------------------------
    // Acceptance test 3 — Dwarf Paladin
    // Combined stats:   STR=5  INT=-6  WIS=3  DEX=-2  CON=4  CHR=-1
    // Combined skills:  DIS=22 DEV=33  SAV=34  STL=0  SRH=19 FOS=12 THN=83 THB=40
    // HitDie=17  ExpFact=155
    // -------------------------------------------------------------------------

    [Fact]
    public void DwarfPaladin_StatModifiers_AreCorrect()
    {
        var state = CharacterFactory.Create(Dwarf(), Paladin(), AllowedCombinations);

        Assert.Equal( 5, state.CombinedStatModifiers.Strength);
        Assert.Equal(-6, state.CombinedStatModifiers.Intelligence);
        Assert.Equal( 3, state.CombinedStatModifiers.Wisdom);
        Assert.Equal(-2, state.CombinedStatModifiers.Dexterity);
        Assert.Equal( 4, state.CombinedStatModifiers.Constitution);
        Assert.Equal(-1, state.CombinedStatModifiers.Charisma);
    }

    [Fact]
    public void DwarfPaladin_SkillValues_AreCorrect()
    {
        var state = CharacterFactory.Create(Dwarf(), Paladin(), AllowedCombinations);

        Assert.Equal(22, state.BaseSkills.Disarming);       // 2 + 20
        Assert.Equal(33, state.BaseSkills.MagicDevice);     // 9 + 24
        Assert.Equal(34, state.BaseSkills.SavingThrow);     // 9 + 25
        Assert.Equal( 0, state.BaseSkills.Stealth);         // -1 + 1
        Assert.Equal(19, state.BaseSkills.Searching);       // 7 + 12
        Assert.Equal(12, state.BaseSkills.SearchFrequency); // 10 + 2
        Assert.Equal(83, state.BaseSkills.MeleeToHit);      // 15 + 68
        Assert.Equal(40, state.BaseSkills.RangedToHit);     // 0 + 40
    }

    [Fact]
    public void DwarfPaladin_CapabilityIds_AreCorrect()
    {
        var state = CharacterFactory.Create(Dwarf(), Paladin(), AllowedCombinations);

        Assert.Equal(["res_blind", "pseudo_id_heavy", "pseudo_id_improv"], state.CapabilityIds);
    }

    [Fact]
    public void DwarfPaladin_StartingEquipment_IsCorrect()
    {
        var state = CharacterFactory.Create(Dwarf(), Paladin(), AllowedCombinations);

        Assert.Equal(6, state.StartingEquipment.Count);
        Assert.Equal(new StartingEquipmentEntry("beginners_handbook",             1, 1), state.StartingEquipment[0]);
        Assert.Equal(new StartingEquipmentEntry("broad_sword",                    1, 1), state.StartingEquipment[1]);
        Assert.Equal(new StartingEquipmentEntry("scroll_of_protection_from_evil",  1, 1), state.StartingEquipment[2]);
        Assert.Equal(new StartingEquipmentEntry("ration_of_food",                 3, 7), state.StartingEquipment[3]);
        Assert.Equal(new StartingEquipmentEntry("wooden_torch",                   3, 7), state.StartingEquipment[4]);
        Assert.Equal(new StartingEquipmentEntry("scroll_of_word_of_recall",       1, 1), state.StartingEquipment[5]);
    }

    [Fact]
    public void DwarfPaladin_HitDieAndExpFact_AreCorrect()
    {
        var state = CharacterFactory.Create(Dwarf(), Paladin(), AllowedCombinations);

        Assert.Equal(17,  state.HitDie);   // race=11 + class=6
        Assert.Equal(155, state.ExpFact);  // race=120 + class=35
    }

    // -------------------------------------------------------------------------
    // Invalid combination guard
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_ForbiddenCombination_ThrowsInvalidOperationException()
    {
        // Dwarf cannot be a Mage per race_class_rules.json
        var ex = Assert.Throws<InvalidOperationException>(
            () => CharacterFactory.Create(Dwarf(), Mage(), AllowedCombinations));

        Assert.Contains("dwarf", ex.Message);
        Assert.Contains("mage", ex.Message);
    }

    // -------------------------------------------------------------------------
    // Capability deduplication
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_OverlappingCapabilities_DeduplicatesPreservingRaceFirst()
    {
        // Half-Troll has sust_str and regen; neither Warrior nor Priest share those.
        // Craft a race that shares one cap with the class to verify dedup.
        var race = new RaceDefinition(
            "gnome", "Gnome",
            new(-1, 2, 0, 2, 1, -2),
            new(10, 12, 12, 3, 6, 13, -8, 12),
            HitDie: 8, ExpFactor: 125,
            Infravision: 4, HistoryChart: 13,
            CapabilityIds: ["free_act", "res_fire"]);  // res_fire appears in class too

        var @class = new ClassDefinition(
            "warrior", "Warrior",
            new(5, -2, -2, 2, 2, -1),
            new(25, 18, 18, 1, 14, 2, 70, 55),
            new(10, 7, 10, 0, 0, 0, 45, 45),
            HitDie: 9, ExpFactor: 0,
            SpellStat: null, FirstSpellLevel: 0, SpellWeight: 0,
            MaxAttacks: 6, MinWeight: 30, AttackMultiplier: 5,
            SenseBase: 9000, SenseDiv: 40,
            CapabilityIds: ["res_fire", "bravery_30"],  // res_fire is duplicate
            StartingEquipment: []);

        var rules = new[] { new RaceClassRule("gnome", "warrior") };
        var state = CharacterFactory.Create(race, @class, rules);

        // res_fire appears once (from race position), free_act from race, bravery_30 from class
        Assert.Equal(["free_act", "res_fire", "bravery_30"], state.CapabilityIds);
    }
}
