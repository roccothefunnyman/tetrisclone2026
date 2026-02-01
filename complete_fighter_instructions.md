# Complete 1-Man Fighter Development Instructions

**Goal:** Build a fully functional 1-man space fighter with unified HUD, modular underlying systems that will scale to multi-player ships, and all core game mechanics.

---

## Design Philosophy

This fighter represents the baseline experience:
- **One player controls everything** via a unified HUD
- **All systems are accessible** but simplified/automated where appropriate
- **Underlying architecture is modular** so systems can be split across multiple players in larger ships
- **Combat is skill-based** requiring power management, shield routing, and tactical thinking

---

## Core Systems Overview

### System Hierarchy
```
Ship (Fighter)
├── PowerDistributionSystem (manages energy allocation)
├── NavigationSystem (6DOF flight)
├── WeaponSystem (beam, kinetic, missiles)
├── ShieldSystem (4-quadrant with routing)
├── SensorSystem (tiered detection, lock-on)
└── HullIntegritySystem (damage, passive repair)
```

Each system is **independent but interconnected** through power and resource dependencies.

---

## PHASE 1: Foundation & Flight

### 1.1 Project Structure Setup

Create the following folder structure:
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── Ship.cs
│   │   └── GameManager.cs
│   ├── Systems/
│   │   ├── PowerDistributionSystem.cs
│   │   ├── NavigationSystem.cs
│   │   ├── WeaponSystem.cs
│   │   ├── ShieldSystem.cs
│   │   ├── SensorSystem.cs
│   │   └── HullIntegritySystem.cs
│   ├── UI/
│   │   ├── UnifiedFighterHUD.cs
│   │   ├── RadarDisplay.cs
│   │   ├── TargetInfoPanel.cs
│   │   └── PowerPresetUI.cs
│   ├── Weapons/
│   │   ├── WeaponBase.cs
│   │   ├── BeamWeapon.cs
│   │   ├── KineticWeapon.cs
│   │   └── MissileWeapon.cs
│   ├── AI/
│   │   └── BasicEnemyAI.cs
│   └── Utilities/
│       └── MathHelper.cs
├── Prefabs/
│   ├── Ships/
│   ├── Weapons/
│   └── Effects/
├── Materials/
├── Scenes/
└── Resources/
    └── Configs/
        ├── ShipConfig.asset
        ├── WeaponConfigs/
        └── FactionConfigs/
```

### 1.2 Ship Core Component

**File: `Assets/Scripts/Core/Ship.cs`**

```csharp
// This is the main ship controller that owns all systems
// Key responsibilities:
- Initialize all ship systems
- Provide central reference point for all components
- Handle ship-level events (destruction, spawn, etc.)
- Manage system dependencies

// Properties:
- ShipConfig (ScriptableObject with ship stats)
- References to all systems (Power, Navigation, Weapons, etc.)
- Ship faction
- Ship unique ID

// Public methods:
- Initialize()
- TakeDamage(float amount, Vector3 hitPosition, DamageType type)
- GetSystem<T>() where T : SystemBase
```

### 1.3 Power Distribution System

**File: `Assets/Scripts/Systems/PowerDistributionSystem.cs`**

```csharp
// Central power management system
// All other systems draw power from this

// Power Categories (3 total):
public enum PowerCategory {
    Engines,    // Affects NavigationSystem
    Weapons,    // Affects WeaponSystem
    Shields     // Affects ShieldSystem
}

// Power Presets:
public enum PowerPreset {
    Balanced,   // 33/33/33
    Combat,     // 20/50/30 (Eng/Wpn/Shld)
    Speed,      // 60/20/20
    Defensive   // 20/20/60
}

// Core properties:
- float totalPowerGeneration (base: 100 units for fighter)
- Dictionary<PowerCategory, float> currentAllocation
- PowerPreset activePreset
- bool isManualMode
- float redistributionSpeed (how fast power shifts between categories)

// Key methods:
- SetPreset(PowerPreset preset)
- SetManualAllocation(PowerCategory category, float percentage)
- float GetPowerLevel(PowerCategory category)
- void Update() // Gradually shift power levels toward target allocation

// Important mechanics:
- When preset changes, power shifts gradually over 1-2 seconds
- Larger ships (future) will have slower redistribution (3-5 seconds)
- Total allocation must always equal 100%
- Fire events when power levels change significantly (> 5% shift)
```

### 1.4 Navigation System

**File: `Assets/Scripts/Systems/NavigationSystem.cs`**

```csharp
// Handles all ship movement and flight physics

// Core properties:
- Rigidbody shipRigidbody
- float currentThrottle (0-100%)
- float maxSpeed (base value, modified by power)
- float turnRate (base value, modified by power)
- float acceleration
- bool inertialDampeningEnabled (default: true)
- PowerDistributionSystem powerSystem (reference)

