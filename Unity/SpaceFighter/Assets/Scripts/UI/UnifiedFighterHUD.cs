using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceFighter.Core;
using SpaceFighter.Systems;

namespace SpaceFighter.UI
{
    /// <summary>
    /// Master HUD controller for the 1-man fighter.
    /// Displays all ship systems on a unified interface.
    /// Designed to scale to multi-station displays in larger ships.
    /// </summary>
    public class UnifiedFighterHUD : MonoBehaviour
    {
        [Header("Ship Reference")]
        [SerializeField] private Ship ship;

        [Header("Hull Display")]
        [SerializeField] private TextMeshProUGUI hullText;
        [SerializeField] private Slider hullBar;
        [SerializeField] private Image hullBarFill;
        [SerializeField] private TextMeshProUGUI repairText;

        [Header("Speed Display")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI maxSpeedText;

        [Header("Throttle Display")]
        [SerializeField] private TextMeshProUGUI throttleText;
        [SerializeField] private Slider throttleBar;
        [SerializeField] private Image throttleBarFill;

        [Header("Power Display")]
        [SerializeField] private TextMeshProUGUI powerPresetText;
        [SerializeField] private Slider enginePowerBar;
        [SerializeField] private Slider weaponPowerBar;
        [SerializeField] private Slider shieldPowerBar;
        [SerializeField] private TextMeshProUGUI enginePowerText;
        [SerializeField] private TextMeshProUGUI weaponPowerText;
        [SerializeField] private TextMeshProUGUI shieldPowerText;

        [Header("Orientation Display")]
        [SerializeField] private TextMeshProUGUI headingText;
        [SerializeField] private TextMeshProUGUI pitchText;
        [SerializeField] private TextMeshProUGUI rollText;

        [Header("Status Indicators")]
        [SerializeField] private GameObject boostIndicator;
        [SerializeField] private TextMeshProUGUI dampeningText;
        [SerializeField] private GameObject criticalWarning;

        [Header("Crosshair")]
        [SerializeField] private RectTransform crosshair;
        [SerializeField] private Image crosshairImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color criticalColor = new Color(1f, 0.3f, 0.2f);
        [SerializeField] private Color boostColor = new Color(0.4f, 0.6f, 1f);

        // Cached system references
        private PowerDistributionSystem powerSystem;
        private NavigationSystem navSystem;
        private HullIntegritySystem hullSystem;

        // Critical warning flash
        private float criticalFlashTimer = 0f;
        private bool criticalFlashState = false;

        private void Start()
        {
            FindShip();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (ship == null || !ship.IsInitialized) return;

            UpdateNavigationDisplay();
            UpdatePowerDisplay();
            UpdateHullDisplay();
            UpdateStatusIndicators();
            UpdateCriticalWarning();
        }

        /// <summary>
        /// Finds the ship reference if not set.
        /// </summary>
        private void FindShip()
        {
            if (ship == null)
            {
                ship = FindFirstObjectByType<Ship>();
            }

            if (ship != null)
            {
                powerSystem = ship.PowerSystem;
                navSystem = ship.NavigationSystem;
                hullSystem = ship.HullSystem;
            }
        }

        /// <summary>
        /// Subscribes to ship system events.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (hullSystem != null)
            {
                hullSystem.OnCriticalHull += OnCriticalHull;
                hullSystem.OnHullRestored += OnHullRestored;
            }

            if (powerSystem != null)
            {
                powerSystem.OnPresetChanged += OnPowerPresetChanged;
            }
        }

        /// <summary>
        /// Unsubscribes from events.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (hullSystem != null)
            {
                hullSystem.OnCriticalHull -= OnCriticalHull;
                hullSystem.OnHullRestored -= OnHullRestored;
            }

            if (powerSystem != null)
            {
                powerSystem.OnPresetChanged -= OnPowerPresetChanged;
            }
        }

        /// <summary>
        /// Updates speed, throttle, and orientation display.
        /// </summary>
        private void UpdateNavigationDisplay()
        {
            if (navSystem == null) return;

            // Speed
            if (speedText != null)
            {
                speedText.text = $"{navSystem.CurrentSpeed:F0} m/s";
            }

            if (maxSpeedText != null)
            {
                maxSpeedText.text = $"/ {navSystem.MaxSpeed:F0}";
            }

            // Throttle
            float throttle = navSystem.CurrentThrottle;
            if (throttleText != null)
            {
                throttleText.text = $"THR: {throttle * 100f:F0}%";
            }

            if (throttleBar != null)
            {
                throttleBar.value = Mathf.Abs(throttle);
            }

            if (throttleBarFill != null)
            {
                if (navSystem.IsBoosting)
                {
                    throttleBarFill.color = boostColor;
                }
                else if (throttle < 0)
                {
                    throttleBarFill.color = warningColor;
                }
                else
                {
                    throttleBarFill.color = normalColor;
                }
            }

            // Orientation
            if (ship != null)
            {
                Vector3 euler = ship.transform.eulerAngles;
                float heading = NormalizeAngle(euler.y);
                float pitch = NormalizeAngle(euler.x);
                float roll = NormalizeAngle(euler.z);

                if (headingText != null) headingText.text = $"HDG {heading:F0}°";
                if (pitchText != null) pitchText.text = $"PIT {pitch:F0}°";
                if (rollText != null) rollText.text = $"ROL {roll:F0}°";
            }
        }

