**Staffs Compendium — MAngband (reference 1.5.3)**

Source notes:
- Staff implementations: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1076)
- Staff charge generation: [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L2336)
- Staff constants: [reference-mangband-1_5_3/src/server/mdefines.h](reference-mangband-1_5_3/src/server/mdefines.h#L1537)
- Dungeon allocation and object groups: [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L300)
- Store stock (which shops list which staffs): [reference-mangband-1_5_3/src/server/init2.c](reference-mangband-1_5_3/src/server/init2.c#L1278)

How to read this file:
- Each entry lists the `SV` constant and its numeric `sval`, the object name (from `object.txt`), the `W:` level (object creation level), `A:` allocation pairs (dungeon depth/rarity), the charge (`pval`) distribution (from `charge_staff()`), whether a direction is required, the exact effect/function call (from `use_staff()`), and damage / radius parameters where applicable.
- Store availability: the static `store_table` in `init2.c` contains explicit `{TV_STAFF, SV_STAFF_*}` slots for the Magic-User store. Probability for a given staff in that store = occurrences / `STORE_CHOICES` (32). If a staff is not present in `store_table`, it is not normally sold in shops and is dungeon-only (see `A:` allocations).

Summary: staffs are normally reliable (no generic `USE_DEVICE` check in `use_staff()`); some staff effects may be aborted (e.g., `identify` may return without consuming charge). See notes per-entry.

Staffs (complete list mapped from `mdefines.h` and `object.txt`):

1) `SV_STAFF_DARKNESS` (sval = 0) — `Darkness` (N:322)
  - Object meta: `I:55:0:0`  W: `5`  A: `5/1 : 50/1`
  - Charges (pval): `randint1(8) + 8` → 9..16 (charged in `charge_staff`).
  - Target: self
  - Effect (code): `set_blind(p_ptr, p_ptr->blind + 3 + randint1(5))`; `unlite_area(p_ptr, 10, 3)` — darkens area within radius 3; may blind. (no damage function)
  - Store: NOT listed in `store_table` (not sold by default).

2) `SV_STAFF_SLOWNESS` (sval = 1) — `Slowness` (N:315)
  - Object meta: `I:55:1:0`  W: `40`  A: `40/1`
  - Charges: `randint1(8) + 8` → 9..16
  - Target: self
  - Effect: `set_slow(p_ptr, p_ptr->slow + randint1(30) + 15)` — apply slowness to player.
  - Store: NOT in `store_table`.

3) `SV_STAFF_HASTE_MONSTERS` (sval = 2) — `Haste Monsters` (N:309)
  - Object meta: `I:55:2:0`  W: `10`  A: `10/1`
  - Charges: `randint1(8) + 8` → 9..16
  - Target: self (affects monsters in LOS)
  - Effect: `speed_monsters(p_ptr)` — hastes monsters.
  - Store: NOT in `store_table`.

4) `SV_STAFF_SUMMONING` (sval = 3) — `Summoning` (N:305)
  - Object meta: `I:55:3:0`  W: `10`  A: `10/1 : 50/1`
  - Charges: `randint1(3) + 1` → 2..4
  - Target: self
  - Effect: `summon_specific(Depth, py, px, p_ptr->dun_depth, 0)` executed 1..4 times (summons 1d4 monsters).
  - Store: NOT in `store_table`.

5) `SV_STAFF_TELEPORTATION` (sval = 4) — `Teleportation` (N:303)
  - Object meta: `I:55:4:0`  W: `20`  A: `20/1`
  - Charges: `randint1(4) + 5` → 6..9
  - Target: self
  - Effect: `teleport_player(p_ptr, 100)` — short/long teleport (range param 100).
  - Store: Sold in Magic-User store: occurrences = 2 → probability 2/32 = 6.25% (per `store_table`).

6) `SV_STAFF_IDENTIFY` (sval = 5) — `Perception` / identify (N:326)
  - Object meta: `I:55:5:0`  W: `10`  A: `10/1`
  - Charges: `randint1(15) + 5` → 6..20
  - Target: self
  - Effect: `ident_spell(p_ptr)` — reveals object powers; may be aborted: code sets `use_charge = FALSE` if `ident_spell()` returns false (no charge consumed).
  - Store: Sold in Magic-User store: occurrences = 2 → probability 6.25%.

7) `SV_STAFF_REMOVE_CURSE` (sval = 6) — `Remove Curse` (N:317)
  - Object meta: `I:55:6:0`  W: `40`  A: `40/1`
  - Charges: `randint1(3) + 4` → 5..7
  - Target: self
  - Effect: `remove_curse(p_ptr)` — remove ordinary curses from equipped items; success indicated in code.
  - Store: NOT in `store_table`.

