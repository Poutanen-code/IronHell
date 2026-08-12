**Prayers Compendium — MAngband (reference 1.5.3)**

Source notes:
- Prayer → book mapping and prayer names: [reference-mangband-1_5_3/src/server/x-spell.c](reference-mangband-1_5_3/src/server/x-spell.c)
- Priest class prayer base data (slevel, smana, sfail): [reference-mangband-1_5_3/lib/edit/p_class.txt](reference-mangband-1_5_3/lib/edit/p_class.txt)
- Book names, allocation and dungeon spawn: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt)
- Store table (which shops stock which prayer svals): [reference-mangband-1_5_3/src/server/init2.c](reference-mangband-1_5_3/src/server/init2.c)
- Live failure formula: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c)

How to read this file:
- Each book section lists the object `sval` and the `W:`/`A:` allocation lines (object.txt). `A:` shows dungeon depth/rarity.
- Store availability is derived from `store_table` (counts out of `STORE_CHOICES = 32`) in `init2.c` for the Temple. If a book is not listed there it is not sold in shops by default.
- Each prayer row shows: **Target** (self / directional / item / area / projectile), **Effect** (from `get_spell_info` or `cast_priest_spell`), and Priest base: `slevel` / `smana` / `sfail` (from `p_class.txt` B: entries).
- Actual failure% is computed by `spell_chance()` in `cmd5.c`: base = `sfail` - 3*(plev - slevel) - stat_adj; minfail = `adj_mag_fail[...]` (with `ZERO_FAIL` class flag exceptions); stun and wield penalties apply; final clamped to [minfail..95]. See `cmd5.c` for details.

-------------------------------------------------------------------------------

Book 0 — Beginners Handbook (sval 0)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L2907)
- W:5:0:30:25  A:5/1 (dungeon alloc depth 5, rarity 1)
- Store availability (Temple): appears 3 times in the Temple table (3/32 ≈ 9.4%). See [init2.c Temple entry](reference-mangband-1_5_3/src/server/init2.c#L1168-L1225).

Prayers:
- Detect Evil (PRAYER_DETECT_EVIL, id=0)
  - Target: self
  - Effect: detect_evil()
  - Priest base: slevel 1 / smana 1 / sfail 10

- Cure Light Wounds (PRAYER_CURE_LIGHT_WOUNDS, id=1)
  - Target: self (also projectable as heal projectile)
  - Effect: heal 2d10
  - Priest base: slevel 1 / smana 2 / sfail 15

- Bless (PRAYER_BLESS, id=2)
  - Target: self
  - Effect: dur 12+d12
  - Priest base: slevel 1 / smana 2 / sfail 20

- Remove Fear (PRAYER_REMOVE_FEAR, id=3)
  - Target: self
  - Effect: clears fear (set_afraid = 0)
  - Priest base: slevel 1 / smana 2 / sfail 25

- Call Light (PRAYER_CALL_LIGHT, id=4)
  - Target: area (centered on caster)
  - Effect: lite_area(damroll(2, plev/2), (plev/10)+1)
  - Priest base: slevel 3 / smana 2 / sfail 25

- Find Traps (PRAYER_FIND_TRAPS, id=5)
  - Target: self
  - Effect: detect_trap()
  - Priest base: slevel 3 / smana 3 / sfail 27

- Detect Doors/Stairs (PRAYER_DETECT_DOORS_STAIRS, id=6)
  - Target: self
  - Effect: detect_sdoor()
  - Priest base: slevel 3 / smana 3 / sfail 27

- Slow Poison (PRAYER_SLOW_POISON, id=7)
  - Target: self
  - Effect: halves current poison (set_poisoned = poisoned/2)
  - Priest base: slevel 3 / smana 3 / sfail 28

-------------------------------------------------------------------------------

Book 1 — Words of Wisdom (sval 1)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L2915)
- W:10:0:30:100  A:10/1
- Store availability (Temple): appears 2 times in the Temple table (2/32 ≈ 6.25%). See [init2.c Temple entry](reference-mangband-1_5_3/src/server/init2.c#L1168-L1225).

Prayers:
- Scare Monster (PRAYER_SCARE_MONSTER, id=8)
  - Target: directional (requires aim)
  - Effect: fear_monster(dir, plev)
  - Priest base: slevel 5 / smana 4 / sfail 29

- Portal (PRAYER_PORTAL, id=9)
  - Target: self
  - Effect: teleport_player(range = 3 * plev)
  - Priest base: slevel 5 / smana 4 / sfail 30

- Cure Serious Wounds (PRAYER_CURE_SERIOUS_WOUNDS, id=10)
  - Target: self (also projectable)
  - Effect: heal 4d10
  - Priest base: slevel 5 / smana 4 / sfail 32

- Chant (PRAYER_CHANT, id=11)
  - Target: self
  - Effect: dur 24+d24 (blessing-like buff)
  - Priest base: slevel 5 / smana 5 / sfail 34

- Sanctuary (PRAYER_SANCTUARY, id=12)
  - Target: self (brief area effect)
  - Effect: sleep_monsters_touch (area pacification)
  - Priest base: slevel 7 / smana 5 / sfail 36

- Satisfy Hunger (PRAYER_SATISFY_HUNGER, id=13)
  - Target: self
  - Effect: set_food(PY_FOOD_MAX - 1)
  - Priest base: slevel 7 / smana 5 / sfail 38

- Remove Curse (PRAYER_REMOVE_CURSE, id=14)
  - Target: self
  - Effect: remove_curse()
  - Priest base: slevel 7 / smana 6 / sfail 38

- Resist Heat/Cold (PRAYER_RESIST_HEAT_COLD, id=15)
  - Target: self
  - Effect: set_oppose_fire/cold for 10+d10
  - Priest base: slevel 7 / smana 7 / sfail 38

-------------------------------------------------------------------------------

Book 2 — Chants and Blessings (sval 2)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L2923)
- W:20:0:30:300  A:20/1
- Store availability: not listed in `store_table` (Temple only stocks svals 0..3). This book spawns in dungeons only (see `A:` above).

