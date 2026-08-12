**Magic Compendium — MAngband (reference 1.5.3)**

Source notes:
- Spell → book mapping and spell names: [reference-mangband-1_5_3/src/server/x-spell.c](reference-mangband-1_5_3/src/server/x-spell.c)
- Mage class spell base data (slevel, smana, sfail): [lib/edit/p_class.txt](lib/edit/p_class.txt)
- Book names, allocation and store-level info: [lib/edit/object.txt](lib/edit/object.txt)
- Store table (which shops stock which book svals): [reference-mangband-1_5_3/src/server/init2.c](reference-mangband-1_5_3/src/server/init2.c)
- Spell failure formula: [reference-mangband-1_5_3/src/server/cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c)

Summary / how to read this compendium:
- Each book section lists the book `sval` (object subtype) and its name, dungeon allocation (`A:` lines) and shop occurrences in the built-in `store_table` (counts out of `STORE_CHOICES = 32`).
- Each spell entry shows: **Target** (directional/item/self/area), **Effect / damage** (from `get_spell_info` / implementation), and Mage base values: `slevel` / `smana` / `sfail` (from `p_class.txt` B: entries for `Mage`).
- Actual failure% uses the function `spell_chance()`: base = `sfail` minus `3*(plev - slevel)` minus stat adjustment `adj_mag_stat[...]`; minimum = `adj_mag_fail[...]` (unless class has `ZERO_FAIL`, in which case min may be <5); stun adds penalty; result capped between minfail..95. See [cmd5.c](reference-mangband-1_5_3/src/server/cmd5.c#L1-L200).

-------------------------------------------------------------------------------

Book 0 — Magic for Beginners (sval 0)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L2877)
- W: 5:0:30:25  A: 5/1  (allocation depth 5, rarity 1)
- Available in Magic-User store: appears 2 times in the store choice table (2/32 ≈ 6.25% per table entry).

Spells:
- Magic Missile (id 0)
  - Target: directional (aim)
  - Effect: dam (3 + floor((plev-1)/5)) d4 (see `get_spell_info`)
  - Mage base: slevel 1 / smana 1 / sfail 22

- Detect Monsters (1)
  - Target: self / area (detect_creatures)
  - Effect: detect creatures in radius
  - Mage base: slevel 1 / smana 1 / sfail 23

- Phase Door (2)
  - Target: self
  - Effect: teleport short range (range 10)
  - Mage base: slevel 1 / smana 2 / sfail 24

- Light Area (3)
  - Target: area (centered on caster)
  - Effect: `lite_area` with dice dependent on level
  - Mage base: slevel 1 / smana 2 / sfail 26

- Treasure Detection (6)
  - Target: self (detect treasure)
  - Effect: detect treasure/objects
  - Mage base: slevel 99 / smana 0 / sfail 0 (unavailable for Mage class by default)

- Cure Light Wounds (5)
  - Target: self
  - Effect: heal 2d8
  - Mage base: slevel 3 / smana 3 / sfail 25

- Detect Objects (7)
  - Target: self (detect objects)
  - Effect: detect objects
  - Mage base: slevel 99 / smana 0 / sfail 0 (unavailable for Mage class by default)

- Find Traps/Doors (4)
  - Target: self
  - Effect: detect traps and secret doors
  - Mage base: slevel 99 / smana 0 / sfail 0 (unavailable for Mage class by default)

- Stinking Cloud (11)
  - Target: directional (area projectile)
  - Effect: `fire_ball(GF_POIS)` with damage = 10 + floor(plev/2)
  - Mage base: slevel 3 / smana 3 / sfail 25

-------------------------------------------------------------------------------

Book 1 — Conjurings and Tricks (sval 1)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L2885)
- W: 10:0:30:100  A: 10/1
- Available in Magic-User store: appears 1 time in the store choice table (1/32).

Spells:
- Confuse Monster (13)
  - Target: directional
  - Effect: confuse_monster(dir, plev)
  - Mage base: slevel 5 / smana 4 / sfail 30

- Lightning Bolt (12)
  - Target: directional (beam/bolt)
  - Effect: dam d6 with dice = 3 + floor((plev-5)/6)
  - Mage base: slevel 5 / smana 4 / sfail 30

- Trap/Door Destruction (19)
  - Target: self / touch
  - Effect: destroy_doors_touch
  - Mage base: slevel 5 / smana 4 / sfail 30

- Cure Poison (25)
  - Target: self
  - Effect: clear poison
  - Mage base: slevel 5 / smana 5 / sfail 35

