using UnityEngine;
using System;
using SpaceFighter.Core;

namespace SpaceFighter.Systems
{
    /// <summary>
    /// Tracks hull damage and handles passive repair over time.
    /// Hull damage occurs when shields fail to absorb incoming damage.
    /// </summary>
    public class HullIntegritySystem : SystemBase
    {
        [Header("Hull Settings")]
        [SerializeField] private float maxHull = 100f;
        [SerializeField] private float currentHull = 100f;

        [Header("Repair Settings")]
        [SerializeField] private float passiveRepairRate = 0.3f; // % per second
        [SerializeField] private float repairDelayAfterDamage = 5f; // seconds
        [SerializeField] private bool repairEnabled = true;

        [Header("Warning Thresholds")]
        [SerializeField] private float criticalThreshold = 25f; // % for critical warning

        // State
        private float timeSinceLastDamage = 0f;
        private Vector3 lastDamageLocation = Vector3.zero;
        private bool isCritical = false;

        // Power system reference
        private PowerDistributionSystem powerSystem;

        // Events
        public event Action<float, float> OnHullChanged; // current, max
        public event Action<float, Vector3> OnDamageTaken; // amount, location
        public event Action OnCriticalHull;
        public event Action OnHullRestored;
        public event Action OnDestroyed;

        // Public accessors
        public float MaxHull => maxHull;
        public float CurrentHull => currentHull;
        public float HullPercentage => (currentHull / maxHull) * 100f;
        public bool IsDestroyed => currentHull <= 0f;
        public bool IsCritical => isCritical;
        public bool RepairEnabled => repairEnabled;
        public float RepairRate => GetEffectiveRepairRate();
        public Vector3 LastDamageLocation => lastDamageLocation;

        public override void Initialize(Ship ship)
        {
            base.Initialize(ship);

            // Get power system reference
            powerSystem = ship.PowerSystem;

            // Load config values
            if (ship.Config != null)
            {
                maxHull = ship.Config.maxHull;
                passiveRepairRate = ship.Config.passiveRepairRate;
                repairDelayAfterDamage = ship.Config.repairDelayAfterDamage;
            }

            // Initialize to full hull
            currentHull = maxHull;
            timeSinceLastDamage = repairDelayAfterDamage; // Start ready to repair
        }

        public override void SystemUpdate()
        {
            base.SystemUpdate();

            UpdateRepairTimer();
            ProcessPassiveRepair();
            CheckCriticalStatus();
        }

        /// <summary>
        /// Updates the time since last damage counter.
        /// </summary>
        private void UpdateRepairTimer()
        {
            if (timeSinceLastDamage < repairDelayAfterDamage)
            {
                timeSinceLastDamage += Time.deltaTime;
            }
        }

        /// <summary>
        /// Processes passive hull repair if conditions are met.
        /// </summary>
        private void ProcessPassiveRepair()
        {
            if (!repairEnabled) return;
            if (currentHull >= maxHull) return;
            if (timeSinceLastDamage < repairDelayAfterDamage) return;

            float repairAmount = GetEffectiveRepairRate() * Time.deltaTime;
            float previousHull = currentHull;

            currentHull = Mathf.Min(currentHull + repairAmount, maxHull);

            if (!Mathf.Approximately(previousHull, currentHull))
            {
                OnHullChanged?.Invoke(currentHull, maxHull);
            }

            // Check if we've restored from critical
            if (isCritical && HullPercentage > criticalThreshold)
            {
                isCritical = false;
                OnHullRestored?.Invoke();
            }
        }

        /// <summary>
        /// Gets the effective repair rate based on average power.
        /// </summary>
        private float GetEffectiveRepairRate()
        {
            float powerFactor = 1f;

            if (powerSystem != null)
            {
                // Repair rate scales with average power (general ship power)
                float avgPower = (powerSystem.GetPowerLevel(PowerCategory.Engines) +
                                 powerSystem.GetPowerLevel(PowerCategory.Weapons) +
                                 powerSystem.GetPowerLevel(PowerCategory.Shields)) / 3f;
                powerFactor = avgPower / 100f;
            }

            // Convert % per second to actual units per second
            return (passiveRepairRate / 100f) * maxHull * powerFactor;
        }

