using UnityEngine;

/// <summary>
/// Creates and manages a ghost/shadow piece that shows where the tetromino will land
/// </summary>
public class GhostPiece : MonoBehaviour
{
    [Header("Ghost Settings")]
    public float GhostAlpha = 0.3f;

    private GameObject ghostObject;
    private Transform pieceTransform;

    void Start()
    {
        pieceTransform = transform;
        CreateGhost();
    }

    void Update()
    {
        if (ghostObject != null)
        {
            UpdateGhostPosition();
        }
    }

    /// <summary>
    /// Creates a ghost copy of the current piece
    /// </summary>
    void CreateGhost()
    {
        // Create ghost as a copy of the piece
        ghostObject = new GameObject("Ghost");
        ghostObject.tag = "Ghost";

        // Copy all child blocks
        foreach (Transform child in pieceTransform)
        {
            if (child == pieceTransform) continue;

            GameObject ghostBlock = new GameObject("GhostBlock");
            ghostBlock.tag = "Ghost";
            ghostBlock.transform.SetParent(ghostObject.transform);
            ghostBlock.transform.localPosition = child.localPosition;
            ghostBlock.transform.localRotation = child.localRotation;
            ghostBlock.transform.localScale = child.localScale;

            // Copy sprite renderer with transparency
            SpriteRenderer originalSR = child.GetComponent<SpriteRenderer>();
            if (originalSR != null)
            {
                SpriteRenderer ghostSR = ghostBlock.AddComponent<SpriteRenderer>();
                ghostSR.sprite = originalSR.sprite;
                ghostSR.sortingOrder = originalSR.sortingOrder - 1;

                // Set transparent color
                Color ghostColor = originalSR.color;
                ghostColor.a = GhostAlpha;
                ghostSR.color = ghostColor;
            }
        }

        UpdateGhostPosition();
    }

    /// <summary>
    /// Updates the ghost position to show landing spot
    /// </summary>
    void UpdateGhostPosition()
    {
        if (ghostObject == null) return;

        // Match rotation with the piece
        ghostObject.transform.rotation = pieceTransform.rotation;

        // Start at piece position
        ghostObject.transform.position = pieceTransform.position;

        // Also need to update child positions to match piece's children
        int childIndex = 0;
        foreach (Transform child in pieceTransform)
        {
            if (child == pieceTransform) continue;
            if (childIndex < ghostObject.transform.childCount)
            {
                ghostObject.transform.GetChild(childIndex).localPosition = child.localPosition;
                ghostObject.transform.GetChild(childIndex).localRotation = child.localRotation;
            }
            childIndex++;
        }

        // Move ghost down until it collides
        while (IsValidGhostPosition(ghostObject.transform))
        {
            ghostObject.transform.position += Vector3.down;
        }

        // Move back up one step
        ghostObject.transform.position += Vector3.up;
    }

    /// <summary>
    /// Checks if the ghost position is valid (doesn't collide with board or pieces)
    /// </summary>
    bool IsValidGhostPosition(Transform ghost)
    {
        foreach (Transform block in ghost)
        {
            Vector2 pos = GameBoard.Round(block.position);

            // Check boundaries
            if (pos.x < 0 || pos.x >= GameBoard.Columns)
                return false;

            if (pos.y < 0)
                return false;

            // Check collision with grid (but not with the actual piece)
            if (pos.y < GameBoard.Rows && GameBoard.Grid[(int)pos.x, (int)pos.y] != null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Destroys the ghost object
    /// </summary>
    public void DestroyGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
        }
    }

    void OnDestroy()
    {
        DestroyGhost();
    }
}