        /// <summary>
        /// Updates power distribution display.
        /// </summary>
        private void UpdatePowerDisplay()
        {
            if (powerSystem == null) return;

            // Preset text
            if (powerPresetText != null)
            {
                string presetName = powerSystem.IsManualMode ? "MANUAL" :
                                   powerSystem.ActivePreset.ToString().ToUpper();
                powerPresetText.text = $"PWR: [{presetName}]";
            }

            // Engine power
            float enginePower = powerSystem.GetPowerLevel(PowerCategory.Engines);
            if (enginePowerBar != null) enginePowerBar.value = enginePower / 100f;
            if (enginePowerText != null) enginePowerText.text = $"E:{enginePower:F0}%";

            // Weapon power
            float weaponPower = powerSystem.GetPowerLevel(PowerCategory.Weapons);
            if (weaponPowerBar != null) weaponPowerBar.value = weaponPower / 100f;
            if (weaponPowerText != null) weaponPowerText.text = $"W:{weaponPower:F0}%";

            // Shield power
            float shieldPower = powerSystem.GetPowerLevel(PowerCategory.Shields);
            if (shieldPowerBar != null) shieldPowerBar.value = shieldPower / 100f;
            if (shieldPowerText != null) shieldPowerText.text = $"S:{shieldPower:F0}%";
        }

        /// <summary>
        /// Updates hull integrity display.
        /// </summary>
        private void UpdateHullDisplay()
        {
            if (hullSystem == null) return;

            float hullPercent = hullSystem.HullPercentage;

            // Hull text
            if (hullText != null)
            {
                hullText.text = $"HULL: {hullPercent:F0}%";

                if (hullPercent <= 25f)
                    hullText.color = criticalColor;
                else if (hullPercent <= 50f)
                    hullText.color = warningColor;
                else
                    hullText.color = normalColor;
            }

            // Hull bar
            if (hullBar != null)
            {
                hullBar.value = hullPercent / 100f;
            }

            if (hullBarFill != null)
            {
                if (hullPercent <= 25f)
                    hullBarFill.color = criticalColor;
                else if (hullPercent <= 50f)
                    hullBarFill.color = warningColor;
                else
                    hullBarFill.color = normalColor;
            }

            // Repair text
            if (repairText != null)
            {
                if (hullSystem.RepairEnabled && hullPercent < 100f)
                {
                    repairText.text = $"REPAIR: +{hullSystem.RepairRate:F1}/s";
                    repairText.gameObject.SetActive(true);
                }
                else
                {
                    repairText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Updates boost and dampening indicators.
        /// </summary>
        private void UpdateStatusIndicators()
        {
            if (navSystem == null) return;

            // Boost indicator
            if (boostIndicator != null)
            {
                boostIndicator.SetActive(navSystem.IsBoosting);
            }

            // Dampening text
            if (dampeningText != null)
            {
                dampeningText.text = navSystem.InertialDampeningEnabled ? "DAMP: ON" : "DAMP: OFF";
                dampeningText.color = navSystem.InertialDampeningEnabled ? normalColor : warningColor;
            }
        }

        /// <summary>
        /// Updates critical hull warning with flashing effect.
        /// </summary>
        private void UpdateCriticalWarning()
        {
            if (hullSystem == null || criticalWarning == null) return;

            if (hullSystem.IsCritical)
            {
                criticalFlashTimer += Time.deltaTime;
                if (criticalFlashTimer >= 0.5f)
                {
                    criticalFlashTimer = 0f;
                    criticalFlashState = !criticalFlashState;
                    criticalWarning.SetActive(criticalFlashState);
                }
            }
            else
            {
                criticalWarning.SetActive(false);
                criticalFlashTimer = 0f;
            }
        }

        /// <summary>
        /// Normalizes angle to -180 to 180 range.
        /// </summary>
        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        // Event handlers
        private void OnCriticalHull()
        {
            if (criticalWarning != null)
            {
                criticalWarning.SetActive(true);
            }
        }

        private void OnHullRestored()
        {
            if (criticalWarning != null)
            {
                criticalWarning.SetActive(false);
            }
        }

        private void OnPowerPresetChanged(PowerPreset preset)
        {
            // Could add visual feedback here (flash, sound, etc.)
        }

        /// <summary>
        /// Sets the ship reference manually.
        /// </summary>
        public void SetShip(Ship newShip)
        {
            UnsubscribeFromEvents();
            ship = newShip;
            FindShip();
            SubscribeToEvents();
        }
    }
}
