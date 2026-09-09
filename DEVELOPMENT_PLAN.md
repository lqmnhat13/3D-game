# DEVELOPMENT_PLAN.md

# Wardens of the Hearth — Development Plan

## 1. Project Overview

**Project Name:** Wardens of the Hearth  
**Engine:** Unity 6000.0.72f1  
**Render Pipeline:** URP  
**Input:** Unity Input System  
**Genre:** 3D Isometric Survival / Base Defense  
**Target:** University coursework / playable vertical slice  
**Recommended Scope:** 3–4 weeks

The game is built around a simple but strong gameplay loop:

```text
DAY
Gather resources
→ prepare defenses
→ refuel the Hearth

NIGHT
Enemies spawn
→ defend the Hearth
→ keep the fire alive
→ survive the wave

Repeat
```

The Hearth is the central gameplay objective. The player must protect it while managing fuel, resources, enemies, and defenses.

---

# 2. Scope Principles

The project should prioritize a **playable, stable vertical slice** over feature count.

## Core Rules

1. Implement one milestone at a time.
2. Do not implement future milestones early unless required by a dependency.
3. Prefer simple, maintainable systems over complex architecture.
4. Avoid unnecessary third-party packages.
5. Reuse existing Unity systems wherever possible.
6. Every milestone must compile and run before moving forward.
7. Unity Console should return to **0 new errors** after each milestone.
8. Large architectural refactors should only happen when a concrete problem justifies them.

## Avoid

- Multiplayer
- Procedural generation
- Behavior Trees
- GOAP AI
- Advanced inventory grids
- Drag-and-drop inventory UI
- Crafting trees
- Complex skill systems
- Cinemachine unless required later
- Advanced save/load system during MVP
- Excessive ScriptableObject abstraction
- Over-engineered dependency injection
- Large third-party frameworks

---

# 3. Current Project Status

## Environment

- Unity: **6000.0.72f1**
- URP: installed
- Input System: installed
- Unity MCP: connected
- Console: 0 errors / 0 warnings at last inspection
- Active Scene: `Assets/Scenes/SampleScene.unity`

## Current Scene

```text
SampleScene
├── Player
├── Ground
├── Main Camera
├── Directional Light
└── Global Volume
```

## Current Milestone

**M2 — Hearth System**

## Completed

### M0 — Project Setup
Status: **COMPLETE**

Confirmed:

- Unity project launches correctly
- URP installed
- Unity Input System installed
- Existing Move action supports WASD
- Unity MCP successfully connected
- Scene inspected successfully
- Console clean
- No gameplay systems currently exist

### M1 — Player Movement + Isometric Camera
Status: **COMPLETE**

Confirmed:

- WASD movement uses the existing Input System action
- Diagonal movement is normalized
- CharacterController gravity works
- Main Camera follows Player at a fixed isometric angle
- Player prefab created
- Scene saved
- Console clean

---

# 4. Milestone Roadmap

```text
M0  Project Setup                         COMPLETE
 ↓
M1  Player Movement + Isometric Camera    COMPLETE
 ↓
M2  Hearth System                         CURRENT
 ↓
M3  Resource Gathering
 ↓
M4  Day / Night Cycle
 ↓
M5  First Enemy + NavMesh AI
 ↓
M6  Wave System
 ↓
M7  Building / Wooden Fence
 ↓
M8  Player Combat
 ↓
M9  Win / Lose Conditions
 ↓
M10 UI / HUD
 ↓
M11 Polish / Audio / Lighting / Extra Content
```

---

# 5. M0 — Project Setup

## Status

**COMPLETE**

## Objective

Prepare the Unity project so development can proceed safely.

## Required Systems

- Unity project
- Git repository
- `.gitignore`
- Visible Meta Files
- Force Text serialization
- Unity Input System
- URP
- Unity MCP
- Codex workflow
- `AGENTS.md`
- `GAME_DESIGN.md`
- `DEVELOPMENT_PLAN.md`

## Exit Criteria

- [x] Project launches
- [x] Scene opens
- [x] Unity MCP connects
- [x] Codex can read live Unity state
- [x] Input System available
- [x] URP available
- [x] Console clean

---

# 6. M1 — Player Movement + Isometric Camera

## Status

**COMPLETE**

## Goal

Create a basic controllable Player and fixed isometric camera.

## Expected Project Folders

```text
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Player/
    │   └── Camera/
    └── Prefabs/
        └── Player/
```

## Expected Files

```text
Assets/_Project/Scripts/Player/PlayerMovement.cs
Assets/_Project/Scripts/Camera/IsometricCameraFollow.cs
Assets/_Project/Prefabs/Player/Player.prefab
```

