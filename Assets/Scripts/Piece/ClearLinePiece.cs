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
        _piece.BoardRef.RowRocket(_piece.Y);
      }
      else
      {
        _piece.BoardRef.ColumnRocket(_piece.X);
      }

      return true;
    }
  }
}