Prayers:
- Neutralize Poison (PRAYER_NEUTRALIZE_POISON, id=16)
  - Target: self
  - Effect: set_poisoned(p_ptr, 0)
  - Priest base: slevel 9 / smana 6 / sfail 38

- Orb of Draining (PRAYER_ORB_OF_DRAINING, id=17)
  - Target: directional (requires aim)
  - Effect: fire_ball(GF_HOLY_ORB, dir, damage = plev + (plev / (CF_BLESS_WEAPON?2:4)) + 3d6)
  - Priest base: slevel 9 / smana 7 / sfail 40

- Cure Critical Wounds (PRAYER_CURE_CRITICAL_WOUNDS, id=18)
  - Target: self (also projectable)
  - Effect: heal 6d10
  - Priest base: slevel 9 / smana 7 / sfail 38

- Sense Invisible (PRAYER_SENSE_INVISIBLE, id=19)
  - Target: self
  - Effect: tim_invis += 24+d24
  - Priest base: slevel 11 / smana 8 / sfail 42

- Protection from Evil (PRAYER_PROTECTION_FROM_EVIL, id=20)
  - Target: self
  - Effect: protevil dur %d+d25 (3*plev base)
  - Priest base: slevel 11 / smana 8 / sfail 42

- Earthquake (PRAYER_EARTHQUAKE, id=21)
  - Target: self / area
  - Effect: earthquake(Depth, py, px, 10)
  - Priest base: slevel 11 / smana 9 / sfail 55

- Sense Surroundings (PRAYER_SENSE_SURROUNDINGS, id=22)
  - Target: self
  - Effect: map_area()
  - Priest base: slevel 13 / smana 10 / sfail 45

- Cure Mortal Wounds (PRAYER_CURE_MORTAL_WOUNDS, id=23)
  - Target: self (also projectable)
  - Effect: heal 8d10
  - Priest base: slevel 13 / smana 11 / sfail 45

- Turn Undead (PRAYER_TURN_UNDEAD, id=24)
  - Target: self / area (turn undead effect)
  - Priest base: slevel 15 / smana 12 / sfail 50

-------------------------------------------------------------------------------