// Key methods:
- void HandleFlightInput()
  // Pitch (W/S), Yaw (A/D), Roll (Q/E)
  // Throttle (Shift/Ctrl)
  // Strafe (Arrow keys or IJKL)
- void ApplyThrust()
- void ApplyRotation()
- void ApplyInertialDampening()
- float GetEffectiveMaxSpeed() // maxSpeed * powerMultiplier
- float GetEffectiveTurnRate() // turnRate * powerMultiplier

// Power dependency:
- Engine power 100% = 1.0x multiplier (full performance)
- Engine power 50% = 0.6x multiplier
- Engine power 25% = 0.4x multiplier
- Engine power 0% = 0.2x multiplier (minimum, can still limp)

// Physics model:
- Use Newtonian physics (momentum-based)
- Apply forces, not direct velocity changes
- Inertial dampening gradually reduces velocity when throttle = 0
- No atmosphere (space), no drag except from dampening
```

### 1.5 Initial HUD

**File: `Assets/Scripts/UI/UnifiedFighterHUD.cs`**

```csharp
// Master HUD controller for 1-man fighter
// Displays all systems on one screen

// UI References (assign in Inspector):
- TextMeshProUGUI speedText
- TextMeshProUGUI throttleText
- Slider hullIntegrityBar
- Image[] shieldQuadrantBars (4 elements: Front/Rear/Port/Starboard)
- TextMeshProUGUI powerPresetText
- Transform crosshair
- RadarDisplay radarDisplay
- TargetInfoPanel targetInfoPanel

// Update methods:
- void UpdateNavigationDisplay()
- void UpdatePowerDisplay()
- void UpdateShieldDisplay()
- void UpdateWeaponsDisplay()
- void UpdateTargetingDisplay()

// Subscribe to system events:
- PowerSystem.OnPowerChanged
- ShieldSystem.OnShieldDamaged
- WeaponSystem.OnWeaponFired
- SensorSystem.OnTargetAcquired
```

---

## PHASE 2: Weapons & Combat

### 2.1 Weapon System Architecture

**File: `Assets/Scripts/Systems/WeaponSystem.cs`**

```csharp
// Manages all weapons on the ship

// Properties:
- List<WeaponBase> installedWeapons
- WeaponBase currentTarget
- PowerDistributionSystem powerSystem
- SensorSystem sensorSystem
- float sharedEnergyPool (for beam weapons)
- float energyRegenRate (power-dependent)

// Key methods:
- void FireWeaponGroup(int groupNumber)
- void SelectTarget(GameObject target)
- void CycleTargets()
- float GetWeaponPowerMultiplier() // Based on power allocation
- void RegenerateEnergy() // Called in Update

// Weapon groups:
- Group 1: Primary weapons (mapped to Mouse1 or Space)
- Group 2: Secondary weapons (mapped to Mouse2)
- Missiles on separate control (mapped to M key with lock requirement)

// Power dependency:
- Weapon power 100% = 1.0x damage, 1.0x fire rate, 1.0x lock speed
- Weapon power 50% = 0.7x damage, 0.8x fire rate, 0.7x lock speed
- Weapon power 25% = 0.5x damage, 0.6x fire rate, 0.5x lock speed
```

### 2.2 Weapon Base Class

**File: `Assets/Scripts/Weapons/WeaponBase.cs`**

```csharp
// Abstract base for all weapon types

public abstract class WeaponBase : MonoBehaviour
{
    // Common properties:
    public string weaponName;
    public int weaponGroup; // 1 or 2
    public float baseCooldown;
    public float currentCooldown;
    public Transform firePoint;
    
    // Abstract methods (implement in derived classes):
    public abstract void Fire(GameObject target, float powerMultiplier);
    public abstract bool CanFire();
    public abstract float GetAmmoPercentage(); // 0-1 for energy, actual count for missiles
    
    // Common methods:
    public void UpdateCooldown(float deltaTime)
    public bool IsReady()
}
```

### 2.3 Beam Weapon

**File: `Assets/Scripts/Weapons/BeamWeapon.cs`**

```csharp
// Instant-hit laser/energy weapon

// Inherits from WeaponBase

// Properties:
- float baseDamage
- float energyCost (draws from WeaponSystem.sharedEnergyPool)
- float range
- LineRenderer beamEffect (visual)
- ParticleSystem hitEffect

// Fire method:
public override void Fire(GameObject target, float powerMultiplier)
{
    // Check if enough energy
    // Raycast from firePoint
    // Apply damage * powerMultiplier
    // Deduct energy cost
    // Show beam visual effect
    // Start cooldown
}

