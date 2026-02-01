# Instructions for Claude Code: Space Fighter Prototype

**Goal:** Create a minimal viable prototype of a single-player space fighter in Unity with a functional HUD and basic flight controls in a starfield environment.

## Phase 1: Project Setup
1. Create a new Unity 3D project (Unity 2021.3 LTS or newer recommended)
2. Set up the basic folder structure:
   - Assets/Scripts
   - Assets/Prefabs
   - Assets/Materials
   - Assets/Scenes
3. Create a main scene called "PrototypeScene"

## Phase 2: Starfield Environment
1. Create a simple starfield background using a skybox or particle system
2. Add ambient lighting to make the space environment visible
3. Consider using a procedural starfield shader or particle system for performance

## Phase 3: Player Ship
1. Create a basic 3D ship model (can start with primitive shapes - capsule/cube combination)
2. Add a Rigidbody component for physics-based movement
3. Implement 6-degrees-of-freedom flight controls:
   - WASD for pitch/yaw
   - Q/E for roll
   - Shift/Ctrl for throttle up/down
   - Spacebar for boost
4. Add a simple camera system following the ship

## Phase 4: Core Flight System
1. Create a ShipController script that handles:
   - Thruster power management
   - Rotation (pitch, yaw, roll)
   - Forward/reverse thrust
   - Inertial dampening toggle
   - Speed limiting
2. Display basic physics data (velocity, orientation)

## Phase 5: Basic HUD (2D UI)
1. Create a Canvas with UI elements showing:
   - Current speed/velocity
   - Throttle percentage
   - Shield status (can be placeholder for now)
   - Power distribution (placeholder bars)
   - Crosshair/targeting reticle
   - Orientation indicator
2. Use TextMeshPro for clean text rendering
3. Style the HUD with a sci-fi aesthetic (green/blue terminal look)

## Phase 6: External Camera System
1. Implement at least 2 camera views:
   - First-person/cockpit view
   - Third-person chase camera
2. Add ability to switch between views (Tab key or similar)

## Technical Requirements:
- Use Unity's new Input System (or legacy if simpler for prototype)
- Keep code modular with separate scripts for: ship movement, HUD display, camera control
- Add comments explaining key systems for future expansion
- Ensure the ship handles like a space sim (momentum-based, not arcade)

## Deliverables:
1. A playable scene where you can fly a ship through a starfield
2. Responsive flight controls with visible HUD feedback
3. Camera switching functionality
4. Clean, commented code ready for expansion

## Testing Checklist:
- Ship responds smoothly to all control inputs
- HUD updates in real-time
- Camera switches work without errors
- No console errors during gameplay
- Ship maintains momentum appropriately in space

---

## Additional Notes for Claude Code:
- Start simple - we can iterate and add complexity later
- Prioritize getting something playable quickly over perfect visuals
- Use Unity's built-in assets where possible to speed up development
- Document any design decisions or limitations
- This prototype will be the foundation for multiplayer and multi-station gameplay later
