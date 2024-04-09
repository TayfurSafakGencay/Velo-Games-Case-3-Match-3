using System.Collections;
using System.Collections.Generic;
using Enum;
using Levels.Main;
using Piece;
using Piece.Animation;
using Unity.Mathematics;
using UnityEngine;
using Vo;
using Random = UnityEngine.Random;

namespace BoardMain
{
  public class Board : MonoBehaviour
  {
    public Level Level;

    [SerializeField]
    private int _height;

    [SerializeField]
    private int _width;

    [SerializeField]
    private float _fillTime;
    
    [SerializeField]
    private GameObject _background;

    [SerializeField]
    private CollectingPieceAnimation _collectingPieceAnimation;

    [Header("Piece Prefabs")]
    [SerializeField]
    private List<PiecePrefab> _piecePrefabs;
    
    private readonly Dictionary<PieceType, GameObject> _piecePrefabsDictionary = new();

    [Header("Initial Pieces")]
    [SerializeField]
    private List<PiecePosition> _initialPieces;
    
    private GamePiece[,] pieces;

    private bool inverse;

    private GamePiece _pressedPiece;

    private GamePiece _enteredPiece;

    private bool _gameOver;

    public bool IsFilling { get; private set; }

    private bool _isSwapping;

    private bool _objectDestroying;

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