// Characteristics:
- Instant hit (raycast)
- No travel time
- No lock required
- Limited by energy pool
- Good sustained DPS
```

### 2.4 Kinetic Weapon

**File: `Assets/Scripts/Weapons/KineticWeapon.cs`**

```csharp
// Projectile-based weapon (cannons, mass drivers)

// Inherits from WeaponBase

// Properties:
- float baseDamage
- int maxAmmo
- int currentAmmo
- float projectileSpeed
- GameObject projectilePrefab
- float projectileLifetime

// Fire method:
public override void Fire(GameObject target, float powerMultiplier)
{
    // Check if ammo available
    // Instantiate projectile
    // Launch with velocity toward target (with lead calculation)
    // Apply damage * powerMultiplier on hit
    // Deduct ammo
    // Start cooldown
}

// Characteristics:
- Physical projectile (travel time)
- Lead calculation needed (predict where target will be)
- No lock required
- Limited ammo
- High burst damage
```

### 2.5 Missile Weapon

**File: `Assets/Scripts/Weapons/MissileWeapon.cs`**

```csharp
// Lock-on missile launcher

// Inherits from WeaponBase

// Properties:
- float baseDamage
- int maxMissiles
- int currentMissiles
- float lockOnTime (base 2-4 seconds)
- float currentLockProgress (0-1)
- GameObject currentLockTarget
- float missileSpeed
- float trackingStrength
- GameObject missilePrefab

// Additional methods:
- void UpdateLockOn(GameObject target, float sensorMultiplier, float powerMultiplier)
- void BreakLock()

// Fire method:
public override void Fire(GameObject target, float powerMultiplier)
{
    // Check if missile available
    // Check if lock is complete (lockProgress >= 1.0)
    // Instantiate missile with target reference
    // Apply damage * powerMultiplier on hit
    // Deduct missile count
    // Reset lock progress
    // Start cooldown
}

// Lock-on mechanics:
- Must hold target in sights
- Lock speed = base time / (sensorMultiplier * powerMultiplier)
- If target leaves lock cone, progress degrades
- Lock cone = 30 degree cone from ship forward
- Visual/audio feedback as lock progresses

// Characteristics:
- Highest single-shot damage
- Requires lock-on time (sensor and power dependent)
- Homing capability
- Very limited ammo
- Good against larger/slower targets
```

### 2.6 Projectile Script

**File: `Assets/Scripts/Weapons/Projectile.cs`**

```csharp
// For kinetic weapon projectiles

// Properties:
- float damage
- float speed
- Vector3 direction
- float lifetime
- GameObject impactEffect

// Methods:
- void OnCollisionEnter(Collision collision)
  // Apply damage to hit object
  // Spawn impact effect
  // Destroy self
```

### 2.7 Missile Script

**File: `Assets/Scripts/Weapons/Missile.cs`**

```csharp
// For guided missiles

// Properties:
- float damage
- float speed
- GameObject target
- float trackingStrength (how aggressively it turns toward target)
- float lifetime
- float armingTime (0.5s safety period after launch)
- GameObject explosionEffect

// Methods:
- void Update()
  // If armed and target exists, steer toward target
  // If lifetime exceeded, explode
- void OnCollisionEnter(Collision collision)
  // If armed, apply damage and explode
  // Spawn explosion effect
  // Destroy self
```

---

## PHASE 3: Shields & Defense

### 3.1 Shield System

**File: `Assets/Scripts/Systems/ShieldSystem.cs`**

```csharp
// Four-quadrant shield system with routing

// Shield Quadrants:
public enum ShieldQuadrant {
    Front,
    Rear,
    Port,      // Left
    Starboard  // Right
}

// Shield Routing Presets:
public enum ShieldPreset {
    Balanced,  // 25/25/25/25
    Forward,   // 70/10/10/10
    Aft,       // 10/70/10/10
    Manual     // Custom allocation
}

// Properties:
- float totalShieldCapacity (base value for fighter, e.g., 100)
- Dictionary<ShieldQuadrant, float> maxShieldPerQuadrant
- Dictionary<ShieldQuadrant, float> currentShieldPerQuadrant
- ShieldPreset activePreset
- float rechargeRate (power-dependent, base 5% per second at full power)
- PowerDistributionSystem powerSystem
- bool autoBalance (default: false)

// Key methods:
- void SetShieldPreset(ShieldPreset preset)
- void SetManualShieldAllocation(ShieldQuadrant quadrant, float percentage)
- float AbsorbDamage(float damage, Vector3 hitPosition)
- ShieldQuadrant DetermineHitQuadrant(Vector3 hitPosition, Vector3 shipForward)
- void RechargeShields(float deltaTime)
- float GetTotalShieldStrength() // Sum of all quadrants
- float GetQuadrantPercentage(ShieldQuadrant quadrant) // 0-1

