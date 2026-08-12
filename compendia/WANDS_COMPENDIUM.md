**Wands Compendium — MAngband (reference 1.5.3)**

Source notes:
- Wand implementations: reference-mangband-1_5_3/src/server/use-obj.c
- Wand constants: reference-mangband-1_5_3/src/server/mdefines.h
- Object metadata (names / I: / W: / A:): reference-mangband-1_5_3/lib/edit/object.txt ("Wands" section)
- Store stock: reference-mangband-1_5_3/src/server/init2.c (Magic-User store table)

Device notes:
- Wands are fired with `aim_wand()` which requires `get_aim_dir()` (directional) and performs a device-use skill check using `p_ptr->skill_dev`, the object's kind level (`k_info[o_ptr->k_idx].level`) and `USE_DEVICE` (3). Confusion halves skill.
- Charges are stored in `o_ptr->pval` and decrement on use.
- Store probability = occurrences in the static `store_table` block / `STORE_CHOICES` (32).

Magic-User store (static slots in `init2.c`): the following wand svals are explicitly listed (each = 1 slot unless noted):
- `SV_WAND_SLOW_MONSTER` — 1/32 (3.125%)
- `SV_WAND_CONFUSE_MONSTER` — 1/32 (3.125%)
- `SV_WAND_SLEEP_MONSTER` — 1/32 (3.125%)
- `SV_WAND_MAGIC_MISSILE` — 1/32 (3.125%)
- `SV_WAND_STINKING_CLOUD` — 1/32 (3.125%)
(All other wand svals are not present in the built-in static store table → 0/32 in the static stock.)

Wand list (sval mapping and effect details from `aim_wand()` / `object.txt`):

- `SV_WAND_HEAL_MONSTER` (sval=0) — "Heal Monster"
  - Target: directional
  - Effect: `heal_monster(dir)` (object text: cures ~4d6 HP)
  - Shop: not listed in static store table (0/32).

- `SV_WAND_HASTE_MONSTER` (1) — "Haste Monster"
  - Target: directional
  - Effect: `speed_monster(dir)`
  - Shop: 0/32.

- `SV_WAND_CLONE_MONSTER` (2) — "Clone Monster"
  - Target: directional
  - Effect: `clone_monster(dir)` (may create a new monster near the target)
  - Shop: 0/32.

- `SV_WAND_TELEPORT_AWAY` (3) — "Teleport Other"
  - Target: directional
  - Effect: `teleport_monster(dir)` (teleports the target)
  - Shop: 0/32.

- `SV_WAND_DISARMING` (4) — "Disarming"
  - Target: directional
  - Effect: `disarm_trap(dir)` / opens doors
  - Shop: 0/32.

- `SV_WAND_TRAP_DOOR_DEST` (5) — "Trap/Door Destruction"
  - Target: directional
  - Effect: `destroy_door(dir)`
  - Shop: 0/32.

- `SV_WAND_STONE_TO_MUD` (6) — "Stone to Mud"
  - Target: directional
  - Effect: `wall_to_mud(dir)` (object text: susceptible monsters on beam take ~20+1d30 damage)
  - Shop: 0/32.

- `SV_WAND_LITE` (7) — "Light"
  - Target: directional
  - Effect: `lite_line(dir)` (object text: light-sensitive creatures take ~6d8 on the line)
  - Shop: 0/32 (not in static stock); dungeon allocation and W: level in `object.txt`.

- `SV_WAND_SLEEP_MONSTER` (8) — "Sleep Monster"
  - Target: directional
  - Effect: `sleep_monster(dir)`
  - Shop: listed in Magic-User store: 1/32 (3.125%).

- `SV_WAND_SLOW_MONSTER` (9) — "Slow Monster"
  - Target: directional
  - Effect: `slow_monster(dir)`
  - Shop: listed in Magic-User store: 1/32 (3.125%).

- `SV_WAND_CONFUSE_MONSTER` (10) — "Confuse Monster"
  - Target: directional
  - Effect: `confuse_monster(dir, 10)` (confusion parameter = 10)
  - Shop: listed in Magic-User store: 1/32 (3.125%).

- `SV_WAND_FEAR_MONSTER` (11) — "Fear Monster"
  - Target: directional
  - Effect: `fear_monster(dir, 10)` (parameter = 10)
  - Shop: 0/32.

- `SV_WAND_DRAIN_LIFE` (12) — "Drain Life"
  - Target: directional
  - Effect: `drain_life(p_ptr, dir, 150)` — code parameter = 150 (damage/drain value)
  - Shop: 0/32.

- `SV_WAND_POLYMORPH` (13) — "Polymorph"
  - Target: directional
  - Effect: `poly_monster(p_ptr, dir)`
  - Shop: 0/32.

