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

**M10 — UI / HUD: COMPLETE**

M2–M4 systems are present in the inspected project; their historical checklists
below have not been re-certified during this M5 task.

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
M2  Hearth System                         PRESENT
 ↓
M3  Resource Gathering                    PRESENT
 ↓
M4  Day / Night Cycle                     PRESENT
 ↓
M5  First Enemy + NavMesh AI               COMPLETE
 ↓
M6  Wave System                           COMPLETE
 ↓
M7  Building / Wooden Fence               COMPLETE
 ↓
M8  Player Combat                          COMPLETE
 ↓
M9  Win / Lose Conditions                 COMPLETE
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

Status: **COMPLETE — validated in Play Mode on 2026-09-12**

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

- [x] NavMesh exists
- [x] Enemy reaches Hearth
- [x] Enemy attacks Hearth
- [x] Hearth health decreases
- [x] Enemy can die
- [x] Console clean

Validation A–G (2026-09-12): the saved NavMesh contains 84 vertices and a
complete spawn-to-Hearth path. The Crawler navigated naturally from (-8, 1, 4),
with no initial test teleport or damage outside its 1.8-unit attack range.
It stopped and faced the Hearth, dealing 10 damage per attack (100 → 90 → 80).
Measured stationary attack intervals were 1.005 and 1.004 seconds.
An explicit test reposition proved Attack → Chase → Attack and renewed navigation.
An overkill attack reduced Hearth health from 5 to 0 without going negative.

EnemyHealth checks passed: initial/max health 30; damage 30 → 18; healing
18 → 23; excess healing clamped to 30; negative/nonfinite amounts ignored;
lethal and repeated damage clamped to zero; healing did not resurrect the enemy.
Dead state disabled the agent and object, with no movement or damage for two
cooldowns. After Play Mode, Hearth Health/Fuel returned to 100/100.

Agent settings: speed 6, acceleration 20, angular speed 360, stopping distance
1.5, radius 0.5, height 2, base offset 1, auto braking/repath enabled.
Attack settings: range 1.8, damage 10, cooldown 1 second.

Repeat in a fresh Play session using **Tools > Validation > Run M5 (in Play Mode)**.
The check temporarily enables background execution and restores it afterward.
The check logs `M5 PASS A-G` on success.
Stop Play Mode afterward to discard test damage.

Scope: one manually placed ShadowCrawler_Test and a reusable ShadowCrawler
prefab. Dead enemies deactivate; respawning/pooling, waves, building, player
combat, win/lose logic, and light-based weakening are not implemented here.

## Exit Criteria

A spawned Shadow Crawler can autonomously:

```text
navigate
→ reach Hearth
→ attack Hearth
```

---

# 11. M6 — Wave System

