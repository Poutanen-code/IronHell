**Rods Compendium — MAngband (reference 1.5.3)**

Source notes:
- Implementations: reference-mangband-1_5_3/src/server/use-obj.c (function `zap_rod`).
- Constants: reference-mangband-1_5_3/src/server/mdefines.h (`SV_ROD_*`, `SV_ROD_MIN_DIRECTION`, `TV_ROD`).
- Dungeon allocation / object meta: reference-mangband-1_5_3/lib/edit/object.txt (N:/I:/W:/A: lines).
- Store stock: reference-mangband-1_5_3/src/server/init2.c (static `store_table`).

Notes — how to read this page:
- For each `SV_ROD_*` entry I list: numeric `sval`, object name (from `object.txt`), whether a direction is required, the effect (from `zap_rod()`), the damage parameters (if any), the rod `pval` (I: third field), the object `level` (W: first field), and the `A:` allocation pairs (dungeon depths/rarities).  
- Failure chance for rods is computed in `zap_rod()` from `chance = p_ptr->skill_dev - lev` (confusion halves `chance`), then compared to `USE_DEVICE` (3). If `chance >= USE_DEVICE` the approximate failure probability = (USE_DEVICE-1)/chance (with `USE_DEVICE == 3` → ~2/chance). If `chance < USE_DEVICE` the zap usually fails (there is a small fallback chance to set `chance = USE_DEVICE`), see the code for exact RNG.
- Direction rule: rods with `sval >= SV_ROD_MIN_DIRECTION (12)` require a direction (and many damaging rods are >=12). If the object is *unaware* the code also forces a direction prompt.
- Store availability: the static `store_table` in `init2.c` defines explicit tval/sval pairs sold by shops. In this reference tree there are no explicit `{ TV_ROD, SV_* }` entries in the `store_table` static array, so rods are not stocked via the static tables by default (Magic shops *accept* rods, but the initial stock table contains no rod slots). Rods are therefore primarily dungeon-generated items; see `A:` lines below for spawn depths/rarities.

Failure example: if `p_ptr->skill_dev = 40` and a rod has `level = 30` then `chance = 40 - 30 = 10`, failure% ≈ 2/10 = 20% (not accounting for confusion or the small low-chance fallback when `chance < USE_DEVICE`).

Rod list (complete for SV_ROD_* defined in `mdefines.h`):

- `SV_ROD_DETECT_TRAP` (sval=0) — Door/Trap detection (object: "Trap Location")
  - Object: N:352 "Trap Location"
  - I: `tval=66` `sval=0` `pval=50`
  - W: `level=5` (W:5/0:weight:cost)
  - A: `5/1` (found at depth 5, rarity 1)
  - Target: self (sval < 12)
  - Effect: `detect_trap()` — no damage.

- `SV_ROD_DETECT_DOOR` (sval=1) — Door/Stair detection ("Door/Stair Location")
  - Object: N:351 "Door/Stair Location"
  - I: `tval=66` `sval=1` `pval=70`
  - W: `level=15`
  - A: `15/1`
  - Target: self
  - Effect: `detect_sdoor()` — no damage.

- `SV_ROD_IDENTIFY` (sval=2) — Object perception/identify ("Perception")
  - Object: N:372 "Perception"
  - I: `tval=66` `sval=2` `pval=10`
  - W: `level=50`
  - A: `50/8`
  - Target: self
  - Effect: `ident_spell()` / reveal item powers (may be abortable); no damage.

- `SV_ROD_RECALL` (sval=3) — Recall ("Recall")
  - Object: N:354 "Recall"
  - I: `tval=66` `sval=3` `pval=60`
  - W: `level=30`
  - A: `30/4`
  - Target: self
  - Effect: `set_recall()` (town recall); no damage.

- `SV_ROD_ILLUMINATION` (sval=4) — Illumination ("Illumination")
  - Object: N:355 "Illumination"
  - I: `tval=66` `sval=4` `pval=30`
  - W: `level=20`
  - A: `20/1`
  - Target: self
  - Effect: `lite_area(p_ptr, damroll(2,8), 2)` — illumination and up to 2d8 damage to light-sensitive creatures (radius 2).

- `SV_ROD_MAPPING` (sval=5) — Mapping / Enlightenment ("Enlightenment")
  - Object: N:371 "Enlightenment"
  - I: `tval=66` `sval=5` `pval=99`
  - W: `level=65`
  - A: `65/4`
  - Target: self
  - Effect: `map_area()` — maps area around player; no damage.

