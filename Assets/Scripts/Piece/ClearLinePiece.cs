namespace Piece
{
  public class ClearLinePiece : ClearablePiece
  {
    public bool IsRow;

    public override bool Clear()
    {
      base.Clear();
      
      if (IsRow)
      {
        _piece.BoardRef.ClearRow(_piece.Y);
      }
      else
      {
        _piece.BoardRef.ClearColumn(_piece.X);
      }

      return true;
    }
  }
}