## Expected Scene

```text
SampleScene
├── Player
├── Ground
├── Main Camera
├── Directional Light
└── Global Volume
```

## Player Requirements

Prototype visual:

- Capsule

Components:

- CharacterController
- PlayerInput
- PlayerMovement

Movement:

- WASD
- Existing Move `Vector2` Input System action
- CharacterController based
- Normalized diagonal movement
- Movement speed configurable using `[SerializeField]`
- Basic gravity
- No jumping yet
- No Rigidbody movement
- No combat
- No inventory
- No interaction system yet

## Camera Requirements

Reuse the existing `Main Camera`.

The camera should:

- use a fixed isometric perspective
- rotate roughly 45 degrees around Y
- look downward roughly 45–60 degrees
- follow Player
- use a configurable offset
- follow smoothly
- not accept rotation input
- not use Cinemachine yet

## Ground Requirements

- Large enough for movement testing
- Collider enabled
- Simple prototype appearance
- No decorative assets required

## Validation

- [x] Project compiles
- [x] Console has 0 new errors
- [x] Player exists
- [x] Ground exists
- [x] Main Camera reused
- [x] WASD movement works
- [x] Diagonal speed is normalized
- [x] Basic gravity works
- [x] Camera follows Player
- [x] Camera stays isometric
- [x] Player prefab created after movement works
- [x] Scene saved

## Exit Criteria

M1 is complete only when:

```text
Player can move reliably
+
Camera follows correctly
+
Scene is saved
+
Player prefab exists
+
Console has no new errors
```

---

# 7. M2 — Hearth System

## Goal

Implement the core mechanic of the game.

The Hearth must have two independent resources:

```text
Hearth
├── Health
└── Fuel
```

## Fuel

Fuel:

- starts at configurable maximum
- decreases during Night
- does not need to decrease during Day for MVP
- controls Hearth light radius
- can later be restored using Wood

Suggested values:

```text
Max Fuel = 100
Min Light Range = 4
Max Light Range = 15
```

Concept:

```text
CurrentLightRadius =
Lerp(MinLightRange, MaxLightRange, Fuel / MaxFuel)
```

## Health

Health:

- only decreases when enemies attack Hearth
- should not automatically decrease with fuel
- reaching 0 causes Game Over

## Hearth Components

Suggested:

```text
Hearth
├── HearthController
├── Point Light
└── Visual fire object
```

## Expected Files

```text
Assets/_Project/Scripts/Hearth/HearthController.cs
```

Potential later interfaces:

```text
IDamageable
```

but do not create unnecessary abstractions unless M5/M8 require them.

## Safe Zone API

Prefer a calculation such as:

```text
IsInsideLight(Vector3 position)
```

instead of continuously resizing physics colliders.

## Validation

- [ ] Hearth exists
- [ ] Fuel is configurable
- [ ] Health is configurable
- [ ] Light range reacts to Fuel
- [ ] No unwanted fuel drain during Day
- [ ] Console clean
- [ ] Hearth state visible in Inspector

## Exit Criteria

Hearth can independently manage:

```text
Health
Fuel
Light Radius
```

---

# 8. M3 — Resource Gathering

## Goal

Allow Player to gather Wood and Stone.

## MVP Resources

```text
Wood
Stone
```

## Player Inventory

Keep inventory simple.

Suggested:

```text
PlayerInventory
├── Wood
└── Stone
```

Avoid:

- inventory slots
- item rarity
- drag/drop
- item databases
- stack objects

unless later gameplay requires them.

## Resource Nodes

MVP:

```text
Tree
Rock
```

Basic interaction:

```text
Player approaches resource
→ interacts
→ resource gives quantity
→ resource disappears / depletes
```

## Expected Files

```text
Assets/_Project/Scripts/Player/PlayerInventory.cs
Assets/_Project/Scripts/Player/PlayerInteractor.cs

Assets/_Project/Scripts/Resources/ResourceNode.cs
Assets/_Project/Scripts/Resources/ResourceType.cs
```

## Validation

- [ ] Player can gather Wood
- [ ] Player can gather Stone
- [ ] Inventory values increase correctly
- [ ] Resource nodes deplete
- [ ] No duplicate resource rewards
- [ ] Console clean

## Exit Criteria

Player can:

```text
Move
→ interact with Tree/Rock
→ receive Wood/Stone
```

---

# 9. M4 — Day / Night Cycle

## Goal

Introduce the main game rhythm.

## MVP States

Use a simple enum:

```text
Day
Night
GameOver
Victory
```

Do not build a large State Pattern unless needed later.

## Suggested Durations

Prototype:

