# Unity Project Instructions

## Project

This is a Unity game project.

Always inspect the existing project before making changes.

Read:

- ProjectSettings/ProjectVersion.txt
- Packages/manifest.json
- relevant scripts
- relevant scenes and prefabs

before assuming the project's architecture.

---

## Unity project rules

Do not modify Unity generated folders:

- Library/
- Temp/
- Logs/
- Obj/
- UserSettings/

Preserve all .meta files.

Never regenerate or change Unity asset GUIDs unless explicitly required.

Never delete a .meta file without deleting its associated asset.

Avoid editing Unity Scene or Prefab YAML manually unless absolutely necessary.

When Unity MCP tools are available, prefer Editor operations for Scene and Prefab changes.

---

## C# rules

Follow the existing project architecture.

Follow existing namespaces.

Use [SerializeField] instead of public fields when a field only needs Inspector access.

Avoid unnecessary GetComponent calls inside Update.

Avoid FindObjectOfType inside Update.

Cache frequently used component references.

Avoid unnecessary allocations inside Update and FixedUpdate.

Prefer small components with clear responsibilities.

Do not install external packages without explicit permission.

---

## Before modifying code

1. Inspect relevant scripts.
2. Inspect dependencies.
3. Understand existing architecture.
4. Explain the intended change briefly.
5. Make the smallest safe change.

Do not rewrite an existing system unless necessary.

---

## After modifying code

Always:

1. allow Unity to recompile
2. inspect compile errors
3. inspect Unity Console
4. fix errors caused by the change
5. run relevant tests if available

Do not claim that a task is complete when compilation errors remain.

---

## Scene modifications

When Unity MCP is available:

1. inspect the active scene first
2. inspect existing GameObjects
3. inspect their components
4. reuse existing objects when possible

Do not create duplicate Player, GameManager, Camera, EventSystem or similar core objects.

Do not delete existing GameObjects unless explicitly requested.

Save the Scene after successful modifications.

---

## Git

Before a large change:

inspect git status.

After completing the task:

summarize:

- files created
- files modified
- GameObjects modified
- components added
- tests performed
- remaining issues

## Game Design

The primary game design specification is stored in:

GAME_DESIGN.md

Before implementing gameplay features:

1. Read GAME_DESIGN.md.
2. Identify the relevant milestone.
3. Implement only that milestone unless explicitly asked otherwise.
4. Do not implement future features early.
5. Prefer the simplest architecture that satisfies the current game design.