      for (int i = 0; i < _initialPieces.Count; i++)
      {
        if (_initialPieces[i].x >= 0 && _initialPieces[i].x < _width
                                    && _initialPieces[i].y >= 0 && _initialPieces[i].y < _height)
        {
          SpawnNewPiece(_initialPieces[i].x, _initialPieces[i].y, _initialPieces[i].type);
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
      GameObject newPiece = Instantiate(_piecePrefabsDictionary[type], GetWorldPosition(x, y), quaternion.identity, transform);

      pieces[x, y] = newPiece.GetComponent<GamePiece>();
      pieces[x, y].Init(x, y, this, type);

      return pieces[x, y];
    }

    public IEnumerator Fill(float time = 0f)
    {
      yield return new WaitForSeconds(time);
      
      bool needsRefill = true;
      IsFilling = true;

      while (_objectDestroying)
      {
        yield return 0;
      }
      
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

      IsFilling = false;
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

          if (pieceBelow.PieceType == PieceType.Empty)
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

              if (diagonalPiece.PieceType != PieceType.Empty) continue;
              bool hasPieceAbove = true;

              for (int aboveY = y; aboveY >= 0; aboveY--)
              {
                GamePiece pieceAbove = pieces[diagX, aboveY];

                if (pieceAbove.IsMovable())
                {
                  break;
                }
                else if (!pieceAbove.IsMovable() && pieceAbove.PieceType != PieceType.Empty)
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

        if (pieceBelow.PieceType != PieceType.Empty) continue;

        Destroy(pieceBelow.gameObject);
        GameObject newPiece = Instantiate(_piecePrefabsDictionary[PieceType.Normal], GetWorldPosition(x, -1), Quaternion.identity, transform);

        pieces[x, 0] = newPiece.GetComponent<GamePiece>();
        pieces[x, 0].Init(x, -1, this, PieceType.Normal);
        pieces[x, 0].MovableComponent.Move(x, 0, _fillTime);
        pieces[x, 0].ColorComponent.SetColor((ColorType)Random.Range(0, pieces[x, 0].ColorComponent.ColorNumber));
        movedPiece = true;
      }

      return movedPiece;
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
      return (piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1)
             || (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    private const float _swapPieceTime = 0.15f;
    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
      if (_gameOver || IsFilling || _isSwapping) return;
    
      if (!piece1.IsMovable() || !piece2.IsMovable()) return;

      pieces[piece1.X, piece1.Y] = piece2;
      pieces[piece2.X, piece2.Y] = piece1;

      if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null
                                                       || piece1.PieceType == PieceType.Rainbow || piece2.PieceType == PieceType.Rainbow
                                                       || piece1.PieceType == PieceType.ColumnClear || piece2.PieceType == PieceType.ColumnClear
                                                       || piece1.PieceType == PieceType.RowClear || piece2.PieceType == PieceType.RowClear
                                                       || piece1.PieceType == PieceType.Bomb || piece2.PieceType == PieceType.Bomb)
      {
        _isSwapping = true;
        
        int piece1X = piece1.X;
        int piece1Y = piece1.Y;

        piece1.MovableComponent.Move(piece2.X, piece2.Y, _swapPieceTime);
        piece2.MovableComponent.Move(piece1X, piece1Y, _swapPieceTime);

        MatchKey matchKey = MatchKey.Empty;

        if (piece1.PieceType == PieceType.Rainbow && piece1.IsClearable() && piece2.IsColored())
        {
          matchKey = MatchKey.Rainbow;
          
          RainbowPiece rainbowPiece = piece1.GetComponent<RainbowPiece>();

          if (rainbowPiece)
          {
            rainbowPiece.SetRainbowItems(piece1, piece2); 
          }

          RainbowSuper(piece1, piece2);
        }
        else if (piece2.PieceType == PieceType.Rainbow && piece2.IsClearable() && piece1.IsColored())
        {
          matchKey = MatchKey.Rainbow;
          
          RainbowPiece rainbowPiece = piece2.GetComponent<RainbowPiece>();

          if (rainbowPiece)
          {
            rainbowPiece.SetRainbowItems(piece2, piece1); 
          }

          RainbowSuper(piece2, piece1);
        }

        if (matchKey == MatchKey.Empty)
        {
          if (piece1.PieceType == PieceType.RowClear || piece1.PieceType == PieceType.ColumnClear)
          {
            matchKey = MatchKey.Column;
            
            RocketSuper(piece1, piece2);
          }
          if (piece2.PieceType == PieceType.RowClear || piece2.PieceType == PieceType.ColumnClear)
          {
            matchKey = MatchKey.Row;

            RocketSuper(piece2, piece1);
          }
        }

        if (matchKey == MatchKey.Empty)
        {
          if (piece1.PieceType == PieceType.Bomb)
          {
            matchKey = MatchKey.Bomb;

            Bomb(piece1, piece2);
          }
          else if (piece2.PieceType == PieceType.Bomb)
          {
            matchKey = MatchKey.Bomb;
            Bomb(piece2, piece1);
          }
        }
        
        ClearAllValidMatches();

        _enteredPiece = null;
        _pressedPiece = null;

        StartCoroutine(Fill());
      
        Level.OnMove();

        _isSwapping = false;
      }
      else
      {
        StartCoroutine(InvalidSwap(piece1, piece2));
      }
    }

    private IEnumerator InvalidSwap(GamePiece piece1, GamePiece piece2)
    {
      _isSwapping = true;
      
      int piece1X = piece1.X;
      int piece1Y = piece1.Y;
      
      int piece2X = piece2.X;
      int piece2Y = piece2.Y;
        
      piece1.MovableComponent.Move(piece2X, piece2Y, _swapPieceTime);
      piece2.MovableComponent.Move(piece1X, piece1Y, _swapPieceTime);
      
      yield return new WaitForSeconds(0.3f);
      
      piece1.MovableComponent.Move(piece1X, piece1Y, _swapPieceTime);
      piece2.MovableComponent.Move(piece2X, piece2Y, _swapPieceTime);
      
      pieces[piece1.X, piece1.Y] = piece1;
      pieces[piece2.X, piece2.Y] = piece2;
      
      _isSwapping = false;
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

      ColorType color = piece.ColorComponent.Color;

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
            if (pieces[x, newY].PieceType == PieceType.Normal)
            {
              horizontalPieces.Add(pieces[x, newY]);
            }
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
                if (pieces[horizontalPieces[i].X, y].PieceType == PieceType.Normal)
                {
                  verticalPieces.Add(pieces[horizontalPieces[i].X, y]);
                }
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
            if (pieces[newX, y].PieceType == PieceType.Normal)
            {
              verticalPieces.Add(pieces[newX, y]);
            }
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
                if (pieces[x, verticalPieces[i].Y].PieceType == PieceType.Normal)
                {
                  horizontalPieces.Add(pieces[x, verticalPieces[i].Y]);
                }
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
          _objectDestroying = true;

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
            specialPieceType = CheckRainbowOrBomb(match);
          }
        
          for (int i = 0; i < match.Count; i++)
          {
            if (!ClearPiece(match[i].X, match[i].Y)) continue;
            needsRefill = true;

            if (match[i] != _pressedPiece && match[i] != _enteredPiece) continue;
            specialPieceX = match[i].X;
            specialPieceY = match[i].Y;
          }

          if (specialPieceType == PieceType.Count) continue;
          Destroy(pieces[specialPieceX, specialPieceY]);
          GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);

