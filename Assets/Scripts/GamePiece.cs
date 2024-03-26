using UnityEngine;

public class GamePiece : MonoBehaviour
{
  private int _x;
  public int X
  {
    get => _x;
    set
    {
      if (IsMovable())
      {
        _x = value;
      }
    }
  }

  private int _y;
  public int Y
  {
    get => _y;
    set
    {
      if (IsMovable())
      {
        _y = value;
      }
    }
  }

  public Board.PieceType Type { get; private set; }

  public Board BoardRef { get; private set; }

  public MovablePiece MovableComponent { get; private set; }
  
  public ColorPiece ColorComponent { get; private set; }
  
  public ClearablePiece ClearableComponent { get; private set; }

  private void Awake()
  {
    MovableComponent = gameObject.GetComponent<MovablePiece>();
    ColorComponent = gameObject.GetComponent<ColorPiece>();
    ClearableComponent = gameObject.GetComponent<ClearablePiece>();
  }

  public void Init(int x, int y, Board board, Board.PieceType type)
  {
    X = x;
    Y = y;
    BoardRef = board;
    Type = type;
  }

  private void OnMouseEnter()
  {
    BoardRef.EnterPiece(this);
  }

  private void OnMouseDown()
  {
    BoardRef.PressPiece(this);
  }

  private void OnMouseUp()
  {
    BoardRef.ReleasePiece();
    
  }

  public bool IsMovable()
  {
    return MovableComponent != null;
  }

  public bool IsColored()
  {
    return ColorComponent != null;
  }

  public bool IsClearable()
  {
    return ClearableComponent != null;
  }
}
