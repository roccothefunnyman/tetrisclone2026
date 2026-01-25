using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Core ship controller handling 6-degrees-of-freedom space flight physics.
/// Provides momentum-based movement with optional inertial dampening.
/// </summary>
public class ShipController : MonoBehaviour
{
    [Header("Thrust Settings")]
    [SerializeField] private float maxThrust = 100f;
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private float throttleChangeRate = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float pitchSpeed = 50f;
    [SerializeField] private float yawSpeed = 50f;
    [SerializeField] private float rollSpeed = 80f;

    [Header("Speed Limits")]
    [SerializeField] private float maxSpeed = 200f;
    [SerializeField] private float maxBoostSpeed = 400f;

    [Header("Inertial Dampening")]
    [SerializeField] private bool inertialDampeningEnabled = true;
    [SerializeField] private float dampeningStrength = 2f;

    [Header("Ship Status")]
    [SerializeField] private float maxShield = 100f;
    [SerializeField] private float maxPower = 100f;

    // Private state
    private Rigidbody rb;
    private float currentThrottle = 0f;
    private bool isBoosting = false;
    private float currentShield;
    private float currentPower;

    // Power distribution (engines, shields, weapons) - placeholder for future expansion
    private float enginePower = 0.33f;
    private float shieldPower = 0.33f;
    private float weaponPower = 0.33f;

    // Public accessors for HUD
    public float CurrentSpeed => rb != null ? rb.velocity.magnitude : 0f;
    public float CurrentThrottle => currentThrottle;
    public float MaxSpeed => isBoosting ? maxBoostSpeed : maxSpeed;
    public bool IsBoosting => isBoosting;
    public bool InertialDampeningEnabled => inertialDampeningEnabled;
    public Vector3 Velocity => rb != null ? rb.velocity : Vector3.zero;
    public Vector3 AngularVelocity => rb != null ? rb.angularVelocity : Vector3.zero;
    public float ShieldPercent => (currentShield / maxShield) * 100f;
    public float PowerPercent => (currentPower / maxPower) * 100f;
    public float EnginePower => enginePower;
    public float ShieldPower => shieldPower;
    public float WeaponPower => weaponPower;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure rigidbody for space physics
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.5f; // Small angular drag for responsive rotation
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Initialize ship status
        currentShield = maxShield;
        currentPower = maxPower;
    }

    private void Update()
    {
        HandleThrottleInput();
        HandleBoostInput();
        HandleDampeningToggle();
    }

    private void FixedUpdate()
    {
        ApplyRotation();
        ApplyThrust();
        ApplyInertialDampening();
        EnforceSpeedLimit();
    }

    /// <summary>
    /// Handles throttle adjustment via Shift (up) and Ctrl (down).
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
    /// Handles boost activation via Spacebar.
    /// </summary>
    private void HandleBoostInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        isBoosting = kb.spaceKey.isPressed && currentThrottle > 0f;
    }

    /// <summary>
    /// Toggles inertial dampening with the V key.
    /// </summary>
    private void HandleDampeningToggle()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.vKey.wasPressedThisFrame)
        {
            inertialDampeningEnabled = !inertialDampeningEnabled;
        }
    }

    /// <summary>
    /// Applies rotation based on WASD (pitch/yaw) and Q/E (roll) inputs.
    /// </summary>
    private void ApplyRotation()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Get rotation inputs
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

        // Calculate rotation torque
        Vector3 rotationTorque = new Vector3(
            pitch * pitchSpeed,
            yaw * yawSpeed,
            roll * rollSpeed
        );

        // Apply rotation in local space
        rb.AddRelativeTorque(rotationTorque * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Applies forward/backward thrust based on current throttle setting.
    /// </summary>
    private void ApplyThrust()
    {
        if (Mathf.Approximately(currentThrottle, 0f)) return;

        float thrustAmount = maxThrust * currentThrottle;

        // Apply boost multiplier if boosting
        if (isBoosting)
        {
            thrustAmount *= boostMultiplier;
        }

        // Apply thrust in the ship's forward direction
        Vector3 thrustForce = transform.forward * thrustAmount;
        rb.AddForce(thrustForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Applies inertial dampening to gradually reduce velocity perpendicular to thrust.
    /// This makes the ship easier to control while maintaining space-sim feel.
    /// </summary>
    private void ApplyInertialDampening()
    {
        if (!inertialDampeningEnabled) return;
        if (rb.velocity.magnitude < 0.1f) return;

        // Calculate the desired velocity direction (ship's forward when throttle > 0)
        Vector3 desiredDirection = currentThrottle > 0 ? transform.forward :
                                   currentThrottle < 0 ? -transform.forward : Vector3.zero;

        // If no throttle, dampen all velocity
        if (Mathf.Approximately(currentThrottle, 0f))
        {
            Vector3 dampeningForce = -rb.velocity.normalized * dampeningStrength;
            rb.AddForce(dampeningForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            return;
        }

        // Calculate perpendicular velocity component
        Vector3 currentVelocityDir = rb.velocity.normalized;
        Vector3 perpendicularVelocity = rb.velocity - Vector3.Project(rb.velocity, desiredDirection);

        // Apply dampening force against perpendicular velocity
        if (perpendicularVelocity.magnitude > 0.1f)
        {
            Vector3 dampeningForce = -perpendicularVelocity.normalized * dampeningStrength;
            rb.AddForce(dampeningForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// Enforces the speed limit based on whether boost is active.
    /// </summary>
    private void EnforceSpeedLimit()
    {
        float currentMaxSpeed = isBoosting ? maxBoostSpeed : maxSpeed;

        if (rb.velocity.magnitude > currentMaxSpeed)
        {
            rb.velocity = rb.velocity.normalized * currentMaxSpeed;
        }
    }

    /// <summary>
    /// Sets the throttle to a specific value (for external control/AI).
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
    /// Sets throttle to maximum forward thrust.
    /// </summary>
    public void FullThrottle()
    {
        currentThrottle = 1f;
    }

    /// <summary>
    /// Takes damage, reducing shield first then hull (for future expansion).
    /// </summary>
    public void TakeDamage(float amount)
    {
        currentShield -= amount;
        if (currentShield < 0)
        {
            currentShield = 0;
        }
    }
}