Status: **COMPLETE — validated in Play Mode on 2026-09-12**

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
3 Crawlers
```

Night 2:

```text
5 Crawlers
```

Night 3:

```text
7 Crawlers
```

Exact values should be easy to rebalance in Inspector.

## Validation

- [x] enemies only spawn during Night
- [x] spawn points work
- [x] correct enemy count
- [x] enemy cleanup works
- [x] Day does not spawn enemies
- [x] Console clean

Implementation: WaveManager uses GameManager.StateChanged and CurrentDay;
EnemySpawner reuses ShadowCrawler.prefab, randomly selects reachable points,
and assigns the Hearth reference. EnemyHealth.Died fires once before deactivation.
Living enemies are tracked by identity, removed once on death, and destroyed.
Day cancels pending spawns, deactivates/destroys survivors, and resets tracking.

Settings: baseEnemyCount 3, enemiesAddedPerNight 2, spawnInterval 1.5 seconds.
Counts use `3 + (CurrentDay - 1) * 2`. Spawn points are North (0, 0.05, 22),
South (0, 0.05, -22), East (22, 0.05, 0), West (-22, 0.05, 0).
All four resolve to the existing NavMesh with complete paths to the Hearth.

Validation A–G passed: initial Day empty; Night 1 spawned 3/3; Night 2 spawned
5/5; measured intervals 1.500–1.505 seconds; valid spawn origins and navigation;
exactly one death notification before deactivation and one count decrement;
Day cleanup with living enemies. A shortened Night 3 spawned 1/7 before Day
cancelled the remaining six; no enemies appeared during the following Day.

Repeat with Tools > Validation > Run M6 (in Play Mode), in a fresh Play session.
The check uses temporary durations and background execution, then restores them.
Exit Play Mode to discard test damage. Final saved durations are 60/60 seconds,
with Hearth Health/Fuel 100/100. ShadowCrawler_Test was removed after validation;
the existing prefab is unchanged. No packages or M7+ systems were added.

Limitations: prototype enemy visuals; no combat input yet (validation uses
TakeDamage); wave completion does not shorten the GameManager's Night timer.

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

Status: **COMPLETE — validated in Play Mode on 2026-09-12**

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

- [x] preview follows mouse
- [x] invalid placement rejected
- [x] Wood cost checked
- [x] Wood deducted
- [x] Fence blocks path
- [x] Enemy can attack Fence
- [x] Fence can be destroyed
- [x] Console clean

Implementation: B toggles build mode; existing Attack input (left click / Enter)
confirms placement. Move and Interact bindings are unchanged. The existing Player
has BuildingSystem with references to Main Camera, Ground, Hearth and WoodenFence.
The prefab has two posts/two rails, a BoxCollider (2.8 x 1.5 x 0.4),
PlaceableBuilding (5 Wood), Fence (50 Health), and a matching box NavMeshObstacle.
Carving is enabled, stationary-only, with 0.1 movement threshold and 0.1s stationary delay.

Placement rounds X/Z to whole units, checks the full footprint on Ground, and uses
a non-allocating overlap box with 0.1-unit clearance. Hearth renderer bounds are
checked explicitly because the existing Hearth has no collider. Preview copies
only the visual child and uses green/red tint; it has no gameplay components.
PlayerInventory is reused with unchanged normal defaults (0 Wood / 0 Stone).

Play Mode validation (`Tools/Validation/Run M7 (in Play Mode)` in a fresh session):

- A: B action, two mouse positions, snapping, and visual-only preview passed.
- B: actual left-click placed one Fence at the preview position; Wood 20 -> 15.
- C: 4 Wood rejected a 5-Wood Fence with no changes.
- D: Hearth, existing Fence, Player, non-Ground and snapped Ground-edge attempts
  were rejected without creating a Fence or spending Wood.
- E: initial 50 Health; 12 damage -> 38; invalid damage ignored; overkill and
  repeated damage clamped to 0, deactivated immediately and removed once.
- Physics: CharacterController was stopped by the Fence collider.
- F: a Fence placed in front of an existing Crawler carved the direct route;
  the alternate path remained complete. Enemy detoured 2.07 units laterally,
  left Fence at 50 Health and attacked Hearth (100 -> 90).
- G: a runtime-only wall across the existing NavMesh produced a partial route.
  EnemySpawner spawned from the blocked side; the enemy approached and attacked
  a Fence for 10 damage at measured 1.000–1.004s intervals, destroyed it, then
  used the restored complete route to attack Hearth. Spawner now accepts partial
  paths so building a wall does not suppress waves.
- H: actual W/E input moved Player and gathered Wood. Existing M6Validation
  passed again after the spawner change: 3/3 then 5/5 enemies, ~1.5s intervals,
  death counting, Day cleanup, and cancellation of pending Night 3 spawns.
  M5Validation also passed without fences: normal Hearth attacks, cooldown,
  attack range, Chase/Attack transitions, damage clamping and death behavior.

Final saved state: Play Mode off; SampleScene saved; no test fences or previews;
Hearth Health/Fuel 100/100; Day/Night durations 60/60; Player Wood/Stone 0/0.
Final Unity Console inspection: 0 errors / 0 warnings. Prototype visual capture:
`Validation/M7/wooden-fence.png`.

Known limits: fixed Fence orientation and unit snapping on the current flat Ground;
keyboard/mouse building only. Fence selection is a local 3-unit search near the
partial-path endpoint, not a maze planner. Carving updates asynchronously; there
is no runtime NavMesh rebake. No repair, upgrades, Player combat or M8+ features.
Runtime test resources, fences and temporary settings are discarded on Play exit.

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

Status: **COMPLETE — validated in Play Mode on 2026-09-12**

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
- Player death stops movement and combat; M9 will trigger Game Over

## Validation

- [x] attack input works
- [x] enemy takes damage
- [x] enemy dies
- [x] cooldown prevents spam
- [x] enemy damages Player
- [x] Player death works
- [x] Console clean

Implementation: Player prefab and scene Player have PlayerHealth (100 max) and
PlayerCombat (15 damage, 2-unit range, 0.6-second cooldown). The existing Attack
action remains unchanged. Left click attacks only outside Build Mode; while Build
Mode is active the same click only confirms Fence placement. A non-allocating
overlap query selects at most one living EnemyHealth, preferring the nearest.

EnemyController keeps its existing 10 damage / 1-second cooldown and now attacks
a living Player already within its 1.8-unit melee range before choosing a blocking
Fence or Hearth. It does not chase Player. Dead Player state does not trigger
GameOver yet; PlayerCombat and PlayerMovement stop until M9 handles defeat.

Play Mode validation (`Tools/Validation/Run M8 (in Play Mode)`):

- PlayerHealth ignored negative/NaN/infinite amounts; damage/heal changed
  100 -> 80 -> 85 and excess healing clamped to 100.
- Left click dealt 15 damage (Crawler 30 -> 15); five spam attempts during the
  cooldown caused no change; the next attack after 0.6s dealt 15 -> 0 and set Dead.
- A target at 2.1 units stayed at 30 Health. With two targets in range, only the
  nearer target changed 30 -> 15; the farther remained at 30.
- In Build Mode, left click placed exactly one Fence and spent 5 Wood without
  damaging a nearby enemy. After exiting Build Mode, left click dealt 15 damage.
- Enemy attacks changed Player 100 -> 90 -> 80 by exact 10-damage hits with the
  existing one-second cooldown. Overkill clamped at 0 and death fired once.
- Dead Player could neither attack nor move; no GameOver/UI/scene reload occurred.
- Existing E input still gathered Wood.

Fresh regression sessions passed after M8: M7 preview/placement/cost/validation,
Player collision, alternate-route carving, Fence attack/destruction and return to
Hearth; M6 Night counts (3 then 5), ~1.5s spawning, death tracking and Day cleanup;
M5 direct Hearth attacks, cooldown, range, Chase/Attack transitions and enemy death.

Final saved state: Play Mode off; SampleScene saved; Player Health 100/100;
Hearth Health/Fuel 100/100; Day/Night 60/60; no test enemies or Fences.
Final Unity Console inspection: 0 errors / 0 warnings.

Known limits: melee has no animation, arc, knockback or aim-facing requirement.
Enemy prioritizes Player only when already within range and does not chase Player.
There is no line-of-sight test between nearby Player and enemy; complex targeting
and full defeat behavior remain M9+ scope.

## Exit Criteria

Player can:

```text
fight enemy
→ take damage
→ kill enemy
```

---

# 14. M9 — Win / Lose Conditions

Status: **COMPLETE — validated in Play Mode on 2026-09-13**

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

- [x] Hearth death causes Game Over
- [x] Player death causes Game Over
- [x] surviving final night causes Victory
- [x] gameplay stops correctly
- [x] Console clean

Implementation: `PlayerHealth.Died` and `HearthController.Destroyed` notify the
existing `GameManager`, which commits one immutable `GameOver` transition. Night
5 completes as `Victory` only while Player and Hearth remain alive; no Day 6 is
entered. Terminal state freezes the timer at zero, deactivates PlayerInput,
disables movement/combat/interaction/building, and disables WaveManager so its
existing cleanup removes active enemies and cancels pending spawns. Time scale is
not changed. Fuel reaching zero remains non-terminal and uses minimum light range.

Play Mode validation passed for Player death during Day and Night, Hearth
destruction, fuel zero, Nights 1–4, Night 5 start/completion, Player and Hearth
failure at the final boundary, one-shot terminal events, immutable GameOver and
Victory, input/system shutdown, unchanged time scale, wave cleanup, and no later
spawning. Fresh M5, M6, M7 and M8 regression validators also passed. Final saved
state: Play Mode off; Player 100/100; Hearth Health/Fuel 100/100; Day/Night 60/60;
five required Nights; no runtime enemies or Fences; Console 0 errors / 0 warnings.

Known limits: M9 has no UI, restart, Main Menu, pause, score, Endless Mode or
save/load behavior. Those remain later milestones.

## Exit Criteria

A complete game can be won or lost.

---

# 15. M10 — UI / HUD

Status: **COMPLETE — validated in Play Mode on 2026-09-13**

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
Assets/_Project/Scripts/UI/EndGameUI.cs
```

