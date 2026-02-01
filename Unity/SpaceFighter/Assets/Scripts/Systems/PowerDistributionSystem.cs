using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using SpaceFighter.Core;

namespace SpaceFighter.Systems
{
    /// <summary>
    /// Power category enumeration for the three main power consumers.
    /// </summary>
    public enum PowerCategory
    {
        Engines,    // Affects NavigationSystem speed and maneuverability
        Weapons,    // Affects WeaponSystem damage, fire rate, lock speed
        Shields     // Affects ShieldSystem recharge rate
    }

    /// <summary>
    /// Preset power distribution configurations.
    /// </summary>
    public enum PowerPreset
    {
        Balanced,   // 33/33/33 - even distribution
        Combat,     // 20/50/30 - weapon-focused
        Speed,      // 60/20/20 - engine-focused
        Defensive   // 20/20/60 - shield-focused
    }

    /// <summary>
    /// Central power management system.
    /// All other systems draw power from this and their effectiveness
    /// is modified by their power allocation.
    /// </summary>
    public class PowerDistributionSystem : SystemBase
    {
        [Header("Power Settings")]
        [SerializeField] private float totalPowerGeneration = 100f;
        [SerializeField] private float redistributionSpeed = 1.5f;

        [Header("Current State")]
        [SerializeField] private PowerPreset activePreset = PowerPreset.Balanced;
        [SerializeField] private bool isManualMode = false;

        // Target and current allocations (0-100 percentage)
        private Dictionary<PowerCategory, float> targetAllocation = new Dictionary<PowerCategory, float>();
        private Dictionary<PowerCategory, float> currentAllocation = new Dictionary<PowerCategory, float>();

        // Preset definitions
        private static readonly Dictionary<PowerPreset, Dictionary<PowerCategory, float>> PresetAllocations =
            new Dictionary<PowerPreset, Dictionary<PowerCategory, float>>
            {
                {
                    PowerPreset.Balanced, new Dictionary<PowerCategory, float>
                    {
                        { PowerCategory.Engines, 33.33f },
                        { PowerCategory.Weapons, 33.33f },
                        { PowerCategory.Shields, 33.34f }
                    }
                },
                {
                    PowerPreset.Combat, new Dictionary<PowerCategory, float>
                    {
                        { PowerCategory.Engines, 20f },
                        { PowerCategory.Weapons, 50f },
                        { PowerCategory.Shields, 30f }
                    }
                },
                {
                    PowerPreset.Speed, new Dictionary<PowerCategory, float>
                    {
                        { PowerCategory.Engines, 60f },
                        { PowerCategory.Weapons, 20f },
                        { PowerCategory.Shields, 20f }
                    }
                },
                {
                    PowerPreset.Defensive, new Dictionary<PowerCategory, float>
                    {
                        { PowerCategory.Engines, 20f },
                        { PowerCategory.Weapons, 20f },
                        { PowerCategory.Shields, 60f }
                    }
                }
            };

        // Events
        public event Action<PowerCategory, float> OnPowerLevelChanged;
        public event Action<PowerPreset> OnPresetChanged;
        public event Action<bool> OnManualModeChanged;

        // Public accessors
        public PowerPreset ActivePreset => activePreset;
        public bool IsManualMode => isManualMode;
        public float TotalPowerGeneration => totalPowerGeneration;

        public override void Initialize(Ship ship)
        {
            base.Initialize(ship);

            // Load config values
            if (ship.Config != null)
            {
                totalPowerGeneration = ship.Config.totalPowerGeneration;
                redistributionSpeed = ship.Config.powerRedistributionSpeed;
            }

            // Initialize allocations
            InitializeAllocations();

            // Apply default preset
            SetPreset(PowerPreset.Balanced, immediate: true);
        }

        private void InitializeAllocations()
        {
            foreach (PowerCategory category in Enum.GetValues(typeof(PowerCategory)))
            {
                targetAllocation[category] = 33.33f;
                currentAllocation[category] = 33.33f;
            }
        }

        public override void SystemUpdate()
        {
            base.SystemUpdate();

            HandleInput();
            UpdatePowerDistribution();
        }

        /// <summary>
        /// Handles keyboard input for power presets.
        /// </summary>
        private void HandleInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            // Number keys for presets
            if (kb.digit1Key.wasPressedThisFrame)
            {
                SetPreset(PowerPreset.Balanced);
            }
            else if (kb.digit2Key.wasPressedThisFrame)
            {
                SetPreset(PowerPreset.Combat);
            }
            else if (kb.digit3Key.wasPressedThisFrame)
            {
                SetPreset(PowerPreset.Speed);
            }
            else if (kb.digit4Key.wasPressedThisFrame)
            {
                SetPreset(PowerPreset.Defensive);
            }

            // P key toggles manual mode
            if (kb.pKey.wasPressedThisFrame)
            {
                ToggleManualMode();
            }
        }

        /// <summary>
        /// Gradually shifts current power levels toward target allocation.
        /// </summary>
        private void UpdatePowerDistribution()
        {
            float shiftAmount = (100f / redistributionSpeed) * Time.deltaTime;
            bool anyChanged = false;

            foreach (PowerCategory category in Enum.GetValues(typeof(PowerCategory)))
            {
                float target = targetAllocation[category];
                float current = currentAllocation[category];

                if (!Mathf.Approximately(current, target))
                {
                    float newValue = Mathf.MoveTowards(current, target, shiftAmount);
                    currentAllocation[category] = newValue;

                    // Fire event if significant change (>1%)
                    if (Mathf.Abs(newValue - current) > 1f)
                    {
                        anyChanged = true;
                        OnPowerLevelChanged?.Invoke(category, newValue);
                    }
                }
            }

            // Normalize to ensure total is exactly 100%
            if (anyChanged)
            {
                NormalizeCurrentAllocation();
            }
        }