```text
Day:   60–90 seconds
Night: 60–90 seconds
```

For testing, expose shorter Inspector values.

## Day

- enemies disabled / not spawned
- brighter environment
- player gathers resources
- player prepares defenses

## Night

- environment becomes dark
- Hearth fuel decreases
- enemies may spawn when M5/M6 is available

## Expected Files

```text
Assets/_Project/Scripts/Core/GameManager.cs
Assets/_Project/Scripts/Core/GameState.cs
```

## Validation

- [ ] Day timer works
- [ ] Night timer works
- [ ] States alternate
- [ ] Directional Light changes appropriately
- [ ] Hearth receives Night state
- [ ] Console clean

## Exit Criteria

```text
Day
→ Night
→ Day
```

cycles reliably.

---

# 10. M5 — First Enemy + NavMesh AI

## Goal

Create the first complete enemy.

## Enemy

Implement only:

**Shadow Crawler**

Characteristics:

- low health
- fast movement
- targets Hearth
- simple melee attack

## AI Technology

Use:

```text
NavMeshAgent
+
simple FSM
```

States:

```text
Chase
Attack
Dead
```

Do not use:

- Behavior Tree
- GOAP
- machine learning
- expensive perception systems

## Basic Logic

```text
Spawn
 ↓
Target Hearth
 ↓
Navigate
 ↓
In attack distance?
 ├── No → Chase
 └── Yes → Attack
```

## Expected Files

```text
Assets/_Project/Scripts/AI/EnemyController.cs
Assets/_Project/Scripts/AI/EnemyHealth.cs
Assets/_Project/Scripts/AI/EnemyState.cs
```

Optional later:

```text
EnemyData ScriptableObject
```

Only introduce this if balancing requires it.

## Validation

- [ ] NavMesh exists
- [ ] Enemy reaches Hearth
- [ ] Enemy attacks Hearth
- [ ] Hearth health decreases
- [ ] Enemy can die
- [ ] Console clean

## Exit Criteria

A spawned Shadow Crawler can autonomously:

```text
navigate
→ reach Hearth
→ attack Hearth
```

---

# 11. M6 — Wave System

## Goal

Spawn enemies during Night.

## WaveManager Responsibilities

- detect Night start
- spawn a configured number of enemies
- use map-edge spawn points
- track active enemies
- stop spawning during Day
- detect end of wave

## Suggested Structure

```text
WaveManager
├── CurrentWave
├── SpawnPoints[]
├── EnemiesRemaining
└── SpawnInterval
```

## Expected Files

```text
Assets/_Project/Scripts/AI/WaveManager.cs
Assets/_Project/Scripts/AI/EnemySpawner.cs
```

## MVP Behavior

Night 1:

```text
5 Crawlers
```

Night 2:

```text
7 Crawlers
```

Night 3:

```text
10 Crawlers
```

Exact values should be easy to rebalance in Inspector.

## Validation

- [ ] enemies only spawn during Night
- [ ] spawn points work
- [ ] correct enemy count
- [ ] enemy cleanup works
- [ ] Day does not spawn enemies
- [ ] Console clean

## Exit Criteria

A complete Night can:

```text
start
→ spawn wave
→ enemies attack Hearth
→ wave ends
```

---

# 12. M7 — Building System / Wooden Fence

## Goal

Allow Player to build defensive structures.

## MVP Building

Only implement:

**Wooden Fence**

Add Spike Trap later if time remains.

## Placement Flow

```text
Select Fence
 ↓
Raycast mouse to Ground
 ↓
Show preview
 ↓
Valid?
 ├── Yes → place
 └── No → reject
```

Use lightweight snapping.

Example:

```text
x = Round(x)
z = Round(z)
```

No advanced grid framework required.

## Fence

Fence should:

- consume Wood
- block enemies
- have Health
- be damageable
- be destroyed at 0 Health

## Expected Files

```text
Assets/_Project/Scripts/Building/BuildingSystem.cs
Assets/_Project/Scripts/Building/PlaceableBuilding.cs
Assets/_Project/Scripts/Building/Fence.cs
```

## Validation

- [ ] preview follows mouse
- [ ] invalid placement rejected
- [ ] Wood cost checked
- [ ] Wood deducted
- [ ] Fence blocks path
- [ ] Enemy can attack Fence
- [ ] Fence can be destroyed
- [ ] Console clean

## Exit Criteria

Player can:

```text
gather Wood
→ place Fence
→ Fence blocks enemy
→ Enemy destroys Fence
```

---

# 13. M8 — Player Combat

## Goal

Allow Player to kill enemies.

## MVP Combat

Implement melee first.

Flow:

