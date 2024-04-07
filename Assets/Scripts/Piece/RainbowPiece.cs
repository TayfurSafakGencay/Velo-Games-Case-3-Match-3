namespace Piece
{
  public class RainbowPiece : ClearablePiece
  {
    public ColorPiece.ColorType Color { get; set; }

    public override bool Clear()
    {
      base.Clear();

      _piece.BoardRef.RainBow(Color);

      return true;
    }
  }
}