8) `SV_STAFF_STARLITE` (sval = 7) — `Starlight` (N:308)
  - Object meta: `I:55:7:0`  W: `20`  A: `20/1`
  - Charges: `randint1(5) + 6` → 7..11
  - Target: multi-directional (fires lines in all 8 directions)
  - Effect: in code: `for (k = 0; k < 8; k++) lite_line(p_ptr, ddd[k]);` — object text: light-sensitive monsters on these lines take ~6d8 points (descriptive). No per-monster RNG in `use_staff()`, rely on `lite_line` behavior.
  - Store: NOT in `store_table`.

9) `SV_STAFF_LITE` (sval = 8) — `Light` (N:306)
  - Object meta: `I:55:8:0`  W: `5`  A: `5/1`
  - Charges: `randint1(20) + 8` → 9..28
  - Target: area (self-centered)
  - Effect: `lite_area(p_ptr, damroll(2,8), 2)` — damage = 2d8 to light-sensitive creatures; radius = 2.
  - Store: Sold in Magic-User store: occurrences = 1 → probability 1/32 = 3.125%.

10) `SV_STAFF_MAPPING` (sval = 9) — `Enlightenment` / mapping (N:328)
  - Object meta: `I:55:9:0`  W: `20`  A: `20/1`
  - Charges: `randint1(5) + 5` → 6..10
  - Target: self
  - Effect: `map_area(p_ptr)` — maps portion of the level.
  - Store: Sold in Magic-User store: occurrences = 1 → 3.125%.

11) `SV_STAFF_DETECT_GOLD` (sval = 10) — `Treasure Location` (N:301)
  - Object meta: `I:55:10:0`  W: `5`  A: `5/1`
  - Charges: `randint1(20) + 8` → 9..28
  - Target: self
  - Effect: `detect_treasure(p_ptr)` — detects nearby treasure.
  - Store: NOT in `store_table` (not sold by Magic-User table above).

12) `SV_STAFF_DETECT_ITEM` (sval = 11) — `Object Location` (N:302)
  - Object meta: `I:55:11:0`  W: `5`  A: `5/1`
  - Charges: `randint1(15) + 6` → 7..21
  - Target: self
  - Effect: `detect_objects_normal(p_ptr)` — detect nearby items.
  - Store: Sold in Magic-User store: occurrences = 1 → 3.125%.

13) `SV_STAFF_DETECT_TRAP` (sval = 12) — `Trap Location` (N:300)
  - Object meta: `I:55:12:0`  W: `10`  A: `10/1`
  - Charges: `randint1(5) + 6` → 7..11
  - Target: self
  - Effect: `detect_trap(p_ptr)` — detect traps in immediate area.
  - Store: Sold in Magic-User store: occurrences = 1 → 3.125%.

14) `SV_STAFF_DETECT_DOOR` (sval = 13) — `Door/Stair Location` (N:316)
  - Object meta: `I:55:13:0`  W: `10`  A: `10/1`
  - Charges: `randint1(8) + 6` → 7..14
  - Target: self
  - Effect: `detect_sdoor(p_ptr)` — detect doors/stairs/secret doors.
  - Store: Sold in Magic-User store: occurrences = 1 → 3.125%.

15) `SV_STAFF_DETECT_INVIS` (sval = 14) — `Detect Invisible` (N:313)
  - Object meta: `I:55:14:0`  W: `5`  A: `5/1`
  - Charges: `randint1(15) + 8` → 9..23
  - Target: self
  - Effect: `detect_invisible(p_ptr, TRUE)` — reveal invisible monsters.
  - Store: Sold in Magic-User store: occurrences = 1 → 3.125%.

16) `SV_STAFF_DETECT_EVIL` (sval = 15) — `Detect Evil` (N:318)
  - Object meta: `I:55:15:0`  W: `20`  A: `20/1`
  - Charges: `randint1(15) + 8` → 9..23
  - Target: self
  - Effect: `detect_evil(p_ptr)` — detect evil monsters.
  - Store: Sold in Magic-User store: occurrences = 1 → 3.125%.

17) `SV_STAFF_CURE_LIGHT` (sval = 16) — `Cure Light Wounds` (N:312)
  - Object meta: `I:55:16:0`  W: `5`  A: `5/1`
  - Charges: `randint1(5) + 6` → 7..11
  - Target: self
  - Effect: `hp_player(p_ptr, randint1(8))` — heals 1..8 HP.
  - Store: NOT in `store_table`.

