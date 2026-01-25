using UnityEngine;

/// <summary>
/// Manages the game board grid and handles collision detection, line clearing, and piece storage.
/// Myer - 40 columns x 20 rows military-themed Tetris variant
/// </summary>
public class GameBoard : MonoBehaviour
{
    public static int Columns = 40;
    public static int Rows = 20;
    public static Transform[,] Grid = new Transform[Columns, Rows];

    /// <summary>
    /// Checks if a position is within the board boundaries
    /// </summary>
    public static bool IsInsideBoard(Vector2 pos)
    {
        return pos.x >= 0 && pos.x < Columns && pos.y >= 0;
    }

    /// <summary>
    /// Validates if a piece can occupy its current position
    /// </summary>
    public static bool IsValidPosition(Transform piece)
    {
        foreach (Transform block in piece)
        {
            if (block.CompareTag("Ghost")) continue;

            Vector2 pos = Round(block.position);

            // Check horizontal boundaries
            if (pos.x < 0 || pos.x >= Columns)
                return false;

            // Check bottom boundary
            if (pos.y < 0)
                return false;

            // Check collision with existing blocks
            if (pos.y < Rows && Grid[(int)pos.x, (int)pos.y] != null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Rounds a position to the nearest grid cell
    /// </summary>
    public static Vector2 Round(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    /// <summary>
    /// Stores a piece's blocks in the grid after it lands
    /// </summary>
    public static void StoreInGrid(Transform piece)
    {
        foreach (Transform block in piece)
        {
            if (block == piece) continue;

            Vector2 pos = Round(block.position);
            if (pos.y >= 0 && pos.y < Rows && pos.x >= 0 && pos.x < Columns)
            {
                Grid[(int)pos.x, (int)pos.y] = block;
            }
        }
    }

    /// <summary>
    /// Checks and clears any full rows, returns number of lines cleared
    /// </summary>
    public static int ClearFullRows()
    {
        int linesCleared = 0;

        for (int y = 0; y < Rows; y++)
        {
            if (IsRowFull(y))
            {
                DeleteRow(y);
                MoveRowsDown(y);
                y--; // Recheck this row index since rows moved down
                linesCleared++;
            }
        }

        return linesCleared;
    }

    /// <summary>
    /// Checks if a row is completely filled
    /// </summary>
    static bool IsRowFull(int y)
    {
        for (int x = 0; x < Columns; x++)
        {
            if (Grid[x, y] == null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Deletes all blocks in a row
    /// </summary>
    static void DeleteRow(int y)
    {
        for (int x = 0; x < Columns; x++)
        {
            if (Grid[x, y] != null)
            {
                Destroy(Grid[x, y].gameObject);
                Grid[x, y] = null;
            }
        }
    }

    /// <summary>
    /// Moves all rows above the deleted row down by one
    /// </summary>
    static void MoveRowsDown(int deletedRow)
    {
        for (int y = deletedRow + 1; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                if (Grid[x, y] != null)
                {
                    Grid[x, y - 1] = Grid[x, y];
                    Grid[x, y] = null;
                    Grid[x, y - 1].position += Vector3.down;
                }
            }
        }
    }

    /// <summary>
    /// Resets the grid for a new game
    /// </summary>
    public static void ResetGrid()
    {
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                if (Grid[x, y] != null)
                {
                    Destroy(Grid[x, y].gameObject);
                    Grid[x, y] = null;
                }
            }
        }
    }
}
