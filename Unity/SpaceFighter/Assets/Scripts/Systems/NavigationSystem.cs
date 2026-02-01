using UnityEngine;
using UnityEngine.InputSystem;
using System;
using SpaceFighter.Core;

namespace SpaceFighter.Systems
{
    /// <summary>
    /// Handles all ship movement and 6-degrees-of-freedom flight physics.
    /// Speed and maneuverability are affected by engine power allocation.
    /// </summary>
    public class NavigationSystem : SystemBase
    {
        [Header("Base Flight Stats")]
        [SerializeField] private float baseMaxSpeed = 500f;
        [SerializeField] private float baseMaxBoostSpeed = 800f;
        [SerializeField] private float baseAcceleration = 100f;
        [SerializeField] private float basePitchSpeed = 50f;
        [SerializeField] private float baseYawSpeed = 50f;
        [SerializeField] private float baseRollSpeed = 80f;
        [SerializeField] private float boostMultiplier = 2f;

        [Header("Throttle Settings")]
        [SerializeField] private float throttleChangeRate = 0.5f;
        [SerializeField] private float currentThrottle = 0f;

        [Header("Strafe Settings")]
        [SerializeField] private float strafeSpeed = 50f;

        [Header("Inertial Dampening")]
        [SerializeField] private bool inertialDampeningEnabled = true;
        [SerializeField] private float dampeningStrength = 2f;

        // Components
        private Rigidbody rb;
        private PowerDistributionSystem powerSystem;

        // State
        private bool isBoosting = false;
        private Vector3 strafeInput = Vector3.zero;

        // Events
        public event Action<bool> OnDampeningChanged;
        public event Action<bool> OnBoostChanged;

        // Public accessors
        public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude : 0f;
        public float CurrentThrottle => currentThrottle;
        public float MaxSpeed => GetEffectiveMaxSpeed();
        public bool IsBoosting => isBoosting;
        public bool InertialDampeningEnabled => inertialDampeningEnabled;
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;
        public Vector3 AngularVelocity => rb != null ? rb.angularVelocity : Vector3.zero;

        public override void Initialize(Ship ship)
        {
            base.Initialize(ship);

            // Get rigidbody
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            // Configure rigidbody for space physics
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Get power system reference
            powerSystem = ship.PowerSystem;

            // Load config values
            if (ship.Config != null)
            {
                baseMaxSpeed = ship.Config.maxSpeed;
                baseMaxBoostSpeed = ship.Config.maxBoostSpeed;
                baseAcceleration = ship.Config.acceleration;
                basePitchSpeed = ship.Config.turnRate;
                baseYawSpeed = ship.Config.turnRate;
                baseRollSpeed = ship.Config.turnRate * 1.5f;
                boostMultiplier = ship.Config.boostMultiplier;
            }
        }

        public override void SystemUpdate()
        {
            base.SystemUpdate();

            HandleThrottleInput();
            HandleBoostInput();
            HandleDampeningToggle();
            HandleStrafeInput();
        }

        public override void SystemFixedUpdate()
        {
            base.SystemFixedUpdate();

            ApplyRotation();
            ApplyThrust();
            ApplyStrafe();
            ApplyInertialDampening();
            EnforceSpeedLimit();
        }

        /// <summary>
        /// Gets the effective max speed based on power allocation and boost.
        /// </summary>
        public float GetEffectiveMaxSpeed()
        {
            float powerMultiplier = GetPowerMultiplier();
            float baseSpeed = isBoosting ? baseMaxBoostSpeed : baseMaxSpeed;
            return baseSpeed * powerMultiplier;
        }

        /// <summary>
        /// Gets the effective turn rate based on power allocation.
        /// </summary>
        public float GetEffectiveTurnRate()
        {
            return basePitchSpeed * GetPowerMultiplier();
        }

        /// <summary>
        /// Gets the power multiplier from the power distribution system.
        /// </summary>
        private float GetPowerMultiplier()
        {
            if (powerSystem != null)
            {
                return powerSystem.GetPowerMultiplier(PowerCategory.Engines);
            }
            return 1f;
        }

        /// <summary>
        /// Handles throttle input via Shift (up) and Ctrl (down).
        /// </summary>
        private void HandleThrottleInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            float throttleInput = 0f;

            if (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed)
            {
                throttleInput = 1f;
            }
            else if (kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed)
            {
                throttleInput = -1f;
            }

            currentThrottle += throttleInput * throttleChangeRate * Time.deltaTime;
            currentThrottle = Mathf.Clamp(currentThrottle, -1f, 1f);
        }

        /// <summary>
        /// Handles boost input via Spacebar.
        /// </summary>
        private void HandleBoostInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            bool wasBosting = isBoosting;
            isBoosting = kb.spaceKey.isPressed && currentThrottle > 0f;

