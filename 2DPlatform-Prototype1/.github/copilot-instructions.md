# Copilot Instructions for 2D Platformer Prototype

Welcome to the 2D Platformer Prototype project! This document provides essential guidelines for AI coding agents to be productive in this codebase. It covers the architecture, workflows, conventions, and integration points specific to this project.

## Project Overview

This project is a Unity-based 2D platformer game. The codebase is organized into the following major components:

- **Assets/Scripts/**: Contains all gameplay logic, including player mechanics, environment interactions, and game systems.
- **Assets/Prefabs/**: Stores reusable game objects with pre-configured components.
- **Assets/Scenes/**: Contains Unity scenes that define the game levels.
- **Assets/Audio/**: Stores sound effects and music.
- **Assets/Environment/**: Contains environment-related assets like tiles and backgrounds.

### Key Systems
- **PlayerController**: Manages player movement, input handling, and interactions.
- **Environment Interactions**: Handles collisions, triggers, and environmental effects.
- **Input System**: Uses Unity's Input System package for handling player inputs.

## Developer Workflows

### Building the Project
- Open the Unity Editor and load the `2DPlatform-Prototype1` project.
- Use `File > Build Settings` to configure the build platform (e.g., PC, Mac, or Web).
- Click `Build` to generate the game executable.

### Running the Game
- Open the desired scene from `Assets/Scenes/` in the Unity Editor.
- Press the Play button in the Unity Editor to test the game.

### Testing
- Unit tests are located in the `Assets/Tests/` directory (if present).
- Run tests using Unity Test Runner: `Window > General > Test Runner`.

### Debugging
- Use Unity's built-in Debug.Log statements for runtime debugging.
- Attach a debugger to the Unity Editor for step-through debugging.

## Project-Specific Conventions

### Code Organization
- Scripts are grouped by functionality (e.g., `Mechanics`, `Environment`).
- Use namespaces to avoid class name conflicts.

### Coding Patterns
- Follow Unity's MonoBehaviour lifecycle methods (e.g., `Start`, `Update`).
- Use `SerializeField` for private fields that need to be exposed in the Unity Inspector.
- Avoid hardcoding values; use ScriptableObjects or Unity's settings where possible.

### Asset Management
- Prefabs should be used for all reusable game objects.
- Keep assets organized in their respective folders (e.g., `Audio`, `Prefabs`, `Scenes`).

## Integration Points

### External Dependencies
- **Unity Input System**: Handles player input. Configurations are stored in `Assets/InputSystem_Actions.inputactions`.
- **TextMesh Pro**: Used for rendering text in the game.
- **Universal Render Pipeline (URP)**: Configured for enhanced graphics.

### Cross-Component Communication
- Use Unity Events or C# events for decoupled communication between components.
- Avoid direct references between unrelated systems; use Unity's messaging system or event-driven patterns.

## Examples

### PlayerController
The `PlayerController` script in `Assets/Scripts/Mechanics/` demonstrates how to handle player input and movement:

```csharp
void Update()
{
    Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    rb.velocity = move * speed;
}
```

### Scene Setup
Scenes in `Assets/Scenes/` should include:
- A Player prefab.
- Environment tiles.
- Camera setup with a Cinemachine virtual camera (if used).

---

This document is a starting point. Update it as the project evolves to ensure it remains accurate and helpful.