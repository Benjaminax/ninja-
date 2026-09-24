# The Paradox

The Paradox is a 2D pixel-art platform adventure built with Unity 6. It combines responsive platforming, action combat, enemy AI, and a data-driven UI in a modular project intended to demonstrate modern Unity development practices.

## Game overview

Explore the forest, navigate hazards, collect items, and defeat enemies while moving through handcrafted platforming encounters. The game emphasizes responsive controls and readable enemy behavior rather than overly complex systems.

### Core features

- Responsive platforming with coyote time, jump buffering, and double-jump support
- Action combat with projectiles, damage reactions, stomps, and enemy defeat states
- Multiple enemy archetypes with distinct patrol, chase, hovering, and attack behaviors
- Pixel-perfect 2D presentation with URP lighting and a Cinemachine camera
- Modular prefabs for the player, enemies, items, hazards, and UI
- RuleTile-based terrain authoring for faster level creation
- Event-driven score and HUD updates using UI Toolkit
- New Unity Input System support for keyboard and controller input

## Technology

- **Engine:** Unity 6.0.6.2f1
- **Rendering:** Universal Render Pipeline with the 2D Renderer
- **Language:** C#
- **UI:** UI Toolkit (UXML and USS)
- **Input:** Unity Input System
- **Repository:** Unity assets and project settings are versioned; generated caches and local build output are ignored

## Getting started

1. Install Unity `6000.6.2f1` through Unity Hub.
2. Clone this repository.
3. Open the repository folder in Unity Hub.
4. Open `Assets/Scenes/Level01_ForestAdventure.unity`.
5. Press **Play** in the Unity Editor.

Unity will regenerate the ignored `Library`, `Temp`, and project-file folders locally. These folders should not be committed.

## Controls

The exact bindings are configured through the Unity Input System asset. The default keyboard layout is:

| Action | Default input |
| --- | --- |
| Move | A / D or Left / Right Arrow |
| Jump | Space |
| Attack | Configured in the Input Actions asset |
| Pause | Escape |

Controller bindings can be configured in the Input Actions asset without changing gameplay scripts.

## Project structure

```text
Assets/
├── Scenes/              Main playable scenes
├── Scripts/Actors/      Player and enemy gameplay
├── Scripts/UI/          HUD and persistent game data
├── Prefabs/             Reusable gameplay objects
├── Animations/          Animator controllers and clips
├── Sprites/             Sprites, tiles, and visual effects
└── Settings/            URP, input, and project settings
Packages/                Unity package dependencies
ProjectSettings/         Unity editor and player configuration
```

## Release notes

### Unreleased

- Initial public repository setup
- Unity 6 project configuration and package manifest
- Forest adventure level with platforming and combat foundations
- Responsive player movement with coyote time, jump buffering, and double jump
- Modular enemy AI for Mushroom, BlueBird, Chicken, FatBird, Turtle, and zombie variants
- Event-driven score and HUD systems
- Pixel-art rendering setup, prefabs, animations, and RuleTile terrain workflow

## Asset attribution

Some environmental tiles and basic enemy assets are adapted from Pixel Adventure 1 and Pixel Adventure 2 assets available through itch.io. Custom project work includes the Mushroom trampoline, Mana Dash jump skill, Energy Blast, and Ultimate Burst.

## Development

This project was developed by Nguyen Huu Sang as a Unity developer portfolio and internship project. Contributions and focused improvements are welcome.