            if (wasBosting != isBoosting)
            {
                OnBoostChanged?.Invoke(isBoosting);
            }
        }

        /// <summary>
        /// Handles inertial dampening toggle with X key.
        /// </summary>
        private void HandleDampeningToggle()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.xKey.wasPressedThisFrame)
            {
                inertialDampeningEnabled = !inertialDampeningEnabled;
                OnDampeningChanged?.Invoke(inertialDampeningEnabled);

                if (debugMode)
                {
                    Debug.Log($"[Navigation] Inertial dampening: {(inertialDampeningEnabled ? "ON" : "OFF")}");
                }
            }
        }

        /// <summary>
        /// Handles strafe input via arrow keys or IJKL.
        /// </summary>
        private void HandleStrafeInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            strafeInput = Vector3.zero;

            // Vertical strafe (up/down)
            if (kb.upArrowKey.isPressed || kb.iKey.isPressed)
            {
                strafeInput.y = 1f;
            }
            else if (kb.downArrowKey.isPressed || kb.kKey.isPressed)
            {
                strafeInput.y = -1f;
            }

            // Horizontal strafe (left/right)
            if (kb.leftArrowKey.isPressed || kb.jKey.isPressed)
            {
                strafeInput.x = -1f;
            }
            else if (kb.rightArrowKey.isPressed || kb.lKey.isPressed)
            {
                strafeInput.x = 1f;
            }
        }

        /// <summary>
        /// Applies rotation based on WASD (pitch/yaw) and Q/E (roll).
        /// </summary>
        private void ApplyRotation()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            float powerMult = GetPowerMultiplier();
            float pitch = 0f;
            float yaw = 0f;
            float roll = 0f;

            // WASD controls pitch and yaw
            if (kb.wKey.isPressed) pitch = 1f;
            if (kb.sKey.isPressed) pitch = -1f;
            if (kb.aKey.isPressed) yaw = -1f;
            if (kb.dKey.isPressed) yaw = 1f;

            // Q/E controls roll
            if (kb.qKey.isPressed) roll = 1f;
            if (kb.eKey.isPressed) roll = -1f;

            // Calculate rotation torque with power multiplier
            Vector3 rotationTorque = new Vector3(
                pitch * basePitchSpeed * powerMult,
                yaw * baseYawSpeed * powerMult,
                roll * baseRollSpeed * powerMult
            );

            // Apply rotation in local space
            rb.AddRelativeTorque(rotationTorque * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Applies forward/backward thrust based on throttle.
        /// </summary>
        private void ApplyThrust()
        {
            if (Mathf.Approximately(currentThrottle, 0f)) return;

            float powerMult = GetPowerMultiplier();
            float thrustAmount = baseAcceleration * currentThrottle * powerMult;

            // Apply boost multiplier
            if (isBoosting)
            {
                thrustAmount *= boostMultiplier;
            }

            // Apply thrust in ship's forward direction
            Vector3 thrustForce = transform.forward * thrustAmount;
            rb.AddForce(thrustForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Applies strafe thrust in local up/right directions.
        /// </summary>
        private void ApplyStrafe()
        {
            if (strafeInput.sqrMagnitude < 0.01f) return;

            float powerMult = GetPowerMultiplier();
            Vector3 strafeForce = (transform.right * strafeInput.x + transform.up * strafeInput.y)
                                  * strafeSpeed * powerMult;

            rb.AddForce(strafeForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Applies inertial dampening to reduce velocity when not thrusting.
        /// </summary>
        private void ApplyInertialDampening()
        {
            if (!inertialDampeningEnabled) return;
            if (rb.linearVelocity.magnitude < 0.1f) return;

            Vector3 desiredDirection = currentThrottle > 0 ? transform.forward :
                                       currentThrottle < 0 ? -transform.forward : Vector3.zero;

            // If no throttle, dampen all velocity
            if (Mathf.Approximately(currentThrottle, 0f))
            {
                Vector3 dampeningForce = -rb.linearVelocity.normalized * dampeningStrength;
                rb.AddForce(dampeningForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
                return;
            }

            // Dampen perpendicular velocity
            Vector3 perpendicularVelocity = rb.linearVelocity - Vector3.Project(rb.linearVelocity, desiredDirection);

            if (perpendicularVelocity.magnitude > 0.1f)
            {
                Vector3 dampeningForce = -perpendicularVelocity.normalized * dampeningStrength;
                rb.AddForce(dampeningForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }

        /// <summary>
        /// Enforces the speed limit.
        /// </summary>
        private void EnforceSpeedLimit()
        {
            float maxSpeed = GetEffectiveMaxSpeed();

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        /// <summary>
        /// Sets throttle to a specific value (for external control/AI).
        /// </summary>
        public void SetThrottle(float value)
        {
            currentThrottle = Mathf.Clamp(value, -1f, 1f);
        }

        /// <summary>
        /// Cuts throttle to zero immediately.
        /// </summary>
        public void CutThrottle()
        {
            currentThrottle = 0f;
        }

        /// <summary>
        /// Sets throttle to maximum.
        /// </summary>
        public void FullThrottle()
        {
            currentThrottle = 1f;
        }

        public override void ResetSystem()
        {
            base.ResetSystem();
            currentThrottle = 0f;
            isBoosting = false;
            inertialDampeningEnabled = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        public override string GetStatusString()
        {
            return $"NAV: SPD {CurrentSpeed:F0}m/s THR {currentThrottle * 100:F0}% " +
                   $"DAMP:{(inertialDampeningEnabled ? "ON" : "OFF")} " +
                   $"{(isBoosting ? "BOOST" : "")}";
        }
    }
}