Book 3 — Exorcism and Dispelling (sval 3)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L2931)
- W:30:0:30:900  A:30/1
- Store availability (Temple): appears 1 time in the Temple table (1/32 ≈ 3.125%). See [init2.c Temple entry](reference-mangband-1_5_3/src/server/init2.c#L1168-L1225).

Prayers:
- Prayer (PRAYER_PRAYER, id=25)
  - Target: self
  - Effect: dur 48+d48 (mass blessing-like effect)
  - Priest base: slevel 15 / smana 14 / sfail 50

- Dispel Undead (PRAYER_DISPEL_UNDEAD, id=26)
  - Target: none / area
  - Effect: dispel_undead(randint1(plev * 3)) — also shown as dmg d(3*plev) in info
  - Priest base: slevel 17 / smana 14 / sfail 55

- Heal (PRAYER_HEAL, id=27)
  - Target: self (also projectable)
  - Effect: heal 300
  - Priest base: slevel 21 / smana 16 / sfail 60

- Dispel Evil (PRAYER_DISPEL_EVIL, id=28)
  - Target: none / area
  - Effect: dispel_evil(randint1(plev * 3)) — shown as dmg d(3*plev) in info
  - Priest base: slevel 25 / smana 20 / sfail 70

- Glyph of Warding (PRAYER_GLYPH_OF_WARDING, id=29)
  - Target: self (warding rune)
  - Effect: warding_glyph()
  - Priest base: slevel 33 / smana 55 / sfail 90

- Holy Word (PRAYER_HOLY_WORD, id=30)
  - Target: self / area
  - Effect: dispel_evil(randint1(plev * 4)), heal 1000, clear negative status
  - Priest base: slevel 39 / smana 32 / sfail 95

-------------------------------------------------------------------------------

Book 4 — Ethereal Openings (sval 4)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L3374)
- W:40:0:30:5000  A:40/1
- Store availability: not sold in `store_table` — dungeon only (see `A:`).

Prayers:
- Blink (PRAYER_BLINK, id=52)
  - Target: self
  - Effect: teleport_player(range 10)
  - Priest base: slevel 35 / smana 50 / sfail 80

- Teleport Self (PRAYER_TELEPORT_SELF, id=53)
  - Target: self
  - Effect: teleport_player(range = 8 * plev)
  - Priest base: slevel 35 / smana 70 / sfail 90

- Teleport Other (PRAYER_TELEPORT_OTHER, id=54)
  - Target: directional (requires aim)
  - Effect: teleport_monster(dir)
  - Priest base: slevel 15 / smana 7 / sfail 70

- Teleport Level (PRAYER_TELEPORT_LEVEL, id=55)
  - Target: self
  - Effect: teleport_player_level()
  - Priest base: slevel 45 / smana 95 / sfail 85

- Word of Recall (PRAYER_WORD_OF_RECALL, id=56)
  - Target: self (uses the book to set recall)
  - Effect: set_recall(o_ptr)
  - Priest base: slevel 3 / smana 3 / sfail 50

- Alter Reality (PRAYER_ALTER_REALITY, id=57)
  - Target: self
  - Effect: alter_reality(FALSE)
  - Priest base: slevel 10 / smana 10 / sfail 80

-------------------------------------------------------------------------------

Book 5 — Godly Insights (sval 5)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L3383)
- W:50:0:30:10000  A:50/1
- Store availability: not sold in `store_table` — dungeon only.

Prayers:
- Detect Monsters (PRAYER_DETECT_MONSTERS, id=31)
  - Target: self
  - Effect: detect_creatures(TRUE)
  - Priest base: slevel 20 / smana 20 / sfail 80

- Detection (PRAYER_DETECTION, id=32)
  - Target: self
  - Effect: detection() / detect_all
  - Priest base: slevel 33 / smana 55 / sfail 90

- Perception (PRAYER_PERCEPTION, id=33)
  - Target: item (identifies items via ident_spell)
  - Effect: ident_spell()
  - Priest base: slevel 20 / smana 20 / sfail 80

- Probing (PRAYER_PROBING, id=34)
  - Target: self
  - Effect: probing()
  - Priest base: slevel 25 / smana 40 / sfail 80

- Clairvoyance (PRAYER_CLAIRVOYANCE, id=35)
  - Target: self
  - Effect: wiz_lite() / reveal level
  - Priest base: slevel 35 / smana 50 / sfail 80

-------------------------------------------------------------------------------

Book 6 — Purifications and Healing (sval 6)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L3392)
- W:60:0:30:30000  A:60/1
- Store availability: not sold in `store_table` — dungeon only.