- Sleep Monster (14)
  - Target: directional
  - Effect: sleep_monster(dir)
  - Mage base: slevel 7 / smana 5 / sfail 30

- Teleport Self (30)
  - Target: self
  - Effect: teleport_player(range = 5 * plev)
  - Mage base: slevel 7 / smana 6 / sfail 40

- Spear of Light (20)
  - Target: directional (line)
  - Effect: `lite_line` (line-of-effect)
  - Mage base: slevel 9 / smana 7 / sfail 44

- Frost Bolt (16)
  - Target: directional
  - Effect: dam d8 with dice = 5 + floor((plev-5)/4)
  - Mage base: slevel 7 / smana 6 / sfail 40

- Wonder (15)
  - Target: directional
  - Effect: random multi-effect (see `spell_wonder` in x-spell.c)
  - Mage base: slevel 7 / smana 10 / sfail 50

-------------------------------------------------------------------------------

Book 2 — Incantations and Illusions (sval 2)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L2893)
- W: 20:0:30:400  A: 20/1
- Available in Magic-User store: appears 1 time in the store choice table (1/32).

Spells:
- Satisfy Hunger (26)
  - Target: self
  - Effect: set_food(PY_FOOD_MAX - 1)
  - Mage base: slevel 5 / smana 5 / sfail 35

- Lesser Recharging (50)
  - Target: item (prompts for item)
  - Effect: recharge(item, 2 + plev/5)
  - Mage base: slevel 9 / smana 7 / sfail 45

- Turn Stone to Mud (21)
  - Target: directional
  - Effect: wall_to_mud(dir)
  - Mage base: slevel 9 / smana 7 / sfail 45

- Fire Bolt (18)
  - Target: directional
  - Effect: dam d8 with dice = 6 + floor((plev-5)/4)
  - Mage base: slevel 10 / smana 7 / sfail 50

- Polymorph Other (35)
  - Target: directional
  - Effect: poly_monster(dir)
  - Mage base: slevel 11 / smana 7 / sfail 25

- Identify (8)
  - Target: item / self (identifies items; prompts user)
  - Effect: ident_spell
  - Mage base: slevel 11 / smana 7 / sfail 25

- Detect Invisible (9)
  - Target: self
  - Effect: detect_invisible(TRUE)
  - Mage base: slevel 15 / smana 5 / sfail 40

- Acid Bolt (17)
  - Target: directional
  - Effect: dam d8 with dice = 8 + floor((plev-5)/4)
  - Mage base: slevel 16 / smana 10 / sfail 40

- Slow Monster (31)
  - Target: directional
  - Effect: slow_monster(dir)
  - Mage base: slevel 17 / smana 9 / sfail 50

-------------------------------------------------------------------------------

Book 3 — Sorcery and Evocations (sval 3)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L2901)
- W: 30:0:30:800  A: 30/1
- Available in Magic-User store: appears 1 time in the store choice table (1/32).

Spells:
- Frost Ball (55)
  - Target: directional (area)
  - Effect: `fire_ball(GF_COLD, dir, 30 + plev, 2)`
  - Mage base: slevel 30 / smana 30 / sfail 95

- Teleport Other (32)
  - Target: directional (monster)
  - Effect: teleport_monster(dir)
  - Mage base: slevel 20 / smana 20 / sfail 50

- Haste Self (29)
  - Target: self
  - Effect: set_fast(randint1(20) + plev)
  - Mage base: slevel 7 / smana 10 / sfail 50

- Mass Sleep (39)
  - Target: none (affects many monsters)
  - Effect: sleep_monsters()
  - Mage base: slevel 20 / smana 20 / sfail 50

- Fire Ball (57)
  - Target: directional (area)
  - Effect: `fire_ball(GF_FIRE, dir, 55 + plev, 2)`
  - Mage base: slevel 32 / smana 24 / sfail 75

- Detect Enchantment (10)
  - Target: self
  - Effect: detect_objects_magic
  - Mage base: slevel 13 / smana 9 / sfail 40

-------------------------------------------------------------------------------

Book 4 — Resistances of Scarabtarices (sval 4)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L3329)
- W: 40:0:30:10000  A: 40/1

