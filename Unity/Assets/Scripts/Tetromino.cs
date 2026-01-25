using UnityEngine;

/// <summary>
/// Controls individual tetromino piece behavior including movement, rotation, and locking
/// </summary>
public class Tetromino : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The pivot point for rotation, relative to the piece's transform")]
    public Vector3 RotationPoint = new Vector3(0.5f, 0.5f, 0f);

    [Header("Movement Settings")]
    public float FallSpeed = 1f;
    public float MoveDelay = 0.1f;
    public float SoftDropMultiplier = 10f;

    private float fallTimer = 0f;
    private float moveTimer = 0f;
    private float baseFallSpeed;

    private GhostPiece ghostPiece;

    void Start()
    {
        baseFallSpeed = FallSpeed;

        // Create ghost piece
        ghostPiece = gameObject.AddComponent<GhostPiece>();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)
            return;

        HandleInput();
        HandleFall();
    }

    void HandleInput()
    {
        // Move Left (with repeat)
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Time.time > moveTimer)
            {
                TryMove(Vector3.left);
                moveTimer = Time.time + MoveDelay;
            }
        }
        // Move Right (with repeat)
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Time.time > moveTimer)
            {
                TryMove(Vector3.right);
                moveTimer = Time.time + MoveDelay;
            }
        }

        // Rotate
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryRotate();
        }

        // Soft Drop
        if (Input.GetKey(KeyCode.DownArrow))
        {
            FallSpeed = baseFallSpeed / SoftDropMultiplier;
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(1);
        }
        else
        {
            FallSpeed = baseFallSpeed;
        }

        // Hard Drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
    }

    void HandleFall()
    {
        fallTimer += Time.deltaTime;

        if (fallTimer >= FallSpeed)
        {
            // Try to move down
            transform.position += Vector3.down;

            if (!GameBoard.IsValidPosition(transform))
            {
                // Can't move down, lock the piece
                transform.position += Vector3.up;
                LockPiece();
            }

            fallTimer = 0f;
        }
    }

    void TryMove(Vector3 direction)
    {
        transform.position += direction;

        if (!GameBoard.IsValidPosition(transform))
        {
            transform.position -= direction;
        }
    }

    void TryRotate()
    {
        Vector3 pivot = transform.position + RotationPoint;
        transform.RotateAround(pivot, Vector3.forward, 90f);

        // Wall kick attempts
        if (!GameBoard.IsValidPosition(transform))
        {
            // Try shifting left
            transform.position += Vector3.left;
            if (GameBoard.IsValidPosition(transform)) return;

            // Try shifting right
            transform.position += Vector3.right * 2;
            if (GameBoard.IsValidPosition(transform)) return;

            // Try shifting left more
            transform.position += Vector3.left * 3;
            if (GameBoard.IsValidPosition(transform)) return;

            // Try shifting right more
            transform.position += Vector3.right * 4;
            if (GameBoard.IsValidPosition(transform)) return;

            // Revert rotation
            transform.position += Vector3.left;
            transform.RotateAround(pivot, Vector3.forward, -90f);
        }
    }

    void HardDrop()
    {
        int dropDistance = 0;

        while (true)
        {
            transform.position += Vector3.down;
            if (!GameBoard.IsValidPosition(transform))
            {
                transform.position += Vector3.up;
                break;
            }
            dropDistance++;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(dropDistance * 2);

        LockPiece();
    }

    void LockPiece()
    {
        // Destroy ghost piece component
        if (ghostPiece != null)
        {
            ghostPiece.DestroyGhost();
            Destroy(ghostPiece);
        }

        // Store blocks in grid
        GameBoard.StoreInGrid(transform);

        // Check for cleared lines
        int linesCleared = GameBoard.ClearFullRows();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LinesCleared(linesCleared);

            // Check for game over (any block above visible area)
            bool gameOver = false;
            foreach (Transform block in transform)
            {
                if (block != transform && block.position.y >= GameBoard.Rows)
                {
                    gameOver = true;
                    break;
                }
            }

            if (gameOver)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                GameManager.Instance.SpawnNextPiece();
            }
        }

        // Disable this script
        enabled = false;
    }

    /// <summary>
    /// Updates the fall speed based on current level
    /// </summary>
    public void SetFallSpeed(float speed)
    {
        baseFallSpeed = speed;
        FallSpeed = speed;
    }
}
