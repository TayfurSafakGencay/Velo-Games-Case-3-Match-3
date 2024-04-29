using System;
using Client.Enum;

namespace Client.Vo
{
  [Serializable]
  public struct PiecePosition
  {
    public PieceType type;
    public int x;
    public int y;
  }
}