// Damage mechanics:
1. Determine which quadrant was hit based on hit position relative to ship
2. Absorb damage from that quadrant first
3. If quadrant shields depleted, overflow damage goes to hull
4. Return remaining damage that penetrated shields

// Recharge mechanics:
- All quadrants recharge simultaneously
- Recharge rate = baseRate * powerMultiplier
- Power multipliers:
  - 100% power = 1.0x (5% per second)
  - 50% power = 0.6x (3% per second)
  - 25% power = 0.3x (1.5% per second)
  - 0% power = 0x (no recharge)
- Shields recharge up to their allocated maximum
- If shields not taking damage for 3+ seconds, recharge at full rate
- If taking damage, recharge at 50% rate

// Shield routing rules:
- Total allocation must equal 100% of capacity
- Minimum 5% per quadrant (can't zero out a facing)
- Switching presets is instant (unlike power redistribution)
- Manual mode allows fine-tuned control
```

### 3.2 Hull Integrity System

**File: `Assets/Scripts/Systems/HullIntegritySystem.cs`**

```csharp
// Tracks hull damage and passive repair

// Properties:
- float maxHull (base value for fighter, e.g., 100)
- float currentHull
- float passiveRepairRate (1-2% per 5 seconds = 0.2-0.4% per second)
- float timeSinceLastDamage
- float repairDelayAfterDamage (5 seconds)
- PowerDistributionSystem powerSystem
- bool repairEnabled (default: true)

// Key methods:
- void TakeDamage(float amount, Vector3 hitLocation)
- void PassiveRepair(float deltaTime)
- float GetHullPercentage() // 0-1
- bool IsDestroyed() // currentHull <= 0
- Vector3 GetDamageLocation() // For visual effects

// Repair mechanics:
- Only repairs when ship hasn't taken damage for 5+ seconds
- Repair rate = baseRate * (generalShipPower / 100)
- General ship power = average of all three power categories
- Repairs up to 100% over time
- Can be toggled off to conserve power in emergencies
- Does NOT repair shields (shields have separate recharge)

// Damage feedback:
- Track where damage occurred for visual effects
- Emit events for HUD to show damage indicators
- Critical damage warnings at <25% hull
```

---

## PHASE 4: Sensors & Targeting

### 4.1 Sensor System

**File: `Assets/Scripts/Systems/SensorSystem.cs`**

```csharp
// Tiered detection and targeting system

// Detection Tiers (range-based):
public enum DetectionTier {
    None,          // Outside sensor range
    LongRange,     // Blip only, no details
    MediumRange,   // IFF and basic info
    ShortRange     // Full details, lock capability
}

// Contact Information:
public class SensorContact {
    public GameObject target;
    public DetectionTier detectionLevel;
    public float range;
    public string iffStatus; // "Friendly", "Enemy", "Unknown"
    public string shipClass; // "Fighter", "Corvette", etc.
    public Vector3 velocity;
    public float hullPercentage; // Only visible at short range
    public float shieldPercentage; // Only visible at short range
}

// Properties:
- float longRangeSensorRange (faction-dependent, e.g., 5000m)
- float mediumRangeSensorRange (2000m)
- float shortRangeSensorRange (500m)
- float lockRange (400m)
- List<SensorContact> detectedContacts
- SensorContact currentTarget
- LayerMask detectionLayers

// Key methods:
- void ScanForContacts()
  // Sphere cast at different ranges
  // Update detection tier for each contact
  // Add new contacts, remove out-of-range
- DetectionTier GetDetectionTier(float range)
- void UpdateContactInformation()
- bool CanLockTarget(GameObject target)
- float GetLockSpeedMultiplier() // Based on detection tier
- SensorContact GetNearestEnemy()
- void CycleTargets()

// Lock mechanics:
- Can only lock missiles at ShortRange tier (< lockRange)
- Lock speed multiplier:
  - ShortRange (close): 1.0x
  - MediumRange: 0.5x (harder to lock)
  - LongRange: 0x (cannot lock)
- Better sensors increase range boundaries (faction difference)

// Faction differences (implemented via ScriptableObject config):
Faction 1 (Might-based):
  - longRangeSensorRange: 4000m
  - mediumRangeSensorRange: 1800m
  - shortRangeSensorRange: 450m
  - lockRange: 350m

Faction 2 (Exploration-based):
  - longRangeSensorRange: 6000m
  - mediumRangeSensorRange: 2500m
  - shortRangeSensorRange: 600m
  - lockRange: 500m

// Events:
- OnContactDetected(SensorContact contact)
- OnContactLost(SensorContact contact)
- OnTargetChanged(SensorContact newTarget)
```

### 4.2 Radar Display

**File: `Assets/Scripts/UI/RadarDisplay.cs`**

```csharp
// 2D tactical radar showing contacts

// Properties:
- RectTransform radarContainer
- GameObject contactBlipPrefab
- Dictionary<GameObject, GameObject> contactBlips (maps targets to UI blips)
- float radarScale (meters per pixel)
- SensorSystem sensorSystem

// Key methods:
- void UpdateRadar()
  // Loop through sensor contacts
  // Create/update UI blips for each
  // Position based on relative position to player
  // Color code by IFF (green=friendly, red=enemy, yellow=unknown)
  // Size based on ship class
  - void PlaceBlip(SensorContact contact)
  - void RemoveBlip(GameObject target)
  
// Visual elements:
- Range rings showing sensor tier boundaries
- Player ship icon (center)
- Directional indicator (shows which way is "forward")
- Contact blips with IFF color coding
```

### 4.3 Target Info Panel

**File: `Assets/Scripts/UI/TargetInfoPanel.cs`**

```csharp
// Detailed information about selected target

// UI Elements:
- TextMeshProUGUI targetName
- TextMeshProUGUI targetClass
- TextMeshProUGUI targetRange
- TextMeshProUGUI targetIFF
- Slider targetHullBar
- Slider targetShieldBar
- TextMeshProUGUI targetVelocity
- GameObject lockIndicator (for missile lock progress)

// Methods:
- void UpdateTargetInfo(SensorContact contact)
  // Display info based on detection tier
  // LongRange: Only name and range
  // MediumRange: Add IFF, class, velocity
  // ShortRange: Add hull/shield status
- void UpdateLockProgress(float progress)
  // Show missile lock progress bar
- void ClearTarget()
```

---

## PHASE 5: UI Integration & Polish

### 5.1 Power Preset UI

**File: `Assets/Scripts/UI/PowerPresetUI.cs`**

```csharp
// UI for selecting power presets and manual override

// UI Elements:
- Button[] presetButtons (Balanced, Combat, Speed, Defensive)
- Button manualOverrideButton
- GameObject manualControlPanel (hidden by default)
- Slider[] categorySliders (Engines, Weapons, Shields)
- TextMeshProUGUI[] categoryPercentageTexts

// Methods:
- void OnPresetButtonClicked(PowerPreset preset)
- void OnManualOverrideClicked()
  // Show manual control panel
  // Populate sliders with current values
- void OnSliderChanged(PowerCategory category, float value)
  // Ensure total = 100% (adjust other sliders proportionally)
  // Send to PowerDistributionSystem
- void UpdateDisplay()
  // Highlight active preset
  // Show current power percentages
  
// Keybinds:
- 1 key: Balanced
- 2 key: Combat
- 3 key: Speed
- 4 key: Defensive
- P key: Toggle manual override panel
```

### 5.2 Shield Routing UI

**File: `Assets/Scripts/UI/ShieldRoutingUI.cs`**

```csharp
// UI for shield quadrant management

// UI Elements:
- Button[] shieldPresetButtons (Balanced, Forward, Aft)
- Button manualShieldButton
- GameObject manualShieldPanel
- Slider[] quadrantSliders (Front, Rear, Port, Starboard)
- Image[] quadrantVisuals (visual shield bubble showing strength per facing)

// Methods:
- void OnShieldPresetClicked(ShieldPreset preset)
- void OnManualShieldClicked()
- void OnQuadrantSliderChanged(ShieldQuadrant quadrant, float value)
  // Ensure total = 100%
  // Minimum 5% per quadrant
- void UpdateShieldVisuals()
  // Update quadrant bars
  // Update shield bubble visualization (brighter = stronger)
  
// Shield bubble visual:
- 2D overlay showing 4 directional segments
- Color intensity = shield strength
- Flash red when that quadrant takes damage
- Pulse when shields critically low

// Keybinds:
- [ key: Cycle shield presets
- ] key: Manual shield control
```

### 5.3 Complete HUD Layout

Update `UnifiedFighterHUD.cs` to integrate all UI components:

```
┌──────────────────────────────────────────────────────────────────┐
│ HULL: ████████░ 89%        FIGHTER-01      PWR: [COMBAT ▼]       │
│ REPAIR: +0.4%/sec                          E:20% W:50% S:30%     │
│                                                                   │
│ SHIELDS:  [F:██▓▓] [R:█░░░]               ┌───RADAR──────┐      │
│           [P:██░░] [S:███░]     ·  ·      │   ·  ·       │      │
│ PRESET: [FWD ▼]                  ○ ○      │  ○    ○      │      │
│                                    ●       │     ●        │      │
│    ╔════════════════════════════╗   ·     │   ·  ·  ·    │      │
│    ║  TARGET: Bandit-03         ║         └──────────────┘      │
│    ║      +→ [LEADING]          ║                                │
│    ║                            ║  ┌─TARGET INFO────────┐       │
│    ║  Range: 340m  Closing      ║  │ Bandit-03 (ENEMY)  │       │
│    ║  Hull: ████░░ 60%          ║  │ Fighter-Class      │       │
│    ║  Shields: ██░░░ 40%        ║  │ Range: 340m        │       │
│    ║                            ║  │ Velocity: 380m/s   │       │
│    ║  [MISSILE LOCK ▓▓▓░░ 65%] ║  │ Closing            │       │
│    ╚════════════════════════════╝  └────────────────────┘       │
│                                                                   │
│ ┌──WEAPONS────────────┐  ┌──FLIGHT──────┐  ┌─ENERGY──────┐     │
│ │ ▓ Beam   [███████]  │  │ SPD: 450 m/s │  │ █████████░  │     │
│ │   READY   0.0s      │  │ THR: 75%     │  │ 92%         │     │
│ │                     │  │ HEADING: 045 │  │ +15/sec     │     │
│ │ ▓ Cannon [24/40]    │  │ DAMP: [ON]   │  └─────────────┘     │
│ │   READY   0.0s      │  └──────────────┘                       │
│ │                     │                                          │
│ │ ▓ Missile[4/8]      │  [1]BAL [2]CMB [3]SPD [4]DEF [P]MANUAL │
│ │   LOCKING... 65%    │  [ ]FWD [ ]AFT [●]BAL [ ]MANUAL SHIELDS │
│ └─────────────────────┘                                          │
│ [FIRE-1:SPACE] [FIRE-2:MOUSE2] [MISSILE:M] [TARGET:T]           │
└──────────────────────────────────────────────────────────────────┘
```

**HUD Sections:**
1. **Top Bar:** Hull, shields, power preset, repair status
2. **Main Viewport:** Targeting reticle, lead indicator, lock progress
3. **Right Panel:** Radar + detailed target info
4. **Bottom Left:** Weapon status and readiness
5. **Bottom Center:** Flight data (speed, throttle, heading)
6. **Bottom Right:** Energy pool for beam weapons
7. **Bottom:** Quick-access keybind reminders

---

## PHASE 6: Combat Balance & Testing

### 6.1 Create Basic Enemy AI

**File: `Assets/Scripts/AI/BasicEnemyAI.cs`**

```csharp
// Simple AI for testing combat

// Behaviors:
1. Patrol behavior (when no target)
   - Fly in lazy circles or figure-8 pattern
   
2. Engage behavior (when player detected)
   - Turn toward player
   - Try to maintain optimal range (300-500m)
   - Fire weapons when in arc
   - Simple evasion (strafe randomly)

3. Flee behavior (when hull < 20%)
   - Turn away from player
   - Max throttle
   - No shooting

// Properties:
- float detectionRange (1000m)
- float optimalEngagementRange (400m)
- float retreatThreshold (20% hull)
- Ship enemyShip (reference to own ship component)
- Transform player (target)

// Keep it simple for prototype - just enough to test weapons/shields
```

### 6.2 Balance Configuration

Create ScriptableObject configs for easy balancing:

**File: `Assets/Resources/Configs/ShipConfig.asset`**

```csharp
[CreateAssetMenu(fileName = "ShipConfig", menuName = "Game/Ship Config")]
public class ShipConfig : ScriptableObject
{
    [Header("Identity")]
    public string shipName;
    public string shipClass; // "Fighter", "Corvette", etc.
    public Faction faction;
    
    [Header("Hull")]
    public float maxHull = 100f;
    public float passiveRepairRate = 0.3f; // % per second
    
    [Header("Shields")]
    public float maxShields = 100f;
    public float baseShieldRechargeRate = 5f; // % per second at full power
    
    [Header("Power")]
    public float totalPowerGeneration = 100f;
    public float powerRedistributionSpeed = 1f; // seconds for full shift
    
    [Header("Navigation")]
    public float maxSpeed = 500f;
    public float turnRate = 45f; // degrees per second
    public float acceleration = 100f;
    
    [Header("Sensors")]
    public float longRangeSensorRange = 5000f;
    public float mediumRangeSensorRange = 2000f;
    public float shortRangeSensorRange = 500f;
    public float lockRange = 400f;
    
    [Header("Weapons")]
    public List<WeaponConfig> installedWeapons;
}
```

**Faction Config for different sensor/weapon balance:**

```csharp
[CreateAssetMenu(fileName = "FactionConfig", menuName = "Game/Faction Config")]
public class FactionConfig : ScriptableObject
{
    public string factionName;
    
    [Header("Combat Philosophy")]
    public float weaponDamageMultiplier = 1.0f;
    public float sensorRangeMultiplier = 1.0f;
    public float powerEfficiencyMultiplier = 1.0f;
    
    [Header("Ship Aesthetics")]
    public Material shipMaterial;
    public Color weaponColor;
    public Color shieldColor;
}

// Faction 1 (Might):
// weaponDamageMultiplier: 1.2x
// sensorRangeMultiplier: 0.8x
// powerEfficiencyMultiplier: 0.9x (less efficient)

// Faction 2 (Exploration):
// weaponDamageMultiplier: 0.9x
// sensorRangeMultiplier: 1.3x
// powerEfficiencyMultiplier: 1.1x (more efficient)
```

### 6.3 Test Scenarios

Create test scenes:

**Scene 1: Flight Test**
- Empty starfield
- Test all flight controls
- Test power presets affecting speed/maneuverability
- No enemies

**Scene 2: Weapons Test**
- Stationary target dummies
- Test all three weapon types
- Test power allocation affecting weapons
- Test ammo/energy management

**Scene 3: Shield Test**
- Enemies that shoot but don't move
- Test shield routing
- Test damage from different directions
- Test shield recharge mechanics
- Test hull damage and passive repair

**Scene 4: Combat Test**
- 2-3 basic AI enemies
- Full combat scenario
- Test target cycling
- Test missile locks
- Test tactical power/shield management

**Scene 5: Sensor Test**
- Enemies at various ranges
- Test sensor tier detection
- Test lock range mechanics
- Test faction sensor differences

---

## PHASE 7: Visual & Audio Polish

### 7.1 Visual Effects

**Required particle effects:**
- Beam weapon firing (bright lance)
- Kinetic weapon muzzle flash + projectile trail
- Missile launch + exhaust trail
- Weapon impacts (different for hull vs shields)
- Shield hit effects (per quadrant)
- Ship explosion (on destruction)
- Engine glow (intensity based on throttle)
- Damage sparks (when hull damaged)

**Shader effects:**
- Shield bubble (transparent, shows quadrant strength)
- Hull damage decals
- Weapon charge-up glow

### 7.2 Audio

**Sound effects needed:**
- Beam weapon fire (sustained laser sound)
- Kinetic weapon fire (cannon thump)
- Missile launch (whoosh)
- Missile lock-on warning (beeping, faster as lock progresses)
- Weapon impacts (different for shields vs hull)
- Shield down warning (alarm)
- Hull critical warning (urgent alarm)
- Power redistribution (subtle electronic hum)
- Engine thrust (varies with throttle)
- Target lock acquired (confirmation beep)
- UI button clicks

**Spatial audio:**
- Weapon fire from enemies (directional)
- Impacts on ship (directional)

### 7.3 Screen Effects

**Post-processing:**
- Slight chromatic aberration when taking heavy damage
- Screen shake on impacts (proportional to damage)
- Red vignette when hull critical (<25%)
- Blue flash when shields depleted

---

## PHASE 8: Controls & Input

### 8.1 Input Mapping

**Flight:**
- W/S: Pitch (up/down)
- A/D: Yaw (left/right)
- Q/E: Roll (counter-clockwise/clockwise)
- Shift/Ctrl: Throttle up/down
- Arrow Keys or IJKL: Strafe (up/down/left/right)
- X: Toggle inertial dampening

**Combat:**
- Space or Mouse1: Fire weapon group 1
- Mouse2: Fire weapon group 2
- M: Fire missiles (if locked)
- T: Cycle targets
- R: Select nearest enemy target

**Systems:**
- 1: Balanced power preset
- 2: Combat power preset
- 3: Speed power preset
- 4: Defensive power preset
- P: Toggle manual power controls

**Shields:**
- [: Cycle shield presets
- ]: Toggle manual shield controls

**Camera:**
- Tab: Cycle camera views (first-person, chase, etc.)
- Mouse: Free look (hold middle mouse button)

**UI:**
- F1: Toggle HUD
- Esc: Pause menu

### 8.2 Input System Implementation

Use Unity's new Input System for better control mapping:

```csharp
// Create Input Actions asset
// Define action maps: Flight, Combat, Systems, UI
// Allow rebinding in settings menu (future)
```

---

## PHASE 9: Performance & Optimization

### 9.1 Optimization Checklist

**Rendering:**
- Use object pooling for projectiles, missiles, effects
- LOD (Level of Detail) for distant ships (future, when more ships)
- Occlusion culling (not critical in open space)
- Efficient particle systems (limit particle count)

**Physics:**
- Raycasts only when needed
- Efficient collision detection (appropriate collision layers)
- Don't physics-update distant/inactive objects

**Code:**
- Cache component references (don't use GetComponent in Update)
- Use events instead of polling where possible
- Minimize garbage collection (avoid frequent allocations)
- Profile regularly (Unity Profiler)

**UI:**
- Only update HUD elements when values change
- Use UI object pooling for dynamic elements (radar blips)
- Avoid Canvas.ForceUpdate() unless necessary

---

## PHASE 10: Testing & Iteration

### 10.1 Core Gameplay Loop Testing

**Test the complete loop:**
1. Spawn in fighter
2. Detect enemy on sensors
3. Navigate to engagement range
4. Acquire target
5. Manage power for combat
6. Route shields based on threat
7. Lock and fire missiles
8. Use beams and kinetics
9. Take damage, manage shields
10. Passive hull repair after combat
11. Pursue or flee based on status

**Key questions to answer:**
- Is power management meaningful or busywork?
- Do shield facings matter in combat?
- Are missiles worth the lock-on time?
- Is passive repair too fast/slow?
- Do the different power presets feel distinct?
- Is combat engaging and skill-based?
- Can you tell the difference between factions?

### 10.2 Balance Tuning

**Metrics to track:**
- Average time to kill (enemy fighter)
- How often shields fully deplete
- How often hull gets damaged
- Power preset usage frequency
- Shield preset usage frequency
- Weapon preference (beam vs kinetic vs missile)
- Survival time against multiple enemies

**Adjust based on data:**
- If shields never deplete → reduce shield capacity or recharge
- If hull always critical → increase shield effectiveness
- If one weapon dominates → adjust damage/cooldown/ammo
- If power management ignored → make power effects more impactful
- If shield routing ignored → make directional damage more punishing

---

## DELIVERABLES CHECKLIST

After completing all phases, you should have:

- [ ] Fully functional 1-man fighter ship
- [ ] 6DOF flight with realistic space physics
- [ ] Unified HUD showing all systems
- [ ] Three weapon types (beam, kinetic, missile) with distinct mechanics
- [ ] Power distribution with presets and manual override
- [ ] Four-quadrant shields with routing options
- [ ] Tiered sensor system with faction differences
- [ ] Missile lock-on mechanics (sensor and power dependent)
- [ ] Passive hull repair system
- [ ] Targeting and radar systems
- [ ] Basic enemy AI for combat testing
- [ ] Visual and audio effects for all actions
- [ ] Balanced, engaging combat gameplay
- [ ] ScriptableObject configs for easy balancing
- [ ] Clean, modular code ready for expansion to multi-player ships
- [ ] Test scenes for each system
- [ ] Performance optimizations

---

## FUTURE EXPANSION NOTES

This 1-man fighter prototype is designed to scale:

**To 2-player ships:**
- Split HUD into Pilot and Weapons stations
- Pilot: Navigation + sensors + power presets
- Weapons: Targeting + firing + shield routing

**To 3-player ships:**
- Add Engineering station
- Engineering: Manual power + shields + repairs + damage control
- Pilot: Navigation + sensors
- Weapons: Targeting + firing

**To networking:**
- Each station becomes a networked player
- ShipSystemManager handles server-side state
- Each player's HUD is their client-side view
- Events synchronize state changes

**Additional features to add later:**
- More ship classes (corvette, cruiser, dreadnought)
- More weapon types (torpedoes, flak, railguns)
- Electronic warfare (jamming, decoys)
- Mission system (objectives, win/loss conditions)
- Damage control minigames
- Subsystem targeting
- Crew skill progression
- Ship customization/loadouts

---

## DEVELOPMENT TIPS

1. **Build incrementally** - Get each system working before moving to next
2. **Test constantly** - Play the game yourself after each major addition
3. **Use debug visualizations** - Draw gizmos for sensor ranges, weapon arcs, etc.
4. **Comment your code** - Especially where systems interact
5. **Version control** - Commit after each working phase
6. **Keep configs flexible** - Use ScriptableObjects, avoid hardcoded values
7. **Profile early** - Don't wait for performance problems
8. **Get feedback** - Have someone playtest as soon as it's remotely playable

---

## SUCCESS CRITERIA

The prototype is successful if a player can:
1. **Feel like a skilled pilot** - combat requires meaningful decisions
2. **Understand all systems** - HUD is clear, not overwhelming
3. **Experience tactical depth** - power/shield management matters
4. **Enjoy the combat** - weapons feel impactful, combat is engaging
5. **See faction differences** - sensors/weapons balance is noticeable
6. **Want to play with friends** - can imagine coordinating with crew

If these are true, you have a solid foundation to build your full multiplayer bridge simulator!

---

## QUESTIONS TO DOCUMENT

As you build, keep notes on:
- What systems feel too complex or too simple?
- Which power preset do you use most? Why?
- Do you ever use manual power/shield controls?
- How often do you take hull damage vs just shield damage?
- Is passive repair noticeable or too subtle?
- Do missiles feel worth the lock-on time?
- Can you effectively fight multiple enemies?
- What's missing that would make combat more fun?

These insights will guide the transition to multi-player ships.

---

**Ready to begin! Start with Phase 1 and work through systematically. Good luck building your space combat simulator!**
