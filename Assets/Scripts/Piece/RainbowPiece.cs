using Enum;

namespace Piece
{
  public class RainbowPiece : ClearablePiece
  {
    private ColorType _color { get; set; }

    private PieceType _pieceType { get; set; }

    private GamePiece _rainbowPiece;

    private GamePiece _anotherPiece;

    public void SetRainbowItems(GamePiece piece1, GamePiece piece2)
    {
      _rainbowPiece = piece1;
      _anotherPiece = piece2;

      _color = _anotherPiece.ColorComponent.Color;
      _pieceType = _anotherPiece.PieceType;
    }

    public override bool Clear()
    {
      base.Clear();

      _piece.BoardRef.RainbowSuper( _rainbowPiece, _anotherPiece);

      return true;
    }
  }
}