```text
Attack input
 ↓
Cooldown
 ↓
Hit detection
 ↓
Enemy receives damage
```

## Expected Files

```text
Assets/_Project/Scripts/Player/PlayerCombat.cs
Assets/_Project/Scripts/Player/PlayerHealth.cs
```

Potential shared system:

```text
IDamageable
Damageable
```

Only introduce if it reduces duplicated combat logic.

## Requirements

- configurable damage
- configurable cooldown
- configurable range
- Player health
- enemy attacks can damage Player
- Player death triggers Game Over

## Validation

- [ ] attack input works
- [ ] enemy takes damage
- [ ] enemy dies
- [ ] cooldown prevents spam
- [ ] enemy damages Player
- [ ] Player death works
- [ ] Console clean

## Exit Criteria

Player can:

```text
fight enemy
→ take damage
→ kill enemy
```

---

# 14. M9 — Win / Lose Conditions

## Goal

Complete the game loop.

## Lose

Game Over when:

```text
Player Health <= 0
OR
Hearth Health <= 0
```

Optional later:

```text
Hearth Fuel == 0
```

should not automatically mean instant defeat unless this is confirmed by playtesting.

A better MVP behavior is:

```text
Fuel = 0
→ light radius at minimum
→ dangerous state

Hearth Health = 0
→ Game Over
```

## Win

Recommended:

```text
Survive 5 Nights
```

## Expected Systems

`GameManager` gains:

```text
Victory()
GameOver()
```

## Validation

- [ ] Hearth death causes Game Over
- [ ] Player death causes Game Over
- [ ] surviving final night causes Victory
- [ ] gameplay stops correctly
- [ ] Console clean

## Exit Criteria

A complete game can be won or lost.

---

# 15. M10 — UI / HUD

## Goal

Make important game information visible.

## MVP HUD

Display:

```text
Player Health
Hearth Health
Hearth Fuel
Wood
Stone
Day / Night
Current Night / Wave
Timer
```

## Screens

Required:

```text
Gameplay HUD
Game Over
Victory
```

Optional:

```text
Main Menu
Pause Menu
```

only if time remains.

## Expected Files

```text
Assets/_Project/Scripts/UI/HUDController.cs
Assets/_Project/Scripts/UI/GameOverUI.cs
Assets/_Project/Scripts/UI/VictoryUI.cs
```

## Validation

- [ ] values update correctly
- [ ] no NullReference errors
- [ ] HUD readable
- [ ] Game Over UI works
- [ ] Victory UI works

---

# 16. M11 — Polish

## Goal

Improve presentation without destabilizing core gameplay.

## Priority Order

### 1. Lighting

- smooth Day/Night transitions
- Hearth flicker
- darker Night
- useful safe-zone visibility

### 2. Audio

- footsteps
- attack sounds
- enemy sound
- wood gathering
- fire crackle
- Night ambience

### 3. VFX

- hit effects
- fire particles
- enemy death
- resource gather particles

### 4. Environment

- trees
- rocks
- simple map boundaries
- ground materials

### 5. Additional Gameplay

Only if time remains:

- Wood Brute
- Spike Trap
- Torch
- Dark Spitter

## Feature Priority

```text
Wood Brute > Spike Trap > Torch > Dark Spitter
```

Dark Spitter should be cut first if time is limited.

---

# 17. Core Architecture

Recommended high-level flow:

```text
                       GameManager
                            │
                    Day / Night State
                            │
               ┌────────────┴─────────────┐
               │                          │
             Hearth                  WaveManager
               │                          │
        Health / Fuel                  Spawner
        Light Radius                     │
               │                       Enemy
        Safe Zone Logic                   │
               │                    NavMeshAgent
         Player / Enemy
```

Player:

```text
Player
├── PlayerMovement
├── PlayerHealth
├── PlayerCombat
├── PlayerInteractor
└── PlayerInventory
```

Avoid a giant `PlayerController.cs`.

---

# 18. Suggested Final Project Structure

```text
Assets/
│
├── _Project/
│   │
│   ├── Art/
│   ├── Audio/
│   ├── Materials/
│   │
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Buildings/
│   │   ├── Resources/
│   │   └── Environment/
│   │
│   ├── Scenes/
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Player/
│   │   ├── Camera/
│   │   ├── Hearth/
│   │   ├── AI/
│   │   ├── Combat/
│   │   ├── Resources/
│   │   ├── Building/
│   │   ├── UI/
│   │   └── Utilities/
│   │
│   └── ScriptableObjects/
│
├── Scenes/
│   └── SampleScene.unity
│
└── Settings/
```

Scene organization can be cleaned up later once the vertical slice works.

