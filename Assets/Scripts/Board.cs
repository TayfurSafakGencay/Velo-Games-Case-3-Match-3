using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    [Serializable]
    public enum PieceType
    {
        Empty,
        Normal,
        Obstacle,
        Count
    }
    
    [Serializable]
    public struct PiecePrefab
    {
        public PieceType Type;
        public GameObject Prefab;
    }
    
    [SerializeField]
    private int _height;

    [SerializeField]
    private int _width;

    [SerializeField]
    private float _fillTime;

    [SerializeField]
    private List<PiecePrefab> _piecePrefabs;

    [SerializeField]
    private GameObject _background;

    private Dictionary<PieceType, GameObject> _piecePrefabsDictionary = new();

    private GamePiece[,] pieces;

    private bool inverse;

    private GamePiece _pressedPiece;

    private GamePiece _enteredPiece;

    private void Start()
    {
        AddPrefabsToDictionary();
        
        Setup();
        SetPieces();
    }

    private void AddPrefabsToDictionary()
    {
        PiecePrefab prefab;
        for (int i = 0; i < _piecePrefabs.Count; i++)
        {
            prefab = _piecePrefabs[i];
            if (!_piecePrefabsDictionary.ContainsKey(prefab.Type))
            {
                _piecePrefabsDictionary.Add(prefab.Type, prefab.Prefab);
            }   
        }
    }

    private void Setup()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Instantiate(_background, GetWorldPosition(x, y), Quaternion.identity, transform);
            }
        }
    }

    private void SetPieces()
    {
        pieces = new GamePiece[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                SpawnNewPiece(x, y, PieceType.Empty);
            }
        }
        
        Destroy(pieces[4,4].gameObject);
        SpawnNewPiece(4, 4, PieceType.Obstacle);
        
        Destroy(pieces[3,4].gameObject);
        SpawnNewPiece(3, 4, PieceType.Obstacle);
        
        Destroy(pieces[2,4].gameObject);
        SpawnNewPiece(2, 4, PieceType.Obstacle);
        
        Destroy(pieces[5,4].gameObject);
        SpawnNewPiece(5, 4, PieceType.Obstacle);
        
        StartCoroutine(Fill());
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        Vector3 position = transform.position;
        return new Vector2(position.x - _width / 2f + x + 0.5f, position.y + _height / 2f - y - 0.5f);
    }

    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = Instantiate(_piecePrefabsDictionary[type], GetWorldPosition(x, y), quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);

        return pieces[x, y];
    }

    public IEnumerator Fill()
    {
        while (FillStep())
        {
            inverse = !inverse;
            yield return new WaitForSeconds(_fillTime);
        }
    }

    public bool FillStep()
    {
        bool movedPiece = false;

        for (int y = _height - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < _width; loopX++)
            {
                int x = loopX;
                
                if (inverse)
                {
                    x = _width - 1 - loopX;
                }
                
                GamePiece piece = pieces[x, y];

                if (!piece.IsMovable()) continue;
                GamePiece pieceBelow = pieces[x, y + 1];

                if (pieceBelow.Type == PieceType.Empty)
                {
                    Destroy(pieceBelow.gameObject);
                    piece.MovableComponent.Move(x, y + 1, _fillTime);
                    pieces[x, y + 1] = piece;
                    SpawnNewPiece(x, y, PieceType.Empty);
                    movedPiece = true;
                }
                else
                {
                    for (int diag = -1; diag <= 1; diag++)
                    {
                        if (diag != 0)
                        {
                            int diagX = x + diag;

                            if (inverse)
                            {
                                diagX = x - diag;
                            }

                            if (diagX >= 0 && diagX < _width)
                            {
                                GamePiece diagonalPiece = pieces[diagX, y + 1];

                                if (diagonalPiece.Type == PieceType.Empty)
                                {
                                    bool hasPieceAbove = true;

                                    for (int aboveY = y; aboveY >= 0; aboveY--)
                                    {
                                        GamePiece pieceAbove = pieces[diagX, aboveY];

                                        if (pieceAbove.IsMovable())
                                        {
                                            break;
                                        }
                                        else if (!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.Empty)
                                        {
                                            hasPieceAbove = false;
                                            break;
                                        }
                                    }

                                    if (!hasPieceAbove)
                                    {
                                        Destroy(diagonalPiece.gameObject);
                                        piece.MovableComponent.Move(diagX, y + 1, _fillTime);
                                        pieces[diagX, y + 1] = piece;
                                        SpawnNewPiece(x, y, PieceType.Empty);
                                        movedPiece = true;
                                        break;
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            GamePiece pieceBelow = pieces[x, 0];

            if (pieceBelow.Type != PieceType.Empty) continue;

            Destroy(pieceBelow.gameObject);
            GameObject newPiece = Instantiate(_piecePrefabsDictionary[PieceType.Normal], GetWorldPosition(x, -1), Quaternion.identity, transform);

            pieces[x, 0] = newPiece.GetComponent<GamePiece>();
            pieces[x, 0].Init(x, -1, this, PieceType.Normal);
            pieces[x, 0].MovableComponent.Move(x, 0, _fillTime);
            pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x,0].ColorComponent.ColorNumber));
            movedPiece = true;
        }

        return movedPiece;
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1)
               || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (!piece1.IsMovable() || !piece2.IsMovable()) return;
        
        pieces[piece1.X, piece1.Y] = piece2;
        pieces[piece2.X, piece2.Y] = piece1;

        int piece1X = piece1.X;
        int piece1Y = piece1.Y;
            
        piece1.MovableComponent.Move(piece2.X, piece2.Y, _fillTime);
        piece2.MovableComponent.Move(piece1X, piece1Y, _fillTime);
    }

    public void PressPiece(GamePiece piece)
    {
        _pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        _enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if (IsAdjacent(_pressedPiece, _enteredPiece))
        {
            SwapPieces(_pressedPiece, _enteredPiece);
        }
    }
}
