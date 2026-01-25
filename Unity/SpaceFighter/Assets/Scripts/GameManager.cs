using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main game manager for the Space Fighter prototype.
/// Handles game state, pausing, and coordinates between systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipController playerShip;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private StarfieldGenerator starfield;

    [Header("Game State")]
    [SerializeField] private bool isPaused = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject controlsPanel;

    // Singleton instance
    public static GameManager Instance { get; private set; }

    public bool IsPaused => isPaused;
    public ShipController PlayerShip => playerShip;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find references if not set
        FindReferences();
    }

    private void Start()
    {
        // Ensure cursor is locked for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide UI panels
        if (pausePanel != null) pausePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    private void Update()
    {
        HandlePauseInput();
        HandleControlsDisplay();
    }

    /// <summary>
    /// Finds component references if not set in inspector.
    /// </summary>
    private void FindReferences()
    {
        if (playerShip == null)
        {
            playerShip = FindFirstObjectByType<ShipController>();
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<HUDController>();
        }

        if (starfield == null)
        {
            starfield = FindAnyObjectByType<StarfieldGenerator>();
        }
    }

    /// <summary>
    /// Handles pause toggle with Escape key.
    /// </summary>
    private void HandlePauseInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Handles controls display toggle with H key.
    /// </summary>
    private void HandleControlsDisplay()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.hKey.wasPressedThisFrame)
        {
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(!controlsPanel.activeSelf);
            }
        }
    }

    /// <summary>
    /// Toggles the pause state.
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Quits to main menu (placeholder for future expansion).
    /// </summary>
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        // SceneManager.LoadScene("MainMenu");
        Debug.Log("Quit to menu - implement when main menu exists");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