- `SV_ROD_DETECTION` (sval=6) — Detection ("Detection")
  - Object: N:375 "Detection"
  - I: `tval=66` `sval=6` `pval=99`
  - W: `level=30`
  - A: `30/8`
  - Target: self
  - Effect: `detection()` — detects treasure/objects/doors/traps/monsters; no damage.

- `SV_ROD_PROBING` (sval=7) — Probing ("Probing")
  - Object: N:353 "Probing"
  - I: `tval=66` `sval=7` `pval=50`
  - W: `level=40`
  - A: `40/4`
  - Target: self
  - Effect: `probing()` — probes monsters (shows HP/flags); no damage.

- `SV_ROD_CURING` (sval=8) — Curing ("Curing")
  - Object: N:373 "Curing"
  - I: `tval=66` `sval=8` `pval=999`
  - W: `level=65`
  - A: `65/8`
  - Target: self
  - Effect: clears blindness/poison/confusion/stun/cut; no damage.

- `SV_ROD_HEALING` (sval=9) — Healing ("Healing")
  - Object: N:374 "Healing"
  - I: `tval=66` `sval=9` `pval=999`
  - W: `level=80`
  - A: `80/8`
  - Target: self
  - Effect: `hp_player(p_ptr, 500)` — heals ~500 HP, clears stun/cut per code; no offensive damage.

- `SV_ROD_RESTORATION` (sval=10) — Restoration ("Restoration")
  - Object: N:376 "Restoration"
  - I: `tval=66` `sval=10` `pval=999`
  - W: `level=80`
  - A: `80/16`
  - Target: self
  - Effect: `restore_level()` + restore stats; no damage.

- `SV_ROD_SPEED` (sval=11) — Speed ("Speed")
  - Object: N:377 "Speed"
  - I: `tval=66` `sval=11` `pval=99`
  - W: `level=95`
  - A: `95/16`
  - Target: self
  - Effect: `set_fast(p_ptr, randint1(30) + 15)` — grants haste; no damage.

- `SV_ROD_TELEPORT_AWAY` (sval=13) — Teleport Other ("Teleport Other")
  - Object: N:364 "Teleport Other"
  - I: `tval=66` `sval=13` `pval=25`
  - W: `level=45`
  - A: `45/2`
  - Target: directional (sval >= 12)
  - Effect: `teleport_monster(p_ptr, dir)` — teleports monsters (beam); no direct damage.

- `SV_ROD_DISARMING` (sval=14) — Disarming ("Disarming")
  - Object: N:365 "Disarming"
  - I: `tval=66` `sval=14` `pval=30`
  - W: `level=35`
  - A: `35/1`
  - Target: directional
  - Effect: `disarm_trap(p_ptr, dir)` — disarms traps/opens doors in beam; no damage.

- `SV_ROD_LITE` (sval=15) — Light line ("Light")
  - Object: N:356 "Light"
  - I: `tval=66` `sval=15` `pval=9`
  - W: `level=10`
  - A: `10/1`
  - Target: directional
  - Effect: `lite_line(p_ptr, dir)` — lights a line; object text: 6d8 damage to light-sensitive creatures on the line.

- `SV_ROD_SLEEP_MONSTER` (sval=16) — Sleep ("Sleep Monster")
  - Object: N:362 "Sleep Monster"
  - I: `tval=66` `sval=16` `pval=18`
  - W: `level=30`
  - A: `30/1`
  - Target: directional
  - Effect: `sleep_monster(p_ptr, dir)` — attempts to put a monster to sleep.

- `SV_ROD_SLOW_MONSTER` (sval=17) — Slow ("Slow Monster")
  - Object: N:361 "Slow Monster"
  - I: `tval=66` `sval=17` `pval=20`
  - W: `level=30`
  - A: `30/1`
  - Target: directional
  - Effect: `slow_monster(p_ptr, dir)` — attempts to slow a monster.

- `SV_ROD_DRAIN_LIFE` (sval=18) — Drain Life ("Drain Life")
  - Object: N:363 "Drain Life"
  - I: `tval=66` `sval=18` `pval=23`
  - W: `level=75`
  - A: `75/4`
  - Target: directional
  - Effect: `drain_life(p_ptr, dir, 150)` — drains ~150 HP from a target (code parameter = 150).

