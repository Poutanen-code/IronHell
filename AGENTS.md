# IronHell AI Agent Rules

- Read `Architecture.md` and relevant ADRs before changing code.
- Make one bounded change at a time; avoid unrelated refactoring.
- Keep `IronHell.Core` deterministic and independent of Godot, networking, files, and databases.
- Preserve server authority: clients send commands; the server validates and changes state.
- Use stable IDs; keep static definitions separate from runtime instances.
- Do not invent MAngband parity behavior. Mark it as verified, inferred, IronHell-specific, or unresolved.
- Keep simulation time explicit and independent of frame rate, wall-clock time, and Godot callbacks.
- Prefer a simple, elegant, easy to read adequate implementation. Do not add speculative abstractions or infrastructure. 
- Aim for performance and cost efficiency. Remember if there are thousands of players running the code simultaneously online. 
- Spot code smells in the pre-existing code areas. Fix them right there and then, don't leave to the later moments. In this refactoring aim for simle, elegant implementation. Less code is better than more code. 
- Do not add or upgrade dependencies unless the task explicitly requires and justifies it.
- Add or update fast, deterministic unit tests for every rule, validator, and bug fix which actually needs automatic testing. 
- Use real integration tests only at boundaries such as SQLite, serialization, and networking.
- Test public behavior, not private implementation. Do not weaken tests merely to make them pass.
- Use injected randomness; failing randomized tests must report a reproducible seed.
- Run the smallest affected test project first, then the full fast suite.
- Exclude slow/integration tests from the inner loop unless the change affects them.
- Keep successful test output quiet; never paste full logs. For failures, show only failing test names and essential error lines.
- A behavior-changing task is incomplete until required tests pass.
- Invalid definitions fail startup with clear errors; expected gameplay rejection uses explicit results.
- Persistent schema changes require explicit migrations; protocol and data formats must be versioned.
- Record source, version, license, and transformation for imported data and assets.
- Do not commit generated logs, temporary files, build output, secrets, or credentials.

## Completion Report

Report only:

- Summary
- Changed files
- Tests: focused and full fast-suite result
- Assumptions
- Remaining risks
- Spotted code smells and potential areas of improvement for effisiency.

Do not paste successful build or test logs.
