# Factory Frenzy — PC Multiplayer

## Overview

Factory Frenzy is a Unity proof-of-concept inspired by obstacle-course games such as *Fall Guys*. It consists of two separate applications: a **VR level editor** and a **PC multiplayer racing game**.

This repository contains the **PC Multiplayer** game. Players compete in a third-person obstacle course; the first player to cross the finish line wins. Levels can initially be assembled directly in Unity for testing, with JSON import connecting the game to the VR editor.

> Project status: Development / academic proof of concept. Features below are project requirements and may not all be implemented yet.

## Technology

- Unity **2022.3.8** (version used by the provided starter project)
- C#
- Unity Netcode for GameObjects
- Unity Starter Assets — Third Person Character Controller
- Keyboard, mouse and screen (PC)
- JSON level loading

## Planned Features

### Required Features

| ID | Feature |
| --- | --- |
| PC-PLA-P1 | Networked third-person controller, jumping, camera and player collisions |
| PC-PLA-P2 | Start platform, player lineup and synchronized countdown |
| PC-PLA-P3 | Finish platform, arrival order and results screen |
| PC-PLA-P4 | Standard platforms, fall detection and respawning |
| PC-TRA-T2 | Launcher trap with target-search and firing state machine |
| PC-LOB-L1 | Multiplayer lobby |

### Additional Features

| ID | Feature |
| --- | --- |
| PC-PLA-P5 | Moving platform |
| PC-PLA-P6 | Trampoline platform |
| PC-PLA-P7 | Checkpoints |
| PC-TRA-T1 | Bumper trap |
| PC-TRA-T3 | Fan trap |
| PC-LOB-L2 | Import levels created in VR from JSON |

## Gameplay Rules

- Each connected player controls a third-person character.
- Players start together and cannot move until the synchronized countdown ends.
- Players can jump between platforms and collide with one another.
- Falling returns a player to their latest checkpoint; the start platform is the initial checkpoint.
- The first player to cross the finish line takes first place; later arrivals are ranked in order.
- **Ten seconds after the first finish**, the race ends and results are displayed to everyone.
- Players who do not reach the finish line are shown without a ranking.

## Getting Started

1. Install **Unity Hub** and **Unity 2022.3.8**.
2. Clone this repository.
3. Open the project folder in Unity Hub.
4. Verify that the required Unity Netcode and Third Person Starter Assets dependencies are installed.
5. Open the multiplayer game scene (scene name to be confirmed).
6. Run a host instance and connect client instances using the lobby/network UI when implemented.

> Connection method, ports and final build instructions depend on the project's networking configuration and should be documented when finalized.

## Multiplayer Architecture

The specification requires **Unity Netcode** for multiplayer synchronization. Networked player prefabs, synchronized movement, race countdown, finish order and relevant trap interactions must work consistently across host and clients. Remote Procedure Calls (RPCs) are used where appropriate for race events.

## VR Level Integration

The companion **Factory Frenzy VR Level Editor** exports levels as JSON. The PC application is intended to deserialize these files and instantiate the corresponding platforms, traps and checkpoints.

Replace this text with a link to the actual VR repository once available.

## Development Workflow

- Track features and bugs in the team Trello board.
- Develop changes in dedicated Git branches and review them before merging.
- Keep Unity-generated folders (such as `Library/`, `Temp/`, `Obj/` and `Builds/`) out of version control.
- Commit Unity `.meta` files alongside their corresponding assets.

## Academic Context

Developed as part of the **Factory Frenzy** final Unity project at **CESI**. The project brief specifies a ten-day proof of concept, with separate VR and PC applications exchanging levels via JSON.

## Credits

Project assets and initial scene are provided through the course's `Factory-Frenzy-2.0.0-Platform-Trap.zip` starter package. The third-person controller comes from Unity Starter Assets. Third-party resources remain subject to their respective licenses.