- `SV_ROD_POLYMORPH` (sval=19) — Polymorph ("Polymorph")
  - Object: N:360 "Polymorph"
  - I: `tval=66` `sval=19` `pval=25`
  - W: `level=35`
  - A: `35/1`
  - Target: directional
  - Effect: `poly_monster(p_ptr, dir)` — attempts to polymorph a monster.

- `SV_ROD_ACID_BOLT` (sval=20) — Acid Bolt ("Acid Bolts")
  - Object: N:370 "Acid Bolts"
  - I: `tval=66` `sval=20` `pval=12`
  - W: `level=40`
  - A: `40/1`
  - Target: directional
  - Effect: `fire_bolt_or_beam(p_ptr, 10, GF_ACID, dir, damroll(12,8))` → bolt damage = 12d8 (object text matches).

- `SV_ROD_ELEC_BOLT` (sval=21) — Lightning Bolt ("Lightning Bolts")
  - Object: N:357 "Lightning Bolts"
  - I: `tval=66` `sval=21` `pval=11`
  - W: `level=20`
  - A: `20/1`
  - Target: directional
  - Effect: `fire_bolt_or_beam(p_ptr, 10, GF_ELEC, dir, damroll(6,6))` → bolt damage = 6d6.

- `SV_ROD_FIRE_BOLT` (sval=22) — Fire Bolt ("Fire Bolts")
  - Object: N:359 "Fire Bolts"
  - I: `tval=66` `sval=22` `pval=15`
  - W: `level=30`
  - A: `30/1`
  - Target: directional
  - Effect: `fire_bolt_or_beam(p_ptr, 10, GF_FIRE, dir, damroll(16,8))` → bolt damage = 16d8.

- `SV_ROD_COLD_BOLT` (sval=23) — Frost Bolt ("Frost Bolts")
  - Object: N:358 "Frost Bolts"
  - I: `tval=66` `sval=23` `pval=13`
  - W: `level=25`
  - A: `25/1`
  - Target: directional
  - Effect: `fire_bolt_or_beam(p_ptr, 10, GF_COLD, dir, damroll(10,8))` → bolt damage = 10d8.

- `SV_ROD_ACID_BALL` (sval=24) — Acid Ball ("Acid Balls")
  - Object: N:369 "Acid Balls"
  - I: `tval=66` `sval=24` `pval=27`
  - W: `level=70`
  - A: `70/1`
  - Target: directional
  - Effect: `fire_ball(p_ptr, GF_ACID, dir, 120, 2)` → center damage = 120, radius = 2.

- `SV_ROD_ELEC_BALL` (sval=25) — Lightning Ball ("Lightning Balls")
  - Object: N:366 "Lightning Balls"
  - I: `tval=66` `sval=25` `pval=23`
  - W: `level=55`
  - A: `55/1`
  - Target: directional
  - Effect: `fire_ball(p_ptr, GF_ELEC, dir, 64, 2)` → center damage = 64, radius = 2.

- `SV_ROD_FIRE_BALL` (sval=26) — Fire Ball ("Fire Balls")
  - Object: N:368 "Fire Balls"
  - I: `tval=66` `sval=26` `pval=30`
  - W: `level=75`
  - A: `75/1`
  - Target: directional
  - Effect: `fire_ball(p_ptr, GF_FIRE, dir, 144, 2)` → center damage = 144, radius = 2.

- `SV_ROD_COLD_BALL` (sval=27) — Frost Ball ("Cold Balls")
  - Object: N:367 "Cold Balls"
  - I: `tval=66` `sval=27` `pval=25`
  - W: `level=60`
  - A: `60/1`
  - Target: directional
  - Effect: `fire_ball(p_ptr, GF_COLD, dir, 96, 2)` → center damage = 96, radius = 2.

Notes and next steps:
- I've documented every `SV_ROD_*` entry, the object metadata (`I:`, `W:`, `A:`) and the exact effect/parameters taken from `zap_rod()` and `object.txt` descriptions.  
- If you want, I can now (A) compute example failure% values for each rod for a set of sample `p_ptr->skill_dev` values (e.g., 20/40/60), or (B) scan `init2.c` again and produce per-store probability tables (if any rod entries appear in your variant's `store_table`). Which should I do next?
