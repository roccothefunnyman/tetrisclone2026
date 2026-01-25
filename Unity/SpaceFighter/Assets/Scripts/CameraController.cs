using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles camera positioning and view switching for the space fighter.
/// Supports cockpit (first-person) and chase (third-person) camera modes.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private ShipController shipController;

    [Header("Camera Modes")]
    [SerializeField] private CameraMode currentMode = CameraMode.Chase;

    [Header("Cockpit View Settings")]
    [SerializeField] private Vector3 cockpitOffset = new Vector3(0f, 0.5f, 1f);
    [SerializeField] private float cockpitFOV = 75f;

    [Header("Chase View Settings")]
    [SerializeField] private Vector3 chaseOffset = new Vector3(0f, 3f, -10f);
    [SerializeField] private float chaseFOV = 60f;
    [SerializeField] private float chaseFollowSpeed = 8f;
    [SerializeField] private float chaseRotationSpeed = 5f;

    [Header("Dynamic Camera Effects")]
    [SerializeField] private bool enableSpeedEffects = true;
    [SerializeField] private float maxFOVBoost = 15f;
    [SerializeField] private float fovTransitionSpeed = 3f;

    [Header("Camera Shake")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float boostShakeIntensity = 0.1f;

    public enum CameraMode
    {
        Cockpit,
        Chase
    }

    private Camera cam;
    private float baseFOV;
    private float targetFOV;
    private Vector3 shakeOffset;

    public CameraMode CurrentMode => currentMode;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            ShipController ship = FindFirstObjectByType<ShipController>();
            if (ship != null)
            {
                target = ship.transform;
                shipController = ship;
            }
        }

        if (shipController == null && target != null)
        {
            shipController = target.GetComponent<ShipController>();
        }

        ApplyCameraMode();
    }

    private void Update()
    {
        HandleViewSwitch();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        switch (currentMode)
        {
            case CameraMode.Cockpit:
                UpdateCockpitCamera();
                break;
            case CameraMode.Chase:
                UpdateChaseCamera();
                break;
        }

        UpdateDynamicEffects();
        ApplyCameraShake();
    }

    /// <summary>
    /// Handles camera view switching with Tab key.
    /// </summary>
    private void HandleViewSwitch()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.tabKey.wasPressedThisFrame)
        {
            CycleViewMode();
        }

        // Direct view mode keys
        if (kb.f1Key.wasPressedThisFrame)
        {
            SetCameraMode(CameraMode.Cockpit);
        }
        else if (kb.f2Key.wasPressedThisFrame)
        {
            SetCameraMode(CameraMode.Chase);
        }
    }

    /// <summary>
    /// Cycles to the next camera mode.
    /// </summary>
    public void CycleViewMode()
    {
        int nextMode = ((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length;
        SetCameraMode((CameraMode)nextMode);
    }

    /// <summary>
    /// Sets the camera to a specific mode.
    /// </summary>
    public void SetCameraMode(CameraMode mode)
    {
        currentMode = mode;
        ApplyCameraMode();
    }

    /// <summary>
    /// Applies settings for the current camera mode.
    /// </summary>
    private void ApplyCameraMode()
    {
        switch (currentMode)
        {
            case CameraMode.Cockpit:
                baseFOV = cockpitFOV;
                break;
            case CameraMode.Chase:
                baseFOV = chaseFOV;
                break;
        }

        targetFOV = baseFOV;
    }

    /// <summary>
    /// Updates cockpit (first-person) camera - locked to ship position and rotation.
    /// </summary>
    private void UpdateCockpitCamera()
    {
        // Position camera at cockpit offset in ship's local space
        transform.position = target.TransformPoint(cockpitOffset);
        transform.rotation = target.rotation;
    }

    /// <summary>
    /// Updates chase (third-person) camera with smooth following.
    /// </summary>
    private void UpdateChaseCamera()
    {
        // Calculate desired position in world space
        Vector3 desiredPosition = target.TransformPoint(chaseOffset);

        // Smoothly move to desired position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            chaseFollowSpeed * Time.deltaTime
        );

        // Smoothly rotate to look at target
        Quaternion desiredRotation = Quaternion.LookRotation(
            target.position - transform.position,
            target.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            chaseRotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Updates dynamic camera effects like FOV changes based on speed.
    /// </summary>
    private void UpdateDynamicEffects()
    {
        if (cam == null || shipController == null) return;

        if (enableSpeedEffects)
        {
            // Increase FOV based on speed percentage
            float speedPercent = shipController.CurrentSpeed / shipController.MaxSpeed;
            float fovBoost = speedPercent * maxFOVBoost;

            // Extra FOV boost when boosting
            if (shipController.IsBoosting)
            {
                fovBoost *= 1.5f;
            }

            targetFOV = baseFOV + fovBoost;
        }
        else
        {
            targetFOV = baseFOV;
        }

        // Smoothly transition FOV
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Applies camera shake effect during boost or impacts.
    /// </summary>
    private void ApplyCameraShake()
    {
        if (!enableCameraShake || shipController == null) return;

        if (shipController.IsBoosting)
        {
            // Generate shake offset
            shakeOffset = new Vector3(
                Random.Range(-boostShakeIntensity, boostShakeIntensity),
                Random.Range(-boostShakeIntensity, boostShakeIntensity),
                0f
            );

            transform.position += transform.TransformDirection(shakeOffset);
        }
    }

    /// <summary>
    /// Triggers a camera shake effect (for impacts, explosions, etc.).
    /// </summary>
    public void TriggerShake(float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float currentIntensity = Mathf.Lerp(intensity, 0f, elapsed / duration);

            shakeOffset = new Vector3(
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity)
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }

    /// <summary>
    /// Sets the target for the camera to follow.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        shipController = newTarget?.GetComponent<ShipController>();
    }
}
