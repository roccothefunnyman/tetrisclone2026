using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the HUD display, updating UI elements with ship status data.
/// Provides real-time feedback on speed, throttle, shields, and orientation.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Ship Reference")]
    [SerializeField] private ShipController ship;

    [Header("Speed Display")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI maxSpeedText;
    [SerializeField] private Slider speedBar;

    [Header("Throttle Display")]
    [SerializeField] private TextMeshProUGUI throttleText;
    [SerializeField] private Slider throttleBar;
    [SerializeField] private Image throttleBarFill;
    [SerializeField] private Color normalThrottleColor = new Color(0.2f, 0.8f, 0.4f);
    [SerializeField] private Color reverseThrottleColor = new Color(0.8f, 0.4f, 0.2f);
    [SerializeField] private Color boostThrottleColor = new Color(0.4f, 0.6f, 1f);

    [Header("Shield Display")]
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private Slider shieldBar;
    [SerializeField] private Image shieldBarFill;

    [Header("Power Distribution")]
    [SerializeField] private Slider enginePowerBar;
    [SerializeField] private Slider shieldPowerBar;
    [SerializeField] private Slider weaponPowerBar;

    [Header("Orientation Display")]
    [SerializeField] private TextMeshProUGUI headingText;
    [SerializeField] private TextMeshProUGUI pitchText;
    [SerializeField] private TextMeshProUGUI rollText;
    [SerializeField] private RectTransform artificialHorizon;

    [Header("Status Indicators")]
    [SerializeField] private GameObject boostIndicator;
    [SerializeField] private GameObject dampeningIndicator;
    [SerializeField] private TextMeshProUGUI dampeningText;

    [Header("Crosshair")]
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color normalCrosshairColor = new Color(0.2f, 1f, 0.4f, 0.8f);
    [SerializeField] private Color targetingCrosshairColor = new Color(1f, 0.4f, 0.2f, 1f);

    [Header("Velocity Vector Indicator")]
    [SerializeField] private RectTransform velocityMarker;
    [SerializeField] private Camera mainCamera;

    private void Update()
    {
        if (ship == null) return;

        UpdateSpeedDisplay();
        UpdateThrottleDisplay();
        UpdateShieldDisplay();
        UpdatePowerDistribution();
        UpdateOrientationDisplay();
        UpdateStatusIndicators();
        UpdateVelocityMarker();
    }

    private void UpdateSpeedDisplay()
    {
        float speed = ship.CurrentSpeed;
        float maxSpeed = ship.MaxSpeed;

        if (speedText != null)
        {
            speedText.text = $"{speed:F0} m/s";
        }

        if (maxSpeedText != null)
        {
            maxSpeedText.text = $"/ {maxSpeed:F0}";
        }

        if (speedBar != null)
        {
            speedBar.value = speed / maxSpeed;
        }
    }

    private void UpdateThrottleDisplay()
    {
        float throttle = ship.CurrentThrottle;

        if (throttleText != null)
        {
            throttleText.text = $"{throttle * 100f:F0}%";
        }

        if (throttleBar != null)
        {
            // Throttle bar shows absolute value, color indicates direction
            throttleBar.value = Mathf.Abs(throttle);
        }

        if (throttleBarFill != null)
        {
            if (ship.IsBoosting)
            {
                throttleBarFill.color = boostThrottleColor;
            }
            else if (throttle < 0)
            {
                throttleBarFill.color = reverseThrottleColor;
            }
            else
            {
                throttleBarFill.color = normalThrottleColor;
            }
        }
    }

    private void UpdateShieldDisplay()
    {
        float shieldPercent = ship.ShieldPercent;

        if (shieldText != null)
        {
            shieldText.text = $"{shieldPercent:F0}%";
        }

        if (shieldBar != null)
        {
            shieldBar.value = shieldPercent / 100f;
        }

        // Change shield bar color based on status
        if (shieldBarFill != null)
        {
            if (shieldPercent > 60f)
            {
                shieldBarFill.color = new Color(0.2f, 0.6f, 1f);
            }
            else if (shieldPercent > 30f)
            {
                shieldBarFill.color = new Color(1f, 0.8f, 0.2f);
            }
            else
            {
                shieldBarFill.color = new Color(1f, 0.3f, 0.2f);
            }
        }
    }

    private void UpdatePowerDistribution()
    {
        if (enginePowerBar != null)
        {
            enginePowerBar.value = ship.EnginePower;
        }

        if (shieldPowerBar != null)
        {
            shieldPowerBar.value = ship.ShieldPower;
        }

        if (weaponPowerBar != null)
        {
            weaponPowerBar.value = ship.WeaponPower;
        }
    }

    private void UpdateOrientationDisplay()
    {
        if (ship == null) return;

        Vector3 euler = ship.transform.eulerAngles;

        // Normalize angles to -180 to 180 range
        float heading = NormalizeAngle(euler.y);
        float pitch = NormalizeAngle(euler.x);
        float roll = NormalizeAngle(euler.z);

        if (headingText != null)
        {
            headingText.text = $"HDG {heading:F0}°";
        }

        if (pitchText != null)
        {
            pitchText.text = $"PIT {pitch:F0}°";
        }

        if (rollText != null)
        {
            rollText.text = $"ROL {roll:F0}°";
        }

        // Update artificial horizon rotation
        if (artificialHorizon != null)
        {
            artificialHorizon.localRotation = Quaternion.Euler(0, 0, roll);
        }
    }

    private void UpdateStatusIndicators()
    {
        if (boostIndicator != null)
        {
            boostIndicator.SetActive(ship.IsBoosting);
        }

        if (dampeningIndicator != null)
        {
            dampeningIndicator.SetActive(ship.InertialDampeningEnabled);
        }

        if (dampeningText != null)
        {
            dampeningText.text = ship.InertialDampeningEnabled ? "DAMP ON" : "DAMP OFF";
            dampeningText.color = ship.InertialDampeningEnabled ?
                new Color(0.2f, 1f, 0.4f) : new Color(0.6f, 0.6f, 0.6f);
        }
    }

    /// <summary>
    /// Updates the velocity vector marker (prograde marker) on the HUD.
    /// Shows where the ship is actually moving versus where it's pointing.
    /// </summary>
    private void UpdateVelocityMarker()
    {
        if (velocityMarker == null || mainCamera == null) return;

        Vector3 velocity = ship.Velocity;

        if (velocity.magnitude < 1f)
        {
            velocityMarker.gameObject.SetActive(false);
            return;
        }

        velocityMarker.gameObject.SetActive(true);

        // Project velocity direction onto screen
        Vector3 velocityWorldPos = ship.transform.position + velocity.normalized * 100f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(velocityWorldPos);

        // Check if velocity marker is in front of camera
        if (screenPos.z < 0)
        {
            velocityMarker.gameObject.SetActive(false);
            return;
        }

        // Convert to canvas position
        velocityMarker.position = screenPos;
    }

    /// <summary>
    /// Normalizes an angle to -180 to 180 range.
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    /// <summary>
    /// Sets the targeting color on the crosshair (for future targeting system).
    /// </summary>
    public void SetTargetingMode(bool isTargeting)
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = isTargeting ? targetingCrosshairColor : normalCrosshairColor;
        }
    }

    /// <summary>
    /// Finds the ship reference if not set.
    /// </summary>
    private void Start()
    {
        if (ship == null)
        {
            ship = FindFirstObjectByType<ShipController>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
}
