using UnityEngine;

namespace Piece
{
  public class BombPiece : ClearablePiece
  {
    public Vector2 Position;

    public override bool Clear()
    {
      base.Clear();

      _piece.BoardRef.Bomb((int)Position.x, (int)Position.y);

      return true;
    }
  }
}