# Wardens of the Hearth

## Game Summary
3D isometric survival-defense game.

The player gathers resources during the day and defends the Hearth during the night.

## Core Loop

Day:
Gather wood and stone
→ prepare defenses
→ refuel Hearth

Night:
Enemies spawn
→ defend Hearth
→ refuel Hearth
→ survive the wave

Repeat until Day 5.

## Win Condition
Survive 5 nights.

## Lose Conditions
- Player health reaches 0
- Hearth health reaches 0

## Hearth System

The Hearth has two independent values:

### Health
Reduced when enemies attack the Hearth.

### Fuel
Decreases during nighttime.

Fuel controls light radius.

Higher fuel:
- larger safe zone
- enemies inside light become weaker

Lower fuel:
- smaller safe zone
- player becomes more vulnerable

## Player

Player systems:

- PlayerMovement
- PlayerHealth
- PlayerCombat
- PlayerInteractor
- PlayerInventory

Controls:
- WASD movement
- mouse aiming
- melee combat initially

## Resources

MVP resources:

- Wood
- Stone

Wood:
- refuel Hearth
- build wooden fences

Stone:
- future defensive structures

## Building

MVP buildings:

### Wooden Fence
Blocks enemy movement.
Has health.

### Spike Trap
Damages enemies.

Torch is optional for later development.

## Enemy Types

### Shadow Crawler
MVP enemy.

Fast.
Low health.
Targets Hearth.

### Wood Brute
Second priority.

Slow.
High health.
Strong against barriers.

### Dark Spitter
Optional.
Implement only if time remains.

## Game States

Use:

- Day
- Night
- GameOver
- Victory

Avoid over-engineering the state machine.

## AI

Use Unity NavMeshAgent.

Basic enemy states:

- Chase
- Attack
- Dead

Priority:
1. obstacle blocking route
2. Hearth
3. nearby Player where appropriate

## MVP Development Order

M0 - Project setup
M1 - Player movement and camera
M2 - Hearth
M3 - Resource gathering
M4 - Day/night
M5 - First enemy
M6 - Wave system
M7 - Fence building
M8 - Combat
M9 - Win/lose
M10 - UI
M11 - Core loop completion
M12 - Visual polish

M11 completes the survival loop with these rules:

- Interact near a non-full Hearth to spend 1 Wood and restore 20 Fuel.
- Each Day reactivates resource nodes, providing 10 Wood and 3 Stone.
- Enemies inside the Hearth light move at 70% of normal speed.
- A Night ends early only after every scheduled enemy has spawned and no enemies remain alive.
- Clearing Night 5 produces Victory without starting Day 6; Player or Hearth death still takes priority.

## Scope Rules

This is a university project.

Avoid:
- unnecessary complex architecture
- multiplayer
- procedural generation
- advanced inventory grids
- behavior trees
- unnecessary third-party dependencies

Prefer simple, maintainable Unity implementations.