Spells:
- Resist Cold (44): dur 20+d20 — slevel 13 / smana 9 / sfail 40
- Resist Fire (45): dur 20+d20 — slevel 23 / smana 12 / sfail 60
- Resist Poison (46): dur 20+d20 — slevel 25 / smana 15 / sfail 60
- Resistance (47): dur 20+d20 — slevel 30 / smana 25 / sfail 75
- Shield (48): dur 30+d20 — slevel 11 / smana 7 / sfail 45

-------------------------------------------------------------------------------

Book 5 — Raal's Tome of Destruction (sval 5)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L3338)
- W: 50:0:30:20000  A: 50/1

Spells:
- Shock Wave (36) — directional area `fire_ball(GF_SOUND, dir, 10 + plev, 2)` — slevel 16 / smana 10 / sfail 40
- Explosion (37) — directional area `fire_ball(GF_SHARDS, dir, 20 + 2*plev, 2)` — slevel 16 / smana 10 / sfail 40
- Cloud Kill (38) — directional area `fire_ball(GF_POIS, dir, 40 + plev/2, 3)` — slevel 25 / smana 15 / sfail 60
- Acid Ball (56) — directional area `fire_ball(GF_ACID, dir, 40 + plev, 2)` — slevel 20 / smana 15 / sfail 70
- Ice Storm (58) — directional area `fire_ball(GF_ICE, dir, 50 + 2*plev, 3)` — slevel 27 / smana 22 / sfail 75
- Meteor Swarm (60) — directional swarm `fire_swarm(..., 30 + plev/2, ...)` — slevel 30 / smana 25 / sfail 85
- Rift (62) — directional beam `fire_beam(GF_GRAVITY, dir, 40 + damroll(plev,7))` — slevel 35 / smana 75 / sfail 90

-------------------------------------------------------------------------------

Book 6 — Mordenkainen's Escapes (sval 6)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L3347)
- W: 60:0:30:30000  A: 60/1

Spells:
- Door Creation (22) — door_creation() — slevel 28 / smana 17 / sfail 65
- Stair Creation (24) — stair_creation() — slevel 30 / smana 25 / sfail 75
- Teleport Level (33) — teleport_player_level() — slevel 11 / smana 7 / sfail 45
- Word of Recall (34) — uses your spellbook for recall — slevel 16 / smana 10 / sfail 40
- Rune of Protection (49) — warding_glyph() — slevel 25 / smana 30 / sfail 80

-------------------------------------------------------------------------------

Book 7 — Tenser's Transformations (sval 7)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L3356)
- W: 70:0:30:50000  A: 70/2

Spells:
- Heroism (27) — self: heal & hero dur — slevel 11 / smana 7 / sfail 45
- Berserker (28) — self: hp boost & shero dur — slevel 16 / smana 10 / sfail 40
- Enchant Armor (51) — item-target — slevel 20 / smana 20 / sfail 50
- Enchant Weapon (52) — item-target — slevel 20 / smana 20 / sfail 50
- Greater Recharging (53) — item-target recharge(...) — slevel 25 / smana 30 / sfail 80
- Elemental Brand (54) — item-target (ammo) — slevel 25 / smana 30 / sfail 80

-------------------------------------------------------------------------------

Book 8 — Kelek's Grimoire of Power (sval 8)
- Object entry: [lib/edit/object.txt](lib/edit/object.txt#L3365)
- W: 80:0:30:90000  A: 80/4

Spells:
- Earthquake (23) — self-centered area earthquake — slevel 23 / smana 12 / sfail 60
- Bedlam (40) — directional confusion ball `fire_ball(GF_OLD_CONF, dir, plev, 4)` — slevel 25 / smana 15 / sfail 60
- Rend Soul (41) — directional nether bolt/beam damroll(11, plev) — slevel 25 / smana 30 / sfail 80
- Banishment (59) — banishment() (see source) — slevel 30 / smana 45 / sfail 95
- Word of Destruction (42) — destroy_area centered on player — slevel 30 / smana 25 / sfail 85
- Mass Banishment (61) — mass_banishment() — slevel 35 / smana 75 / sfail 90
- Chaos Strike (43) — directional chaos ball damroll(13, plev) — slevel 35 / smana 30 / sfail 60
- Mana Storm (63) — directional area `fire_ball(GF_MANA, dir, 300 + 2*plev, 3)` — slevel 42 / smana 30 / sfail 95

-------------------------------------------------------------------------------

If you'd like a machine-readable export (CSV/JSON) with one row per spell including: `id,name,book_sval,book_name,target,effect_formula,mage_slevel,smana,sfail,store_counts,alloc_pairs`, tell me which format and I'll generate it and add it to the repo.