          if (specialPieceType == PieceType.RowClear && newPiece.IsColored() && match[0].IsColored())
          {
            newPiece.SetPieceTypeInitial(PieceType.RowClear, ColorType.Any);
          }
          else if (specialPieceType == PieceType.ColumnClear && newPiece.IsColored() && match[0].IsColored())
          {
            newPiece.SetPieceTypeInitial(PieceType.ColumnClear, ColorType.Any);
          }
          else if (specialPieceType == PieceType.Rainbow && newPiece.IsColored())
          {
            newPiece.SetPieceTypeInitial(PieceType.Rainbow, ColorType.Any);
          }
        }
      }

      return needsRefill;
    }

    public PieceType CheckRainbowOrBomb(List<GamePiece> matches)
    {
      int xCount = 0;
      int yCount = 0;
      
      for (int i = 0; i < matches.Count - 1; i++)
      {
        if (matches[i].X == matches[i + 1].X)
        {
          xCount++;
        }

        if (matches[i].Y == matches[i + 1].Y)
        {
          yCount++;
        }
      }

      if (xCount >= 4 || yCount >= 4)
      {
        return PieceType.Rainbow;
      }
      
      return PieceType.Bomb;
    }

    public bool ClearPiece(int x, int y)
    {
      GamePiece piece = pieces[x, y];

      if (!piece.IsClearable() || piece.ClearableComponent.IsBeingCleared) return false;
      
      piece.ClearableComponent.Clear();

      if (piece.PieceType == PieceType.Normal)
      {
        StartCoroutine(StartDestroyAnimation(piece));  
      }

      SpawnNewPiece(x, y, PieceType.Empty);
      ClearObstacles(x, y);


      return true;
    }

    private IEnumerator StartDestroyAnimation(GamePiece piece)
    {
      yield return new WaitForSeconds(_swapPieceTime);

      Vector3 objectPosition = piece.transform.position;
      _collectingPieceAnimation.AddObjects(objectPosition, piece);
    }

    public void ClearObstacles(int x, int y)
    {
      for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++)
      {
        if (adjacentX != x && adjacentX >= 0 && adjacentX < _width)
        {
          GamePiece piece = pieces[adjacentX, y];
          if (piece.PieceType != PieceType.Obstacle || !piece.IsClearable()) continue;
        
          bool isCleared = piece.ClearableComponent.Clear();
          
          if (isCleared)
          {
            SpawnNewPiece(adjacentX, y, PieceType.Empty);
          }
        }
      }

      for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY++)
      {
        if (adjacentY != y && adjacentY >= 0 && adjacentY < _height)
        {
          GamePiece piece = pieces[x, adjacentY];
          if (piece.PieceType != PieceType.Obstacle || !piece.IsClearable()) continue;

          bool isCleared = piece.ClearableComponent.Clear();

          if (isCleared)
          {
            SpawnNewPiece(x, adjacentY, PieceType.Empty);
          }
        }
      }
    }

    public void RowRocket(int row)
    {
      for (int x = 0; x < _width; x++)
      {
        ClearPiece(x, row);
      }
    }

    public void ColumnRocket(int column)
    {
      for (int y = 0; y < _height; y++)
      {
        ClearPiece(column, y);
      }
    }

    private const int _chanceOfCreatingSpecialObjectByRainbow = 20;
    public void RainbowSuper(GamePiece rainbowPiece, GamePiece anotherPiece)
    {
      _objectDestroying = true;

      for (int x = 0; x < _width; x++)
      {
        for (int y = 0; y < _height; y++)
        {
          if (anotherPiece.PieceType == PieceType.Rainbow)
          {
            ClearPiece(x, y);
          }
          else if (anotherPiece.PieceType == PieceType.Bomb)
          {
            RainbowCommons(rainbowPiece, anotherPiece, x, y, PieceType.Bomb);
          }
          else if (anotherPiece.PieceType == PieceType.RowClear || anotherPiece.PieceType == PieceType.ColumnClear)
          {
            PieceType newPieceType = Random.Range(0, 2) == 0 ? PieceType.RowClear : PieceType.ColumnClear;
            RainbowCommons(rainbowPiece, anotherPiece, x, y, newPieceType);
          }
          else if (pieces[x, y].IsColored() && pieces[x, y].ColorComponent.Color == anotherPiece.ColorComponent.Color)
          {
            ClearPiece(x, y);
          }
        }
      }

      // FinishDestroyingObjectCallers();
      // StartCoroutine(Fill(0.75f));
    }

    private void RainbowCommons(GamePiece rainbowPiece, GamePiece anotherPiece, int x, int y, PieceType newPieceType)
    {
      // if (pieces[x, y].PieceType != PieceType.Normal) return;
      //
      // if (rainbowPiece.X == x && rainbowPiece.Y == y)
      // {
      //   rainbowPiece.ClearableComponent.Clear();
      //   return;
      // }
      // if (anotherPiece.X == x && anotherPiece.Y == y)
      // {
      //   anotherPiece.Activate();
      //   return;
      // }
      //
      // if (Random.Range(0, 101) >= _chanceOfCreatingSpecialObjectByRainbow) return;
      //
      // Destroy(pieces[x, y].gameObject);
      // GamePiece newPiece = SpawnNewPiece(x, y, newPieceType);
      // pieces[x, y] = newPiece;
      // pieces[x, y].SetSpecialPieceTypeAndActivate(newPieceType, ColorType.Any);
    }

    public void RocketSuper(GamePiece rocketPiece, GamePiece anotherPiece)
    {
      _objectDestroying = true;
      
      if (anotherPiece.PieceType == PieceType.ColumnClear || anotherPiece.PieceType == PieceType.RowClear)
      {
        switch (rocketPiece.PieceType)
        {
          case PieceType.ColumnClear:
            anotherPiece.SetPieceTypeInitial(PieceType.RowClear, ColorType.Any);
            break;
          case PieceType.RowClear:
            anotherPiece.SetPieceTypeInitial(PieceType.ColumnClear, ColorType.Any);
            break;
        }

        rocketPiece.ClearableComponent.Clear();
        anotherPiece.ClearableComponent.Clear();
      }
      else if (anotherPiece.PieceType == PieceType.Bomb)
      {
        GamePiece superRocketPiece = DestroyAndCreateNewPiece(_pressedPiece, _pressedPiece.X, _pressedPiece.Y, PieceType.SuperRocket, ColorType.Any);
        superRocketPiece.ClearableComponent.Clear();

        for (int x = _pressedPiece.X - 1; x <= _pressedPiece.X + 1; x++)
        {
          if (x == _pressedPiece.X || x < 0 || x >= _width) continue;

          GamePiece piece = DestroyAndCreateNewPiece(pieces[x, _pressedPiece.Y], x, _pressedPiece.Y, PieceType.ColumnClear, ColorType.Any);
          piece.ClearableComponent.Clear();
        }

        for (int y = _pressedPiece.Y - 1; y <= _pressedPiece.Y + 1; y++)
        {
          if (y == _pressedPiece.Y || y < 0 || y >= _height) continue;

          GamePiece piece = DestroyAndCreateNewPiece(pieces[_pressedPiece.X, y], _pressedPiece.X, y, PieceType.RowClear, ColorType.Any);
          piece.ClearableComponent.Clear();
        }
      }
      else if (anotherPiece.IsClearable() && anotherPiece.PieceType == PieceType.Normal)
      {
        rocketPiece.ClearableComponent.Clear();
      }
    }

    public void Bomb(GamePiece bombPiece, GamePiece anotherPiece)
    {
      if (anotherPiece.PieceType == PieceType.Normal)
      {
        bombPiece.ClearableComponent.Clear();
      }
      else if (anotherPiece.PieceType == PieceType.Bomb)
      {
        
      }
    }

    public void ClearBomb(GamePiece bombPiece)
    {
      for (int adjacentX = bombPiece.X - 1; adjacentX <= bombPiece.X + 1; adjacentX++)
      {
        if (adjacentX < 0 || adjacentX >= _width) continue;

        for (int adjacentY = bombPiece.Y - 1; adjacentY <= bombPiece.Y + 1; adjacentY++)
        {
          if (adjacentY < 0 || adjacentY >= _height) continue;
          ClearPiece(adjacentX, adjacentY);
        }
      }
    }

    private GamePiece DestroyAndCreateNewPiece(GamePiece gamePiece, int x, int y, PieceType pieceType, ColorType colorType)
    {
      Destroy(gamePiece.gameObject);
      GamePiece newPiece = SpawnNewPiece(x, y, pieceType);
      pieces[x, y] = newPiece;
      pieces[x, y].SetPieceTypeInitial(pieceType, colorType);

      return pieces[x, y];
    }

    private const float _destroyingObjectTime = 0.5f;

    public void FinishDestroyingObjectCallers(float time = _destroyingObjectTime)
    {
      StopCoroutine(FinishDestroyingObject(time));
      StartCoroutine(FinishDestroyingObject(time));
    }

    private IEnumerator FinishDestroyingObject(float time)
    {
      yield return new WaitForSeconds(time);

      _objectDestroying = false;
    }

    public void Fillers()
    {
      ClearAllValidMatches();

      StartCoroutine(Fill());
    }

    private IEnumerator ChangeFillTime(float fillTime, float newFillTime = 0.1f)
    {
      _fillTime = fillTime;
      
      yield return new WaitForSeconds(0.25f);
      
      while (IsFilling)
        yield return 0;
      
      _fillTime = newFillTime;
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
          if (pieces[x, y].PieceType == type)
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

      for (int i = 0; i < _initialPieces.Count; i++)
      {
        if (_initialPieces[i].type == type)
        {
          gamePieces.Add(_initialPieces[i]);
        }
      }

      return gamePieces;
    }
  }
}