18) `SV_STAFF_CURING` (sval = 17) — `Curing` (N:319)
  - Object meta: `I:55:17:0`  W: `25`  A: `25/1`
  - Charges: `randint1(3) + 4` → 5..7
  - Target: self
  - Effect: clears blindness/poison/confusion/stun/cut.
  - Store: NOT in `store_table`.

19) `SV_STAFF_HEALING` (sval = 18) — `Healing` (N:329)
  - Object meta: `I:55:18:0`  W: `70`  A: `70/2`
  - Charges: `randint1(2) + 1` → 2..3
  - Target: self
  - Effect: `hp_player(p_ptr, 300)` — heals ~300 HP; clears stun/cut.
  - Store: NOT in `store_table`.

20) `SV_STAFF_THE_MAGI` (sval = 19) — `the Magi` (N:325)
  - Object meta: `I:55:19:0`  W: `70`  A: `70/2`
  - Charges: `randint1(2) + 2` → 3..4
  - Target: self
  - Effect: `do_res_stat(p_ptr, A_INT)` and restores mana to max (`p_ptr->csp = p_ptr->msp`).
  - Store: NOT in `store_table`.

21) `SV_STAFF_SLEEP_MONSTERS` (sval = 20) — `Sleep Monsters` (N:311)
  - Object meta: `I:55:20:0`  W: `10`  A: `10/1`
  - Charges: `randint1(5) + 6` → 7..11
  - Target: self (affects monsters in LOS)
  - Effect: `sleep_monsters(p_ptr)` — attempt to sleep monsters.
  - Store: NOT in `store_table`.

22) `SV_STAFF_SLOW_MONSTERS` (sval = 21) — `Slow Monsters` (N:310)
  - Object meta: `I:55:21:0`  W: `10`  A: `10/1`
  - Charges: `randint1(5) + 6` → 7..11
  - Target: self
  - Effect: `slow_monsters(p_ptr)` — attempt to slow monsters.
  - Store: NOT in `store_table`.

23) `SV_STAFF_SPEED` (sval = 22) — `Speed` (N:314)
  - Object meta: `I:55:22:0`  W: `40`  A: `40/1`
  - Charges: `randint1(3) + 4` → 5..7
  - Target: self
  - Effect: `set_fast(p_ptr, randint1(30) + 15)` or increase existing speed by 5.
  - Store: NOT in `store_table`.

24) `SV_STAFF_PROBING` (sval = 23) — `Probing` (N:321)
  - Object meta: `I:55:23:0`  W: `30`  A: `30/1`
  - Charges: `randint1(6) + 2` → 3..8
  - Target: self
  - Effect: `probing(p_ptr)` — show monster HP/flags in LOS.
  - Store: NOT in `store_table`.

25) `SV_STAFF_DISPEL_EVIL` (sval = 24) — `Dispel Evil` (N:320)
  - Object meta: `I:55:24:0`  W: `50`  A: `50/1`
  - Charges: `randint1(3) + 4` → 5..7
  - Target: self (affects evil monsters in LOS)
  - Effect (code): `dispel_evil(p_ptr, 60)` — object text: inflicts 60 points on evil monsters in LOS.
  - Store: NOT in `store_table`.

26) `SV_STAFF_POWER` (sval = 25) — `Power` (N:324)
  - Object meta: `I:55:25:0`  W: `70`  A: `70/2`
  - Charges: `randint1(3) + 1` → 2..4
  - Target: self
  - Effect: `dispel_monsters(p_ptr, 120)` — object text: inflicts 120 points of damage on all monsters in LOS.
  - Store: NOT in `store_table`.

27) `SV_STAFF_HOLINESS` (sval = 26) — `Holiness` (N:327)
  - Object meta: `I:55:26:0`  W: `70`  A: `70/2`
  - Charges: `randint1(2) + 2` → 3..4
  - Target: self / area (affects evil monsters)
  - Effect: `dispel_evil(p_ptr, 120)` + `set_protevil(...)` + cures and `hp_player(p_ptr, 50)` — multi-effect (damage to evil monsters 120 center per `dispel_evil` call).
  - Store: NOT in `store_table`.

28) `SV_STAFF_BANISHMENT` (sval = 27) — `Banishment` (N:323)
  - Object meta: `I:55:27:0`  W: `70`  A: `70/4`
  - Charges: `randint1(2) + 1` → 2..3
  - Target: self (effect uses removal of monsters by symbol)
  - Effect: `banishment(p_ptr)` — may fail (code sets `use_charge = FALSE` if `banishment()` returns false); object text: removes chosen symbol monsters (you take 1d4 damage per removed monster).
  - Store: NOT in `store_table`.