## Validation

- [x] values update correctly
- [x] no NullReference errors
- [x] HUD readable
- [x] Game Over UI works
- [x] Victory UI works

Implementation: SampleScene has one Screen Space - Overlay Canvas with a
top-left HUD plus inactive GameOverPanel and VictoryPanel overlays. HUDController
uses serialized PlayerHealth, PlayerInventory, HearthController, GameManager and
WaveManager references. It displays Player/Hearth Health, Fuel, Wood, Stone,
Day/Night and day number, whole-second time remaining, and living enemy count.
EndGameUI listens to GameManager.StateChanged and shows exactly one terminal
panel. Existing UGUI Text is used because TextMeshPro font essentials are not
configured in Assets; no package or UI asset import was needed.

Play Mode validation passed for starting HUD values; Wood and Stone gathering;
Player and Hearth damage; Fuel consumption; timer, phase and day changes; live
wave count and cleanup; Player-death Game Over; and controlled Night 5 Victory.
Fresh M8 and M7 control/combat/interaction/building regressions passed. M6 waves
passed with runtime-only elevated Player/Hearth health to isolate the older
wave validator from M9 terminal defeat, and M5 passed with its expected
runtime-only crawler. M9 night defeat/cleanup, Hearth defeat, fuel-zero,
Victory and both final-boundary priority validators passed.

Final saved state: Play Mode off; Player 100/100; Hearth Health/Fuel 100/100;
Day/Night 60/60; five required Nights; no runtime enemies or Fences; Console
0 errors / 0 warnings.

Known limits: no restart/menu buttons, pause UI, animations, advanced styling,
score, Endless Mode or save/load. Those remain outside M10.

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
- M5 — First Enemy + NavMesh AI (validated 2026-09-12; M2–M4 systems were already present)
- M6 — Wave System (validated 2026-09-12)
- M7 — Building System / Wooden Fence (validated 2026-09-12)
- M8 — Player Combat (validated 2026-09-12)
- M9 — Win / Lose Conditions (validated 2026-09-13)
- M10 — UI / HUD (validated 2026-09-13)

## Current

- M10 — COMPLETE

## Next

- M11 — Polish (not started; requires a separate request)

## Not Started

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

**M10 is complete.** See section 15 for Play Mode validation and limitations.
M11 is next, but requires a separate implementation request.

---

# End of Development Plan
