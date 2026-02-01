using UnityEngine;
using System;
using System.Collections.Generic;
using SpaceFighter.Systems;

namespace SpaceFighter.Core
{
    /// <summary>
    /// Main ship controller that owns and coordinates all ship systems.
    /// This is the central reference point for all ship components.
    /// </summary>
    public class Ship : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ShipConfig shipConfig;
        [SerializeField] private bool autoInitialize = true;

        [Header("Systems (Auto-populated if null)")]
        [SerializeField] private PowerDistributionSystem powerSystem;
        [SerializeField] private NavigationSystem navigationSystem;
        [SerializeField] private HullIntegritySystem hullSystem;
        // Future systems:
        // [SerializeField] private WeaponSystem weaponSystem;
        // [SerializeField] private ShieldSystem shieldSystem;
        // [SerializeField] private SensorSystem sensorSystem;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Ship identity
        private string shipId;
        private bool isInitialized = false;
        private bool isDestroyed = false;

        // Cached system list for iteration
        private List<SystemBase> allSystems = new List<SystemBase>();

        // Events
        public event Action<Ship> OnShipInitialized;
        public event Action<Ship> OnShipDestroyed;
        public event Action<float, Vector3, DamageType> OnDamageTaken;

        // Public accessors
        public string ShipName => shipConfig != null ? shipConfig.shipName : "Unknown";
        public string ShipClass => shipConfig != null ? shipConfig.shipClass : "Unknown";
        public Faction ShipFaction => shipConfig != null ? shipConfig.faction : Faction.Neutral;
        public string ShipId => shipId;
        public ShipConfig Config => shipConfig;
        public bool IsInitialized => isInitialized;
        public bool IsDestroyed => isDestroyed;

        // System accessors
        public PowerDistributionSystem PowerSystem => powerSystem;
        public NavigationSystem NavigationSystem => navigationSystem;
        public HullIntegritySystem HullSystem => hullSystem;

        private void Awake()
        {
            // Generate unique ship ID
            shipId = Guid.NewGuid().ToString().Substring(0, 8);

            if (autoInitialize)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (!isInitialized || isDestroyed) return;

            // Update all systems
            foreach (var system in allSystems)
            {
                if (system != null && system.IsEnabled)
                {
                    system.SystemUpdate();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!isInitialized || isDestroyed) return;

            // Fixed update all systems
            foreach (var system in allSystems)
            {
                if (system != null && system.IsEnabled)
                {
                    system.SystemFixedUpdate();
                }
            }
        }

        /// <summary>
        /// Initializes the ship and all its systems.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning($"[Ship] {ShipName} already initialized");
                return;
            }

            // Create default config if none assigned
            if (shipConfig == null)
            {
                shipConfig = ScriptableObject.CreateInstance<ShipConfig>();
                Debug.LogWarning($"[Ship] No ShipConfig assigned, using defaults");
            }

            // Find or create systems
            FindOrCreateSystems();

            // Initialize all systems
            InitializeSystems();

            isInitialized = true;

            if (debugMode)
            {
                Debug.Log($"[Ship] {ShipName} ({ShipClass}) initialized with ID: {shipId}");
            }

            OnShipInitialized?.Invoke(this);
        }

        /// <summary>
        /// Finds existing systems on the GameObject or creates them.
        /// </summary>
        private void FindOrCreateSystems()
        {
            // Power Distribution System
            if (powerSystem == null)
            {
                powerSystem = GetComponent<PowerDistributionSystem>();
                if (powerSystem == null)
                {
                    powerSystem = gameObject.AddComponent<PowerDistributionSystem>();
                }
            }

            // Navigation System
            if (navigationSystem == null)
            {
                navigationSystem = GetComponent<NavigationSystem>();
                if (navigationSystem == null)
                {
                    navigationSystem = gameObject.AddComponent<NavigationSystem>();
                }
            }

            // Hull Integrity System
            if (hullSystem == null)
            {
                hullSystem = GetComponent<HullIntegritySystem>();
                if (hullSystem == null)
                {
                    hullSystem = gameObject.AddComponent<HullIntegritySystem>();
                }
            }

            // Build system list
            allSystems.Clear();
            allSystems.Add(powerSystem);
            allSystems.Add(navigationSystem);
            allSystems.Add(hullSystem);
        }

        /// <summary>
        /// Initializes all ship systems with configuration.
        /// </summary>
        private void InitializeSystems()
        {
            foreach (var system in allSystems)
            {
                if (system != null)
                {
                    system.Initialize(this);
                }
            }
        }

        /// <summary>
        /// Gets a specific system by type.
        /// </summary>
        public T GetSystem<T>() where T : SystemBase
        {
            foreach (var system in allSystems)
            {
                if (system is T typedSystem)
                {
                    return typedSystem;
                }
            }
            return null;
        }

        /// <summary>
        /// Applies damage to the ship.
        /// Damage is first absorbed by shields, then hull.
        /// </summary>
        public void TakeDamage(float amount, Vector3 hitPosition, DamageType damageType = DamageType.Generic)
        {
            if (isDestroyed) return;

            float remainingDamage = amount;

            // TODO: Route through shield system first when implemented
            // remainingDamage = shieldSystem.AbsorbDamage(amount, hitPosition);

            // Apply remaining damage to hull
            if (remainingDamage > 0 && hullSystem != null)
            {
                hullSystem.TakeDamage(remainingDamage, hitPosition);
            }

            OnDamageTaken?.Invoke(amount, hitPosition, damageType);

            // Check for destruction
            if (hullSystem != null && hullSystem.IsDestroyed)
            {
                DestroyShip();
            }

            if (debugMode)
            {
                Debug.Log($"[Ship] {ShipName} took {amount} damage at {hitPosition}");
            }
        }

        /// <summary>
        /// Destroys the ship.
        /// </summary>
        public void DestroyShip()
        {
            if (isDestroyed) return;

            isDestroyed = true;

            if (debugMode)
            {
                Debug.Log($"[Ship] {ShipName} destroyed!");
            }

            OnShipDestroyed?.Invoke(this);

            // TODO: Spawn explosion effect
            // TODO: Handle respawn or game over logic
        }

        /// <summary>
        /// Resets the ship to full health and default state.
        /// </summary>
        public void ResetShip()
        {
            isDestroyed = false;

            foreach (var system in allSystems)
            {
                if (system != null)
                {
                    system.ResetSystem();
                }
            }

            if (debugMode)
            {
                Debug.Log($"[Ship] {ShipName} reset to full status");
            }
        }
    }

    /// <summary>
    /// Types of damage that can be inflicted.
    /// </summary>
    public enum DamageType
    {
        Generic,
        Beam,
        Kinetic,
        Missile,
        Collision
    }
}
