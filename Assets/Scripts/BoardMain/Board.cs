using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enum;
using Levels.Main;
using Panel;
using Piece;
using Piece.Animation;
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

    public bool IsGamePiecesClickable { get; private set; } = true;
    public bool IsFilling { get; private set; }

    private bool _isSwapping;

    private bool _objectDestroying;

    private int _destroyingObjectCount;

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
      GameObject newPiece = Instantiate(_piecePrefabsDictionary[type], GetWorldPosition(x, y), Quaternion.identity, transform);

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

          RainbowSuper(piece1, piece2);
        }
        else if (piece2.PieceType == PieceType.Rainbow && piece2.IsClearable() && piece1.IsColored())
        {
          matchKey = MatchKey.Rainbow;

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
      if (_pressedPiece == null || _enteredPiece == null)
        return;

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
          SetObjectDestroying(true);

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
          else if (specialPieceType == PieceType.Bomb && newPiece.IsColored())
          {
            newPiece.SetPieceTypeInitial(PieceType.Bomb, ColorType.Any);
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

      bool isCleared = piece.ClearableComponent.Clear();

      // if (piece.PieceType == PieceType.Normal)
      // {
      //   StartCoroutine(StartDestroyAnimation(piece));
      // }
      
      Level.OnPieceCleared(piece);

      if (!isCleared) return false;
      
      SpawnNewPiece(x, y, PieceType.Empty);
      ClearObstacles(x, y);

      return true;
    }

    private IEnumerator StartDestroyAnimation(GamePiece piece)
    {
      yield return new WaitForSeconds(_swapPieceTime);

      // Vector3 objectPosition = piece.transform.position;
      // _collectingPieceAnimation.AddObjects(objectPosition, piece);
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
          if (!isCleared) continue;
          
          SpawnNewPiece(adjacentX, y, PieceType.Empty);
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

    #region Rocket

    [SerializeField]
    private Transform _rocketPool;

    private readonly Queue<GameObject> objectPoolQueue = new();

    private const float _rocketDisableTime = 1.5f;

    private const int _waitPieceDestroying = 80;
    
    private const int _waitToFill = 200;

    public async Task RowRocket(GameObject halfRocket, int x, int y, PieceType pieceType)
    {
      GetRocketFromPool(out GameObject leftRocket, out GameObject rightRocket, halfRocket);

      SetHalfRocket(leftRocket, new Vector2(x - 1, y), Quaternion.Euler(0, 0, 90), "Left Rocket");
      SetHalfRocket(rightRocket, new Vector2(x + 1, y), Quaternion.Euler(0, 0, 270), "Right Rocket");

      Task task1 = RightClearRowRocket(x, y);
      Task task2 = LeftClearRowRocket(x, y);

      await Task.WhenAll(task1, task2);
      await Task.Delay(_waitToFill);
      
      FinishDestroyingObjectCallers(0f, pieceType);

      Fillers();
    }

    public async Task ColumnRocket(GameObject halfRocket, int x, int y, PieceType pieceType)
    {
      GetRocketFromPool(out GameObject upRocket, out GameObject downRocket, halfRocket);

      SetHalfRocket(upRocket, new Vector2(x, y + 1), Quaternion.Euler(0, 0, 180), "Up Rocket");
      SetHalfRocket(downRocket, new Vector2(x, y - 1), Quaternion.Euler(0, 0, 0), "Down Rocket");

      Task task1 = BottomClearColumnRocket(x, y);
      Task task2 = UpperClearColumnRocket(x, y);

      await Task.WhenAll(task1, task2);
      await Task.Delay(_waitToFill);

      FinishDestroyingObjectCallers(0f, pieceType);

      Fillers();
    }

    private void SetHalfRocket(GameObject rocket, Vector2 pos, Quaternion rotation, string newName)
    {
      rocket.transform.position = GetWorldPosition((int)pos.x, (int)pos.y);
      rocket.transform.localRotation = rotation;
      rocket.name = newName;
      rocket.SetActive(true);
    }

    private void GetRocketFromPool(out GameObject rocket1, out GameObject rocket2, GameObject halfRocket)
    {
      if (objectPoolQueue.Count > 1)
      {
        rocket1 = objectPoolQueue.Dequeue();
        rocket2 = objectPoolQueue.Dequeue();

        StartCoroutine(AddToPool(_rocketDisableTime, rocket1));
        StartCoroutine(AddToPool(_rocketDisableTime, rocket2));
      }
      else
      {
        rocket1 = CreateHalfRocket(halfRocket);
        rocket2 = CreateHalfRocket(halfRocket);
      }
    }

    private GameObject CreateHalfRocket(GameObject halfRocket)
    {
      GameObject newRocketObject = Instantiate(halfRocket, Vector3.zero, Quaternion.identity, _rocketPool);
      newRocketObject.SetActive(false);
      newRocketObject.GetComponent<Rocket>().SetBoard(this);

      StartCoroutine(AddToPool(_rocketDisableTime, newRocketObject));

      return newRocketObject;
    }

    private IEnumerator AddToPool(float time, GameObject rocket)
    {
      yield return new WaitForSeconds(time);
      objectPoolQueue.Enqueue(rocket);
    }

    public void RocketSuper(GamePiece rocketPiece, GamePiece anotherPiece)
    {
      SetObjectDestroying(true);

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
        
        rocketPiece.X = _pressedPiece.X; rocketPiece.Y = _pressedPiece.Y;
        anotherPiece.X = _pressedPiece.X; anotherPiece.Y = _pressedPiece.Y;

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

    public async Task RightClearRowRocket(int x, int y)
    {
      for (int i = x; i >= 0; i--)
      {
        ClearPiece(i, y);

        await Task.Delay(_waitPieceDestroying);
      }
    }

    public async Task LeftClearRowRocket(int x, int y)
    {
      for (int i = x; i < _width; i++)
      {
        ClearPiece(i, y);
        
        await Task.Delay(_waitPieceDestroying);
      }
    }

    public async Task BottomClearColumnRocket(int x, int y)
    {
      for (int i = y; i < _height; i++)
      {
        ClearPiece(x, i);
        
        await Task.Delay(_waitPieceDestroying);
      }
    }

    public async Task UpperClearColumnRocket(int x, int y)
    {
      for (int i = y; i >= 0; i--)
      {
        ClearPiece(x, i);

        await Task.Delay(_waitPieceDestroying);
      }
    }

    #endregion


    private const int _chanceOfCreatingSpecialObjectByRainbow = 20;

    public void RainbowSuper(GamePiece rainbowPiece, GamePiece anotherPiece)
    {
      SetObjectDestroying(true);

      rainbowPiece.GetComponent<RainbowPiece>().SetPieces(anotherPiece);

      rainbowPiece.ClearableComponent.Clear();
    }

    public async Task ClearRainbow(GamePiece rainbowPiece, GamePiece anotherPiece, ColorType colorType)
    {
      await ClearRainbowTask(rainbowPiece, anotherPiece, colorType);
      await Task.Delay(_waitToFill);

      FinishDestroyingObjectCallers(0f, PieceType.Rainbow);
      
      Fillers();
    }

    private async Task ClearRainbowTask(GamePiece rainbowPiece, GamePiece anotherPiece, ColorType colorType)
    {
      for (int x = 0; x < _width; x++)
      {
        for (int y = 0; y < _height; y++)
        {
          if (x == rainbowPiece.X && y == rainbowPiece.Y)
          {
            ClearPiece(x, y);
          }
          
          if (colorType != ColorType.Any)
          {
            if (pieces[x, y].IsColored() && pieces[x, y].ColorComponent.Color == colorType)
            {
              ClearPiece(x, y);
            }
          }
          else if (anotherPiece.PieceType == PieceType.Rainbow)
          {
            ClearPiece(x, y);
          }
          else if (anotherPiece.PieceType == PieceType.Bomb)
          {
            if (x == anotherPiece.X && y == anotherPiece.Y)
              anotherPiece.ClearableComponent.Clear();

            if (Random.Range(0, 101) >= _chanceOfCreatingSpecialObjectByRainbow || pieces[x, y].PieceType != PieceType.Normal) continue;

            GamePiece gamePiece = DestroyAndCreateNewPiece(pieces[x, y], x, y, PieceType.Bomb, ColorType.Any);
            gamePiece.ClearableComponent.Clear();
          }
          else if (anotherPiece.PieceType == PieceType.RowClear || anotherPiece.PieceType == PieceType.ColumnClear)
          {
            if (x == anotherPiece.X && y == anotherPiece.Y)
              anotherPiece.ClearableComponent.Clear();

            if (Random.Range(0, 101) >= _chanceOfCreatingSpecialObjectByRainbow || pieces[x, y].PieceType != PieceType.Normal) continue;

            PieceType newPieceType = Random.Range(0, 2) == 0 ? PieceType.RowClear : PieceType.ColumnClear;
            GamePiece gamePiece = DestroyAndCreateNewPiece(pieces[x, y], x, y, newPieceType, ColorType.Any);
            gamePiece.ClearableComponent.Clear();
          }
          else if (pieces[x, y].IsColored() && pieces[x, y].ColorComponent.Color == anotherPiece.ColorComponent.Color)
          {
            ClearPiece(x, y);
          }
        }
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
        GamePiece superRocketPiece = DestroyAndCreateNewPiece(bombPiece, bombPiece.X, bombPiece.Y, PieceType.SuperBomb, ColorType.Any);
        superRocketPiece.ClearableComponent.Clear();
      }
    }

    public async Task ClearBomb(GamePiece bombPiece, int radius = 1)
    {
      await ClearBombTask(bombPiece, radius);
      await Task.Delay(_waitToFill * 4);

      FinishDestroyingObjectCallers(0f, PieceType.Bomb);
      
      Fillers();
    }

    private async Task ClearBombTask(GamePiece bombPiece, int radius = 1)
    {
      for (int adjacentX = bombPiece.X - radius; adjacentX <= bombPiece.X + radius; adjacentX++)
      {
        if (adjacentX < 0 || adjacentX >= _width) continue;

        for (int adjacentY = bombPiece.Y - radius; adjacentY <= bombPiece.Y + radius; adjacentY++)
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

    public void SetPieceClickable(bool value)
    {
      IsGamePiecesClickable = value;
    }

    private const float _destroyingObjectTime = 0.0f;

    public void FinishDestroyingObjectCallers(float time = _destroyingObjectTime, PieceType pieceType = PieceType.Normal)
    {
      if (_destroyingObjectCount > 0)
      {
        if (pieceType == PieceType.Normal) return;

        _destroyingObjectCount--;

        if (_destroyingObjectCount > 0) return;
      }

      StopCoroutine(FinishDestroyingObject(time));
      StartCoroutine(FinishDestroyingObject(time));
    }

    private IEnumerator FinishDestroyingObject(float time)
    {
      yield return new WaitForSeconds(time);

      _objectDestroying = false;
    }

    public void IncreaseDestroyingObjectCount()
    {
      _destroyingObjectCount++;
    }
    
    public int GetObjectDestroyingCount()
    {
      return _destroyingObjectCount;
    }

    public void SetObjectDestroying(bool value)
    {
      _objectDestroying = value;
    }

    private const float _standardFillTime = 0.15f;

    public void Fillers(float newFillTime = _standardFillTime)
    {
      ChangeFillTime(newFillTime);

      ClearAllValidMatches();

      StartCoroutine(Fill());
    }

    private void ChangeFillTime(float newFillTime = _standardFillTime)
    {
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

    #region Skills

    public SkillKey SkillKey { get; private set; } = SkillKey.Empty;

    [Header("Skills")]
    public SkillPanel SkillPanel;

    public void SetSkillType(SkillKey skillKey)
    {
      SkillKey = skillKey;
    }

    public void PaintPiece(GamePiece piece)
    {
      piece.ColorComponent.SetColor(SkillPanel.GetColorType());
      Fillers();

      SkillPanel.DecreaseSkillCount(SkillKey.Paint);
      SetSkillType(SkillKey.Empty);
    }

    public void BreakPiece(int x, int y)
    {
      SkillPanel.DecreaseSkillCount(SkillKey.Break);

      SetObjectDestroying(true);
      ClearPiece(x, y);
      Fillers();
      SetSkillType(SkillKey.Empty);
    }

    #endregion
  }
}