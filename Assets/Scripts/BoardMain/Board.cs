using System;
using System.Collections;
using System.Collections.Generic;
using Levels.Main;
using Piece;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoardMain
{
  public class Board : MonoBehaviour
  {
    [Serializable]
    public enum PieceType
    {
      Empty,
      Normal,
      Obstacle,
      RowClear,
      ColumnClear,
      Rainbow,
      Count
    }

    [Serializable]
    public struct PiecePrefab
    {
      public PieceType Type;
      public GameObject Prefab;
    }
  
    [Serializable]
    public struct PiecePosition
    {
      public PieceType type;
      public int x;
      public int y;
    }

    public Level Level;

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

    public List<PiecePosition> initialPieces;

    private Dictionary<PieceType, GameObject> _piecePrefabsDictionary = new();

    private GamePiece[,] pieces;

    private bool inverse;

    private GamePiece _pressedPiece;

    private GamePiece _enteredPiece;

    private bool _gameOver;

    private bool _isFilling;

    public bool IsFilling => _isFilling;

    private void Awake()
    {
      AddPrefabsToDictionary();

      Setup();
      SetPieces();
    }

    private void AddPrefabsToDictionary()
    {
      for (int i = 0; i < _piecePrefabs.Count; i++)
      {
        PiecePrefab prefab = _piecePrefabs[i];
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

      for (int i = 0; i < initialPieces.Count; i++)
      {
        if (initialPieces[i].x >= 0 && initialPieces[i].x < _width
                                    && initialPieces[i].y >= 0 && initialPieces[i].y < _height)
        {
          SpawnNewPiece(initialPieces[i].x, initialPieces[i].y, initialPieces[i].type);
        }
      }
    
      for (int x = 0; x < _width; x++)
      {
        for (int y = 0; y < _height; y++)
        {
          if (pieces[x, y] == null)
          {
            SpawnNewPiece(x, y, PieceType.Empty);
          }
        }
      }

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
      bool needsRefill = true;
      _isFilling = true;
    
      while (needsRefill)
      {
        yield return new WaitForSeconds(_fillTime);

        while (FillStep())
        {
          inverse = !inverse;
          yield return new WaitForSeconds(_fillTime);
        }

        needsRefill = ClearAllValidMatches();
      }

      _isFilling = false;
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
              if (diag == 0) continue;
            
              int diagX = x + diag;

              if (inverse)
              {
                diagX = x - diag;
              }

              if (diagX < 0 || diagX >= _width) continue;
              GamePiece diagonalPiece = pieces[diagX, y + 1];

              if (diagonalPiece.Type != PieceType.Empty) continue;
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

              if (hasPieceAbove) continue;
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

      for (int x = 0; x < _width; x++)
      {
        GamePiece pieceBelow = pieces[x, 0];

        if (pieceBelow.Type != PieceType.Empty) continue;

        Destroy(pieceBelow.gameObject);
        GameObject newPiece = Instantiate(_piecePrefabsDictionary[PieceType.Normal], GetWorldPosition(x, -1), Quaternion.identity, transform);

        pieces[x, 0] = newPiece.GetComponent<GamePiece>();
        pieces[x, 0].Init(x, -1, this, PieceType.Normal);
        pieces[x, 0].MovableComponent.Move(x, 0, _fillTime);
        pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.ColorNumber));
        movedPiece = true;
      }

      return movedPiece;
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
      return (piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1)
             || (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
      if (_gameOver) return;
    
      if (!piece1.IsMovable() || !piece2.IsMovable()) return;

      pieces[piece1.X, piece1.Y] = piece2;
      pieces[piece2.X, piece2.Y] = piece1;

      if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null
                                                       || piece1.Type == PieceType.Rainbow || piece2.Type == PieceType.Rainbow)
      {
        int piece1X = piece1.X;
        int piece1Y = piece1.Y;

        piece1.MovableComponent.Move(piece2.X, piece2.Y, _fillTime);
        piece2.MovableComponent.Move(piece1X, piece1Y, _fillTime);

        if (piece1.Type == PieceType.Rainbow && piece1.IsClearable() && piece2.IsColored())
        {
          RainbowPiece rainbowPiece = piece1.GetComponent<RainbowPiece>();

          if (rainbowPiece)
          {
            rainbowPiece.Color = piece2.ColorComponent.Color;
          }

          ClearPiece(piece1.X, piece1.Y);
        }
        else if (piece2.Type == PieceType.Rainbow && piece2.IsClearable() && piece1.IsColored())
        {
          RainbowPiece rainbowPiece = piece2.GetComponent<RainbowPiece>();

          if (rainbowPiece)
          {
            rainbowPiece.Color = piece1.ColorComponent.Color;
          }

          ClearPiece(piece2.X, piece2.Y);
        }

        ClearAllValidMatches();

        if (piece1.Type == PieceType.RowClear || piece1.Type == PieceType.ColumnClear)
        {
          ClearPiece(piece1.X, piece1.Y);
        }
        if (piece2.Type == PieceType.RowClear || piece2.Type == PieceType.ColumnClear)
        {
          ClearPiece(piece2.X, piece2.Y);
        }

        _enteredPiece = null;
        _pressedPiece = null;

        StartCoroutine(Fill());
      
        Level.OnMove();
      }
      else
      {
        pieces[piece1.X, piece1.Y] = piece1;
        pieces[piece2.X, piece2.Y] = piece2;
      }
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

    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
      if (!piece.IsColored()) return null;

      List<GamePiece> matchingPieces = new();
      List<GamePiece> horizontalPieces = new();
      List<GamePiece> verticalPieces = new();

      horizontalPieces.Add(piece);

      ColorPiece.ColorType color = piece.ColorComponent.Color;

      for (int dir = 0; dir <= 1; dir++)
      {
        for (int xOffset = 1; xOffset < _width; xOffset++)
        {
          int x;
          if (dir == 0) // Left
            x = newX - xOffset;
          else // Right
            x = newX + xOffset;

          if (x < 0 || x >= _width)
            break;

          if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color)
          {
            horizontalPieces.Add(pieces[x, newY]);
          }
          else
          {
            break;
          }
        }
      }

      if (horizontalPieces.Count >= 3)
      {
        for (int i = 0; i < horizontalPieces.Count; i++)
        {
          matchingPieces.Add(horizontalPieces[i]);
        }
      }

      // // For L and T shape.
      if (horizontalPieces.Count >= 3)
      {
        for (int i = 0; i < horizontalPieces.Count; i++)
        {
          for (int dir = 0; dir <= 1; dir++)
          {
            for (int yOffset = 1; yOffset < _height; yOffset++)
            {
              int y;

              if (dir == 0) // Down
                y = newY - yOffset;
              else // Up
                y = newY + yOffset;

              if (y < 0 || y >= _height)
                break;

              if (pieces[horizontalPieces[i].X, y].IsColored() && pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
              {
                verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
              }
              else break;
            }
          }

          if (verticalPieces.Count < 2)
          {
            verticalPieces.Clear();
          }
          else
          {
            for (int j = 0; j < verticalPieces.Count; j++)
            {
              matchingPieces.Add(verticalPieces[j]);
            }

            break;
          }
        }
      }

      if (matchingPieces.Count >= 3)
      {
        return matchingPieces;
      }

      // Vertical
      horizontalPieces.Clear();
      verticalPieces.Clear();
      verticalPieces.Add(piece);

      for (int dir = 0; dir <= 1; dir++)
      {
        for (int yOffset = 1; yOffset < _height; yOffset++)
        {
          int y;
          if (dir == 0) // Up
            y = newY - yOffset;
          else // Down
            y = newY + yOffset;

          if (y < 0 || y >= _height)
            break;

          if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color)
          {
            verticalPieces.Add(pieces[newX, y]);
          }
          else
          {
            break;
          }
        }
      }

      if (verticalPieces.Count >= 3)
      {
        for (int i = 0; i < verticalPieces.Count; i++)
        {
          matchingPieces.Add(verticalPieces[i]);
        }
      }

      // For L and T shape.
      if (verticalPieces.Count >= 3)
      {
        for (int i = 0; i < verticalPieces.Count; i++)
        {
          for (int dir = 0; dir <= 1; dir++)
          {
            for (int xOffset = 1; xOffset < _width; xOffset++)
            {
              int x;

              if (dir == 0) // Left
                x = newX - xOffset;
              else // Right
                x = newX + xOffset;

              if (x < 0 || x >= _width)
                break;

              if (pieces[x, verticalPieces[i].Y].IsColored() && pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
              {
                horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
              }
              else break;
            }
          }
             
          if (horizontalPieces.Count < 2)
          {
            horizontalPieces.Clear();
          }
          else
          {
            for (int j = 0; j < horizontalPieces.Count; j++)
            {
              matchingPieces.Add(horizontalPieces[j]);
            }
          
            break;
          }
        }
      }

      if (matchingPieces.Count >= 3)
      {
        return matchingPieces;
      }

      return null;
    }

    public bool ClearAllValidMatches()
    {
      bool needsRefill = false;

      for (int y = 0; y < _height; y++)
      {
        for (int x = 0; x < _width; x++)
        {
          if (!pieces[x, y].IsClearable()) continue;
          List<GamePiece> match = GetMatch(pieces[x, y], x, y);

          if (match == null) continue;
          PieceType specialPieceType = PieceType.Count;
          GamePiece randomPiece = match[Random.Range(0, match.Count)];
          int specialPieceX = randomPiece.X;
          int specialPieceY = randomPiece.Y;

          if (match.Count == 4)
          {
            if (_pressedPiece == null || _enteredPiece == null)
            {
              specialPieceType = (PieceType)Random.Range((int)PieceType.RowClear, (int)PieceType.ColumnClear);
            }
            else if (_pressedPiece.Y == _enteredPiece.Y)
            {
              specialPieceType = PieceType.RowClear;
            }
            else
            {
              specialPieceType = PieceType.ColumnClear;
            }
          }
          else if (match.Count >= 5)
          {
            specialPieceType = PieceType.Rainbow;
          }
        
          for (int i = 0; i < match.Count; i++)
          {
            if (ClearPiece(match[i].X, match[i].Y))
            {
              needsRefill = true;

              if (match[i] == _pressedPiece || match[i] == _enteredPiece)
              {
                specialPieceX = match[i].X;
                specialPieceY = match[i].Y;
              }
            }
          }

          if (specialPieceType != PieceType.Count)
          {
            Destroy(pieces[specialPieceX, specialPieceY]);
            GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

            if (specialPieceType == PieceType.RowClear || specialPieceType == PieceType.ColumnClear
                && newPiece.IsColored() && match[0].IsColored())
            {
              newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
            }
            else if (specialPieceType == PieceType.Rainbow && newPiece.IsColored())
            {
              newPiece.ColorComponent.SetColor(ColorPiece.ColorType.Any);
            }
          }
        }
      }

      return needsRefill;
    }

    public bool ClearPiece(int x, int y)
    {
      if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsBeingCleared)
      {
        pieces[x, y].ClearableComponent.Clear();
        SpawnNewPiece(x, y, PieceType.Empty);
      
        ClearObstacles(x, y);

        return true;
      }

      return false;
    }

    public void ClearObstacles(int x, int y)
    {
      for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
      {
        if (adjacentX != x && adjacentX >= 0 && adjacentX < _width)
        {
          GamePiece piece = pieces[adjacentX, y];
          if (piece.Type != PieceType.Obstacle || !piece.IsClearable()) continue;
        
          piece.ClearableComponent.Clear();
          SpawnNewPiece(adjacentX, y, PieceType.Empty);
        }
      }

      for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
      {
        if (adjacentY != y && adjacentY >= 0 && adjacentY < _height)
        {
          GamePiece piece = pieces[x, adjacentY];
          if (piece.Type != PieceType.Obstacle || !piece.IsClearable()) continue;

          piece.ClearableComponent.Clear();
          SpawnNewPiece(x, adjacentY, PieceType.Empty);
        }
      }
    }

    public void ClearRow(int row)
    {
      for (int x = 0; x < _width; x++)
      {
        ClearPiece(x, row);
      }
    }

    public void ClearColumn(int column)
    {
      for (int y = 0; y < _height; y++)
      {
        ClearPiece(column, y);
      }
    }

    public void ClearColor(ColorPiece.ColorType color)
    {
      for (int x = 0; x < _width; x++)
      {
        for (int y = 0; y < _height; y++)
        {
          if (pieces[x, y].IsColored() && pieces[x, y].ColorComponent.Color == color || color == ColorPiece.ColorType.Any)
          {
            ClearPiece(x, y);
          }
        }
      }
    }

    public void GameOver()
    {
      _gameOver = true;
    }

    public List<GamePiece> GetTypeOfPieces(PieceType type)
    {
      List<GamePiece> gamePieces = new();

      for (int x = 0; x < _width; x++)
      {
        for (int y = 0; y < _height; y++)
        {
          if (pieces[x, y].Type == type)
          {
            gamePieces.Add(pieces[x, y]);
          }
        }
      }

      return gamePieces;
    }

    public List<PiecePosition> GetInitialPieces(PieceType type)
    {
      List<PiecePosition> gamePieces = new();

      for (int i = 0; i < initialPieces.Count; i++)
      {
        if (initialPieces[i].type == type)
        {
          gamePieces.Add(initialPieces[i]);
        }
      }

      return gamePieces;
    }
  }
}