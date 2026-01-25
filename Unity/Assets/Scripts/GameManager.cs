using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game controller for Myer - handles spawning, scoring, levels, and game state
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Tetromino Prefabs")]
    [Tooltip("Assign all 8 tetromino prefabs: I, J, L, O, S, T, Z, X (Cross)")]
    public GameObject[] Tetrominoes;

    [Header("Spawn Settings")]
    public Transform SpawnPoint;

    [Header("UI References")]
    public Text ScoreText;
    public Text LevelText;
    public Text LinesText;
    public Text FinalScoreText;
    public GameObject GameOverPanel;
    public GameObject PausePanel;

    [Header("Next Piece Preview")]
    public Transform NextPiecePreviewPoint;

    [Header("Military Color Palette")]
    public Color[] PieceColors = new Color[]
    {
        new Color(0.29f, 0.36f, 0.14f, 1f), // I - Army Green
        new Color(0.36f, 0.25f, 0.20f, 1f), // J - Dark Brown
        new Color(0.55f, 0.45f, 0.33f, 1f), // L - Tan/Khaki
        new Color(0.42f, 0.56f, 0.14f, 1f), // O - Olive Drab
        new Color(0.33f, 0.42f, 0.18f, 1f), // S - Dark Olive
        new Color(0.24f, 0.31f, 0.12f, 1f), // T - Forest Green
        new Color(0.49f, 0.42f, 0.31f, 1f), // Z - Coyote Brown
        new Color(0.55f, 0.27f, 0.07f, 1f)  // X - Saddle Brown (Cross)
    };

    [Header("Game Settings")]
    public float BaseDropSpeed = 1f;
    public float SpeedIncreasePerLevel = 0.1f;
    public float MinDropSpeed = 0.1f;
    public int LinesPerLevel = 10;

    // Game state
    private int score = 0;
    private int level = 1;
    private int totalLines = 0;
    private int nextPieceIndex;
    private GameObject currentPiece;
    private GameObject nextPiecePreview;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize game
        IsGameOver = false;
        IsPaused = false;

        // Hide panels
        if (GameOverPanel != null)
            GameOverPanel.SetActive(false);
        if (PausePanel != null)
            PausePanel.SetActive(false);

        // Pick first next piece and spawn
        nextPieceIndex = Random.Range(0, Tetrominoes.Length);
        SpawnNextPiece();

        UpdateUI();
    }

    void Update()
    {
        // Pause toggle
        if (Input.GetKeyDown(KeyCode.P) && !IsGameOver)
        {
            TogglePause();
        }

        // Restart on game over
        if (IsGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Spawns the next piece and queues a new random piece
    /// </summary>
    public void SpawnNextPiece()
    {
        if (Tetrominoes == null || Tetrominoes.Length == 0)
        {
            Debug.LogError("No tetromino prefabs assigned to GameManager!");
            return;
        }

        // Destroy preview
        if (nextPiecePreview != null)
        {
            Destroy(nextPiecePreview);
        }

        // Spawn current piece
        int currentIndex = nextPieceIndex;
        Vector3 spawnPos = SpawnPoint != null ? SpawnPoint.position : new Vector3(GameBoard.Columns / 2, GameBoard.Rows, 0);

        currentPiece = Instantiate(Tetrominoes[currentIndex], spawnPos, Quaternion.identity);
        ApplyColor(currentPiece, currentIndex);

        // Set fall speed based on level
        Tetromino tetro = currentPiece.GetComponent<Tetromino>();
        if (tetro != null)
        {
            float speed = Mathf.Max(MinDropSpeed, BaseDropSpeed - (level - 1) * SpeedIncreasePerLevel);
            tetro.SetFallSpeed(speed);
        }

        // Queue next piece
        nextPieceIndex = Random.Range(0, Tetrominoes.Length);

        // Create preview
        if (NextPiecePreviewPoint != null)
        {
            nextPiecePreview = Instantiate(Tetrominoes[nextPieceIndex], NextPiecePreviewPoint.position, Quaternion.identity);
            ApplyColor(nextPiecePreview, nextPieceIndex);

            // Disable tetromino script on preview
            Tetromino previewTetro = nextPiecePreview.GetComponent<Tetromino>();
            if (previewTetro != null)
            {
                previewTetro.enabled = false;
            }

            // Scale down preview
            nextPiecePreview.transform.localScale = Vector3.one * 0.6f;
        }
    }

    /// <summary>
    /// Applies the military color to a piece
    /// </summary>
    void ApplyColor(GameObject piece, int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= PieceColors.Length) return;

        SpriteRenderer[] renderers = piece.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
        {
            sr.color = PieceColors[colorIndex];
        }
    }

    /// <summary>
    /// Called when lines are cleared
    /// </summary>
    public void LinesCleared(int lines)
    {
        if (lines <= 0) return;

        // Scoring: 100, 300, 500, 800 for 1, 2, 3, 4+ lines
        int[] pointsTable = { 0, 100, 300, 500, 800 };
        int pointsIndex = Mathf.Min(lines, 4);
        score += pointsTable[pointsIndex] * level;

        totalLines += lines;

        // Level up check
        int newLevel = (totalLines / LinesPerLevel) + 1;
        if (newLevel > level)
        {
            level = newLevel;
            Debug.Log($"Level Up! Now level {level}");
        }

        UpdateUI();
    }

    /// <summary>
    /// Adds points to the score (for soft/hard drops)
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    /// <summary>
    /// Updates all UI elements
    /// </summary>
    void UpdateUI()
    {
        if (ScoreText != null)
            ScoreText.text = score.ToString();
        if (LevelText != null)
            LevelText.text = level.ToString();
        if (LinesText != null)
            LinesText.text = totalLines.ToString();
    }

    /// <summary>
    /// Toggles pause state
    /// </summary>
    public void TogglePause()
    {
        IsPaused = !IsPaused;

        if (PausePanel != null)
            PausePanel.SetActive(IsPaused);

        Time.timeScale = IsPaused ? 0f : 1f;
    }

    /// <summary>
    /// Triggers game over state
    /// </summary>
    public void GameOver()
    {
        IsGameOver = true;

        if (GameOverPanel != null)
            GameOverPanel.SetActive(true);

        if (FinalScoreText != null)
            FinalScoreText.text = score.ToString();

        Debug.Log($"Game Over! Final Score: {score}");
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        GameBoard.ResetGrid();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Returns current level (for speed calculations)
    /// </summary>
    public int GetCurrentLevel()
    {
        return level;
    }
}
