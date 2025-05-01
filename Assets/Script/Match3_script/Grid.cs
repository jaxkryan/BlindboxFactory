using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Grids : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        OBSTACLE,
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };

    public int xDim;
    public int yDim;
    public float fillTime;

    public MinigameLevel level;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private GamePiece[,] pieces;
    private GameObject[,] backgroundTiles; // Array to store background tiles

    private bool inverse = false;

    private GamePiece pressedPiece;
    private GamePiece enteredPiece;

    private bool gameOver = false;

    // Cache board origin and half-width for GetWorldPosition.
    private Vector2 boardOrigin;
    private float halfX;

    public TextMeshProUGUI resetText;
    [SerializeField] private float tileSize = 0.3f;

    // -------------------- Unity Lifecycle --------------------

    private void Awake()
    {
        // Initialize the piece prefab dictionary
        piecePrefabDict = new Dictionary<PieceType, GameObject>(piecePrefabs.Length);
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        // Initialize the pieces array
        pieces = new GamePiece[xDim, yDim];
        backgroundTiles = new GameObject[xDim, yDim]; // Initialize the background tiles array

        // Spawn background tiles
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnBackgroundTile(x, y);
            }
        }

        // Spawn empty pieces
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y, PieceType.EMPTY);
            }
        }

        StartCoroutine(Fill());
    }

    private void OnEnable()
    {
        // Recalculate boardOrigin and reposition pieces and background tiles when the GameObject is enabled
        CenterGridOnParent();
        RepositionPieces();
    }

    private void CenterGridOnParent()
    {
        // Calculate the total size of the grid
        float gridWidth = xDim * tileSize;
        float gridHeight = yDim * tileSize;

        // Center the grid by offsetting boardOrigin from the GameObject's position
        Vector2 parentPosition = transform.position;
        boardOrigin = new Vector2(
            parentPosition.x - (gridWidth / 2f) + (tileSize / 2f), // Center horizontally
            parentPosition.y - (gridHeight / 2f) + (tileSize / 2f)  // Center vertically
        );
        Debug.Log($"Centered grid: boardOrigin set to {boardOrigin} for parent position {parentPosition}");
    }

    private void RepositionPieces()
    {
        // Reposition all existing pieces based on the updated boardOrigin
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                // Reposition pieces
                if (pieces[x, y] != null)
                {
                    Vector2 newPos = GetWorldPosition(x, y);
                    pieces[x, y].transform.position = new Vector3(newPos.x, newPos.y, -5f);
                    Debug.Log($"Repositioned piece at ({x}, {y}) to {pieces[x, y].transform.position}");
                }

                // Reposition background tiles
                if (backgroundTiles[x, y] != null)
                {
                    Vector2 newPos = GetWorldPosition(x, y);
                    backgroundTiles[x, y].transform.position = new Vector3(newPos.x, newPos.y, 0f); // Background at z = 0 (behind pieces)
                    Debug.Log($"Repositioned background tile at ({x}, {y}) to {backgroundTiles[x, y].transform.position}");
                }
            }
        }
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(boardOrigin.x + x * tileSize, boardOrigin.y + y * tileSize);
    }

    private void SpawnBackgroundTile(int x, int y)
    {
        if (backgroundPrefab == null)
        {
            Debug.LogWarning("BackgroundPrefab is not assigned in the Inspector!");
            return;
        }

        // Instantiate the background tile and parent it to this GameObject (the Grid)
        GameObject backgroundTile = Instantiate(backgroundPrefab, transform);

        // Set the background tile's position relative to the boardOrigin
        Vector2 worldPos = GetWorldPosition(x, y);
        backgroundTile.transform.position = new Vector3(worldPos.x, worldPos.y, 0f); // z = 0 to place behind pieces

        // Store the background tile in the array
        backgroundTiles[x, y] = backgroundTile;
    }

    private GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        // Instantiate the piece and parent it to this GameObject (the Grid)
        GameObject pieceObject = Instantiate(piecePrefabDict[type], transform);

        // Set the piece's position relative to the boardOrigin
        Vector2 worldPos = GetWorldPosition(x, y);
        pieceObject.transform.position = new Vector3(worldPos.x, worldPos.y, -5f); // Set z to -5 to match Match3Panel

        GamePiece piece = pieceObject.GetComponent<GamePiece>();
        piece.Init(x, y, this, type);
        pieces[x, y] = piece;
        return piece;
    }

    // -------------------- Game Logic Methods --------------------

    public IEnumerator Fill()
    {
        Debug.Log("Filling the pool");
        while (true)
        {
            yield return new WaitForSeconds(fillTime);

            // Let pieces fall until none can move.
            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }

            Debug.Log("Clearing matches...");
            bool clearedMatches = ClearAllValidMatches();
            Debug.Log("Matches cleared!");

            if (!clearedMatches)
            {
                if (!HasValidMoves())
                {
                    Debug.Log("No valid moves available, resetting board.");
                    ResetBoard();
                    yield break; // Exit this coroutine – ResetBoard will start a new one.
                }
            }
        }
    }

    public bool FillStep()
    {
        bool movedPiece = false;

        // Loop from top to bottom
        for (int y = 1; y < yDim; y++)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = inverse ? (xDim - 1 - loopX) : loopX;
                GamePiece piece = pieces[x, y];

                if (piece.IsMoveable())
                {
                    GamePiece pieceBelow = pieces[x, y - 1];

                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MoveableComponent.Move(x, y - 1, fillTime);
                        pieces[x, y - 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                }
            }
        }

        // Spawn new pieces at the top row
        for (int x = 0; x < xDim; x++)
        {
            GamePiece topPiece = pieces[x, yDim - 1];
            if (topPiece.Type == PieceType.EMPTY)
            {
                Destroy(topPiece.gameObject);
                GameObject newPiece = Instantiate(piecePrefabDict[PieceType.NORMAL],
                                                  GetWorldPosition(x, yDim), // Spawn above the top row
                                                  Quaternion.identity,
                                                  transform);
                pieces[x, yDim - 1] = newPiece.GetComponent<GamePiece>();
                pieces[x, yDim - 1].Init(x, yDim, this, PieceType.NORMAL);
                pieces[x, yDim - 1].MoveableComponent.Move(x, yDim - 1, fillTime);
                pieces[x, yDim - 1].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, yDim - 1].ColorComponent.NumColor));
                movedPiece = true;
            }
        }

        return movedPiece;
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
               (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPiece(GamePiece piece1, GamePiece piece2)
    {
        if (gameOver) { return; }

        if (piece1.IsMoveable() && piece2.IsMoveable())
        {
            // Swap in the grid.
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            List<GamePiece> match1 = GetMatch(piece1, piece2.X, piece2.Y);
            List<GamePiece> match2 = GetMatch(piece2, piece1.X, piece1.Y);

            if (match1 != null || match2 != null)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;
                piece1.MoveableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MoveableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();

                pressedPiece = null;
                enteredPiece = null;

                StartCoroutine(Fill());
                level.OnMove();
            }
            else
            {
                // No match was found, so revert the swap.
                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;
            }
        }
    }

    public void PressedPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    public void EnteredPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if (IsAdjacent(pressedPiece, enteredPiece))
        {
            SwapPiece(pressedPiece, enteredPiece);
        }
    }

    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (!piece.IsColored())
            return null;

        ColorPiece.ColorType color = piece.ColorComponent.Color;
        List<GamePiece> horizontalPieces = new List<GamePiece>(xDim);
        List<GamePiece> verticalPieces = new List<GamePiece>(yDim);
        List<GamePiece> matchingPieces = new List<GamePiece>(Mathf.Max(xDim, yDim));

        // Check horizontal.
        horizontalPieces.Add(piece);
        for (int dir = 0; dir <= 1; dir++)
        {
            for (int xOffset = 1; xOffset < xDim; xOffset++)
            {
                int x = (dir == 0) ? newX - xOffset : newX + xOffset;
                if (x < 0 || x >= xDim)
                    break;
                GamePiece p = pieces[x, newY];
                if (p.IsColored() && p.ColorComponent.Color == color)
                {
                    horizontalPieces.Add(p);
                }
                else
                {
                    break;
                }
            }
        }
        if (horizontalPieces.Count >= 3)
        {
            matchingPieces.AddRange(horizontalPieces);
        }
        // Check for L or T shapes based on the horizontal match.
        if (horizontalPieces.Count >= 3)
        {
            for (int i = 0; i < horizontalPieces.Count; i++)
            {
                verticalPieces.Clear();
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int yOffset = 1; yOffset < yDim; yOffset++)
                    {
                        int y = (dir == 0) ? newY - yOffset : newY + yOffset;
                        if (y < 0 || y >= yDim)
                            break;
                        GamePiece p = pieces[horizontalPieces[i].X, y];
                        if (p.IsColored() && p.ColorComponent.Color == color)
                        {
                            verticalPieces.Add(p);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (verticalPieces.Count >= 2)
                {
                    matchingPieces.AddRange(verticalPieces);
                    break;
                }
            }
        }
        if (matchingPieces.Count >= 3)
        {
            Debug.Log("Got a match!");
            return matchingPieces;
        }

        horizontalPieces.Clear();
        verticalPieces.Clear();
        // Check vertical.
        verticalPieces.Add(piece);
        for (int dir = 0; dir <= 1; dir++)
        {
            for (int yOffset = 1; yOffset < yDim; yOffset++)
            {
                int y = (dir == 0) ? newY - yOffset : newY + yOffset;
                if (y < 0 || y >= yDim)
                    break;
                GamePiece p = pieces[newX, y];
                if (p.IsColored() && p.ColorComponent.Color == color)
                {
                    verticalPieces.Add(p);
                }
                else
                {
                    break;
                }
            }
        }
        if (verticalPieces.Count >= 3)
        {
            matchingPieces.AddRange(verticalPieces);
        }
        // Check for L or T shapes based on the vertical match.
        if (verticalPieces.Count >= 3)
        {
            for (int i = 0; i < verticalPieces.Count; i++)
            {
                horizontalPieces.Clear();
                for (int dir = 0; dir <= 1; dir++)
                {
                    for (int xOffset = 1; xOffset < xDim; xOffset++)
                    {
                        int x = (dir == 0) ? newX - xOffset : newX + xOffset;
                        if (x < 0 || x >= xDim)
                            break;
                        GamePiece p = pieces[x, verticalPieces[i].Y];
                        if (p.IsColored() && p.ColorComponent.Color == color)
                        {
                            horizontalPieces.Add(p);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (horizontalPieces.Count >= 2)
                {
                    matchingPieces.AddRange(horizontalPieces);
                    break;
                }
            }
        }
        if (matchingPieces.Count >= 3)
        {
            Debug.Log("Got a match!");
            return matchingPieces;
        }
        return null;
    }

    public bool ClearAllValidMatches()
    {
        Debug.Log("Clear All!");
        bool needsRefill = false;
        HashSet<GamePiece> allMatchedPieces = new HashSet<GamePiece>();

        // First, find all matches across the grid
        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);
                    if (match != null)
                    {
                        allMatchedPieces.UnionWith(match);
                    }
                }
            }
        }

        // If multiple separate matches were found, clear them all
        if (allMatchedPieces.Count > 0)
        {
            foreach (GamePiece piece in allMatchedPieces)
            {
                if (ClearPiece(piece.X, piece.Y))
                {
                    needsRefill = true;
                }
            }

            // Notify MinigameLevel if more than 3 pieces were cleared
            if (allMatchedPieces.Count > 3)
            {
                level.OnBigMatch(allMatchedPieces.Count);
            }
        }

        return needsRefill;
    }

    public bool ClearPiece(int x, int y)
    {
        Debug.Log("Clear piece at " + x + ", " + y);
        if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsBeingCleared)
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);
            return true;
        }
        return false;
    }

    private bool CheckSwapForMatch(int x1, int y1, int x2, int y2)
    {
        GamePiece p1 = pieces[x1, y1];
        GamePiece p2 = pieces[x2, y2];

        // Temporarily swap in the grid.
        pieces[x1, y1] = p2;
        pieces[x2, y2] = p1;

        bool validMove = (GetMatch(p1, x2, y2) != null || GetMatch(p2, x1, y1) != null);

        // Swap back.
        pieces[x1, y1] = p1;
        pieces[x2, y2] = p2;

        return validMove;
    }

    private bool HasValidMoves()
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y] != null && pieces[x, y].IsMoveable())
                {
                    // Check right neighbor.
                    if (x < xDim - 1 && pieces[x + 1, y] != null && pieces[x + 1, y].IsMoveable())
                    {
                        if (CheckSwapForMatch(x, y, x + 1, y))
                        {
                            return true;
                        }
                    }
                    // Check below.
                    if (y < yDim - 1 && pieces[x, y + 1] != null && pieces[x, y + 1].IsMoveable())
                    {
                        if (CheckSwapForMatch(x, y, x, y + 1))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void ResetBoard()
    {
        // Clear the existing board (pieces only, not background tiles)
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y] != null)
                {
                    Destroy(pieces[x, y].gameObject);
                }
            }
        }

        // Display reset text
        if (resetText != null)
        {
            resetText.text = "Resetting The Board";
            resetText.gameObject.SetActive(true);
            StartCoroutine(HideResetText());
        }

        // Recreate the board (pieces only)
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y, PieceType.EMPTY);
            }
        }

        StartCoroutine(Fill());
    }

    private IEnumerator HideResetText()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        resetText.gameObject.SetActive(false);
    }

    public void ResetGame()
    {
        gameOver = false;
    }

    public void GameOver()
    {
        gameOver = true;
    }
}