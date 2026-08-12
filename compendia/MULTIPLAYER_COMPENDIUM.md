# Multiplayer Compendium — MAngband (reference 1.5.3)

> Source files:
> - Common networking: [reference-mangband-1_5_3/src/common/net-basics.h](reference-mangband-1_5_3/src/common/net-basics.h), [net-imps.c/h](reference-mangband-1_5_3/src/common/net-imps.c), [net-pack.c/h](reference-mangband-1_5_3/src/common/net-pack.c)
> - Packet definitions: [reference-mangband-1_5_3/src/common/pack.h](reference-mangband-1_5_3/src/common/pack.h)
> - Shared types: [reference-mangband-1_5_3/src/common/types.h](reference-mangband-1_5_3/src/common/types.h)
> - Server networking: [reference-mangband-1_5_3/src/server/net-server.c](reference-mangband-1_5_3/src/server/net-server.c), [net-game.c](reference-mangband-1_5_3/src/server/net-game.c)
> - Server main / game loop: [reference-mangband-1_5_3/src/server/main.c](reference-mangband-1_5_3/src/server/main.c), [dungeon.c](reference-mangband-1_5_3/src/server/dungeon.c)
> - Client networking: [reference-mangband-1_5_3/src/client/net-client.c](reference-mangband-1_5_3/src/client/net-client.c), [net-client.h](reference-mangband-1_5_3/src/client/net-client.h)
> - Client initialization: [reference-mangband-1_5_3/src/client/c-init.c](reference-mangband-1_5_3/src/client/c-init.c)
> - Party system: [reference-mangband-1_5_3/src/server/party.c](reference-mangband-1_5_3/src/server/party.c)
> - Admin console: [reference-mangband-1_5_3/src/server/control.c](reference-mangband-1_5_3/src/server/control.c)
> - Server config: [reference-mangband-1_5_3/mangband.cfg](reference-mangband-1_5_3/mangband.cfg)

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Source Code Organization](#2-source-code-organization)
3. [Common Networking Layer (net-basics, net-imps)](#3-common-networking-layer)
4. [Packet Serialization (net-pack)](#4-packet-serialization)
5. [Packet Protocol (pack.h)](#5-packet-protocol)
6. [Server Bootstrap and Main Loop](#6-server-bootstrap-and-main-loop)
7. [Server Network Loop — The Infinite Loop](#7-server-network-loop--the-infinite-loop)
8. [Connection Lifecycle](#8-connection-lifecycle)
9. [Player State Machine](#9-player-state-machine)
10. [Authentication and Login](#10-authentication-and-login)
11. [WebSocket Support](#11-websocket-support)
12. [The Game Tick — `dungeon()`](#12-the-game-tick--dungeon)
13. [Energy and Level Speed](#13-energy-and-level-speed)
14. [Command Buffering and Execution](#14-command-buffering-and-execution)
15. [Data Streams (Display)](#15-data-streams-display)
16. [Indicators (HUD)](#16-indicators-hud)
17. [Custom Commands](#17-custom-commands)
18. [Message and Chat System](#18-message-and-chat-system)
19. [Party System](#19-party-system)
20. [PvP and Hostility System](#20-pvp-and-hostility-system)
21. [Player Visibility](#21-player-visibility)
22. [Shared Dungeon and Level Persistence](#22-shared-dungeon-and-level-persistence)
23. [Keepalive and Timeout](#23-keepalive-and-timeout)
24. [Unique Monster Respawning (Multiplayer)](#24-unique-monster-respawning-multiplayer)
25. [Meta-Server Reporting](#25-meta-server-reporting)
26. [Admin Console](#26-admin-console)
27. [Client Architecture](#27-client-architecture)
28. [Client Network Loop](#28-client-network-loop)
29. [Client Setup and Data Sync](#29-client-setup-and-data-sync)
30. [Server Configuration](#30-server-configuration)
31. [Key Algorithms Summary](#31-key-algorithms-summary)
32. [IronHell Adoption Guide](#32-ironhell-adoption-guide)

---

## 1. Architecture Overview

MAngband uses a **strict authoritative server** model:

```
┌─────────────────────────────────────────────┐
│                   SERVER                     │
│                                              │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐ │
│  │ Listener │   │ Timers   │   │  Game     │ │
│  │ (TCP)    │──▶│ dungeon_ │──▶│  State    │ │
│  │          │   │ tick     │   │ (cave,    │ │
│  └────┬─────┘   │ second_  │   │  players, │ │
│       │         │ tick     │   │  monsters)│ │
│       ▼         └──────────┘   └──────────┘ │
│  ┌──────────┐                                │
│  │Connection│◀──── read/write buffers ──────▶│
│  │  Pool    │                                │
│  └──────────┘                                │
└──────────┬──────────────────────────┬────────┘
           │  TCP (raw or WebSocket)  │
      ┌────▼────┐              ┌─────▼───┐
      │ Client  │              │ Client  │
      │   #1    │              │   #2    │
      └─────────┘              └─────────┘
```

**Key principles:**
- **Server is authoritative**: all game state (dungeon, monsters, items, player stats) lives on the server. Clients are thin display terminals.
- **TCP-based**: all communication is over persistent TCP connections (with optional WebSocket wrapping for browser clients).
- **Non-blocking I/O**: all sockets are non-blocking. The server uses `select()` to multiplex across all connections.
- **Shared world**: all players exist in the same world. Dungeon levels are shared — if two players are on depth 5, they see the same cave.
- **Energy-based turns**: the server ticks at `cfg_fps` (default 75) frames per second. Each tick awards energy to all entities; actions consume energy.

---

## 2. Source Code Organization

```
src/
├── common/               # Shared between client and server
│   ├── net-basics.c/h    # CharQueue, element lists, element groups
│   ├── net-imps.c/h      # TCP networking: listeners, callers, connections, timers
│   ├── net-pack.c/h      # Binary serialization: cq_printf / cq_scanf
│   ├── pack.h            # Packet type constants (PKT_LOGIN, PKT_WALK, etc.)
│   ├── types.h           # Shared structs (cave_type, party_type, stream_type, etc.)
│   ├── defines.h         # Global constants (MAX_PLAYERS=1000, MAX_DEPTH=128)
│   ├── md5.c/h           # MD5 hashing
│   ├── sha1.c/h          # SHA-1 (for WebSocket handshake)
│   ├── base64encode.c/h  # Base64 (for WebSocket handshake)
│   └── z-*.c/h           # Utility libs (RNG, memory, strings, files)
│
├── server/               # Authoritative game server
│   ├── main.c            # Entry point, init_stuff(), play_game()
│   ├── net-server.c      # Network setup, accept/login/read/close, main loop
│   ├── net-server.h      # Server networking API
│   ├── net-game.c        # Packet handlers, send/recv functions, command processing
│   ├── net-game.h        # Packet ↔ handler table (PACKET/PCOMMAND macros)
│   ├── dungeon.c         # Game tick: dungeon(), process_player_begin/end, process_world
│   ├── party.c           # Party system, hostility, PvP
│   ├── control.c         # Admin console (TCP port + 1)
│   ├── cave.c            # Dungeon grid operations, everyone_lite_spot()
│   ├── generate.c        # Level generation
│   ├── monster2.c        # Monster lifecycle
│   ├── melee1.c/melee2.c # Combat and AI
│   └── ...               # 30+ other game systems
│
├── client/               # Display client (thin terminal)
│   ├── client.c          # Entry point, main()
│   ├── c-init.c          # Setup, login flow, Game_loop, data sync
│   ├── net-client.c      # Client networking: connect, send/recv, keepalive
│   ├── net-client.h      # Client packet ↔ handler table
│   ├── c-cmd.c/c-cmd0.c  # Client-side command processing
│   ├── ui.c/h            # UI abstraction layer
│   ├── main-sdl.c        # SDL display module
│   ├── main-gcu.c        # ncurses display module
│   ├── main-win.c        # Windows display module
│   └── z-term.c/h        # Terminal abstraction (virtual terminal grid)
```

---

## 3. Common Networking Layer

### CharQueue (cq) — The Byte Buffer

All network I/O flows through `cq` (CharQueue) — a simple circular byte buffer used as both read and write buffers for every connection:

```c
typedef struct char_queue {
    char *buf;  // Raw byte buffer
    int len;    // Write position (end of data)
    int pos;    // Read position (start of unread data)
    int max;    // Buffer capacity
    int flush;  // Flush state
    int err;    // Error code
} cq;
```

**Buffer sizes:**
- `PD_SMALL_BUFFER` = 4 KB (command buffers, UDP senders)
- `PD_LARGE_BUFFER` = 32 KB (TCP connection read/write buffers)

**Key operations:**
- `cq_printf(cq, fmt, ...)` — serialize data into buffer (write)
- `cq_scanf(cq, fmt, ...)` — deserialize data from buffer (read)
- `cq_copyf(src, fmt, dst)` — copy formatted data between buffers
- `cq_slide(cq)` — compact buffer (shift unread data to position 0)
- `CQ_LEN(cq)` — bytes available for reading
- `CQ_SPACE(cq)` — bytes available for writing

### Network Implementations (net-imps)

The `net-imps` module provides five event-driven primitives, each built atop non-blocking sockets and `select()`:

| Primitive | Purpose | Socket Type | Callback |
|-----------|---------|-------------|----------|
| **Listener** | Accept incoming TCP connections | TCP listen | `accept_cb(fd, listener)` |
| **Connection** | Bidirectional TCP stream | TCP connected | `receive_cb`, `send_cb`, `close_cb` |
| **Caller** | Initiate outgoing TCP connection | TCP connecting | `connect_cb`, `failure_cb` |
| **Sender** | Periodic UDP datagram output | UDP | `send_cb(interval, wbuf)` |
| **Timer** | Periodic callback at interval | N/A | `timeout_cb(microsec, timer)` |

**Connection structure:**
```c
struct connection_type {
    int conn_fd;           // Socket file descriptor
    callback receive_cb;   // Called when data arrives
    callback send_cb;      // Optional: connection wrapper (WebSocket)
    callback close_cb;     // Called on disconnect
    int close;             // Flag: schedule this connection for closing
    char host_addr[24];    // Peer IP address string
    cq rbuf;               // Read buffer (incoming data)
    cq wbuf;               // Write buffer (outgoing data)
    int user;              // User-defined data (player index)
    data uptr;             // User-defined pointer (console_connection)
};
```

### The select() Loop

All networking uses a single `select()` call per iteration:

```c
void network_pause(micro timeout) {
    nfds = MAX(lnfds, cnfds, crfds, refds);
    select(nfds + 1, &rd, &wd, NULL, &tv);  // tv = 0.002ms
}
```

`handle_connections()` iterates over all connections:
1. `recvfrom()` — read available bytes into `rbuf`
2. Call `receive_cb` — process buffered data
3. `sendto()` — flush `wbuf` to socket
4. If `close` flag is set, tear down connection

---

## 4. Packet Serialization

### Format Strings

`cq_printf` and `cq_scanf` use printf-like format strings with type specifiers:

| Format | Type | Size | Description |
|--------|------|------|-------------|
| `%c` | `signed char` | 1 byte | Signed 8-bit integer |
| `%b` | `unsigned char` | 1 byte | Unsigned 8-bit integer |
| `%d` | `s16b` | 2 bytes | Signed 16-bit, big-endian |
| `%ud` | `u16b` | 2 bytes | Unsigned 16-bit, big-endian |
| `%l` | `s32b` | 4 bytes | Signed 32-bit, big-endian |
| `%ul` | `u32b` | 4 bytes | Unsigned 32-bit, big-endian |
| `%uv` | `u64b` | 1-9 bytes | Variable-length unsigned (WebSocket style) |
| `%s` | `char*` | variable | Null-terminated string |
| `%S` | `char*` | variable | Length-prefixed string |
| `%T` | `char*` | variable | Raw text (no length prefix, no null term) |

**Byte order:** All multi-byte integers are serialized in **network byte order** (big-endian).

### Transaction Pattern

Send functions use a transaction pattern to handle buffer overflow:
```c
int start_pos = ct->wbuf.len;  // Save position
if (!cq_printf(&ct->wbuf, "%c%d", PKT_TYPE, value)) {
    ct->wbuf.len = start_pos;  // Rollback on failure
    client_withdraw(ct);        // Kill connection
}
```

### Cave Data Compression (RLE)

For map data, specialized functions handle Run-Length Encoding:
- `cq_printc()` / `cq_scanc()` — cave view data with RLE
- `cq_printac()` / `cq_scanac()` — attr/char pairs with RLE

Streams can use different RLE modes: `RLE_NONE`, `RLE_CLASSIC`, `RLE_LARGE`.

---

## 5. Packet Protocol

### Packet Structure

Every packet is a single `byte` packet type followed by type-specific payload:

```
[1 byte: PKT_TYPE] [N bytes: payload per scheme]
```

There is **no length header** — the receiver must know the expected payload format (the "scheme") for each packet type. If a packet is partially received, the read position is rewound and processing resumes on the next network iteration.

### Packet Categories

| Range | Direction | Category | Examples |
|-------|-----------|----------|----------|
| 0-11 | Bidirectional | Administrative | `PKT_LOGIN(1)`, `PKT_PLAY(3)`, `PKT_QUIT(4)`, `PKT_BASIC_INFO(7)`, `PKT_KEEPALIVE(12)` |
| 12-19 | Bidirectional | Setup | `PKT_STRUCT_INFO(13)`, `PKT_VISUAL_INFO(15)`, `PKT_RESIZE(16)`, `PKT_COMMAND(17)` |
| 20-59 | Server → Client | Game state | `PKT_PLUSSES(20)`, `PKT_CHAR_INFO(26)`, `PKT_INVEN(30)`, `PKT_MESSAGE(46)`, `PKT_STORE(51)`, `PKT_MINI_MAP(55)` |
| 60-64 | Bidirectional | Interaction | `PKT_DIRECTION(60)`, `PKT_ITEM(61)`, `PKT_PARTY(63)` |
| 70-118 | Client → Server | Player actions | `PKT_WALK(70)`, `PKT_RUN(71)`, `PKT_REST(72)`, `PKT_PATHFIND(73)`, `PKT_PURCHASE(107)`, `PKT_MASTER(118)` |
| 121-123 | Server → Client | Status | `PKT_FAILURE(121)`, `PKT_SUCCESS(122)`, `PKT_CLEAR(123)` |
| 150-169 | Various | Hacks/Extensions | `PKT_FLUSH(150)`, `PKT_CURSOR(151)`, `PKT_CHANGEPASS(162)`, `PKT_CHANNEL(165)`, `PKT_TERM(167)`, `PKT_KEY(168)` |
| 170-190 | Server → Client | Data streams | `PKT_STREAM(170)` base, dynamically assigned |
| 191-254 | Server → Client | Indicators | `PKT_INDICATOR(191)` base, dynamically assigned |

### Command Schemes

Each command packet has a "scheme" — a format string defining its payload:

```c
PACKET(PKT_WALK,     "%c",      recv_walk)      // direction byte
PACKET(PKT_REST,     "",         recv_toggle_rest) // no payload
PACKET(PKT_PATHFIND, "%c%c",    recv_pathfind)  // y, x
PACKET(PKT_MESSAGE,  "%s",      recv_message)   // text string
PACKET(PKT_PARTY,    "%d%s",    recv_party)     // command + name
PACKET(PKT_CHANNEL,  "%ud%c%s", recv_channel)   // id, mode, name
```

Complex packets use `SCHEME_*` constants for the custom command system:
```
SCHEME_EMPTY        = no payload
SCHEME_ITEM         = item index
SCHEME_DIR          = direction
SCHEME_ITEM_DIR     = item + direction
SCHEME_STRING       = string
SCHEME_VALUE        = 32-bit value
SCHEME_ITEM_DIR_VALUE = item + direction + value
...etc (29 total schemes)
```

---

## 6. Server Bootstrap and Main Loop

### Startup Sequence (`main.c` → `play_game()`)

```
main()
  ├── init_stuff()           # Resolve file paths (ANGBAND_PATH or PKGDATADIR)
  ├── init_angband()         # Load game data (monsters, items, terrain)
  ├── play_game(new_game)
  │   ├── load_server_info() # Load persistent server state
  │   ├── server_birth()     # If new game: seed RNG, generate town, init stores
  │   ├── spells_init()      # Prepare spell tables
  │   ├── flavor_init()      # Randomize item flavors (e.g. "Bubbling potion")
  │   ├── reset_visuals()    # Reset visual mappings
  │   └── setup_network_server()
  │       ├── add_timer(dungeon_tick, ONE_SECOND / cfg_fps)   # Game tick timer
  │       ├── add_timer(second_tick, ONE_SECOND)               # Keepalive check
  │       ├── add_sender(meta, 8800, 4s, report_to_meta)      # UDP to meta
  │       ├── add_listener(cfg_tcp_port, accept_client)        # Game port
  │       ├── add_listener(cfg_tcp_port+1, accept_console)     # Console port
  │       ├── alloc_server_memory()  # Player arrays
  │       └── setup_tables()         # Packet handler tables
  │
  └── network_loop()         # INFINITE LOOP — never returns
```

### Memory Allocation

```c
MAX_PLAYERS = 1000   // Maximum concurrent connections
```

Four reference arrays map between connection indices (`ind`) and player indices (`Ind`):

| Array | Lookup | Returns |
|-------|--------|---------|
| `Conn[ind]` | Connection index → | `connection_type*` pointer |
| `PConn[Ind]` | Player index → | `connection_type*` pointer |
| `Get_Ind[ind]` | Connection index → | Player index |
| `Get_Conn[Ind]` | Player index → | Connection index |

Player indices (`Ind`) are 1-based and contiguous: `Players[1..NumPlayers]`. When a player leaves mid-list, the last player is swapped into their slot via `reindex_player()`.

---

## 7. Server Network Loop — The Infinite Loop

```c
void network_loop() {
    while (1) {
        handle_listeners(first_listener);       // Accept new TCP connections
        handle_connections(first_connection);    // Read/write all connections
        handle_senders(first_sender, delta_t);  // UDP sends (meta-server)
        handle_timers(first_timer, delta_t);    // Fire timers (game tick, etc.)

        post_process_players();                 // Execute buffered commands
        network_pause(2000);                    // select() with 0.002ms timeout
    }
}
```

### Processing Order Per Iteration

1. **Accept new connections** — `handle_listeners` calls `accept()` on the listen socket, invokes `accept_client()` callback for each new fd.

2. **Service existing connections** — `handle_connections` iterates all connections:
   - `recvfrom()` into `rbuf`
   - Call `receive_cb()` (which is `client_read`, `client_login`, `hub_read`, `websocket_receive`, or `console_read` depending on state)
   - `sendto()` from `wbuf`
   - Close and cleanup if `close` flag is set

3. **UDP sends** — `handle_senders` fires the meta-server reporter at its interval (4 seconds).

4. **Timers** — `handle_timers` fires:
   - `dungeon_tick` at `ONE_SECOND / cfg_fps` (~13.3ms at 75 FPS)
   - `second_tick` at `ONE_SECOND`

5. **Post-process players** — `post_process_players()` flushes all player command buffers immediately after network I/O (in addition to being triggered inside `dungeon_tick`).

6. **Sleep** — `network_pause(2000)` calls `select()` with a 2μs timeout, yielding the CPU briefly.

---

## 8. Connection Lifecycle

### Stage 1: Accept

```
Client connects to TCP_PORT (default 18346)
  ↓
accept_client(fd):
  - add_connection(fd, hub_read, hub_close)
  - denaglefd(fd)     // Disable Nagle's algorithm for low latency
```

### Stage 2: Hub Read (Connection Type Detection)

The first 2 bytes from the client determine the connection type:

```c
u16b conntype;
cq_scanf(&ct->rbuf, "%ud", &conntype);

switch (connection_type_ok(conntype)) {
    case CONNTYPE_PLAYER:    // 0x01 — Native client
        ct->receive_cb = client_login;
        send_play(ct, PLAYER_EMPTY);
        break;
    case CONNTYPE_WEBSOCKET: // 0x4745 ("GE" = start of "GET" HTTP)
        ct->receive_cb = websocket_handshake;
        break;
    case CONNTYPE_CONSOLE:   // 0x02 or various
        accept_console(ct);
        break;
    case CONNTYPE_OLDPLAYER: // 0x00 — Legacy version, rejected
        client_abort(ct, "Incompatible");
        break;
}
```

### Stage 3: Login (client_login)

```
Client sends: PKT_LOGIN + version(u16b) + real_name + host_name + nick_name + pass_word
  ↓
Server validates:
  1. client_version_ok(version)     — Must match server version class
  2. client_names_ok(nick, real, host) — ASCII only, no spaces at start, etc.
  3. scoop_player(nick, pass)       — Password check against savefile
  ↓
Server checks for reconnection:
  - If same nick already connected → player_drop() old, reuse player_type
  - If same nick in p_list (orphaned) → resume existing character
  - Otherwise → player_alloc() + load_player() or new character
  ↓
Add to players list: eg_add(players, ct, p_ptr)
  ↓
Switch callback: ct->receive_cb = client_read
  ↓
Send initial data burst:
  - send_stats_info(ct)          — Stat names (STR, INT, etc.)
  - send_race_info(ct)           — Available races
  - send_class_info(ct)          — Available classes
  - send_server_info(ct)         — Server capabilities (counts of indicators, streams, etc.)
  - send_inventory_info(ct)      — Inventory slot layout
  - send_objflags_info(ct)       — Object flag display
  - send_floor_info(ct)          — Floor item display
  - send_optgroups_info(ct)      — Option categories
  - send_char_info(ct, p_ptr)    — Character state (triggers client state machine)
```

### Stage 4: Active Play (client_read)

```c
int client_read(int data1, data data2) {
    connection_type *ct = data2;
    player_type *p_ptr = players->list[ct->user]->data2;

    p_ptr->idle = 0;  // Reset timeout

    while (cq_len(&ct->rbuf)) {
        pkt = CQ_GET(&ct->rbuf);   // Read packet type byte
        next_scheme = schemes[pkt]; // Look up expected format
        result = handlers[pkt](ct, p_ptr);  // Dispatch to handler

        if (result != 1) break;     // 0=incomplete, -1=error
    }

    if (result == 0) ct->rbuf.pos = start_pos;  // Rewind partial packet
    else if (result == 1) cq_slide(&ct->rbuf);   // Compact buffer
    return result;  // -1 kills connection
}
```

### Stage 5: Disconnect

Three disconnect scenarios:

| Cause | Mechanism | Effect |
|-------|-----------|--------|
| Client quits cleanly | `PKT_QUIT` received | `client_close()` → `player_drop()` |
| Network error | `recvfrom()` returns 0 or error | `ct->close = 1` → `client_close()` |
| Ping timeout | `second_tick`: `idle > 15` | `client_kill(ct, "Ping timeout")` → `player_leave()` |

**Player drop vs. leave:**
- `player_drop(ind)` — detaches the connection from the player. If the player is in-game (`PLAYER_PLAYING`), they enter `PLAYER_LEAVING` state and linger for up to 60 seconds before being removed.
- `player_leave(p_idx)` — fully removes the player from the game: saves character, clears dungeon position, broadcasts departure, swaps last player into the slot.

---

## 9. Player State Machine

```
PLAYER_EMPTY (0)    ──send login──▶  PLAYER_NAMED (1)
PLAYER_NAMED (1)    ──send char──▶   PLAYER_SHAPED (?)
                    ──load save──▶   PLAYER_FULL (4)
                    ──dead save──▶   PLAYER_BONE (3)
PLAYER_BONE (3)     ──reroll──▶      PLAYER_NAMED (1)
                    ──restart──▶     PLAYER_NAMED (1)
PLAYER_FULL (4)     ──send_play──▶   PLAYER_READY (?)
                    ──enter game──▶  PLAYER_PLAYING (6)
PLAYER_PLAYING (6)  ──disconnect──▶  PLAYER_LEAVING (7)
PLAYER_LEAVING (7)  ──reconnect──▶   PLAYER_PLAYING (6)
                    ──timeout──▶     (removed from game)
```

The state is stored in `p_ptr->state` and transmitted to the client via `PKT_CHAR_INFO`. The client mirrors this state to drive its setup UI (character creation, rolling, etc.).

---

## 10. Authentication and Login

### Password System

MAngband stores passwords **in player savefiles**, not in a separate database:

```
1. Client sends: PKT_LOGIN + version + real_name + host_name + nick_name + pass_word
2. Server calls: scoop_player(nick, pass)
   - Loads the savefile header for "nick"
   - Compares password hash
   - Returns <0 if password is wrong
3. On failure: client_abort(ct, "Incorrect password.")
4. On success: proceed to load or create character
```

### Name Validation

```c
bool client_names_ok(nick_name, real_name, host_name) {
    // Real name and host name: must not be empty
    // Replace non-ASCII non-printable chars with '?'
    // Nick name: only [a-zA-Z0-9 ], no leading space
    // Force capitalize first letter
    // Right-trim trailing spaces
    // Reserved name "server" is rejected
}
```

### Version Validation

Version is a 16-bit value packed as `0xMNPE` (Major.miNor.Patch.Extra):
```c
bool client_version_ok(u16b version) {
    // Extra (alpha/beta/devel/stable) must match exactly
    // Must be at least version 1.5.0
}
```

### Reconnection

If a player with the same nickname is already connected:
1. The old connection is killed with "Reconnect from other location"
2. The player pointer is kept and reattached to the new connection
3. If the player was `PLAYER_LEAVING`, they are resumed seamlessly

---

## 11. WebSocket Support

MAngband supports **RFC 6455 WebSocket** connections for browser-based clients.

### Detection

The first 2 bytes `0x4745` correspond to ASCII "GE" — the start of `GET /` from an HTTP upgrade request. This triggers the WebSocket handshake path.

### Handshake

```c
int websocket_handshake(int data1, data data2) {
    // Read HTTP headers line by line
    // Extract "Sec-WebSocket-Key" header
    // Concatenate with magic GUID "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
    // SHA-1 hash → Base64 encode → "Sec-WebSocket-Accept" header
    // Send HTTP 101 response:
    //   "HTTP/1.1 101 Switching Protocols\r\n"
    //   "Upgrade: websocket\r\n"
    //   "Connection: Upgrade\r\n"
    //   "Sec-WebSocket-Accept: <hash>\r\n"
    //   "\r\n"

    ct->receive_cb = websocket_receive;
}
```

### Frame Wrapping

After handshake, all data is wrapped in WebSocket frames:
- **Outbound** (`websocket_send`): adds frame header before each `wbuf` flush
- **Inbound** (`websocket_read`): strips frame header, extracts payload into a temporary buffer, then processes as normal TCP data via `websocket_exec`

The inner protocol is identical to raw TCP — the WebSocket layer is transparent. Only `CONNTYPE_PLAYER` connections are allowed over WebSocket.

---

## 12. The Game Tick — `dungeon()`

The `dungeon_tick` timer fires at `ONE_SECOND / cfg_fps` (default: 1,000,000 / 75 ≈ 13,333 μs). Each tick calls `dungeon()`:

```c
void dungeon(void) {
    // === END OF PREVIOUS TURN ===

    // 1. Check for player deaths (iterate backwards!)
    for (i = NumPlayers; i > 0; i--)
        if (Players[i]->death) player_death(Players[i]);

    // 2. Deallocate empty dungeon levels
    for (j = -MAX_WILD+1; j < MAX_DEPTH; j++)
        if (players_on_depth[j] == 0 && cave[j] && j != 0)
            dealloc_dungeon_level(j);

    // 3. Handle pending level transitions
    for (i = 1; i <= NumPlayers; i++)
        if (p_ptr->new_level_flag) {
            // Generate cave if needed
            // Clear fog of war
            // Place player in empty square
            // Remove nearby hounds (anti-instakill)
            // Recalculate panel, view, lighting
            p_ptr->new_level_flag = FALSE;
        }

    // 4. Compact object/monster lists if near capacity

    // 5. For each player: process_player_end()
    //    - Execute buffered commands (if enough energy)
    //    - Auto-retaliate against adjacent monsters
    //    - Handle running
    //    - Process timers (poison, regen, status effects)
    //    - Process inventory (recharge, fuel, etc.)
    //    - Word of recall

    // 6. Check for deaths again

    // === BEGIN NEW TURN ===
    turn++;

    // 7. For each player: process_player_begin()
    //    - Award energy based on speed

    // 8. process_monsters()
    //    - All monsters take their turns (AI, movement, spells, melee)

    // 9. process_objects()
    //    - Object timers, effects

    // 10. For each player: process_world()
    //     - Every 50 turns: day/night cycle, random monster spawn

    // 11. process_various()
    //     - Server save (every SERVER_SAVE minutes)
    //     - Unique respawn timers
    //     - Level unstatic
    //     - Store restocking
    //     - Town day/night cycle

    // 12. regen_monsters()
    //     - Every 100 turns

    // 13. For each player: handle_stuff()
    //     - Flush redraw flags → send display updates to client
}
```

### Important: Turn Order

`process_player_end()` runs **before** `process_player_begin()`. This means players spend energy from the previous turn before receiving energy for the new turn. This prevents "energy stacking" exploits.

---

## 13. Energy and Level Speed

### Level Speed Table

The energy cost of one action varies by dungeon depth:

```c
u32b level_speed(int Depth) {
    if (Depth <= 0) return level_speeds[0] * 5;  // Town: 37,500
    else return level_speeds[Depth] * 5;          // Dungeon: 45,000 - 100,000
}
```

| Depth | level_speeds[] | level_speed() (×5) | Effective Turn Length |
|-------|---------------|---------------------|---------------------|
| 0 (Town) | 7,500 | 37,500 | Fast — easy town navigation |
| 10 (500') | 9,900 | 49,500 | Slightly slower |
| 20 (1000') | 10,500 | 52,500 | Normal |
| 40 (2000') | 12,500 | 62,500 | Moderate |
| 60 (3000') | 15,000 | 75,000 | Slow |
| 80 (4000') | 19,000 | 95,000 | Very slow |
| 100+ | 20,000 | 100,000 | Slowest |

### Energy Accrual (`process_player_begin`)

```c
energy = extract_energy[p_ptr->pspeed];   // Speed table lookup
energy *= (p_ptr->bubble_speed / 100.0);  // Time bubble modifier
if (running) energy *= (RUNNING_FACTOR / 100.0);  // Bonus while running
p_ptr->energy += energy;

// Cap energy at level_speed (one full action)
if (p_ptr->energy > level_speed(depth))
    p_ptr->energy = level_speed(depth);
```

A player can act when `p_ptr->energy >= level_speed(depth)`. Deeper levels require more energy per action, effectively making gameplay slower and more deliberate.

### Energy Buildup (optional)

With the `ENERGY_BUILDUP` option, excess energy carries over between turns:
```c
p_ptr->energy_buildup = min(excess, level_speed(depth));
```
This allows players to "save up" energy when idle, then act immediately.

---

## 14. Command Buffering and Execution

### Two-Phase Command Processing

Commands are not executed immediately when received. Instead:

1. **Phase 1 — Receive** (`recv_command` in `net-game.c`):
   - Client sends `PKT_WALK`, `PKT_REST`, etc.
   - `recv_command()` copies the packet from the connection's `rbuf` into the player's `cbuf` (command buffer)
   - The command buffer is a separate `cq` of `PD_SMALL_BUFFER` (4 KB)

2. **Phase 2 — Execute** (`process_player_commands` in `net-game.c`):
   - Called from `process_player_end()` during the game tick
   - Also called from `post_process_players()` after network I/O
   - Reads commands from `cbuf` one at a time
   - Each command's handler returns:
     - **2** = success, continue processing more commands
     - **1** = success, stop (action taken, energy spent)
     - **0** = not enough energy, rewind buffer (try again next tick)
     - **-1** = fatal error

```c
int process_player_commands(player_type *p_ptr) {
    while (cq_len(&p_ptr->cbuf)) {
        pkt = CQ_GET(&p_ptr->cbuf);
        do_cmd__before(p_ptr, pkt);          // Reset AFK, disturb if resting
        result = pcommands[pkt](p_ptr);      // Execute
        if (result) do_cmd__after(p_ptr, pkt, result);  // Noise, energy reset
        if (!(result >= 1 && result <= 2)) break;
    }
    if (result == 0) p_ptr->cbuf.pos = start_pos;  // Rewind
    return result;
}
```

### Energy Cost System

Each command has an `energy_cost` field:
- **0** = free (chat, look, redraw)
- **1** = standard action cost (1 × level_speed)
- **N** = fractional (1/Nth of level_speed per blow in multi-attack)

After energy-costing commands execute, `energy_buildup` is reset to prevent exploits.

---

## 15. Data Streams (Display)

MAngband uses a **stream system** to send map and visual data to clients. Streams are the primary mechanism for rendering the dungeon on the client.

### Stream Definition

```c
struct stream_type {
    byte pkt;          // Network packet ID (dynamically assigned from PKT_STREAM range)
    byte addr;         // Destination window/term
    byte rle;          // RLE compression mode
    byte flag;         // Flags (SF_TRANSPARENT, etc.)
    u16b min_row;      // Minimum dimensions
    byte min_col;
    u16b max_row;      // Maximum dimensions
    byte max_col;
    u32b window_flag;  // Which window displays this
    cptr mark;         // Internal name
    cptr window_desc;  // Display name
};
```

### Subscription Model

The client **subscribes** to streams by sending `PKT_RESIZE` with desired dimensions:
```c
send_stream_size(stream_id, rows, cols);
```

The server tracks subscription state per player:
```c
p_ptr->stream_wid[st]  // Subscribed width (0 = unsubscribed)
p_ptr->stream_hgt[st]  // Subscribed height
```

### Sending Map Data

Map data is sent as rows of (attr, char) pairs:

```c
// Single cell update:
stream_char(p_ptr, stream_id, y, x);

// Full row update:
stream_line(p_ptr, stream_id, y);
```

Each cell is packed as `(y << 8 | x)` with a high bit flag for single-cell vs. row mode.

### Stream Negotiation

During login, the server sends `PKT_STREAM` info for each available stream. The client responds with `PKT_RESIZE` for streams it wants. The server only sends data for subscribed streams.

---

## 16. Indicators (HUD)

Indicators are the server-driven HUD elements (HP, MP, stats, depth, gold, etc.).

### Indicator Definition

```c
struct indicator_type {
    byte pkt;      // Network packet ID (from PKT_INDICATOR range)
    byte type;     // INDITYPE_TINY (byte), INDITYPE_NORMAL (s16b), INDITYPE_LARGE (s32b), INDITYPE_STRING
    byte amnt;     // Number of values per update
    byte win;      // Destination window
    s16b row, col; // Display position
    u32b flag;     // Flags
    cptr prompt;   // Display format/label
    u64b redraw;   // PR_ flag that triggers this indicator
    cptr mark;     // Internal name
};
```

### How Indicators Work

1. Game logic sets `p_ptr->redraw |= PR_HP` (or similar flag)
2. `handle_stuff(p_ptr)` checks redraw flags
3. For each dirty indicator, calls `send_indication(p_ptr, indicator_id, value1, value2, ...)`
4. Client receives the indicator packet and updates the appropriate screen position

### Indicator Negotiation

Like streams, indicators are negotiated during setup:
- Server sends `PKT_INDICATOR` info for each indicator
- Client requests missing indicators via `PKT_BASIC_INFO + RQ_INDI`
- Server sends definitions incrementally until all are received

---

## 17. Custom Commands

MAngband uses a **custom command** system to define game commands declaratively rather than hardcoding each one:

```c
struct custom_command_type {
    char m_catch;          // Key the client maps to this command (e.g. 'j' for steal)
    char pkt;              // Packet type (0 = auto-assigned)
    byte scheme;           // Payload format (SCHEME_ITEM, SCHEME_DIR, etc.)
    byte energy_cost;      // Energy cost (0 = free, N = 1/Nth of level_speed)
    cccb do_cmd_callback;  // Server-side function to execute
    u32b flag;             // Command flags
    byte tval;             // Item type filter
    char prompt[MSG_LEN];  // Prompt text
    char display[MAX_CHARS]; // Display name
};
```

Custom commands are defined in `tables.c` and transmitted to the client during setup. The client learns what keys map to what commands dynamically, allowing the server to add new commands without client updates.

### Command Flow

```
Client: User presses 'j'
  → Client looks up custom command for 'j'
  → Client sends PKT_COMMAND + command_id + payload (per scheme)
  ↓
Server: recv_command()
  → Copies PKT_COMMAND + id + payload into p_ptr->cbuf
  ↓
Server: process_player_commands()
  → Reads command from cbuf
  → Dispatches to do_cmd_callback
  → Returns result (energy spent or not)
```

---

## 18. Message and Chat System

### Channels

MAngband supports **multiple chat channels**:

```c
struct channel_type {
    char name[MAX_CHARS];  // e.g. "#public", "#party"
    s32b id;               // Unique ID
    s32b num;              // Number of members
    byte mode;             // Channel mode flags
};
```

- Default channel: `#public` (all players auto-join)
- Party channel: automatic for party members
- Players can join/leave channels

### Message Flow

```
Client sends:  PKT_MESSAGE + text string
  ↓
Server recv_message():
  - If text starts with ':' → command (e.g. ":party create Foo")
  - If text starts with '@' → private message
  - Otherwise → broadcast to current channel
  ↓
Server sends to recipients:  PKT_MESSAGE + type + text
```

### Message Types

Messages have a `type` field used for sound and color:
```
MSG_GENERIC, MSG_HIT, MSG_MISS, MSG_FLEE, MSG_DROP, MSG_KILL,
MSG_LEVEL, MSG_DEATH, MSG_STUDY, MSG_TELEPORT, MSG_SHOOT, ...
```

### Message Repeat

To reduce bandwidth, `PKT_MESSAGE_REPEAT` tells the client to repeat the last message N times instead of sending duplicate strings.

---

## 19. Party System

### Party Structure

```c
struct party_type {
    char name[80];     // Party name
    char owner[20];    // Owner's character name
    s32b num;          // Member count
    hturn created;     // Creation timestamp
};
```

Maximum `MAX_PARTIES` parties can exist simultaneously.

### Party Operations

| Operation | Command | Rules |
|-----------|---------|-------|
| **Create** | `:party create <name>` | Player must not be in a party; name must be unique |
| **Add** | `:party add <player>` | Only owner can add; target must be partyless and non-hostile |
| **Remove** | `:party remove <player>` | Only owner can remove |
| **Leave** | `:party leave` | Anyone can leave; if owner leaves, party is disbanded |

### Party Effects

- Party members **cannot damage each other** in PvP (depending on hostility settings)
- Party members share the same dungeon level persistence
- Party chat channel is automatic
- Experience sharing is **not** implemented in MAngband 1.5.3 (each player gets full XP for their kills)

---

## 20. PvP and Hostility System

### Hostility List

Each player maintains a linked list of `hostile_type` entries:

```c
struct hostile_type {
    s32b id;            // ID of the hostile player
    hostile_type *next; // Next in linked list
};
```

### PvP Resolution (`pvp_okay()`)

The `pvp_okay()` function is called before every instance of player-to-player damage. It returns `TRUE` if damage should be dealt:

```c
bool pvp_okay(player_type *attacker, player_type *target, int mode) {
    // mode: 0=test, 1=melee, 2=direct ranged, 3=indirect ranged

    int hostility = cfg_pvp_hostility;  // Server-wide setting

    // Check safe zones (depth, wilderness radius, level difference)
    if (in_safe_zone) hostility = cfg_pvp_safehostility;

    switch (hostility) {
        case 3:  return FALSE;                           // NEVER fight
        case 2:  return both_hostile_to_each_other;      // SAFE
        case 1:  return attacker_hostile; auto_retaliate; // NORMAL
        case 0:  return TRUE_for_indirect; auto_hostile;  // DANGEROUS
        case -1: return !same_party;                      // BRUTAL
    }
}
```

### Hostility Levels (Server Config)

| Level | Name | Behavior |
|-------|------|----------|
| **3** | Peaceful | Players NEVER fight |
| **2** | Safe (default) | Both must be hostile to each other (1.1.0 style) |
| **1** | Normal | Attacker must be hostile; target auto-retaliates |
| **0** | Dangerous | No hostility needed for splash damage (0.7.2 style) |
| **-1** | Brutal | Everyone fights unless in the same party |

### Safe Zones

The server can configure safe zones where a stricter hostility level applies:
- `PVP_SAFEDEPTH` — dungeon depth threshold
- `PVP_SAFERADIUS` — wilderness radius from town
- `PVP_SAFELEVEL` — level difference threshold

---

## 21. Player Visibility

### Per-Player Visibility Arrays

Each player maintains visibility data for every other player and monster:

```c
bool play_vis[MAX_PLAYERS+1];  // Can I see this player?
bool play_los[MAX_PLAYERS+1];  // Do I have line-of-sight to this player?
byte play_det[MAX_PLAYERS+1];  // Detect spell fade timer for this player
bool mon_vis[MAX_M_IDX];       // Can I see this monster?
byte mon_det[MAX_M_IDX];       // Detect spell fade timer for this monster
```

### Updating Visibility

```c
update_player(p_ptr)      // Recalculate other players' visibility of p_ptr
update_players()          // Recalculate all player-player visibility
update_monsters(TRUE)     // Recalculate all monster visibility
everyone_lite_spot(Depth, y, x)  // Notify all players on Depth that (y,x) changed
```

When a player moves:
1. Clear old position: `cave[Depth][old_y][old_x].m_idx = 0`
2. Set new position: `cave[Depth][new_y][new_x].m_idx = 0 - player_index` (negative = player)
3. `everyone_lite_spot()` on both old and new positions
4. `update_player()` to recalculate who can see this player

### Cave Grid Tracking

Each cave cell stores either a monster index (positive) or a player index (negative):
```c
struct cave_type {
    byte info;     // Flags (CAVE_GLOW, CAVE_ROOM, CAVE_ICKY, etc.)
    byte feat;     // Terrain feature
    s16b o_idx;    // Object index (or 0)
    s16b m_idx;    // Monster index (positive), Player index (negative), or 0
};
```

### Per-Player Fog of War

Each player has a private `cave_flag` array:
```c
byte cave_flag[MAX_HGT][MAX_WID];  // CAVE_MARK = explored, CAVE_SEEN = currently visible
```

The server only sends map data for cells the player can currently see (via streams). Fog of war is entirely server-authoritative.

---

## 22. Shared Dungeon and Level Persistence

### One Cave Per Depth

All players on the same dungeon depth share the same `cave[Depth]` array. There are no instanced dungeons.

```c
cave_type **cave[MAX_DEPTH];  // cave[depth][y][x]
s16b players_on_depth[MAX_DEPTH];  // Count of players per depth
```

### Level Generation

A level is generated on-demand when the first player enters:
```c
if (players_on_depth[Depth] && !cave[Depth]) {
    alloc_dungeon_level(Depth);
    generate_cave(p_ptr, Depth, auto_scum);
}
```

### Level Persistence ("Static" Levels)

Levels persist as long as `players_on_depth[depth] > 0`:
- A player descending stairs increments `players_on_depth[new_depth]` and decrements `players_on_depth[old_depth]`
- When `players_on_depth[depth]` reaches 0 and no player is physically on it, the level is eventually deallocated
- The `cfg_level_unstatic_chance` option provides random "unstaticing" of empty levels to prevent permanent persistence

### Town and Special Levels

- Town (`depth == 0`) is **never** deallocated
- Special levels (quests) are also preserved
- Wilderness levels (`depth < 0`) are deallocated daily at dawn with monster wipe

---

## 23. Keepalive and Timeout

### Server-Side Timeout

The `second_tick` timer fires every second:

```c
int second_tick(int data1, data data2) {
    for (i = 1; i < p_max + 1; i++) {
        if (p_list[i]->idle++ > 15) {
            // 15 seconds without input → disconnect
            if (p_list[i]->conn != -1)
                client_kill(ct, "Ping timeout");
            player_leave(i);
        }
        p_list[i]->afk_seconds++;
    }
}
```

The `idle` counter is reset to 0 every time `client_read()` receives data.

### Client-Side Keepalive

The client sends `PKT_KEEPALIVE` every second:

```c
int keepalive_timer(int data1, data data2) {
    if (recd_pings == sent_pings) {
        send_keepalive(sent_pings++);  // Send
    } else {
        lag_mark = 10000;  // Display lag warning
    }
}
```

The server echoes `PKT_KEEPALIVE` back with the same timestamp. The client measures round-trip time and displays a lag meter.

### Starvation Disconnect

If a player is AFK while starving (food < PY_FOOD_STARVE) for >= `DISCONNECT_STARVING` seconds, the server forcibly disconnects them.

---

## 24. Unique Monster Respawning (Multiplayer)

In single-player Angband, killed uniques stay dead. In MAngband, uniques respawn:

```c
// Every minute, check each dead unique:
if (r_ptr->flags1 & RF1_UNIQUE && r_ptr->max_num == 0) {
    // Morgoth (DROP_CHOSEN) never respawns
    if (r_ptr->flags1 & RF1_DROP_CHOSEN) continue;

    // Set timer on first check after death
    if (r_ptr->respawn_timer < 0) {
        r_ptr->respawn_timer = cfg_unique_respawn_time * (r_ptr->level + 1);
        r_ptr->respawn_timer = MIN(r_ptr->respawn_timer, cfg_unique_max_respawn_time);
    }

    // Decrement each minute
    r_ptr->respawn_timer--;

    // Respawn when timer reaches 0
    if (!r_ptr->respawn_timer) {
        r_ptr->max_num = 1;         // Alive again
        r_ptr->respawn_timer = -1;  // Reset
    }
}
```

Timer formula: `cfg_unique_respawn_time × (monster_level + 1)`, capped at `cfg_unique_max_respawn_time`.

**Per-player kill tracking**: each player has `r_killed[]` to track which uniques they've personally killed. A unique only spawns on a level if at least one player on that level hasn't killed it.

---

## 25. Meta-Server Reporting

MAngband servers can register with a central meta-server for server discovery:

```c
// UDP sender fires every 4 seconds:
first_sender = add_sender(NULL, cfg_meta_address, 8800, ONE_SECOND * 4, report_to_meta);
```

The report is a simple text datagram:
```
<hostname>:<port> Number of players: <N> Names: <name1> <name2> ... Version: <major>.<minor>.<patch>
```

On shutdown, a special death report (address + whitespace) is sent to deregister.

---

## 26. Admin Console

The server listens on `TCP_PORT + 1` for admin console connections:

```c
add_listener(first_listener, cfg_tcp_port + 1, accept_console);
```

### Console Protocol

The console uses simple text-based commands over TCP:
1. Authenticate with `CONSOLE_PASSWORD` (from `mangband.cfg`)
2. Send text commands, receive text responses

### Console Commands

Available commands include:
- Player management (kick, ban)
- Server state (player list, shutdown timer)
- Chat monitoring (listen to channels)
- Dungeon master operations

The console connection uses a separate `console_connection` struct that wraps the standard `connection_type` with auth state and channel subscriptions.

---

## 27. Client Architecture

The client is a **thin display terminal** — it has no game logic. All it does is:
1. Send player inputs to the server
2. Receive display updates and render them

### Client Components

| Component | File | Purpose |
|-----------|------|---------|
| **Main loop** | `c-init.c` | `Game_loop()` — network + input + render cycle |
| **Network** | `net-client.c` | TCP connection, send/recv functions |
| **Input** | `c-cmd.c`, `c-cmd0.c` | Keyboard → packet translation |
| **Display** | `z-term.c` + `main-*.c` | Virtual terminal grid + platform renderers |
| **UI** | `ui.c` | High-level UI (menus, prompts, file browser) |
| **Inventory** | `c-inven.c` | Inventory/equipment display |
| **Store** | `c-store.c` | Shop interface |
| **Spells** | `c-spell.c` | Spell list display |

### Display Modules

| Module | Platform | Notes |
|--------|----------|-------|
| `main-sdl.c` | SDL 1.2 | Cross-platform graphical |
| `main-sdl2.c` | SDL 2.0 | Modern cross-platform graphical |
| `main-gcu.c` | ncurses | Terminal/console |
| `main-win.c` | Windows GDI | Native Windows |
| `main-x11.c` | X11 | Unix/Linux |
| `main-crb.c` | macOS Carbon | Legacy macOS |

### z-term: Virtual Terminal

The client uses an abstract `term` (virtual terminal grid) as its rendering surface:
- Multiple terms can be active simultaneously (main view, inventory, monster list, etc.)
- Each term is a 2D array of (attr, char) pairs
- Display modules (`main-*.c`) render terms to actual screen pixels
- The server sends stream data that populates specific terms

---

## 28. Client Network Loop

```c
void network_loop() {
    handle_connections(first_connection);  // Read/write server connection
    handle_callers(first_caller);          // Pending connection attempts
    handle_timers(first_timer, delta_t);   // Keepalive timer
    network_pause(1000);                   // select() with 1μs timeout
}
```

### Client Game Loop

```c
static void Game_loop(void) {
    while (1) {
        network_loop();          // Process network I/O
        request_command(FALSE);  // Check for keyboard input
        while (command_cmd) {
            process_command();   // Send command packet to server
            command_cmd = 0;
            request_command(FALSE);
        }
        process_requests();      // Handle server-side prompts
        flush_now();             // Flush input queue
        flush_updates();         // Render: redraw_stuff(), window_stuff(), Term_fresh()
    }
}
```

### Key difference from server

The client loop runs **as fast as possible**, limited only by `network_pause()` and display refresh. It does not have a fixed tick rate — it simply reacts to server updates and user input.

---

## 29. Client Setup and Data Sync

### Setup State Machine (`Setup_loop`)

After connecting, the client enters a setup loop that negotiates game data:

```
1. send_handshake(CONNTYPE_PLAYER)
   ↓
2. Server sends PKT_PLAY(PLAYER_EMPTY)
   → Client sends PKT_LOGIN (credentials)
   ↓
3. Server sends initial data burst:
   - Stats, races, classes, server info, inventory layout, options
   - PKT_CHAR_INFO with player state
   ↓
4. sync_data() loop:
   - Request indicators: send_request(RQ_INDI, count)
   - Request streams: send_request(RQ_STRM, count)
   - Request commands: send_request(RQ_CMDS, count)
   - Request item testers: send_request(RQ_ITEM, count)
   - Request options: send_request(RQ_OPTS, count)
   - Each request gets a batch response
   - Loop until all data received
   ↓
5. client_setup() — configure visual preferences
   send_visual_info() — send flavor/object/monster/terrain visuals
   send_options() — send player options
   send_settings() — send client settings
   ↓
6. send_play(PLAY_ENTER) — request to enter the game
   ↓
7. Server: player_enter() — add player to game world
   → Sends PKT_PLAY(PLAYER_PLAYING)
   ↓
8. client_ready() — initialize display
   send_play(PLAY_PLAY) — acknowledge
   ↓
9. → Game_loop()
```

### Incremental Data Sync

The `sync_data()` function is called every network iteration during setup. It tracks what has been requested vs. received and sends incremental requests:

```c
bool sync_data(void) {
    bool data_ready = TRUE;
    sync_data_piece(RQ_INDI, &asked_indicators, known_indicators, serv_info.val1, &data_ready);
    sync_data_piece(RQ_STRM, &asked_streams, known_streams, serv_info.val2, &data_ready);
    sync_data_piece(RQ_CMDS, &asked_commands, custom_commands, serv_info.val3, &data_ready);
    sync_data_piece(RQ_ITEM, &asked_testers, known_item_testers, serv_info.val4, &data_ready);
    sync_data_piece(RQ_OPTS, &asked_options, known_options, options_max, &data_ready);
    return data_ready;
}
```

---

## 30. Server Configuration

The `mangband.cfg` file controls all server-side settings:

### Network Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `TCP_PORT` | 18346 | Main game port (console = port + 1) |
| `REPORT_TO_METASERVER` | false | Register with meta-server |
| `META_ADDRESS` | "mangband.org" | Meta-server hostname |
| `BIND_NAME` / `BIND_IP` | — | Bind to specific interface |

### Security

| Setting | Default | Description |
|---------|---------|-------------|
| `CONSOLE_PASSWORD` | (must set) | Admin console password |
| `CONSOLE_LOCAL_ONLY` | false | Only accept local console connections |
| `DUNGEON_MASTER_NAME` | "Gandalf" | Name of the DM character |
| `SECRET_DUNGEON_MASTER` | true | Hide DM from player list |

### Gameplay

| Setting | Default | Description |
|---------|---------|-------------|
| `FPS` | 75 | Server tick rate (frames per second) |
| `IRONMAN` | false | Ironman mode (no recall, etc.) |
| `NEWBIES_CANNOT_DROP` | true | Level 1 characters can't drop items |
| `NO_STEAL` | false | Disable player stealing |
| `SAFE_RECHARGE` | false | Don't destroy wands on failed recharge |

### PvP Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `PVP_HOSTILITY` | 2 | Main PvP mode (see §20) |
| `PVP_NOTIFY` | true | Warn when someone becomes hostile |
| `PVP_SAFEHOSTILITY` | — | Hostility level in safe zones |
| `PVP_SAFEDEPTH` | — | Safe zone depth limit |
| `PVP_SAFERADIUS` | — | Safe zone wilderness radius |
| `PVP_SAFELEVEL` | — | Safe zone level difference |

---

## 31. Key Algorithms Summary

### Algorithm 1: Server Main Loop (Infinite)

```
LOOP FOREVER:
  1. handle_listeners()     — accept new TCP connections
  2. handle_connections()   — for each connection:
       a. recv() → rbuf
       b. receive_cb(rbuf)  — parse packets, dispatch handlers
       c. send(wbuf)        — flush outbound data
       d. close if flagged
  3. handle_senders()       — UDP meta-server report
  4. handle_timers()        — fire game tick and second tick
  5. post_process_players() — flush all command buffers
  6. select(0.002ms)        — yield CPU
```

### Algorithm 2: Connection Handshake

```
CLIENT                          SERVER
  │                               │
  │──── TCP connect ────────────▶ │ accept_client(fd)
  │                               │ → add_connection(fd, hub_read)
  │                               │
  │──── conntype (2 bytes) ─────▶ │ hub_read()
  │                               │ → detect PLAYER/WEBSOCKET/CONSOLE
  │                               │ → switch to client_login callback
  │                               │
  │◀──── PKT_PLAY(EMPTY) ─────── │ send_play(PLAYER_EMPTY)
  │                               │
  │──── PKT_LOGIN ──────────────▶ │ client_login()
  │     version+real+host+nick    │ → validate version, name, password
  │     +password                 │ → load/create character
  │                               │ → switch to client_read callback
  │                               │
  │◀──── burst: races, classes,   │ send_race_info, send_class_info, etc.
  │       server info, char_info  │
  │                               │
  │──── sync requests ──────────▶ │ (indicators, streams, commands, options)
  │◀──── sync responses ──────── │
  │                               │
  │──── visual info, options ───▶ │
  │──── PKT_PLAY(ENTER) ───────▶ │ player_enter()
  │                               │ → add to p_list, setup location
  │                               │ → broadcast "X has entered the game"
  │◀──── PKT_PLAY(PLAYING) ───── │
  │                               │
  │══════ GAMEPLAY ══════════════ │
```

### Algorithm 3: Game Tick (dungeon)

```
EVERY (1/FPS) SECONDS:
  // End of previous turn
  FOR EACH dead player: player_death()
  deallocate empty levels
  handle pending level transitions (generate, place)
  FOR EACH player: process_player_end()
    → execute buffered commands (if enough energy)
    → auto-retaliate, handle running
    → process status timers, regen, inventory
    → word of recall
  check deaths again

  // Begin new turn
  turn++
  FOR EACH player: process_player_begin()
    → energy += extract_energy[speed]
    → cap energy at level_speed(depth)
  process_monsters()     → all monster AI/actions
  process_objects()      → object timers
  FOR EACH player: process_world()
    → every 50 turns: day/night, random monster spawn
  process_various()      → saves, unique timers, stores
  regen_monsters()       → every 100 turns
  FOR EACH player: handle_stuff()
    → flush display updates (streams + indicators)
```

### Algorithm 4: Packet Dispatch

```
RECEIVE CALLBACK (client_read):
  WHILE bytes in rbuf:
    pkt_type = read 1 byte
    scheme = lookup_table[pkt_type]
    handler = handler_table[pkt_type]
    result = handler(connection, player)
    IF result == 0: rewind, wait for more bytes
    IF result == -1: kill connection
    IF result == 1: continue to next packet
  compact rbuf
```

### Algorithm 5: Command Buffering

```
NETWORK receives PKT_WALK:
  recv_command():
    copy packet header + payload → player.cbuf

GAME TICK process_player_commands():
  WHILE commands in cbuf:
    read packet type
    check energy requirement
    IF enough energy:
      execute command handler
      deduct energy
    ELSE:
      rewind cbuf, try again next tick
```

### Algorithm 6: PvP Resolution

```
pvp_okay(attacker, target, mode):
  hostility_level = server_config
  IF in_safe_zone: hostility_level = safe_config

  SWITCH hostility_level:
    3 (PEACEFUL):  RETURN FALSE
    2 (SAFE):      RETURN both_are_hostile_to_each_other
    1 (NORMAL):    RETURN attacker_is_hostile (auto-set target hostile)
    0 (DANGEROUS): RETURN TRUE for indirect (auto-set both hostile)
    -1 (BRUTAL):   RETURN NOT same_party
```

---

## 32. IronHell Adoption Guide

### What to Adopt vs. What to Replace

| MAngband Mechanism | Adopt? | IronHell Approach |
|-------------------|--------|-------------------|
| TCP raw sockets | **Replace** | Use Socket.IO over WebSocket for browser + Electron |
| `cq_printf` / `cq_scanf` binary serialization | **Replace** | Use JSON or MessagePack over Socket.IO |
| `select()` event loop | **Replace** | Use Node.js event loop (async/await) |
| Authoritative server model | **Adopt** | All game state on server; client is display-only |
| Energy-based turn system | **Adopt** | Already implemented (see SYSTEMS_COMPENDIUM) |
| Command buffering (cbuf) | **Adopt** | Buffer client commands, execute when energy available |
| Stream subscription model | **Adapt** | Send visible map region per player; delta updates |
| Indicator system | **Adapt** | Send HUD data as structured state updates |
| Player state machine | **Adopt** | EMPTY → NAMED → FULL → PLAYING → LEAVING |
| Party system | **Adopt** | Party CRUD with owner, shared dungeon levels |
| PvP hostility system | **Adopt** | Configurable hostility levels, safe zones |
| Shared dungeon levels | **Adopt** | Same cave for all players on same depth |
| Per-player fog of war | **Adopt** | Server tracks what each player has seen |
| Player visibility arrays | **Adopt** | Per-player tracking of who can see whom |
| Unique respawning | **Adopt** | Timer-based respawn for multiplayer |
| Keepalive / timeout | **Adopt** | Heartbeat with 15-second timeout |
| Level persistence (static) | **Adopt** | Keep level alive while players are on it |
| WebSocket support | **Native** | IronHell is browser-first; WS is primary transport |
| Admin console | **Adapt** | Use Socket.IO admin namespace or REST API |
| Meta-server reporting | **Optional** | Could use a lobby server for server discovery |

### Recommended Architecture for IronHell

```
┌─────────────────────────────────────────────┐
│              IronHell Server                  │
│           (Node.js + TypeScript)              │
│                                              │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐ │
│  │ Socket.IO│   │ Game     │   │ Zustand   │ │
│  │ Server   │──▶│ Loop     │──▶│ Store     │ │
│  │          │   │ (energy  │   │ (state)   │ │
│  └────┬─────┘   │  ticks)  │   └──────────┘ │
│       │         └──────────┘                 │
│       │  ┌──────────────────────────────┐    │
│       └──│  Per-Player State            │    │
│          │  - Command buffer            │    │
│          │  - Fog of war                │    │
│          │  - Visibility arrays         │    │
│          │  - Stream subscriptions      │    │
│          └──────────────────────────────┘    │
└──────────┬──────────────────────────┬────────┘
           │  Socket.IO (WebSocket)   │
      ┌────▼────┐              ┌─────▼───┐
      │ React   │              │ React   │
      │ Client  │              │ Client  │
      │ (Vite)  │              │ (Vite)  │
      └─────────┘              └─────────┘
```

### Key Protocol Design Decisions

1. **Use Socket.IO events instead of raw packet bytes** — each MAngband PKT_* becomes a named Socket.IO event with a JSON payload
2. **Use rooms for dungeon levels** — all players on depth N join room `depth:N`; broadcasts go to the room
3. **Use acknowledgments** — Socket.IO ack callbacks replace the custom keepalive mechanism
4. **Delta updates** — instead of sending full map rows, send only changed cells
5. **State reconciliation** — client Zustand store is a projection of server state, updated via Socket.IO events

### Packet → Socket.IO Event Mapping

| MAngband Packet | Socket.IO Event | Payload |
|----------------|-----------------|---------|
| `PKT_LOGIN` | `auth:login` | `{ nick, pass, version }` |
| `PKT_PLAY` | `game:state` | `{ state: "playing" }` |
| `PKT_WALK` | `cmd:walk` | `{ direction: 4 }` |
| `PKT_MESSAGE` | `chat:message` | `{ text, channel }` |
| `PKT_PARTY` | `party:action` | `{ action: "create", name }` |
| `PKT_STREAM` (map) | `stream:map` | `{ cells: [{y,x,attr,char}] }` |
| `PKT_INDICATOR` (HP) | `indicator:hp` | `{ current: 50, max: 100 }` |
| `PKT_INVEN` | `inventory:update` | `{ slot, item }` |
| `PKT_KEEPALIVE` | Socket.IO built-in ping | Automatic |
| `PKT_QUIT` | `disconnect` | Socket.IO built-in |
