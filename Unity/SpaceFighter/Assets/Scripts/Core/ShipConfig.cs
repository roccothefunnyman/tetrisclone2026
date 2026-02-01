using UnityEngine;
using System.Collections.Generic;

namespace SpaceFighter.Core
{
    /// <summary>
    /// ScriptableObject configuration for ship stats and properties.
    /// Allows easy balancing and faction differences.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipConfig", menuName = "SpaceFighter/Ship Config")]
    public class ShipConfig : ScriptableObject
    {
        [Header("Identity")]
        public string shipName = "Fighter";
        public string shipClass = "Fighter";
        public Faction faction = Faction.Neutral;

        [Header("Hull")]
        [Tooltip("Maximum hull integrity")]
        public float maxHull = 100f;
        [Tooltip("Passive repair rate (% per second)")]
        [Range(0f, 5f)]
        public float passiveRepairRate = 0.3f;
        [Tooltip("Delay before passive repair begins (seconds)")]
        public float repairDelayAfterDamage = 5f;

        [Header("Shields")]
        [Tooltip("Maximum total shield capacity")]
        public float maxShields = 100f;
        [Tooltip("Shield recharge rate at full power (% per second)")]
        [Range(0f, 20f)]
        public float baseShieldRechargeRate = 5f;
        [Tooltip("Minimum shield allocation per quadrant (%)")]
        [Range(0f, 25f)]
        public float minShieldPerQuadrant = 5f;

        [Header("Power")]
        [Tooltip("Total power generation (units)")]
        public float totalPowerGeneration = 100f;
        [Tooltip("Time for full power redistribution (seconds)")]
        [Range(0.5f, 10f)]
        public float powerRedistributionSpeed = 1.5f;

        [Header("Navigation")]
        [Tooltip("Maximum forward speed (m/s)")]
        public float maxSpeed = 500f;
        [Tooltip("Maximum boost speed (m/s)")]
        public float maxBoostSpeed = 800f;
        [Tooltip("Base turn rate (degrees per second)")]
        public float turnRate = 45f;
        [Tooltip("Acceleration (m/s^2)")]
        public float acceleration = 100f;
        [Tooltip("Boost thrust multiplier")]
        public float boostMultiplier = 2f;

        [Header("Sensors")]
        [Tooltip("Long range sensor distance (m) - blip only")]
        public float longRangeSensorRange = 5000f;
        [Tooltip("Medium range sensor distance (m) - IFF and basic info")]
        public float mediumRangeSensorRange = 2000f;
        [Tooltip("Short range sensor distance (m) - full details")]
        public float shortRangeSensorRange = 500f;
        [Tooltip("Maximum lock-on range (m)")]
        public float lockRange = 400f;

        [Header("Weapons Energy")]
        [Tooltip("Maximum energy pool for beam weapons")]
        public float maxWeaponEnergy = 100f;
        [Tooltip("Energy regeneration rate at full power (units per second)")]
        public float energyRegenRate = 15f;
    }

    /// <summary>
    /// Ship faction enumeration for IFF and balance differences.
    /// </summary>
    public enum Faction
    {
        Neutral,
        Faction1_Might,      // Combat-focused: stronger weapons, weaker sensors
        Faction2_Exploration // Exploration-focused: better sensors, efficient power
    }
}
