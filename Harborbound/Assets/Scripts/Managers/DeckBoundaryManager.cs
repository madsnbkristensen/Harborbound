using UnityEngine;

public class DeckBoundaryManager : MonoBehaviour
{
    [SerializeField]
    private PlayerBoatSpriteController spriteController;
    private SpriteRenderer spriteRenderer;

    // List of edge colliders to form the boundary
    private EdgeCollider2D[] edgeColliders = new EdgeCollider2D[4]; // One for each side

    [Header("Deck Boundary Points (Local Space)")]
    [SerializeField]
    private Vector2[] rightBoundaryPoints;

    [SerializeField]
    private Vector2[] upBoundaryPoints;

    [SerializeField]
    private Vector2[] downBoundaryPoints;

    [SerializeField]
    private Vector2[] rightUpBoundaryPoints;

    [SerializeField]
    private Vector2[] leftDownBoundaryPoints;

    // Cache for performance
    private Sprite lastSprite;
    private bool lastFlipX;

    private void Awake()
    {
        // Initialize default boundary points if not set
        InitializeDefaultBoundaryPoints();
    }

    private void InitializeDefaultBoundaryPoints()
    {
        // Right Boundary Points (if not set)
        if (rightBoundaryPoints == null || rightBoundaryPoints.Length == 0)
        {
            rightBoundaryPoints = new Vector2[4]
            {
                new Vector2(-7, -4),
                new Vector2(-7, -1),
                new Vector2(2, -1),
                new Vector2(2, -4),
            };
        }

        // Up Boundary Points (if not set)
        if (upBoundaryPoints == null || upBoundaryPoints.Length == 0)
        {
            upBoundaryPoints = new Vector2[4]
            {
                new Vector2(-4, 0),
                new Vector2(4, 0),
                new Vector2(4, -4.5f),
                new Vector2(-4, -4.5f),
            };
        }

        // Down Boundary Points (if not set)
        if (downBoundaryPoints == null || downBoundaryPoints.Length == 0)
        {
            downBoundaryPoints = new Vector2[4]
            {
                new Vector2(-2, 4),
                new Vector2(-4, -2.5f),
                new Vector2(4, -2.5f),
                new Vector2(2, 4),
            };
        }

        // Right Up Boundary Points (if not set)
        if (rightUpBoundaryPoints == null || rightUpBoundaryPoints.Length == 0)
        {
            rightUpBoundaryPoints = new Vector2[4]
            {
                new Vector2(-5.5f, -3),
                new Vector2(-1, -4.5f),
                new Vector2(4.5f, -1),
                new Vector2(-1, 0),
            };
        }

        // Left Down Boundary Points (if not set)
        if (leftDownBoundaryPoints == null || leftDownBoundaryPoints.Length == 0)
        {
            leftDownBoundaryPoints = new Vector2[4]
            {
                new Vector2(1, 2),
                new Vector2(5, 1),
                new Vector2(1, -3),
                new Vector2(-3, -2),
            };
        }
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Create edge colliders
        CreateEdgeColliders();

        // Make sure boat has a Rigidbody2D for proper collision
        EnsureRigidbody();

        // Initialize with current sprite
        UpdateDeckBoundary();
    }

    private void EnsureRigidbody()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Boat moves by transform, not physics
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void CreateEdgeColliders()
    {
        // Remove any existing polygon collider
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider != null)
        {
            DestroyImmediate(polyCollider);
        }

        // Create 4 edge colliders (one for each side of the boundary)
        for (int i = 0; i < 4; i++)
        {
            GameObject edgeObj = new GameObject($"DeckEdge_{i}");
            // add tag to game object
            edgeObj.tag = "DeckEdge";
            edgeObj.transform.SetParent(transform);
            edgeObj.transform.localPosition = Vector3.zero;
            edgeObj.transform.localRotation = Quaternion.identity;
            edgeObj.transform.localScale = Vector3.one;

            EdgeCollider2D edgeCollider = edgeObj.AddComponent<EdgeCollider2D>();
            edgeCollider.isTrigger = false;

            edgeColliders[i] = edgeCollider;
        }