- `SV_WAND_STINKING_CLOUD` (14) — "Stinking Cloud"
  - Target: directional / area
  - Effect: `fire_ball(GF_POIS, dir, 12, 2)` — ball center parameter = 12, radius = 2
  - Shop: listed in Magic-User store: 1/32 (3.125%).

- `SV_WAND_MAGIC_MISSILE` (15) — "Magic Missile"
  - Target: directional
  - Effect: `fire_bolt_or_beam(GF_MISSILE, dir, damroll(3,4))` — bolt damage = 3d4
  - Shop: listed in Magic-User store: 1/32 (3.125%).

- `SV_WAND_ACID_BOLT` (16) — "Acid Bolt"
  - Target: directional
  - Effect: `fire_bolt_or_beam(GF_ACID, dir, damroll(10,8))` — bolt = 10d8
  - Shop: 0/32.

- `SV_WAND_ELEC_BOLT` (17) — "Lightning Bolt"
  - Target: directional
  - Effect: `fire_bolt_or_beam(GF_ELEC, dir, damroll(6,6))` — bolt = 6d6
  - Shop: 0/32.

- `SV_WAND_FIRE_BOLT` (18) — "Fire Bolt"
  - Target: directional
  - Effect: `fire_bolt_or_beam(GF_FIRE, dir, damroll(12,8))` — bolt = 12d8
  - Shop: 0/32.

- `SV_WAND_COLD_BOLT` (19) — "Cold Bolt"
  - Target: directional
  - Effect: `fire_bolt_or_beam(GF_COLD, dir, damroll(6,8))` — bolt = 6d8
  - Shop: 0/32.

- `SV_WAND_ACID_BALL` (20) — "Acid Ball"
  - Target: directional
  - Effect: `fire_ball(GF_ACID, dir, 120, 2)` — center = 120, radius = 2
  - Shop: 0/32.

- `SV_WAND_ELEC_BALL` (21) — "Lightning Ball"
  - Target: directional
  - Effect: `fire_ball(GF_ELEC, dir, 64, 2)` — center = 64, radius = 2
  - Shop: 0/32.

- `SV_WAND_FIRE_BALL` (22) — "Fire Ball"
  - Target: directional
  - Effect: `fire_ball(GF_FIRE, dir, 144, 2)` — center = 144, radius = 2
  - Shop: 0/32.

- `SV_WAND_COLD_BALL` (23) — "Cold Ball"
  - Target: directional
  - Effect: `fire_ball(GF_COLD, dir, 96, 2)` — center = 96, radius = 2
  - Shop: 0/32.

- `SV_WAND_WONDER` (24) — "Wand of Wonder"
  - Target: directional / special
  - Effect: random `wand_wonder()` behavior (code uses `sval == SV_WAND_WONDER` to pick a random effect)
  - Shop: 0/32.

- `SV_WAND_ANNIHILATION` (25) — "Annihilation"
  - Target: directional
  - Effect: `drain_life(p_ptr, dir, 250)` — parameter = 250
  - Shop: 0/32.

- `SV_WAND_DRAGON_FIRE` (26) — "Dragon Fire"
  - Target: directional
  - Effect: `fire_ball(GF_FIRE, dir, 200, 3)` — center = 200, radius = 3
  - Shop: 0/32.

- `SV_WAND_DRAGON_COLD` (27) — "Dragon Cold"
  - Target: directional
  - Effect: `fire_ball(GF_COLD, dir, 160, 3)` — center = 160, radius = 3
  - Shop: 0/32.

- `SV_WAND_DRAGON_BREATH` (28) — "Dragon Breath"
  - Target: directional
  - Effect: one of (acid 200/3, elec 160/3, fire 200/3, cold 160/3, poison 120/3) chosen randomly (see code switch)
  - Shop: 0/32.

Notes on failure:
- `aim_wand()` does an explicit device-use skill check: `chance = p_ptr->skill_dev` (halved if confused), then `chance -= lev` where `lev = k_info[o_ptr->k_idx].level`. If `chance < USE_DEVICE` a small fallback random chance sets `chance = USE_DEVICE` in rare cases. The code then checks `(chance < USE_DEVICE) || (randint1(chance) < USE_DEVICE)` to fail the use. In practice compute per-wand failure using sample `p_ptr->skill_dev` and the rod/wand `k_info[].level` to get realistic probabilities.

References:
- `aim_wand()` implementation: reference-mangband-1_5_3/src/server/use-obj.c (lines ~1338-1470)
- Wand object metadata: reference-mangband-1_5_3/lib/edit/object.txt (Wands section)
- Store table: reference-mangband-1_5_3/src/server/init2.c

If you want, I can append a machine-readable CSV/JSON row per wand with: `sval,name,target,effect,parameters,store_counts,alloc_pairs,level` (and add it to `compendia/wands.json`).
