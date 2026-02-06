using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Piece prefabPiece;
    public Tilemap tilemap;
    public TetronimoData[] tetronimos;
    public Vector2Int boardSize;
    public Vector2Int startPosition;

    public float dropInterval = 0.5f;

    float dropTime = 0.0f;
    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();
    Piece activePiece;
    
    private List<Tetronimo> tetronimoBag = new List<Tetronimo>();
    int left
    { get { return -boardSize.x / 2; } }
    int right
    { get { return boardSize.x / 2; } }
    int top 
    { get { return boardSize.y / 2; } }
    int bottom 
    { get { return -boardSize.y / 2; } }

    private void Start()
    {
        SpawnPiece();
    }

    private void Update()
    {
        if (tetrisManager.gameOver) return;
        dropTime += Time.deltaTime;

        if (dropTime >= dropInterval)
        {
            dropTime = 0.0f;
            Clear(activePiece);
            bool moveResult = activePiece.Move(Vector2Int.down);
            Set(activePiece);

            if (!moveResult)
            {
                activePiece.freeze = true;
                CheckBoard();
                SpawnPiece();
            }
        }
    }


    //This ensures that each piece comes out randomly once in each row aa if it were being drawn from a bag
    void RefillBag()
    {
        tetronimoBag.Clear(); //old bag clearing
        foreach (Tetronimo t in System.Enum.GetValues(typeof(Tetronimo)))
        {
            tetronimoBag.Add(t); // all pieces are added
        }
        

        //mixes randomly
        for (int i = 0; i < tetronimoBag.Count; i++)
        {
            Tetronimo temp = tetronimoBag[i];
            int randomIndex = Random.Range(i, tetronimoBag.Count);
            tetronimoBag[i] = tetronimoBag[randomIndex];
            tetronimoBag[randomIndex] = temp;
        }
    }

    public void SpawnPiece()
    {
        activePiece = Instantiate(prefabPiece);

        // if the bag is empty for refill it
        if (tetronimoBag.Count == 0)
        {
            RefillBag();
        }

        //takes the next item from the bag and crosses it off the list
        Tetronimo t = tetronimoBag[0];
        tetronimoBag.RemoveAt(0);

        activePiece.Initialize(this, t);

        CheckEndGame();

        Set(activePiece);
    }

    void CheckEndGame()
    {
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }

    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);

        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        activePiece = null;
        tilemap.ClearAllTiles();
        pieces.Clear();

        tetronimoBag.Clear(); 
        SpawnPiece();
    }

    void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);
            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);
            pieces[cellPosition] = piece;
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top) return false;

            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }


    //F piece cannot be cleaned alone
    bool IsLineFull(int y)
    {
        bool hasNonFBlock = false; 

        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);

            if (!tilemap.HasTile(cellPosition)) return false;

            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];
                if (piece.data.tetronimo != Tetronimo.F)
                {
                    hasNonFBlock = true; // f piece found in line
                }
            }
        }
        //if there are only F pieces the line is not considered full
        return hasNonFBlock;
    }

    void DestroyLine(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];
                piece.ReduceActiveCount();
                SetTile(cellPosition, null);
            }
        }
    }

    void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                if (pieces.ContainsKey(cellPosition))
                {
                    Piece currentPiece = pieces[cellPosition];

                    SetTile(cellPosition, null);

                    cellPosition.y -= 1;
                    SetTile(cellPosition, currentPiece);
                }
            }
        }
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();

        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }

        if (destroyedLines.Count > 0)
        {
            tetrisManager.ChangeScoreWithPiece(destroyedLines.Count, activePiece.data.tetronimo);
        }
    }
}