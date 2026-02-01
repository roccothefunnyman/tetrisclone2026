using UnityEngine;

namespace SpaceFighter.Core
{
    /// <summary>
    /// Abstract base class for all ship systems.
    /// Provides common functionality and interface for system management.
    /// </summary>
    public abstract class SystemBase : MonoBehaviour
    {
        [Header("System Base")]
        [SerializeField] protected bool systemEnabled = true;
        [SerializeField] protected bool debugMode = false;

        protected Ship ownerShip;
        protected bool isInitialized = false;

        /// <summary>
        /// Whether this system is currently enabled and operational.
        /// </summary>
        public bool IsEnabled => systemEnabled;

        /// <summary>
        /// Whether this system has been properly initialized.
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Reference to the ship that owns this system.
        /// </summary>
        public Ship OwnerShip => ownerShip;

        /// <summary>
        /// Called by Ship.cs to initialize this system.
        /// Override in derived classes to perform system-specific initialization.
        /// </summary>
        public virtual void Initialize(Ship ship)
        {
            ownerShip = ship;
            isInitialized = true;

            if (debugMode)
            {
                Debug.Log($"[{GetType().Name}] Initialized for ship: {ship.ShipName}");
            }
        }

        /// <summary>
        /// Called by Ship.cs each frame to update this system.
        /// Override in derived classes for system-specific logic.
        /// </summary>
        public virtual void SystemUpdate()
        {
            if (!systemEnabled || !isInitialized) return;
        }

        /// <summary>
        /// Called by Ship.cs each fixed update for physics-based systems.
        /// Override in derived classes for physics-related logic.
        /// </summary>
        public virtual void SystemFixedUpdate()
        {
            if (!systemEnabled || !isInitialized) return;
        }

        /// <summary>
        /// Enables or disables this system.
        /// </summary>
        public virtual void SetEnabled(bool enabled)
        {
            systemEnabled = enabled;
            OnSystemEnabledChanged(enabled);
        }

        /// <summary>
        /// Called when the system's enabled state changes.
        /// Override to react to enable/disable events.
        /// </summary>
        protected virtual void OnSystemEnabledChanged(bool enabled)
        {
            if (debugMode)
            {
                Debug.Log($"[{GetType().Name}] System {(enabled ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Gets a formatted status string for debugging/UI.
        /// Override to provide system-specific status information.
        /// </summary>
        public virtual string GetStatusString()
        {
            return $"{GetType().Name}: {(systemEnabled ? "ONLINE" : "OFFLINE")}";
        }

        /// <summary>
        /// Resets the system to its default state.
        /// Override to provide system-specific reset logic.
        /// </summary>
        public virtual void ResetSystem()
        {
            if (debugMode)
            {
                Debug.Log($"[{GetType().Name}] System reset");
            }
        }
    }
}
