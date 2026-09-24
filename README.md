# Wayfinder - 2D Pixel Adventure

**Wayfinder** is a 2D Platformer developed to research and implement modern game programming techniques within **Unity 6**. The project focuses on optimizing player experience (**Game Feel**), building a modular **AI System**, and utilizing the advanced **UI Toolkit** workflow.

---

## 1. Technical Overview
* **Engine:** Unity 6 (6000.3.10f1) - Leveraging the latest performance features.
* **Render Pipeline:** Universal Render Pipeline (URP) - Optimized 2D Renderer for lighting and post-processing.
* **Input System:** New Input System (Package) - Event-driven input handling with multi-device support.
* **Language:** C# (Clean Code architecture).
* **UI System:** UI Toolkit (UXML & USS) - Modern Web-based styling (Flexbox) for high-performance interfaces.
* **Source Architecture:**
    * **OOP (Inheritance):** Base `Enemy.cs` class encapsulates shared logic (Health, Damage, Hit/Death states). Specialized classes like `ChickenAI` and `TurtleAI` extend specific behaviors.
    * **Singleton Pattern:** Implemented in `GameData` to manage global score and persistent data across levels.
    * **Observer Pattern:** Utilizing C# Events to decouple game logic from the UI system.

---

## 2. Technical Implementation

### 2.1. Advanced Movement (Game Feel)
* **Coyote Time & Jump Buffer:** Implemented in `PlayerMove.cs` for more forgiving and responsive jumping mechanics.
    * **Logic:** `coyoteTimeCounter` allows for jumps shortly after leaving a ledge; `jumpBufferCounter` caches jump inputs slightly before grounding.
    * **Result:** Significantly improved control fluidness and "professional" gameplay feel.
* **Double Jump Logic:** Integrated `jumpCount` with the **Animator Controller** to trigger unique mid-air states.

### 2.2. AI & Environmental Awareness
The AI system uses a modular architecture combining **Finite State Machines (FSM)** with physical sensing techniques.

#### 2.2.1. Core Sensing Mechanics
* **Asynchronous Simulation:** Randomized initial directions and `stateTimer` offsets in `Start()` ensure enemies don't move in a synchronized, "robotic" pattern.
* **Raycasting Detection:** Real-time environmental scanning using `Physics2D.Raycast` to detect ledge boundaries and walls for intelligent flipping logic.
* **Position & Distance Detection:** Dynamic calculation of `xDistance` and `yDistance` relative to the Player to trigger "Chase" or "Attack" states.
* **Dynamic Hit Detection:** Uses `ContactPoint2D.normal` to determine interaction logic: **Stomp** (Damage to enemy from above) vs. **Hit** (Damage to player from sides).

#### 2.2.2. Enemy Archetype Design
1.  **Mushroom (Hybrid AI):** Combines timer-based Idle/Run states with Raycast cliff-detection to prevent accidental falls.
2.  **BlueBird (Coordinate-Based AI):** Implements **Ping-pong Patrol** logic relative to a `startPosition` within a defined `patrolRange`.
3.  **Chicken (Reactive AI):** Features **Proximity Detection**; triggers high-speed `isChasing` state while maintaining cliff-safety raycasts.
4.  **FatBird (Trigger-Based AI):** Uses **`Mathf.Sin`** for hovering patrol and switches `Rigidbody2D` to **Dynamic** for high-velocity vertical attacks when the player is detected below.
5.  **Turtle (Simple State AI):** Optimized FSM that cycles "Spike/No-Spike" states based on simple timers, maximizing performance for high-density encounters.

### 2.3. UI & Data Management
* **UI Toolkit Workflow:** Complete separation of C# logic and UXML/USS presentation.
* **Event-Driven Score System:**
    * **Observer Pattern:** `static event Action` notifications replace `Update()` polling, saving CPU cycles.
    * **Unity 6 Features:** Uses **Setter Properties** and the **`[CreateProperty]`** attribute for enhanced data observability.

### 2.4. Level Design
* **Intelligent RuleTiles:** Application of **ScriptableObject-based RuleTiles** to automate terrain generation. The system recognizes neighbors to auto-select correct sprites (corners, edges, surfaces), drastically reducing manual design time.

---

## 3. Optimization & Asset Management
* **Prefab Architecture:** Modularized Player, Enemy, and Item prefabs for centralized updates and reusability.
* **Pixel Art Consistency:** Configuration of **Point Filter** and **No Compression** for all sprites. Integrated **Pixel Perfect Camera** to eliminate pixel jitter during movement.
* **Git Flow Pipeline:**
    1.  **Initialize:** .gitignore and folder structure.
    2.  **Env Setup:** URP, Layers, and Tags.
    3.  **Core Gameplay:** Input System & Player Controller.
    4.  **AI System:** Base classes and specific archetypes.
    5.  **UI/Data:** Event-driven binding.
    6.  **Tooling:** RuleTiles implementation.
    7.  **Final Polish:** Performance tuning and documentation.

---

## 4. Art & Animation Pipeline

### 4.1. AI-Assisted Workflow
Combining **Generative AI** with manual **Aseprite** refinement to optimize solo production speed.
* **AI Generation:** Used **Pixellab.ai** (Aseprite Plugin) for rapid frame drafting, reducing concept time by ~70%.
* **Manual Polish:** Hand-cleaned pixels and color palette standardization. All assets exported as Sprite Sheets at **32x32 PPU**.

### 4.2. Asset Attribution
* **Custom Assets:** Mushroom (Trampoline), Mana Dash (JumpSkill), Energy Blast, and Ultimate Burst were custom-designed for this project.
* **Third-party Assets:** Environmental tiles and basic enemies integrated from **Pixel Adventure 1 & 2 (itch.io)** to focus resources on specialized mechanics.

---

## 5. Contact & Portfolio
* **Developer:** Nguyen Huu Sang
* **Role:** Unity Developer Intern
* **Email:** 24huusang6a2@gmail.com
* **Project Status:** Core features and AI systems completed.