---

# 19. Validation Workflow for Every Milestone

Codex should follow this flow:

```text
Inspect
 ↓
Plan
 ↓
Implement smallest change
 ↓
Unity recompiles
 ↓
Inspect Console
 ↓
Fix errors
 ↓
Play Mode test
 ↓
Exit Play Mode
 ↓
Save Scene
 ↓
Report changes
```

## Mandatory Checklist

After every implementation:

- [ ] Unity compilation completed
- [ ] Console inspected
- [ ] No new errors remain
- [ ] Required Scene objects exist
- [ ] Required components exist
- [ ] Play Mode tested where applicable
- [ ] Scene saved
- [ ] Changed files reported
- [ ] Changed GameObjects reported
- [ ] Remaining limitations reported

---

# 20. Git Workflow

Before a milestone:

```bash
git status
git add .
git commit -m "Before Mx <milestone name>"
```

After implementation:

```bash
git diff
```

If accepted:

```bash
git add .
git commit -m "Complete Mx <milestone name>"
```

Recommended history:

```text
Initial Unity project
↓
M0 Project setup
↓
M1 Player movement
↓
M2 Hearth
↓
M3 Resources
↓
M4 Day Night
↓
M5 Enemy AI
↓
M6 Waves
↓
M7 Building
↓
M8 Combat
↓
M9 Win Lose
↓
M10 UI
↓
M11 Polish
```

---

# 21. Risk Management

## Highest Risk Systems

### NavMesh + Dynamic Barriers

Potential issue:

Enemy path may not naturally understand newly placed fences.

Mitigation options:

1. simple collision + attack barrier
2. NavMeshObstacle with carving
3. limited predefined build area

Prefer the smallest stable solution.

### Building System

Placement can become unnecessarily complex.

Mitigation:

Use:

```text
Raycast
+
simple snapping
+
placement validation
```

Do not implement a full strategy-game grid.

### Dark Spitter

High complexity because it needs:

- ranged positioning
- projectile
- line-of-sight
- aim
- ranged attack behavior

Therefore it is optional.

---

# 22. Cut List

If schedule becomes tight, cut features in this order:

```text
1. Dark Spitter
2. Torch
3. Spike Trap
4. Wood Brute
5. Advanced Day/Night transitions
6. Main Menu
7. Pause Menu
8. Extra environment decoration
```

Do **not** cut:

```text
Player Movement
Hearth
Resource Gathering
Day/Night
One Enemy
Wave System
Fence
Combat
Win/Lose
Basic HUD
```

These form the core vertical slice.

---

# 23. Definition of MVP Complete

The project reaches MVP when the following complete loop works:

```text
Player starts game
 ↓
Moves using WASD
 ↓
Collects Wood
 ↓
Refuels Hearth
 ↓
Places Fence
 ↓
Night begins
 ↓
Enemies spawn
 ↓
Enemies path toward Hearth
 ↓
Fence blocks enemies
 ↓
Enemies attack defenses
 ↓
Player fights enemies
 ↓
Hearth fuel decreases
 ↓
Player keeps Hearth alive
 ↓
Wave ends
 ↓
Next Day starts
 ↓
After final Night
 ↓
Victory
```

If this loop works reliably, the project is considered a successful coursework vertical slice.

---

# 24. Milestone Status Template

Whenever a milestone is completed, update this section.

## Completed

- M0 — Project Setup
- M1 — Player Movement + Isometric Camera

## Current

- M2 — Hearth System

## Next

- M3 — Resource Gathering

## Not Started

- M4 — Day / Night Cycle
- M5 — First Enemy + NavMesh AI
- M6 — Wave System
- M7 — Building System / Fence
- M8 — Player Combat
- M9 — Win / Lose
- M10 — UI / HUD
- M11 — Polish

---

# 25. Update Rules

When updating this file:

1. Never mark a milestone complete because code merely exists.
2. Mark complete only after Play Mode validation where relevant.
3. Record any known limitation before moving on.
4. Update Current / Next milestone.
5. Avoid rewriting future milestone requirements unless the game design changes.
6. If scope changes, document why.

Example:

```text
M1 COMPLETE

Validated:
- WASD works
- diagonal movement normalized
- camera follows correctly
- Console 0 errors

Known limitation:
- camera smoothing needs polish

Current:
M2 Hearth System
```

---

# 26. Current Immediate Task

Implement:

**M2 — Hearth System**

Do not begin M3 until M2 passes all exit criteria.

Expected immediate result:

```text
Hearth
├── Health
├── Fuel
└── Light Radius
```

M2 has not been implemented yet.

---

# End of Development Plan