Prayers:
- Cure Serious Wounds (PRAYER_CURE_SERIOUS_WOUNDS2, id=36)
  - Target: self (projectable)
  - Effect: heal 4d10
  - Priest base: slevel 30 / smana 40 / sfail 80

- Cure Mortal Wounds (PRAYER_CURE_MORTAL_WOUNDS2, id=37)
  - Target: self (projectable)
  - Effect: heal 8d10
  - Priest base: slevel 35 / smana 50 / sfail 80

- Healing (PRAYER_HEALING, id=38)
  - Target: self (projectable)
  - Effect: heal 2000
  - Priest base: slevel 15 / smana 5 / sfail 50

- Restoration (PRAYER_RESTORATION, id=39)
  - Target: self
  - Effect: stat restores / resurrection (variant-specific)
  - Priest base: slevel 17 / smana 7 / sfail 60

- Remembrance (PRAYER_REMEMBRANCE, id=40)
  - Target: self
  - Effect: restore_level() / restore XP to others (variant-specific)
  - Priest base: slevel 30 / smana 40 / sfail 80

-------------------------------------------------------------------------------

Book 7 — Holy Infusions (sval 7)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L3401)
- W:80:0:30:50000  A:80/2
- Store availability: not sold in `store_table` — dungeon only.

Prayers:
- Unbarring Ways (PRAYER_UNBARRING_WAYS, id=46)
  - Target: self / touch
  - Effect: destroy_doors_touch()
  - Priest base: slevel 5 / smana 6 / sfail 50

- Recharging (PRAYER_RECHARGING, id=47)
  - Target: item
  - Effect: recharge(item, 15)
  - Priest base: slevel 15 / smana 20 / sfail 80

- Dispel Curse (PRAYER_DISPEL_CURSE, id=48)
  - Target: self
  - Effect: remove_all_curse()
  - Priest base: slevel 25 / smana 40 / sfail 80

- Enchant Weapon (PRAYER_ENCHANT_WEAPON, id=49)
  - Target: item
  - Effect: enchant_spell(... weapon ...)
  - Priest base: slevel 35 / smana 50 / sfail 80

- Enchant Armor (PRAYER_ENCHANT_ARMOUR, id=50)
  - Target: item
  - Effect: enchant_spell(... armor ...)
  - Priest base: slevel 37 / smana 60 / sfail 85

- Elemental Brand (PRAYER_ELEMENTAL_BRAND, id=51)
  - Target: item (weapon)
  - Effect: brand_weapon(TRUE)
  - Priest base: slevel 45 / smana 95 / sfail 85

-------------------------------------------------------------------------------

Book 8 — Wrath of God (sval 8)
- Object entry: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L3410)
- W:100:0:30:100000  A:100/4
- Store availability: not sold in `store_table` — dungeon only.

Prayers:
- Dispel Undead (PRAYER_DISPEL_UNDEAD2, id=41)
  - Target: none / area
  - Effect: dispel_undead(randint1(plev * 4))
  - Priest base: slevel 35 / smana 70 / sfail 90

- Dispel Evil (PRAYER_DISPEL_EVIL2, id=42)
  - Target: none / area
  - Effect: dispel_evil(randint1(plev * 4))
  - Priest base: slevel 45 / smana 60 / sfail 75

- Banish Evil (PRAYER_BANISH_EVIL, id=43)
  - Target: none / area
  - Effect: banish_evil(p_ptr, 100)
  - Priest base: slevel 25 / smana 25 / sfail 80

- Word of Destruction (PRAYER_WORD_OF_DESTRUCTION, id=44)
  - Target: none / area
  - Effect: destroy_area(Depth, py, px, 15, TRUE)
  - Priest base: slevel 35 / smana 35 / sfail 80

- Annihilation (PRAYER_ANNIHILATION, id=45)
  - Target: directional (requires aim)
  - Effect: drain_life(dir, 200)
  - Priest base: slevel 45 / smana 60 / sfail 75

-------------------------------------------------------------------------------

Notes / next steps:
- I used `spell_list` and `spell_names` in `x-spell.c` to map prayers → books, `get_spell_info` + `cast_priest_spell` for effects, and `p_class.txt` B: entries for base `slevel/smana/sfail`.
- If you want a CSV/JSON export with one row per prayer (id,name,book_sval,target,effect,slevel,smana,sfail,store_count,alloc_depth,alloc_rarity), tell me which format; I can generate and add it to the repo.