29) `SV_STAFF_EARTHQUAKES` (sval = 28) — `Earthquakes` (N:304)
  - Object meta: `I:55:28:0`  W: `40`  A: `40/1`
  - Charges: `randint1(5) + 3` → 4..8
  - Target: self (area)
  - Effect: `earthquake(Depth, py, px, 10)` — radius 10; object text: monsters may take ~4d8 if they can evade, or be destroyed if trapped.
  - Store: NOT in `store_table`.

30) `SV_STAFF_DESTRUCTION` (sval = 29) — `*Destruction*` (N:307)
  - Object meta: `I:55:29:0`  W: `50`  A: `50/1 : 70/1`
  - Charges: `randint1(3) + 1` → 2..4
  - Target: self (area)
  - Effect: `destroy_area(Depth, py, px, 15, TRUE)` — radius 15; removes monsters/objects in area (artifacts immune); object text warns of blindness side-effect.
  - Store: NOT in `store_table`.

Store availability (summary):
- Magic-User store (the only static store table block that lists staffs) contains the following staff slots (out of `STORE_CHOICES = 32`):
  - `SV_STAFF_LITE` (1 slot) → 1/32 = 3.125%
  - `SV_STAFF_MAPPING` (1 slot) → 1/32 = 3.125%
  - `SV_STAFF_DETECT_TRAP` (1 slot) → 1/32
  - `SV_STAFF_DETECT_DOOR` (1 slot) → 1/32
  - `SV_STAFF_DETECT_ITEM` (1 slot) → 1/32
  - `SV_STAFF_DETECT_INVIS` (1 slot) → 1/32
  - `SV_STAFF_DETECT_EVIL` (1 slot) → 1/32
  - `SV_STAFF_TELEPORTATION` (2 slots) → 2/32 = 6.25%
  - `SV_STAFF_IDENTIFY` (2 slots) → 2/32 = 6.25%

Notes on failure / reliability:
- `use_staff()` contains no `USE_DEVICE`-style device-skill failure roll; staff effects are applied directly and are therefore reliable when their internal preconditions are met. Some staff calls may abort and avoid consuming a charge (e.g., `identify` and `banishment` check their routine's return value and may set `use_charge = FALSE`). There is no single numeric failure% formula for staffs like `spell_chance()` for spells.

References:
- `use_staff()` implementation: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1076)
- charge generation (`charge_staff()`): [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L2336)
- object definitions & allocations (`N:`, `I:`, `W:`, `A:`): [reference-mangband-1_5_3/lib/edit/object.txt](reference-mangband-1_5_3/lib/edit/object.txt#L300)
- store stock table: [reference-mangband-1_5_3/src/server/init2.c](reference-mangband-1_5_3/src/server/init2.c#L1278)

Source notes:
- Staff implementations: [reference-mangband-1_5_3/src/server/use-obj.c#L800-L1200](reference-mangband-1_5_3/src/server/use-obj.c)
- Staff constants: [reference-mangband-1_5_3/src/server/mdefines.h](reference-mangband-1_5_3/src/server/mdefines.h)
- Store stock: [reference-mangband-1_5_3/src/server/init2.c](reference-mangband-1_5_3/src/server/init2.c)

Device notes (staffs):
- Staffs are used via `use_staff()`; many staff effects do not use a device-skill check (staff use is typically reliable provided the staff has charges and the player meets basic conditions).
- Staffs consume charges/pvals; the `use_staff()` implementation handles charge decrement and effect application. Some staff effects require `get_aim_dir()` (directional) while others are self/area.

Common staff types (SV_*), target, effect (summary):
- `SV_STAFF_LITE`
  - Target: area
  - Effect: `lite_area()`

- `SV_STAFF_MAPPING`
  - Target: self
  - Effect: `map_area()`

- `SV_STAFF_TELEPORTATION`
  - Target: self
  - Effect: `teleport_player()` / teleport short/long

- `SV_STAFF_HEALING` / `SV_STAFF_HEALING_MINOR`
  - Target: self
  - Effect: `hp_player()` and status cures (stun/cut removed)

- `SV_STAFF_SPEED`
  - Target: self
  - Effect: `set_fast()` / speed effect

- `SV_STAFF_HOLINESS`
  - Target: area / self
  - Effect: dispel_evil/protect and heal allies (MAngband specifics)

- `SV_STAFF_DESTRUCTION` / `SV_STAFF_STAR_DESTRUCTION`
  - Target: area
  - Effect: `destroy_area()` style powerful area damage

Store / Dungeon availability:
- Staffs appear in `init2.c` store tables in Magic-User and some general/temple/black-market picks depending on the server's `store_table`. Dungeon allocation is driven by `object.txt` `A:` entries per object group.