        /// <summary>
        /// Checks and updates critical hull status.
        /// </summary>
        private void CheckCriticalStatus()
        {
            bool wasCritical = isCritical;
            isCritical = HullPercentage <= criticalThreshold && currentHull > 0;

            if (isCritical && !wasCritical)
            {
                OnCriticalHull?.Invoke();

                if (debugMode)
                {
                    Debug.Log($"[Hull] CRITICAL - Hull at {HullPercentage:F0}%!");
                }
            }
        }

        /// <summary>
        /// Applies damage to the hull.
        /// Called by Ship.cs after shields have absorbed what they can.
        /// </summary>
        public void TakeDamage(float amount, Vector3 hitLocation)
        {
            if (amount <= 0 || IsDestroyed) return;

            float previousHull = currentHull;
            currentHull = Mathf.Max(0, currentHull - amount);
            lastDamageLocation = hitLocation;
            timeSinceLastDamage = 0f;

            OnDamageTaken?.Invoke(amount, hitLocation);
            OnHullChanged?.Invoke(currentHull, maxHull);

            if (debugMode)
            {
                Debug.Log($"[Hull] Took {amount:F1} damage. Hull: {HullPercentage:F0}%");
            }

            // Check for destruction
            if (currentHull <= 0)
            {
                OnDestroyed?.Invoke();

                if (debugMode)
                {
                    Debug.Log("[Hull] DESTROYED!");
                }
            }
        }

        /// <summary>
        /// Sets repair enabled state.
        /// Can be disabled to conserve power in emergencies.
        /// </summary>
        public void SetRepairEnabled(bool enabled)
        {
            repairEnabled = enabled;

            if (debugMode)
            {
                Debug.Log($"[Hull] Passive repair: {(enabled ? "ENABLED" : "DISABLED")}");
            }
        }

        /// <summary>
        /// Instantly repairs hull by specified amount (for pickups, stations, etc.).
        /// </summary>
        public void RepairHull(float amount)
        {
            if (amount <= 0 || IsDestroyed) return;

            float previousHull = currentHull;
            currentHull = Mathf.Min(currentHull + amount, maxHull);

            if (!Mathf.Approximately(previousHull, currentHull))
            {
                OnHullChanged?.Invoke(currentHull, maxHull);

                if (debugMode)
                {
                    Debug.Log($"[Hull] Repaired {amount:F1}. Hull: {HullPercentage:F0}%");
                }
            }
        }

        /// <summary>
        /// Fully repairs the hull.
        /// </summary>
        public void FullRepair()
        {
            currentHull = maxHull;
            isCritical = false;
            OnHullChanged?.Invoke(currentHull, maxHull);
            OnHullRestored?.Invoke();
        }

        public override void ResetSystem()
        {
            base.ResetSystem();
            currentHull = maxHull;
            timeSinceLastDamage = repairDelayAfterDamage;
            isCritical = false;
            lastDamageLocation = Vector3.zero;
            repairEnabled = true;
        }

        public override string GetStatusString()
        {
            string repairStatus = "";
            if (repairEnabled && currentHull < maxHull)
            {
                if (timeSinceLastDamage >= repairDelayAfterDamage)
                {
                    repairStatus = $" +{GetEffectiveRepairRate():F1}/s";
                }
                else
                {
                    repairStatus = $" (repair in {repairDelayAfterDamage - timeSinceLastDamage:F1}s)";
                }
            }

            return $"HULL: {HullPercentage:F0}%{repairStatus}{(isCritical ? " CRITICAL!" : "")}";
        }
    }
}