        /// <summary>
        /// Sets a power preset. Power shifts gradually unless immediate is true.
        /// </summary>
        public void SetPreset(PowerPreset preset, bool immediate = false)
        {
            if (!PresetAllocations.ContainsKey(preset)) return;

            activePreset = preset;
            isManualMode = false;

            var presetValues = PresetAllocations[preset];
            foreach (var kvp in presetValues)
            {
                targetAllocation[kvp.Key] = kvp.Value;

                if (immediate)
                {
                    currentAllocation[kvp.Key] = kvp.Value;
                    OnPowerLevelChanged?.Invoke(kvp.Key, kvp.Value);
                }
            }

            OnPresetChanged?.Invoke(preset);
            OnManualModeChanged?.Invoke(false);

            if (debugMode)
            {
                Debug.Log($"[PowerSystem] Preset set to: {preset}");
            }
        }

        /// <summary>
        /// Sets manual allocation for a specific category.
        /// Automatically adjusts other categories to maintain 100% total.
        /// </summary>
        public void SetManualAllocation(PowerCategory category, float percentage)
        {
            isManualMode = true;
            percentage = Mathf.Clamp(percentage, 0f, 100f);

            float oldValue = targetAllocation[category];
            float difference = percentage - oldValue;

            targetAllocation[category] = percentage;

            // Distribute the difference among other categories proportionally
            float otherTotal = 0f;
            foreach (PowerCategory other in Enum.GetValues(typeof(PowerCategory)))
            {
                if (other != category)
                {
                    otherTotal += targetAllocation[other];
                }
            }

            if (otherTotal > 0)
            {
                foreach (PowerCategory other in Enum.GetValues(typeof(PowerCategory)))
                {
                    if (other != category)
                    {
                        float ratio = targetAllocation[other] / otherTotal;
                        targetAllocation[other] -= difference * ratio;
                        targetAllocation[other] = Mathf.Max(0f, targetAllocation[other]);
                    }
                }
            }

            NormalizeTargetAllocation();
            OnManualModeChanged?.Invoke(true);
        }

        /// <summary>
        /// Toggles between preset mode and manual mode.
        /// </summary>
        public void ToggleManualMode()
        {
            isManualMode = !isManualMode;
            OnManualModeChanged?.Invoke(isManualMode);

            if (debugMode)
            {
                Debug.Log($"[PowerSystem] Manual mode: {isManualMode}");
            }
        }

        /// <summary>
        /// Gets the current power level for a category (0-100).
        /// </summary>
        public float GetPowerLevel(PowerCategory category)
        {
            return currentAllocation.ContainsKey(category) ? currentAllocation[category] : 0f;
        }

        /// <summary>
        /// Gets the power multiplier for a category (0-1).
        /// Used by other systems to scale their effectiveness.
        /// </summary>
        public float GetPowerMultiplier(PowerCategory category)
        {
            float powerLevel = GetPowerLevel(category);

            // Non-linear scaling: even low power provides some capability
            // 100% power = 1.0x multiplier
            // 50% power = 0.6x multiplier
            // 25% power = 0.4x multiplier
            // 0% power = 0.2x multiplier (minimum capability)
            return 0.2f + (powerLevel / 100f) * 0.8f;
        }

        /// <summary>
        /// Gets the target allocation for a category.
        /// </summary>
        public float GetTargetAllocation(PowerCategory category)
        {
            return targetAllocation.ContainsKey(category) ? targetAllocation[category] : 0f;
        }

        /// <summary>
        /// Normalizes target allocation to exactly 100%.
        /// </summary>
        private void NormalizeTargetAllocation()
        {
            float total = 0f;
            foreach (var kvp in targetAllocation)
            {
                total += kvp.Value;
            }

            if (total > 0 && !Mathf.Approximately(total, 100f))
            {
                float scale = 100f / total;
                var categories = new List<PowerCategory>(targetAllocation.Keys);
                foreach (var category in categories)
                {
                    targetAllocation[category] *= scale;
                }
            }
        }

        /// <summary>
        /// Normalizes current allocation to exactly 100%.
        /// </summary>
        private void NormalizeCurrentAllocation()
        {
            float total = 0f;
            foreach (var kvp in currentAllocation)
            {
                total += kvp.Value;
            }

            if (total > 0 && !Mathf.Approximately(total, 100f))
            {
                float scale = 100f / total;
                var categories = new List<PowerCategory>(currentAllocation.Keys);
                foreach (var category in categories)
                {
                    currentAllocation[category] *= scale;
                }
            }
        }

        public override void ResetSystem()
        {
            base.ResetSystem();
            SetPreset(PowerPreset.Balanced, immediate: true);
        }

        public override string GetStatusString()
        {
            return $"Power: E:{GetPowerLevel(PowerCategory.Engines):F0}% " +
                   $"W:{GetPowerLevel(PowerCategory.Weapons):F0}% " +
                   $"S:{GetPowerLevel(PowerCategory.Shields):F0}% " +
                   $"[{(isManualMode ? "MANUAL" : activePreset.ToString().ToUpper())}]";
        }
    }
}
