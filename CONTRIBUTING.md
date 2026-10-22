# Contributing to Factory Frenzy — PC Multiplayer Game

Thank you for contributing to **Factory Frenzy PC**, the Unity multiplayer obstacle-course game. Players race through levels made in the VR Editor; the PC application loads those levels from JSON.

## Getting started

1. Clone the PC Multiplayer repository.
2. Open it with the Unity version recorded in `ProjectSettings/ProjectVersion.txt` or the repository `README.md`.
3. Restore packages using Unity Package Manager.
4. Open the game's main scene and test a local session before editing.
5. When changing multiplayer behavior, test with a host and at least one client.

The project uses Unity Netcode for GameObjects. Avoid introducing network logic that works only in single-player mode.

## Branches and commits

- Keep `main` stable and ready for demonstration.
- Work on feature branches such as `feature/pc-checkpoints`, `fix/pc-respawn-sync`, or `docs/pc-lobby`.
- Use focused commits, for example `feat(pc): add checkpoint respawn` or `fix(netcode): sync race countdown`.
- Open a pull request for review before merging into `main`.
- Resolve merge conflicts and rerun relevant multiplayer tests before merging.

## Scope of contributions

The PC application covers the following specification features:

**Required features**

- Multiplayer third-person character controller (`PC-PLA-P1`).
- Start platform, countdown, and movement lock (`PC-PLA-P2`).
- Finish platform, rankings, and end-of-race results (`PC-PLA-P3`).
- Standard platforms and fall/respawn handling (`PC-PLA-P4`).
- Launcher trap with detection, aiming, and firing states (`PC-TRA-T2`).
- Multiplayer lobby (`PC-LOB-L1`).

**Additional features**

- Moving platform (`PC-PLA-P5`).
- Trampoline (`PC-PLA-P6`).
- Checkpoints (`PC-PLA-P7`).
- Bumper trap (`PC-TRA-T1`).
- Fan trap (`PC-TRA-T3`).
- Importing levels created by the VR Editor (`PC-LOB-L2`).

## Code and Unity conventions

- Use `PascalCase` for C# classes and methods, and `camelCase` for local variables and private fields.
- Prefer private `[SerializeField]` fields to unnecessary public fields.
- Keep network state, player movement, race rules, UI, and level loading responsibilities separated.
- Preserve `.meta` files and Unity asset GUIDs.
- Use prefabs for shared gameplay objects and avoid duplicating logic across traps.
- Do not commit `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, or `Logs/`.
- Coordinate edits to shared scenes, prefabs, and project settings.

## Multiplayer rules

- Make the server authoritative for shared gameplay outcomes, such as race starts, finish rankings, checkpoint validation, and respawns.
- Use RPCs and network variables only where appropriate; avoid broadcasting effects that are purely local unless all clients need to see them.
- Validate ownership and permissions before accepting client requests.
- Test interactions with more than one player, including simultaneous trigger entries.
- Ensure late or disconnected players cannot corrupt race state.
- Do not rely on the host-only experience as proof that synchronization works.

## Shared level JSON contract

The PC application consumes levels exported by the VR Editor. Keep the data contract consistent between the two repositories.

- Coordinate changes to element IDs, transforms, platform parameters, and serialized data models with the VR team.
- Validate imported data and handle missing or malformed fields gracefully.
- Maintain a representative test JSON level covering supported objects.
- Test any schema change end to end: export from VR, import into PC, and play the resulting level.
- Document any breaking format change before merging it.

## Gameplay and UI testing

Check relevant gameplay behavior before merging:

- [ ] The project opens without new Console errors.
- [ ] Host and client can join the same lobby/session.
- [ ] Player positions and movements synchronize correctly.
- [ ] The race countdown and movement lock are consistent across clients.
- [ ] Start and finish logic produces correct results and rankings.
- [ ] Falling returns players to their last valid checkpoint or the start.
- [ ] Traps and moving platforms behave consistently in multiplayer.
- [ ] Visual feedback does not accidentally retrigger gameplay logic.
- [ ] Imported JSON levels spawn the expected objects.
- [ ] Relevant behavior is tested with a host and at least one client.
- [ ] Scenes, prefabs, and required `.meta` files are committed.

## Pull request template

Each pull request should include:

- **Summary:** What changed and why?
- **Related feature:** Specification ID or Trello card.
- **Testing:** Host/client setup and results.
- **Networking impact:** Any new RPCs, network variables, ownership rules, or synchronization changes.
- **JSON compatibility:** Whether level loading or the shared format changes.
- **Evidence:** Screenshots, logs, or a short video when useful.

## Team collaboration

Use the Trello board to track tasks, ownership, blockers, and review status. Keep commits and pull requests traceable so the team can explain the project's architecture and Git contributions during the final evaluation.