        Debug.Log("Created 4 edge colliders for deck boundary");
    }

    private void Update()
    {
        // Only update when sprite or flip state changes
        if (spriteRenderer.sprite != lastSprite || spriteRenderer.flipX != lastFlipX)
        {
            UpdateDeckBoundary();

            // Update cache
            lastSprite = spriteRenderer.sprite;
            lastFlipX = spriteRenderer.flipX;
        }
    }

    public EdgeCollider2D[] GetEdgeColliders()
    {
        return edgeColliders;
    }

    private void UpdateDeckBoundary()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        // Get boundary points for current sprite
        Vector2[] boundaryPoints = GetBoundaryForCurrentSprite();

        // Apply to edge colliders
        if (boundaryPoints != null && boundaryPoints.Length >= 4)
        {
            // Divide the polygon into 4 edges
            for (int i = 0; i < 4; i++)
            {
                // Get two consecutive points for this edge
                int startIdx = i;
                int endIdx = (i + 1) % boundaryPoints.Length;

                Vector2[] edgePoints = new Vector2[]
                {
                    boundaryPoints[startIdx],
                    boundaryPoints[endIdx],
                };

                // Apply points to the edge collider
                if (edgeColliders[i] != null)
                {
                    edgeColliders[i].points = edgePoints;
                }
            }
        }
    }

    private Vector2[] GetBoundaryForCurrentSprite()
    {
        // Get current sprite and flip state
        Sprite currentSprite = spriteRenderer.sprite;
        bool isFlipped = spriteRenderer.flipX;

        // Default values
        Vector2[] selectedPoints = null;
        bool needFlip = false;

        // Determine which points to use
        if (currentSprite == spriteController.spriteUp)
        {
            selectedPoints = upBoundaryPoints;
        }
        else if (currentSprite == spriteController.spriteDown)
        {
            selectedPoints = downBoundaryPoints;
        }
        else if (currentSprite == spriteController.spriteRight)
        {
            selectedPoints = rightBoundaryPoints;
            needFlip = isFlipped; // Flip for left movement
        }
        else if (currentSprite == spriteController.spriteRightUp)
        {
            selectedPoints = rightUpBoundaryPoints;
            needFlip = isFlipped; // Flip for left-up
        }
        else if (currentSprite == spriteController.spriteLeftDown)
        {
            selectedPoints = leftDownBoundaryPoints;
            needFlip = isFlipped;
        }

        // Default if no match
        if (selectedPoints == null)
        {
            Debug.LogWarning("Unknown boat sprite orientation");
            return rightBoundaryPoints; // Fallback
        }

        // Apply flipping if needed
        if (needFlip)
        {
            Vector2[] flippedPoints = new Vector2[selectedPoints.Length];
            for (int i = 0; i < selectedPoints.Length; i++)
            {
                flippedPoints[i] = new Vector2(-selectedPoints[i].x, selectedPoints[i].y);
            }
            return flippedPoints;
        }

        return selectedPoints;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Make sure default points are initialized for gizmos
            if (
                rightBoundaryPoints == null
                || rightBoundaryPoints.Length == 0
                || upBoundaryPoints == null
                || upBoundaryPoints.Length == 0
                || downBoundaryPoints == null
                || downBoundaryPoints.Length == 0
                || rightUpBoundaryPoints == null
                || rightUpBoundaryPoints.Length == 0
                || leftDownBoundaryPoints == null
                || leftDownBoundaryPoints.Length == 0
            )
            {
                InitializeDefaultBoundaryPoints();
            }

            // Edit mode visualization
            DrawEditorBoundaryPoints();
        }
        else
        {
            // Play mode - draw active edge colliders
            Gizmos.color = Color.green;

            foreach (EdgeCollider2D edge in edgeColliders)
            {
                if (edge == null)
                    continue;

                Vector2[] points = edge.points;
                if (points.Length < 2)
                    continue;

                for (int i = 0; i < points.Length - 1; i++)
                {
                    Vector2 start = transform.TransformPoint(points[i]);
                    Vector2 end = transform.TransformPoint(points[i + 1]);
                    Gizmos.DrawLine(start, end);

                    // Draw a small sphere at each endpoint
                    Gizmos.DrawSphere(start, 0.05f);
                    if (i == points.Length - 2)
                        Gizmos.DrawSphere(end, 0.05f);
                }
            }
        }
    }

    private void DrawEditorBoundaryPoints()
    {
        if (spriteController == null)
            return;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            return;

        // Determine which boundary to show
        Vector2[] pointsToDraw = null;
        Sprite currentSprite = renderer.sprite;

        if (currentSprite == spriteController.spriteUp)
            pointsToDraw = upBoundaryPoints;
        else if (currentSprite == spriteController.spriteDown)
            pointsToDraw = downBoundaryPoints;
        else if (currentSprite == spriteController.spriteRight)
            pointsToDraw = rightBoundaryPoints;
        else if (currentSprite == spriteController.spriteRightUp)
            pointsToDraw = rightUpBoundaryPoints;
        else if (currentSprite == spriteController.spriteLeftDown)
            pointsToDraw = leftDownBoundaryPoints;

        if (pointsToDraw != null)
        {
            Gizmos.color = Color.yellow;
            DrawBoundaryPoints(pointsToDraw, renderer.flipX);
        }
        else
        {
            // If no match, show all boundaries
            Gizmos.color = Color.yellow;
            DrawBoundaryPoints(rightBoundaryPoints, false);

            Gizmos.color = Color.green;
            DrawBoundaryPoints(upBoundaryPoints, false);

            Gizmos.color = Color.blue;
            DrawBoundaryPoints(downBoundaryPoints, false);

            Gizmos.color = Color.magenta;
            DrawBoundaryPoints(rightUpBoundaryPoints, false);

            Gizmos.color = Color.cyan;
            DrawBoundaryPoints(leftDownBoundaryPoints, false);
        }
    }

    private void DrawBoundaryPoints(Vector2[] points, bool flipX)
    {
        if (points == null || points.Length < 2)
            return;

        // Handle flipping
        Vector2[] pointsToUse = points;
        if (flipX)
        {
            pointsToUse = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                pointsToUse[i] = new Vector2(-points[i].x, points[i].y);
            }
        }

        // Draw lines
        for (int i = 0; i < pointsToUse.Length; i++)
        {
            Vector2 start = transform.TransformPoint(pointsToUse[i]);
            Vector2 end = transform.TransformPoint(pointsToUse[(i + 1) % pointsToUse.Length]);
            Gizmos.DrawLine(start, end);
        }

        // Draw points
        for (int i = 0; i < pointsToUse.Length; i++)
        {
            Vector3 pointPos = transform.TransformPoint(pointsToUse[i]);
            Gizmos.DrawSphere(pointPos, 0.05f);
        }
    }
}
